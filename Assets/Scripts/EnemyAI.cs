using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Combat")]
    public GameObject projectilePrefab; 
    public Transform firePoint;        
    public float fireRate = 2f; // In Seconds

    private float _fireTimer = 0f;      // Timer to track cooldown
    private Transform _playerTransform; // Reference to the player

    void Start()
    {
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void Update()
    {
        if (_playerTransform == null) return;

        // Point the enemy towards the player
        // transform.LookAt(_playerTransform);

        // Tick down the fire timer
        _fireTimer -= Time.deltaTime;

        if (_fireTimer <= 0f)
        {
            Shoot();
            _fireTimer = fireRate;
        }
    }

    void Shoot()
    {
        var shot = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation, this.gameObject.transform);
        EnemyProjectile projScript = shot.GetComponent<EnemyProjectile>();
        projScript.Initialize(5f, 10f, this.gameObject);
    }
}