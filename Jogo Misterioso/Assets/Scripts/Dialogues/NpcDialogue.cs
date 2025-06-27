using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using System;
using UnityEngine.Analytics;
using System.Collections.Generic;
using Unity.VisualScripting;
public class NpcDialogue : MonoBehaviour, IInteractable, ICancelableDialogue
{
    [Header("Prefab UI (Panel + TMP_Text)")]
    public GameObject npcDialoguePanelPrefab;

    [Header("Dados de Diálogo")]
    public NpcInteractionDialogue[] npcDialogueSequence;

    [Header("Item (opcional)")]
    [Tooltip("Se definido, este item será dado ao jogador")]
    public GameObject itemPrefab;

    [Header("Mais opcoes")]
    public bool manyInteractions = false;

    public bool moonTalks = true;

    
    private bool adicionar;

    // — campos internos —
    private GameObject inventoryPanel;
    private InventoryController inventoryController;

    private GameObject npcDialoguePanelInstance;
    private TMP_Text   npcDialogueText, npcNameText;
    private bool       panelInited;

    private int  npcDialogueIndex;
    private bool npcIsTyping, npcIsDialogueActive;
    private Image npcPortraitImage;
    private Button npcCloseButton;

    //rastreador do bloco de diálogo atual
    private int currentDialogueDataIndex = 0;

    public static event Action<NpcDialogue> OnDialogueEnded;

    public Progress progress;

