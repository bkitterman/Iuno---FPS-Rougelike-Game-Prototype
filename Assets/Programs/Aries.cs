using UnityEngine;

public class Aries : MonoBehaviour, ProgramInstance
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
        GameEvents.OnMeleeDamageDealt += OnDamageEvent;
    }

    /// <summary>
    /// Unsubscribe from events, remove any buffs/debuffs, and do other cleanup as needed.
    /// 
    /// This is for removing program from memory.
    /// </summary>
    public void OnDeactivate()
    {
        ActiveStacks = 0;
        GameEvents.OnMeleeDamageDealt -= OnDamageEvent;
        IsActive = false;
    }

    /// <summary>
    /// Add item to player inventory, and any other necesities.
    /// </summary>
    public void OnPickup()
    {
        // Get Stat Controller
        MaxStacks++;
    }

    /// <summary>
    /// For when a routine is picked up
    /// </summary>
    public void OnRoutinePickup()
    {
        MaxStacks++;
        ActiveStacks++;
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
        Debug.Log("Aires apply called erronuously");
    }

    /// <summary>
    /// Method performed when an event subscribed to occurs.
    /// 
    /// This should only act when program is active.
    /// </summary>
    public void OnDamageEvent(GameObject target, float damage, Vector3 direction)
    {
        Rigidbody rb = target.GetComponent<Rigidbody>();

        if(rb!= null)
        {
            rb.AddForce(direction * (Data.Multiplier * ActiveStacks), ForceMode.Impulse);
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
