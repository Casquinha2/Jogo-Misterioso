using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq;

public class ObjDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    [Header("Diálogo")]
    public ObjectInteractionDialogue objDialogueData;

    [Header("Item (opcional)")]
    [Tooltip("Se definido, este item será dado ao jogador")]
    public GameObject itemPrefab;

    // --- campos internos ---
    GameObject inventoryPanel;
    InventoryController inventoryController;
    GameObject objDialoguePanelInstance;
    TMP_Text objDialogueText;
    bool panelInited;

    int  objDialogueIndex;
    bool objIsTyping, objIsDialogueActive;

    void Awake()
    {
        // só procura o InventoryController se for necessário
        if (itemPrefab != null)
        {
            inventoryController = FindFirstObjectByType<InventoryController>();
            if (inventoryController == null)
                Debug.LogError("❌ InventoryController não encontrado!", this);

            // procura o painel, ativo ou inativo
            inventoryPanel = GameObject.FindWithTag("InventoryPanel");
            if (inventoryPanel == null)
            {
                var t = Resources
                    .FindObjectsOfTypeAll<Transform>()
                    .FirstOrDefault(x => x.CompareTag("InventoryPanel"));
                if (t != null)
                    inventoryPanel = t.gameObject;
            }

            if (inventoryPanel == null)
                Debug.LogError("InventoryPanel não encontrado (nem inativo)!", this);
        }

        // valida o prefab de UI
        if (objDialoguePanelPrefab == null)
        {
            Debug.LogError("🚫 Prefab de UI não atribuído em ObjDialogue!", this);
            enabled = false;
        }
    }

    void OnEnable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   += CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue+= HandleResume;
    }

    void OnDisable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   -= CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue -= HandlePause;
        DialogueManager.Instance.OnResumeDialogue-= HandleResume;
    }

    public bool CanInteract() => !objIsDialogueActive;

    public void Interact()
    {
        // 1) Cancela diálogos anteriores e inicializa UI
        DialogueManager.Instance.RequestNewDialogue(this);
        if (!panelInited) InitDialoguePanel();

        // 2) Se houver itemPrefab, faz a lógica de adicionar
        if (itemPrefab != null && inventoryController != null && inventoryPanel != null)
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
                    break;
                }
            }

            if (!hasItem)
                inventoryController.AddItem(itemPrefab);
        }

        // 3) Inicia ou avança no diálogo
        if (objIsDialogueActive) NextLine();
        else                  StartObjDialog();
    }

    void InitDialoguePanel()
    {
        var canvas = GameObject.FindWithTag("UICanvas");
        if (canvas == null)
        {
            Debug.LogError("Canvas não encontrado na cena!", this);
            return;
        }

        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            canvas.transform,
            worldPositionStays: false
        );

        objDialoguePanelInstance.tag = "DialoguePanel";
        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        objDialoguePanelInstance.SetActive(false);
        panelInited = true;
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
        foreach (char ch in objDialogueData.dialogueLines[objDialogueIndex])
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
        if (objDialoguePanelInstance != null)
        {
            objDialogueText.text = "";
            objDialoguePanelInstance.SetActive(false);
        }
    }
}