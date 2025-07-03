using UnityEngine;
using System.Collections;
using TMPro;
using System;

public class ObjDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject objDialoguePanelPrefab;

    [Header("Dados de Diálogo")]
    public ObjectInteractionDialogue objDialogueData;

    [Header("Item (opcional)")]
    public GameObject itemPrefab;
    public ItemActionType itemAction = ItemActionType.Adicionar;

    [Header("Adicionar progresso?")]
    public bool adicionarProgresso = false;

    [Header("Se o adicionarProgresso for true, adicionar o Personagens gameobject")]
    public Progress progress;

    public enum ItemActionType
    {
        Adicionar,
        Remover
    }


    private GameObject inventoryPanel;
    private InventoryController inventoryController;

    private GameObject objDialoguePanelInstance;
    private TMP_Text objDialogueText;

    private int objDialogueIndex;
    private bool objIsTyping, objIsDialogueActive;

    public static event Action<ObjDialogue> OnDialogueEnded;

    private static ObjDialogue currentActiveDialogue;

    void Start()
    {
        if (itemPrefab != null)
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

        DialogueManager.Instance.OnNewDialogue += CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue += HandleResume;
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNewDialogue -= CancelDialogue;
            DialogueManager.Instance.OnPauseDialogue -= HandlePause;
            DialogueManager.Instance.OnResumeDialogue -= HandleResume;
        }
    }

    bool InventoryContainsItem(GameObject item)
    {
        foreach (Transform slotT in inventoryPanel.transform)
        {
            var slot = slotT.GetComponent<Slot>();
            if (slot?.currentItem == null) continue;

            var invItem = slot.currentItem.GetComponent<Item>();
            if (invItem != null && invItem.ID == item.GetComponent<Item>().ID)
            {
                return true;
            }
        }
        return false;
    }


    public bool CanInteract() => !objIsDialogueActive;

    public void Interact()
    {
        if (objDialogueData == null || (PauseController.IsGamePaused && !objIsDialogueActive))
            return;

        if (itemPrefab != null && inventoryPanel != null && inventoryController != null)
        {
            if (itemAction == ItemActionType.Remover)
            {
                foreach (Transform slotT in inventoryPanel.transform)
                {
                    var slot = slotT.GetComponent<Slot>();
                    if (slot?.currentItem == null) continue;
                    var invItem = slot.currentItem.GetComponent<Item>();
                    if (invItem != null && invItem.ID == itemPrefab.GetComponent<Item>().ID)
                    {
                        Destroy(slot.currentItem);
                        slot.currentItem = null;
                        break;
                    }
                }
            }
            
            if (itemAction == ItemActionType.Adicionar)
            {
                // Só adiciona se ainda não tiver esse item no inventário
                if (!InventoryContainsItem(itemPrefab))
                {
                    inventoryController.AddItem(itemPrefab);
                }
            }

        }


        DialogueManager.Instance.RequestNewDialogue(this);

        // Cancela o diálogo anterior se estiver ativo
        if (currentActiveDialogue != null && currentActiveDialogue != this)
        {
            currentActiveDialogue.CancelDialogue();
        }

        currentActiveDialogue = this;

        if (objIsDialogueActive) NextLine();
        else StartObjDialog();
    }

    void StartObjDialog()
    {
        objIsDialogueActive = true;
        objDialogueIndex = 0;

        CreateDialoguePanel();

        StartCoroutine(TypeLine());
    }

    void CreateDialoguePanel()
    {
        if (objDialoguePanelInstance != null) return;

        var canvas = GameObject.FindWithTag("UICanvas");
        if (canvas == null)
        {
            Debug.LogError("🚫 Canvas não encontrado!", this);
            return;
        }

        objDialoguePanelInstance = Instantiate(objDialoguePanelPrefab, canvas.transform, false);
        objDialogueText = objDialoguePanelInstance.GetComponentInChildren<TMP_Text>();
        objDialoguePanelInstance.SetActive(true);
    }

    void DestroyDialoguePanel()
    {
        if (objDialoguePanelInstance != null)
        {
            Destroy(objDialoguePanelInstance);
            objDialoguePanelInstance = null;
            objDialogueText = null;
        }
    }

    void NextLine()
    {
        if (objIsTyping)
        {
            StopAllCoroutines();
            objDialogueText?.SetText(objDialogueData.dialogueLines[objDialogueIndex]);
            objIsTyping = false;
            return;
        }

        objDialogueIndex++;

        if (objDialogueIndex < objDialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        objIsTyping = true;
        objDialogueText.text = "";

        foreach (var ch in objDialogueData.dialogueLines[objDialogueIndex])
        {
            objDialogueText.text += ch;
            yield return new WaitForSeconds(objDialogueData.typingSpeed);
        }

        objIsTyping = false;

        if (objDialogueData.autoProgressLines.Length > objDialogueIndex &&
            objDialogueData.autoProgressLines[objDialogueIndex])
        {
            yield return new WaitForSeconds(objDialogueData.autoProgressDelay);
            NextLine();
        }
    }

    public void CancelDialogue()
    {
        if (!objIsDialogueActive) return;

        StopAllCoroutines();
        objIsDialogueActive = false;

        DestroyDialoguePanel();

        if (currentActiveDialogue == this)
            currentActiveDialogue = null;
    }

    void HandlePause()
    {
        if (!objIsDialogueActive || objDialoguePanelInstance == null) return;
        StopAllCoroutines();
        objDialoguePanelInstance.SetActive(false);
    }

    void HandleResume()
    {
        if (!objIsDialogueActive) return;

        if (objDialoguePanelInstance != null)
        {
            objDialoguePanelInstance.SetActive(true);
            StartCoroutine(TypeLine());
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        objIsDialogueActive = false;

        DestroyDialoguePanel();

        if (currentActiveDialogue == this)
            currentActiveDialogue = null;

        OnDialogueEnded?.Invoke(this);

        if (adicionarProgresso && progress != null)
        {
            progress.AddProgress();
        }
    }
}