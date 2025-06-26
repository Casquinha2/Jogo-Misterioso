using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasScaler))]
public class UIResize : MonoBehaviour
{
    public enum Mode
    {
        ScreenSpaceOverlay,
        ScreenSpaceCamera
    }

    [Header("Modo de Renderização")]
    public Mode mode = Mode.ScreenSpaceOverlay;

    [Header("Configurações para ScreenSpaceCamera")]
    [Tooltip("Se vazio, usa Camera.main")]
    public Camera uiCamera;
    public float planeDistance = 100f;
    public string sortingLayer = "UI";

    private Canvas        _canvas;
    private RectTransform _rt;
    private CanvasScaler  _scaler;

    private void Awake()
    {
        _canvas = GetComponent<Canvas>();
        _rt     = _canvas.GetComponent<RectTransform>();
        _scaler = GetComponent<CanvasScaler>();

        if (uiCamera == null)
            uiCamera = Camera.main;
    }

    private void Start()
    {
        if (uiCamera == null)
        {
            Debug.LogError("[UIResize] não encontrou Camera.main!");
            enabled = false;
            return;
        }

        if (mode == Mode.ScreenSpaceCamera)
            SetupCameraSpace();
        else
            StartCoroutine(SetupOverlayEndOfFrame());
    }

    private void SetupCameraSpace()
    {
        _canvas.renderMode       = RenderMode.ScreenSpaceCamera;
        _canvas.worldCamera      = uiCamera;
        _canvas.planeDistance    = planeDistance;
        _canvas.sortingLayerName = sortingLayer;

        // Mantém ScaleWithScreenSize para ajustar UI por resolução
        _scaler.uiScaleMode        = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _scaler.referenceResolution = new Vector2(1920, 1080);
    }

    private IEnumerator SetupOverlayEndOfFrame()
    {
        // espera o fim do frame para pixelRect estar correto
        yield return new WaitForEndOfFrame();

        // força redraw e passa a pixel-perfect
        Canvas.ForceUpdateCanvases();
        _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        _scaler.scaleFactor = 1f;

        // switch para Overlay
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // recupera o rect letterboxed da câmera
        Rect vp = uiCamera.pixelRect;

        // ajusta anchors do Canvas root para essa área
        //_rt.anchorMin = new Vector2(vp.xMin / Screen.width, vp.yMin / Screen.height);
        //_rt.anchorMax = new Vector2(vp.xMax / Screen.width, vp.yMax / Screen.height);
        _rt.offsetMin = _rt.offsetMax = Vector2.zero;

        Debug.Log($"[UIResize] vp: {vp}, anchors: {_rt.anchorMin} - {_rt.anchorMax}");
    }
}
