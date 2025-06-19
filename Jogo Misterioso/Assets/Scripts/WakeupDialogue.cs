using NUnit.Framework.Constraints;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WakeupDialogue : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Scene currentScene = SceneManager.GetActiveScene ();
        string sceneName = currentScene.name;

        if (sceneName == "Piso1Scene")
        {
            
        }
    }
}
