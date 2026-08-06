using UnityEngine;

public enum StatModType
{
    Flat,       
    PercentAdd, 
    PercentMult
}

public class StatModifier
{
    public readonly Stat TargetStat;
    public readonly float Value;
    public readonly StatModType Type;
    public readonly object Source;

    public StatModifier(float value, StatModType type, object source)
    {
        Value = value;
        Type = type;
        Source = source;
    }
}

[System.Serializable]
public class StatModifierData
{
    [Header("Stat Modifier Properties")]
    public Stat TargetStat;
    public float Value;
    public StatModType Type;
}