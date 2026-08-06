using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebugScreen : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public Player debugItem;
    public enum DisplayInfo { activeItems, playerStats, damageOut }
    public DisplayInfo display;

    private float damageHighest;
    private float damageTotal;
    private float damageCurrent;

    private float timeSinceDamage;
    [SerializeField] private float currentDamageTime = 3f;


    void Start()
    {
        switch (display) 
        {
            case DisplayInfo.activeItems:
                textUI.text = debugItem.ActivePrograms.Count > 0 ? debugItem.PrintDebugList() : "None";
                break;
            case DisplayInfo.playerStats:
                textUI.text = debugItem.Stats.PrintDebugList();
                break;
            case DisplayInfo.damageOut:
                GameEvents.OnDamageDealt += DamageHandler;
                break;
        }

    }

    void LateUpdate()
    {
        switch (display)
        {
            case DisplayInfo.activeItems:
                textUI.text = debugItem.ActivePrograms.Count > 0 ? debugItem.PrintDebugList() : "None";
                break;
            case DisplayInfo.playerStats:
                textUI.text = debugItem.Stats.PrintDebugList();
                break;
            case DisplayInfo.damageOut:
                buildDamageOutText();
                break;
        }
    }

    void DamageHandler(GameObject target, float damage, bool isCrit, bool onHit)
    {
        timeSinceDamage = 0f;
        damageTotal += damage;
        damageCurrent += damage;
    }

    void buildDamageOutText()
    {
        timeSinceDamage += Time.deltaTime;
        if(timeSinceDamage > currentDamageTime)
        {
            if(damageCurrent > damageHighest) damageHighest = damageCurrent;
            damageCurrent = 0f;
        }
        
        textUI.text = $"Highest:\t{damageHighest.ToString("N0")}\nCurrent: \t{damageCurrent.ToString("N0")}\nTotal:  \t{damageTotal.ToString("N0")}";
    }
}
