using UnityEngine;
using System.Collections.Generic; // For lists

public class EnemyMeleeWeapon : EnemyWeaponInstance
{
    public WeaponData Data { get; private set; }
    private EnemyBrain brain;

    [Header("Melee Specifics")]
    [SerializeField] private float attackRange = 2f;    // How far the melee reaches
    [SerializeField] private float attackRadius = 1f;   // How wide the attack is
    //[SerializeField] private float attackAngle = 90f;   // The arc of the swing (degrees)
    [SerializeField] private LayerMask damageMask;
    private EnemyModifierManager modifierManager;
    private StatsController stats;

    // --- Weapon State ---
    private float cooldownTimer;

    public override void Initialize(WeaponData data, EnemyBrain ownerBrain, EnemyModifierManager manager)
    {
        Data = data;
        brain = ownerBrain;
        modifierManager = manager;
        stats = brain.Stats;

        // Get specific melee stats if needed from Data or a MeleeWeaponData SO
        //if (data is BasicMeleeData meleeData) // Example if using specific SO
        //{
        //    attackRange = meleeData.attackRange;
        //    // attackDamage = meleeData.attackDamage; // Get damage from SO
        //}
        //else 
        //{
             attackRange = Data.Range;
        //}

        if (ownerBrain == null) Debug.LogWarning("Brain missing");
        if (ownerBrain.GetData() == null) Debug.LogWarning("Accesor Error");
        damageMask = ownerBrain.GetData().targetMask;
    }

    void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }

    public override void TryAttack(Transform target) // Target might be null for melee AoE
    {
        if (cooldownTimer > 0)
        {
            return; // On cooldown
        }
        PerformMeleeAttack();

        // Use the cooldown defined in the specific melee data or base WeaponData
        cooldownTimer = Data.ReloadTime;
    }

    private void PerformMeleeAttack()
    {
        // --- Damage Detection ---
        // Option 1: Simple OverlapSphere (Good starting point)
        Collider[] hits = Physics.OverlapSphere(transform.position + transform.forward * (attackRange / 2f), attackRadius, damageMask);

        List<GameObject> alreadyDamaged = new List<GameObject>(); // Prevent multi-hits

        foreach (Collider hit in hits)
        {
            // Prevent double damage to the same target in one swing
            if (alreadyDamaged.Contains(hit.gameObject)) continue;

            PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(this.gameObject, transform.position, Data.Damage * stats.AllDamageMultiplier.GetValue());

                alreadyDamaged.Add(hit.gameObject); // Mark as damaged
            }
        }

        // --- Play VFX/SFX ---
        // (e.g., Play swing animation, sound effect)
        // animator?.SetTrigger("Attack");
        // audioSource?.PlayOneShot(swingSound);
    }
}