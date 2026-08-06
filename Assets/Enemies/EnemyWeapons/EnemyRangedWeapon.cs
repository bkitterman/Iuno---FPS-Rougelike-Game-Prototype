using UnityEngine;

public class EnemyRangedWeapon : EnemyWeaponInstance
{
    // --- Data & References ---
    public WeaponData Data { get; private set; }
    private EnemyBrain brain;
    private Transform playerCamera; // To aim at the player's center

    [Header("Weapon Components")]
    [SerializeField] private Transform muzzlePoint;
    [SerializeField] private GameObject projectilePrefab; // Can be set here or from Data
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip shootSound;

    private EnemyModifierManager modifierManager;
    private StatsController stats;

    // --- Weapon State ---
    private float cooldownTimer;

    /// <summary>
    /// Initialize the weapon with its data and owner
    /// </summary>
    public override void Initialize(WeaponData data, EnemyBrain ownerBrain, EnemyModifierManager manager)
    {
        Data = data;
        brain = ownerBrain;
        modifierManager = manager;
        stats = brain.Stats;

        // Get projectile from Data if not set here
        if (projectilePrefab == null && Data.ProjectilePrefab != null)
        {
            projectilePrefab = Data.ProjectilePrefab;
        }

        // Find the player's camera for precise aiming
        playerCamera = Camera.main.transform;

        // Find muzzle point if not assigned
        if (muzzlePoint == null)
        {
            muzzlePoint = transform;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError($"Ranged weapon {Data.name} has no projectile prefab!", this);
        }
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Called by the EnemyWeaponHandler
    /// </summary>
    public override void TryAttack(Transform target)
    {
        if (cooldownTimer > 0 || target == null)
        {
            return; // On cooldown or no target
        }

        PerformRangedAttack(target);

        // Use the fire rate from the WeaponData
        cooldownTimer = 60f / Data.RoundsPerMinute;
    }

    /// <summary>
    /// Aims at the target and fires a projectile
    /// </summary>
    private void PerformRangedAttack(Transform target)
    {
        // --- 1. Calculate Aim Direction ---
        Vector3 aimPoint;

        // Try to get the player's hitbox/center for precise aiming
        Hitbox playerHitbox = target.GetComponentInChildren<Hitbox>();
        if (playerHitbox != null && playerHitbox.TargetHealth != null && playerHitbox.TargetHealth.Center != null)
        {
            aimPoint = playerHitbox.TargetHealth.Center.position;
        }
        else
        {
            // Fallback: aim at the player's camera (a good center-mass target)
            aimPoint = playerCamera.transform.position - new Vector3(0f,0.3f,0f);
        }

        Vector3 direction = (aimPoint - muzzlePoint.position).normalized;

        // --- 2. Fire the Projectile ---
        if (projectilePrefab != null)
        {
            GameObject proj = Instantiate(projectilePrefab, muzzlePoint.position, Quaternion.LookRotation(direction));

            // Assuming the projectile has a script to initialize it
            EnemyProjectile projScript = proj.GetComponent<EnemyProjectile>();
            if (projScript != null)
            {
                // Pass damage, speed, and the enemy as the source
                projScript.Initialize(Data.Damage * stats.AllDamageMultiplier.GetValue(), 
                    Data.ProjectileSpeed, 
                    brain.gameObject);
            }
        }

        // --- 3. Play VFX/SFX ---
        if (muzzleFlash != null)
        {
            muzzleFlash.Play();
        }

        if (audioSource != null && shootSound != null)
        {
            audioSource.PlayOneShot(shootSound);
        }
    }
}