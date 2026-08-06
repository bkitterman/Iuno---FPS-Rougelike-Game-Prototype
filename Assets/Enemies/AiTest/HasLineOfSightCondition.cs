using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasLineOfSight", story: "[Agent] has line-of-sight of [target]", category: "Conditions", id: "3c155bc550267f5bf3271f67885a3635")]
public partial class HasLineOfSightCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    EnemyTargeting targeting;

    public override bool IsTrue()
    {
        if (targeting.HasLineOfSight) return true;
        return true;
    }

    public override void OnStart()
    {
        targeting = Agent.Value.GetComponent<EnemyTargeting>();
        if (targeting == null)
        {
            Debug.LogError("HasTargetNode: Missing EnemyTargeting component on agent!", Agent);

        }
    }

    public override void OnEnd()
    {
    }
}
