using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public GameObject mainCam;
    public GameObject lightCamera;
    public GameObject mapBounderFolder;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(mainCam);
        DontDestroyOnLoad(lightCamera);
        DontDestroyOnLoad(mapBounderFolder);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
