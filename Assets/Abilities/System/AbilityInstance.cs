using UnityEngine;

public interface AbilityInstance
{
    // A property to hold the static data (cooldown, name, etc.)
    AbilityData Data { get; set; }

    // A property to check if the ability is on cooldown
    bool IsOnCooldown { get; }

    // A property to get the remaining cooldown (for UI)
    float CooldownRemaining { get; }

    int CurrentCharges { get; }

    /// <summary>
    /// Called every frame by ability manager
    /// </summary>
    void AbilityUpdate();

    /// <summary>
    /// Called when the ability is first equipped to the player.
    /// Use this to get references (like PlayerMovement, StatsController).
    /// </summary>
    void OnEquip(GameObject owner);

    /// <summary>
    /// Called when the player presses the ability button.
    /// This is where the "try to use" logic goes (check cooldown, etc.).
    /// </summary>
    void TryActivate();

    /// <summary>
    /// Called when the ability is unequipped.
    /// </summary>
    void OnUnequip();
}
