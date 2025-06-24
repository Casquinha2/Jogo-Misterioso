using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using System.Linq;
using System.Collections;

[DefaultExecutionOrder(-100)]
public class GameSceneManager : MonoBehaviour
{
    [Header("DB de Checkpoints")]
    public CheckpointDatabase checkpointDB;

    [Header("Camera")]
    public CinemachineConfiner2D confiner;
    public Transform            mapBoundsRoot;
    private Transform boundNode;

    // guarda o checkpoint que queremos atingir
    string targetCheckpointId;
    bool   waitingForTargetScene;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // Determina qual o checkpoint que está em PlayerPrefs
        string lastId = PlayerPrefs.GetString("LastCheckpoint",
                          checkpointDB.checkpoints[0].id);
        targetCheckpointId = lastId;

        // Procura o checkpoint no DB
        var cp = checkpointDB.checkpoints
                    .FirstOrDefault(c => c.id == lastId);

        // Se for cena diferente, subscreve só agora o callback
        if (cp != null && SceneManager.GetActiveScene().name != cp.sceneName)                  
        {
            waitingForTargetScene = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(cp.sceneName, LoadSceneMode.Single);
        }
        else // já estamos na cena certa: só faz a inicialização inline
        {
            StartCoroutine(AfterSceneLoad());
        }
    }

    void OnDestroy()
    {
        // segurança
        if (waitingForTargetScene)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Só dispara quando entra uma cena nova
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ignora todos exceto o que realmente queremos
        var cp = checkpointDB.checkpoints
                    .FirstOrDefault(c => c.id == targetCheckpointId);
        if (!waitingForTargetScene || cp == null || scene.name != cp.sceneName)
            return;

        // garantimos que só tratamos esta callback uma vez
        waitingForTargetScene = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        StartCoroutine(AfterSceneLoad());
    }

    IEnumerator AfterSceneLoad()
    {
        // espera um frame para tudo instanciar
        yield return null;

        // reposiciona o Player
        var cp = checkpointDB.checkpoints
                    .First(c => c.id == targetCheckpointId);
        var player = GameObject.FindWithTag("Player");
        if (player) player.transform.position = cp.SpawnPos;

        // espera outro frame para o collider aparecer
        yield return null;

        // e só agora fazes o bounds certo
        UpdateCameraBounds(cp);
    }

    void UpdateCameraBounds(Checkpoint cp)
    {
        if (confiner == null || mapBoundsRoot == null)
        {
            Debug.LogWarning("[GSM] Confiner ou MapBounds não configurados.");
            return;
        }

        var nodeName = cp.displayName;


        // "Quarto" é filho direto de MapBounds, então não precisa buscar mais fundo
        boundNode = mapBoundsRoot
            .GetComponentsInChildren<Transform>(true) // inclui inativos
            .FirstOrDefault(t => t.name == nodeName && t != mapBoundsRoot);

        if (boundNode == null)
        {
            Debug.LogError($"[GSM] Não encontrei '{nodeName}' em MapBounds (mesmo em profundidade).");
            return;
        }

        var poly = boundNode.GetComponent<PolygonCollider2D>();
        if (poly == null)
        {
            Debug.LogError($"[GSM] '{nodeName}' não tem PolygonCollider2D.");
            return;
        }

        confiner.BoundingShape2D = poly;
        Debug.Log($"[GSM] Confiner agora usa o bounding “{nodeName}”.");
    }

}
