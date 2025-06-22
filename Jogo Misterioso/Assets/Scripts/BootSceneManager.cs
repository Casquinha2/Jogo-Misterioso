using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class BootSceneManager : MonoBehaviour
{
    [Header("Prefabs Persistentes")]
    public GameObject[] persistentPrefabs;

    [Header("Cenas")]
    public string initialScene = "QuartoScene";      // onde estão os teus prefabs originais
    public CheckpointDatabase checkpointDB;          // arrasta o CheckpointDB.asset

    bool didInit = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 1) Instancia e marca todos os teus prefabs persistentes
        foreach (var prefab in persistentPrefabs)
        {
            var go = Instantiate(prefab);
            DontDestroyOnLoad(go);
        }

        // 2) Começa o pipeline de load/unload
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(initialScene, LoadSceneMode.Additive);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // só queremos atuar quando a initialScene terminar de carregar
        if (didInit || scene.name != initialScene) return;
        didInit = true;

        // 3) Descarrega a cena de QuartoScene agora que já temos os persistentPrefabs
        SceneManager.UnloadSceneAsync(initialScene);

        // 4) Lê o checkpoint e carrega a cena certa em modo Single
        string lastId = PlayerPrefs.GetString("LastCheckpoint", "Inicio");
        var cp = checkpointDB.checkpoints.FirstOrDefault(c => c.id == lastId);
        string targetScene = cp != null ? cp.sceneName : initialScene;
        SceneManager.LoadScene(targetScene, LoadSceneMode.Single);

        // 5) limpa este listener
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
