using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealth : MonoBehaviour
{
    // Public States
    public TextMeshProUGUI text;
    public Player player;
    [SerializeField] private float lerpSpeed = 5f;

    // Private States
    private float currentHealth;
    private float targetValue;
    private float maxHealth;

    // Objects
    public Slider healthSlider;

    void Start()
    {
        maxHealth = player.Stats.MaxHealth.GetValue();
        currentHealth = maxHealth;

        targetValue = 1f;
        healthSlider.value = 1f;

        updateUI(); 
    }

    public void TakeDamage(GameObject source, Vector3 hitPoint, float amount)
    {
        if (Random.value < player.Stats.DodgeChance.GetValue())
        {
            SystemLog.Instance.PostMessage("Dodged!");
            return;
        }

        amount = amount / player.Stats.Defense.GetValue();

        GameEvents.ReportPlayerDamageTaken(source, hitPoint, amount);

        currentHealth -= amount;

        updateUI();

        if (currentHealth <= 0f) { Die(); }
    }

    public void Heal(float amount)
    {
        currentHealth += amount;
        if(currentHealth > maxHealth) currentHealth = maxHealth;
        updateUI();
    }

    private void Update()
    {
        if (!Mathf.Approximately(healthSlider.value, targetValue))
        {
            healthSlider.value = Mathf.Lerp(healthSlider.value, targetValue, Time.deltaTime * lerpSpeed);
        }

        float newHealth = player.Stats.MaxHealth.GetValue();
        if (newHealth == maxHealth)
            return;

        currentHealth += (newHealth - maxHealth);
        maxHealth = newHealth;

        targetValue = Mathf.Clamp01(currentHealth / maxHealth);
        updateUI();
    }

    void updateUI()
    {
        text.text = Mathf.CeilToInt(currentHealth) + " / " + Mathf.CeilToInt(maxHealth);
        targetValue = Mathf.Clamp01(currentHealth / maxHealth);
    }

    void Die()
    {
        SystemLog.Instance.PostMessage("Player Died");
        // later: respawn or game over screen

        currentHealth = maxHealth;

        targetValue = 1f;
        healthSlider.value = 1f;

        updateUI();
    }
}