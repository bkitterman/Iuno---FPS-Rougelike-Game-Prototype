using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "PSU", menuName = "Hardware/PSU")]
public class PSUData : HardwareData
{
    [Header("PSU Specifics")]
    [Tooltip("The total power this PSU can provide.")]
    public string Certification = "GOLD";
    public float PowerOutput_W = 750f;

    //TODO Create tardeoff data here
}

[CreateAssetMenu(fileName = "PSUDatabase", menuName = "Database/Hardware/PSU")]
public class PSUDatabase : ScriptableObject
{
    [Header("Database List")]
    public List<PSUData> PSUList;
}
