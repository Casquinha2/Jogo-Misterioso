using UnityEngine;
using System.Collections;

public class OpenCustomUI : MonoBehaviour, IInteractable
{
    public GameObject panel;

    public GameObject qr;

    public QrReveal qrRevealScript;


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
        if (qr != null)
        {
            qrRevealScript.totalClicks = 0;
            qr.SetActive(true);
        }

        panel.SetActive(false);
        
    }
}
