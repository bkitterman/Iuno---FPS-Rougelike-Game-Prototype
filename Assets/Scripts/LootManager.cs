using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance;

    [SerializeField] private ProgramDatabase programDatabase;
    [SerializeField] private GameObject programPickupPrefab;
    [Range(0, 1)] [SerializeField] private float optimizedChance = 0.02f; // 2% chance for a drop to be Optimized
    private Transform playerTransform; // For checking distance/suites

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Find the player
        playerTransform = FindAnyObjectByType<Player>().transform;
    }

    /// <summary>
    /// This is called by an enemy's health script on death.
    /// </summary>
    public void ProcessLootDrop(Vector3 position, LootTableData table)
    {
        if (table == null) return;

        // 1. Roll for overall drop chance
        if (Random.value > table.overallDropChance)
        {
            return; 
        }

        // 2. Roll for Rarity
        RarityData chosenRarity = ChooseRarity(table.rarityTable);

        // 3. Roll for the specific Program
        ProgramData programToDrop = ChooseProgram(chosenRarity, table);

        if (programToDrop == null) return; // No items of that rarity found

        // 4. Spawn the item
        SpawnLoot(position, programToDrop);
    }

    private RarityData ChooseRarity(List<RarityDropChance> table)
    {
        int totalWeight = table.Sum(entry => entry.weight);
        int roll = Random.Range(0, totalWeight);

        foreach (var entry in table)
        {
            if (roll < entry.weight)
            {
                return entry.rarity;
            }
            roll -= entry.weight;
        }
        return table.First().rarity; // Fallback
    }

    private ProgramData ChooseProgram(RarityData rarity, LootTableData tableRules)
    {
        // 1. Get all programs of the chosen rarity
        List<ProgramData> potentialPrograms = programDatabase.ProgramList
            .Where(p => p.Rarity == rarity) 
            .ToList();

        if (potentialPrograms.Count == 0)
        {
            return null;
        }

        // 2. Get player's current suite counts
        if (playerTransform == null) playerTransform = FindAnyObjectByType<Player>().transform;
        Player player = playerTransform.GetComponent<Player>();
        var suiteCounts = player.GetActiveSuiteCounts(); 
         
        // 3. Build a new weighted list for the final roll
        Dictionary<ProgramData, float> tunedProgramWeights = new Dictionary<ProgramData, float>();

        foreach (var program in potentialPrograms)
        {
            float weight = 10f; // Base weight for all items

            // Check for the "suite tuning" bonus
            if (suiteCounts.ContainsKey(program.ApplicationSuite) &&
                suiteCounts[program.ApplicationSuite] >= tableRules.suiteBonusThreshold &&
                !player.HasCollectedProgram(program.ProgramName)) // Check if uncollected
            {
                weight *= tableRules.suiteBonusMultiplier; // Apply the "slight boost"
            }

            tunedProgramWeights.Add(program, weight);
        }

        // 4. Perform the final weighted roll
        float totalWeight = tunedProgramWeights.Sum(p => p.Value);
        float roll = Random.Range(0, totalWeight);

        foreach (var pair in tunedProgramWeights)
        {
            if (roll < pair.Value)
            {
                return pair.Key;
            }
            roll -= pair.Value;
        }
        return potentialPrograms.First();
    }

    private void SpawnLoot(Vector3 position, ProgramData data)
    {
        GameObject lootGO = Instantiate(programPickupPrefab, position, Quaternion.identity);

        ProgramPickup pickupScript = lootGO.GetComponent<ProgramPickup>();
        if (pickupScript != null)
        {
            pickupScript.Initialize(data);
        }

        Player player = playerTransform.GetComponent<Player>();
        bool canBeOptimized = data.canBeOptimized && (Random.value < optimizedChance || player.CheckIfProgramOptimized(data));
        pickupScript.isOptimized = canBeOptimized;

        //TODO visual flair
    }
}