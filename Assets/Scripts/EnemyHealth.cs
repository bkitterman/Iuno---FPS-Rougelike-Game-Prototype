using System;
using System.Collections;


using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    private float maxHealth;
    private float currentHealth;

    [SerializeField] private HealthBar healthBarPrefab;
    [SerializeField] private bool canDie = true;
    public Transform Center;
    private HealthBar healthBarInstance;
    private StatsController _statsController;
    private EnemyModifierManager modifierManager;
    private EnemyBrain brain;

    void Awake()
    {
        _statsController = GetComponent<StatsController>();
        modifierManager = GetComponent<EnemyModifierManager>();
        brain = GetComponent<EnemyBrain>();
    }

    void Start()
    {
        maxHealth = _statsController.MaxHealth.GetValue();
        currentHealth = maxHealth;

        if (healthBarPrefab != null)
        {
            // Spawn bar slightly above head
            Vector3 offset = new Vector3(0, 2f, 0);
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + offset, Quaternion.identity, transform);
            healthBarInstance.SetHealth(currentHealth, maxHealth);
        }
    }

    void FixedUpdate()
    {
        var temp = _statsController.MaxHealth.GetValue();
        if( temp != maxHealth)
        {
            currentHealth += temp - maxHealth;
            maxHealth = temp;
        }
    }

    /// <summary>
    /// Deal damage to this target, and flash the damage number of damage dealt.
    /// 
    /// </summary>
    /// <param name="amount">The amount of damage to take. </param>
    /// <param name="precision">True if hit was precise. </param>
    /// <param name="hitPoint">The place the hit occured, used to spawn damage number. </param>
    public void TakeDamage(Transform source, float amount, bool isPrecision, Vector3 hitPoint, bool onHitEnabled = false, bool isDoT = false, bool isMelee = false)
    {
        float finalDamage = amount / _statsController.Defense.GetValue();

        currentHealth = Mathf.Max(currentHealth -= finalDamage, 0);
        healthBarInstance.SetHealth(currentHealth, maxHealth);

        if(isDoT || hitPoint == Vector3.zero)
            DamagePopupManager.Instance.CreatePopup(Center, amount, isPrecision);
        else 
            DamagePopupManager.Instance.CreatePopup(hitPoint, amount, isPrecision);

        if(modifierManager != null)
            amount = modifierManager.ProcessOnTakeDamage(amount, hitPoint);

        GameEvents.ReportDamageDealt(this.gameObject, finalDamage, isPrecision, onHitEnabled);
        ReportDamage(source);
        if (currentHealth <= 0f)
        {
            // Die();
            if (canDie)
                Die(isMelee);
            else
                DevDie();
        }

    }

    /// <summary>
    /// Destroy this object.
    /// 
    /// Dev mode is now so its just going to invis the enemy for a second
    /// </summary>
    void Die(bool isMelee)
    {
        GameEvents.ReportEnemyKilled(this.gameObject);
        brain.ReportDeath(brain);

        LootTableData table = GetComponent<EnemyBrain>().GetData().LootTable;
        LootManager.Instance.ProcessLootDrop(transform.position, table);
        AmmoManager.Instance.ProcessAmmoDrop(transform.position, brain.EnemyData.AmmoDropMultiplier, isMelee);

        Destroy(healthBarInstance);
        Destroy(gameObject);
    }

    void DevDie()
    {
        GameEvents.ReportEnemyKilled(this.gameObject);
        SystemLog.Instance.PostMessage("Killed Dummie.");
        currentHealth = maxHealth;
        healthBarInstance.SetHealth(currentHealth, maxHealth);
    }

    public event Action<Transform> OnDamage;
    public void ReportDamage(Transform source)
    {
        OnDamage?.Invoke(source);
    }
}
