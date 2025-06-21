using UnityEngine;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.UI;

public class PortaInteraction : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Dados de Diálogo")]
    public ObjectInteractionDialogue objDialogueData;
    public int indexDialogueCerto;

    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    public GameObject itemPrefab;

    // Se quiseres podes deixar estes campos públicos mas não pavimentados no Inspector:
    [Header("Referências Dinâmicas (se não arrastares no Inspector)")]
    private Transform uiRoot;
    private GameObject inventoryPanel;
    private GameObject tutorialPanel;

    // Estes três campos vão ser obtidos por código:
    private GameObject player;                  
    private PolygonCollider2D mapBoundary;      
    private CinemachineConfiner2D confiner;     

    [SerializeField] string checkpointID;

    // estado interno…
    private GameObject objDialoguePanelInstance;
    private TMP_Text   objDialogueText;
    private string[]   rightDialogue, wrongDialogue, selectedDialogueLines;
    private int        objDialogueIndex;
    private bool       objIsTyping, objIsDialogueActive;

    void Awake()
    {
        // 1) Player  
        if (player == null)
            player = GameObject.FindWithTag("Player");

        // 2) Confiner (assume só um na cena)
        if (confiner == null)
            confiner = FindFirstObjectByType<CinemachineConfiner2D>();

        // 3) MapBoundary – vai buscar o MapBounds root e encontra o filho de nome == checkpointID
        if (mapBoundary == null)
        {
            var root = GameObject.Find("MapBounds")?.transform;
            if (root != null)
            {
                var node = root.Find(checkpointID);
                if (node != null)
                    mapBoundary = node.GetComponent<PolygonCollider2D>();
            }
        }

        // 4) Inventário e Tutorial Panel – procura por tags ou por nome
        if (inventoryPanel == null)
            inventoryPanel = GameObject.FindWithTag("InventoryPanel");
        if (tutorialPanel == null)
            tutorialPanel = GameObject.FindWithTag("TutorialPanel");

        // 5) UI Root – por defeito, procura o Canvas
        if (uiRoot == null)
            uiRoot = FindFirstObjectByType<Canvas>()?.transform;

        // 6) inventário
        //inventoryController = FindFirstObjectByType<InventoryController>();
    }

    void Start()
    {
        // 1) Valida prefab e UI root
        if (objDialoguePanelPrefab == null || uiRoot == null)
        {
            Debug.LogError("Prefab ou UI Root em falta!", this);
            enabled = false;
            return;
        }

        // 2) Instancia o painel de diálogo
        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            uiRoot,
            worldPositionStays: false
        );
        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        objDialoguePanelInstance.SetActive(false);
        objDialogueText.text = "";

        // 3) Separa as linhas de diálogo
        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];
    }

    void OnEnable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue += CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue += HandleResume;
    }

    void OnDisable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue -= CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue -= HandlePause;
        DialogueManager.Instance.OnResumeDialogue -= HandleResume;
    }

    // ICancelableDialogue
    public void CancelDialogue() => EndDialogue();

    // pausa temporária
    private void HandlePause()
    {
        if (!objIsDialogueActive) return;
        StopAllCoroutines();
        objDialoguePanelInstance.SetActive(false);
    }

    // retoma o typing
    private void HandleResume()
    {
        if (!objIsDialogueActive) return;
        objDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    // IInteractable
    public bool CanInteract() => !objIsDialogueActive;

    public void Interact()
    {
        // checa o item no inventário
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

        // solicita cancelamento de outros diálogos
        DialogueManager.Instance?.RequestNewDialogue(this);

        // escolhe que conjunto de linhas mostrar
        selectedDialogueLines = hasItem ? rightDialogue : wrongDialogue;

        // avança a linha ou inicia diálogo
        if (objIsDialogueActive) NextLine();
        else StartObjDialogue();
    }

    private void StartObjDialogue()
    {
        objIsDialogueActive = true;
        objDialogueIndex = 0;
        objDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    private void NextLine()
    {
        if (objIsTyping)
        {
            StopAllCoroutines();
            objDialogueText.text = selectedDialogueLines[objDialogueIndex];
            objIsTyping = false;
        }
        else if (++objDialogueIndex < selectedDialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else EndDialogue();
    }

    private IEnumerator TypeLine()
    {
        objIsTyping = true;
        objDialogueText.text = "";
        foreach (char ch in selectedDialogueLines[objDialogueIndex])
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
        objDialoguePanelInstance.SetActive(false);

        // só dispara a mudança de cena se estiver no diálogo “certo”
        if (selectedDialogueLines == rightDialogue)
        {
            ClearAllDialoguePanels();

            // Aqui já tens o confiner e mapBoundary atribuídos em Awake()
            confiner.BoundingShape2D = mapBoundary;
            CheckpointManager.I.SaveCheckpoint(checkpointID);
            CheckpointManager.I.LoadCheckpoint(checkpointID);

            // destrói o tutorialPanel se existir na cena
            if (tutorialPanel != null)
                Destroy(tutorialPanel);
        }
    }

    void ClearAllDialoguePanels()
    {
        // procura recursivamente todos os childs taggeados
        foreach (var tr in uiRoot.GetComponentsInChildren<Transform>(true))
        {
            if (tr.CompareTag("DialoguePanel"))
            {
                Destroy(tr.gameObject);
            }
        }
    }


}