using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    [Header("Data (Set automatically by Brain)")]
    private float baseSpeed;

    [Header("State")]
    public bool IsMoving => Agent.velocity.magnitude > 0.1f;
    public bool HasReachedDestination => Agent.isOnNavMesh && !Agent.pathPending && Agent.remainingDistance <= Agent.stoppingDistance;
    public Vector3 CurrentDestination;

    // --- Component References ---
    public NavMeshAgent Agent { get; private set; }
    private EnemyBrain brain; // Reference to the brain for data/state

    void Start()
    {
        Agent = GetComponent<NavMeshAgent>();
        brain = GetComponent<EnemyBrain>();
    }

    /// <summary>
    /// Called by EnemyBrain during initialization.
    /// </summary>
    public void Initialize(EnemyData data)
    {
        baseSpeed = data.moveSpeed;
        if (Agent == null) Agent = GetComponent<NavMeshAgent>();
        Agent.speed = baseSpeed;
        // Configure other NavMeshAgent properties like acceleration, angularSpeed, stoppingDistance
        // Agent.acceleration = ...;
        // Agent.angularSpeed = ...;
        // Agent.stoppingDistance = ...; 
    }

    /// <summary>
    /// Commands the agent to move towards a target position.
    /// </summary>
    public void MoveTo(Vector3 destination)
    {
        if (Agent.isOnNavMesh && Agent.destination != destination)
        {
            CurrentDestination = destination;
            Agent.SetDestination(destination);
            Agent.isStopped = false; // Ensure agent can move
        }
    }

    /// <summary>
    /// Stops the agent's current movement.
    /// </summary>
    public void Stop()
    {
        if (Agent.isOnNavMesh)
        {
            Agent.ResetPath();
            Agent.isStopped = true; // Stop current movement
        }
    }

    /// <summary>
    /// Instantly rotates the agent to face a target position (on the horizontal plane).
    /// </summary>
    public void FaceTargetInstantly(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = lookRotation;
        }
    }

    /// <summary>
    /// Sets the agent's movement speed (e.g., for sprinting or slowing effects).
    /// </summary>
    public void SetSpeed(float speedMultiplier)
    {
        Agent.speed = baseSpeed * speedMultiplier;
    }

    /// <summary>
    /// Resets the agent's movement speed to its base value.
    /// </summary>
    public void ResetSpeed()
    {
        Agent.speed = baseSpeed;
    }
}