using UnityEngine;

[CreateAssetMenu(fileName = "Stat Modifier Data", menuName = "Status Effects/Stat Modifier Data")]
public class StatModifierEffectData : StatusEffectData
{
    [Header("Stat Modifier Properties")]
    public Stat ModifierStat;
    public float ModifierValue;
    public StatModType ModifierType;

    public override StatusEffect CreateEffectInstance()
    {
        return new StatModifierEffect(this);
    }
}