using UnityEngine;
using System.Collections;
using TMPro;

public class ObjDialogue : MonoBehaviour, IInteractable
{
    public ObjectInteractionDialogue objDialogueData;
    public GameObject objDialoguePanel;
    public TMP_Text objDialogueText;
    
    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;

    private InventoryController inventoryController;
    public GameObject itemPrefab;

    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
    }
    public bool CanInteract()
    {
        return !objIsDialogueActive;
    }

    public void Interact()
    {
        if (itemPrefab)
        {
            inventoryController.AddItem(itemPrefab);
        }
        

        if (objIsDialogueActive)
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
        objIsDialogueActive = true;
        objDialogueIndex = 0;

        //nameText.SetText(objDialogueData.name);

        objDialoguePanel.SetActive(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (objIsTyping)
        {
            StopAllCoroutines();
            objDialogueText.SetText(objDialogueData.dialogueLines[objDialogueIndex]);
            objIsTyping = false;
        }
        else if (++objDialogueIndex < objDialogueData.dialogueLines.Length)
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
        objIsTyping = true;
        objDialogueText.SetText("");

        foreach(char letter in objDialogueData.dialogueLines[objDialogueIndex])
        {
            objDialogueText.text += letter;
            yield return new WaitForSeconds(objDialogueData.typingSpeed);
        }

        objIsTyping = false;

        if(objDialogueData.autoProgressLines.Length > objDialogueIndex && objDialogueData.autoProgressLines[objDialogueIndex])
        {
            yield return new WaitForSeconds(objDialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void EndobjDialogue()
    {
        StopAllCoroutines();
        objIsDialogueActive = false;
        objDialogueText.SetText("");
        objDialoguePanel.SetActive(false);

    }

}
