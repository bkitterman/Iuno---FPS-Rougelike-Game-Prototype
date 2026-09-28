using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Memory", menuName = "Hardware/Memory")]
public class MemoryData : HardwareData
{
    [Header("Memory Specifics")]
    public float MemoryCapacity_GB = 32f;
}

