using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Ammo", menuName = "Database/AmmoData")]
public class AmmoDatabase : ScriptableObject
{
    public List<AmmoData> Ammo = new();
}

