using UnityEngine;

public class DamageOverTimeEffect : StatusEffect
{
    private DamageOverTimeEffectData _data;

    public float DamagePerTick;
    public float TickInterval;
    private float _timeUntilNextTick;
    private EnemyHealth _enemyHealth;
    private Transform position;

    // Constructor to set values
    public DamageOverTimeEffect(DamageOverTimeEffectData data)
    {
        _data = data;
        Duration = this.TimeRemaining = _data.Duration;
        DamagePerTick = _data.Damage;
        TickInterval = _data.Interval;
        _timeUntilNextTick = TickInterval;
    }

    public override void ApplyEffect(GameObject target)
    {
        _enemyHealth = target.GetComponent<EnemyHealth>();
        position = target.GetComponent<Transform>();
    }

    public override void RemoveEffect(GameObject target) { /* Nothing to clean up */ }

    public override StatusEffectData GetData() { return _data; }

    public override void Tick(float deltaTime)
    {
        base.Tick(deltaTime); 

        _timeUntilNextTick -= deltaTime;
        if (_timeUntilNextTick <= 0)
        {
            if (_enemyHealth != null) _enemyHealth.TakeDamage(null, DamagePerTick, false, Vector3.zero, false, true);
            _timeUntilNextTick = TickInterval;
        }
    }
}