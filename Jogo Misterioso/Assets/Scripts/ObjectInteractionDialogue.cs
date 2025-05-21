using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectInteraction", menuName = "Object Interaction")]
public class ObjectInteractionDialogue : ScriptableObject
{
    public string objectName;
    public string[] dialogueLines;
    public float typingSpeed = 0.05f;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 1f;
}
