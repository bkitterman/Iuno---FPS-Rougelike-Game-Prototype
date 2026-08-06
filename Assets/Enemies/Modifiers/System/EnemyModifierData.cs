using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Modifier", menuName = "Enemy/Modifier")]
public class EnemyModifierData : ScriptableObject
{
    [Header("Info")]
    public string Name = "New Modifier"; // Must Match class name
    public string Description = "What this modifier does.";
    public string Version = "1.0.0";
    public int Id = 0;

    [Header("Visuals")]
    public Material EliteMaterial; 
    public GameObject VfxPrefab; 

    [Header("Data-Driven Stat Changes")]
    [Tooltip("Stat changes that are applied once when the modifier is added.")]
    public List<StatModifierData> StatModifiers; 
}