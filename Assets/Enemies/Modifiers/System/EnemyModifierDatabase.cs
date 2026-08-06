using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Modifier Database", menuName = "Database/Modifier Database")]
public class EnemyModifierDatabase : ScriptableObject
{
    public List<EnemyModifierData> allModifiers;
}