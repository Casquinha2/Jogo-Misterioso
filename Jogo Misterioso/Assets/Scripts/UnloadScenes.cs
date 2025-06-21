using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SceneMaganer : MonoBehaviour
{
    [Header("Checkpoint DB (arrasta aqui)")]
    public CheckpointDatabase checkpointDB;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneUnloaded += OnSceneUnloaded;

        // 1) Bootstrapping do checkpoint
        string lastId = PlayerPrefs.GetString("LastCheckpoint", "Inicio");
        var cp = checkpointDB.checkpoints.FirstOrDefault(c => c.id == lastId);
        if (cp != null)
        {
            Debug.Log($"[GameManager] Carregando cena “{cp.sceneName}” (checkpoint {lastId})");
            SceneManager.LoadScene(cp.sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning($"[GameManager] Checkpoint “{lastId}” não encontrado, fica na cena atual.");
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void Start()
    {
        // podes comentar este Start se já estiveres a usar LoadSceneMode.Single no Awake
        if (SceneManager.GetSceneByName("Piso1Scene").isLoaded)
        {
            Debug.Log("[GameManager] Piso1Scene está carregada. Vou descarregar…");
            SceneManager.UnloadSceneAsync("Piso1Scene");
        }
        else
        {
            Debug.Log("[GameManager] Piso1Scene não está carregada ao Start()");
        }
    }

    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[GameManager] Cena “{scene.name}” descarregada. A limpar DialoguePanels…");
        var panels = GameObject.FindGameObjectsWithTag("DialoguePanel");
        foreach (var p in panels)
            Destroy(p);
    }
}
