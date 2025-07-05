using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using System.Linq;

public class Porta62Codes : MonoBehaviour
{
    [Header("Identificador único deste puzzle/porta")]
    [Tooltip("Será usado em SessionState para bloquear a interação")]
    [SerializeField] private string puzzleID;

    [Header("UI do puzzle")]
    [Tooltip("Arraste o GameObject que contém o Canvas deste puzzle")]
    [SerializeField] private GameObject panel;
    [Tooltip("Arraste o TextMeshProUGUI do dígito 1")]
    [SerializeField] private TextMeshProUGUI num1;
    [Tooltip("Arraste o TextMeshProUGUI do dígito 2")]
    [SerializeField] private TextMeshProUGUI num2;
    [Tooltip("Arraste o TextMeshProUGUI do dígito 3")]
    [SerializeField] private TextMeshProUGUI num3;

    [Header("Checkpoint & Diálogo pós-teleporte")]
    [SerializeField] private string checkpointID;
    [SerializeField] private ObjDialogue objDialogue;

    [Header("Referência ao script que controla a UI")]
    [SerializeField] private OpenCustomUI openUI;

    private CinemachineCamera virtualCamera;
    private int totalClicks1, totalClicks2, totalClicks3;

    void Awake()
    {
        // 1) Captura a câmera virtual (tag CmCamera em um VirtualCamera do Cinemachine)
        var camGO = GameObject.FindGameObjectWithTag("CmCamera");
        if (camGO != null)
            virtualCamera = camGO.GetComponent<CinemachineCamera>();
        else
            Debug.LogWarning("[Porta62Codes] Não encontrou GameObject com tag 'CmCamera'.");

        // 2) Se já concluímos este puzzle, desativa este script
        if (!string.IsNullOrEmpty(puzzleID) &&
            SessionState.solvedPuzzles.Contains(puzzleID))
        {
            DisableInteraction();
        }
    }

    void Start()
    {
        // 1) Valida referências obrigatórias
        if (panel == null)
        {
            Debug.LogError("[Porta62Codes] 'panel' não foi atribuído no Inspector!");
            enabled = false;
            return;
        }

        if (num1 == null || num2 == null || num3 == null)
        {
            Debug.LogError("[Porta62Codes] Um ou mais TextMeshProUGUI não foram atribuídos no Inspector!");
            enabled = false;
            return;
        }

        // 2) Esconde o painel e inicializa valores
        panel.SetActive(false);
        ResetDigits();
    }

    void OnEnable()
    {
        // Garante que, sempre que o script for habilitado, os dígitos voltem a zero
        ResetDigits();
    }

    // Chamado pelos botões de cada dígito
    public void AddClicks1()
    {
        totalClicks1 = (totalClicks1 + 1) % 10;
        num1.text = totalClicks1.ToString();
    }

    public void AddClicks2()
    {
        totalClicks2 = (totalClicks2 + 1) % 10;
        num2.text = totalClicks2.ToString();
    }

    public void AddClicks3()
    {
        totalClicks3 = (totalClicks3 + 1) % 10;
        num3.text = totalClicks3.ToString();
    }

    // Verifica se o código está correto
    public void CodeVerification()
    {
        if (num1.text == "4" && num2.text == "8" && num3.text == "7")
        {
            panel.SetActive(false);

            if (!string.IsNullOrEmpty(puzzleID))
            {
                SessionState.solvedPuzzles.Add(puzzleID);
                openUI?.MarkAsSolved();
                DisableInteraction();
            }

            TeleportPlayer();
            TriggerPostTeleportDialogue();
        }
    }

    private void TeleportPlayer()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[Porta62Codes] Player não encontrado.");
            return;
        }

        // Encontra waypoint em MapBounds/Piso 1/62/Corredor62_Waypoint
        var mbRoot = GameObject.Find("MapBounds")?.transform;
        var piso1  = mbRoot?.Find("Piso 1");
        var obj62  = piso1?.Find("62");
        var wp     = obj62?
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == "Corredor62_Waypoint" && t != obj62);

        if (wp == null)
        {
            Debug.LogError("[Porta62Codes] Waypoint 'Corredor62_Waypoint' não encontrado.");
            return;
        }

        wp.gameObject.SetActive(true);

        // Atualiza confiner
        var confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        var poly     = piso1?.Find("Corredor62")?.GetComponent<PolygonCollider2D>();
        if (confiner != null && poly != null)
        {
            confiner.BoundingShape2D = poly;
            confiner.InvalidateBoundingShapeCache();
        }

        // Carrega checkpoint (opcional)
        if (!string.IsNullOrEmpty(checkpointID))
            CheckpointManager.I.LoadCheckpoint(checkpointID);

        // Move Player e notifica a câmera virtual
        var oldPos = player.transform.position;
        var newPos = new Vector3(-1.22f, 8f, 0f);
        player.transform.position = newPos;

        if (virtualCamera != null)
            virtualCamera.OnTargetObjectWarped(player.transform, newPos - oldPos);
    }

    private void TriggerPostTeleportDialogue()
    {
        objDialogue?.Interact();
    }

    private void DisableInteraction()
    {
        // desativa apenas este script, mantém collider ativo
        enabled = false;
    }

    private void ResetDigits()
    {
        totalClicks1 = totalClicks2 = totalClicks3 = 0;
        num1.text = num2.text = num3.text = "0";
    }
}
