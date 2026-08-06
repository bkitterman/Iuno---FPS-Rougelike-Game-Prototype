using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GrenadePrefab : MonoBehaviour
{
    private float explosionDamage;
    private float hitDamage;
    private float radius;
    private StatsController playerStats;
    [SerializeField] private GameObject explosion;

    public void Initialize(float explosionDamage, float projectileDamage, float rad, float delay)
    {
        this.explosionDamage = explosionDamage;
        hitDamage = projectileDamage;
        radius = rad;
        StartCoroutine(ExplodeTimer(delay));
    }

    private IEnumerator ExplodeTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    List<Hitbox> hit = new List<Hitbox>();
    void OnCollisionEnter(Collision collision)
    {
        Hitbox enemy = collision.gameObject.GetComponentInChildren<Hitbox>();
        if (enemy != null && hit.Contains(enemy) == false) 
        {
            enemy.TakeDamage(this.gameObject.transform,hitDamage, Vector3.zero, false, true);
            hit.Add(enemy);
        }
        
    }

    void OnCollisionStay(Collision collision)
    {
        // This function is called continuously while the object is touching another collider.
    }

    void OnCollisionExit(Collision collision)
    {
        // This function is called when the object stops touching another collider.
    }

    private void Explode()
    {
        // 1. Find all colliders in the radius
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);

        // 2. Damage all enemies
        List<EnemyHealth> targets = new List<EnemyHealth>();
        foreach (Collider hit in hits)
        {
            Hitbox enemy = hit.GetComponent<Hitbox>();
            if (enemy == null) // In case collider is not enemy
            {
                continue;
            }

            EnemyHealth target = enemy.TargetHealth;
            if (targets.Contains(target)) 
            {
                continue;
            }

            targets.Add(target);

            if (enemy != null)
            {
                // TODO: Add falloff calculation here
                float proximity = (transform.position - hit.transform.position).magnitude;
                float falloff = 1f - (proximity / radius);
                float finalDamage = explosionDamage * falloff;

                enemy.TakeDamage(this.gameObject.transform,finalDamage, Vector3.zero, true, false); 
            }
        }

        // 3. Spawn explosion VFX/SFX
        if (explosion != null)
        {
            // Create the explosion at the grenade's current position and with no rotation
            GameObject vfx = Instantiate(explosion, transform.position, Quaternion.identity);

            // Pro-tip: Destroy the explosion effect after 5 seconds
            Destroy(vfx, 5f);
        }

        // 4. Clean up
        Destroy(gameObject);
    }
}
