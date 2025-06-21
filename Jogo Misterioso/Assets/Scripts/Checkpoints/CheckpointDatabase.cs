// Assets/Scripts/Checkpoints/CheckpointDatabase.cs
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Checkpoint DB", fileName = "CheckpointDB")]
public class CheckpointDatabase : ScriptableObject
{
  public Checkpoint[] checkpoints;
}
