using System.Collections.Generic;
using System.Collections.ObjectModel;

public class PlayerStat
{
    public float BaseValue;
    public string BaseValueSource;
    private readonly List<StatModifier> _statModifiers;
    public readonly ReadOnlyCollection<StatModifier> StatModifiers;

    public PlayerStat(float baseValue, string source = "UNKNOWN")
    {
        BaseValue = baseValue;
        BaseValueSource = source;
        _statModifiers = new List<StatModifier>();
        StatModifiers = _statModifiers.AsReadOnly();
    }

    public void AddModifier(StatModifier mod)
    {
        _statModifiers.Add(mod);
    }

    public bool RemoveModifier(StatModifier mod)
    {
        return _statModifiers.Remove(mod);
    }

    public bool RemoveAllModifiersFromSource(object source)
    {
        // Remove any modifiers that came from a specific source (like one Program)
        int numRemovals = _statModifiers.RemoveAll(mod => mod.Source == source);
        return numRemovals > 0;
    }

    public bool RemoveAllModifiersFromType<T>() where T : class
    {
        int numRemovals = _statModifiers.RemoveAll(mod => mod.Source is T);
        return numRemovals > 0;
    }

    public float GetPrePercentAddValue()
    {
        float finalValue = BaseValue;

        // Apply all Flat modifiers first.
        for (int i = 0; i < _statModifiers.Count; i++)
        {
            if (_statModifiers[i].Type == StatModType.Flat)
            {
                finalValue += _statModifiers[i].Value;
            }
        }
        return finalValue;
    }

    public float GetPrePercentMultValue()
    {
        float finalValue = BaseValue;
        float sumPercentAdd = 0;

        // 1. Apply all Flat modifiers first.
        for (int i = 0; i < _statModifiers.Count; i++)
        {
            if (_statModifiers[i].Type == StatModType.Flat)
            {
                finalValue += _statModifiers[i].Value;
            }
            // 2. Sum up all PercentAdd modifiers.
            else if (_statModifiers[i].Type == StatModType.PercentAdd)
            {
                sumPercentAdd += _statModifiers[i].Value;
            }
        }

        // 3. Apply the sum of PercentAdd modifiers.
        finalValue *= 1 + sumPercentAdd;

        return finalValue;
    }

    public float GetValue(float testValue = float.NaN)
    {
        float finalValue = (float.IsNaN(testValue) ? BaseValue : testValue);
        float sumPercentAdd = 0;

        // 1. Apply all Flat modifiers first.
        for (int i = 0; i < _statModifiers.Count; i++)
        {
            if (_statModifiers[i].Type == StatModType.Flat)
            {
                finalValue += _statModifiers[i].Value;
            }
            // 2. Sum up all PercentAdd modifiers.
            else if (_statModifiers[i].Type == StatModType.PercentAdd)
            {
                sumPercentAdd += _statModifiers[i].Value;
            }
        }

        // 3. Apply the sum of PercentAdd modifiers.
        finalValue *= 1 + sumPercentAdd;

        // 4. Apply all PercentMult modifiers.
        for (int i = 0; i < _statModifiers.Count; i++)
        {
            if (_statModifiers[i].Type == StatModType.PercentMult)
            {
                finalValue *= 1 + _statModifiers[i].Value;
            }
        }

        return finalValue;
    }
}