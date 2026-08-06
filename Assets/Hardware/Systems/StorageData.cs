using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Storage", menuName = "Hardware/Storage")]
public class StorageData : HardwareData
{
    [Header("Storage Specifics")]
    public float StorageCapacity_GB = 1024f;
}

[CreateAssetMenu(fileName = "StorageDatabase", menuName = "Database/Hardware/Storage")]
public class StorageDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<StorageData> StorageList;
}
