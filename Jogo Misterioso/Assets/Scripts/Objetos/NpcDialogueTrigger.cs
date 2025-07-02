using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class NpcDialogueTrigger : MonoBehaviour
{
    [Tooltip("Arraste aqui o componente NpcDialogue (pode estar no mesmo GameObject ou em um filho).")]
    public NpcDialogue npcDialogue;

    void Reset()
    {
        // Garante que o collider seja trigger
        var bc = GetComponent<BoxCollider2D>();
        bc.isTrigger = true;
    }

    void Awake()
    {
        // Se você não tiver atribuído no inspector, tenta achar automaticamente
        if (npcDialogue == null)
            npcDialogue = GetComponentInChildren<NpcDialogue>();
        if (npcDialogue == null)
            Debug.LogError($"NpcDialogue não encontrado em {name}", this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && npcDialogue != null)
        {
            npcDialogue.Interact();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && npcDialogue != null)
        {
            npcDialogue.EndDialogue();
        }
    }
}

