using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MemoryDatabase", menuName = "Database/Hardware/Memory")]
public class MemoryDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<MemoryData> MemoryList;
}