using UnityEngine;

[CreateAssetMenu(menuName = "Game/Checkpoint", fileName = "Checkpoint_")]
public class Checkpoint : ScriptableObject
{
  public string id;           
  public string displayName;  // ex: "Entrada Piso 1"
  public string sceneName;
  public Vector2 playerPosition;

  public Vector3 SpawnPos => new Vector3(playerPosition.x, playerPosition.y, 0f);
}


