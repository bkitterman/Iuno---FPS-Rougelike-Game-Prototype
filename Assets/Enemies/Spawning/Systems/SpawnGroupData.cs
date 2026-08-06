using UnityEngine;

[CreateAssetMenu(fileName = "Spawn Group", menuName = "Spawning/Spawn Group")]
public class SpawnGroupData : ScriptableObject
{
    [Tooltip("The type of enemy to spawn")]
    public EnemyData enemyToSpawn;
    [Tooltip("The number of enemies to spawn")]
    public int count = 1;
    [Tooltip("Time before next wave starts")]
    public float delayAfter = 0.5f; 
}