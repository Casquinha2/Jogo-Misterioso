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

    [Header("Teleport local (opcional)")]
    [SerializeField] Vector2 teleportPosition;

    [Header("Bounds a ativar/desativar")]
    [SerializeField] GameObject inactivate;
    [SerializeField] GameObject activate;

    [Header("Cena destino")]
    [SerializeField] string sceneToLoad;

    [Header("UI de carregamento (opcional)")]
    [SerializeField] GameObject panel;  // se ficar vazio, tudo continua OK
    [SerializeField] float seconds = 0.5f;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player") || IsTransitioning) return;

        confiner.gameObject.SetActive(false);

        // 1) Bloqueia qualquer input
        IsTransitioning = true;

        // 2) Avise o usuário (se tiver painel)
        panel?.SetActive(true);

        // 3) Liga/desliga bounds locais
        if (activate != null)
        {
            activate.SetActive(true);
        }
        

        // 4) Opcional: warp local no mapa atual
        var playerT = collision.transform;
        var oldPos = playerT.position;

        playerT.position = new Vector3(teleportPosition.x, teleportPosition.y, oldPos.z);
        virtualCamera.OnTargetObjectWarped(playerT, playerT.position - oldPos);

        confiner.gameObject.SetActive(true);

        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        // 5) Inicia rotina de load
        StartCoroutine(DoLoadScene());
    }

    private IEnumerator DoLoadScene()
    {
        // A) Aguarda um pouco (seleyendo o painel)
        if (panel != null)
            yield return new WaitForSecondsRealtime(seconds);

        // B) Carrega cena de forma assíncrona
        var loadOp = SceneManager.LoadSceneAsync(sceneToLoad);
        loadOp.allowSceneActivation = true;
        yield return loadOp;

        // C) Um frame de buffer para tudo montar
        yield return null;

        // D) Fecha painel (se existir) e desativa o antigo bound
        panel?.SetActive(false);
        if (inactivate != null)
            inactivate?.SetActive(false);

        // E) Libera input
        IsTransitioning = false;
    }
}