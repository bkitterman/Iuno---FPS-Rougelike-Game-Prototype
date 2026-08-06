using UnityEngine;

[CreateAssetMenu(fileName = "Melee Enemy", menuName = "Enemy/Melee")]
public class BasicMeleeEnemyData : EnemyData
{
    [Header("Melee Specifics")]
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float attackDamage = 25f; // This could also come from an assigned weapon in availableWeapons
    public float lungeSpeedMultiplier = 2f; // Multiplier applied during attack lunge
    public float lungeDuration = 0.3f;
}