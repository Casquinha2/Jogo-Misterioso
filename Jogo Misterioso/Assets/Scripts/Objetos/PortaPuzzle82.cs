using UnityEngine;
using Unity.Cinemachine;
using TMPro;
using System.Collections.Generic;

public class PsicologiaPuzzle : MonoBehaviour
{
    [Header("Identificador único deste puzzle")]
    [Tooltip("Será usado em SessionState para bloquear a interação")]
    [SerializeField] private string puzzleID;

    [Header("Letras do puzzle")]
    [SerializeField] private List<TMP_Text> letras;

    [Header("Referências")]
    [SerializeField] private GameObject panel;
    [SerializeField] private OpenCustomUI openUI; // <-- Nova referência direta

    private Progress progress;
    private CinemachineCamera virtualCamera;
    private int progressao = 0;
    private bool jaResolvido;

    void Awake()
    {
        progress = FindFirstObjectByType<Progress>();
        if (progress == null)
            Debug.LogError("[PsicologiaPuzzle] Progress não encontrado na cena!");

        if (openUI == null)
            Debug.LogWarning("[PsicologiaPuzzle] Referência 'openUI' não atribuída no Inspector.");
    }

    void Start()
    {
        // Pega a câmera virtual, se houver
        var cam = GameObject.FindGameObjectWithTag("CmCamera");
        if (cam != null)
            virtualCamera = cam.GetComponent<CinemachineCamera>();

        // Se já resolvemos este puzzle, marca e fecha o painel
        if (!string.IsNullOrEmpty(puzzleID) &&
            SessionState.solvedPuzzles.Contains(puzzleID))
        {
            jaResolvido = true;
            panel.SetActive(false);
        }
        else
        {
            // Garante que o painel comece fechado
            panel.SetActive(false);
        }
    }

    public void check(TMP_Text letra)
    {
        if (jaResolvido) 
            return;

        switch (progressao)
        {
            case 0: if (letra.text == "D") Next(letra); else ResetarTudo(); break;
            case 1: if (letra.text == "I") Next(letra); else ResetarTudo(); break;
            case 2: if (letra.text == "R") Next(letra); else ResetarTudo(); break;
            case 3: if (letra.text == "E") Next(letra); else ResetarTudo(); break;
            case 4: if (letra.text == "I") Next(letra); else ResetarTudo(); break;
            case 5: if (letra.text == "T") Next(letra); else ResetarTudo(); break;
            case 6:
                if (letra.text == "O")
                {
                    letra.color = Color.green;
                    Correto();
                }
                else ResetarTudo();
                break;
        }
    }

    private void Next(TMP_Text letra)
    {
        progressao++;
        letra.color = new Color32(0, 255, 0, 255);
    }

    private void ResetarTudo()
    {
        progressao = 0;
        foreach (var lt in letras)
            lt.color = Color.white;
    }

    private void Correto()
    {
        panel.SetActive(false);
        jaResolvido = true;

        if (!string.IsNullOrEmpty(puzzleID))
            SessionState.solvedPuzzles.Add(puzzleID);

        if (openUI != null)
            openUI.MarkAsSolved();

        var player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            var mbRoot = GameObject.Find("MapBounds")?.transform;
            var piso = mbRoot?.Find("Piso 2");
            var corredor = piso?.Find("CorredorPsicologia");
            var poly = corredor?.GetComponent<PolygonCollider2D>();
            var conf = FindFirstObjectByType<CinemachineConfiner2D>();

            if (conf != null && poly != null)
            {
                conf.BoundingShape2D = poly;
                conf.InvalidateBoundingShapeCache();
            }

            var oldPos = player.transform.position;
            var newPos = new Vector3(91.57f, 69.75f, 0);
            player.transform.position = newPos;
            virtualCamera?.OnTargetObjectWarped(player.transform, newPos - oldPos);
        }

        progress.AddProgress();
    }
}
