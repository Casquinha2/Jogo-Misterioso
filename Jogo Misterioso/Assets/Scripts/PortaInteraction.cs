using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class PortaInteraction : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Dados de Diálogo")]
    public ObjectInteractionDialogue objDialogueData;
    public int indexDialogueCerto;   // ponto de corte nas linhas

    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    [Header("Inventário e Outros")]
    public GameObject itemPrefab;
    public GameObject inventoryPanel;
    public GameObject tutorialPanel;
    public GameObject player;
    public PolygonCollider2D mapBoundary;
    public CinemachineConfiner2D confiner;

    // instâncias criadas em runtime
    private GameObject objDialoguePanelInstance;
    private TMP_Text objDialogueText;

    private string[] rightDialogue, wrongDialogue, selectedDialogueLines;
    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;
    private InventoryController inventoryController;

    void Start()
    {
        // 1) Valida prefab
        if (objDialoguePanelPrefab == null)
        {
            Debug.LogError("Arraste o prefab de diálogo em objDialoguePanelPrefab!", this);
            enabled = false;
            return;
        }

        // 2) Instancia o painel dentro do Canvas
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Não encontrei Canvas na cena.", this);
            enabled = false;
            return;
        }

        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            canvas.transform,
            worldPositionStays: false
        );

        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        if (objDialogueText == null)
        {
            Debug.LogError("O prefab não tem TMP_Text em filho!", this);
            enabled = false;
            return;
        }

        objDialoguePanelInstance.SetActive(false);
        objDialogueText.text = "";

        // 3) Separa as linhas em “errado” e “certo”
        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];

        // 4) Inventário
        inventoryController = FindFirstObjectByType<InventoryController>();
    }

    void OnEnable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   += CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue += HandleResume;
    }

    void OnDisable()
    {
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   -= CancelDialogue;
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
        objDialogueText.text = "";
        objDialoguePanelInstance.SetActive(false);

        // se passou no teste, destrava e troca de cena
        if (selectedDialogueLines == rightDialogue)
        {
            confiner.BoundingShape2D = mapBoundary;
            Destroy(tutorialPanel);
            player.transform.position = new Vector3(-1.47f, 1.9f, 0f);
            SceneManager.LoadScene("Piso1Scene");
        }
    }
}