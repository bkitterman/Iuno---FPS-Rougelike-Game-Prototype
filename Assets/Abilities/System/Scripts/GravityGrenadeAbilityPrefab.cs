using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GravityGrenadePrefab : MonoBehaviour
{
    private float explosionDamage;
    private float pullDamage;
    private float pullRadius;
    private float pullForce;
    private float pullDuration;
    private bool exploded = false;
    private float damageTime = 0.5f;
    [SerializeField] private GameObject explosion;

    public void Initialize(float explosionDamage, float pullDamage, float radius, float force, float duration)
    {
        this.explosionDamage = explosionDamage;
        this.pullDamage = pullDamage;
        pullRadius = radius;
        pullForce = force;
        pullDuration = duration;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Explode on first impact
        if (!exploded)
        {
            exploded = true;
            Explode();
        }
    }

    private void Explode()
    {
        List<EnemyHealth> hitTargets = new List<EnemyHealth>();
        Collider[] hits = Physics.OverlapSphere(transform.position, pullRadius);
        foreach (Collider hit in hits)
        {
            Hitbox enemy = hit.GetComponent<Hitbox>();
            if (enemy != null && hitTargets.Contains(enemy.TargetHealth) == false)
            {
                hitTargets.Add(enemy.TargetHealth);
                enemy.TakeDamage(this.gameObject.transform, explosionDamage, Vector3.zero, true, false);
            }
        }

        // Start the pull Coroutine
        StartCoroutine(PullEnemies());

        // 3. Spawn explosion VFX/SFX
        if (explosion != null)
        {
            // Create the explosion at the grenade's current position and with no rotation
            GameObject vfx = Instantiate(explosion, transform.position, Quaternion.identity);

            Destroy(vfx, pullDuration);
        }

        // Disable the grenade's visuals/collider but keep the object for the pull duration
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        GetComponent<Rigidbody>().isKinematic = true; // Stop it from moving

        // Destroy after the pull finishes
    }

    private IEnumerator PullEnemies()
    {
        float pullTimer = pullDuration; // Timer for the overall duration
        float damageTickTimer = 0f;    // Timer specifically for damage ticks

        List<EnemyHealth> hitTargets = new List<EnemyHealth>();
        while (pullTimer > 0f)
        {
            pullTimer -= Time.deltaTime;
            damageTickTimer -= Time.deltaTime;

            // Check if it's time for a damage tick
            bool applyDamageThisFrame = false;
            if (damageTickTimer <= 0f)
            {
                applyDamageThisFrame = true;
                damageTickTimer = damageTime; // Reset the tick timer
            }

            Collider[] hits = Physics.OverlapSphere(transform.position, pullRadius);

            if (applyDamageThisFrame) hitTargets.Clear();

            foreach (Collider hit in hits)
            {
                Rigidbody enemyRb = hit.GetComponentInParent<Rigidbody>();
                if (enemyRb != null)
                {
                    // Calculate direction towards the grenade center
                    Vector3 direction = (transform.position - hit.transform.position).normalized;

                    // Apply continuous force using Acceleration (ignores mass)
                    enemyRb.AddForce(direction * pullForce, ForceMode.Acceleration);

                    // Apply damage if the tick timer is ready
                    if (applyDamageThisFrame)
                    {
                        Hitbox enemy = hit.GetComponent<Hitbox>();
                        if (enemy != null && hitTargets.Contains(enemy.TargetHealth) == false)
                        {
                            enemy.TakeDamage(this.gameObject.transform, pullDamage, Vector3.zero, false, false);
                            hitTargets.Add(enemy.TargetHealth);
                        }
                    }
                }
            }

            yield return null; // Wait for the next frame
        }

    }
}