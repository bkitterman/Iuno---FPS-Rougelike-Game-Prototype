using UnityEngine;

[CreateAssetMenu(fileName = "Grenade", menuName = "Ability/Grenades/Grenade")]
public class GrenadeAbilityData : AbilityData
{
    [Header("Grenade Stats")]
    public GameObject grenadePrefab; // The projectile to spawn
    public float explosionDamage = 150f;
    public float projectileDamage = 10f;
    public float radius = 5f;
    public float delay = 3f;
    public float throwForce = 20f;
    // You can add more stats here later, like "Damage," "Duration," "Range," etc.
}