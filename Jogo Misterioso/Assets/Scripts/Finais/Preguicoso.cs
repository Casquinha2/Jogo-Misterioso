using UnityEngine;
using UnityEngine.SceneManagement;

public class Preguicoso : MonoBehaviour
{
    public GameObject canvas;
    void Awake()
    {
        canvas.SetActive(false);
    }

    public void Restart()
    {
        canvas.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void ExitGame()
    {
        #if (UNITY_EDITOR)
            UnityEditor.EditorApplication.isPlaying = false;
        #elif (UNITY_STANDALONE)
            Application.Quit();
        #elif (Unity_WEBGL)
            SceneManager.LoadScene("QuitScene");
        #endif
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;
        canvas.gameObject.SetActive(true);
        PauseController.SetPause(true);
    }
}
