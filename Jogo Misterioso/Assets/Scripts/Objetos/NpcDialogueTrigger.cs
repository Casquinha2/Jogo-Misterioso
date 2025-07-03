using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

[RequireComponent(typeof(BoxCollider2D))]
public class NpcDialogueTrigger : MonoBehaviour
{
    [Tooltip("Arraste aqui o componente NpcDialogue (pode estar no mesmo GameObject ou em um filho).")]
    public NpcDialogue npcDialogue;

    [SerializeField] private PolygonCollider2D   mapBoundary;
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private CinemachineCamera     virtualCamera; // CemichineCamera é o correto
    [SerializeField] private GameObject            panel;
    [SerializeField] private string                sceneToLoad;
    [SerializeField] Transform mapBoundRoot;

    private Vector3 inicialPlayerCoords;
    private GameObject player;
    private bool dialogueFinished;

    void Reset()
    {
        // Garante que o collider seja trigger
        var bc = GetComponent<BoxCollider2D>();
        bc.isTrigger = true;
    }

    void Awake()
    {
        // Se você não tiver atribuído no inspector, tenta achar automaticamente
        if (npcDialogue == null)
            npcDialogue = GetComponentInChildren<NpcDialogue>();
        if (npcDialogue == null)
            Debug.LogError($"[Trajado 1] NpcDialogue não encontrado em {name}", this);


        player = GameObject.FindWithTag("Player");
        if (player == null)
            Debug.LogError("[Trajado 1] Player não encontrado!", this);

    }

    void OnEnable()
    {
        // Guarda a posição do player assim que ativas este objecto (quando apanha a capa)
        inicialPlayerCoords = player.transform.position;
        Debug.Log($"[Trajado 1] posição inicial guardada = {inicialPlayerCoords}");
    }
    void OnDisable()
    {
        NpcDialogue.OnDialogueEnded -= HandleDialogueEnded;
    }
    private void HandleDialogueEnded(NpcDialogue dlg)
    {
        // só queremos a notificação do nosso NPC
        if (dlg == npcDialogue)
        {
            dialogueFinished = true;
            NpcDialogue.OnDialogueEnded -= HandleDialogueEnded;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && npcDialogue != null)
        {
            dialogueFinished = false;
            NpcDialogue.OnDialogueEnded += HandleDialogueEnded;

            npcDialogue.Interact();
            StartCoroutine(ResetCheckpoint());
        }
    }

    IEnumerator ResetCheckpoint()
    {
        yield return new WaitUntil(() => dialogueFinished);

        PauseController.SetPause(true);
        panel.SetActive(true);

        Transform piso2 = mapBoundRoot.Find("Piso 2");
        piso2.gameObject.SetActive(true);

        // 2) Espera breve em tempo real
        yield return new WaitForSecondsRealtime(1f);

        // 3) Carregar nova cena
        var loadOp = SceneManager.LoadSceneAsync(sceneToLoad);
        loadOp.allowSceneActivation = true;
        yield return loadOp;  // aguarda até a cena estar ativa

        // antes de mover o player
        Vector3 oldPos = player.transform.position;
        Vector3 newPos = inicialPlayerCoords;
        Vector3 delta  = newPos - oldPos;

        // teleporta
        player.transform.position = newPos;

        // agora faz o warp
        virtualCamera.OnTargetObjectWarped(player.transform, delta);
        Debug.Log($"[Trajado 1] Warping camera por delta = {delta}");

        // 6) Reconfigurar o confiner em alguns frames
        yield return null;
        confiner.gameObject.SetActive(true);
        yield return null;
        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        foreach (Transform child in mapBoundRoot)
        {
            if (child.gameObject.name != "Piso 2")
            {
                child.gameObject.SetActive(false);
            }
        }

        // 7) Fecha UI e despausa após um tempinho
        yield return new WaitForSecondsRealtime(1f);
        panel.SetActive(false);
        PauseController.SetPause(false);
    }
}

