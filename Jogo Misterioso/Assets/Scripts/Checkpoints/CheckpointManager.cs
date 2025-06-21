using System.Collections;      // ← para IEnumerator
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;              // ← (opcional, se precisares de LINQ aqui também)


public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager I { get; private set; }
    [SerializeField] CheckpointDatabase database;

    void Awake()
    {
        if (I == null) { I = this; DontDestroyOnLoad(this); }
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
        if (cp == null) return;

        var current = SceneManager.GetActiveScene().name;
        if (current == cp.sceneName)
        {
            Debug.Log($"[CheckpointManager] Já estou em {current}, não recarrego.");
            StartCoroutine(AfterLoadPosition(cp));
            return;
        }

        Debug.Log($"[CheckpointManager] LoadScene({cp.sceneName})");
        SceneManager.LoadScene(cp.sceneName, LoadSceneMode.Single);
        StartCoroutine(AfterLoadPosition(cp));
    }


    IEnumerator AfterLoadPosition(Checkpoint cp)
    {
        yield return null; // espera a cena carregar
        var p = GameObject.FindWithTag("Player");
        if (p != null) p.transform.position = cp.SpawnPos;
    }
}

