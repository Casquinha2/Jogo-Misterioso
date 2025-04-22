using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class ObjectInteraction1 : MonoBehaviour, IInteractable
{
    public ObjectInteraction objdialogueData;
    public GameObject objdialoguePanel;
    public TMP_Text objdialogueText;

    private int objdialogueIndex;
    private bool objisTyping, objisDialogueActive;

    public bool CanInteract()
    {
        return !objisDialogueActive;
    }

    public void Interact()
    {
        if (objisDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartObjDialog();
        }
    }

    void StartObjDialog()
    {
        objisDialogueActive = true;
        objdialogueIndex = 0;

        objdialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (objisTyping)
        {
            StopAllCoroutines();
            objdialogueText.SetText(objdialogueData.dialogueLines[objdialogueIndex]);
            objisTyping = false;
        }
        else if (++objdialogueIndex < objdialogueData.dialogueLines.Length)
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
        objisTyping = true;
        objdialogueText.SetText("");

        foreach(char letter in objdialogueData.dialogueLines[objdialogueIndex])
        {
            objdialogueText.text += letter;
            yield return new WaitForSeconds(objdialogueData.typingSpeed);
        }

        objisTyping = false;

        if(objdialogueData.autoProgressLines.Length > objdialogueIndex && objdialogueData.autoProgressLines[objdialogueIndex])
        {
            yield return new WaitForSeconds(objdialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndobjDialogue()
    {
        StopAllCoroutines();
        objisDialogueActive = false;
        objdialogueText.SetText("");
        objdialoguePanel.SetActive(false);

    }

}
