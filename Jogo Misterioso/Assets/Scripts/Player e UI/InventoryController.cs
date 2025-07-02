using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    private List<InventorySaveData> inventorySaveData = new List<InventorySaveData>();


    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    private int slotCount = 12;
    public GameObject[] itemPrefabs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        DontDestroyOnLoad(gameObject);

        itemDictionary = FindFirstObjectByType<ItemDictionary>();

        SetInventoryItems(inventorySaveData);

        /*
        for(int i = 0; i < slotCount; i++)
        {
            Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
            if(i < itemPrefabs.Length)
            {
                GameObject item = Instantiate(itemPrefabs[i], slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = item;
            }
        }
        */
    }

    public bool AddItem(GameObject itemPrefab)
    {
        //Procura um slot vazio
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                return true;
            }
        }

        Debug.Log("Inventory is full!");
        return false;
    }
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
            }
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        StartCoroutine(RebuildInventory(inventorySaveData));
    }

    private IEnumerator RebuildInventory(List<InventorySaveData> dataList)
    {
        // 1) Limpa todos os filhos
        for (int i = inventoryPanel.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryPanel.transform.GetChild(i).gameObject);
        }

        // 2) Aguarda um frame para o Unity processar os Destroy()
        yield return null;

        // 3) (Re)cria os slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }


        // 4) Preenche com os itens salvos
        foreach (var data in dataList)
        {
            if (data.slotIndex < slotCount)
            {
                var slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                var itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    var item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = item;
                }
                else
                {
                    Debug.LogError($"Item ID {data.itemID} não encontrado!");
                }
            }
            else
            {
                Debug.LogError($"SlotIndex {data.slotIndex} fora de alcance!");
            }
        }
    }
    
    public void ClearInventory()
    {
        foreach (Transform slotT in inventoryPanel.transform)
        {
            var slot = slotT.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) 
                continue;

            Destroy(slot.currentItem);
            slot.currentItem = null;
        }
        Debug.Log("[InventoryController] Inventário completamente limpo.");
    }
}
