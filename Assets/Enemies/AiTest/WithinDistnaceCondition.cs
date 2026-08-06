using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "WithinDistnace", story: "[Agent] is within [Distance] of [Target]", category: "Enemy/Attack", id: "d5de3abe2baca9855765387de0f3511c")]
public partial class WithinDistnaceCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<float> Distance;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    EnemyTargeting targeting;

    public override bool IsTrue()
    {
        if(targeting.TargetDistance < Distance.Value)
        {
            return true;
        }
        return false;
    }

    public override void OnStart()
    {
        targeting = Agent.Value.GetComponent<EnemyTargeting>();
    }

    public override void OnEnd()
    {
    }
}
