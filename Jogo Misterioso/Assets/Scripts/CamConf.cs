using UnityEngine;
using Unity.Cinemachine;

[RequireComponent(typeof(Collider2D))]
public class CamConf : MonoBehaviour
{
    public enum ZoomDirection { In, Out }
    public enum Opcao         { Sim, Nao }

    [Header("Virtual Camera")]
    [SerializeField] private CinemachineCamera      vCam;
    [SerializeField] private float                  zoomAmount     = 2f;
    [SerializeField] private ZoomDirection          zoomDirection  = ZoomDirection.In;
    [SerializeField] private float                  fastDamping    = 0.1f;
    [SerializeField] private float                  normalDamping  = 1f;
    [SerializeField] private Opcao                  opcao          = Opcao.Sim;

    [Header("Zoom Out On Exit")]
    [Tooltip("Se marcado, ao sair do trigger faz zoom-out restaurando estado original")]
    [SerializeField] private bool                   zoomOutOnExit  = false;

    private float                       _originalSize;
    private CinemachineConfiner2D      _confiner;
    private CinemachinePositionComposer _composer;

    private void Start()
    {
        if (vCam == null)
            vCam = FindFirstObjectByType<CinemachineCamera>();

        if (vCam == null)
        {
            Debug.LogError("[CamConf] Nenhuma CinemachineCamera encontrada.");
            enabled = false;
            return;
        }

        // Guarda tamanho original apenas na primeira cena
        if (ZoomState.OriginalSize < 0f)                
            ZoomState.OriginalSize = vCam.Lens.OrthographicSize;
        _originalSize = ZoomState.OriginalSize;

        _confiner = vCam.GetComponent<CinemachineConfiner2D>();
        _composer = vCam
            .GetCinemachineComponent(CinemachineCore.Stage.Body)
            as CinemachinePositionComposer;
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (!col.CompareTag("Player")) return;

        var lens = vCam.Lens;

        if (opcao == Opcao.Sim)
        {
            if (!ZoomState.IsZoomed)
            {
                lens.OrthographicSize = zoomDirection == ZoomDirection.In
                    ? Mathf.Max(0.1f, _originalSize - zoomAmount)
                    : _originalSize + zoomAmount;
                ZoomState.IsZoomed = true;
            }
            else
            {
                lens.OrthographicSize = _originalSize;
                ZoomState.IsZoomed = false;
            }
            vCam.Lens = lens;
            AjustaDamping(ZoomState.IsZoomed);
        }
        else // Opcao.Nao
        {
            lens.OrthographicSize = zoomDirection == ZoomDirection.In
                ? Mathf.Max(0.1f, _originalSize - zoomAmount)
                : _originalSize + zoomAmount;
            vCam.Lens = lens;
            AjustaDamping(true);
        }

        _confiner?.InvalidateBoundingShapeCache();
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        // Se não for Player ou zoomOutOnExit desligado, não faz nada
        if (!zoomOutOnExit || !col.CompareTag("Player")) 
            return;

        // Restaura zoom e damping
        var lens = vCam.Lens;
        lens.OrthographicSize = _originalSize;
        vCam.Lens = lens;

        ZoomState.IsZoomed = false;
        AjustaDamping(false);

        _confiner?.InvalidateBoundingShapeCache();
    }

    private void AjustaDamping(bool zoomed)
    {
        if (_composer == null) return;
        float d = zoomed ? fastDamping : normalDamping;
        _composer.Damping = new Vector3(d, d, _composer.Damping.z);
    }
}
