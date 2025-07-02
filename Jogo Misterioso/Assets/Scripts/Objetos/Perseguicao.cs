using UnityEngine;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine.SceneManagement;

public class Perseguicao : MonoBehaviour
{
    [SerializeField] private PolygonCollider2D   mapBoundary;
    [SerializeField] private CinemachineConfiner2D confiner;
    [SerializeField] private CinemachineCamera     virtualCamera; // CemichineCamera é o correto
    [SerializeField] private GameObject            panel;
    [SerializeField] private string                sceneToLoad;
    [SerializeField] Transform mapBoundRoot;

    private Vector3 inicialPlayerCoords;
    private GameObject player;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
        if (player == null)
            Debug.LogError("[Perseguicao] Player não encontrado!", this);
    }

    void OnEnable()
    {
        // Guarda a posição do player assim que ativas este objecto (quando apanha a capa)
        inicialPlayerCoords = player.transform.position;
        Debug.Log($"[Perseguicao] posição inicial guardada = {inicialPlayerCoords}");
    }

    public IEnumerator Apanhado()
    {
        // 1) Pausar e mostrar UI
        PauseController.SetPause(true);
        panel.SetActive(true);

        Transform pisoNegativo1 = mapBoundRoot.Find("Piso -1");
        pisoNegativo1.gameObject.SetActive(true);

        // 2) Espera breve em tempo real
        yield return new WaitForSecondsRealtime(1f);

        // 3) Carregar nova cena
        var loadOp = SceneManager.LoadSceneAsync(sceneToLoad);
        loadOp.allowSceneActivation = true;
        yield return loadOp;  // aguarda até a cena estar ativa

        // antes de mover o player
        Vector3 oldPos = player.transform.position;
        Vector3 newPos = inicialPlayerCoords;
        Vector3 delta  = newPos - oldPos;

        // teleporta
        player.transform.position = newPos;

        // agora faz o warp
        virtualCamera.OnTargetObjectWarped(player.transform, delta);
        Debug.Log($"[Perseguicao] Warping camera por delta = {delta}");

        // 6) Reconfigurar o confiner em alguns frames
        yield return null;
        confiner.gameObject.SetActive(true);
        yield return null;
        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        foreach (Transform child in mapBoundRoot)
        {
            if (child.gameObject.name != "Piso -1")
            {
                child.gameObject.SetActive(false);
            }
        }

        // 7) Fecha UI e despausa após um tempinho
        yield return new WaitForSecondsRealtime(1f);
        panel.SetActive(false);
        PauseController.SetPause(false);
    }
}
