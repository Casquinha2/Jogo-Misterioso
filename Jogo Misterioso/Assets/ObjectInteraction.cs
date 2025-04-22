using UnityEngine;

[CreateAssetMenu(fileName = "NewObjectInteraction", menuName = "Object Interaction")]
public class ObjectInteraction : ScriptableObject
{
    public string[] dialogueLines;
    public float typingSpeed = 0.05f;
    public bool[] autoProgressLines;
    public float autoProgressDelay = 2.5f;
}
