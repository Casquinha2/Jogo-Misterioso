using UnityEngine;

public class Progress : MonoBehaviour
{
    [Header("Personagens Principais")]
    public Transform principais;

    [Header("Personagens Secundarias")]
    public Transform secundarias;

    [Header("Personagens Perseguicao")]
    public GameObject perseguicao;

    [Header("Objetos importantes")]
    public Transform objetos;

    private int progress;
    private MapBoundActivate mapBoundScript;
    private InventoryController inventoryController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        mapBoundScript = FindFirstObjectByType<MapBoundActivate>();
        if (mapBoundScript == null)
            Debug.LogError("Não encontrei MapBoundActivate na cena!");

        inventoryController = FindFirstObjectByType<InventoryController>();
        if (inventoryController == null)
            Debug.LogError("[Progress] InventoryController não encontrado na cena!");

        progress = 0;
        AtualizarProgress();
        
        perseguicao.SetActive(false);
    }

    // Update is called once per frame
    public void AddProgress()
    {
        progress++;
        Debug.Log($"[Progress] novo valor = {progress}");
        AtualizarProgress();

        mapBoundScript.AfterProgress(progress);

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
            else if (i.name == "CCCaloiro")
            {
                switch (progress)
                {
                    case 0:

                        i.gameObject.SetActive(false);
                        break;

                    case 8:
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
                    case 7:
                        i.SetActive(false);
                        break;
                }
            }
            else if (i.name == "Trajado 1")
            {
                GameObject filho = i.transform.GetChild(0).gameObject;
                switch (progress)
                {
                    case 0:
                        i.SetActive(false);
                        filho.SetActive(false);
                        break;

                    case 3:
                        i.SetActive(true);
                        filho.SetActive(true);
                        break;
                    case 4:
                        filho.SetActive(false);
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
                            i.SetActive(false);
                            break;

                        case 9:
                            i.SetActive(true);
                            i.transform.localPosition = new Vector3(131.61f, -142.43f, 0f);
                            break;

                        case 10:
                            i.SetActive(false);
                            break;


                    }
                }
                if (i.name == "PapeisDireito")
                {
                    switch (progress)
                    {
                        case 0:
                            i.SetActive(false);
                            break;
                        case 6:
                            i.SetActive(true);
                            break;
                        case 7:
                            i.SetActive(false);
                            break;
                    }
                }
                if (i.name == "Livros")
                {
                    switch (progress)
                    {
                        case 0:
                            i.SetActive(false);
                            break;

                        case 7:
                            i.SetActive(true);
                            break;

                        case 8:
                            i.SetActive(false);
                            break;
                    }
                }
            }

        if (progress == 10)
        {
            perseguicao.SetActive(true);
        }
        else
        {
            perseguicao.SetActive(false);
        }

        if (progress == 4 || progress == 11)
        {
            if (inventoryController != null)
                inventoryController.ClearInventory();
            else
                Debug.LogWarning("[Progress] InventoryController não atribuído; inventário não limpo.");
        }
    }
}
