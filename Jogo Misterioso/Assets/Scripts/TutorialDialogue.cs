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
    private bool tutorialIsTyping;
    
    // Use a HashSet to ensure keys are tracked uniquely
    private HashSet<int> inputs = new HashSet<int>();

    void Start()
    {
        tutorialDialoguePanel.SetActive(true);
        StartTutorialDialog();
    }

    void Update() {
        // Add unique key presses to the HashSet
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            inputs.Add(1);
        }
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            inputs.Add(2);
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            inputs.Add(3);
        }
        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            inputs.Add(4);
        }
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            inputs.Add(5);
        }
        if (Input.GetKeyDown(KeyCode.Tab) || Input.GetKeyDown(KeyCode.Escape))
        {
            inputs.Add(6);
        }

        // Only check input after the current line finished typing
        if (!tutorialIsTyping)
        {
            if (tutorialDialogueIndex == 0 && new HashSet<int> { 1, 2, 3, 4 }.IsSubsetOf(inputs))
            {
                NextLine();
            }
            else if (tutorialDialogueIndex == 1 && inputs.Contains(5))
            {
                NextLine();
            }
            else if (tutorialDialogueIndex == 2 && inputs.Contains(6))
            {
                NextLine();
            }
        }
    }

    void StartTutorialDialog()
    {
        tutorialDialogueIndex = 0;
        inputs.Clear(); // Clear any previous key inputs
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
            inputs.Clear();
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
        
        if (tutorialDialogueData.autoProgressLines.Length > tutorialDialogueIndex && tutorialDialogueData.autoProgressLines[tutorialDialogueIndex])
        {
            yield return new WaitForSeconds(tutorialDialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndobjDialogue()
    {
        StopAllCoroutines();
        tutorialDialogueText.SetText("");
        tutorialDialoguePanel.SetActive(false);
    }
}
