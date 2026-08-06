using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Wave", menuName = "Spawning/Wave")]
public class WaveData : ScriptableObject
{
    [Tooltip("The sequence of enemies to spawn")]
    public List<SpawnGroupData> spawnGroups;
    [Tooltip("Delay before first group spawns")]
    public float initialDelay = 1.0f;
    [Tooltip("Time after this wave finishes")]
    public float timeBetweenWaves = 5.0f;

    // Later: Conditionals?
}