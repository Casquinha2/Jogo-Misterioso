using UnityEngine;
using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

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

    [Header("Checkpoint ID")]
    [SerializeField] private string checkpointID;

    // ↓ ↓ ↓ campos resolvidos em Start() via Tags ↓ ↓ ↓
    private Transform uiRoot;                     
    private GameObject inventoryPanel;            
    private GameObject tutorialPanel;             
    private GameObject player;                   
    private PolygonCollider2D mapBoundary;        
    private CinemachineConfiner2D confiner;       

    // Estado interno
    private GameObject objDialoguePanelInstance;
    private TMP_Text   objDialogueText;
    private string[]   rightDialogue, wrongDialogue, selectedDialogueLines;
    private int        objDialogueIndex;
    private bool       objIsTyping, objIsDialogueActive;

<<<<<<<< HEAD:Jogo Misterioso/Assets/Scripts/PortaInteraction.cs
========
    // Cena fixa de destino
    private const string piso1SceneName = "Piso1Scene";
    private bool shouldSetupAfterLoad = false;

    private Transform piso1;

>>>>>>>> 4856127 (adicionei personagens nas assets e irmao no jogo):Jogo Misterioso/Assets/Scripts/Dialogues/PortaInteraction.cs
    void Start()
    {
        // ——— Validação obrigatória do itemPrefab ———
        if (itemPrefab == null)
        {
            Debug.LogError("❌ itemPrefab não atribuído em PortaInteraction! Desligando script...", this);
            enabled = false;
            return;
        }

        // 1) Player
        player = GameObject.FindWithTag("Player");

        // 2) Confiner
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();

        // 3) MapBoundary, dentro de "MapBounds"/checkpointID
        var mbRoot = GameObject.Find("MapBounds")?.transform;
        if (mbRoot != null)
        {
            var node = mbRoot.Find(checkpointID);
            if (node != null)
                mapBoundary = node.GetComponent<PolygonCollider2D>();
        }

<<<<<<<< HEAD:Jogo Misterioso/Assets/Scripts/PortaInteraction.cs
        // 4) InventoryPanel (ativo ou inativo)
========
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
>>>>>>>> 4856127 (adicionei personagens nas assets e irmao no jogo):Jogo Misterioso/Assets/Scripts/Dialogues/PortaInteraction.cs
        inventoryPanel = GameObject.FindWithTag("InventoryPanel");
        if (inventoryPanel == null)
        {
        // vai buscar TODOS os Transforms, inclusive inativos
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

        // 5) TutorialPanel (ativo ou inativo)
        tutorialPanel = GameObject.FindWithTag("TutorialPanel");
        if (tutorialPanel == null)
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.CompareTag("TutorialPanel"))
                {
                    tutorialPanel = go;
                    break;
                }

        // 6) UI Root → Canvas marcado como "UICanvas"
        GameObject uiGo = GameObject.FindWithTag("UICanvas");
        if (uiGo == null)
            foreach (var go in SceneManager.GetActiveScene().GetRootGameObjects())
                if (go.CompareTag("UICanvas"))
                {
                    uiGo = go;
                    break;
                }
        uiRoot = uiGo?.transform;

        // ——— Validações iniciais ———
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

        // 7) instancia o painel de diálogo (o prefab já traz tag "DialoguePanel")
        objDialoguePanelInstance = Instantiate(
            objDialoguePanelPrefab,
            uiRoot,
            worldPositionStays: false
        );
        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        objDialoguePanelInstance.SetActive(false);
        objDialogueText.text = "";

        // 8) separa linhas de diálogo certo/errado
        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];
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
        // 1) verifica se o jogador tem o item (sempre obrigatório)
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

        // 2) cancela outros diálogos e limpa painéis
        DialogueManager.Instance.RequestNewDialogue(this);

        // 3) escolhe as linhas
        selectedDialogueLines = hasItem ? rightDialogue : wrongDialogue;

        // 4) inicia ou avança
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
            StartCoroutine(TypeLine());
        else
            EndDialogue();
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

    public void CancelDialogue() => EndDialogue();

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

        // se acertou → confiner, checkpoint e remove tutorial
        if (selectedDialogueLines == rightDialogue)
        {


            
            /*

                FAZEWWR AQUI TELA FICAR PRETA PARA UPDATE

            */




            foreach (var tr in uiRoot.GetComponentsInChildren<Transform>(true))
                if (tr.CompareTag("DialoguePanel"))
                    Destroy(tr.gameObject);

            if (confiner != null && mapBoundary != null)
                confiner.BoundingShape2D = mapBoundary;

            if (!string.IsNullOrEmpty(checkpointID))
            {
                CheckpointManager.I.SaveCheckpoint(checkpointID);
                CheckpointManager.I.LoadCheckpoint(checkpointID);
            }

            if (tutorialPanel != null)
                Destroy(tutorialPanel);
        }
    }
<<<<<<<< HEAD:Jogo Misterioso/Assets/Scripts/PortaInteraction.cs
========

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
>>>>>>>> 4856127 (adicionei personagens nas assets e irmao no jogo):Jogo Misterioso/Assets/Scripts/Dialogues/PortaInteraction.cs
}