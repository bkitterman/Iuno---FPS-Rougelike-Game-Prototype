using UnityEngine;

public abstract class StatusEffectData : ScriptableObject
{
    [Header("Base Properties")]
    public float Duration;
    public StackingType StackingBehavior;
    public Sprite Icon;
    public string Name;
    [TextArea] public string Description = "No description provided.";
    public int Priority = 0;
    public bool IsBuff = true;

    // This method will create the runtime instance of the effect
    public abstract StatusEffect CreateEffectInstance();
}