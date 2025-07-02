using UnityEngine;

public class VerificarApanhado : MonoBehaviour
{
    [SerializeField] private Perseguicao perseguicao;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player")) return;

        StartCoroutine(perseguicao.Apanhado());
    }
}
