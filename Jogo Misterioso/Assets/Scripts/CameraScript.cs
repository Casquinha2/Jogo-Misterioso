using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScript : MonoBehaviour
{
    public GameObject cmCam;
    public GameObject lightCamera;
    public GameObject mapBounderFolder;
    [SerializeField] float targetAspect = 16f / 9f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        DontDestroyOnLoad(cmCam);
        DontDestroyOnLoad(lightCamera);
        DontDestroyOnLoad(mapBounderFolder);
    }


    void Start()
    {
        Camera cam = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / Screen.height;
        float scaleHeight = windowAspect / targetAspect;

        if (scaleHeight < 1f)
        {
            // letterbox: barras pretas acima/baixo
            Rect r = cam.rect;
            r.width  = 1f;
            r.height = scaleHeight;
            r.x      = 0;
            r.y      = (1f - scaleHeight) / 2f;
            cam.rect = r;
        }
        else
        {
            // pillarbox: barras pretas laterais
            float scaleWidth = 1f / scaleHeight;
            Rect r = cam.rect;
            r.width  = scaleWidth;
            r.height = 1f;
            r.x      = (1f - scaleWidth) / 2f;
            r.y      = 0;
            cam.rect = r;
        }
    }
    

    
}
