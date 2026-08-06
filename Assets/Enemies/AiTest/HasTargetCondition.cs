using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "HasTarget", story: "[Agent] has possible [Target]", category: "Conditions", id: "c230fe2f6a5ba5d8cab5912f49acaf29")]
public partial class HasTargetCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Agent;
    [SerializeReference] public BlackboardVariable<Transform> Target;
    
    EnemyTargeting targeting;

    public override bool IsTrue()
    {
        if (targeting.CurrentTarget != null)
            return true;
        return false;
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
