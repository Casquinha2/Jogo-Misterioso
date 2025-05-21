using UnityEngine;
using System.Collections;
using TMPro;

public class ObjDialogue : MonoBehaviour, IInteractable
{
    public ObjectInteractionDialogue objDialogueData;
    public GameObject objDialoguePanel;
    public TMP_Text objDialogueText;
    
    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive, hasItem;

    private InventoryController inventoryController;
    public GameObject itemPrefab;
    public GameObject inventoryPanel;

    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        hasItem = false;
    }
    public bool CanInteract()
    {
        return !objIsDialogueActive;
    }

    public void Interact()
    {
        if (itemPrefab)
        {   
            
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
                            break;
                        }

                    }
                }
            }

            if (!hasItem)
            {
                inventoryController.AddItem(itemPrefab);
            }
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
