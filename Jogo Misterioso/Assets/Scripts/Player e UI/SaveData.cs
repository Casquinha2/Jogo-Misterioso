using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary; //NOME DA CENA QUE ESTA NA CAMERA (patio, sala 62...)
    public List<InventorySaveData> inventorySaveData;
    public int progressData;

    public string sceneToLoad;
}


