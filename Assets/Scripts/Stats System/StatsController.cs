using UnityEngine;

public class StatsController : MonoBehaviour
{
    public Player player;
    public int health;

    // Player
    public PlayerStat MaxHealth { get; private set; }
    public PlayerStat MoveSpeed { get; private set; }
    
    public PlayerStat Defense { get; private set; }
    public PlayerStat DodgeChance { get; private set; }

    public PlayerStat AllDamageMultiplier { get; private set; }
    public PlayerStat SecondaryDamageMultiplier { get; private set; }

    public PlayerStat MaxStorage {  get; private set; }
    public PlayerStat MaxMemory { get; private set; }

    // Weapon
    public PlayerStat WeaponDamage { get; private set; }
    public PlayerStat FireRate { get; private set; }
    public PlayerStat MaxAmmo { get; private set; }
    public PlayerStat AmmoReserve { get; private set; }
    public PlayerStat ReloadSpeed { get; private set; }

    public PlayerStat RecoilHorizontalPerShot { get; private set; }
    public PlayerStat RecoilVerticalPerShot { get; private set; }
    public PlayerStat BulletSpread { get; private set; }

    public PlayerStat AirborneEffectiveness { get; private set; }
    public PlayerStat AirborneDamageMultiplier { get; private set; }

    public PlayerStat MagazineDamageBonusThreshold {  get; private set; }
    public PlayerStat MagazineDamageBonusMultiplier { get; private set; }

    public PlayerStat BulletBounces { get; private set; }
    public PlayerStat RoundsPerShot { get; private set; }
    public PlayerStat AmmoConsumptionChance { get; private set; }

    public PlayerStat Cleave { get; private set; }


    void Awake()
    {
        // Initialize all stats with their base values
        if(player != null)
        {
            MaxHealth = new PlayerStat(health != 0 ? health : 100f, "Player");
            MoveSpeed = new PlayerStat(7f, "Player");  

            Defense = new PlayerStat(1f, "Player");
            DodgeChance = new PlayerStat(0, "Player");

            AllDamageMultiplier = new PlayerStat(1f, "Player");
            SecondaryDamageMultiplier = new PlayerStat(1f, "Player");

            MaxStorage = new PlayerStat(1024f, "Player/Storage");
            MaxMemory = new PlayerStat(64f, "Player/Memory");

            // Weapon
            RecoilHorizontalPerShot = new PlayerStat(1f, "Weapon");
            RecoilVerticalPerShot = new PlayerStat(1f, "Weapon");
            BulletSpread = new PlayerStat(1f);

            AirborneEffectiveness = new PlayerStat(1.5f, "Player");
            AirborneDamageMultiplier = new PlayerStat(1f, "Player");

            MaxAmmo = new PlayerStat(100, "Weapon");
            AmmoReserve = new PlayerStat(300, "Weapon");

            WeaponDamage = new PlayerStat(1f, "Weapon");
            FireRate = new PlayerStat(60f / 1f, "Weapon");

            ReloadSpeed = new PlayerStat(1f, "Weapon");

            // Weapon-affected Player Stats
            MagazineDamageBonusThreshold = new PlayerStat(0f, "Player");
            MagazineDamageBonusMultiplier = new PlayerStat(1f, "Player");
            RoundsPerShot = new PlayerStat(1, "Player");
            BulletBounces = new PlayerStat(0, "Player");
            AmmoConsumptionChance = new PlayerStat(1, "Player");

            Cleave = new PlayerStat(1f, "Player");

            GameEvents.OnWeaponEquipped += OnGunEquipEvent;
            return;
        }

        // For Enemies
        MaxHealth = new PlayerStat(health != 0 ? health : 100f);
        Defense = new PlayerStat(1f);
        AllDamageMultiplier = new PlayerStat(1f);
        MoveSpeed = new PlayerStat(1f);
    }

    private void OnGunEquipEvent(IWeapon weapon)
    {
        RecoilHorizontalPerShot.BaseValue = weapon.Data.RecoilHorizontalPerShot;
        RecoilVerticalPerShot.BaseValue = weapon.Data.RecoilVerticalPerShot;
        BulletSpread.BaseValue = weapon.Data.BulletSpreadAngle;

        MaxAmmo.BaseValue = weapon.Data.Magazine;
        AmmoReserve.BaseValue = weapon.Data.AmmoReserve;
        WeaponDamage.BaseValue = weapon.Data.Damage;
        FireRate.BaseValue = 60f / weapon.Data.RoundsPerMinute;

        ReloadSpeed.BaseValue = weapon.Data.ReloadTime;
    }

