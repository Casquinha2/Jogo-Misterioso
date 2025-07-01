using UnityEngine;
using Unity.Cinemachine;
using System.Collections;
using Unity.VisualScripting;


public class MapTransition : MonoBehaviour
{
    public static bool IsTransitioning { get; private set; }

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

    [Header("UI de Carregamento")]
    [SerializeField] GameObject panel;
    [SerializeField] float seconds = 0.5f;

    private Transform playerT;
    private Vector3 oldPos, newPos;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        confiner.gameObject.SetActive(false);

        IsTransitioning = true;

        if (panel != null)
            panel.SetActive(true);

        // 1) Guarda a posição antiga
        Transform playerT = collision.transform;
        Vector3 oldPos = playerT.position;



        // 3) Calcula a nova posição e teleporta o jogador
        Vector3 newPos = oldPos;
        switch (direction)
        {
            case Direction.Up: newPos.y += additivePos; break;
            case Direction.Down: newPos.y -= additivePos; break;
            case Direction.Left: newPos.x -= additivePos; break;
            case Direction.Right: newPos.x += additivePos; break;
        }
        playerT.position = newPos;

        // 4) Informa a Cinemachine do warp, para ela ajustar imediatamente a câmera

        StartCoroutine(ConfinerCamera());


        if (panel != null)
            StartCoroutine(CloseLoading());
        else
            IsTransitioning = false;
    }

    private IEnumerator ConfinerCamera()
    {
        Vector3 delta = newPos - oldPos;
        virtualCamera.OnTargetObjectWarped(playerT, delta);

        yield return null;

        confiner.gameObject.SetActive(true);

        yield return null;

        // 2) Atualiza o confiner e força recálculo
        confiner.BoundingShape2D = mapBoundary;
        confiner.InvalidateBoundingShapeCache();
    }

    private IEnumerator CloseLoading()
    {

        yield return new WaitForSeconds(seconds);
        panel.SetActive(false);
        IsTransitioning = false;

    }
}