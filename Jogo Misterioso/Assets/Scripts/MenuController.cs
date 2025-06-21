using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    private GameObject tutorialPanel;

    TMP_Text tutorialText;

    void Start()
    {
        // encontra o painel e o texto pela tag
        if (tutorialPanel == null)
            tutorialPanel = GameObject.FindWithTag("TutorialPanel");

        tutorialText = tutorialPanel.GetComponentInChildren<TMP_Text>();

        // mostra sempre o painel no arranque
        tutorialPanel.SetActive(true);

        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        // Tab/Esc só alterna o painel de tutorial, não o menu
        if (Keyboard.current.tabKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (tutorialText.text != "TUTORIAL ACABADO")
                tutorialPanel.SetActive(!tutorialPanel.activeSelf);
        }
    }
}