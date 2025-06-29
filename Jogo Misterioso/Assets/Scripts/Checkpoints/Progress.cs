using UnityEngine;

public class Progress : MonoBehaviour
{
    [Header("Personagens Principais")]
    public Transform principais;

    [Header("Personagens Secundarias")]
    public Transform secundarias;

    [Header("Objetos importantes")]
    public Transform objetos;

    private int progress;
    private MapBoundActivate mapBoundScript;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapBoundScript = FindFirstObjectByType<MapBoundActivate>();
        if (mapBoundScript == null)
            Debug.LogError("Não encontrei MapBoundActivate na cena!");

        progress = 0;
        AtualizarProgress();
    }

    // Update is called once per frame
    public void AddProgress()
    {
        progress++;
        Debug.Log($"[Progress] novo valor = {progress}");
        AtualizarProgress();

        if (progress == 6)
        {
            mapBoundScript.AfterProgress(progress);
        }
    }

    public int GetProgress()
    {
        return progress;
    }

    public void AtualizarProgress()
    {
        foreach (Transform child in principais)
        {
            GameObject i = child.gameObject;

            if (i.name == "Irmao De Praxe")
            {
                switch (progress)
                {
                    case 0:
                        i.SetActive(true);
                        i.transform.localPosition = new Vector3(59.86f, 8.74f, 0f);
                        break;
                }
            }
            else if (i.name == "Pato Guilherme")
            {
                switch (progress)
                {
                    case 0:
                        i.SetActive(false);
                        break;
                    case 1:
                        i.SetActive(true);
                        i.transform.localPosition = new Vector3(-37.19f, 33.13f, 0f);
                        break;
                    case 4:
                        Debug.Log("FAZ NADA");
                        break;
                }
            }
            else if (i.name == "Praxe Psicologia")
            {
                switch (progress)
                {
                    case 0:

                        i.gameObject.SetActive(false);
                        break;

                    case 4:
                        i.gameObject.SetActive(true);
                        break;
                }
            }
        }

        foreach (Transform child in secundarias)
        {
            GameObject i = child.gameObject;

            if (i.name == "Psicologia Homem")
            {
                switch (progress)
                {
                    case 0:
                        i.SetActive(false);
                        i.transform.localPosition = new Vector3(88.69f, 70.33f, 0f);
                        break;
                    case 6:
                        i.SetActive(true);
                        break;
                }
            }
        }

        foreach (Transform child in objetos)
        {
            GameObject i = child.gameObject;

            if (i.name == "Capa")
            {
                switch (progress)
                {
                    case 0:
                        i.SetActive(false);
                        i.transform.localPosition = new Vector3(-14.39f, 79.39f, 0f);
                        break;

                    case 2:
                        i.SetActive(true);
                        break;

                    case 3:
                        Destroy(i);
                        break;
                }
            }
        }
    }
}
