// DialogueManager.cs
using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // Disparado antes de qualquer diálogo novo (para cancelar os em curso)
    public event Action<ICancelableDialogue> OnNewDialogue;

    // Disparado quando o jogo é pausado (menu aberto)
    public event Action OnPauseDialogue;
    // Disparado quando o jogo é retomado (menu fechado)
    public event Action OnResumeDialogue;

    void Awake()
    {
        // garante que o objeto que vai ser preservado é o root
        var go = this.gameObject.transform.root.gameObject;
        DontDestroyOnLoad(go);
        Instance = this;
    }


    // Chama-se sempre que um ICancelableDialogue quer começar
    public void RequestNewDialogue(ICancelableDialogue requester)
    {
        // avisa listeners, informando quem solicitou
        OnNewDialogue?.Invoke(requester);

        // destrói antigos panels...
        var panels = GameObject.FindGameObjectsWithTag("DialoguePanel");
        for (int i = 0; i < panels.Length; i++)
            Destroy(panels[i]);
    }


    public void PauseDialogue()  => OnPauseDialogue?.Invoke();
    public void ResumeDialogue() => OnResumeDialogue?.Invoke();
}
