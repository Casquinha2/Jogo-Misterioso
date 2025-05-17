using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class TutorialDialogue : MonoBehaviour
{
    public ObjectInteractionDialogue tutorialDialogueData;
    public GameObject tutorialDialoguePanel;
    public TMP_Text tutorialDialogueText;
    
    private int tutorialDialogueIndex;
    private bool tutorialIsTyping, tutorialIsDialogueActive;
    
    // Use a HashSet to ensure keys are tracked uniquely
    HashSet<int> teste = new HashSet<int>();

    void Start()
    {
        tutorialDialoguePanel.SetActive(true);
        StartTutorialDialog();
    }

    void Update() {
        // Add unique key presses to the HashSet
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            teste.Add(1);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            teste.Add(2);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            teste.Add(3);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            teste.Add(4);
        }
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            teste.Add(5);
        }
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            teste.Add(6);
        }

        // Only check input after the current line finished typing
        if (!tutorialIsTyping)
        {
            if (tutorialDialogueIndex == 0 && new HashSet<int> { 1, 2, 3, 4 }.IsSubsetOf(teste))
            {
                NextLine();
            }
            else if (tutorialDialogueIndex == 1 && teste.Contains(5))
            {
                NextLine();
            }
            else if (tutorialDialogueIndex == 2 && teste.Contains(6))
            {
                NextLine();
            }
        }
    }

    void StartTutorialDialog()
    {
        tutorialIsDialogueActive = true;
        tutorialDialogueIndex = 0;
        teste.Clear(); // Clear any previous key inputs
        StartCoroutine(TypeLine());
    }
    
    void NextLine()
    {
        // If the text is still being typed, complete it immediately.
        if (tutorialIsTyping)
        {
            StopAllCoroutines();
            tutorialDialogueText.SetText(tutorialDialogueData.dialogueLines[tutorialDialogueIndex]);
            tutorialIsTyping = false;
        }
        else if (++tutorialDialogueIndex < tutorialDialogueData.dialogueLines.Length)
        {
            // Clear the input for the next dialogue line
            teste.Clear();
            StartCoroutine(TypeLine());
        }
        else
        {
            EndobjDialogue();
        }
    }

    IEnumerator TypeLine() 
    {
        tutorialIsTyping = true;
        tutorialDialogueText.SetText("");
        foreach (char letter in tutorialDialogueData.dialogueLines[tutorialDialogueIndex])
        {
            tutorialDialogueText.text += letter;
            yield return new WaitForSeconds(tutorialDialogueData.typingSpeed);
        }
        tutorialIsTyping = false;
    }

    public void EndobjDialogue()
    {
        StopAllCoroutines();
        tutorialIsDialogueActive = false;
        tutorialDialogueText.SetText("");
        tutorialDialoguePanel.SetActive(false);
    }
}
