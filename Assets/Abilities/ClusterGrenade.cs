using UnityEngine;

public class ClusterGrenade : AbilityInstance
{
    public AbilityData Data { get; set; }
    public bool IsOnCooldown => CooldownRemaining > 0;
    public float CooldownRemaining { get; private set; }
    public int CurrentCharges { get; private set;  }

    private ClusterGrenadeAbilityData clusterData;
    private Transform cameraTransform;
    private StatsController _statsController;


    public void OnEquip(GameObject owner)
    {
        clusterData = (ClusterGrenadeAbilityData)Data; // Cast to our specific data type
        cameraTransform = owner.GetComponent<PlayerMovement>().playerCamera;
        _statsController = owner.GetComponent<StatsController>();
    }

    public void TryActivate()
    {
        if (IsOnCooldown) return;
        CooldownRemaining = clusterData.Cooldown;

        // 1. Spawn the MAIN grenade prefab
        GameObject mainGrenadeObj = Object.Instantiate(
            clusterData.mainGrenadePrefab,
            cameraTransform.position + cameraTransform.forward,
            cameraTransform.rotation
        );

        // 2. Pass data to the main grenade's script
        ClusterGrenade_Prefab mainLogic = mainGrenadeObj.GetComponent<ClusterGrenade_Prefab>();
        mainLogic.Initialize(clusterData,
             clusterData.initialDamage *_statsController.AllDamageMultiplier.GetValue() * _statsController.SecondaryDamageMultiplier.GetValue(),
             clusterData.submunitionDamage *_statsController.AllDamageMultiplier.GetValue()); // Pass the whole data object

        // 3. Throw it
        Rigidbody rb = mainGrenadeObj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(cameraTransform.forward * clusterData.throwForce, ForceMode.Impulse);
        }
    }

    public void AbilityUpdate()
    {
        if (CooldownRemaining > 0)
        {
            CooldownRemaining -= Time.deltaTime;
        }
    }

    public void OnUnequip() { }
}