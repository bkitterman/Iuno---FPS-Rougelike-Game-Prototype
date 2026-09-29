using UnityEngine;

using System.Collections;
using System.Collections.Generic;

public class PlayerMelee : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform camTransform; 
    [SerializeField] private StatsController statsController;
    [SerializeField] private Transform weaponHolder; 

    [Header("Settings")]
    [SerializeField] private float meleeRange = 2f;
    [SerializeField] private float meleeRadius = 0.5f;
    [SerializeField] private float meleeCooldown = 0.8f;

    private float _cooldownTimer = 0f;

    void Update()
    {
        // Tick down the cooldown timer
        if (_cooldownTimer > 0)
        {
            _cooldownTimer -= Time.deltaTime;
        }
    }

    public void TryMelee()
    {
        if (_cooldownTimer > 0)
        {
            return; // Can't punch yet, on cooldown
        }

        // --- Hit Detection Logic will go here ---
        DetectAndDamage();

        // --- Visual Feedback Logic will go here ---
        StartCoroutine(PunchAnimation());

        // Reset the cooldown
        _cooldownTimer = meleeCooldown;
    }

    private void DetectAndDamage()
    {
        // A list to track enemies player has already hit in this single punch
        List<GameObject> hitEnemies = new List<GameObject>();

        // Fire a sphere forward from the camera's position
        RaycastHit[] hits = Physics.SphereCastAll(camTransform.position, meleeRadius, camTransform.forward, meleeRange);

        foreach (RaycastHit hit in hits)
        {
            // Check if player hit an enemy and haven't already hit it in this swing
            Hitbox enemyHealth = hit.collider.GetComponent<Hitbox>();
            if (enemyHealth != null && !hitEnemies.Contains(enemyHealth.transform.parent.gameObject))
            {
                // Get the player's current damage from the stats system
                float damage = statsController.WeaponDamage.GetValue() * statsController.AllDamageMultiplier.GetValue();

                enemyHealth.TakeDamage(this.gameObject.transform,damage, hit.point, true, false);

                GameEvents.ReportMeleeDamageDealt(enemyHealth.transform.parent.gameObject, damage, camTransform.forward);

                // Add to the list to prevent hitting the same enemy multiple times
                hitEnemies.Add(enemyHealth.transform.parent.gameObject);
            }
        }
    }

    private IEnumerator PunchAnimation()
    {
        Vector3 originalPosition = weaponHolder.localPosition;
        Vector3 punchPosition = originalPosition + Vector3.forward * 0.5f; // Punch forward
        float punchDuration = 0.1f;
        float returnDuration = 0.2f;

        // Animate forward
        float elapsedTime = 0f;
        while (elapsedTime < punchDuration)
        {
            weaponHolder.localPosition = Vector3.Lerp(originalPosition, punchPosition, elapsedTime / punchDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        weaponHolder.localPosition = punchPosition;

        // Animate back
        elapsedTime = 0f;
        while (elapsedTime < returnDuration)
        {
            weaponHolder.localPosition = Vector3.Lerp(punchPosition, originalPosition, elapsedTime / returnDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        weaponHolder.localPosition = originalPosition;
    }
}