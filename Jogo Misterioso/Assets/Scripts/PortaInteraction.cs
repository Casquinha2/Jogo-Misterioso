using UnityEngine;
using System.Collections;
using TMPro;

public class PortaInteraction : MonoBehaviour, IInteractable
{
    public ObjectInteractionDialogue objDialogueData;
    public int index; // Determines where to slice the dialogueLines array.
    public GameObject objDialoguePanel;
    public TMP_Text objDialogueText; // Single TMP_Text for displaying dialogue.

    private string[] selectedDialogueLines;
    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;
    private string[] wrongDialogue, rightDialogue;

    private InventoryController inventoryController;
    public GameObject itemPrefab;
    public GameObject inventoryPanel;

    void Start()
    {
        // Initialize your inventory controller.
        inventoryController = FindFirstObjectByType<InventoryController>();
        
        // Slice dialogueLines into right and wrong sections.
        rightDialogue = objDialogueData.dialogueLines[..index];   // Elements from 0 to index-1.
        wrongDialogue = objDialogueData.dialogueLines[index..];    // Elements from index to end.
    }

    public bool CanInteract()
    {
        return !objIsDialogueActive;
    }

    public void Interact()
    {
        bool hasItem = false;
        // Inventory check: Does the inventory contain the item?
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Debug.Log($"Slot item name: {slot.currentItem.name}");
                if (slot.currentItem.name == itemPrefab.name)
                {
                    hasItem = true;
                    Debug.Log("Key detected in inventory!");
                    break;
                }
            }
        }


        // Select the proper dialogue array based on the inventory check.
        selectedDialogueLines = hasItem ? rightDialogue : wrongDialogue;

        // Use the same TMP_Text (objDialogueText) to display the dialogue.
        if (objIsDialogueActive)
        {
            NextLine(objDialogueText);
        }
        else
        {
            StartObjDialogue(objDialogueText);
        }        
    }

    void StartObjDialogue(TMP_Text dialogueText)
    {
        objIsDialogueActive = true;
        objDialogueIndex = 0;
        objDialoguePanel.SetActive(true);
        StartCoroutine(TypeLine(dialogueText));
    }

    void NextLine(TMP_Text dialogueText)
    {
        if (objIsTyping)
        {
            StopAllCoroutines();
            // Instantly display the full line if the player presses to advance.
            dialogueText.SetText(selectedDialogueLines[objDialogueIndex]);
            objIsTyping = false;
        }
        else if (++objDialogueIndex < selectedDialogueLines.Length)
        {
            StartCoroutine(TypeLine(dialogueText));
        }
        else
        {
            EndDialogue(dialogueText);
        }
    }

    IEnumerator TypeLine(TMP_Text dialogueText)
    {
        objIsTyping = true;
        dialogueText.SetText("");

        // Iterate over each character in the current line.
        foreach (char letter in selectedDialogueLines[objDialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(objDialogueData.typingSpeed);
        }

        objIsTyping = false;

        // Automatically progress the dialogue if auto-progression is configured.
        if (objDialogueData.autoProgressLines.Length > objDialogueIndex &&
            objDialogueData.autoProgressLines[objDialogueIndex])
        {
            yield return new WaitForSeconds(objDialogueData.autoProgressDelay);
            NextLine(dialogueText);
        }
    }

    public void EndDialogue(TMP_Text dialogueText)
    {
        StopAllCoroutines();
        objIsDialogueActive = false;
        dialogueText.SetText("");
        objDialoguePanel.SetActive(false);
    }
}
