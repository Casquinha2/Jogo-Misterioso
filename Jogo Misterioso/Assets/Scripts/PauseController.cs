using UnityEngine;

public class PauseController : MonoBehaviour
{
    private const float DefaultFixedDelta = 0.02f; // valor padrão do Unity

    public static bool IsGamePaused { get; private set; } = false;

    public static void SetPause(bool pause)
    {
        IsGamePaused = pause;

        if (pause)
        {
            // congela a física, animações e qualquer código que use Time.deltaTime
            Time.timeScale = 0f;
            Time.fixedDeltaTime = 0f;
        }
        else
        {
            // volta ao funcionamento normal
            Time.timeScale = 1f;
            Time.fixedDeltaTime = DefaultFixedDelta;
        }
    }
}
