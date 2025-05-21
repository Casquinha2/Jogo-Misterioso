using UnityEngine;
using System.Collections;
using TMPro;

public class PortaInteraction : MonoBehaviour, IInteractable
{
    public ObjectInteractionDialogue objDialogueData;
    public int indexDialogueCerto; // Determines where to slice the dialogueLines array.
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
        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];   // Elements from 0 to index-1.
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];    // Elements from index to end.
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
                Item inventoryItem = slot.currentItem.GetComponent<Item>();
                if (inventoryItem != null)
                {
                    // Get the desired item ID from the prefab
                    int desiredItemID = itemPrefab.GetComponent<Item>().ID;
                    
                    // Compare the inventory item's ID with the desired item's ID
                    if (inventoryItem.ID == desiredItemID)
                    {
                        hasItem = true;

                        // Remove the item visually by setting the parent to null
                        slot.currentItem.transform.SetParent(null); 

                        // Destroy the GameObject (removes it from the UI)
                        Destroy(slot.currentItem);

                        // Clear the slot reference so the inventory knows it's empty
                        slot.currentItem = null;


                        //ir para proximo mapa



                        break;
                    }

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
