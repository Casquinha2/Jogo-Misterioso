using UnityEngine;
using UnityEngine.SceneManagement;
public class UnloadScenes : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneManager.UnloadSceneAsync("Piso1Scene");
    }
}
