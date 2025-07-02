using UnityEngine;
using TMPro; 
using System.Collections.Generic; // para List<> 
using System.Linq;
using Unity.Cinemachine;

public class PsicologiaPuzzle : MonoBehaviour
{
    // lista pra guardar cada letra como GameObject 
    [SerializeField] private List<TMP_Text> letras;
    [SerializeField] private GameObject porta;
    [SerializeField] private GameObject panel;

    private CinemachineCamera virtualCamera;

    private Progress progress;

    void Awake()
    {
        progress = FindFirstObjectByType<Progress>();
        if (progress == null)
            Debug.LogError("[PsicologiaPuzzle] Não encontrei nenhum Progress na cena!");
    }


    private int progressao = 0;

    public void check(TMP_Text letra)
    {
        switch (progressao)
        {
            case 0:
                if (letra.text == "D")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 1:
                if (letra.text == "I")
                {
                    progressao++;
                    letra.color = new Color32(140, 255, 140, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 2:
                if (letra.text == "R")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 3:
                if (letra.text == "E")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 4:
                if (letra.text == "I")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 5:
                if (letra.text == "T")
                {
                    progressao++;
                    letra.color = new Color32(0, 255, 0, 255);
                }
                else
                {
                    ResetarTudo();
                }
                break;
            case 6:
                if (letra.text == "O")
                {
                    letra.color = new Color32(0, 255, 0, 255);
                    Correto();

                }
                else
                {
                    ResetarTudo();
                }
                break;
        }
    }

    private void ResetarTudo()
    {
        progressao = 0;
        foreach (var lt in letras)
            lt.color = new Color32(255, 255, 255, 255);
    }

    void Correto()
    {


        panel.SetActive(false);

        // 1) Achar o Player
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("[PortaPuzzle82] Player não encontrado.");
            return;
        }

        // 2) Achar o caminho correto até o waypoint
        var mbRoot = GameObject.Find("MapBounds")?.transform;
        if (mbRoot == null)
        {
            Debug.LogError("[PortaPuzzle82] Não encontrei o root 'MapBounds'.");
            return;
        }

        var piso = mbRoot.Find("Piso 2");
        if (piso == null)
        {
            Debug.LogError("[PortaPuzzle82] Não encontrei 'Piso 2'.");
            return;
        }

        // 4) Atualizar o Confiner dinamicamente
        var confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        var corredorPsicologia = piso.Find("CorredorPsicologia");
        var poly = corredorPsicologia.GetComponent<PolygonCollider2D>();
        if (confiner != null && poly != null)
        {
            confiner.BoundingShape2D = poly;
            Debug.Log("[PortaPuzzle82] Confiner atualizado para CorredorPsicologia.");
        }

        // 6) Mover o Player
        player.transform.position = new Vector3(91.57f, 69.75f, 0);

        if (confiner != null)
        {
            confiner.InvalidateBoundingShapeCache();
        }

        Vector3 oldPos = player.transform.position;
        Vector3 newPos = new Vector3(91.57f, 69.75f, 0);
        player.transform.position = newPos;

        if (virtualCamera != null)
        {
            Vector3 offset = newPos - oldPos;
            virtualCamera.OnTargetObjectWarped(player.transform, offset);
        }
        
        progress.AddProgress();
    }
}
