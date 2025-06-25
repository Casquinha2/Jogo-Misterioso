using UnityEngine;
using Unity.Cinemachine;
using System.Collections;


public class MapTransitionScenes : MonoBehaviour
{

    public static bool IsTransitioning { get; private set; }

    [Header("Zona de confinamento (não trigger)")]
    [SerializeField] PolygonCollider2D mapBoundary;

    [Header("Confiner da Virtual Camera")]
    [SerializeField] CinemachineConfiner2D confiner;

    [Header("Virtual Camera que segue o Player")]
    [SerializeField] CinemachineCamera virtualCamera;

    [Header("Coordenadas para o tp")]
    [SerializeField] Vector2 teleportPosition;


    [Header("MapBounds das cenas a ativar/Desativar")]
    [SerializeField] GameObject inactivate;

    [SerializeField] GameObject activate;

    [Header("Checkpoint ID")]
    [SerializeField] private string checkpointID;

    [Header("UI de Carregamento")]
    [SerializeField] GameObject panel;
    [SerializeField] float seconds = 1.0f;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;


        IsTransitioning = true;

        if (panel != null)
            panel.SetActive(true);

        activate.SetActive(true);


        // 1) Guarda a posição antiga
        Transform playerT = collision.transform;
        Vector3 oldPos = playerT.position;

        // 2) Atualiza o confiner e força recálculo
        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        // 3) Calcula a nova posição e teleporta o jogador
        Vector3 newPos = new Vector3(teleportPosition.x, teleportPosition.y, 0f);

        playerT.position = newPos;

        // 4) Informa a Cinemachine do warp, para ela ajustar imediatamente a câmera
        Vector3 delta = newPos - oldPos;
        virtualCamera.OnTargetObjectWarped(playerT, delta);

        if (!string.IsNullOrEmpty(checkpointID))
        {
            CheckpointManager.I.SaveCheckpoint(checkpointID);
            CheckpointManager.I.LoadCheckpoint(checkpointID);
        }

        if (panel != null)
        {
            Debug.Log($"▶️ Iniciando coroutine CloseLoading por {seconds}s");
            StartCoroutine(CloseLoading());
        }
        else
        {   
            Debug.Log("⚠️ panel == null, desativando inactivate imediatamente");
            inactivate.SetActive(false);
        }
        
    }

    private IEnumerator CloseLoading()
    {
        // 1) espera o tempo de loading
        yield return new WaitForSecondsRealtime(seconds);

        // 2) manda sumir o painel
        panel.SetActive(false);
        IsTransitioning = false;

        // 3) aqui sim, espera a Unity “processar” a desativação do painel
        //    usando yield, e não um loop bloqueante
        yield return null;  

        // 4) só depois disso desativa o inactivate
        inactivate.SetActive(false);
    }

}