using UnityEngine;

public class Progress : MonoBehaviour
{
    private int progress;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        progress = 0;
    }

    // Update is called once per frame
    public void AddProgress()
    {
        progress++;
        Debug.Log($"[Progress] novo valor = {progress}");
    }

    public int GetProgress()
    {
        return progress;
    }
}
