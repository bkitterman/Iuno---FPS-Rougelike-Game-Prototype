using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private StatusEffectData onHitEffect;
    private float damage;
    private GameObject sourceEnemy;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
    }

    public void Initialize(float dmg, float speed, GameObject source)
    {
        damage = dmg;
        sourceEnemy = source;
        rb.linearVelocity = transform.forward * speed;

        // Destroy after a few seconds to clean up misses
        Destroy(gameObject, 5f);
    }

    void OnTriggerEnter(Collider other)
    {
        // Don't hit the enemy that fired it
        if (sourceEnemy.gameObject != null) // Ensure enemy is still alive
        {
            if (other.gameObject == sourceEnemy || other.transform.IsChildOf(sourceEnemy.transform))
            {
                return;
            }
        }
        // Check if enemy hit the player
        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(sourceEnemy, this.gameObject.transform.position, damage);
            
            if(onHitEffect != null)
            {
                StatusEffectManager statusManager = other.GetComponent<StatusEffectManager>();
                statusManager.ApplyEffect(onHitEffect);
            }

            // Destroy on hit
            Destroy(this.gameObject);
            return;
        }

        // Destroy projectile if it hits any other object that is not a trigger
        if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}