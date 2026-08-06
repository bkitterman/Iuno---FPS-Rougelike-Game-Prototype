using UnityEngine;
using System.Collections.Generic;

public class EnemyAbilityHandler : MonoBehaviour
{
    // --- State ---
    private List<AbilityInstance> availableAbilities = new List<AbilityInstance>();

    // --- Component References ---
    private EnemyBrain brain;

    void Awake()
    {
        brain = GetComponent<EnemyBrain>();
    }

    /// <summary>
    /// Called by EnemyBrain during initialization. Creates ability instances.
    /// </summary>
    public void Initialize(EnemyData data)
    {
        // Clear old abilities if re-initializing
        availableAbilities.Clear();

        if (data.availableAbilities != null)
        {
            foreach (AbilityData abilityData in data.availableAbilities)
            {
                if (abilityData == null) continue;

                // Create a runtime instance to hold the logic and state (cooldown)
                AbilityInstance newAbilityInstance = CreateAbilityInstance(abilityData);
                if (newAbilityInstance != null)
                {
                    availableAbilities.Add(newAbilityInstance);
                }
            }
        }
    }

    /// <summary>
    /// Instantiates the logic for a specific ability based on its AbilityData.
    /// </summary>
    private AbilityInstance CreateAbilityInstance(AbilityData data)
    {
        // Get the class name from the Scriptable Object
        System.Type abilityType = System.Type.GetType(data.ClassName);

        if (abilityType != null && typeof(AbilityInstance).IsAssignableFrom(abilityType))
        {
            // Create an instance of the ability logic
            AbilityInstance abilityLogic = (AbilityInstance)System.Activator.CreateInstance(abilityType);
            abilityLogic.Data = data;

            abilityLogic.OnEquip(this.gameObject); // Pass the enemy GameObject as owner
            return abilityLogic;
        }
        else
        {
            Debug.LogError($"Ability class not found or doesn't implement IAbility: {data.ClassName}", this);
            return null;
        }
    }

    void Update()
    {
        // Tick cooldowns for all available abilities
        foreach (AbilityInstance ability in availableAbilities)
        {
            ability.AbilityUpdate();
        }
    }

    /// <summary>
    /// Attempts to use an ability by its index in the available list.
    /// Called by the EnemyBrain or Behavior Tree.
    /// </summary>
    public bool TryUseAbility(int index, Transform target)
    {
        if (index < 0 || index >= availableAbilities.Count)
        {
            Debug.LogWarning($"Tried to use invalid ability index: {index} with max of {availableAbilities.Count}", this);
            return false; // Invalid index
        }

        AbilityInstance ability = availableAbilities[index];

        if (ability.IsOnCooldown == false)
        {
            ability.TryActivate();

            //Debug.Log($"Enemy used ability: {ability.Data.AbilityName}", this);
            return true; // Successfully used
        }

        return false; // On cooldown
    }
}