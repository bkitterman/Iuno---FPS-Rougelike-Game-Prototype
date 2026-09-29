using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Attack", story: "[Agent] attacks [target]", category: "Enemy/Attack", id: "3ddcae0cfa3b8325664d79b585d68382")]
public partial class AttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    EnemyWeaponHandler weaponHandler;
    EnemyTargeting targeting;
    EnemyMovement movement;
    EnemyData data;

    protected override Status OnStart()
    {
        weaponHandler = Agent.Value.GetComponent<EnemyWeaponHandler>();
        targeting = Agent.Value.GetComponent<EnemyTargeting>();
        movement = Agent.Value.GetComponent<EnemyMovement>(); // Optional

        if (weaponHandler == null || targeting == null)
        {
            Debug.LogError("AttackNode: Missing required components (WeaponHandler or Targeting) on agent!", Agent);
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // --- Perform the Action ---
        // Ensure enemy is stopped and facing target before attacking (important for melee)
        movement?.Stop();
        movement?.FaceTargetInstantly(targeting.CurrentTarget.position); // Ensure facing

        // Tell the weapon handler to attack the target
        weaponHandler.Attack(targeting.CurrentTarget);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

