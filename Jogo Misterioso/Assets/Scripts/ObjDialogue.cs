using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;

public class ObjDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    [Header("Dados de Diálogo")]
    public ObjectInteractionDialogue objDialogueData;

    [Header("Item (opcional)")]
    [Tooltip("Se definido, este item será dado ao jogador")]
    public GameObject itemPrefab;

    // — campos internos —
    private GameObject inventoryPanel;
    private InventoryController inventoryController;

    private GameObject objDialoguePanelInstance;
    private TMP_Text   objDialogueText;
    private bool       panelInited;

    private int  objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;

    void Start()
    {
        // 1) Valida prefab de UI
        if (objDialoguePanelPrefab == null)
        {
            Debug.LogError("🚫 Prefab de UI não atribuído em ObjDialogue!", this);
            enabled = false;
            return;
        }

        // 2) Se houver um itemPrefab, resolve InventoryController e InventoryPanel
        if (itemPrefab != null)
        {
            // 2a) InventoryController
            inventoryController = FindFirstObjectByType<InventoryController>();
            if (inventoryController == null)
                Debug.LogError("❌ InventoryController não encontrado!", this);

            // 2b) InventoryPanel ativo
            inventoryPanel = GameObject.FindWithTag("InventoryPanel");

            // 2c) InventoryPanel inativo? procura entre todos os Transforms
            if (inventoryPanel == null)
            {
                foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                {
                    if (t.gameObject.CompareTag("InventoryPanel"))
                    {
                        inventoryPanel = t.gameObject;
                        break;
                    }
                }
            }

            if (inventoryPanel == null)
                Debug.LogError("❌ InventoryPanel não encontrado (nem inativo)!", this);
        }

        // 3) Instancia e configura o painel de diálogo
        var canvas = GameObject.FindWithTag("UICanvas");
        if (canvas == null)
        {
            Debug.LogError("🚫 Não encontrei nenhum Canvas na cena!", this);
            enabled = false;
            return;
        }

        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            canvas.transform,
            worldPositionStays: false
        );
        objDialogueText    = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        objDialoguePanelInstance.SetActive(false);
        panelInited        = true;

        // 4) Subscreve eventos
        DialogueManager.Instance.OnNewDialogue   += CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue+= HandleResume;
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNewDialogue   -= CancelDialogue;
            DialogueManager.Instance.OnPauseDialogue -= HandlePause;
            DialogueManager.Instance.OnResumeDialogue-= HandleResume;
        }
    }

    public bool CanInteract() => !objIsDialogueActive;

    public void Interact()
    {
        // 1) Lógica de inventário (se itemPrefab definido)
        bool hasItem = false;
        if (itemPrefab != null && inventoryPanel != null && inventoryController != null)
        {
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

        // 2) Cancela diálogos em curso
        DialogueManager.Instance.RequestNewDialogue(this);

        // 3) Inicia ou avança no diálogo
        if (objIsDialogueActive) NextLine();
        else                     StartObjDialog();
    }

    void InitDialoguePanel()
    {
        // (não usado aqui, pois já instanciámos no Start)
    }

    void StartObjDialog()
    {
        objIsDialogueActive = true;
        objDialogueIndex    = 0;
        objDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (objIsTyping)
        {
            StopAllCoroutines();
            objDialogueText.text = objDialogueData.dialogueLines[objDialogueIndex];
            objIsTyping = false;
            return;
        }

        if (++objDialogueIndex < objDialogueData.dialogueLines.Length)
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

        if (objDialogueData.autoProgressLines.Length > objDialogueIndex &&
            objDialogueData.autoProgressLines[objDialogueIndex])
        {
            yield return new WaitForSeconds(objDialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void CancelDialogue()
    {
        if (!panelInited) return;
        EndDialogue();
    }

    void HandlePause()
    {
        if (!objIsDialogueActive) return;
        StopAllCoroutines();
        objDialoguePanelInstance.SetActive(false);
    }

    void HandleResume()
    {
        if (!objIsDialogueActive) return;
        objDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        objIsDialogueActive = false;
        objDialoguePanelInstance.SetActive(false);
    }
}