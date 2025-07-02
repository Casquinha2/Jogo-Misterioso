using UnityEngine;
using UnityEngine.UIElements;

public class LivrosPuzzle : MonoBehaviour
{
    [SerializeField] private ObjectInteractionDialogue[] sequenceFirst;
    [SerializeField] private ObjectInteractionDialogue[] sequenceSecond;
    [SerializeField] private ObjectInteractionDialogue[] sequenceTirth;
    [SerializeField] private ObjectInteractionDialogue[] sequenceFourth;
    [SerializeField] private ObjectInteractionDialogue[] sequenceFifth;
    [SerializeField] private ObjDialogue dialogueScript;

    ObjectInteractionDialogue[] current; // array selecionada
    int currentIndex;                    // posição dentro do array
    bool isRunning;
    private bool adicionarprogress;

    private int sequenceNum;
    private bool verde, vermelho, azul, amarelo, roxo;

    void Start()
    {
        Debug.Log("Comecou");
        sequenceNum = 0;
        verde = vermelho = azul = amarelo = roxo = false;
    }

    public void VerdeClick()
    {
        Debug.Log($"Verde click: {verde}");
        
        if (!verde)
        {
            verde = true;
            Click();
        }
    }
    public void VermelhoClick()
    {
        if (!vermelho)
        {
            vermelho = true;
            Click();
        }
    }
    public void AzulClick()
    {
        if (!azul)
        {
            azul = true;
            Click();
        }
    }
    public void AmareloClick()
    {
        if (!amarelo)
        {
            amarelo = true;
            Click();
        }
    }
    public void RoxoClick()
    {
        if (!roxo)
        {
            roxo = true;
            Click();
        }
    }

    void OnEnable()
    {
        ObjDialogue.OnDialogueEnded += HandleDialogueEnded;
    }
    void OnDisable()
    {
        ObjDialogue.OnDialogueEnded -= HandleDialogueEnded;
    }

    void Click()
    {
        sequenceNum++;

        switch (sequenceNum)
        {
            case 1: current = sequenceFirst; adicionarprogress = false; break;
            case 2: current = sequenceSecond; adicionarprogress = false; break;
            case 3: current = sequenceTirth; adicionarprogress = false; break;
            case 4: current = sequenceFourth; adicionarprogress = false; break;
            case 5: current = sequenceFifth; adicionarprogress = true; break;
        }

        if (current == null || current.Length == 0) return;

        currentIndex = 0;
        isRunning = true;
        StartCurrentBlock();
    }
    
    void StartCurrentBlock()
    {
        // injeta o dialogueData no ObjDialogue
        dialogueScript.objDialogueData = current[currentIndex];
        dialogueScript.adicionarProgresso = adicionarprogress;
        dialogueScript.Interact();
    }

    void HandleDialogueEnded(ObjDialogue who)
    {
        // garante que é o nosso dialogueScript que acabou
        if (!isRunning || who != dialogueScript) 
            return;

        currentIndex++;
        if (currentIndex < current.Length)
        {
            StartCurrentBlock();
        }
        else
        {
            isRunning = false;
            // opcional: disparar um evento próprio para sinalizar fim da sequência
            // e.g. OnSequenceEnded?.Invoke(currentSeqId);
        }
    }
}
