// MenuController.cs
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;

    void Start()
    {
        menuCanvas.SetActive(false);
        tutorialPanel?.SetActive(true);
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool open = !menuCanvas.activeSelf;
            menuCanvas.SetActive(open);

            if (tutorialPanel && tutorialText.text != "TUTORIAL ACABADO")
                tutorialPanel.SetActive(!tutorialPanel.activeSelf);

            Time.timeScale = open ? 0 : 1;

            if (open) DialogueManager.Instance?.PauseDialogue();
            else      DialogueManager.Instance?.ResumeDialogue();
        }
    }
}