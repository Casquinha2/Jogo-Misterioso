using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine;
using System.Collections;
using System.ComponentModel.Design;

public class MapTransitionScenes : MonoBehaviour
{
    public static bool IsTransitioning { get; private set; }

    [Header("Bounds da cena atual")]
    [SerializeField] PolygonCollider2D mapBoundary;
    [SerializeField] CinemachineConfiner2D confiner;
    [SerializeField] CinemachineCamera virtualCamera;

    [Header("Teleport local")]
    [SerializeField] Vector2 teleportPosition;

    [Header("Posição de setup da câmera (opcional)")]
    [SerializeField] private Vector2 cameraSetupPosition;
    [SerializeField] private bool useCameraSetupPosition = false;


    [Header("Bounds a ativar/desativar")]
    [SerializeField] GameObject inactivate;
    [SerializeField] GameObject activate;

    [Header("Cena destino")]
    [SerializeField] string sceneToLoad = "";

    [Header("UI de carregamento (opcional)")]
    [SerializeField] GameObject panel;  // se ficar vazio, tudo continua OK
    [SerializeField] float seconds = 0.5f;




    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || IsTransitioning) return;

        IsTransitioning = true;
        panel?.SetActive(true);

        if (activate != null)
            activate.SetActive(true);

        StartCoroutine(HandleCameraSafeTransition(collision.transform));
    }

    private IEnumerator HandleCameraSafeTransition(Transform playerT)
    {
        var oldPos = playerT.position;

        if (useCameraSetupPosition)
        {
            // 1) Teleporta para posição temporária
            var tempPos = new Vector3(cameraSetupPosition.x, cameraSetupPosition.y, oldPos.z);
            playerT.position = tempPos;

            confiner.gameObject.SetActive(true);

            confiner.BoundingShape2D = mapBoundary;
            confiner.InvalidateBoundingShapeCache();
            virtualCamera.OnTargetObjectWarped(playerT, tempPos - oldPos);


            // 2) Espera um frame para a câmera se mover
            yield return null;
        }

        // 3) Teleporta para posição final
        var finalPos = new Vector3(teleportPosition.x, teleportPosition.y, oldPos.z);
        playerT.position = finalPos;

        virtualCamera.OnTargetObjectWarped(playerT, finalPos - oldPos);

        yield return null;

        // 4) Espera a câmera se ajustar antes de tirar o painel e seguir
        StartCoroutine(DoLoadScene());
    }

    private IEnumerator DoLoadScene()
    {
        if (panel != null)
            yield return new WaitForSecondsRealtime(seconds);

        if (!string.IsNullOrWhiteSpace(sceneToLoad))
        {
            var loadOp = SceneManager.LoadSceneAsync(sceneToLoad);
            loadOp.allowSceneActivation = true;
            yield return loadOp;
            yield return null;
        }

        panel?.SetActive(false);
        if (inactivate != null)
            inactivate.SetActive(false);

        IsTransitioning = false;
    }
}