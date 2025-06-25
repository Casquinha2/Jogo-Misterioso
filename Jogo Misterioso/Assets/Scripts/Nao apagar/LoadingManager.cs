using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public static class LoadingManager
{
    private static GameObject _blackPanel;
    private static bool       _subscribed = false;

    /// <summary>
    /// Registra o painel de loading. Deve ser chamado uma única vez no seu Start().
    /// </summary>
    public static void RegisterPanel(GameObject panel)
    {
        _blackPanel = panel;
    }

    /// <summary>
    /// Ativa imediatamente a tela preta.
    /// </summary>
    public static void ShowLoading()
    {
        if (_blackPanel != null)
            _blackPanel.SetActive(true);
    }

    /// <summary>
    /// Esconde o loading após 1 segundo via coroutine no runner passado.
    /// </summary>
    public static void HideLoadingWithDelay(MonoBehaviour runner)
    {
        if (_blackPanel == null || runner == null) return;
        runner.StartCoroutine(DelayedHide());
    }

    private static IEnumerator DelayedHide()
    {
        yield return new WaitForSeconds(1f);
        _blackPanel.SetActive(false);
    }

    /// <summary>
    /// Se ainda não inscrito, registra um callback para sceneLoaded
    /// e esconde automaticamente após o load.
    /// </summary>
    public static void SubscribeAutoHide()
    {
        if (_subscribed) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
        _subscribed = true;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // aqui você pode optar por chamar HideLoadingWithDelay via algum runner,
        // mas se quiser somente ocultar na hora:
        if (_blackPanel != null)
            _blackPanel.SetActive(false);

        SceneManager.sceneLoaded -= OnSceneLoaded;
        _subscribed = false;
    }
}
