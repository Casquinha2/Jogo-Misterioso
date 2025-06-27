using UnityEditor;
using UnityEngine;

// Aponta este editor para o seu MonoBehaviour Progress
[CustomEditor(typeof(Progress))]
public class ProgressEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // Desenha os campos normais
        DrawDefaultInspector();

        EditorGUILayout.Space();

        // Pega a instância alvo
        Progress prog = (Progress)target;

        // Botão que chama AddProgress
        if (GUILayout.Button("▶️ Add Progress"))
        {
            // Marca a cena como "suja" para permitir Undo/Redo
            Undo.RecordObject(prog, "Add Progress");
            prog.AddProgress();

            // Se for alterar transform, marca a cena
            EditorUtility.SetDirty(prog);
        }

        EditorGUILayout.Space();

        // Mostra o progress num Label só de leitura
        EditorGUILayout.LabelField("Progress atual", prog.GetProgress().ToString());
    }
}

