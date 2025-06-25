using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapBoundActivate : MonoBehaviour
{
    [SerializeField] Transform mapBounds;
    private List<GameObject> allChildren = new List<GameObject>();

    void Awake()
    {
        // coleta os filhos UMA vez
        foreach (Transform child in mapBounds)
            allChildren.Add(child.gameObject);
    }

    void Start()
    {
        string nomeCena = SceneManager.GetActiveScene().name;

        foreach (GameObject child in allChildren)
        {
            bool deveAtivar = nomeCena switch
            {
                "QuartoScene" when child.name == "QuartoScene" => true,
                "Piso0Scene"  when child.name == "Piso 0"     => true,
                "Piso1Scene"  when child.name == "Piso 1"     => true,
                "Piso2Scene"  when child.name == "Piso 2"     => true,
                _ => false
            };

            child.SetActive(deveAtivar);
        }

        enabled = false;
    }

}