using UnityEngine;

public abstract class StatusEffect
{
    public float Duration;
    public float TimeRemaining;

    // Called once when the effect is first applied
    public abstract void ApplyEffect(GameObject target);

    // Called once when the effect expires
    public abstract void RemoveEffect(GameObject target);

    public abstract StatusEffectData GetData();

    // Called every frame by the manager
    public virtual void Tick(float deltaTime)
    {
        TimeRemaining -= deltaTime;
    }
}