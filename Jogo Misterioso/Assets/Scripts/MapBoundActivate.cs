using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapBoundActivate : MonoBehaviour
{
    [SerializeField] Transform mapBounds;

    // Pode usar List em vez de array para não precisar saber o tamanho antes
    private List<GameObject> allChildren = new List<GameObject>();

    void Awake()
    {
        // Pega todos os filhos no Awake (uma vez só)
        foreach (Transform child in mapBounds)
            allChildren.Add(child.gameObject);
    }

    void OnEnable()
    {
        // Assina o evento de cena carregada
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        // Cancela a inscrição ao desativar este componente
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Esse método é chamado toda vez que uma cena é carregada
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string nomeCena = scene.name;
        foreach (GameObject child in allChildren)
        {
            // Padronize nomes pra evitar espaços/exceções…
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
    }
}