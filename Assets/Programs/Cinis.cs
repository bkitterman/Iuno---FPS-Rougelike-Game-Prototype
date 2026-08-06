using UnityEngine;

public class Cinis : MonoBehaviour, ProgramInstance
{
    public ProgramData Data { get; set; }
    public int ActiveStacks { get; set; }
    public int MaxStacks { get; set; }
    public bool IsActive { get; set; }
    public bool IsOptimized { get; set; }
    public StatsController _statsController { get; set; }

    /// <summary>
    /// Subscribe to events, apply any buffs/debuffs, and do any other neccesities.
    /// 
    /// This is for adding program to memory.
    /// </summary>
    public void OnActivate(int count = 1)
    {
        IsActive = true;
        ActiveStacks = count < MaxStacks ? count : MaxStacks;
        var threshold = new StatModifier(.5f, StatModType.Flat, this);
        _statsController.MagazineDamageBonusThreshold.AddModifier(threshold);
        Apply(ActiveStacks);
    }

    /// <summary>
    /// Unsubscribe from events, remove any buffs/debuffs, and do other cleanup as needed.
    /// 
    /// This is for removing program from memory.
    /// </summary>
    public void OnDeactivate()
    {
        _statsController.MagazineDamageBonusMultiplier.RemoveAllModifiersFromSource(this);
        _statsController.MagazineDamageBonusThreshold.RemoveAllModifiersFromSource(this);
        ActiveStacks = 0;
        IsActive = false;
    }

    /// <summary>
    /// Add item to player inventory, and any other necesities.
    /// </summary>
    public void OnPickup()
    {
        // Get Stat Controller
        _statsController = GetComponentInParent<StatsController>();
        MaxStacks++;
    }

    /// <summary>
    /// For when a routine is picked up
    /// </summary>
    public void OnRoutinePickup()
    {
        MaxStacks++;
        ActiveStacks++;
        Apply();
    }

    /// <summary>
    /// Remove item from player inventory and any cleanup regarding.
    /// </summary>
    public void OnRemoval()
    {
        // Remove from memory
    }

    /// <summary>
    /// Method for apply any effects. Kept seperate as this shoul dbe called
    /// as routines/stacks of this program/item increase or decrease.
    /// 
    /// This should only act when program is active.
    /// </summary>
    public void Apply(int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            var damageModifier = new StatModifier(Data.Multiplier, Data.RoutineScaling, this);
            _statsController.MagazineDamageBonusMultiplier.AddModifier(damageModifier);
        }
    }

    /// <summary>
    /// Called when the program optimizes in memory to ensure its up to date.
    /// </summary>
    public void Optimize() { }

    /// <summary>
    /// Return the buffs multiplier value.
    /// </summary>
    public float GetProgramMultiplier()
    {
        return Data.Multiplier * ActiveStacks;
    }

    public string toString() { return Data.ProgramName; }
}
