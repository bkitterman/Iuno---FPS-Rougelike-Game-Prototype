using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsInRange", story: "[Agent] is in range of [Target]", category: "Enemy/Attack", id: "cbf9c5e6cf016e0c652833a7242a31ea")]
public partial class IsInRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;

    EnemyTargeting targeting;
    EnemyData data;
    public override bool IsTrue()
    {
        if (targeting.TargetDistance <= data.range)
            return true;
        return false;
    }

    public override void OnStart()
    {
        targeting = Agent.Value.GetComponent<EnemyTargeting>();
        data = Agent.Value.GetComponent<EnemyBrain>().EnemyData;
        // weaponHandler = Agent.Value.GetComponent<EnemyWeaponHandler>();
        if (targeting == null)
        {
            Debug.LogError("IsTargetInRangeNode: Missing EnemyTargeting component on agent!", Agent);
        }
    }

    public override void OnEnd()
    {
    }
}
