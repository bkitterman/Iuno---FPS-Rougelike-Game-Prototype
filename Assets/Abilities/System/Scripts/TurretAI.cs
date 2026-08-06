using UnityEngine;

public class TurretAI : MonoBehaviour
{
    // Stats passed from the ability
    private float damage;
    private float fireRate;
    private float range;
    private float speed;
    private Transform playerToFollow;

    [Header("Aiming")] // Add this section
    [SerializeField] private Transform turretHead; // OPTIONAL: Assign a part that visually rotates (e.g., the gun model)
    [SerializeField] private float turnSpeed = 10f;

    // Internal state
    private float fireCooldownTimer;
    private Transform currentTarget;
    [SerializeField] private LayerMask enemyMask; // Set this in the Inspector
    [SerializeField] private Transform firePoint; // Assign the child object
    [SerializeField] private GameObject projectilePrefab; // Assign a simple projectile
    
    public void Initialize(float dmg, float rate, float rng, float spd, Transform player)
    {
        damage = dmg;
        fireRate = 1f / rate; // Convert shots per second to cooldown time
        range = rng;
        playerToFollow = player;
        speed = spd;
    }

    void Update()
    {
        // --- 1. Follow the Player ---
        if (playerToFollow != null)
        {
            // Slightly adjusted offset for aiming clearance
            transform.position = playerToFollow.position + 
                playerToFollow.up * 1.65f - 
                playerToFollow.right * 0.5f +
                playerToFollow.forward * 0.75f;

            // Turret base now follows player rotation smoothly
            Quaternion targetBaseRotation = playerToFollow.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetBaseRotation, Time.deltaTime * 5f);
        }

        // --- 2. Find Target ---
        FindTarget();

        // --- 3. Aim ---
        bool hasTarget = currentTarget != null;
        if (hasTarget)
        {
            // Get target center
            Hitbox hitbox = currentTarget.GetComponentInChildren<Hitbox>();
            Vector3 aimPoint = currentTarget.position + Vector3.up; // Default aim point
            if (hitbox != null && hitbox.TargetHealth != null && hitbox.TargetHealth.Center != null)
            {
                aimPoint = hitbox.TargetHealth.Center.position;
            }

            // Calculate target rotation
            Vector3 directionToTarget = (aimPoint - firePoint.position).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            // Smoothly rotate the visual part (if assigned)
            if (turretHead != null)
            {
                turretHead.rotation = Quaternion.Slerp(turretHead.rotation, targetRotation, Time.deltaTime * turnSpeed);
            }

            // Always rotate the fire point instantly for accurate shooting
            firePoint.rotation = targetRotation;
        }

        // --- 4. Shoot ---
        fireCooldownTimer -= Time.deltaTime;

        // Only shoot if we have a target AND the cooldown is ready
        if (hasTarget && fireCooldownTimer <= 0f)
        {
            //float angleDifference = Quaternion.Angle(firePoint.rotation, targetRotation);
            //if (angleDifference < 5f) // Only fire if within 5 degrees
            //{
                Shoot();
                fireCooldownTimer = fireRate;
            //}
        }
    }

    void Shoot()
    {
        Quaternion projectileRotation = firePoint.rotation * Quaternion.Euler(0, 0, 90);
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, projectileRotation);

        proj.GetComponent<TurretProjectile>().Init(this.gameObject,damage, speed);
    }

    void FindTarget()
    {
        // --- 1. Check Current Target Validity ---
        if (currentTarget != null)
        {
            // Still in range?
            if (Vector3.Distance(transform.position, currentTarget.position) > range)
            {
                currentTarget = null; // Out of range
            }
            // Still visible?
            else if (!HasLineOfSight(currentTarget))
            {
                currentTarget = null; // Lost line of sight
            }
            else if (playerToFollow != null && Vector3.Angle(playerToFollow.forward, currentTarget.position - playerToFollow.position) > 75f) // 150 / 2 = 75
            {
                currentTarget = null; // Outside allowed angle
            }
        }

        // --- 2. Find New Target (if needed) ---
        if (currentTarget == null)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, range, enemyMask);
            float closestDist = float.MaxValue;
            Transform potentialTarget = null;

            foreach (Collider hit in hits)
            {
                if (HasLineOfSight(hit.transform))
                {
                    if (playerToFollow != null)
                    {
                        Vector3 directionToHit = hit.transform.position - playerToFollow.position;
                        float angle = Vector3.Angle(playerToFollow.forward, directionToHit);

                        // Skip this target if it's outside the 150-degree arc (75 degrees on each side)
                        if (angle > 75f) 
                        {
                            continue; // Go to the next hit in the list
                        }
                    }

                    float dist = Vector3.Distance(transform.position, hit.transform.position);
                    if (dist < closestDist)
                    {
                        closestDist = dist;
                        potentialTarget = hit.transform;
                    }
                }
            }
            currentTarget = potentialTarget; // Assign the best valid target found
        }
    }

    /// <summary>
    /// Checks if the turret has a clear line of sight to the target's center.
    /// </summary>
    bool HasLineOfSight(Transform target)
    {
        Hitbox hitbox = target.GetComponent<Hitbox>();
        if (hitbox == null || hitbox.TargetHealth == null || hitbox.TargetHealth.Center == null)
        {
            Debug.LogWarning("Target missing Hitbox or TargetHealth.Center transform!", target);
            return false; // Cannot verify LoS without center point
        }

        Vector3 targetCenter = hitbox.TargetHealth.Center.position;
        Vector3 directionToTarget = (targetCenter - firePoint.position).normalized;
        float distanceToTarget = Vector3.Distance(firePoint.position, targetCenter);

        // small offset (direction * 0.1f) to start the ray slightly in front
        // to avoid hitting the turret's own collider.
        if (Physics.Raycast(firePoint.position + directionToTarget * 0.1f, directionToTarget, out RaycastHit hit, distanceToTarget))
        {
            if (hit.transform == target || hit.transform.IsChildOf(hitbox.TargetHealth.transform))
            {
                return true; // Clear line of sight
            }
        }
        else
        {
            return true;
        }

        return false;
    }
}