using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TabController : MonoBehaviour
{
    public TextMeshProUGUI[] tabText;
    public GameObject[] pages;
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
        for(int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(false);
        }

        if (tabNo == 3){
            ExitGame();
        }else{
            pages[tabNo].SetActive(true);
        }
    }

    public void Back(int tabNo)
    {
        pages[tabNo].SetActive(false);
    }

    void ExitGame()
    {
        #if (UNITY_EDITOR)
            UnityEditor.EditorApplication.isPlaying = false;
        #elif (UNITY_STANDALONE)
            Application.Quit();
        #elif (Unity_WEBGL)
            SceneManager.LoadScene("QuitScene");
        #endif
    }
}
