using UnityEngine;

public class Bondoso : MonoBehaviour
{
    [SerializeField] private NpcDialogue npcDialogue;
    [SerializeField] private GameObject panelEscolha;
    [SerializeField] private GameObject panelLuz;
    [SerializeField] private GameObject panelFinal;

    [SerializeField] private GameObject itemPrefab;

    private GameObject inventoryPanel;
    private InventoryController inventoryController;


    void Start()
    {
        inventoryController = FindFirstObjectByType<InventoryController>();
            if (inventoryController == null)
                Debug.LogError("❌ InventoryController não encontrado!", this);

            inventoryPanel = GameObject.FindWithTag("InventoryPanel");
            if (inventoryPanel == null)
            {
                foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                {
                    if (t.gameObject.CompareTag("InventoryPanel"))
                    {
                        inventoryPanel = t.gameObject;
                        break;
                    }
                }
            }

            if (inventoryPanel == null)
                Debug.LogError("❌ InventoryPanel não encontrado (nem inativo)!", this);
    }
    public void BondosoClick()
    {
        panelEscolha.SetActive(false);
        PauseController.SetPause(false);
        //14 progress
        npcDialogue.Interact();
    }

    public void AddEstatua()
    { 
        inventoryController.AddItem(itemPrefab);  
    }

    public void DesligarLuz()
    {
        panelLuz.SetActive(true);
    }
    public void LigarLuz()
    {
        panelLuz.SetActive(false);
    }

    public void Final()
    {
        panelFinal.SetActive(true);
    }
    
    public void ExitGame()
    {
        #if (UNITY_EDITOR)
                UnityEditor.EditorApplication.isPlaying = false;
        #elif (UNITY_STANDALONE)
                    Application.Quit();
        #elif (Unity_WEBGL)
                    SceneManager.LoadScene("QuitScene");
        #endif
    }
}   
