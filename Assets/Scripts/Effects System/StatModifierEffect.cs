using UnityEngine;

public class StatModifierEffect : StatusEffect
{
    private StatModifierEffectData _data;
    private Stat _statToModify;
    private StatsController _targetStats;
    private StatModifier _modifier;

    public StatModifierEffect(StatModifierEffectData data)
    {
        _data = data;
        Duration = TimeRemaining = data.Duration;
    }

    public override void ApplyEffect(GameObject target)
    {
        _targetStats = target.GetComponent<StatsController>();
        if (_targetStats != null)
        {
            _modifier = new StatModifier(_data.ModifierValue, _data.ModifierType, this);

            switch (_data.ModifierStat) 
            {
                case Stat.Defense:
                    _targetStats.Defense.AddModifier(_modifier);
                    break;
                case Stat.AmmoConsumptionChance:
                    _targetStats.AmmoConsumptionChance.AddModifier(_modifier);
                    break;
                case Stat.ReloadSpeed:
                    _targetStats.ReloadSpeed.AddModifier(_modifier);
                    break;
                default:
                    Debug.LogWarning($"Stat {_data.ModifierStat} is not supported by StatModifierEffect.", target);
                    break;
            }
        } else
        {
            Debug.LogWarning("Target does not have StatsController component. StatModifierEffect cannot be applied.", target); ;
        }
    }

    public override StatusEffectData GetData()
    {
        return _data;
    }

    public override void RemoveEffect(GameObject target)
    {
        if (_targetStats != null)
        {
            switch (_data.ModifierStat)
            {
                case Stat.Defense:
                    _targetStats.Defense.RemoveModifier(_modifier);
                    break;
                case Stat.AmmoConsumptionChance:
                    Debug.Log("Ammo consumptiion removed");
                    _targetStats.AmmoConsumptionChance.RemoveModifier(_modifier);
                    break;
                case Stat.ReloadSpeed:
                    Debug.Log("Reload speed removed");
                    _targetStats.ReloadSpeed.RemoveModifier(_modifier);
                    break;
            }
        }
    }
}