using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Core", menuName = "Hardware/Core")]
public class CoreData : HardwareData
{
    [Header("Core Specifics")]
    [Tooltip("Passive stat boosts this core provides.")]
    public List<StatModifierData> StatsPassiveBonuses;
    public List<StatusEffectData> StatusEffectPassiveBonuses;
}

