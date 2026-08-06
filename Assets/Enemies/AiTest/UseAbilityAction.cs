using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "UseAbility", story: "[Agent] uses [Ability] [X]", category: "Enemy/Attack", id: "73da224cd8d31a6d0a6b511375e77482")]
public partial class UseAbilityAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<AbilityData> Ability;
    [SerializeReference] public BlackboardVariable<int> X;
    EnemyTargeting targeting;
    EnemyAbilityHandler handler;

    protected override Status OnStart()
    {
        targeting = Agent.Value.GetComponent<EnemyTargeting>();
        handler = Agent.Value.GetComponent<EnemyAbilityHandler>();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        handler.TryUseAbility(X, targeting.CurrentTarget);
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

