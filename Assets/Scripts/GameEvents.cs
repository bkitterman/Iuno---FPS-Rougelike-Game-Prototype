using System;
using UnityEngine;

public static class GameEvents
{
    public static event Action<GameObject> OnEnemyKilled;
    public static void ReportEnemyKilled(GameObject enemy)
    {
        OnEnemyKilled?.Invoke(enemy);
    }

    public static event Action OnPlayerReload;
    public static void ReportPlayerReload()
    {
        OnPlayerReload?.Invoke();
    }

    //public static event Action<int> OnPlayerWeaponFired;
    //public static void ReportPlayerWeaponFired(int currentAmmo)
    //{
    //    OnPlayerWeaponFired?.Invoke(currentAmmo);
    //}

    public static event Action OnPlayerMiss;
    public static void ReportPlayerMiss()
    {
        OnPlayerMiss?.Invoke();
    }

    public static event Action<GameObject, float, bool, bool> OnDamageDealt;
    public static void ReportDamageDealt(GameObject target, float damage, bool isCrit, bool onHitEnabled)
    {
        OnDamageDealt?.Invoke(target, damage, isCrit, onHitEnabled);
    }

    public static event Action<GameObject, Vector3, float> OnPlayerDamageTaken;
    public static void ReportPlayerDamageTaken(GameObject target, Vector3 hitPoint, float damage)
    {
        OnPlayerDamageTaken?.Invoke(target, hitPoint, damage);
    }

    public static event Action<GameObject, float, Vector3> OnMeleeDamageDealt;
    public static void ReportMeleeDamageDealt(GameObject target, float damage, Vector3 direction)
    {
        OnMeleeDamageDealt?.Invoke(target, damage, direction);
    }

    public static event Action<ProgramData> OnProgramPickedUp;
    public static void ReportProgramPickedUp(ProgramData program)
    {
        OnProgramPickedUp?. Invoke(program);
    }

    public static event Action<AmmoType> OnAmmoPickedUp;
    public static void ReportAmmoPickedUp(AmmoType type)
    {
        OnAmmoPickedUp?.Invoke(type);
    }

    public static event Action<ProgramData> OnProgramDrop;
    public static void ReportProgramDrop(ProgramData program)
    {
        OnProgramDrop?.Invoke(program);
    }

    public static event Action<ProgramData> OnProgramEnabled;
    public static void ReportProgramEnabled(ProgramData program)
    {
        OnProgramEnabled?.Invoke(program);
    }

    public static event Action<ProgramData> OnProgramDisabled;
    public static void ReportProgramDisabled(ProgramData program)
    {
        OnProgramDisabled?.Invoke(program);
    }

    public static event Action<IWeapon> OnWeaponEquipped;
    public static void ReportWeaponEquipped(IWeapon weapon)
    {
        OnWeaponEquipped?.Invoke(weapon);
    }

    public static event Action<AbilityData> OnAbilityEquipped;
    public static void ReportAbilityEquipped(AbilityData data)
    {
        OnAbilityEquipped?.Invoke(data);
    }

    public static event Action<HardwareData> OnHardwareEquipped;
    public static void ReportHardwareEquipped(HardwareData data)
    {
        OnHardwareEquipped?.Invoke(data);
    }

}