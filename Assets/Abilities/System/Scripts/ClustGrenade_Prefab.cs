using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClusterGrenade_Prefab : MonoBehaviour
{
    private ClusterGrenadeAbilityData data;
    private float explosionDamage;
    private float secondaryDamage;
    [SerializeField] private GameObject explosion;

    public void Initialize(ClusterGrenadeAbilityData clusterData, float damage, float SecondaryDamage)
    {
        data = clusterData;
        StartCoroutine(ClusterTimer());
        secondaryDamage = SecondaryDamage;
        explosionDamage = damage;
    }

    private IEnumerator ClusterTimer()
    {
        yield return new WaitForSeconds(data.initialDelay);
        SpawnSubmunitions();
    }

    private void SpawnSubmunitions()
    {
        List<EnemyHealth> hitTargets = new List<EnemyHealth>();
        Collider[] hits = Physics.OverlapSphere(transform.position, data.initialRadius);
        foreach (Collider hit in hits)
        {
            Hitbox enemy = hit.GetComponent<Hitbox>();
            if (enemy != null && hitTargets.Contains(enemy.TargetHealth) == false)
            {
                hitTargets.Add(enemy.TargetHealth);
                enemy.TakeDamage(this.gameObject.transform,explosionDamage, Vector3.zero, true, false);
            }
        }


        for (int i = 0; i < data.submunitionCount; i++)
        {
            // Spawn a submunition slightly offset from the center
            Vector3 spawnPos = transform.position + Random.insideUnitSphere * 0.1f;
            GameObject subObj = Instantiate(data.submunitionPrefab, spawnPos, Random.rotation);

            // Initialize the submunition
            MiniGrenade_Prefab subLogic = subObj.GetComponent<MiniGrenade_Prefab>();
            subLogic.Initialize(secondaryDamage, data.submunitionRadius, data.submunitionDelay);

            // Add a small force to spread them out
            Rigidbody subRb = subObj.GetComponent<Rigidbody>();
            if (subRb != null)
            {
                Vector3 spreadDir = (subObj.transform.position - transform.position).normalized;
                subRb.AddForce(spreadDir * data.submunitionSpreadForce, ForceMode.Impulse);

                // Inherit some velocity from the parent grenade
                Rigidbody parentRb = GetComponent<Rigidbody>();
                if (parentRb != null)
                {
                    subRb.linearVelocity += parentRb.linearVelocity * 0.5f;
                }
            }
        }

        // Clean up the main grenade
        // 3. Spawn explosion VFX/SFX
        if (explosion != null)
        {
            // Create the explosion at the grenade's current position and with no rotation
            GameObject vfx = Instantiate(explosion, transform.position, Quaternion.identity);

            // Destroy the explosion effect after 5 seconds
            Destroy(vfx, 5f);
        }
        Destroy(gameObject);
    }
}