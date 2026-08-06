using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Memory", menuName = "Hardware/Memory")]
public class MemoryData : HardwareData
{
    [Header("Memory Specifics")]
    public float MemoryCapacity_GB = 32f;
}

[CreateAssetMenu(fileName = "MemoryDatabase", menuName = "Database/Hardware/Memory")]
public class MemoryDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<MemoryData> MemoryList;
}
