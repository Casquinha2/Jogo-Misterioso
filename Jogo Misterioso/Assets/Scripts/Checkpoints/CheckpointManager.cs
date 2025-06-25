using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Unity.Cinemachine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager I { get; private set; }

    [SerializeField] CheckpointDatabase database;
    [SerializeField] Transform mapBoundsRoot;
    [SerializeField] CinemachineConfiner2D confiner;

    string targetId;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void LoadCheckpoint(string id)
    {
        var cp = database.checkpoints.FirstOrDefault(c => c.id == id);
        if (cp == null)
        {
            Debug.LogError($"[CPM] Não encontrei checkpoint com id '{id}' no DB!");
            return;
        }
        else
            Debug.Log($"[CPM] Carreguei checkpoint '{id}' na cena '{cp.sceneName}' em {cp.SpawnPos}");


        // guarda o id para usar no callback
        targetId = id;

        if (SceneManager.GetActiveScene().name == cp.sceneName)
        {
            // mesma cena → só faz warp
            StartCoroutine(DoWarp(cp));
        }
        else
        {
            // cena diferente → espera carregar
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(cp.sceneName, LoadSceneMode.Single);
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // desregista já para não chamar várias vezes
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // recupera o checkpoint pela targetId
        var cp = database.checkpoints.First(c => c.id == targetId);
        StartCoroutine(DoWarp(cp));
    }

    IEnumerator DoWarp(Checkpoint cp)
    {
        // espera um frame para todos os objetos instanciar
        yield return null;

        // 1) Teleporta o player como já fazia  
        var player = GameObject.FindWithTag("Player");
        var rb2d   = player.GetComponent<Rigidbody2D>();
        rb2d.simulated = false;
        rb2d.position  = cp.SpawnPos;
        rb2d.linearVelocity  = Vector2.zero;
        rb2d.simulated = true;
        Physics2D.SyncTransforms();

        // 2) Atenção: atualiza o confiner para o polígono deste checkpoint
        //    Assumindo que em MapBounds há um child com o mesmo nome de cp.displayName:
        var boundTransform = mapBoundsRoot
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == cp.displayName);
        if (boundTransform == null)
            Debug.LogError($"Não achei MapBounds/{cp.displayName}");
        else
        {
            var poly = boundTransform.GetComponent<PolygonCollider2D>();
            if (poly == null)
                Debug.LogError($"'{cp.displayName}' não tem PolygonCollider2D");
            else
            {
                confiner.BoundingShape2D = poly;
                confiner.InvalidateBoundingShapeCache();
                Debug.Log($"Confiner agora em '{cp.displayName}'");
            }
        }
    }

    #if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (database == null) return;
        Gizmos.color = Color.magenta;
        foreach (var cp in database.checkpoints)
            if (cp.id != "1")
            {
                Gizmos.DrawSphere(cp.SpawnPos, 0.1f);
            }
            

    }
    #endif
}
