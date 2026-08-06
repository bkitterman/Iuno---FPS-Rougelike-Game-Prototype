using UnityEngine;
using System.Collections.Generic;

public class EnemyWeaponHandler : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private Transform weaponHoldPoint; 

    // --- State ---
    public EnemyWeaponInstance CurrentWeapon { get; private set; } 
    private List<EnemyWeaponInstance> weaponInventory = new List<EnemyWeaponInstance>();
    private int currentWeaponIndex = -1;

    // --- Component References ---
    private EnemyBrain brain;
    private EnemyModifierManager modifierManager;


    void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        modifierManager = brain.GetComponent<EnemyModifierManager>();
        if (weaponHoldPoint == null)
        {
            Debug.LogWarning("WeaponHoldPoint not assigned. Weapon visuals may not work.", this);
        }
    }

    /// <summary>
    /// Called by EnemyBrain during initialization. Instantiates and equips weapons.
    /// </summary>
    public void Initialize(EnemyData data, EnemyBrain brain)
    {
        this.brain = brain;

        // Clear old weapons
        foreach (EnemyWeaponInstance weapon in weaponInventory)
        {
            if (weapon != null && weapon.gameObject != null)
                Destroy(weapon.gameObject);
        }

        weaponInventory.Clear();
        CurrentWeapon = null;
        currentWeaponIndex = -1;

        if (this.brain == null) Debug.LogWarning("Brain missing on hihg");

        // Instantiate weapons from data
        if (data.availableWeapons != null && data.availableWeapons.Count > 0)
        {
            foreach (WeaponData weaponData in data.availableWeapons)
            {
                GameObject weaponObj = new GameObject(weaponData.name + "_EnemyInstance"); // Prefab placeholder
                if (weaponHoldPoint != null)
                {
                    weaponObj.transform.SetParent(weaponHoldPoint);
                    weaponObj.transform.localPosition = Vector3.zero;
                    weaponObj.transform.localRotation = Quaternion.identity;
                }

                if (weaponData.IsMelee)
                {
                    EnemyMeleeWeapon meleeInstance = weaponObj.AddComponent<EnemyMeleeWeapon>();
                    meleeInstance.Initialize(weaponData, brain, modifierManager); 
                    weaponInventory.Add(meleeInstance); 
                }
                else
                {
                    EnemyRangedWeapon rangedInstance = weaponObj.AddComponent<EnemyRangedWeapon>();
                    rangedInstance.Initialize(weaponData, brain, modifierManager);
                    weaponInventory.Add(rangedInstance);
                }

                weaponObj.SetActive(false); 
            }
            // Equip the first weapon by default
            EquipWeapon(0);
        }
        else
        {
            Debug.LogWarning($"Enemy {data.enemyName} has no weapons defined in EnemyData.", this);
        }
    }

    /// <summary>
    /// Equips a weapon from the inventory by its index.
    /// </summary>
    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= weaponInventory.Count || index == currentWeaponIndex)
        {
            return; // Invalid index or already equipped
        }

        // Deactivate the current weapon
        if (CurrentWeapon != null)
        {
            CurrentWeapon.gameObject.SetActive(false);
        }

        // Activate the new weapon
        currentWeaponIndex = index;
        CurrentWeapon = weaponInventory[currentWeaponIndex];
        CurrentWeapon.gameObject.SetActive(true);

        // Optionally, inform the Brain/AI about the new weapon's properties (e.g., range)
        // brain.UpdateAttackParameters(CurrentWeapon.Data);
    }

    /// <summary>
    /// Attempts to fire the currently equipped weapon at the specified target.
    /// </summary>
    public void Attack(Transform target)
    {
        if (CurrentWeapon != null)
        {
            CurrentWeapon.TryAttack(target);
        }
        else
        {
            Debug.LogWarning("Enemy tried to attack but has no weapon equipped.", this);
        }
    }
}