    void Start()
    {
        // 1) Valida prefab de UI
        if (npcDialoguePanelPrefab == null)
        {
            Debug.LogError("🚫 Prefab de UI não atribuído em npcDialogue!", this);
            enabled = false;
            return;
        }

        // 2) Se houver um itemPrefab, resolve InventoryController e InventoryPanel
        if (itemPrefab != null)
        {
            // 2a) InventoryController
            inventoryController = FindFirstObjectByType<InventoryController>();
            if (inventoryController == null)
                Debug.LogError("❌ InventoryController não encontrado!", this);

            // 2b) InventoryPanel ativo
            inventoryPanel = GameObject.FindWithTag("InventoryPanel");

            // 2c) InventoryPanel inativo? procura entre todos os Transforms
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

        // 3) Instancia e configura o painel de diálogo
        var canvas = GameObject.FindWithTag("UICanvas");
        if (canvas == null)
        {
            Debug.LogError("🚫 Não encontrei nenhum Canvas na cena!", this);
            enabled = false;
            return;
        }

        npcDialoguePanelInstance = Instantiate(
            npcDialoguePanelPrefab,
            canvas.transform,
            worldPositionStays: false
        );

        // assume que dentro do prefab existem exactamente estes GameObject names
        var dialogueGO = npcDialoguePanelInstance.transform.Find("Dialogue Text");
        var nameGO     = npcDialoguePanelInstance.transform.Find("Name Text");
        var imageGO = npcDialoguePanelInstance.transform.Find("Image");
        var buttonGO = npcDialoguePanelInstance.transform.Find("CloseButton");

        if (dialogueGO != null)
            npcDialogueText = dialogueGO.GetComponent<TMP_Text>();
        if (nameGO != null)
            npcNameText = nameGO.GetComponent<TMP_Text>();
        if (imageGO != null)
            npcPortraitImage = imageGO.GetComponent<Image>();
        if (buttonGO != null)
            npcCloseButton = buttonGO.GetComponent<Button>();

        npcCloseButton.onClick.AddListener(EndDialogue);


        npcDialoguePanelInstance.SetActive(false);
        panelInited        = true;

        // 4) Subscreve eventos
        DialogueManager.Instance.OnNewDialogue   += CancelDialogue;
        DialogueManager.Instance.OnPauseDialogue += HandlePause;
        DialogueManager.Instance.OnResumeDialogue+= HandleResume;
    }

    void OnDestroy()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnNewDialogue   -= CancelDialogue;
            DialogueManager.Instance.OnPauseDialogue -= HandlePause;
            DialogueManager.Instance.OnResumeDialogue-= HandleResume;
        }
    }

    public bool CanInteract() => !npcIsDialogueActive;

    public void Interact()
    {
        if (npcDialogueSequence == null || (PauseController.IsGamePaused && !npcIsDialogueActive))
            return;

        if (npcIsDialogueActive) 
            return;

        // 1) Lógica de inventário (se itemPrefab definido)
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

        // 2) Cancela diálogos em curso
        DialogueManager.Instance.RequestNewDialogue(this);
        
        // 3) Inicia ou avança no diálogo
        if (npcIsDialogueActive) NextLine();
        else StartNPCDialog();
    }


    void StartNPCDialog()
    {
        if (manyInteractions)
            GetDialoguesSequence(progress.GetProgress());

        npcIsDialogueActive = true;
        npcDialogueIndex      = 0;
        currentDialogueDataIndex = 0;

        var currentDialogue = npcDialogueSequence[currentDialogueDataIndex];

        npcNameText.SetText(currentDialogue.npcName);
        npcPortraitImage.sprite = currentDialogue.npcPortrait;

        npcDialoguePanelInstance.SetActive(true);

        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    void ShowCurrentBlock()
    {
        var data = npcDialogueSequence[currentDialogueDataIndex];

        npcDialogueIndex   = 0;
        npcNameText.text   = data.npcName;
        npcPortraitImage.sprite = data.npcPortrait;
        npcDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }


    void GetDialoguesSequence(int maxIndex)
    {
        var list = new List<NpcInteractionDialogue>();

        string npcName = gameObject.name;

        // 1) Lógica para pasta do NPC
        for (int i = maxIndex; i >= 0; i--)
        {
            var entries = Resources.LoadAll<NpcInteractionDialogue>(
                $"Characters/{npcName}/{i} Interacao"
            );
            if (entries != null && entries.Length > 0)
            {
                list.AddRange(entries);
                break;  // achou o maior i, sai do loop
            }
        }

        if (moonTalks)
        {
            // 2) Lógica para pasta "Moon"
            for (int i = maxIndex; i >= 0; i--)
            {
                var entries = Resources.LoadAll<NpcInteractionDialogue>(
                    $"Characters/Moon/Moon Interacao/{npcName}/{i} Interacao"
                );
                if (entries != null && entries.Length > 0)
                {
                    list.AddRange(entries);
                    break;  // achou o maior i em Moon, sai
                }
            }
        }

        // 3) (Opcional) ordena por nome do asset ou outra regra
        list.Sort((a, b) => a.name.CompareTo(b.name));

        foreach (NpcInteractionDialogue i in list)
        {
            if (i.adicionarProgresso)
            {
                adicionar = true;
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
                ShowCurrentBlock();  // começa o próximo bloco
            }
            else
            {
                EndDialogue(); // terminou todos os blocos
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
            yield return new WaitForSeconds(currentDialogue.typingSpeed);
        }

        npcIsTyping = false;

        if (currentDialogue.autoProgressLines.Length > npcDialogueIndex &&
            currentDialogue.autoProgressLines[npcDialogueIndex])
        {
            yield return new WaitForSeconds(currentDialogue.autoProgressDelay);
            NextLine();
        }
    }


    public void CancelDialogue()
    {
        if (!panelInited || !npcIsDialogueActive) return;
        EndDialogue();
    }

    void HandlePause()
    {
        if (!npcIsDialogueActive) return;
        StopAllCoroutines();
        npcDialoguePanelInstance.SetActive(false);
    }

    void HandleResume()
    {
        if (!npcIsDialogueActive) return;
        npcDialoguePanelInstance.SetActive(true);
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        npcIsDialogueActive = false;
        npcDialoguePanelInstance.SetActive(false);
        OnDialogueEnded?.Invoke(this);
        PauseController.SetPause(false);

        //npcDialogueSequence = new NpcInteractionDialogue[0];

        if (adicionar && progress != null)
        {
            progress.AddProgress();
        }
            
    }
}