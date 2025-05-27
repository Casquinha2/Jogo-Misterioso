using UnityEngine;
using System.Collections;
using TMPro;
public class ObjDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
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
    // Subscribe to the global dialogue cancellation event. 
    void OnEnable()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnNewDialogue += CancelDialogue;
    }
    void OnDisable()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnNewDialogue -= CancelDialogue;
    }
    // This method is called when any dialogue is about to start. 
    public void CancelDialogue()
    {
        EndDialogue();
    }
    public bool CanInteract()
    {
        return !objIsDialogueActive;
    }
    public void Interact()
    {
        // Notify all dialogues (including this one) that a new dialogue is about to start 
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.RequestNewDialogue(this);

        // Check for key/item logic. 
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
                        // Get the desired item ID from the prefab. 
                        int desiredItemID = itemPrefab.GetComponent<Item>().ID;
                        // Compare the inventory item's ID with the desired item's ID. 
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
        // Continue with dialogue. 
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
            EndDialogue();
        }
    }
    IEnumerator TypeLine()
    {
        objIsTyping = true;
        objDialogueText.SetText("");
        foreach (char letter in objDialogueData.dialogueLines[objDialogueIndex])
        {
            objDialogueText.text += letter;
            yield return new WaitForSeconds(objDialogueData.typingSpeed);
        }
        objIsTyping = false;
        if (objDialogueData.autoProgressLines.Length > objDialogueIndex && objDialogueData.autoProgressLines[objDialogueIndex])
        {
            yield return new WaitForSeconds(objDialogueData.autoProgressDelay);
            NextLine();
        }
    }
    public void EndDialogue()
    {
        StopAllCoroutines();
        objIsDialogueActive = false;
        objDialogueText.SetText("");
        objDialoguePanel.SetActive(false);
    }
}