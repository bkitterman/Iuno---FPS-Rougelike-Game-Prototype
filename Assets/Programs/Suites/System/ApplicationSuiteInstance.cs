using UnityEngine;

public interface ApplicationSuiteInstance
{
    public ApplicationSuiteData Data { get; set; }
    public bool IsActive { get; set; }

    /// <summary>
    /// Subscribe to events, apply any buffs/debuffs, and do any other neccesities.
    /// 
    /// This is for adding program to memory.
    /// </summary>
    void OnActivate(float value, int tier);

    /// <summary>
    /// Unsubscribe from events, remove any buffs/debuffs, and do other cleanup as needed.
    /// 
    /// This is for removing program from memory.
    /// </summary>
    void OnDeactivate();

    /// <summary>
    /// Return the buffs multiplier value.
    /// </summary>
    float GetSuiteMultiplier();

    string toString();
}
