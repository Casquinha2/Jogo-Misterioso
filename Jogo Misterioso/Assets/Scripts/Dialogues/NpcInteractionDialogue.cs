using UnityEngine;

[CreateAssetMenu(fileName = "NewNpcInteraction", menuName = "Dialogue/NPC Interaction")]
public class NpcInteractionDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public float typingSpeed = 0.05f;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1f;
}

