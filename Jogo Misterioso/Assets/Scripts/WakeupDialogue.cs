using UnityEngine;
using System.Collections;

public class WakeupDialogue : MonoBehaviour
{
    [Header("Identificador único deste diálogo")]
    [SerializeField] private string dialogueKey = "WakeupDialogue1";

    private ObjDialogue objDialogue;

    void Start()
    {
        // se já mostramos antes, destrói imediatamente
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
        // espera um frame para ObjDialogue.Start() rodar e
        // evitar capturar o cancelamento automático
        yield return null;

        objDialogue.Interact();

        // só agora inscreve no evento de fim de diálogo verdadeiro
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

        // marca como mostrado e destrói
        WakeUpTracker.Shown.Add(dialogueKey);
        Destroy(gameObject);
    }
}
