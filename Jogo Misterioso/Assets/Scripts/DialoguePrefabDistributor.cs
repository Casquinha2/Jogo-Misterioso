using UnityEngine;

// Este script deve ir no GameObject “Objetos do Jogo”
[ExecuteAlways]     // para funcionar também no Editor quando muda o valor
public class DialoguePrefabDistributor : MonoBehaviour
{
    [Tooltip("Arrasta aqui o prefab de painel de diálogo (Panel + TMP_Text)")]
    public GameObject dialoguePanelPrefab;

    void Awake()   => Distribute();
    void OnValidate()  => Distribute();  // dispara no Editor sempre que mudar algo

    private void Distribute()
    {
        if (dialoguePanelPrefab == null) return;

        // percorre todos os filhos diretos
        foreach (Transform child in transform)
        {
            // se tiver ObjDialogue, atribui
            var od = child.GetComponent<ObjDialogue>();
            if (od != null)
                od.objDialoguePanelPrefab = dialoguePanelPrefab;

            // se tiver PortaInteraction, atribui também
            var pi = child.GetComponent<PortaInteraction>();
            if (pi != null)
                pi.objDialoguePanelPrefab = dialoguePanelPrefab;
        }
    }
}

