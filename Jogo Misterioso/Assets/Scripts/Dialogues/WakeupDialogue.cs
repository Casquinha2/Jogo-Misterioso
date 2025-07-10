using UnityEngine;
using System.Collections;

public class WakeupDialogue : MonoBehaviour
{
    [Header("Identificador único deste diálogo")]
    [SerializeField] private string dialogueKey = "WakeupDialogue1";

    private ObjDialogue objDialogue;

    void Start()
    {
        // Se já mostramos antes, destrói imediatamente
        if (WakeUpTracker.Shown.Contains(dialogueKey))
        {
            Destroy(gameObject);
            return;
        }

        objDialogue = GetComponent<ObjDialogue>();
        StartCoroutine(DelayedInteractAndSubscribe());
    }

    IEnumerator DelayedInteractAndSubscribe()
    {
        yield return null;  // espera ObjDialogue.Start()

        objDialogue.Interact();
        ObjDialogue.OnDialogueEnded += HandleEnd;
    }

    void OnDisable()
    {
        ObjDialogue.OnDialogueEnded -= HandleEnd;
    }

    private void HandleEnd(ObjDialogue ended)
    {
        if (ended != objDialogue) 
            return;

        // Marca como mostrado e destroi
        WakeUpTracker.Shown.Add(dialogueKey);
        Destroy(gameObject);
    }
}
