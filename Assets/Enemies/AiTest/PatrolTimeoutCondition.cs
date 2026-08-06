using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PatrolTimeoutCondition", story: "[Agent] can Patrol", category: "Enemy/Movement", id: "e5766b44525e204c5fe5e1d9fd5e1e2a")]
public partial class PatrolTimeoutCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;

    EnemyBrain brain;

    public override void OnStart()
    {
        // Get the necessary components from the Agent (the enemy GameObject)
        brain = Agent.Value.GetComponent<EnemyBrain>();

        if (brain == null)
        {
            Debug.LogError("MoveToTargetNode: Missing required components on agent!", Agent);
        }
    }

    public override bool IsTrue()
    {
        if (brain != null && brain.CanPatrol)
        {
            brain.StartPatrol();
            return true;
        }
        return false;
    }

    public override void OnEnd()
    {
    }
}
