using UnityEngine;

public class ShoulderTurret : AbilityInstance
{
    // Interface properties
    public AbilityData Data { get; set; }
    public bool IsOnCooldown => CooldownRemaining > 0;
    public float CooldownRemaining { get; private set; }
    public int CurrentCharges { get; private set; }

    // Private references & state
    private TurretAbilityData turretData;
    private Transform playerTransform;
    private GameObject currentTurretInstance;
    private float durationTimer;

    public void OnEquip(GameObject owner)
    {
        turretData = (TurretAbilityData)Data; // Cast the data
        playerTransform = owner.transform;
    }

    public void TryActivate()
    {
        
        // Can't activate if on cooldown OR if a turret already exists
        if (IsOnCooldown || currentTurretInstance != null) return;

        CooldownRemaining = turretData.Cooldown;
        durationTimer = turretData.duration;

        // --- Spawn the Turret ---
        // Spawn slightly above and behind the player
        Vector3 spawnPos = playerTransform.position + playerTransform.up * 1.5f - playerTransform.forward * -2f;
        currentTurretInstance = Object.Instantiate(turretData.turretPrefab, spawnPos, playerTransform.rotation);

        // --- Initialize the Turret's AI ---
        TurretAI turretAI = currentTurretInstance.GetComponent<TurretAI>();
        if (turretAI != null)
        {
            turretAI.Initialize(
                turretData.damagePerShot,
                turretData.fireRate,
                turretData.range,
                turretData.projectileSpeed,
                playerTransform 
            );
        }
    }

    public void AbilityUpdate()
    {
        // Tick cooldown
        if (CooldownRemaining > 0)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        // Tick duration if a turret exists
        if (currentTurretInstance != null)
        {
            durationTimer -= Time.deltaTime;
            if (durationTimer <= 0)
            {
                // Duration is over, destroy the turret
                Object.Destroy(currentTurretInstance);
                currentTurretInstance = null; // Clear the reference
            }
        }
    }

    public void OnUnequip()
    {
        // Clean up the turret if the ability is unequipped
        if (currentTurretInstance != null)
        {
            Object.Destroy(currentTurretInstance);
            currentTurretInstance = null;
        }
    }
}