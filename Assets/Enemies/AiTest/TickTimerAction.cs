using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "TickTimer", story: "Tick [timer]", category: "Helpers", id: "b9bf64f1428bb4793ccadbfd3125ad41")]
public partial class TickTimerAction : Action
{
    [SerializeReference] public BlackboardVariable<float> Timer;

    protected override Status OnStart()
    {
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

