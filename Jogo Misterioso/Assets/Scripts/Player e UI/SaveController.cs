using System.IO;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[DefaultExecutionOrder(-5)]
public class SaveController : MonoBehaviour
{
    private static SaveData pendingLoadData;
    private string saveLocation;
    private InventoryController inventoryController;
    [SerializeField] private Progress progress;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private GameObject loadingScreen;

    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindFirstObjectByType<InventoryController>();
        if (progress == null)
            progress = FindFirstObjectByType<Progress>();
    }

    public void SaveGame()
    {
        var saveData = new SaveData
        {
            playerPosition    = GameObject.FindWithTag("Player").transform.position,
            mapBoundary       = FindFirstObjectByType<CinemachineConfiner2D>()
                                   .BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            progressData      = progress.GetProgress(),
            sceneToLoad       = SceneManager.GetActiveScene().name,
            solvedPuzzles     = SessionState.solvedPuzzles.ToList()
        };
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    public void OnClickLoad()
    {
        settingsMenu.SetActive(false);
        loadingScreen.SetActive(true);
        PauseController.SetPause(false);
        GameObject.FindWithTag("Player")
                  .GetComponent<PlayerMovement>()
                  .canMove = false;

        LoadGame();
    }

    private void LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            SaveGame();
            return;
        }

        pendingLoadData = JsonUtility
            .FromJson<SaveData>(File.ReadAllText(saveLocation));

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(pendingLoadData.sceneToLoad);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(ApplySaveAfterSceneReady());
    }

    private IEnumerator ApplySaveAfterSceneReady()
    {
        var data = pendingLoadData;

        // 1) espera até o MapBoundsRoot estar disponível
        yield return new WaitUntil(() =>
            GameObject.FindWithTag("MapBoundsRoot") != null
        );

        // 2) referências principais
        var player = GameObject.FindWithTag("Player");
        if (player == null)
        {
            Debug.LogError("[ApplySave] Player não encontrado após cena carregada!");
            yield break;
        }

        var vCam = FindFirstObjectByType<CinemachineCamera>();
        if (vCam == null)
            Debug.LogError("[ApplySave] CinemachineCamera não encontrado!");

        // 3) remove Confiner existente do vCam
        if (vCam != null)
        {
            var existingConfiner = vCam.GetComponent<CinemachineConfiner2D>();
            if (existingConfiner != null)
            {
                Destroy(existingConfiner);
                yield return null; // espera remoção
            }
        }

        // 4) teleporta player
        player.transform.position = data.playerPosition;

        // 5) teleporta câmera
        if (vCam != null)
        {
            Vector3 camTarget = new Vector3(
                data.playerPosition.x,
                data.playerPosition.y,
                vCam.transform.position.z
            );
            vCam.transform.position = camTarget;
        }

        // 6) recria Confiner no vCam
        var mapRoot = GameObject.FindWithTag("MapBoundsRoot");
        if (mapRoot != null && vCam != null)
        {
            var poly = mapRoot
                .GetComponentsInChildren<PolygonCollider2D>(true)
                .FirstOrDefault(p => p.gameObject.name == data.mapBoundary);

            if (poly != null)
            {
                var newConf = vCam.gameObject.AddComponent<CinemachineConfiner2D>();
                newConf.BoundingShape2D = poly;
                newConf.InvalidateBoundingShapeCache();
                newConf.OnTargetObjectWarped(
                    vCam as CinemachineVirtualCameraBase,
                    player.transform,
                    Vector3.zero
                );
            }
            else
            {
                Debug.LogError($"[ApplySave] Polygon '{data.mapBoundary}' não encontrado.");
            }
        }
        else
        {
            Debug.LogError("[ApplySave] MapBoundsRoot ou vCam ausente, pulando Confiner.");
        }

        // 7) restaura inventário, progresso e puzzles
        if (inventoryController == null)
            inventoryController = FindFirstObjectByType<InventoryController>();
        if (inventoryController != null)
            inventoryController.SetInventoryItems(data.inventorySaveData);
        else
            Debug.LogError("[ApplySave] inventoryController null!");

        if (progress == null)
            progress = FindFirstObjectByType<Progress>();
        if (progress != null)
            progress.SetProgress(data.progressData);
        else
            Debug.LogError("[ApplySave] progress null!");

        SessionState.solvedPuzzles = new HashSet<string>(data.solvedPuzzles);

        // 8) fecha loading e libera movimento
        if (loadingScreen != null)
            loadingScreen.SetActive(false);
        else
            Debug.LogError("[ApplySave] loadingScreen null!");

        var pm = player.GetComponent<PlayerMovement>();
        if (pm != null)
            pm.canMove = true;
        else
            Debug.LogError("[ApplySave] PlayerMovement não encontrado!");

        Debug.Log("[ApplySave] Cena pronta, tudo configurado.");
    }
}
