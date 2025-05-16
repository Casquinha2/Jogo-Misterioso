using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System;

public class TutorialDialogue : MonoBehaviour
{
    public ObjectInteractionDialogue tutorialDialogueData;
    public GameObject tutorialDialoguePanel;
    public TMP_Text tutorialDialogueText;
    
    private int tutorialDialogueIndex;
    private bool tutorialIsTyping, tutorialIsDialogueActive, wpress, apress, spress, dpress, uppress, downpress, leftpress, rightpress, epress, enterpress;

    void Start()
    {
        tutorialDialoguePanel.SetActive(true);
        wpress = uppress = false;
        apress = leftpress = false;
        spress = downpress = false;
        dpress = rightpress = false;
        epress = enterpress = false;

    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            wpress = uppress = true;
        }
        else if(Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            apress = leftpress = true;
        }
        else if(Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            spress = downpress = true;
        }
        else if(Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            dpress = rightpress = true;
        }
        else if(Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            epress = enterpress = true;
        }
    }



    void StartTutorialDialog()
    {
        tutorialIsDialogueActive = true;
        tutorialDialogueIndex = 0;

        //nameText.SetText(objDialogueData.name);

        tutorialDialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (tutorialIsTyping)
        {
            StopAllCoroutines();
            tutorialDialogueText.SetText(tutorialDialogueData.dialogueLines[tutorialDialogueIndex]);
            tutorialIsTyping = false;
        }
        else if (++tutorialDialogueIndex < tutorialDialogueData.dialogueLines.Length)
        {
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

        foreach(char letter in tutorialDialogueData.dialogueLines[tutorialDialogueIndex])
        {
            tutorialDialogueText.text += letter;
            yield return new WaitForSeconds(tutorialDialogueData.typingSpeed);
        }

        tutorialIsTyping = false;

        if(tutorialDialogueData.autoProgressLines.Length > tutorialDialogueIndex && tutorialDialogueData.autoProgressLines[tutorialDialogueIndex])
        {
            yield return new WaitForSeconds(tutorialDialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndobjDialogue()
    {
        StopAllCoroutines();
        tutorialIsDialogueActive = false;
        tutorialDialogueText.SetText("");
        tutorialDialoguePanel.SetActive(false);

    }

}
