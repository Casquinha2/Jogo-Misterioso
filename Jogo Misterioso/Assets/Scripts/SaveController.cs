using System.IO;
using Unity.Cinemachine;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        Debug.Log("Save file path: " + saveLocation);
        inventoryController = FindFirstObjectByType<InventoryController>();
        Debug.Log("Save file path: " + inventoryController);
    }

    public void SaveGame()
    {
        try
        {
            SaveData saveData = new SaveData
            {
                playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
                mapBoundary = FindFirstObjectByType<CinemachineConfiner2D>().BoundingShape2D.gameObject.name,
                inventorySaveData = inventoryController.GetInventoryItems()

            };

            
            File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
            Debug.Log($"Game saved successfully to {saveLocation}");

            Debug.Log($"Items guardados inv {saveData.inventorySaveData}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to save game: {e.Message}");
        }
    }

    public void LoadGame()
    {
        if(File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));

            GameObject.FindGameObjectWithTag("Player").transform.position = saveData.playerPosition;

            FindFirstObjectByType<CinemachineConfiner2D>().BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();

            inventoryController.SetInventoryItems(saveData.inventorySaveData);
        }
        else
        {
            SaveGame();
        }
    }
}