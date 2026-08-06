using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;

using System;

// Require essential components to ensure they exist on the prefab
[RequireComponent(typeof(StatsController))]
[RequireComponent(typeof(StatusEffectManager))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTargeting))]
[RequireComponent(typeof(EnemyMovement))]
[RequireComponent(typeof(EnemyWeaponHandler))]
[RequireComponent(typeof(EnemyAbilityHandler))]
[RequireComponent(typeof(EnemyModifierManager))]

public class EnemyBrain : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] public EnemyData EnemyData;

    public EnemySpawnManager source;

    // --- Component References ---
    public StatsController Stats { get; private set; }
    public StatusEffectManager StatusManager { get; private set; }
    public NavMeshAgent Agent { get; private set; }
    public EnemyTargeting Targeting { get; private set; }
    public EnemyMovement Movement { get; private set; }
    public EnemyWeaponHandler WeaponHandler { get; private set; }
    public EnemyAbilityHandler AbilityHandler { get; private set; }
    public EnemyHealth Health { get; private set; }
    public EnemyModifierManager modifierManager { get; private set; }  
    private BehaviorGraphAgent behaviorTree; 

    // --- States ---
    public bool CanPatrol => patrolCooldown <= 0f && !isPatrolling;
    private bool isPatrolling = false;

    // --- Timers ---
    private float patrolCooldown = 0f;

    void Awake()
    {
        // --- Get Core Component References ---
        Stats = GetComponent<StatsController>();
        StatusManager = GetComponent<StatusEffectManager>();
        Agent = GetComponent<NavMeshAgent>();
        Targeting = GetComponent<EnemyTargeting>();
        Movement = GetComponent<EnemyMovement>();
        WeaponHandler = GetComponent<EnemyWeaponHandler>();
        AbilityHandler = GetComponent<EnemyAbilityHandler>();
        Health = GetComponent<EnemyHealth>();

        behaviorTree = GetComponent<BehaviorGraphAgent>();

        // --- Initialize Components from Data ---
        if (EnemyData == null)
        {
            Debug.LogError("EnemyData not assigned!", this);
            return;
        }

        InitializeMovement();
        InitializeTargeting();
        InitializeAbilities();
        InitializeWeapons();

        Health.OnDamage += HandleDamageTaken;
    }

    void Start()
    {
        InitializeStats();

        patrolCooldown = EnemyData.PatrolTime;
        behaviorTree.SetVariableValue("SearchTimer", EnemyData.SearchTime);
    }

    void Update()
    {
        // Count timer down if not patrolling/Moving
        if(!CanPatrol && !Movement.IsMoving)
        {
            patrolCooldown -= Time.deltaTime;
        }   

        // If patrol interrupted, reset
        // If Patrol finished, reset
        if((isPatrolling && Targeting.CurrentTarget != null) ||
            (isPatrolling && !Movement.IsMoving))
        {
            patrolCooldown = EnemyData.PatrolTime;
            isPatrolling = false;
        }
    }

    void InitializeStats()
    {
        // Non-zero chance this is called prior to Stats Awake method. Might cause cascading issues later

        // Set up base stats from the EnemyData SO
        Stats.MaxHealth.BaseValue = EnemyData.maxHealth;
        Stats.MoveSpeed.BaseValue = EnemyData.moveSpeed;

        // Initialize current health, etc.
    }

    void InitializeMovement()
    {
        Movement.Initialize(EnemyData);
        // Set acceleration, angular speed, etc.
    }

    void InitializeTargeting()
    {
        Targeting.Initialize(EnemyData);
    }

    void InitializeWeapons()
    {
        WeaponHandler.Initialize(EnemyData, this);
    }

    void InitializeAbilities()
    {
        AbilityHandler.Initialize(EnemyData);
    }


    // --- Set States ---
    public void StartPatrol()
    {
        isPatrolling = true;
    }

    // --- Events ---
    public void HandleDamageTaken(Transform source)
    {
        if (source != null && Targeting.CurrentTarget == null)
        {
            behaviorTree.SetVariableValue("Target", source);
            behaviorTree.SetVariableValue("State", State.Searching);
        }
    }

    // --- Public Accessor ---
    public EnemyData GetData()
    {
        return EnemyData;
    }

    public event Action<EnemyBrain> OnDeath;
    public void ReportDeath(EnemyBrain brain)
    {
        OnDeath?.Invoke(brain);
    }
}


