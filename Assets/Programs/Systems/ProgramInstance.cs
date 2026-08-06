using UnityEngine;

public interface ProgramInstance
{
    ProgramData Data { get; set; }
    int ActiveStacks { get; set; }
    int MaxStacks { get; set; }
    bool IsActive { get; set; }
    bool IsOptimized { get; set; }
    StatsController _statsController {  get; set; }

    /// <summary>
    /// Subscribe to events, apply any buffs/debuffs, and do any other neccesities.
    /// 
    /// This is for adding program to memory.
    /// </summary>
    void OnActivate(int count = 1);

    /// <summary>
    /// Unsubscribe from events, remove any buffs/debuffs, and do other cleanup as needed.
    /// 
    /// This is for removing program from memory.
    /// </summary>
    void OnDeactivate();

    /// <summary>
    /// Add item to player inventory, and any other necesities.
    /// </summary>
    void OnPickup();

    /// <summary>
    /// For when a routine is picked up
    /// </summary>
    void OnRoutinePickup();

    /// <summary>
    /// Remove item from player inventory and any cleanup regarding.
    /// </summary>
    void OnRemoval();

    /// <summary>
    /// Method for apply any effects. Kept seperate as this shoul dbe called
    /// as routines/stacks of this program/item increase or decrease.
    /// 
    /// This should only act when program is active.
    /// </summary>
    void Apply(int count = 0);

    /// <summary>
    /// Called when the program optimizes in memory to ensure its up to date.
    /// </summary>
    void Optimize();

    /// <summary>
    /// Return the buffs multiplier value.
    /// </summary>
    float GetProgramMultiplier();

    string toString();
}
