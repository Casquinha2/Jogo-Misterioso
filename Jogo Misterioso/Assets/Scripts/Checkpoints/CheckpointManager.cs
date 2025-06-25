#if UNITY_EDITOR
using System.Collections;
using System.Linq;
using UnityEngine;
using Unity.Cinemachine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager I { get; private set; }

    [SerializeField] CheckpointDatabase database;
    [SerializeField] Transform mapBoundsRoot;
    [SerializeField] CinemachineCamera vcam;

    void Awake()
    {
        if (I == null)
        {
            I = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void LoadCheckpoint(string id)
    {
        var cp = database.checkpoints.FirstOrDefault(x => x.id == id);
        if (cp == null) { Debug.LogError($"Checkpoint '{id}' não existe."); return; }
        StartCoroutine(DoWarp(cp));
    }

    IEnumerator DoWarp(Checkpoint cp)
    {
        // 1) espera readiness
        float t = 0;
        while (!GameSceneManager.isReady && t < 2f) { t += Time.deltaTime; yield return null; }
        if (!GameSceneManager.isReady) yield break;
        yield return null; yield return null;

        var conf = vcam.GetComponent<CinemachineConfiner2D>();
        if (conf == null) { Debug.LogError("Confiner2D não encontrado"); yield break; }

        conf.gameObject.SetActive(false);
        


        // 2) teleporta o PLAYER
        var player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) { Debug.LogError("Player não encontrado"); yield break; }

        Vector3 oldP = player.position;
        player.position = cp.SpawnPos;
       
        var sceneKey = cp.sceneName.Replace("Scene", "");
        string floor = sceneKey.StartsWith("Piso") && int.TryParse(sceneKey.Substring(4), out var idx)
            ? $"Piso {idx}"
            : sceneKey;

        // 1) calcula a posição alvo da câmara
        var mainCam = Camera.main.transform;
        float camZ    = mainCam.position.z;                       // normalmente -10
        Vector3 camP  = new Vector3(cp.SpawnPos.x,
                                    cp.SpawnPos.y,
                                    camZ);

        // 2) força a câmara para lá usando o Brain
        vcam.ForceCameraPosition(camP, Quaternion.identity);





        yield return null; // dá um frame pro CineCam

        // 7) atualiza o Confiner2D
        
        


        var floorTF = mapBoundsRoot
            .GetComponentsInChildren<Transform>(true)
            .FirstOrDefault(t => t.name == floor);
        if (floorTF == null) { Debug.LogError($"Piso '{floor}' não existe"); yield break; }

        // 9) ativa só o collider do checkpoint
        var polys = floorTF.GetComponentsInChildren<PolygonCollider2D>(true);

        var targetPoly = polys.FirstOrDefault(p => p.name == cp.displayName);
        if (targetPoly == null) { Debug.LogError($"Poly '{cp.displayName}' não achada"); yield break; }

        yield return null;

        conf.gameObject.SetActive(true);

        // 10) aplica shape e invalida cache
        conf.BoundingShape2D = targetPoly;
        conf.InvalidateBoundingShapeCache();

        yield return null;
    }
}
#endif
