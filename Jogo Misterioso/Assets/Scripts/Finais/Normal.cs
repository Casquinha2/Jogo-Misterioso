using UnityEngine;
using UnityEngine.SceneManagement;

public class Normal : MonoBehaviour
{
    [SerializeField] private Progress progress;

    [SerializeField] private Canvas escolha;
    [SerializeField] private Canvas fim;

    void Start()
    {
        escolha.gameObject.SetActive(false);
        fim.gameObject.SetActive(false);
    }
    public void Final()
    {
        PauseController.SetPause(true);
        escolha.gameObject.SetActive(true);
    }

    public void Acabar()
    {
        escolha.gameObject.SetActive(false);
        fim.gameObject.SetActive(true);
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

    public void Restart()
    {
        fim.gameObject.SetActive(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
