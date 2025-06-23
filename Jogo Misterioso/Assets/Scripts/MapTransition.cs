using UnityEngine;
using Unity.Cinemachine;


public class MapTransition : MonoBehaviour
{
    [Header("Zona de confinamento (não trigger)")]
    [SerializeField] PolygonCollider2D mapBoundary;

    [Header("Confiner da Virtual Camera")]
    [SerializeField] CinemachineConfiner2D confiner;

    [Header("Virtual Camera que segue o Player")]
    [SerializeField] CinemachineCamera virtualCamera;

    public enum Direction { Up, Down, Left, Right }
    [SerializeField] Direction direction;

    [Header("Distância de teleporte")]
    [SerializeField] float additivePos = 2f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        // 1) Guarda a posição antiga
        Transform playerT = collision.transform;
        Vector3 oldPos = playerT.position;

        // 2) Atualiza o confiner e força recálculo
        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();

        // 3) Calcula a nova posição e teleporta o jogador
        Vector3 newPos = oldPos;
        switch (direction)
        {
            case Direction.Up:    newPos.y += additivePos; break;
            case Direction.Down:  newPos.y -= additivePos; break;
            case Direction.Left:  newPos.x -= additivePos; break;
            case Direction.Right: newPos.x += additivePos; break;
        }
        playerT.position = newPos;

        // 4) Informa a Cinemachine do warp, para ela ajustar imediatamente a câmera
        Vector3 delta = newPos - oldPos;
        virtualCamera.OnTargetObjectWarped(playerT, delta);
    }
}