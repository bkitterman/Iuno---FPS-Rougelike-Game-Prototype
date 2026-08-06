using UnityEngine;
using UnityEngine.AI;

public class EnemyDashAbility : AbilityInstance
{
    // Interface properties
    public AbilityData Data { get; set; }
    public bool IsOnCooldown => CooldownRemaining > 0;
    public float CooldownRemaining { get; private set; }
    public int CurrentCharges {  get; private set; }

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashDuration = 0.2f;
    private Vector3 dashVelocity;

    // Enemy references
    private NavMeshAgent agent;
    private Transform enemyTransform;
    private EnemyBrain brain; 

    public void OnEquip(GameObject owner)
    {
        agent = owner.GetComponent<NavMeshAgent>();
        enemyTransform = owner.transform;
        brain = owner.GetComponent<EnemyBrain>();
        // Initialize charges/cooldown
        CooldownRemaining = 0f;
    }

    public void TryActivate()
    {
        if (IsOnCooldown || isDashing || agent == null || !agent.isOnNavMesh) return;

        CooldownRemaining = Data.Cooldown; 
        brain.Movement.FaceTargetInstantly(brain.Targeting.CurrentTarget.position);
        isDashing = true;
        dashTimer = dashDuration;

        // Get Dash Direction (e.g., towards the current target)
        Vector3 dashDirection;
        if (brain != null && brain.Targeting.CurrentTarget != null)
        {
            dashDirection = (brain.Targeting.CurrentTarget.position - enemyTransform.position).normalized;
            // Ignore vertical difference for a horizontal dash
            dashDirection.y = 0;
            dashDirection.Normalize();
        }
        else
        {
            dashDirection = enemyTransform.forward; // Default to forward if no target
        }

        // Calculate Dash Velocity
        // We use agent.speed as a base multiplier, plus the ability's value
        // Or directly use Data.Value if it represents speed. Let's assume Data.Value is distance.
        float dashSpeed = Data.Value / dashDuration;
        dashVelocity = dashDirection * dashSpeed;

        // --- Temporarily take control from NavMeshAgent ---
        if (agent.enabled)
        {
            agent.isStopped = true; // Stop path following
            agent.ResetPath();
            agent.velocity = dashVelocity; // Apply the dash velocity directly
        }

        // Optional: Play dash animation/VFX
    }

    public void AbilityUpdate()
    {
        // Tick cooldown
        if (CooldownRemaining > 0)
        {
            CooldownRemaining -= Time.deltaTime;
        }

        // Handle dash movement duration
        if (isDashing)
        {
            if (dashTimer > 0)
            {
                // The agent's velocity is already set, just tick the timer
                // If agent.velocity doesn't work well, you can directly move transform:
                // enemyTransform.position += dashVelocity * Time.deltaTime;
                dashTimer -= Time.deltaTime;
            }
            else
            {
                isDashing = false;
                // --- Give control back to NavMeshAgent ---
                if (agent.enabled && agent.isOnNavMesh)
                {
                    agent.velocity = Vector3.zero; // Stop the dash velocity
                    agent.isStopped = false; // Allow path following again
                                             // The brain's Update loop will issue a new MoveTo command
                }
            }
        }
    }

    public void OnUnequip() { }
}