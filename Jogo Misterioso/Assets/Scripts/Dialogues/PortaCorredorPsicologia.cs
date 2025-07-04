using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Cinemachine;
using System.Collections;
using System.Linq;

public class PortaCorredorPsicologia : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Dados de Diálogo")]
    [SerializeField] private ObjectInteractionDialogue objDialogueData;
    [SerializeField] private int indexDialogueCerto;

    [Header("Prefab UI (Panel + TMP_Text)")]
    [SerializeField] private GameObject objDialoguePanelPrefab;

    [Header("Item (obrigatório)")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Transição de cena")]
    [SerializeField] private string nameMapBound;
    [SerializeField] private bool telaCarregamento = false;
    [SerializeField] private Vector2 coordenadas;

    [Header("Persistência em sessão")]
    [Tooltip("Identificador único deste puzzle na sessão")]
    [SerializeField] private string puzzleID;
    private bool jaResolvido;

    // referências internas…
    private Transform uiRoot;
    private GameObject inventoryPanel;
    private GameObject player;
    private PolygonCollider2D mapBoundary;
    private CinemachineConfiner2D confiner;
    private GameObject objDialoguePanelInstance;
    private GameObject blackPanel;
    private TMP_Text objDialogueText;
    private string[] rightDialogue, wrongDialogue, selectedDialogueLines;
    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;

    void Start()
    {
        if (itemPrefab == null)
        {
            Debug.LogError("❌ itemPrefab não atribuído em PortaInteraction! Desligando script...", this);
            enabled = false;
            return;
        }

        player = GameObject.FindWithTag("Player");
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();

        var mbRoot = GameObject.Find("MapBounds")?.transform;
        if (mbRoot == null)
        {
            Debug.LogError("Não encontrei o GameObject 'MapBounds'!", this);
            return;
        }

        var bound = mbRoot
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == nameMapBound);
        if (bound == null)
        {
            Debug.LogError($"Não encontrei o child {bound} dentro de MapBounds!", this);
            return;
        }

        mapBoundary = bound.GetComponent<PolygonCollider2D>();
        if (mapBoundary == null)
        {
            Debug.LogError($"O GameObject {bound} não possui PolygonCollider2D!", this);
            return;
        }

        inventoryPanel = GameObject.FindWithTag("InventoryPanel");
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
            Debug.LogError("❌ InventoryPanel (mesmo inativo) não encontrado!", this);

        GameObject uiGo = GameObject.FindWithTag("UICanvas");
        if (uiGo == null)
        {
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.CompareTag("UICanvas"))
                {
                    uiGo = go;
                    break;
                }
        }
        uiRoot = uiGo?.transform;

        var allUiTs = uiRoot.GetComponentsInChildren<Transform>(true);
        var loadT = allUiTs.FirstOrDefault(t => t.name == "Loading");
        if (loadT != null)
        {
            blackPanel = loadT.gameObject;
            blackPanel.SetActive(false);
            LoadingManager.RegisterPanel(blackPanel);

        }


        if (objDialoguePanelPrefab == null)
        {
            Debug.LogError("❌ Prefab de diálogo não atribuído!", this);
            enabled = false;
            return;
        }
        if (uiRoot == null)
        {
            Debug.LogError("❌ Canvas (UICanvas) não encontrado!", this);
            enabled = false;
            return;
        }

        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            uiRoot,
            worldPositionStays: false
        );
        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        objDialoguePanelInstance.SetActive(false);
        objDialogueText.text = "";

        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];

        // 1) Checa se já foi resolvido nesta sessão
        if (!string.IsNullOrEmpty(puzzleID) &&
            SessionState.solvedPuzzles.Contains(puzzleID))
        {
            jaResolvido = true;
            ForceDisableInteraction();
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

    public bool CanInteract()
    {
        if (jaResolvido) 
            return false;
        return !objIsDialogueActive;
    }
    public void Interact()
    {
        bool hasItem = false;
        if (inventoryPanel != null)
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
        }

        DialogueManager.Instance.RequestNewDialogue(this);
        selectedDialogueLines = hasItem ? rightDialogue : wrongDialogue;

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
        else
        {
            EndDialogue();
        }
    }

    private IEnumerator TypeLine()
    {
        objIsTyping = true;
        objDialogueText.text = "";
        foreach (var ch in selectedDialogueLines[objDialogueIndex])
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
        if (!objIsDialogueActive) return;
        EndDialogue();
    }

    private void HandlePause()
    {
        if (!objIsDialogueActive) return;
        StopAllCoroutines();
        objDialoguePanelInstance.SetActive(false);
    }

    private void HandleResume()
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

        if (selectedDialogueLines == rightDialogue)
        {
            // limpa painéis antigos…
            if (!jaResolvido && !string.IsNullOrEmpty(puzzleID))
            {
                jaResolvido = true;
                // 2) marca resolvido apenas na memória desta sessão
                SessionState.solvedPuzzles.Add(puzzleID);
                ForceDisableInteraction();
            }

            StartCoroutine(DoTransitionToPiso1());
        }
    }

    

    private IEnumerator DoTransitionToPiso1()
    {
        if (blackPanel != null && telaCarregamento)
            LoadingManager.ShowLoading();

        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        // reposiciona player antes de continuar…
        yield return null;
        player = GameObject.FindWithTag("Player");
        if (player)
            player.transform.position = new Vector3(coordenadas.x, coordenadas.y, 0f);

        if (telaCarregamento)
        {
            LoadingManager.SubscribeAutoHide();
            LoadingManager.HideLoadingWithDelay(this);
        }
    }

    private void ForceDisableInteraction()
    {
        // bloqueia CanInteract() e collider
        enabled = false;
        if (TryGetComponent<Collider2D>(out var c))
            c.enabled = false;
    }
}