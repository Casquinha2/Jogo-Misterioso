using UnityEngine;
using System.Diagnostics;
using Debug = UnityEngine.Debug;   

public class PositionWatcher : MonoBehaviour
{
    [Header("Player")]
    public Transform playerTransform;        // Deixe vazio para auto-find
    private Vector3 lastPlayerPos;

    [Header("Camera")]
    public Transform cameraTransform;        // Deixe vazio para usar Camera.main
    private Vector3 lastCameraPos;

    void Start()
    {
        // resolve Player
        if (playerTransform == null)
        {
            var go = GameObject.FindGameObjectWithTag("Player");
            if (go != null) playerTransform = go.transform;
            else Debug.LogWarning("[Watcher] não achei tag 'Player'");
        }
        if (playerTransform != null)
            lastPlayerPos = playerTransform.position;

        // resolve Camera
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
        if (cameraTransform != null)
            lastCameraPos = cameraTransform.position;
        else
            Debug.LogWarning("[Watcher] não achei Camera.main; arraste no Inspector");
    }

    void Update()
    {
        // PLAYER
        if (playerTransform != null)
        {
            var currP = playerTransform.position;
            if (currP != lastPlayerPos)
            {
                Debug.Log($"[Watcher] Player: {lastPlayerPos} → {currP}");
                LogStackTrace();
                lastPlayerPos = currP;
            }
        }

        // CAMERA
        if (cameraTransform != null)
        {
            var currC = cameraTransform.position;
            if (currC != lastCameraPos)
            {
                Debug.Log($"[Watcher] Câmera: {lastCameraPos} → {currC}");
                LogStackTrace();
                lastCameraPos = currC;
            }
        }
    }

    void LogStackTrace()
    {
        var st = new StackTrace(2, true);  
        Debug.Log(st.ToString());
    }
}
