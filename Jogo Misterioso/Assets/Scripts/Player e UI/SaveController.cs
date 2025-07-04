using System.IO;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

[DefaultExecutionOrder(-5)]
public class SaveController : MonoBehaviour
{
    private static SaveData pendingLoadData;
    private string saveLocation;
    private InventoryController inventoryController;
    [SerializeField] private Progress progress;

    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject loadingScreen;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log("Save file path: " + saveLocation);
        inventoryController = FindFirstObjectByType<InventoryController>();
        Debug.Log("Save file path: " + inventoryController);

        if (progress == null)
        {
            progress = FindFirstObjectByType<Progress>();
            if (progress == null)
            {
                Debug.LogError("Progress não foi encontrado na cena!");
            }
            else
            {
                Debug.Log("Progress atribuído automaticamente.");
            }
        }
    }

    public void SaveGame()
    {
        try
        {
            SaveData saveData = new SaveData
            {
                playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
                mapBoundary = FindFirstObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,
                inventorySaveData = inventoryController.GetInventoryItems(),
                progressData = progress.GetProgress(),
                sceneToLoad = SceneManager.GetActiveScene().name,
                solvedPuzzles = SessionState.solvedPuzzles.ToList()

            };


            File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
            Debug.Log($"Game saved successfully to {saveLocation}");

            Debug.Log($"Items guardados inv {saveData.inventorySaveData}");
            Debug.Log($"Progress guardado int {saveData.progressData}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void OnClickLoad()
    {
        settingsMenu.SetActive(false);    // Fecha menu
        loadingScreen.SetActive(true);    // Mostra loading
        PauseController.SetPause(false);   // Despausa o jogo (importante!)

        LoadGame();  // Executa load
    }


    private void LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            SaveGame(); // Se não houver save, cria um
            return;
        }

        pendingLoadData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

        if (SceneManager.GetActiveScene().name != pendingLoadData.sceneToLoad)
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(pendingLoadData.sceneToLoad);
        }
        else
        {
            ApplySave(pendingLoadData);
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Evita múltiplas chamadas
        StartCoroutine(ApplySaveNextFrame());
    }

    private IEnumerator ApplySaveNextFrame()
    {
        // Espera 1 frame
        yield return null;

        // Aplica o save depois de tudo estar carregado
        ApplySave(pendingLoadData);
    }



    private void ApplySave(SaveData data)
    {
        PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canMove = false;  // Bloqueia o movimento
        }


        var player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.transform.position = data.playerPosition;
        }
        else
        {
            Debug.LogError("[ApplySave] Player não encontrado!");
            return; // sem player não adianta continuar
        }

        var confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        var virtualCamera = FindFirstObjectByType<CinemachineCamera>();

        if (virtualCamera == null)
        {
            Debug.LogError("[ApplySave] CinemachineVirtualCamera não encontrado!");
            return;
        }

        // Teletransporta a câmera para a posição do player antes de atualizar o confiner
        virtualCamera.transform.position = new Vector3(data.playerPosition.x, data.playerPosition.y, virtualCamera.transform.position.z);

        GameObject mapBoundsRoot = GameObject.FindGameObjectWithTag("MapBoundsRoot");
        if (mapBoundsRoot == null)
        {
            Debug.LogError("[ApplySave] GameObject com a tag 'MapBoundsRoot' não foi encontrado!");
            return;
        }

        var polygonsInMapBounds = mapBoundsRoot.GetComponentsInChildren<PolygonCollider2D>(true);
        var targetPolygon = polygonsInMapBounds.FirstOrDefault(p => p.gameObject.name == data.mapBoundary);

        if (confiner != null && targetPolygon != null)
        {
            confiner.BoundingShape2D = targetPolygon;
        }
        else
        {
            Debug.LogError("[ApplySave] Confiner ou PolygonCollider2D com nome correspondente não encontrados!");
        }

        // Continua com inventory, progress e puzzles
        if (inventoryController == null)
        {
            inventoryController = FindFirstObjectByType<InventoryController>();
        }
        if (inventoryController != null)
        {
            inventoryController.SetInventoryItems(data.inventorySaveData);
        }
        else
        {
            Debug.LogError("[ApplySave] InventoryController null!");
        }

        if (progress == null)
        {
            progress = FindFirstObjectByType<Progress>();
        }
        if (progress != null)
        {
            progress.SetProgress(data.progressData);
        }
        else
        {
            Debug.LogError("[ApplySave] Progress null!");
        }

        SessionState.solvedPuzzles = new HashSet<string>(data.solvedPuzzles);

        Debug.Log("[ApplySave] Save aplicado com sucesso.");

        StartCoroutine(HideLoadingAfterDelay(5f)); // espera 5 segundos
    }

    private IEnumerator HideLoadingAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);

        if (loadingScreen != null)
        {
            loadingScreen.SetActive(false);
        }

        // Aqui libera o movimento após a espera
        PlayerMovement playerMovement = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerMovement>();
        if (playerMovement != null)
        {
            playerMovement.canMove = true;      
        }
    }

}