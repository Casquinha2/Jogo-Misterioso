using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    
    // Event used when a dialogue is about to start.
    public event Action OnNewDialogue;

    // Static reference to the currently active dialogue (could be set to a common interface)
    public static ICancelableDialogue ActiveDialogue { get; private set; }



    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Debug.Log("DialogueManager Instance initialized: " + Instance);
        }
        else
        {
            Debug.LogWarning("Duplicate DialogueManager found and destroyed!");
            Destroy(gameObject);
        }
    }


    // Call this method before starting any new dialogue.
    public void RequestNewDialogue(ICancelableDialogue newDialogue)
    {

        Debug.Log("New dialogue requested: " + newDialogue);

        // First, cancel any active dialogue.
        if (ActiveDialogue != null && ActiveDialogue != newDialogue)
        {
            MonoBehaviour mb = ActiveDialogue as MonoBehaviour;
            if (mb != null && mb.gameObject != null)
            {
                ActiveDialogue.CancelDialogue();
            }
            ActiveDialogue = newDialogue;
        }
        ActiveDialogue = newDialogue;
        OnNewDialogue?.Invoke();
    }
}
