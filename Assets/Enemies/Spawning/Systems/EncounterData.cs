using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Encounter", menuName = "Spawning/Encounter")]
public class EncounterData : ScriptableObject
{
    [Tooltip("Waves in the encounter")]
    public List<WaveData> waves; 
}