using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using Unity.Cinemachine;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

public class PortaInteraction : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Dados de Diálogo")]
    public ObjectInteractionDialogue objDialogueData;
    public int indexDialogueCerto;

    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    [Header("Item (obrigatório)")]
    [Tooltip("Arrasta aqui o prefab do item que será entregue ao jogador")]
    public GameObject itemPrefab;

    // ↓ ↓ ↓ campos resolvidos em Start() via Tags ↓ ↓ ↓
    private Transform uiRoot;
    private GameObject inventoryPanel;
    private GameObject tutorialPanel;
    private GameObject player;
    private PolygonCollider2D mapBoundary;
    private CinemachineConfiner2D confiner;

    // Estado interno do diálogo
    private GameObject objDialoguePanelInstance;
    private GameObject blackPanel;
    private TMP_Text objDialogueText;
    private string[]   rightDialogue, wrongDialogue, selectedDialogueLines;
    private int        objDialogueIndex;
    private bool       objIsTyping, objIsDialogueActive;

    // Cena fixa de destino
    private const string piso1SceneName = "Piso1Scene";
    private bool shouldSetupAfterLoad = false;

    private Transform piso1;

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

        piso1 = mbRoot
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "Piso 1");
        if (piso1 == null)
        {
            Debug.LogError("Não encontrei o child '62' dentro de MapBounds!", this);
            return;
        }

        var bound62 = piso1
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "62");
        if (bound62 == null)
        {
            Debug.LogError("Não encontrei o child '62' dentro de MapBounds!", this);
            return;
        }

        mapBoundary = bound62.GetComponent<PolygonCollider2D>();
        if (mapBoundary == null)
        {
            Debug.LogError("O GameObject '62' não possui PolygonCollider2D!", this);
            return;
        }

        // InventoryPanel (pode estar inativo)
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

        // TutorialPanel
        tutorialPanel = GameObject.FindWithTag("TutorialPanel");
        if (tutorialPanel == null)
        {
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.CompareTag("TutorialPanel"))
                {
                    tutorialPanel = go;
                    break;
                }
        }

        // UI Root (Canvas)
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

        // logo após:
        var allUiTs = uiRoot.GetComponentsInChildren<Transform>(true);
        var loadT   = allUiTs.FirstOrDefault(t => t.name == "Loading");
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

        // Instancia painel de diálogo (inativo inicialmente)
        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            uiRoot,
            worldPositionStays: false
        );
        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        objDialoguePanelInstance.SetActive(false);
        objDialogueText.text = "";

        // Separa linhas de diálogo
        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];
    }

     void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   += CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue+= HandleResume;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (DialogueManager.Instance == null) return;
        DialogueManager.Instance.OnNewDialogue   -= CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue -= HandlePause;
        DialogueManager.Instance.OnResumeDialogue-= HandleResume;
    }

    public bool CanInteract() => !objIsDialogueActive;

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
        else                     StartObjDialogue();
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

    public void CancelDialogue()
    {
        if (!objIsDialogueActive || objDialoguePanelInstance == null)
        return;

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

        // Se acertou, dispara a transição
        if (selectedDialogueLines == rightDialogue)
        {
            // limpa diálogos antigos
            foreach (var tr in uiRoot.GetComponentsInChildren<Transform>(true))
                if (tr.CompareTag("DialoguePanel"))
                    Destroy(tr.gameObject);

            // inicia o processo de loading + reposição + troca de cena
            StartCoroutine(DoTransitionToPiso1());
        }
    }

    private IEnumerator DoTransitionToPiso1()
    {
        // 1) mostra o loading
        if (blackPanel != null)
            LoadingManager.ShowLoading();


        piso1.gameObject.SetActive(true);

        // 2) espera um frame pra garantir que o UI realmente apareça
        yield return null;

        // 3) reposiciona player antes do unload
        player = GameObject.FindWithTag("Player");
        if (player)
            player.transform.position = new Vector3(-1.47f, 1.90f, 0);

        // 4) sinaliza que, no OnSceneLoaded, devemos ajustar confiner e tutorial
        shouldSetupAfterLoad = true;

        // 5) dispara o load (síncrono)
        SceneManager.LoadScene(piso1SceneName);
        LoadingManager.SubscribeAutoHide();
        LoadingManager.HideLoadingWithDelay(this);

    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!shouldSetupAfterLoad || scene.name != piso1SceneName) return;
        shouldSetupAfterLoad = false;

        // ajusta confiner
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        var bound62 = GameObject
            .Find("MapBounds")?
            .GetComponentsInChildren<PolygonCollider2D>(true)
            .FirstOrDefault(c => c.gameObject.name == "62");
        if (confiner != null && bound62 != null)
        {
            confiner.BoundingShape2D = bound62;
            confiner.InvalidateBoundingShapeCache();
        }

        // warpa a câmera
        var vcam = FindFirstObjectByType<CinemachineCamera>();
        player = GameObject.FindWithTag("Player");
        if (vcam != null && player != null)
            vcam.OnTargetObjectWarped(player.transform, Vector3.zero);

        // destrói tutorial
        tutorialPanel = GameObject.FindWithTag("TutorialPanel");
        if (tutorialPanel) Destroy(tutorialPanel);

        // desativa loading 1 segundo depois
        StartCoroutine(HideLoadingAfterDelay());
    }

    private IEnumerator HideLoadingAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (blackPanel != null)
            blackPanel.SetActive(false);

    }
}