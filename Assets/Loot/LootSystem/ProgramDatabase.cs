using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Database", menuName = "Database/Programs")]
public class ProgramDatabase : ScriptableObject
{
    public List<ProgramData> ProgramList;
}
