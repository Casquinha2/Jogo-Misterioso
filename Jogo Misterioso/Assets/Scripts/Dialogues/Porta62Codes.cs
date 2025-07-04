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
    [SerializeField] private TextMeshProUGUI num1;
    [SerializeField] private TextMeshProUGUI num2;
    [SerializeField] private TextMeshProUGUI num3;
    [SerializeField] private GameObject panel;

    [Header("Checkpoint & Diálogo pós-teleporte")]
    [SerializeField] private string checkpointID;
    [SerializeField] private ObjDialogue objDialogue;

    [Header("Referência direta ao script que controla a UI")]
    [SerializeField] private OpenCustomUI openUI;


    private CinemachineCamera virtualCamera;
    private int totalClicks1, totalClicks2, totalClicks3;

    void Start()
    {
        // Captura a câmera virtual
        var cam = GameObject.FindGameObjectWithTag("CmCamera");
        if (cam != null)
            virtualCamera = cam.GetComponent<CinemachineCamera>();

        // Se já resolvemos este puzzle nesta sessão, bloqueia a interação
        if (!string.IsNullOrEmpty(puzzleID) &&
            SessionState.solvedPuzzles.Contains(puzzleID))
        {
            DisableInteraction();
        }
    }
    public void AddClicks1() { totalClicks1 = (totalClicks1 + 1) % 10; num1.text = totalClicks1.ToString(); }
    public void AddClicks2() { totalClicks2 = (totalClicks2 + 1) % 10; num2.text = totalClicks2.ToString(); }
    public void AddClicks3() { totalClicks3 = (totalClicks3 + 1) % 10; num3.text = totalClicks3.ToString(); }

    void OnEnable()
    {
        num1.text = "0";
        num2.text = "0";
        num3.text = "0";
    }


    public void CodeVerification()
    {
        // Quando o código estiver correto:
        if (num1.text == "4" && num2.text == "8" && num3.text == "7")
        {
            // 1) Fecha o painel
            panel.SetActive(false);

            // 2) Marca como resolvido nesta sessão
            if (!string.IsNullOrEmpty(puzzleID))
            {
                // 1) marca o puzzle como resolvido (string)
                SessionState.solvedPuzzles.Add(puzzleID);

                // 2) marca a UI da porta como resolvida
                if (openUI != null)
                    openUI.MarkAsSolved();


                // 3) opcional: só então você desabilita o próprio script Porta62Codes
                DisableInteraction();
            }




            // 3) Continua com seu fluxo de teleporte, confiner e diálogo:
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

            // 2) Achar o caminho correto até o waypoint
            var mbRoot = GameObject.Find("MapBounds")?.transform;
            if (mbRoot == null)
            {
                Debug.LogError("[Porta62Codes] Não encontrei o root 'MapBounds'.");
                return;
            }

            var piso1 = mbRoot.Find("Piso 1");
            if (piso1 == null)
            {
                Debug.LogError("[Porta62Codes] Não encontrei 'Piso 1'.");
                return;
            }

            var obj62 = piso1.Find("62");
            if (obj62 == null)
            {
                Debug.LogError("[Porta62Codes] Não encontrei '62' dentro de Piso 1.");
                return;
            }

            var wpTransform = obj62.GetComponentsInChildren<Transform>(true) // inclui inativos
                .FirstOrDefault(t => t.name == "Corredor62_Waypoint" && t != obj62);

            if (wpTransform == null)
            {
                Debug.LogError("[Porta62Codes] Não encontrei o Transform 'Corredor62_Waypoint'.");
                return;
            }

            // 3) Ativar o waypoint (se estiver inativo)
            wpTransform.gameObject.SetActive(true);

            // 4) Atualizar o Confiner dinamicamente
            var confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            var corredor62 = piso1.Find("Corredor62");
            var poly = corredor62.GetComponent<PolygonCollider2D>();
            if (confiner != null && poly != null)
            {
                confiner.BoundingShape2D = poly;
                Debug.Log("[Porta62Codes] Confiner atualizado para Corredor62_Waypoint.");
            }

            // 5) Salvar e carregar checkpoint
            if (!string.IsNullOrEmpty(checkpointID))
            {
                CheckpointManager.I.LoadCheckpoint(checkpointID);
            }

            // 6) Mover o Player
            player.transform.position = new Vector3(-1.22f, 8f, 0);

            if (confiner != null)
            {
                confiner.InvalidateBoundingShapeCache();
            }

            Vector3 oldPos = player.transform.position;
            Vector3 newPos = new Vector3(-1.22f, 8f, 0);
            player.transform.position = newPos;

            if (virtualCamera != null)
            {
                Vector3 offset = newPos - oldPos;
                virtualCamera.OnTargetObjectWarped(player.transform, offset);
            }

            TriggerPostTeleportDialogue();
    }
    private void TriggerPostTeleportDialogue()
    {
        if (objDialogue != null)
            objDialogue.Interact();
    }

    private void DisableInteraction()
    {
        // desliga só o Porta62Codes, não o collider
        enabled = false;
    }
}
