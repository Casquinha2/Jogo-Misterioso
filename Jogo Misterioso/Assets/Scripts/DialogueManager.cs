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
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this method before starting any new dialogue.
    public void RequestNewDialogue(ICancelableDialogue newDialogue)
    {
        // First, cancel any active dialogue.
        if (ActiveDialogue != null && ActiveDialogue != newDialogue)
        {
            ActiveDialogue.CancelDialogue();
        }
        ActiveDialogue = newDialogue;
        OnNewDialogue?.Invoke();
    }
}
