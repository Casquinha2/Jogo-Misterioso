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
    public Transform mapBoundsRoot;

    string targetCheckpointId;
    bool waitingForTargetScene;

    public static bool isReady = false;

    void Awake()
    {
        isReady = false;

        DontDestroyOnLoad(gameObject);

        string lastId = PlayerPrefs.GetString("LastCheckpoint",
                        checkpointDB.checkpoints[0].id);
        targetCheckpointId = lastId;

        var cp = checkpointDB.checkpoints
                    .FirstOrDefault(c => c.id == lastId);

        if (cp != null && SceneManager.GetActiveScene().name != cp.sceneName)
        {
            waitingForTargetScene = true;
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(cp.sceneName, LoadSceneMode.Single);
        }
        else
        {
            // Se já está na cena correta, já avisa que está pronto
            isReady = true;
            // Dispara o warp direto no checkpoint atual
            CheckpointManager.I.LoadCheckpoint(targetCheckpointId);
        }
    }

    void OnDestroy()
    {
        if (waitingForTargetScene)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var cp = checkpointDB.checkpoints
                    .FirstOrDefault(c => c.id == targetCheckpointId);
        if (!waitingForTargetScene || cp == null || scene.name != cp.sceneName)
            return;

        waitingForTargetScene = false;
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // Sinaliza ready e dispara warp no CheckpointManager
        isReady = true;
        CheckpointManager.I.LoadCheckpoint(targetCheckpointId);
    }
}
