// Assets/Editor/CheckpointBootstrapperEditor.cs
using System.Linq;        // ToList(), Select()
using UnityEditor;
using UnityEngine;

public class CheckpointBootstrapperEditor : EditorWindow
{
    [MenuItem("Debug/Choose Start Checkpoint")]
    static void Open() => GetWindow<CheckpointBootstrapperEditor>("Start Checkpoint");

    CheckpointDatabase db;
    int selectedIndex;
    string[] ids;
    string[] displayNames;

    void OnEnable()
    {
        // Carrega o asset do DB
        db = AssetDatabase.LoadAssetAtPath<CheckpointDatabase>(
               "Assets/Checkpoints/CheckpointDB.asset");
        if (db == null) return;

        // Prepara arrays de IDs e nomes para o popup
        ids          = db.checkpoints.Select(c => c.id).ToArray();
        displayNames = db.checkpoints.Select(c => c.displayName).ToArray();

        // Determina o índice a partir do último checkpoint salvo
        var last = PlayerPrefs.GetString("LastCheckpoint", ids[0]);
        selectedIndex = System.Array.IndexOf(ids, last);
        if (selectedIndex < 0) selectedIndex = 0;
    }

    void OnGUI()
    {
        EditorGUILayout.LabelField("Checkpoint de Início (Play Mode)", EditorStyles.boldLabel);

        if (db == null)
        {
            EditorGUILayout.HelpBox("Não encontrei CheckpointDB em Assets/Checkpoints!", MessageType.Error);
            return;
        }

        // Dropdown mostrando o displayName, mas guardando o índice
        selectedIndex = EditorGUILayout.Popup(
            new GUIContent("Checkpoint"),
            selectedIndex,
            displayNames
        );

        GUILayout.Space(10);
        if (GUILayout.Button("Salvar e Play"))
        {
            // Grava o id (único) do checkpoint escolhido
            PlayerPrefs.SetString("LastCheckpoint", ids[selectedIndex]);
            PlayerPrefs.Save();


            

            // Entra no Play Mode
            EditorApplication.isPlaying = true;
        }
    }
}