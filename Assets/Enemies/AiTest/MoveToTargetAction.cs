using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToTarget", story: "[Agent] moves to [target]", category: "Enemy/Movement", id: "4b9ebff5a51393d1e950364d09b3fbad")]
public partial class MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    bool success = true;

    EnemyBrain brain;
    EnemyMovement movement;
    EnemyTargeting targeting;
    BehaviorGraphAgent graphAgent;

    protected override Status OnStart()
    {
        // Get the necessary components from the Agent (the enemy GameObject)
        brain = Agent.Value.GetComponent<EnemyBrain>();
        movement = Agent.Value.GetComponent<EnemyMovement>();
        targeting = Agent.Value.GetComponent<EnemyTargeting>();
        graphAgent = Agent.Value.GetComponent<BehaviorGraphAgent>();

        if (brain == null || movement == null || targeting == null)
        {
            success = false;
            Debug.LogError("MoveToTargetNode: Missing required components on agent!", Agent);
            return Status.Failure;
        }

        return Status.Running;
    }

    // Called every frame while the node is running
    protected override Status OnUpdate()
    {
        Transform currentTarget = targeting.CurrentTarget;

        // --- 1. Check if target is still visible ---
        if (currentTarget == null)
        {
            // If not visible, search
            BlackboardVariable<State> state;
            graphAgent.GetVariable("State", out state);

            if (state == State.Searching)
            {
                currentTarget = Target.Value;
            }
            else if (state == State.Chasing || state == State.Attacking)
            {
                GameObject obj = new GameObject("Temp/LastKnownLocation");
                Transform t = obj.transform;
                t.position = movement.CurrentDestination;
                brain.HandleDamageTaken(t);
                currentTarget = t;
            } 
            else
            {
                success = false;
                return Status.Failure;
            }
        }

        

        // --- 2. Perform the Action ---
        // Tell the movement component to go to the target's position
        movement.MoveTo(currentTarget.position);

        // --- 3. Return Status ---
        // Check if the movement component has reached the destination
        if (movement.HasReachedDestination)
        {
            return Status.Success;
        }
        else
        {
            return Status.Running;
        }
    }

    protected override void OnEnd()
    {
        if (success)
        {
            movement?.Stop();
        }
    }
}

