using UnityEngine;

[CreateAssetMenu(fileName = "Gravity Grenade", menuName = "Ability/Grenades/Gravity Grenade")]
public class GravityGrenadeAbilityData : AbilityData
{
    [Header("Grenade Stats")]
    public GameObject grenadePrefab; // The projectile to spawn
    public float explosionDamage = 150f;
    public float pullDamage = 10f;
    public float radius = 5f;
    public float pullForce = 3f;
    public float throwForce = 20f;
    public float pullTime = 5f;
}