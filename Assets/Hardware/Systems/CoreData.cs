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

[CreateAssetMenu(fileName = "CoreDatabase", menuName = "Database/Hardware/Core")]
public class CoreDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<CoreData> CoreList;
}
