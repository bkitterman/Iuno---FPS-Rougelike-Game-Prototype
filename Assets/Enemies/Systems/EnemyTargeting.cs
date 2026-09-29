using UnityEngine;
using System.Collections.Generic; 
using System.Linq;

public class EnemyTargeting : MonoBehaviour
{
    [Header("Data (Set automatically by Brain)")]
    [SerializeField] private float detectionRadius;
    [SerializeField] private float lineOfSightRange;
    [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [Header("State (Read by Brain/AI)")]
    public Transform CurrentTarget { get; private set; }
    public bool HasLineOfSight { get; private set; }
    public float TargetDistance { get; private set; } = float.MaxValue;

    [Header("Configuration")]
    [SerializeField] private float targetingInterval = 0.2f; // How often to scan for new targets (seconds)
    [SerializeField] private Transform eyeLocation; 

    private float targetingTimer;
    private List<Transform> potentialTargets = new List<Transform>();

    /// <summary>
    /// Called by EnemyBrain during initialization.
    /// </summary>
    public void Initialize(EnemyData data)
    {
        detectionRadius = data.detectionRadius;
        lineOfSightRange = data.lineOfSightRange;
        targetMask = data.targetMask;
        obstacleMask = data.obstacleMask;

        if (eyeLocation == null)
        {
            eyeLocation = transform;
        }
    }

    void Update()
    {
        targetingTimer -= Time.deltaTime;
        if (targetingTimer <= 0f)
        {
            ScanForTargets();
            targetingTimer = targetingInterval;
        }

        ValidateCurrentTarget();
    }

    /// <summary>
    /// Scans the detection radius for potential targets.
    /// </summary>
    void ScanForTargets()
    {
        potentialTargets.Clear();
        Collider[] targetsInRadius = Physics.OverlapSphere(eyeLocation.position, detectionRadius, targetMask);

        // Sort targets by distance (closest first)
        potentialTargets = targetsInRadius
                            .OrderBy(t => Vector3.Distance(eyeLocation.position, t.transform.position))
                            .Select(t => t.transform)
                            .ToList();

        // If enemy doesn't have a current target, try to pick the closest valid one
        if (CurrentTarget == null && potentialTargets.Count > 0)
        {
            // Find the first potential target that enemy has LoS to
            foreach (Transform potential in potentialTargets)
            {
                if (CheckLineOfSight(potential))
                {
                    CurrentTarget = potential;

                    HasLineOfSight = true;
                    TargetDistance = Vector3.Distance(eyeLocation.position, CurrentTarget.position);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Checks if the current target is still valid (in range, LoS).
    /// </summary>
    void ValidateCurrentTarget()
    {
        if (CurrentTarget == null)
        {
            HasLineOfSight = false;
            TargetDistance = float.MaxValue;
            return;
        }

        TargetDistance = Vector3.Distance(eyeLocation.position, CurrentTarget.position);
        
        // Check if out of range
        if (TargetDistance > lineOfSightRange)
        {
            CurrentTarget = null;
            HasLineOfSight = false;
            TargetDistance = float.MaxValue;
            return;
        }

        HasLineOfSight = CheckLineOfSight(CurrentTarget);
        if (!HasLineOfSight)
        {
            CurrentTarget = null;
            TargetDistance = float.MaxValue;
        }
    }

    /// <summary>
    /// Performs a Raycast to check for obstructions between eyeLocation and the target.
    /// </summary>
    bool CheckLineOfSight(Transform target)
    {
        Vector3 direction = (target.position - eyeLocation.position).normalized;
        float distance = Vector3.Distance(eyeLocation.position, target.position);

        if (Physics.Raycast(eyeLocation.position, direction, out RaycastHit hit, distance, obstacleMask))
        {
            return false;
        }
        return true;
    }
}