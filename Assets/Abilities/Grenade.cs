using UnityEngine;

public class Grenade : AbilityInstance
{
    // Interface Properties
    public AbilityData Data { get; set; }
    public bool IsOnCooldown => CooldownRemaining > 0;
    public float CooldownRemaining { get; private set; }
    public int CurrentCharges { get; private set; }

    private Transform cameraTransform;
    private GrenadeAbilityData grenadeData;
    private StatsController _statsController;

    /// <summary>
    /// Called every frame by ability manager
    /// </summary>
    public void AbilityUpdate()
    {
        // Just tick the cooldown
        if (CooldownRemaining > 0)
        {
            CooldownRemaining -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Called when the ability is first equipped to the player.
    /// Use this to get references (like PlayerMovement, StatsController).
    /// </summary>
    public void OnEquip(GameObject owner)
    {
        grenadeData = (GrenadeAbilityData)Data;
        cameraTransform = owner.GetComponent<PlayerMovement>().playerCamera;
        _statsController = owner.GetComponent<StatsController>();
    }

    /// <summary>
    /// Called when the player presses the ability button.
    /// This is where the "try to use" logic goes (check cooldown, etc.).
    /// </summary>
    public void TryActivate()
    {
        if (IsOnCooldown) return;

        CooldownRemaining = Data.Cooldown;

        // 1. Create the grenade from the prefab
        GameObject grenadeObj = Object.Instantiate(
            grenadeData.grenadePrefab,
            cameraTransform.position + cameraTransform.forward, // Spawn in front of cam
            cameraTransform.rotation
        );

        // 2. Pass the stats to the prefab's script
        GrenadePrefab grenadeLogic = grenadeObj.GetComponent<GrenadePrefab>();
        grenadeLogic.Initialize(
            grenadeData.explosionDamage * _statsController.AllDamageMultiplier.GetValue() * _statsController.SecondaryDamageMultiplier.GetValue(),
            grenadeData.projectileDamage * _statsController.AllDamageMultiplier.GetValue(),
            grenadeData.radius,
            grenadeData.delay
        );

        // 3. Add force to throw it
        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(cameraTransform.forward * grenadeData.throwForce, ForceMode.Impulse);
        }
    }

    /// <summary>
    /// Called when the ability is unequipped.
    /// </summary>
    public void OnUnequip()
    {

    }
}
