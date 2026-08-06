using UnityEngine;

public class GravityGrenade : AbilityInstance
{
    public AbilityData Data { get; set; }
    public bool IsOnCooldown => CooldownRemaining > 0;
    public float CooldownRemaining { get; private set; }
    public int CurrentCharges { get; private set; } 

    private GravityGrenadeAbilityData grenadeData;
    private Transform cameraTransform;
    private StatsController _statsController;

    public void OnEquip(GameObject owner)
    {
        grenadeData = (GravityGrenadeAbilityData)Data;
        cameraTransform = owner.GetComponent<PlayerMovement>().playerCamera;

        _statsController = owner.GetComponent<StatsController>();
    }

    public void TryActivate()
    {
        if (IsOnCooldown) return;
        CooldownRemaining = grenadeData.Cooldown;

        GameObject grenadeObj = Object.Instantiate(
            grenadeData.grenadePrefab,
            cameraTransform.position + cameraTransform.forward,
            cameraTransform.rotation
        );

        // Pass stats to the prefab
        GravityGrenadePrefab grenadeLogic = grenadeObj.GetComponent<GravityGrenadePrefab>();
        grenadeLogic.Initialize(
            grenadeData.explosionDamage * _statsController.AllDamageMultiplier.GetValue() * _statsController.SecondaryDamageMultiplier.GetValue(),
            grenadeData.pullDamage * _statsController.AllDamageMultiplier.GetValue(),
            grenadeData.radius,
            grenadeData.pullForce, // Pull force
            grenadeData.pullTime
        );

        Rigidbody rb = grenadeObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(cameraTransform.forward * grenadeData.throwForce, ForceMode.Impulse);
        }
    }

    public void AbilityUpdate()
    {
        if (CooldownRemaining > 0) CooldownRemaining -= Time.deltaTime;
    }
    public void OnUnequip() { }
}