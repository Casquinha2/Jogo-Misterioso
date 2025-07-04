using UnityEngine;

public class OpenCustomUI : MonoBehaviour, IInteractable
{
    [Header("Persistência (0 = sem persistência)")]
    [SerializeField] private int idInteractable = 0;

    [Header("Progress-based disabling/enabling (-1 = ignorar)")]
    [SerializeField] private int disableAfterProgress = -1;
    [SerializeField] private int enableAfterProgress  = -1;

    [Header("Referências de UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject qr;
    [SerializeField] private QrReveal qrRevealScript;

    // Só “true” depois que chamar MarkAsSolved()
    private bool jaResolvido = false;

    void Start()
    {
        panel.SetActive(false);

        // Persistência via SessionState
        if (idInteractable > 0 && SessionState.solvedInteractables.Contains(idInteractable))
            jaResolvido = true;

        // Thresholds de progresso também bloqueiam antes de resolver
        var prog = FindFirstObjectByType<Progress>()?.GetProgress() ?? 0;
        if (disableAfterProgress >= 0 && prog >= disableAfterProgress)
            jaResolvido = true;
        if (enableAfterProgress  >= 0 && prog < enableAfterProgress)
            jaResolvido = true;
    }

    public bool CanInteract()
    {
        // só interage enquanto não estiver “resolvido” e sem UI aberta
        return !jaResolvido && !panel.activeSelf;
    }

    public void Interact()
    {
        // não faz nada se já resolvido ou se o painel já estiver aberto
        if (jaResolvido || panel.activeSelf)
            return;

        // abre a UI — mas NÃO marca resolvido aqui!
        panel.SetActive(true);
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // fecha sempre ao sair da área
        panel.SetActive(false);

        // reseta o QR, se houver
        if (qr != null)
        {
            qrRevealScript.totalClicks = 0;
            qr.SetActive(true);
        }
    }

    /// <summary>
    /// Chame este método **somente** quando o puzzle for realmente resolvido!
    /// </summary>
    public void MarkAsSolved()
    {
        if (jaResolvido) 
            return;

        jaResolvido = true;
        panel.SetActive(false);

        // Se quisermos persistência, armazenamos o ID
        if (idInteractable > 0)
            SessionState.solvedInteractables.Add(idInteractable);
    }
}
