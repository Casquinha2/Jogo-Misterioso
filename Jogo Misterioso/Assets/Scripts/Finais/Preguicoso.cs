using UnityEngine;
using UnityEngine.SceneManagement;

public class Preguicoso : MonoBehaviour
{
    public Canvas canvas;
    public GameObject player;
    void Awake()
    {
        canvas.gameObject.SetActive(false);
    }

    public void Restart()
    {
        canvas.gameObject.SetActive(false);
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
