using UnityEngine;

public class Ferrum : MonoBehaviour, ApplicationSuiteInstance
{
    public ApplicationSuiteData Data { get; set; }
    public bool IsActive { get; set; }

    private StatsController _statsController;
    private Gun _equipedGun;
    
    private float _effectValue = 0;
    private int tier = 0;
    private bool firstSetup = false; 
    private StatusEffectManager _statusManager;

    /// <summary>
    /// Subscribe to events, apply any buffs/debuffs, and do any other neccesities.
    /// 
    /// This is for adding program to memory.
    /// </summary>
    public void OnActivate(float value, int tier)
    {
        if(firstSetup == false)
        {
            _statsController = GetComponent<StatsController>();
            _statusManager = GetComponent<StatusEffectManager>();
        }

        this.tier = tier;
        if(tier > 0)
            GameEvents.OnPlayerReload += OnReloadEvent;
        GameEvents.OnEnemyKilled += OnKillEvent;

        _effectValue = value;
        IsActive = true;
    }

    /// <summary>
    /// Unsubscribe from events, remove any buffs/debuffs, and do other cleanup as needed.
    /// 
    /// This is for removing program from memory.
    /// </summary>
    public void OnDeactivate()
    {
        IsActive = false;
        if(tier > 0)
            GameEvents.OnPlayerReload -= OnReloadEvent;
        GameEvents.OnEnemyKilled -= OnKillEvent;
    }

    public void OnKillEvent(GameObject target)
    {
        _statusManager.ApplyEffect(Data.BonusTiers[0].Effect);
    }

    public void OnReloadEvent()
    {
        _statusManager.ApplyEffect(Data.BonusTiers[1].Effect);
    }

    /// <summary>
    /// Return the buffs multiplier value.
    /// </summary>
    public float GetSuiteMultiplier()
    {
        return _effectValue;
    }

    public string toString() { return Data.Name; }
}
