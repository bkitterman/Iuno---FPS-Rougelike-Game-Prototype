using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "StorageDatabase", menuName = "Database/Hardware/Storage")]
public class StorageDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<StorageData> StorageList;
}