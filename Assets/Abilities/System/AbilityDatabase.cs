using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "AbilityDatabase", menuName = "Database/Ability")]
public class AbilityDatabase : ScriptableObject
{
    public List<AbilityData> AbilityList;
}
