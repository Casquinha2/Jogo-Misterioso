using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class ObjDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    [Header("Dados de Diálogo")]
    public ObjectInteractionDialogue objDialogueData;

    [Header("Item (opcional)")]
    public GameObject itemPrefab;

    [Header("Adicionar progresso?")]
    public bool adicionarProgresso = false;

    [Header("Se o adicionarProgresso for true, adicionar o Personagens gameobject")]
    public Progress progress;

    // Internos
    private GameObject inventoryPanel;
    private InventoryController inventoryController;

    private GameObject objDialoguePanelInstance;
    private TMP_Text objDialogueText;
    private bool panelInited;

    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;

    public static event Action<ObjDialogue> OnDialogueEnded;

    void Start()
    {
        Debug.Log($"[Start] dialogueData? {objDialogueData}, panelPrefab? {objDialoguePanelPrefab}");

        // Validações iniciais
        if (objDialoguePanelPrefab == null)
        {
            Debug.LogError("🚫 Prefab de UI não atribuído em ObjDialogue!", this);
            enabled = false;
            return;
        }

        // Busca inventário (só se for item)
        if (itemPrefab != null)
        {
            inventoryController = FindFirstObjectByType<InventoryController>();
            if (inventoryController == null)
                Debug.LogError("❌ InventoryController não encontrado!", this);

            inventoryPanel = GameObject.FindWithTag("InventoryPanel");
            if (inventoryPanel == null)
            {
                // busca inativo
                foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                    if (t.gameObject.CompareTag("InventoryPanel"))
                    {
                        inventoryPanel = t.gameObject;
                        break;
                    }

                if (inventoryPanel == null)
                    Debug.LogError("❌ InventoryPanel não encontrado (nem inativo)!", this);
            }
        }

        // Instancia o panel dentro do Canvas
        var canvas = GameObject.FindWithTag("UICanvas");
        if (canvas == null)
        {
            Debug.LogError("🚫 Canvas não encontrado!", this);
            enabled = false;
            return;
        }

        objDialoguePanelInstance = Instantiate(objDialoguePanelPrefab, canvas.transform, false);
        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();

        // MARCAÇÃO IMPORTANTE: nunca chamar SetActive sem checar a existência
        if (objDialoguePanelInstance)
            objDialoguePanelInstance.SetActive(false);

        Debug.Log($"Painel instanciado? {objDialoguePanelInstance}", this);


        panelInited = true;
    }

    void OnDestroy()
    {
        // Remove todos os delegates para não receber callbacks após a destruição
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNewDialogue += HandleAnyDialogueStarted;
            DialogueManager.Instance.OnPauseDialogue -= HandlePause;
            DialogueManager.Instance.OnResumeDialogue-= HandleResume;
        }

        // Se por acaso o panel for filho de algo que também foi destruído, destruir reference
        if (objDialoguePanelInstance)
            Destroy(objDialoguePanelInstance);
    }

    public bool CanInteract() => !objIsDialogueActive;
    
    void OnEnable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   += HandleAnyDialogueStarted;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue+= HandleResume;
    }

    void OnDisable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   -= HandleAnyDialogueStarted;
        DialogueManager.Instance.OnPauseDialogue -= HandlePause;
        DialogueManager.Instance.OnResumeDialogue-= HandleResume;
    }

    public void Interact()
    {
        Debug.Log($"[Interact] pausa? {PauseController.IsGamePaused}");


        // Bloqueia se já estiver em diálogo ou se o jogo estiver pausado
        if (objDialogueData == null || (PauseController.IsGamePaused && !objIsDialogueActive))
            return;

        // Lógica de pegar/devolver item
        if (itemPrefab != null && inventoryPanel != null && inventoryController != null)
        {
            bool hasItem = false;
            foreach (Transform slotT in inventoryPanel.transform)
            {
                var slot = slotT.GetComponent<Slot>();
                if (slot?.currentItem == null) continue;
                var invItem = slot.currentItem.GetComponent<Item>();
                if (invItem != null && invItem.ID == itemPrefab.GetComponent<Item>().ID)
                {
                    hasItem = true;
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                    break;
                }
            }
            if (!hasItem)
                inventoryController.AddItem(itemPrefab);
        }


        // Se já estava ativo, avança; senão, inicia
        if (objIsDialogueActive)
            NextLine();
        else
        {
            DialogueManager.Instance.RequestNewDialogue(this);
            StartObjDialog();
        }

    }

    void StartObjDialog()
    {
        Debug.Log($"[StartObjDialog] entrando — painel instância = {objDialoguePanelInstance}");

        if (objDialogueData.dialogueLines == null || objDialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning($"[StartObjDialog] Sem linhas de diálogo definidas para {name}");
            EndDialogue();
            return;
        }


        objIsDialogueActive  = true;
        objDialogueIndex      = 0;

        if (objDialoguePanelInstance) 
            objDialoguePanelInstance.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (objIsTyping)
        {
            StopAllCoroutines();
            objDialogueText.SetText(objDialogueData.dialogueLines[objDialogueIndex]);
            objIsTyping = false;
            return;
        }

        objDialogueIndex++;
        if (objDialogueIndex < objDialogueData.dialogueLines.Length)
            StartCoroutine(TypeLine());
        else
            EndDialogue();
    }

    IEnumerator TypeLine()
    {
        objIsTyping = true;
        objDialogueText.text = "";

        foreach (var ch in objDialogueData.dialogueLines[objDialogueIndex])
        {
            objDialogueText.text += ch;
            yield return new WaitForSeconds(objDialogueData.typingSpeed);
        }

        objIsTyping = false;

        // auto-avanço?
        if (objDialogueData.autoProgressLines.Length > objDialogueIndex &&
            objDialogueData.autoProgressLines[objDialogueIndex])
        {
            yield return new WaitForSeconds(objDialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void CancelDialogue()
    {
        Debug.Log($"[CancelDialogue] panelInited={panelInited}, active={objIsDialogueActive}");

        Debug.Log($"[{name}] CancelDialogue disparado — panelInited? {panelInited}, objIsDialogueActive? {objIsDialogueActive}");

        if (!panelInited || !objIsDialogueActive) return;
        StopAllCoroutines();
        objIsDialogueActive = false;
        objDialoguePanelInstance.SetActive(false);
    
    }
    private void HandleAnyDialogueStarted(ICancelableDialogue startedBy)
    {
        // se for este mesmo obj, ignora
        if (ReferenceEquals(startedBy, this)) return;
        // senão, cancela o diálogo ativo
        CancelDialogue();
    }

    void HandlePause()
    {
        if (!objIsDialogueActive) return;

        StopAllCoroutines();
        if (objDialoguePanelInstance) 
            objDialoguePanelInstance.SetActive(false);
    }

    void HandleResume()
    {
        if (!objIsDialogueActive) return;

        if (objDialoguePanelInstance) 
            objDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        objIsDialogueActive = false;

        if (objDialoguePanelInstance) 
            objDialoguePanelInstance.SetActive(false);

        OnDialogueEnded?.Invoke(this);

        if (adicionarProgresso && progress != null)
            progress.AddProgress();
    }
}
