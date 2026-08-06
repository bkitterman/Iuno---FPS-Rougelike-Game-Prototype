using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class StatusEffectManager : MonoBehaviour
{
    private List<StatusEffect> _activeEffects = new List<StatusEffect>();
    [SerializeField] private StatusEffectUIManager uIManager;

    public void ApplyEffect(StatusEffectData data)
    {
        // Check if an effect from the same data source already exists
        var existingEffect = _activeEffects.FirstOrDefault(e => e.GetData() == data); 

        if (existingEffect != null)
        {
            if (data.StackingBehavior == StackingType.RefreshDuration)
            {
                existingEffect.TimeRemaining = data.Duration;
                return; 
            }
            if (data.StackingBehavior == StackingType.DoNotStack)
            {
                return;
            }
        }

        // --- Create and Add New Effect ---
        StatusEffect newEffect = data.CreateEffectInstance();
        newEffect.ApplyEffect(this.gameObject);
        _activeEffects.Add(newEffect);

        if(uIManager != null)
        {
            // --- Update UI ---
            if (uIManager.UIEffectCount == 5) uIManager.RearrangeStatusEffectsUI();
            else uIManager.AddStatusEffect(newEffect);
        }
    }

    void Update()
    {
        // Iterate backwards to safely remove items
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.Tick(Time.deltaTime);

            if (effect.TimeRemaining <= 0)
            {
                effect.RemoveEffect(this.gameObject);
                _activeEffects.RemoveAt(i);
                if (uIManager != null)
                {
                    // --- Update UI ---
                    uIManager.RemoveStatusEffect(effect) ;
                }
            }
        }
    }

    public bool HasEffect(StatusEffectData data)
    {
        return (_activeEffects.FirstOrDefault(effect => effect.GetData() == data) != null);
    }

    public List<StatusEffect> GetActiveEffects() { return _activeEffects; }
    
    public int GetStackCount(StatusEffectData data)
    {
        int count = 0;

        for (int i = 0; i < _activeEffects.Count; i++)
        {
            var effect = _activeEffects[i];
            if (effect.GetData() == data) count++;
        }

        return count;
    }
}