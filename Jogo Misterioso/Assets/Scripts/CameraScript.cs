using UnityEngine;

public class CameraScript : MonoBehaviour
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
