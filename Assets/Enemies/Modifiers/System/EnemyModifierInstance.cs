using UnityEngine;

public interface EnemyModifierInstance
{
    /// <summary>
    // Called once by the EnemyModifierManager when this modifier is first applied.
    /// </summary>
    void OnApply(EnemyBrain brain, EnemyModifierManager manager);

    /// <summary>
    // Called every frame by the EnemyModifierManager.
    // Use for ongoing behaviors (like Stealth or Teleporting).
    /// </summary>
    void Update();

    /// <summary>
    // A hook called by the enemy's health script when it takes damage.
    // Use for Body Armor, Mirror Coated, etc.
    /// </summary>
    /// <param name="damageAmount">The incoming damage</param>
    /// <returns>The *modified* damage amount</returns>
    float OnTakeDamage(float damageAmount, Vector3 damageSource);

    /// <summary>
    // A hook called by the enemy's weapon handler when it deals damage.
    // Use for Backstabber, Diseased, etc.
    /// </summary>
    void OnDealDamage(PlayerHealth player, float damage);

    /// <summary>
    // Called when the enemy is destroyed, to clean up.
    /// </summary>
    void OnRemove();
}