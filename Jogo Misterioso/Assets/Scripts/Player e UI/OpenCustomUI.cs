using UnityEngine;
using UnityEngine.UI;

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
        Debug.Log($"[OpenCustomUI{idInteractable}] Start called");
        panel.SetActive(false);

        // Log de valores iniciais
        var prog = FindFirstObjectByType<Progress>()?.GetProgress() ?? 0;
        Debug.Log($"[OpenCustomUI{idInteractable}] Valores iniciais: prog={prog}, disableAfterProgress={disableAfterProgress}, enableAfterProgress={enableAfterProgress}");
        Debug.Log($"[OpenCustomUI{idInteractable}] Solved IDs: {string.Join(",", SessionState.solvedInteractables)}");

        // Persistência
        if (idInteractable > 0 && SessionState.solvedInteractables.Contains(idInteractable))
        {
            jaResolvido = true;
            Debug.Log($"[OpenCustomUI{idInteractable}] Desativado por persistência (ID encontrado)");
        }

        // Desativação por progresso alcançado
        if (disableAfterProgress >= 0 && prog >= disableAfterProgress)
        {
            jaResolvido = true;
            Debug.Log($"[OpenCustomUI{idInteractable}] Desativado: prog {prog} >= disableAfterProgress {disableAfterProgress}");
        }

        // Ativação após progresso mínimo
        if (enableAfterProgress >= 0 && prog < enableAfterProgress)
        {
            jaResolvido = true;
            Debug.Log($"[OpenCustomUI{idInteractable}] Ainda não ativado: prog {prog} < enableAfterProgress {enableAfterProgress}");
        }

        Debug.Log($"[OpenCustomUI{idInteractable}] Estado inicial jaResolvido={jaResolvido}");
    }

    public bool CanInteract()
    {
        bool can = !jaResolvido && !panel.activeSelf;
        Debug.Log($"[OpenCustomUI{idInteractable}] CanInteract() => jaResolvido={jaResolvido}, panelActive={panel.activeSelf} => {can}");
        return can;
    }

    public void Interact()
    {
        Debug.Log($"[OpenCustomUI{idInteractable}] Interact() called");

        if (jaResolvido)
        {
            Debug.Log($"[OpenCustomUI{idInteractable}] Interação ignorada: já resolvido");
            return;
        }
        if (panel.activeSelf)
        {
            Debug.Log($"[OpenCustomUI{idInteractable}] Interação ignorada: painel já aberto");
            return;
        }

        panel.SetActive(true);
        Debug.Log($"[OpenCustomUI{idInteractable}] Painel aberto");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("PlayerInteraction")) return;

        Debug.Log($"[OpenCustomUI{idInteractable}] OnTriggerExit2D: fechando painel/resetando QR");
        panel.SetActive(false);

        if (qr != null)
        {
            qrRevealScript.totalClicks = 0;
            qr.SetActive(true);
            Debug.Log($"[OpenCustomUI{idInteractable}] QR reiniciado");
        }
    }

    /// <summary>
    /// Chame este método **somente** quando o puzzle for realmente resolvido!
    /// </summary>
    public void MarkAsSolved()
    {
        Debug.Log($"[OpenCustomUI{idInteractable}] MarkAsSolved() called");
        if (jaResolvido)
        {
            Debug.Log($"[OpenCustomUI{idInteractable}] MarkAsSolved ignorado: já marcado");
            return;
        }

        jaResolvido = true;
        panel.SetActive(false);
        Debug.Log($"[OpenCustomUI{idInteractable}] Puzzle marcado como resolvido e painel fechado");

        if (idInteractable > 0)
        {
            SessionState.solvedInteractables.Add(idInteractable);
            Debug.Log($"[OpenCustomUI{idInteractable}] Persistência: ID {idInteractable} adicionado");
        }
    }
}
