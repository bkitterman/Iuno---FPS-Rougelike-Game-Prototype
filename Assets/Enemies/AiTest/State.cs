using System;
using Unity.Behavior;

[BlackboardEnum]
public enum State
{
    Idle,
	Patrolling,
	Searching,
	Chasing,
	Attacking,
}
