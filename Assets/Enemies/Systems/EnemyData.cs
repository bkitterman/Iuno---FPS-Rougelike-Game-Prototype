using UnityEngine;
using System.Collections.Generic; // For Lists

// Base class for all enemy types
public abstract class EnemyData : ScriptableObject
{
    [Header("Identification & Visuals")]
    public string enemyName = "New Enemy";
    public GameObject modelPrefab;
    public Sprite icon;

    [Header("Core Stats (Base Values)")]
    public float maxHealth = 100f;
    public float moveSpeed = 5f;
    public float range = 2f;

    [Header("Targeting & Perception")]
    public float detectionRadius = 30f;
    public float lineOfSightRange = 40f;
    public LayerMask targetMask;
    public LayerMask obstacleMask;

    [Header("AI & Behavior")]
    public string aiClassName = "BasicEnemyAI";

    [Header("Combat Capabilities")]
    public List<WeaponData> availableWeapons; // List of weapons this enemy *can* use
    public List<AbilityData> availableAbilities; // List of abilities this enemy *can* use

    [Header("Patrol")]
    public bool CanPatrol = true;
    public float PatrolTime = 5f;
    public float PatrolRadius = 15f;
    public float SearchTime = 5f;

    [Header("Spawning")]
    public int spawnCost = 10;

    [Header("Loot Table")]
    public LootTableData LootTable;
    [Range(0f, float.PositiveInfinity)] public float AmmoDropMultiplier = 1f; // Multiplier for the amount of ammo dropped

}