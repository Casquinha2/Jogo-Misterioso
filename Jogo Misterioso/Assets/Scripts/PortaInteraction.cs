using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;

public class PortaInteraction : MonoBehaviour, IInteractable, ICancelableDialogue
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
    public GameObject tutorialPanel;
    public GameObject player;
    [SerializeField] PolygonCollider2D mapBoundry;
    public CinemachineConfiner2D confiner;

    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
        rightDialogue = objDialogueData.dialogueLines[..indexDialogueCerto];
        wrongDialogue = objDialogueData.dialogueLines[indexDialogueCerto..];
    }
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
    public void CancelDialogue()
    {
        Debug.Log("Cancelling dialogue on: " + gameObject.name);
        if (gameObject == null || !this) return;

        EndDialogue();
    }

    public bool CanInteract()
    {
        return !objIsDialogueActive;
    }
    public void Interact()
    {
        bool hasItem = false;
        // Inventory check 
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item inventoryItem = slot.currentItem.GetComponent<Item>();
                if (inventoryItem != null)
                {
                    int desiredItemID = itemPrefab.GetComponent<Item>().ID;
                    if (inventoryItem.ID == desiredItemID)
                    {
                        hasItem = true;
                        slot.currentItem.transform.SetParent(null);
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                        if (objDialoguePanel.activeSelf)
                        {
                            StopAllCoroutines();
                            objIsDialogueActive = false;
                            objDialogueText.SetText("");
                            objDialoguePanel.SetActive(false);
                        }
                        break;
                    }
                }
            }
        }

        // Before starting the door dialogue, cancel other active dialogues. 
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.RequestNewDialogue(this);



        selectedDialogueLines = hasItem ? rightDialogue : wrongDialogue;

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
            dialogueText.SetText(selectedDialogueLines[objDialogueIndex]);
            objIsTyping = false;
        }
        else if (++objDialogueIndex < selectedDialogueLines.Length)
        {
            StartCoroutine(TypeLine(dialogueText));
        }
        else
        {
            EndDialogue();
        }
}
    IEnumerator TypeLine(TMP_Text dialogueText)
    {
        objIsTyping = true;
        dialogueText.SetText("");
        foreach (char letter in selectedDialogueLines[objDialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(objDialogueData.typingSpeed);
        }
        objIsTyping = false;
        if (objDialogueData.autoProgressLines.Length > objDialogueIndex && objDialogueData.autoProgressLines[objDialogueIndex])
        {
            yield return new WaitForSeconds(objDialogueData.autoProgressDelay);
            NextLine(dialogueText);
        }
    }

    //[System.Obsolete]
    public void EndDialogue()
    {
        StopAllCoroutines();
        objIsDialogueActive = false;
        objDialogueText.SetText("");
        objDialoguePanel.SetActive(false);

        if (selectedDialogueLines == rightDialogue)
        {
            confiner.BoundingShape2D = mapBoundry;
            Destroy(tutorialPanel);
            player.transform.position = new Vector3(-1.47f, 1.9f, 0f);
            // Now safely load the scene
            SceneManager.LoadScene("Piso1Scene");
        }
    }
}