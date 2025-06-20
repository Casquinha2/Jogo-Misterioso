using UnityEngine;
using UnityEngine.SceneManagement;

public class UnloadScenes : MonoBehaviour
{
    void Awake()
    {
        // torna este objecto persistente
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneUnloaded -= OnSceneUnloaded;
    }

    void Start()
    {
        // apenas para o teu unload inicial
        if (SceneManager.GetSceneByName("Piso1Scene").isLoaded)
        {
            Debug.Log("[UnloadScenes] Piso1Scene está carregada. Vou descarregar…");
            SceneManager.UnloadSceneAsync("Piso1Scene");
        }
        else
        {
            Debug.Log("[UnloadScenes] Piso1Scene não está carregada ao Start()");
        }
    }

    
    void OnSceneUnloaded(Scene scene)
    {
        Debug.Log($"[UnloadScenes] Cena “{scene.name}” foi descarregada. A limpar dialog panels…");

        // 1) procura por tag
        var panelsByTag = GameObject.FindGameObjectsWithTag("DialoguePanel");
        Debug.Log($"[UnloadScenes] Encontrei {panelsByTag.Length} panels com tag “DialoguePanel”.");
        foreach (var p in panelsByTag)
        {
            Debug.Log($"[UnloadScenes] A destruir panel (tag): {p.name}");
            Destroy(p);
        }
    }
    
}
