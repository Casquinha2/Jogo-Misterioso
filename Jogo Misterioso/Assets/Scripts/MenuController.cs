using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class MenuController : MonoBehaviour
{
    public GameObject menuCanvas;
    public GameObject tutorialPanel;
    public TMP_Text tutorialText;
    
    // Reference to the dialogue panel that you want hidden by default in new scenes.
    public GameObject objDialoguePanel;
    
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }
    
    void Start()
    {
        menuCanvas.SetActive(false);
        if (tutorialPanel)
        {
            tutorialPanel.SetActive(true);
        }
    }
    
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // When any new scene loads, hide the dialogue panel.
        // You can add a condition here if you only want to hide it 
        // for specific scenes (e.g., if (scene.name == "Piso1Scene") {...})
        if (objDialoguePanel)
        {
            objDialoguePanel.SetActive(false);
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