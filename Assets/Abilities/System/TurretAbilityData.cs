using UnityEngine;

[CreateAssetMenu(fileName = "Turret Ability", menuName = "Ability/Turret")]
public class TurretAbilityData : AbilityData
{
    [Header("Turret Stats")]
    public GameObject turretPrefab; // The prefab of the turret buddy
    public float duration = 15f;    // How long the turret lasts
    public float fireRate = 2f;     // Shots per second
    public float damagePerShot = 5f;
    public float projectileSpeed = 20f;
    public float range = 25f;       // How far it can see/shoot
}