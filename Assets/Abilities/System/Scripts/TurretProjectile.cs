using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    private float speed;
    private float damage;
    private GameObject source;

    public void Init(GameObject source,float damage, float speed)
    {
        this.source = source;
        this.speed = speed;
        this.damage = damage;
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        Hitbox enemy = other.gameObject.GetComponentInChildren<Hitbox>();
        if (enemy != null)
        {
            
            enemy.TakeDamage((source != null ? source.transform : null), damage, Vector3.zero, true, false);
        }

        // Destroy the projectile after it hits something
        Destroy(gameObject);
    }

    public void SetSource(GameObject source) { this.source = source; }
}
