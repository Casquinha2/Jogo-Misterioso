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


    void Update()
    {
        Camera cam = GetComponent<Camera>();
        float windowAspect = (float)Screen.width / Screen.height;

        float tolerance = 0.01f; // margem de erro pequena
        if (Mathf.Abs(windowAspect - targetAspect) < tolerance)
        {
            // Aspecto já é igual ao desejado — usar tela cheia (sem barras pretas)
            cam.rect = new Rect(0, 0, 1, 1);
        }
        else
        {
            float scaleHeight = windowAspect / targetAspect;

            if (scaleHeight < 1f)
            {
                // letterbox: barras pretas acima/baixo
                Rect r = new Rect();
                r.width = 1f;
                r.height = scaleHeight;
                r.x = 0;
                r.y = (1f - scaleHeight) / 2f;
                cam.rect = r;
            }
            else
            {
                // pillarbox: barras pretas laterais
                float scaleWidth = 1f / scaleHeight;
                Rect r = new Rect();
                r.width = scaleWidth;
                r.height = 1f;
                r.x = (1f - scaleWidth) / 2f;
                r.y = 0;
                cam.rect = r;
            }
        }
    }  
}
