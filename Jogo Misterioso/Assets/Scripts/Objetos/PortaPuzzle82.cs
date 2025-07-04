using UnityEngine;
using UnityEngine.UI;
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

    private Progress progress;
    private CinemachineCamera virtualCamera;
    private int progressao = 0;
    private bool jaResolvido;

    void Awake()
    {
        progress = FindFirstObjectByType<Progress>();
        if (progress == null)
            Debug.LogError("[PsicologiaPuzzle] Progress não encontrado na cena!");
    }

    void Start()
    {
        // pega a câmera virtual
        var cam = GameObject.FindGameObjectWithTag("CmCamera");
        if (cam != null)
            virtualCamera = cam.GetComponent<CinemachineCamera>();

        // Se já resolvemos este puzzle nesta sessão, bloqueia tudo
        if (!string.IsNullOrEmpty(puzzleID) &&
            SessionState.solvedPuzzles.Contains(puzzleID))
        {
            jaResolvido = true;
            panel.SetActive(false);
            DisableInteraction();
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
        // fecha o painel
        panel.SetActive(false);

        // marca resolvido nesta sessão
        if (!string.IsNullOrEmpty(puzzleID) && !jaResolvido)
        {
            jaResolvido = true;
            SessionState.solvedPuzzles.Add(puzzleID);
            DisableInteraction();
        }

        // teleporta jogador e atualiza confiner
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;

        var mbRoot = GameObject.Find("MapBounds")?.transform;
        var piso   = mbRoot?.Find("Piso 2");
        var corredor = piso?.Find("CorredorPsicologia");
        var poly   = corredor?.GetComponent<PolygonCollider2D>();
        var conf   = FindFirstObjectByType<CinemachineConfiner2D>();

        if (conf != null && poly != null)
        {
            conf.BoundingShape2D = poly;
            conf.InvalidateBoundingShapeCache();
        }

        Vector3 oldPos = player.transform.position;
        Vector3 newPos = new Vector3(91.57f, 69.75f, 0);
        player.transform.position = newPos;
        virtualCamera?.OnTargetObjectWarped(player.transform, newPos - oldPos);

        // incrementa progress
        progress.AddProgress();
    }

    private void DisableInteraction()
    {
        // desliga este script
        enabled = false;

        // impede futuras chamadas de check(...)
        foreach (var lt in letras)
        {
            if (lt.TryGetComponent<Button>(out var btn))
                btn.interactable = false;
        }

        // opcional: desativa collider se houver
        if (TryGetComponent<Collider2D>(out var c))
            c.enabled = false;
    }
}
