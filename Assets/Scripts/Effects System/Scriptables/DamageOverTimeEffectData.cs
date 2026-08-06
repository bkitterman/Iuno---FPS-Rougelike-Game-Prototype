using UnityEngine;

[CreateAssetMenu(fileName = "DoT Effect Data", menuName = "Status Effects/DoT Effect Data")]
public class DamageOverTimeEffectData : StatusEffectData
{
    [Header("DoT Properties")]
    public float Damage;
    public float Interval;

    public override StatusEffect CreateEffectInstance()
    {
        return new DamageOverTimeEffect(this);
    }
}