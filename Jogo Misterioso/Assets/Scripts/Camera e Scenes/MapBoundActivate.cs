using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapBoundActivate : MonoBehaviour
{
    [SerializeField] Transform mapBounds;
    private List<GameObject> allChildren = new List<GameObject>();
    public List<GameObject> deactivate = new List<GameObject>();

    void Awake()
    {
        // coleta os filhos UMA vez
        foreach (Transform child in mapBounds)
            allChildren.Add(child.gameObject);
    }

    public void Start()
    {
        foreach (GameObject i in deactivate)
        {
            if (i.name == "CorredorBiblioteca_Waypoint")
            {
                i.SetActive(true);
            }
            else
            {
                i.SetActive(false);
            }
        }

        string nomeCena = SceneManager.GetActiveScene().name;

        foreach (GameObject child in allChildren)
        {
            bool deveAtivar = nomeCena switch
            {
                "QuartoScene" when child.name == "Quarto" => true,
                "Piso-1Scene" when child.name == "Piso -1" => true,
                "Piso0Scene" when child.name == "Piso 0" => true,
                "Piso1Scene" when child.name == "Piso 1" => true,
                "Piso2Scene" when child.name == "Piso 2" => true,
                _ => false
            };

            child.SetActive(deveAtivar);
        }
        enabled = false;
    }

    public void AfterProgress(int progress)
    {
        foreach (GameObject child in deactivate)
        {
            if (child.name == "Corredor62_Waypoint" && progress >= 1)
            {
                child.SetActive(true);
            }
            if ((child.name == "CorredorPsicologia_Waypoint" || child.name == "82_Waypoint") && progress >= 6)
            {
                child.SetActive(true);
            }
            else if (child.name == "CorredorBiblioteca_Waypoint" && progress == 8)
            {
                child.SetActive(false);
            }
            else if (child.name == "CorredorBiblioteca_Waypoint" && progress >= 9)
            {
                child.SetActive(true);
            }
            else if (child.name == "Corredor63_Waypoint" && progress >= 13)
            {
                child.SetActive(true);
            }
        }
    }
}