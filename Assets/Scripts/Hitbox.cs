using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [Header("Damage Multiplier")]
    public float damageMultiplier = 1f;
    public bool isPrecision = false;
    public EnemyHealth TargetHealth;
    public CrosshairController crosshair;

    /// <summary>
    /// Deal damage to the hitbox's health target, and flash the hitmarker of the player.
    ///
    /// </summary>
    /// <param name="baseDamage">The amount of damage done, prior to multiplier. </param>
    /// <param name="hitPoint">The hit point, used to pass to take damage in order to spawn damage numbers.</param>
    /// <param name="onHitEnabled">True if On Hit Effects can be triggered</param>
    /// <param name="canCrit">Defaults to true, specifies if it can crit.</param>
    public void TakeDamage(Transform source,float baseDamage, Vector3 hitPoint, bool onHitEnabled, bool canCrit = true)
    {
        TargetHealth.TakeDamage(
            source,
            canCrit ? baseDamage * damageMultiplier : baseDamage, 
            canCrit ? isPrecision : false, 
            hitPoint, 
            onHitEnabled);
    }
}
