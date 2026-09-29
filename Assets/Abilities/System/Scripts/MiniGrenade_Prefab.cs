using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGrenade_Prefab : MonoBehaviour
{
    private float explosionDamage;
    private float radius;
    [SerializeField] private GameObject explosion;

    public void Initialize(float dmg, float rad, float delay)
    {
        explosionDamage = dmg;
        radius = rad;
        StartCoroutine(ExplodeTimer(delay));
    }

    private IEnumerator ExplodeTimer(float delay)
    {
        yield return new WaitForSeconds(delay);
        Explode();
    }

    private void Explode()
    {
        List<EnemyHealth> hitTargets = new List<EnemyHealth>();
        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        foreach (Collider hit in hits)
        {
            Hitbox enemy = hit.GetComponent<Hitbox>();
            if (enemy != null && hitTargets.Contains(enemy.TargetHealth) == false)
            {
                hitTargets.Add(enemy.TargetHealth);
                enemy.TakeDamage(this.gameObject.transform,explosionDamage, Vector3.zero, true, false);
            }
        }

        // 3. Spawn explosion VFX/SFX
        if (explosion != null)
        {
            // Create the explosion at the grenade's current position and with no rotation
            GameObject vfx = Instantiate(explosion, transform.position, Quaternion.identity);

            Destroy(vfx, 5f);
        }

        Destroy(gameObject);
    }
}