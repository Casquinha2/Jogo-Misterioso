using UnityEngine;

public class Cameraload : MonoBehaviour
{
    public GameObject mainCam;
    public GameObject lightCamera;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(mainCam);
        DontDestroyOnLoad(lightCamera);
    }
}
