using System.Collections.Generic;
using UnityEditor.Animations;
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

    void Start()
    {
        foreach (GameObject i in deactivate)
        {
            i.SetActive(false);
        }



        string nomeCena = SceneManager.GetActiveScene().name;

        foreach (GameObject child in allChildren)
        {
            bool deveAtivar = nomeCena switch
            {
                "QuartoScene" when child.name == "Quarto" => true,
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
        if (progress == 6)
        {
            foreach (GameObject child in deactivate)
            {
                if (child.name == "CorredorPsicologia_Waypoint" || child.name == "82_Waypoint")
                {
                    child.SetActive(true);
                }
            }
        }
    }

}