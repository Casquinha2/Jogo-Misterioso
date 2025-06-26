// Coloque este arquivo em Assets/Editor/CinemachineConfiner2DDebug.cs

using UnityEditor; 
using UnityEngine;
using Unity.Cinemachine;

// Executa assim que o Unity abrir
[InitializeOnLoad]
static class CinemachineConfiner2DDebug
{
    static CinemachineConfiner2DDebug()
    {
        // Hook na GUI do Scene View
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sv)
    {
        // Busca todos os Cin.Confiner2D, mesmo inativos
        var confin­ers = Object.FindObjectsByType<CinemachineConfiner2D>(
            FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (var conf in confin­ers)
        {
            // Só desenha se for um PolygonCollider2D
            var poly = conf.BoundingShape2D as PolygonCollider2D;
            if (poly == null)
                continue;

            // Puxa o primeiro Path (polígonos simples)
            var pts = poly.GetPath(0);

            // Desenha cada aresta no Scene View
            Handles.color = Color.green;
            for (int i = 0; i < pts.Length; i++)
            {
                Vector3 a = poly.transform.TransformPoint(pts[i]);
                Vector3 b = poly.transform.TransformPoint(
                    pts[(i + 1) % pts.Length]);
                Handles.DrawLine(a, b);
            }
        }
    }
}
