using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "PSUDatabase", menuName = "Database/Hardware/PSU")]
public class PSUDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<PSUData> PSUList;
}
