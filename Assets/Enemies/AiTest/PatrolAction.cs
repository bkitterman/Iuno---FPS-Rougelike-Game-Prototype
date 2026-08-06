using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Patrol", story: "[Agent] Patrols", category: "Enemy/Movement", id: "944019c5b7ee2bbabc7a029575c4e60a")]
public partial class PatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    
    EnemyBrain brain;
    EnemyMovement movement;

    protected override Status OnStart()
    {
        // Get the necessary components from the Agent (the enemy GameObject)
        brain = Agent.Value.GetComponent<EnemyBrain>();
        movement = Agent.Value.GetComponent<EnemyMovement>();

        if (brain == null || movement == null)
        {
            Debug.LogError("MoveToTargetNode: Missing required components on agent!", Agent);
            return Status.Failure;
        }

        if (movement.IsMoving) return Status.Running;

        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere * brain.GetData().PatrolRadius;
        randomDirection += Agent.Value.transform.position;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomDirection, out hit, brain.GetData().PatrolRadius, UnityEngine.AI.NavMesh.AllAreas))
        {
            movement.MoveTo(hit.position);
        }

        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

