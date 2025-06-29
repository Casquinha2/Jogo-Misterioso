using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Cinemachine;
using System.Collections;
using System.Linq;

public class PortaUniversal : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Dados de Diálogo")]
    [SerializeField] private ObjectInteractionDialogue objDialogueData;
    [SerializeField] private int indexDialogueCerto;

    [Header("Prefab UI (Panel + TMP_Text)")]
    [SerializeField] private GameObject objDialoguePanelPrefab;

    [Header("Item (obrigatório)")]
    [Tooltip("Arrasta aqui o prefab do item que será entregue ao jogador")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Transição de Cena")]
    [SerializeField] private string nameMapBound;
    [SerializeField] private bool telaCarregamento = false;
    [SerializeField] private Vector2 coordenadas;

    [Header("Adicionar progresso?")]
    [SerializeField] private bool adicionarProgresso = false;

    // Referências resolvidas em Start()
    private Transform uiRoot;
    private GameObject inventoryPanel;
    private GameObject player;
    private PolygonCollider2D mapBoundary;
    private CinemachineConfiner2D confiner;
    private Progress progress;

    // Estado interno do diálogo
    private GameObject objDialoguePanelInstance;
    private GameObject blackPanel;
    private TMP_Text objDialogueText;
    private string[] rightDialogue, wrongDialogue, selectedDialogueLines;
    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;

    void Start()
    {
        // Progress
        if (adicionarProgresso)
        {
            progress = FindFirstObjectByType<Progress>();
            if (progress == null)
                Debug.LogWarning("⚠️ adicionarProgresso está ativado, mas não há nenhum Progress na cena.", this);
        }

        // Item obrigatório
        if (itemPrefab == null)
        {
            Debug.LogError("❌ itemPrefab não atribuído em PortaUniversal! Desligando script...", this);
            enabled = false;
            return;
        }

        // Player e Cinemachine
        player = GameObject.FindWithTag("Player");
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();

        // Bound do mapa
        var mbRoot = GameObject.Find("MapBounds")?.transform;
        if (mbRoot == null)
        {
            Debug.LogError("Não encontrei o GameObject 'MapBounds'!", this);
            return;
        }
        var bound = mbRoot.GetComponentsInChildren<Transform>(true)
                        .FirstOrDefault(t => t.name == nameMapBound);
        if (bound == null)
        {
            Debug.LogError($"Não encontrei o child '{nameMapBound}' dentro de MapBounds!", this);
            return;
        }
        mapBoundary = bound.GetComponent<PolygonCollider2D>();
        if (mapBoundary == null)
        {
            Debug.LogError($"O GameObject '{nameMapBound}' não possui PolygonCollider2D!", this);
            return;
        }

        // InventoryPanel (pode estar inativo)
        inventoryPanel = GameObject.FindWithTag("InventoryPanel");
        if (inventoryPanel == null)
        {
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                if (t.CompareTag("InventoryPanel"))
                {
                    inventoryPanel = t.gameObject;
                    break;
                }
        }
        if (inventoryPanel == null)
            Debug.LogError("❌ InventoryPanel (mesmo inativo) não encontrado!", this);

        // UI Root (UICanvas)
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
        if (uiGo == null)
        {
            Debug.LogError("❌ Canvas (UICanvas) não encontrado!", this);
            enabled = false;
            return;
        }
        uiRoot = uiGo.transform;

        // Loading panel
        var loadT = uiRoot.GetComponentsInChildren<Transform>(true)
                          .FirstOrDefault(t => t.name == "Loading");
        if (loadT != null)
        {
            blackPanel = loadT.gameObject;
            blackPanel.SetActive(false);
            LoadingManager.RegisterPanel(blackPanel);
        }

        // Instancia painel de diálogo (inativo)
        if (objDialoguePanelPrefab == null)
        {
            Debug.LogError("❌ Prefab de diálogo não atribuído!", this);
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

        // Divide as linhas
        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];
    }

    private void OnEnable()
    {
        DialogueManager.Instance.OnNewDialogue += HandleAnyDialogueStarted;
    }

    private void OnDisable()
    {
        DialogueManager.Instance.OnNewDialogue -= HandleAnyDialogueStarted;
    }



    public bool CanInteract() => !objIsDialogueActive;

    public void Interact()
    {
        // Checa inventário
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

        // Inicia diálogo
        DialogueManager.Instance.RequestNewDialogue(this);
        selectedDialogueLines = hasItem ? rightDialogue : wrongDialogue;

        if (objIsDialogueActive) NextLine();
        else
        {
            StartObjDialogue();
            if (!hasItem) StartCoroutine(AutoCloseDialogue(2f));
        }
    }

    private IEnumerator AutoCloseDialogue(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }

    private void StartObjDialogue()
    {
        objIsDialogueActive = true;
        objDialogueIndex    = 0;
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
    private void HandleAnyDialogueStarted(ICancelableDialogue startedBy)
    {
        // ignora se quem pediu foi você mesmo
        if (ReferenceEquals(startedBy, this)) return;
        CancelDialogue();
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

        // Se diálogo “certo”, avança transição
        if (selectedDialogueLines == rightDialogue)
        {
            // limpa painéis antigos
            foreach (var tr in uiRoot.GetComponentsInChildren<Transform>(true))
                if (tr.CompareTag("DialoguePanel"))
                    Destroy(tr.gameObject);

            if (adicionarProgresso && progress != null)
                progress.AddProgress();

            StartCoroutine(DoTransition());
        }
    }

    private IEnumerator DoTransition()
    {
        if (blackPanel != null && telaCarregamento)
            LoadingManager.ShowLoading();

        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        yield return null; // garante frame pra UI

        // Reposiciona jogador
        player = GameObject.FindWithTag("Player");
        if (player != null)
            player.transform.position = new Vector3(coordenadas.x, coordenadas.y, 0f);

        if (telaCarregamento)
        {
            LoadingManager.SubscribeAutoHide();
            LoadingManager.HideLoadingWithDelay(this);
        }
    }
}
