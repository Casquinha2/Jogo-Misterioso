using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TabController : MonoBehaviour
{
    public TextMeshProUGUI[] tabText;
    public GameObject[] pages;

    [SerializeField] private SaveController saveController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }
    }

    public void ActivateTab(int tabNo)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        if (tabNo == 3)
        {
            ExitGame();
        }
        else if (tabNo == 2 && SceneManager.GetActiveScene().name == "QuartoScene")
        {
            return;
        }
        else
        {
            pages[tabNo].SetActive(true);
        }
    }

    public void Back(int tabNo)
    {
        pages[tabNo].SetActive(false);
    }

    void ExitGame()
    {
        saveController.SaveGame();
        
        #if (UNITY_EDITOR)
        UnityEditor.EditorApplication.isPlaying = false;
        #elif (UNITY_STANDALONE)
            Application.Quit();
        #elif (Unity_WEBGL)
            SceneManager.LoadScene("QuitScene");
        #endif
    }
}
