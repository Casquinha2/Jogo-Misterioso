using UnityEngine;

public class OpenCustomUI : MonoBehaviour, IInteractable
{
    [Header("Persistência (0 = sem persistência)")]
    [SerializeField] private int idInteractable = 0;

    [Header("Progress-based disabling/enabling (-1 = ignorar)")]
    [Tooltip("Se ≥0 e progress ≥ este valor, o objeto é desativado")]
    [SerializeField] private int disableAfterProgress = -1;
    [Tooltip("Se ≥0 e progress < este valor, o objeto é desativado")]
    [SerializeField] private int enableAfterProgress = -1;

    [Header("Referências de UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject qr;
    [SerializeField] private QrReveal qrRevealScript;

    private bool jaResolvido;

    void Start()
    {
        panel.SetActive(false);

        // 1) Primeiro, checa persistência própria
        if (idInteractable > 0 && PlayerPrefs.GetInt($"Interactable_{idInteractable}", 0) == 1)
        {
            ForceDisableInteraction();
            return;
        }

        // 2) Agora checa progress para habilitar/desabilitar por threshold
        var prog = FindFirstObjectByType<Progress>()?.GetProgress() ?? 0;

        // Se atingiu ou passou do valor para desativar  
        if (disableAfterProgress >= 0 && prog >= disableAfterProgress)
        {
            ForceDisableInteraction();
            return;
        }

        // Se ainda não atingiu o valor para ativar  
        if (enableAfterProgress >= 0 && prog < enableAfterProgress)
        {
            ForceDisableInteraction();
        }
    }

    public bool CanInteract()
    {
        return enabled && !panel.activeSelf;
    }

    public void Interact()
    {
        if (!panel.activeSelf)
        {
            if (idInteractable > 0 && !jaResolvido)
            {
                jaResolvido = true;
                PlayerPrefs.SetInt($"Interactable_{idInteractable}", 1);
                PlayerPrefs.Save();
                ForceDisableInteraction();
            }
            panel.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        // só procede se quem saiu do trigger for o objeto taggeado como "Player"
        if (!other.CompareTag("Player"))
            return;

        if (qr != null)
        {
            qrRevealScript.totalClicks = 0;
            qr.SetActive(true);
        }

        panel.SetActive(false);
    }


    public int GetID() => idInteractable;

    public void ForceDisableInteraction()
    {
        jaResolvido = true;
        panel.SetActive(false);
        enabled = false;
        if (TryGetComponent<Collider2D>(out var c)) c.enabled = false;
    }

    public void ForceEnableInteraction()
    {
        jaResolvido = false;
        panel.SetActive(false);
        enabled = true;
        if (TryGetComponent<Collider2D>(out var c)) c.enabled = true;
    }

    public void MarkAsSolved()
    {
        if (idInteractable > 0 && !jaResolvido)
        {
            jaResolvido = true;
            PlayerPrefs.SetInt($"Interactable_{idInteractable}", 1);
            PlayerPrefs.Save();
            ForceDisableInteraction();
        }
    }
}
