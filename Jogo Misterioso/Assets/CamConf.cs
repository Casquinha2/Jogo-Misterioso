using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class CamConf : MonoBehaviour
{
    public enum ZoomDirection { In, Out }

    [Header("Virtual Camera (se houver mais que uma, atribua manualmente)")]
    [SerializeField] private CinemachineCamera vCam;

    [Header("Quanto mudar o OrthographicSize")]
    [SerializeField] private float zoomAmount = 2f;

    [Header("Direção do Zoom ao entrar: In = reduz, Out = aumenta")]
    [SerializeField] private ZoomDirection zoomDirection = ZoomDirection.In;

    [Header("Damping quando em zoom")]
    [Tooltip("Quão rápido a câmara “trava” no jogador")]
    [SerializeField] private float fastDamping = 0.1f;

    [Header("Damping normal da câmara")]
    [SerializeField] private float normalDamping = 1f;

    private float _originalSize;
    private bool  _isZoomed = false;
    private CinemachineConfiner2D _confiner;
    private CinemachinePositionComposer _composer;

    private void Start()
    {
        // tenta encontrar a vCam se não atribuída
        if (vCam == null)
            vCam = FindFirstObjectByType<CinemachineCamera>();

        if (vCam == null)
        {
            Debug.LogError("[CamConf] não encontrou CinemachineCamera na cena!");
            enabled = false;
            return;
        }

        // guarda o tamanho inicial
        _originalSize = vCam.Lens.OrthographicSize;
        _confiner = vCam.GetComponent<CinemachineConfiner2D>();

        _composer = vCam
            .GetCinemachineComponent(CinemachineCore.Stage.Body)
            as CinemachinePositionComposer;
        if (_composer == null)
            Debug.LogWarning("[CamConf] Não encontrou CinemachinePositionComposer no Body.");
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        // obtém struct Lens, altera e reassocia
        var lens = vCam.Lens;

        if (!_isZoomed)
        {
            // faz zoom in ou out
            if (zoomDirection == ZoomDirection.In)
                lens.OrthographicSize = Mathf.Max(0.1f, _originalSize - zoomAmount);
            else
                lens.OrthographicSize = _originalSize + zoomAmount;

            _isZoomed = true;
        }
        else
        {
            // repõe tamanho original
            lens.OrthographicSize = _originalSize;
            _isZoomed = false;
        }

        vCam.Lens = lens;

        if (_composer != null)
        {
            float d = _isZoomed ? fastDamping : normalDamping;
            // PositionComposer usa Vector3: X=eixo horizontal, Y=vertical, Z=profundidade
            _composer.Damping = new Vector3(d, d, _composer.Damping.z);
        }

        if (_confiner != null)
            _confiner.InvalidateBoundingShapeCache();
    }
}
