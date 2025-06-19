// DialogueManager.cs
using UnityEngine;
using System;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    // Disparado antes de qualquer diálogo novo (para cancelar os em curso)
    public event Action OnNewDialogue;
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
        // cancela todos os diálogos em curso
        OnNewDialogue?.Invoke();
        // e depois quem pediu arranca o seu diálogo
        requester.CancelDialogue();  
    }

    public void PauseDialogue()  => OnPauseDialogue?.Invoke();
    public void ResumeDialogue() => OnResumeDialogue?.Invoke();
}
