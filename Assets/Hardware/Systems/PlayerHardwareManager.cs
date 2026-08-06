using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Player))] // Or StatsController, wherever stats live
public class PlayerHardwareManager : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private StatsController statsController;
    [SerializeField] private AbilityManager abilityManager;

    [Header("Default Hardware")]
    [SerializeField] private PSUData defaultPSU;
    [SerializeField] private MemoryData defaultMemory;
    [SerializeField] private StorageData defaultStorage;
    [SerializeField] private List<CoreData> defaultCores;

    [Header("Equipped Hardware")]
    public PSUData EquippedPSU { get; private set; }
    public MemoryData EquippedMemory { get; private set; }
    public StorageData EquippedStorage { get; private set; }
    public List<CoreData> EquippedCores { get; private set; } = new List<CoreData>();

    private List<HardwareData> poweredDownComponents = new List<HardwareData>();

    void Awake()
    {
        if (statsController == null)
            statsController = GetComponent<StatsController>();

        InitializeDefaultHardware();
    }

    void Start()
    {
        // Run a full recalculation of all stats
        RecalculateAllHardwareStats();
    }

    /// <sSummary>
    /// Equips the default hardware specified in the inspector and runs a stat check.
    /// </summary>
    private void InitializeDefaultHardware()
    {
        EquippedPSU = defaultPSU;
        EquippedMemory = defaultMemory;
        EquippedStorage = defaultStorage;
        EquippedCores.AddRange(defaultCores);

    }

    /// <sSummary>
    /// This is the public method your "Hardware" UI will call.
    /// </summary>
    public void SwapHardware(HardwareData newHardware, int index = 0)
    {
        if (newHardware == null) return;

        // Use 'is' pattern matching to check the type and equip it
        if (newHardware is PSUData newPSU)
        {
            EquippedPSU = newPSU;
        }
        else if (newHardware is MemoryData newMemory)
        {
            EquippedMemory = newMemory;
        }
        else if (newHardware is StorageData newStorage)
        {
            EquippedStorage = newStorage;
        }
        else if (newHardware is CoreData newCore)
        {
            EquippedCores[index] = newCore;
        }

        // After any change, recalculate everything
        RecalculateAllHardwareStats();

        GameEvents.ReportHardwareEquipped(newHardware);
    }

    public float GetPowerUtilization(bool percent = true)
    {
        // 1. Check Power
        float totalPowerOutput = EquippedPSU?.PowerOutput_W ?? 0f;
        float totalPowerDraw = 0f;

        // Calculate draw for all components
        if (EquippedMemory != null) totalPowerDraw += EquippedMemory.PowerDraw_W;
        if (EquippedStorage != null) totalPowerDraw += EquippedStorage.PowerDraw_W;
        foreach (var core in EquippedCores)
        {
            if (core != null) totalPowerDraw += core.PowerDraw_W;
        }

        foreach (var ability in abilityManager.ActiveAbilities)
        {
            if (ability != null) totalPowerDraw += ability.Data.PowerDraw_W;
        }

        if(percent)
            return (totalPowerDraw / EquippedPSU.PowerOutput_W);
        return totalPowerDraw;

        // ... (Add power draw for equipped abilities, weapon later) ...


        // TODO: Add logic to shut down components if totalPowerDraw > totalPowerOutput
        // For now, we assume it's all powered.

        // 1. Power down abilities
        // 2. Start with powering down expensive cores
        // 3. If power falls below memory/storage, start to kill player.

    }

    private void RecalculateAllHardwareStats()
    {
        poweredDownComponents.Clear();

        // 2. Apply Memory & Storage
        statsController.MaxMemory.BaseValue = EquippedMemory?.MemoryCapacity_GB ?? 0f;
        statsController.MaxStorage.BaseValue = EquippedStorage?.StorageCapacity_GB ?? 0f;

        // 3. Apply Core Bonuses
        // Remove old cores first, must be done still
        statsController.RemoveAllModifiersFromSourceType<CoreData>();

        // Reinit all
        foreach (var core in EquippedCores)
        {
            if (core == null) continue;

            foreach (var modifier in core.StatsPassiveBonuses)
            {
                // Find the stat on the controller and add the modifier
                PlayerStat stat = statsController.GetStatModifierByType(modifier.TargetStat); 
                if (stat != null)
                {
                    // Add the modifier, using the CoreData SO as the "source"
                    stat.AddModifier(new StatModifier(modifier.Value, modifier.Type, core));
                }
            }
        }
    }
}