using UnityEngine;

[CreateAssetMenu(fileName = "Cluster Grenade", menuName = "Ability/Grenades/Cluster Grenade")]
public class ClusterGrenadeAbilityData : AbilityData
{
    [Header("Cluster Grenade Stats")]
    public GameObject mainGrenadePrefab; // Prefab for the initial grenade
    public GameObject submunitionPrefab; // Prefab for the smaller cluster grenades
    public int submunitionCount = 4;
    public float initialDelay = 0.75f;    // Time until main grenade releases submunitions
    public float submunitionDelay = 1.25f; // Time until submunitions explode
    public float throwForce = 20f;
    public float initialDamage = 10f;
    public float initialRadius = 1.5f;

    [Header("Submunition Stats")]
    public float submunitionDamage = 50f;
    public float submunitionRadius = 3f;
    public float submunitionSpreadForce = 5f; // Force to push submunitions apart
}