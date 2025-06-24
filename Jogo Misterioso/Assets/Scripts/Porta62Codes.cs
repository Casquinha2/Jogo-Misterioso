using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using System.Linq;
using System.Collections;


public class Porta62Codes : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI num1;
    [SerializeField] private TextMeshProUGUI num2;
    [SerializeField] private TextMeshProUGUI num3;
    [SerializeField] private GameObject panel;
    [SerializeField] private GameObject blackPanel;
    [SerializeField] private string checkpointID;


    private int totalClicks1, totalClicks2, totalClicks3;

    public void AddClicks1() { totalClicks1 = (totalClicks1 + 1) % 10; num1.text = totalClicks1.ToString(); }
    public void AddClicks2() { totalClicks2 = (totalClicks2 + 1) % 10; num2.text = totalClicks2.ToString(); }
    public void AddClicks3() { totalClicks3 = (totalClicks3 + 1) % 10; num3.text = totalClicks3.ToString(); }


    public void CodeVerification()
    {
        if (num1.text == "8" && num2.text == "4" && num3.text == "7")
        {
            panel.SetActive(false);
            blackPanel.SetActive(true);

            

            blackPanel.SetActive(false);

            // 1) Ache o Player
            var player = GameObject.FindWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("[Porta62Codes] Player não encontrado.");
                return;
            }

            // 2) Ache o waypoint pela hierarquia “MapBounds/Corredor62/Corredor62_Waypoint”
            var mbRoot = GameObject.Find("MapBounds")?.transform;
            if (mbRoot == null)
            {
                Debug.LogError("[Porta62Codes] Não encontrei o root 'MapBounds'.");
                return;
            }

            var bound62 = mbRoot.Find("62");
            if (bound62 == null)
            {
                Debug.LogError("[Porta62Codes] Não encontrei 'Corredor62' em MapBounds.");
                return;
            }

            // Busca recursiva para garantir que encontra mesmo inativo
            var wpTransform = bound62.GetComponentsInChildren<Transform>(true)
                                     .FirstOrDefault(t => t.name == "Corredor62_Waypoint");
            if (wpTransform == null)
            {
                Debug.LogError("[Porta62Codes] Não encontrei o Transform do waypoint!");
                return;
            }

            var waypoint = wpTransform.gameObject;
            waypoint.SetActive(true);


            // 4) Ajusta o confiner dinamicamente
            var confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            var corredor62Bound = mbRoot.Find("Corredor62");
            var poly = corredor62Bound.GetComponent<PolygonCollider2D>();
            if (confiner != null && poly != null)
            {
                confiner.BoundingShape2D = poly;
                Debug.Log("[Porta62Codes] Confiner atualizado para Corredor62.");
            }

            if (!string.IsNullOrEmpty(checkpointID))
            {
                CheckpointManager.I.SaveCheckpoint(checkpointID);
                CheckpointManager.I.LoadCheckpoint(checkpointID);
            }

            player.transform.position = new Vector3(-1.22f, 8f, 0);
        }
    }
}
