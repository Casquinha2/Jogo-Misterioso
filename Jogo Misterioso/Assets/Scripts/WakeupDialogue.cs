using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WakeupDialogue : MonoBehaviour
{
    private bool hasShown = false;
    private ObjDialogue objDialogue;

    void Start()
    {
        objDialogue = GetComponent<ObjDialogue>();
        StartCoroutine(DelayedInteract());
    }

    IEnumerator DelayedInteract()
    {
        // espera um frame para todos os ObjDialogue.Start() acontecerem
        yield return null;

        // agora dispara de forma segura
        if (!hasShown)
        {
            objDialogue.Interact();
            hasShown = true;
        }
    }
}
