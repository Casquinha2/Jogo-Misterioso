using UnityEngine;

public class OpenCustomUI : MonoBehaviour, IInteractable
{
    public GameObject panel;

    void Start()
    {
        panel.SetActive(false);
    }

    public bool CanInteract()
    {
        return !panel.activeSelf;
    }

    public void Interact()
    {
        if (!panel.activeSelf)
            panel.SetActive(true);
    }

    // Fechar o painel quando o player sair do trigger
    // usando UnityEngine;
    // substitui OnTriggerExit por:
    void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Saio do range");
        panel.SetActive(false);
    }

}
