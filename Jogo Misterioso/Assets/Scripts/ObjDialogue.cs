// ObjDialogue.cs
using UnityEngine;
using System.Collections;
using TMPro;

public class ObjDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    public ObjectInteractionDialogue objDialogueData;
    public GameObject itemPrefab;
    private GameObject inventoryPanel;

    GameObject objDialoguePanelInstance;
    TMP_Text objDialogueText;
    bool panelInited;

    int    objDialogueIndex;
    bool   objIsTyping, objIsDialogueActive;
    bool   hasItem;
    InventoryController inventoryController;

    void Start()
    {
        if (inventoryPanel == null)
            inventoryPanel = GameObject.FindWithTag("InventoryPanel");

        if (inventoryPanel == null)
        {
            Debug.LogError("Não encontrei o InventoryPanel na cena!", this);
            enabled = false;
            return;
        }

        if (objDialoguePanelPrefab == null)
        {
            var distributor = FindFirstObjectByType<DialoguePrefabDistributor>();
            if (distributor != null)
                objDialoguePanelPrefab = distributor.dialoguePanelPrefab;
        }

        if (objDialoguePanelPrefab == null)
        {
            Debug.LogError("🚫 Prefab de UI não atribuído em ObjDialogue!", this);
            enabled = false;
            return;
        }

        if (objDialoguePanelPrefab == null)
            Debug.LogError("Arrasta o prefab de UI em objDialoguePanelPrefab!", this);

        inventoryController = FindFirstObjectByType<InventoryController>();

        
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

    public void CancelDialogue()
    {
        if (!panelInited) 
            return;    // nada a limpar se ainda não inicializaste o painel
        EndDialogue();
    }

    public bool CanInteract()   => !objIsDialogueActive;

    void HandlePause()
    {
        if (!objIsDialogueActive) return;
        StopAllCoroutines();
        objDialoguePanelInstance.SetActive(false);
    }

    void HandleResume()
    {
        if (!objIsDialogueActive) return;
        // retoma a typing da linha atual
        objDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    public void Interact()
    {
        if (!panelInited)
            InitDialoguePanel();
        // cancela outros diálogos
        DialogueManager.Instance?.RequestNewDialogue(this);

        // lógica de inventário (opcional)
        hasItem = false;
        if (itemPrefab != null)
        {
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
            if (!hasItem) inventoryController.AddItem(itemPrefab);
        }

        if (objIsDialogueActive) NextLine();
        else                  StartObjDialog();
    }
    void InitDialoguePanel()
    {
        // garante que tens um prefab atribuído
        if (objDialoguePanelPrefab == null)
            Debug.LogError("🚫 Prefab de UI não atribuído em ObjDialogue!", this);

        // instancia no Canvas
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
            Debug.LogError("Não encontrei Canvas na cena.", this);

        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            canvas.transform,
            worldPositionStays: false
        );
        
        Debug.Log($"[InitDialoguePanel] Instanciei painel: {objDialoguePanelInstance.name}, tag = {objDialoguePanelInstance.tag}");


        objDialogueText = objDialoguePanelInstance
            .GetComponentInChildren<TMP_Text>();
        if (objDialogueText == null)
            Debug.LogError("O prefab não tem TMP_Text em filho!", this);

        objDialoguePanelInstance.SetActive(false);
        objDialogueText.text = "";

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
        }
        else if (++objDialogueIndex < objDialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else EndDialogue();
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

    public void EndDialogue()
    {
        StopAllCoroutines();
        objIsDialogueActive = false;
        objDialogueText.text = "";
        objDialoguePanelInstance.SetActive(false);
    }
}
