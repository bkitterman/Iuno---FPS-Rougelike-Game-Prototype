using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Loot Table", menuName = "Loot/Loot Table")]
public class LootTableData : ScriptableObject
{
    [Header("Overall Drop Chance")]
    [Tooltip("The chance (0.0 to 1.0) that this enemy will drop *anything* at all.")]
    [Range(0, 1)]
    public float overallDropChance = 0.1f; // 10% chance to drop anything

    [Header("Rarity Weights")]
    [Tooltip("The weighted chances for each rarity. Higher numbers are more common.")]
    public List<RarityDropChance> rarityTable;

    [Header("Tuning")]
    [Tooltip("How many programs from a suite are needed to trigger the bonus.")]
    public int suiteBonusThreshold = 4;

    [Tooltip("A multiplier (e.g., 1.5 = 50% boost) for uncollected programs in a qualifying suite.")]
    public float suiteBonusMultiplier = 1.5f;
}

// This simple class doesn't need its own file
[System.Serializable]
public class RarityDropChance
{
    // You'll need a public enum for this
    // public enum ProgramRarity { Common, Uncommon, Rare, Legendary, Mythical }
    public RarityData rarity; 
    public int weight; // e.g., Common = 100, Uncommon = 30, Rare = 10, Legendary = 3
}