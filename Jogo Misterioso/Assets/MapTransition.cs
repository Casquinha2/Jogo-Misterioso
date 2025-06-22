using UnityEngine;
using Unity.Cinemachine;
using UnityEditor.Experimental.GraphView;

public class MapTransition : MonoBehaviour
{
    [SerializeField] PolygonCollider2D mapBoundry;
    CinemachineConfiner2D confiner;
    [SerializeField] Direction direction;
    [SerializeField] float additivepos = 2f;
    enum Direction { Up, Down, Left, Right }

    private void Awake()
    {
        confiner = FindFirstObjectByType<CinemachineConfiner2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            confiner.BoundingShape2D = mapBoundry;
            UpdatePlayerPosition(collision.gameObject);
        }
    }

    private void UpdatePlayerPosition(GameObject player)
    {
        Debug.Log("A tentar tp");
        Vector3 newPos = player.transform.position;

        switch (direction)
        {
            case Direction.Up:
                newPos.y += additivepos;
                break;

            case Direction.Down:
                newPos.y -= additivepos;
                break;

            case Direction.Left:
                newPos.x += additivepos;
                break;

            case Direction.Right:
                newPos.x -= additivepos;
                break;
        }

        player.transform.position = newPos;
    }
}
