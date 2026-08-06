using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Database/Weapons")]
public class WeaponDatabase : ScriptableObject
{
    public List<WeaponData> WeaponList;
}
