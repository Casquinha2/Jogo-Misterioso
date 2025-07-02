using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using System.Collections.Generic;

public class NpcDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject npcDialoguePanelPrefab;

    [Header("Dados de Diálogo")]
    public NpcInteractionDialogue[] npcDialogueSequence;

    [Header("Item (opcional)")]
    public GameObject itemPrefab;

    [Header("Mais opcoes")]
    public bool manyInteractions = false;
    public bool moonTalks = true;

    private bool adicionarProgresso;

    private GameObject inventoryPanel;
    private InventoryController inventoryController;

    private GameObject npcDialoguePanelInstance;
    private TMP_Text npcDialogueText, npcNameText;
    private Image npcPortraitImage;
    private Button npcCloseButton;

    private int npcDialogueIndex;
    private bool npcIsTyping, npcIsDialogueActive;

    private int currentDialogueDataIndex = 0;
    private int lastInteraction;

    public static event Action<NpcDialogue> OnDialogueEnded;

    public Progress progress;

    private static NpcDialogue currentActiveDialogue;

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

    public bool CanInteract() => !npcIsDialogueActive;

    public void Interact()
    {
        if (npcDialogueSequence.Length == 0)
        {
            GetDialoguesSequence(progress.GetProgress());
        }

        if (PauseController.IsGamePaused && !npcIsDialogueActive)
            return;

        if (npcIsDialogueActive)
            return;

        bool hasItem = false;
        if (itemPrefab != null && inventoryPanel != null && inventoryController != null)
        {
            foreach (Transform slotT in inventoryPanel.transform)
            {
                var slot = slotT.GetComponent<Slot>();
                if (slot?.currentItem == null) continue;
                var invItem = slot.currentItem.GetComponent<Item>();
                if (invItem != null && invItem.ID == itemPrefab.GetComponent<Item>().ID)
                {
                    hasItem = true;
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                    break;
                }
            }

            if (!hasItem)
                inventoryController.AddItem(itemPrefab);
        }

        DialogueManager.Instance.RequestNewDialogue(this);

        if (currentActiveDialogue != null && currentActiveDialogue != this)
        {
            currentActiveDialogue.CancelDialogue();
        }

        currentActiveDialogue = this;

        StartNPCDialog();
    }

    void StartNPCDialog()
    {
        if (manyInteractions)
            GetDialoguesSequence(progress.GetProgress());

        npcIsDialogueActive = true;
        npcDialogueIndex = 0;
        currentDialogueDataIndex = 0;

        CreateDialoguePanel();

        var currentDialogue = npcDialogueSequence[currentDialogueDataIndex];

        npcNameText.SetText(currentDialogue.npcName);
        npcPortraitImage.sprite = currentDialogue.npcPortrait;

        npcDialoguePanelInstance.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    void CreateDialoguePanel()
    {
        if (npcDialoguePanelInstance != null) return;

        var canvas = GameObject.FindWithTag("UICanvas");
        if (canvas == null)
        {
            Debug.LogError("🚫 Canvas não encontrado!", this);
            return;
        }

        npcDialoguePanelInstance = Instantiate(npcDialoguePanelPrefab, canvas.transform, false);

        npcDialogueText = npcDialoguePanelInstance.transform.Find("Dialogue Text")?.GetComponent<TMP_Text>();
        npcNameText = npcDialoguePanelInstance.transform.Find("Name Text")?.GetComponent<TMP_Text>();
        npcPortraitImage = npcDialoguePanelInstance.transform.Find("Image")?.GetComponent<Image>();
        npcCloseButton = npcDialoguePanelInstance.transform.Find("CloseButton")?.GetComponent<Button>();

        if (npcCloseButton != null)
            npcCloseButton.onClick.AddListener(EndDialogue);
    }

    void DestroyDialoguePanel()
    {
        if (npcDialoguePanelInstance != null)
        {
            Destroy(npcDialoguePanelInstance);
            npcDialoguePanelInstance = null;
            npcDialogueText = null;
            npcNameText = null;
            npcPortraitImage = null;
            npcCloseButton = null;
        }
    }

    void ShowCurrentBlock()
    {
        var data = npcDialogueSequence[currentDialogueDataIndex];
        npcDialogueIndex = 0;
        npcNameText.text = data.npcName;
        npcPortraitImage.sprite = data.npcPortrait;
        npcDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    void GetDialoguesSequence(int maxIndex)
    {
        var list = new List<NpcInteractionDialogue>();
        string npcName = gameObject.name;

        for (int i = maxIndex; i >= 0; i--)
        {
            var entries = Resources.LoadAll<NpcInteractionDialogue>($"Characters/{npcName}/{i} Interacao");
            if (entries.Length > 0)
            {
                list.AddRange(entries);
                lastInteraction = i;
                break;
            }
        }

        if (moonTalks)
        {
            for (int i = maxIndex; i >= 0; i--)
            {
                var entries = Resources.LoadAll<NpcInteractionDialogue>($"Characters/Moon/Moon Interacao/{npcName}/{i} Interacao");
                if (entries.Length > 0)
                {
                    list.AddRange(entries);
                    break;
                }
            }
        }

        list.Sort((a, b) =>
        {
            int ExtractNumber(string name)
            {
                var match = System.Text.RegularExpressions.Regex.Match(name, @"^\d+");
                return match.Success ? int.Parse(match.Value) : int.MaxValue;
            }
            return ExtractNumber(a.name).CompareTo(ExtractNumber(b.name));
        });

        foreach (var i in list)
        {
            if (i.adicionarProgresso)
            {
                adicionarProgresso = true;
                break;
            }
        }

        npcDialogueSequence = list.ToArray();
    }

    void NextLine()
    {
        var currentDialogue = npcDialogueSequence[currentDialogueDataIndex];

        if (npcIsTyping)
        {
            StopAllCoroutines();
            npcDialogueText.SetText(currentDialogue.dialogueLines[npcDialogueIndex]);
            npcIsTyping = false;
            return;
        }

        npcDialogueIndex++;

        if (npcDialogueIndex < currentDialogue.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            currentDialogueDataIndex++;
            if (currentDialogueDataIndex < npcDialogueSequence.Length)
            {
                ShowCurrentBlock();
            }
            else
            {
                EndDialogue();
            }
        }
    }

    IEnumerator TypeLine()
    {
        npcIsTyping = true;
        npcDialogueText.text = "";

        var currentDialogue = npcDialogueSequence[currentDialogueDataIndex];

        foreach (var ch in currentDialogue.dialogueLines[npcDialogueIndex])
        {
            npcDialogueText.text += ch;
            yield return new WaitForSecondsRealtime(currentDialogue.typingSpeed);
        }

        npcIsTyping = false;

        if (currentDialogue.autoProgressLines.Length > npcDialogueIndex &&
            currentDialogue.autoProgressLines[npcDialogueIndex])
        {
            yield return new WaitForSecondsRealtime(currentDialogue.autoProgressDelay);
            NextLine();
        }
    }

    public void CancelDialogue()
    {
        if (!npcIsDialogueActive) return;
        EndDialogue();
    }

    void HandlePause()
    {
        if (!npcIsDialogueActive || npcDialoguePanelInstance == null) return;
        StopAllCoroutines();
        npcDialoguePanelInstance.SetActive(false);
    }

    void HandleResume()
    {
        if (!npcIsDialogueActive || npcDialoguePanelInstance == null) return;
        npcDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        npcIsDialogueActive = false;
        PauseController.SetPause(false);

        DestroyDialoguePanel();

        if (currentActiveDialogue == this)
            currentActiveDialogue = null;

        OnDialogueEnded?.Invoke(this);

        if (adicionarProgresso && progress != null && lastInteraction == progress.GetProgress())
        {
            progress.AddProgress();
        }
    }
}