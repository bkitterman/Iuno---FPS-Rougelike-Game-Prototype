using UnityEngine;

public class Rapax : MonoBehaviour, ApplicationSuiteInstance
{
    public ApplicationSuiteData Data { get; set; }
    public bool IsActive { get; set; }

    private StatsController _statsController;
    private PlayerHealth _playerHealth;
    
    private float _effectValue = 0;
    private bool firstSetup = false;

    /// <summary>
    /// Subscribe to events, apply any buffs/debuffs, and do any other neccesities.
    /// 
    /// This is for adding program to memory.
    /// </summary>
    public void OnActivate(float value, int tier)
    {
        if(firstSetup == false)
        {
            _playerHealth = GetComponentInParent<PlayerHealth>();
            _statsController = GetComponentInParent<StatsController>();
        }

        _effectValue = value;
        IsActive = true;
        GameEvents.OnEnemyKilled += OnKillEvent;
    }

    /// <summary>
    /// Unsubscribe from events, remove any buffs/debuffs, and do other cleanup as needed.
    /// 
    /// This is for removing program from memory.
    /// </summary>
    public void OnDeactivate()
    {
        IsActive = false;
        GameEvents.OnEnemyKilled -= OnKillEvent;
    }

    public void OnKillEvent(GameObject target)
    {
        foreach (IWeapon weapon in _playerHealth.player.WeaponInventory)
        {
            if (weapon is Gun gun)
            {
                gun.CurrentAmmo = (int) _statsController.MaxAmmo.GetValue(gun.Data.Magazine);
            }
        }
        _playerHealth.Heal(_statsController.MaxHealth.GetValue() * _effectValue);
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
