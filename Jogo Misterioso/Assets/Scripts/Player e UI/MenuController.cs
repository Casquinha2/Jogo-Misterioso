using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.InputSystem;
using System.Linq;

public class MenuController : MonoBehaviour
{
    // NOTA: como o Menu fica em “Donotdestroy” e começa desativado,
    // precisamos de um método de busca que encontre GameObjects inativos.
    GameObject menuCanvas;

    GameObject tutorialPanel;
    TMP_Text tutorialText;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // 1) tenta FindWithTag (só encontra ativos)
        menuCanvas = GameObject.FindWithTag("MenuCanvas");

        // 2) fallback: busca em TODOS os Transforms, inclusive inativos
        if (menuCanvas == null)
        {
            var all = Resources.FindObjectsOfTypeAll<Transform>();
            menuCanvas = all
                .Where(t => t.CompareTag("MenuCanvas"))
                .Select(t => t.gameObject)
                .FirstOrDefault();
        }

        if (menuCanvas == null)
            Debug.LogError("MenuCanvas não encontrado! Ativa-o ou confirma a Tag.");

        // 3) garante que o menu começa fechado
        else
            menuCanvas.SetActive(false);

        // 4) só iremos ligar o tutorial quando "QuartoScene" carregar
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // ativa / localiza o painel de tutorial apenas na QuartoScene
        if (scene.name != "QuartoScene") return;

        // busca ativo ou inativo (igual ao menuCanvas)
        tutorialPanel = GameObject.FindWithTag("TutorialPanel");
        if (tutorialPanel == null)
        {
            var roots = SceneManager.GetActiveScene().GetRootGameObjects();
            tutorialPanel = roots.FirstOrDefault(go => go.CompareTag("TutorialPanel"));
        }

        if (tutorialPanel != null)
        {
            tutorialText = tutorialPanel.GetComponentInChildren<TMP_Text>();
            tutorialPanel.SetActive(true);
        }
        else Debug.LogWarning("TutorialPanel não encontrado em QuartoScene.");
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame ||
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            // 1) Toggle do menu
            if (menuCanvas != null)
            {
                if (!menuCanvas.activeSelf && PauseController.IsGamePaused)
                {
                    return;
                }
                
                bool open = !menuCanvas.activeSelf;
                menuCanvas.SetActive(open);

                // 2) pausar / retomar jogo
                PauseController.SetPause(menuCanvas.activeSelf);
                
                if (open) DialogueManager.Instance?.PauseDialogue();
                else DialogueManager.Instance?.ResumeDialogue();
            }

            // 3) Toggle do tutorial (caso este exista na cena)
            if (tutorialPanel != null && tutorialText != null)
            {
                // só esconde se o texto final não foi alcançado
                if (tutorialText.text != "TUTORIAL ACABADO")
                    tutorialPanel.SetActive(!tutorialPanel.activeSelf);
            }
        }
    }
}
