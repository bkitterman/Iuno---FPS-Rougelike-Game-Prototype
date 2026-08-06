using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

public class StatusEffectUIManager : MonoBehaviour
{
    [SerializeField] private GameObject statusEffectPrefab;
    [SerializeField] private StatusEffectManager manager;
    [SerializeField] private Transform uiPanel;

    private List<StatusEffectUIHolder> activePrefabsList = new();

    public int UIEffectCount = 0;

    void Update()
    {
        foreach (var item in activePrefabsList)
        {
            item.SetText(item.Effect.GetData().Name, item.Effect.TimeRemaining);
        }
    }

    public void AddStatusEffect(StatusEffect effect)
    {
        StatusEffectUIHolder temp = activePrefabsList.FirstOrDefault(p => p.Effect == effect);
        
        if (temp != default)
        {
            temp.SetText(effect.GetData().Name, effect.TimeRemaining);
            temp.Effect = effect;
            return;
        }


        GameObject newUI = Instantiate(statusEffectPrefab, uiPanel);
        StatusEffectUIHolder holder = newUI.GetComponent<StatusEffectUIHolder>();

        holder.Effect = effect;
        holder.Icon.sprite = effect.GetData().Icon;
        holder.Priority = effect.GetData().Priority;
        holder.SetText(effect.GetData().Name, effect.TimeRemaining);

        holder.SetBuff(effect.GetData().IsBuff);

        activePrefabsList.Add(holder);

        UIEffectCount++;
    }

    public void RearrangeStatusEffectsUI()
    {
        List<StatusEffect> allEffects = manager.GetActiveEffects().OrderBy(e => e.GetData().Priority).ToList();

        List<StatusEffect> replaceEffects = new();
        List<StatusEffectUIHolder> replaceables = new();

        // Check if first 6 effects are already displayed, collect missing
        for (int i = 0; i < 6; i++)
        {
            var effect = allEffects[i];
            int index = activePrefabsList.FindIndex(p => p.Effect == effect);
            if (index != -1)
            {
                replaceEffects.Add(effect);
            }
        }

        // Clear replaced UI slots
        for (int i = 0;i < 6; i++)
        {
            var uiHolder = activePrefabsList[i];
            if (!replaceEffects.Contains(uiHolder.Effect))
            {
                activePrefabsList.RemoveAt(i);
                Destroy(uiHolder.gameObject);
                UIEffectCount--;
            }
        }

        // Add missing UI slots
        foreach (var effect in replaceEffects)
        {
            AddStatusEffect(effect);
        }

    }

    public void RemoveStatusEffect(StatusEffect effect)
    {
        if (effect == null) return;
        foreach (var holder in activePrefabsList) 
        { 
            if (holder.Effect == effect) 
            { 
                Destroy(holder.gameObject); activePrefabsList.Remove(holder);
                break;
            }
        }

        UIEffectCount--;

        if (UIEffectCount == 4)
        {
            List<StatusEffect> allEffects = manager.GetActiveEffects().OrderBy(e => e.GetData().Priority).ToList();
               
            foreach (var lastEffect in allEffects)
            {
                if(activePrefabsList.Any(p => p.Effect != lastEffect)) 
                    AddStatusEffect(lastEffect);

            }
        }
    }
}
