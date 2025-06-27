using UnityEngine;

public class Progress : MonoBehaviour
{
    [Header("Personagens Principais")]
    public Transform principais;

    [Header("Personagens Secundarias")]
    public Transform secundarias;

    private int progress;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        progress = 0;
        AtualizarProgress();
    }

    // Update is called once per frame
    public void AddProgress()
    {
        progress++;
        Debug.Log($"[Progress] novo valor = {progress}");
        AtualizarProgress();
    }

    public int GetProgress()
    {
        return progress;
    }

    public void AtualizarProgress()
    {
        foreach (Transform child in principais)
        {
            GameObject npc = child.gameObject;

            if (npc.name == "Irmao De Praxe")
            {
                switch (progress)
                {
                    case 0:
                        npc.SetActive(true);
                        npc.transform.localPosition = new Vector3(98.37f, -21.46f, 0f);
                        break;
                    case 1:
                        Debug.Log("Teste nos tps dos personagens");
                        break;
                }
            }
            else if (npc.name == "Pato Guilherme")
            {
                switch (progress)
                {
                    case 0:
                        npc.SetActive(false);
                        break;
                    case 1:
                        npc.SetActive(true);
                        npc.transform.localPosition = new Vector3(1.07f, 2.38f, 0f);
                        break;
                    case 3:
                        Debug.Log("Teste nos tps dos personagens");
                        break;
                }
            }
        }
    }
}
