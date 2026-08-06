using UnityEngine;
using System.Collections.Generic;

public class AbilityManager : MonoBehaviour
{
    [Header("Ability Loadout")]
    [SerializeField] private List<AbilityData> equippedAbilityData;

    public AbilityInstance[] ActiveAbilities = new AbilityInstance[3];

    void Start()
    {
        EquipAbilities();
    }

    void Update()
    {
        for (int i = 0; i < ActiveAbilities.Length; i++)
        {
            if (ActiveAbilities[i] != null)
            {
                ActiveAbilities[i].AbilityUpdate();
            }
        }
    }

    void EquipAbilities()
    {
        for (int i = 0; i < equippedAbilityData.Count && i < 3; i++)
        {
            AbilityData data = equippedAbilityData[i];
            if (data == null) continue;

            System.Type abilityType = System.Type.GetType(data.ClassName);

            if (abilityType != null)
            {
                // Create an instance of the ability logic
                AbilityInstance newAbility = (AbilityInstance)System.Activator.CreateInstance(abilityType);
                newAbility.Data = data;
                newAbility.OnEquip(this.gameObject); // Pass the player (owner)

                // Add it to our active array
                ActiveAbilities[i] = newAbility;
            }
            else
            {
                Debug.LogError($"Ability class not found: {data.AbilityName}");
            }
        }
    }

    /// <summary>
    /// Swaps an equipped ability at a specific slot for a new one.
    /// </summary>
    public void SwapAbility(int slotIndex, AbilityData newData)
    {
        if (slotIndex < 0 || slotIndex >= ActiveAbilities.Length) return;

        // --- 1. Unequip and Destroy the Old Ability ---
        if (ActiveAbilities[slotIndex] != null)
        {
            ActiveAbilities[slotIndex].OnUnequip();
            ActiveAbilities[slotIndex] = null;
        }

        // --- 2. Equip and Create the New Ability ---
        if (newData == null) return;

        System.Type abilityType = System.Type.GetType(newData.ClassName);

        if (abilityType != null)
        {
            AbilityInstance newAbility = (AbilityInstance)System.Activator.CreateInstance(abilityType);
            newAbility.Data = newData;
            newAbility.OnEquip(this.gameObject);

            ActiveAbilities[slotIndex] = newAbility;
        }
        else
        {
            Debug.LogError($"Ability class not found: {newData.ClassName}");
        }

        GameEvents.ReportAbilityEquipped(newData);
    }

    public void OnAbility1Pressed()
    {
        if (ActiveAbilities[0] != null)
        {
            ActiveAbilities[0].TryActivate();
        }
    }

    public void OnAbility2Pressed()
    {
        if (ActiveAbilities[1] != null)
        {
            ActiveAbilities[1].TryActivate();
        }
    }

    public void OnAbility3Pressed()
    {
        if (ActiveAbilities[2] != null)
        {
            ActiveAbilities[2].TryActivate();
        }
    }

    
}