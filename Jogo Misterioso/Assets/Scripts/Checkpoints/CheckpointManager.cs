using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager I { get; private set; }

    [SerializeField] CheckpointDatabase database;

    // guarda o checkpoint que queremos carregar
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

    public void SaveCheckpoint(string id)
    {
        PlayerPrefs.SetString("LastCheckpoint", id);
        PlayerPrefs.Save();
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

        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[CheckpointManager] Player não encontrado.");
            yield break;
        }

        var rb2d = player.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            // desliga a simulação só neste frame
            rb2d.simulated = false;
            rb2d.position       = cp.SpawnPos;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.simulated = true;

            // força o motor 2D a reconhecer a posição agora
            Physics2D.SyncTransforms();
            Debug.Log($"[CheckpointManager] Player teleportado para {cp.SpawnPos}");
        }
        else
        {
            player.transform.position = cp.SpawnPos;
            Debug.Log($"[CheckpointManager] Player movido (sem físico) para {cp.SpawnPos}");
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
