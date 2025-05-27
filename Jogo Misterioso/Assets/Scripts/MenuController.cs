using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuController : MonoBehaviour
{
    
    public GameObject menuCanvas;
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    void Start()
    {
        menuCanvas.SetActive(false);
        if (tutorialPanel)
        {
            tutorialPanel.SetActive(true);            
        }


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            menuCanvas.SetActive(!menuCanvas.activeSelf);

            if (tutorialPanel && tutorialText.text != "TUTORIAL ACABADO")
            {
                tutorialPanel.SetActive(!tutorialPanel.activeSelf);
            }

            // If menu is active, pause; if it's inactive, unpause
            Time.timeScale = menuCanvas.activeSelf ? 0 : 1;
        }
    }

}
