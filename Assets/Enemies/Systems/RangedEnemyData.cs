using UnityEngine;

[CreateAssetMenu(fileName = "Ranged", menuName = "Enemy/Ranged")]
public class RangedEnemyData : EnemyData
{
    [Header("Ranged Specifics")]
    [Tooltip("The minimum distance the enemy will try to keep from the player. It will flee if the player is closer than this.")]
    public float minimumAttackRange = 5f;

    [Tooltip("The ideal distance to stop and start shooting.")]
    public float optimalAttackRange = 20f;
}