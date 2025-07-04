using UnityEngine;
using UnityEngine.SceneManagement;

public class DialogueCleanup : MonoBehaviour
{
    void Awake()
    {
        // Garante que este objeto viva entre cenas
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CleanupDialoguePanels();
    }

    /// <summary>
    /// Encontra todos os objetos taggeados como "DialoguePanel" e os destrói.
    /// </summary>
    public void CleanupDialoguePanels()
    {
        var panels = GameObject.FindGameObjectsWithTag("DialoguePanel");
        foreach (var panel in panels)
            Destroy(panel);
    }
}