    public PlayerStat GetStatModifierByType(Stat stat)
    {
        switch (stat)
        {
            // Player
            case Stat.Health: return MaxHealth;
            case Stat.MoveSpeed: return MoveSpeed;

            case Stat.Defense: return Defense;
            case Stat.DodgeChance: return DodgeChance;

            case Stat.AllDamageMultiplier: return AllDamageMultiplier;
            case Stat.SecondaryDamageMultiplier: return SecondaryDamageMultiplier;

            case Stat.Memory: return MaxMemory;
            case Stat.Storage: return MaxStorage;

            // Weapon
            case Stat.WeaponDamage: return WeaponDamage;
            case Stat.FireRate: return FireRate;
            case Stat.MaxAmmo: return MaxAmmo;
            case Stat.ReloadSpeed: return ReloadSpeed;

            case Stat.RecoilHorizontalPerShot: return RecoilHorizontalPerShot;
            case Stat.RecoilVerticalPerShot: return RecoilVerticalPerShot;
            case Stat.BulletSpread: return BulletSpread;

            case Stat.AirborneEffectiveness: return AirborneEffectiveness;
            case Stat.AirborneDamageMultiplier: return AirborneDamageMultiplier;

            case Stat.MagazineDamageBonusThreshold: return MagazineDamageBonusThreshold;
            case Stat.MagazineDamageBonusMultiplier: return MagazineDamageBonusMultiplier;

            case Stat.BulletBounces: return BulletBounces;
            case Stat.RoundsPerShot: return RoundsPerShot;
            case Stat.AmmoConsumptionChance: return AmmoConsumptionChance;
            case Stat.Cleave: return Cleave;
        }
        return null;
    }

    public void RemoveAllModifiersFromSourceType<T>() where T : class
    {
        // Player
        MaxHealth.RemoveAllModifiersFromType<T>();
        MoveSpeed.RemoveAllModifiersFromType<T>();

        Defense.RemoveAllModifiersFromType<T>();
        DodgeChance.RemoveAllModifiersFromType<T>();

        AllDamageMultiplier.RemoveAllModifiersFromType<T>();
        SecondaryDamageMultiplier.RemoveAllModifiersFromType<T>();

        MaxStorage.RemoveAllModifiersFromType<T>();
        MaxMemory.RemoveAllModifiersFromType<T>();

        // Weapon
        WeaponDamage.RemoveAllModifiersFromType<T>();
        FireRate.RemoveAllModifiersFromType<T>();
        MaxAmmo.RemoveAllModifiersFromType<T>();
        ReloadSpeed.RemoveAllModifiersFromType<T>();

        RecoilHorizontalPerShot.RemoveAllModifiersFromType<T>();
        RecoilVerticalPerShot.RemoveAllModifiersFromType<T>();
        BulletSpread.RemoveAllModifiersFromType<T>();

        AirborneEffectiveness.RemoveAllModifiersFromType<T>();
        AirborneDamageMultiplier.RemoveAllModifiersFromType<T>();

        MagazineDamageBonusThreshold.RemoveAllModifiersFromType<T>();
        MagazineDamageBonusMultiplier.RemoveAllModifiersFromType<T>();

        BulletBounces.RemoveAllModifiersFromType<T>();
        RoundsPerShot.RemoveAllModifiersFromType<T>();
        AmmoConsumptionChance.RemoveAllModifiersFromType<T>();

        Cleave.RemoveAllModifiersFromType<T>();
    }


    public string PrintDebugList()
    {
        string bob = "";

        bob += $"Damage: \t{WeaponDamage.GetValue()}\n" +
            $"Storage: \t{MaxStorage.GetValue()}\n" +
            $"Memory: \t{MaxMemory.GetValue()}\n" +
            $"Fire Rate: \t{FireRate.GetValue()}\n" +
            $"Health: \t{MaxHealth.GetValue()}\n" +
            $"Speed:\t{MoveSpeed.GetValue()}\n" +
            $"Ammo: \t{MaxAmmo.GetValue()}\n" +
            $"Defense: \t{Defense.GetValue()}\n" + 
            $"Dodge: \t{DodgeChance.GetValue()}\n" +
            $"RecoilH: \t{RecoilHorizontalPerShot.GetValue()}\n" +
            $"RecoilV: \t{RecoilVerticalPerShot.GetValue()}\n" +
            $"Spread: \t{BulletSpread.GetValue()}\n" +
            $"Reload: \t{ReloadSpeed.GetValue()}\n" +
            $"Spread: \t{BulletBounces.GetValue()}\n" +
            $"Rounds/Shot: \t{RoundsPerShot.GetValue()}\n" +
            $"Mag Bonus Threshold: \t{MagazineDamageBonusThreshold.GetValue()}\n" +
            $"Mag Bonus Multiplier: \t{MagazineDamageBonusMultiplier.GetValue()}\n" +
            $"Airborne Effectiveness: \t{AirborneEffectiveness.GetValue()}\n" +
            $"Secondary Damage Multiplier: \t{SecondaryDamageMultiplier.GetValue()}\n" +
            $"Ammo Consumption Chance: \t{AmmoConsumptionChance.GetValue()}\n" +
            $"Cleave: \t{Cleave.GetValue()}\n";

        return bob;
    }
}