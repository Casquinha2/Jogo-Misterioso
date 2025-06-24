using UnityEngine;
using Unity.Cinemachine;
using System.ComponentModel.Design;


public class MapTransitionScenes : MonoBehaviour
{
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



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        activate.SetActive(true);

        inactivate.SetActive(false);

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

    }
}