using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CoreDatabase", menuName = "Database/Hardware/Core")]
public class CoreDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<CoreData> CoreList;
}
