using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class Player : MonoBehaviour
{   
    public StatsController Stats;
    public InventoryManager manager;
    public PlayerState state;

    [Header("Weapon Loadout")]
    public List<GameObject> gunPrefabs;
    public Transform weaponPivot;     

    public IWeapon EquippedWeapon { get; private set; }

    private int equippedWeaponIndex = 0;

    // This will hold all the weapon *instances*
    public List<IWeapon> WeaponInventory = new();

    public List<ProgramInstance> ActivePrograms = new();
    public List<ProgramInstance> StoredPrograms = new();

    private Dictionary<ApplicationSuiteData, int> _suiteCounts = new();
    public List<ApplicationSuiteInstance> ActiveSuites = new();

    void Awake()
    {
        state = GetComponent<PlayerState>();
    }

    void Start()
    {
        GameEvents.OnProgramEnabled += handleProgramEnabled;

        InitializeGunLoadout();
    }

    private void handleProgramEnabled(ProgramData data)
    {
        CheckAndApplySuites();
        sortActivePrograms();
    }

    private void sortActivePrograms()
    {
        ActivePrograms.Sort((a, b) => a.Data.ApplicationSuite.Name.CompareTo(b.Data.ApplicationSuite.Name));
    }


    private void OnTriggerEnter(Collider other)
    {
        // Check if player collided with a program pickup
        ProgramPickup program = other.gameObject.GetComponent<ProgramPickup>();
 
        if (program != null)
        {
            OnProgramEnter(program);
            return;
        
        }

        // Check if player collided with an ammo pickup
        AmmoPickup ammo = other.gameObject.GetComponent<AmmoPickup>();
        if (ammo != null)
        {
            OnAmmoEnter(ammo);
        }

    }

    // ------------------------ PROGRAMS ------------------------
    private void OnProgramEnter(ProgramPickup pickup)
    {        
        var data = pickup.programData;
        ProgramInstance existingInstance = StoredPrograms.FirstOrDefault(p => p.Data == data);

        float memoryUtilPercent = CurrentMemoryUtilization() / Stats.MaxMemory.GetValue();
        float storageUtilPercent = CurrentStorageUtilization() / Stats.MaxStorage.GetValue();

        if (existingInstance != null)
        {
            if (ActivePrograms.Contains(existingInstance) &&
                memoryUtilPercent + (data.RoutineSize / Stats.MaxMemory.GetValue()) <= 1f)
            {
                existingInstance.OnRoutinePickup();

                if (existingInstance.IsOptimized == false && pickup.isOptimized == true)
                    existingInstance.Optimize();
            }
            else
            {
                existingInstance.MaxStacks++;
            }

        }
        else
        {
            // if storage full, don't add, just return
            if (storageUtilPercent + (data.Storage / Stats.MaxStorage.GetValue()) > 1f)
                return;

            System.Type programType = System.Type.GetType(data.ProgramName);

            if (programType != null)
            {
                ProgramInstance newInstance = (ProgramInstance)gameObject.AddComponent(programType);

                // Initialize the new instance with its data
                newInstance.Data = data;
                newInstance.MaxStacks = 0;
                newInstance.ActiveStacks = 0;
                newInstance.IsOptimized = pickup.isOptimized;

                newInstance.OnPickup();

                StoredPrograms.Add(newInstance);

                // TODO toggle later
                float val = (memoryUtilPercent + newInstance.Data.RoutineSize) / Stats.MaxMemory.GetValue();
                if (val <= 1f)
                    manager.ActivateProgram(data, 1);
            }
            else
            {
                Debug.LogError($"Could not find class with name: {data.ProgramName}");
            }
        }

        GameEvents.ReportProgramPickedUp(data);

        // Destroy the pickup object from the world
        if (!pickup.IsPermanent)
            Destroy(pickup.gameObject);
        
    }

    public bool HasCollectedProgram(string programName)
    {
        return StoredPrograms.FirstOrDefault(p => p.Data.ProgramName == programName) != null;
    }
    
    public bool CheckIfProgramOptimized(ProgramData data)
    {
        var program =  StoredPrograms.FirstOrDefault(p => p.Data == data);

        if (program != null) return program.IsOptimized;
        return false;
    }

    // ------------------------ SUITES ------------------------
    private void CheckAndApplySuites()
    {
        // 1. Clear the old counts
        _suiteCounts.Clear();

        // 2. Count all programs of each suite in the player's active inventory
        foreach (var programInstance in ActivePrograms)
        {
            if (programInstance.Data.ApplicationSuite != null)
            {
                var suiteData = programInstance.Data.ApplicationSuite;
                if (!_suiteCounts.ContainsKey(suiteData))
                {
                    _suiteCounts[suiteData] = 0;
                }
                _suiteCounts[suiteData]++;
            }
        }

        // 3. Remove all previously applied suite bonuses
        foreach (var bonus in ActiveSuites)
        {
            bonus.OnDeactivate();
        }
        ActiveSuites.Clear();

        // 4. Check each suite player has and apply the highest met tier bonus
        foreach (var suitePair in _suiteCounts)
        {
            var suiteData = suitePair.Key;
            var count = suitePair.Value;

            SuiteBonusTier bestTier = null;

            // Find the best tier the player qualifies for
            foreach (var tier in suiteData.BonusTiers)
            {
                if (count >= tier.RequiredPrograms)
                {
                    bestTier = tier;
                }
            }

            if (bestTier != null)
            {
                System.Type programType = System.Type.GetType(suiteData.Name);
                ApplicationSuiteInstance newInstance = (ApplicationSuiteInstance)gameObject.AddComponent(programType);

                // Initialize the new instance with its data
                newInstance.Data = suiteData;
                newInstance.OnActivate(bestTier.EffectValue, suiteData.BonusTiers.IndexOf(bestTier));
                ActiveSuites.Add(newInstance);
                
            }
        }
    }

    public bool IsSuiteActive(ApplicationSuiteData data)
    {
        return ActiveSuites.FirstOrDefault(p => p.Data == data) != null;
    }

    public Dictionary<ApplicationSuiteData, int> GetActiveSuiteCounts()
    {
        return _suiteCounts;
    }

    // ------------------------ MEMORY AND STORAGE ------------------------
    public float CurrentMemoryUtilization()
    {
        float currentMemoryUsed = 0f;
        foreach (var program in ActivePrograms)
        {
            if (program.IsOptimized)
                currentMemoryUsed += program.Data.OptimizedMemoryCost + (program.ActiveStacks * program.Data.OptimizedRoutineCost);
            else
                currentMemoryUsed += program.Data.Memory + (program.ActiveStacks * program.Data.RoutineSize);
        }

        // Add memory usage from OS, Firewall, etc.
        // currentMemoryUsed += player.OS.MemoryCost; 

        return currentMemoryUsed;
    }

    public float CurrentStorageUtilization()
    {
        float currentStorageUsed = 0f;
        foreach (var program in StoredPrograms)
        {
            currentStorageUsed += program.IsOptimized ? program.Data.OptimizedStorageCost : program.Data.Storage;
        }

        return currentStorageUsed;
    }

    // ------------------------ GUNS ------------------------
    void InitializeGunLoadout()
    {
        // Instantiate all guns from the prefab list
        for (int i = 0; i < gunPrefabs.Count; i++)
        {
            GameObject weaponObj = Instantiate(gunPrefabs[i], weaponPivot);
            IWeapon weaponInstance = weaponObj.GetComponent<IWeapon>();

            if (weaponInstance != null)
            {
                weaponInstance.Player = this;
                weaponInstance.FpsCam = Camera.main;
                weaponInstance.PlayerState = state;
                WeaponInventory.Add(weaponInstance);
                weaponObj.SetActive(false); // Start with all guns disabled
            }
        }

        // Equip the first gun
        EquipGun(0);
    }

    public void EquipGun(int index)
    {
        if (index >= WeaponInventory.Count)
        {
            index = 0; // Wrap around to the first gun
        } 
        else if (index < 0)
        {
            Debug.LogError($"Invalid gun index: {index}");
            return;
        }

        // 1. Deactivate the currently equipped gun (if one exists)
        if (EquippedWeapon != null)
        {
            EquippedWeapon.gameObject.SetActive(false);
        }

        // 2. Set the new gun as equipped
        equippedWeaponIndex = index;
        EquippedWeapon = WeaponInventory[equippedWeaponIndex];

        // 3. Activate the new gun's GameObject
        EquippedWeapon.gameObject.SetActive(true);

        // 4. CRITICAL: Update player stats with the new gun's data
        GameEvents.ReportWeaponEquipped(EquippedWeapon);
    }

    public List<AmmoType> GetEquippedAmmoType()
    {
        if (EquippedWeapon == null)
        {
            return new List<AmmoType>{AmmoType.None}; // Default or error case
        }
        List<AmmoType> emptyList = new() { EquippedWeapon.Data.AmmoType };

        return emptyList;
    }

    private void OnAmmoEnter(AmmoPickup pickup)
    {
        bool destroy = false;
        int shareCount = 0;
        List<(IWeapon, (float, int))> ammoShare = new();
        // Count the ammo types in the player's inventory
        foreach ( IWeapon weapon in WeaponInventory)
        {
            if (weapon is Gun gun && gun.Data.AmmoType == pickup.Type)
            {
                shareCount++;
                int maxReserve = (int)Stats.AmmoReserve.GetValue(gun.Data.AmmoReserve);
                float percent = ((float)(gun.CurrentAmmo + gun.AmmoReserve)) /
                    (Stats.MaxAmmo.GetValue(gun.Data.Magazine) + maxReserve);
                ammoShare.Add((weapon, (percent, maxReserve)));
            }
        }

        ammoShare.Sort((a,b) => a.Item2.Item1.CompareTo(b.Item2.Item1));

        foreach (var weaponShare in ammoShare)
        {
            IWeapon weapon = weaponShare.Item1;
            if (weapon is Gun gun)
            {
                int maxReserve = weaponShare.Item2.Item2;

                // Ensure space for 1 or more round in reserves
                if (gun.AmmoReserve < maxReserve)
                {
                    // Decide how much to pickup
                    if (gun.AmmoReserve + pickup.Amount > maxReserve)
                    {
                        pickup.Amount -= (maxReserve - gun.AmmoReserve);
                        gun.AmmoReserve = maxReserve;
                        destroy = true;
                    }
                    else
                    {
                        gun.AmmoReserve += pickup.Amount;
                        destroy = true;
                        break; // Exit after sharing with one gun to prevent overfilling multiple guns
                    }
                }
            }
        }

        // Destroy the pickup object from the world if used
        if (destroy)
        {
            if (!pickup.IsPermanent)
                Destroy(pickup.gameObject);
            GameEvents.ReportAmmoPickedUp(pickup.Type);
        }
    }

    public void SwapWeapon(int index, bool fromScroll = false)
    {
        if (fromScroll)
        {
            if (index > 0)
            {
                index = (equippedWeaponIndex + 1) % WeaponInventory.Count;
            }
            else
            {
                index = (equippedWeaponIndex + 2) % WeaponInventory.Count;
            }
        }
        
        EquipGun(index);
    }

    // ------------------------ DEBUG ------------------------
    public string PrintDebugList()
    {
        string bob = "--- PROGRAMS ---\n";
        foreach (var program in ActivePrograms)
        {
            bob += $"<size=\"3\">{program.Data.ProgramName} {(program.IsOptimized == true ? "Optimized " : "")}- {program.ActiveStacks} / {program.MaxStacks}\n" +
                $"<size=\"2\">\t{program.Data.InDepthDescription}\n" +
                $"\tCurrent: <color=\"green\">{program.GetProgramMultiplier()}<color=\"white\">\n";
        }
        bob += "--- SUITES ---\n";
        foreach (var suite in ActiveSuites)
        {
            bob += $"<size=\"3\">{suite.Data.Name}\n" +
                $"<size=\"2\">\t{suite.Data.InDepthDescription}\n" +
                $"\tCurrent: <color=\"green\">{suite.GetSuiteMultiplier()}<color=\"white\">\n";
        }
        return bob;
    }
}
