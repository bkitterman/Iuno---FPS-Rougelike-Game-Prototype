using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance;

    [SerializeField][Range(1, 2)] private float meleeDropMultiplier = 1.5f;
    [SerializeField] private AmmoDatabase ammoDatabase;
    private Player player;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        // Find the player
        player = FindAnyObjectByType<Player>();
    }

    private void OnEnable()
    {
        if (player == null) player = FindAnyObjectByType<Player>();
    }

    /// <summary>
    /// This is called by an enemy's health script on death.
    /// </summary>
    public void ProcessAmmoDrop(Vector3 position, float dropMultiplier, bool isMelee)
    {
        player = FindAnyObjectByType<Player>();

        // Increase drop chance if melee kill
        if (isMelee)
        {
            dropMultiplier *= meleeDropMultiplier;
        }

        Dictionary<AmmoType, (int current, int max)> ammoCounts = new();

        // Calculate total ammo percentage
        /* FORMULA
        playerAmmoRequired = (Total Current Player Ammo / Max Player Ammo)%
         
        let x = 30, let y = above formula

        if y = 0 (Full Ammo), then drop chance = 0;
        if y = 1 (No ammo), then drop chance = 1;

         */

        for (int i = 0; i < player.WeaponInventory.Count; i++)
        {
            IWeapon weapon = player.WeaponInventory[i];

            if(weapon is Gun gun)
            {
                AmmoType type = gun.Data.AmmoType;

                int currentAmmo = gun.CurrentAmmo + gun.AmmoReserve;
                int maxAmmo = (int)(player.Stats.MaxAmmo.GetValue(gun.Data.Magazine) + player.Stats.AmmoReserve.GetValue(gun.Data.AmmoReserve));

                if (ammoCounts.ContainsKey(type))
                {
                    ammoCounts[type] = (ammoCounts[type].current + currentAmmo, ammoCounts[type].max + maxAmmo);
                }
                else
                {
                    ammoCounts[type] = (currentAmmo, maxAmmo);
                }
            }
        }

        if(ammoCounts.Count == 0)
        {
            return; // Player has no weapons that use ammo, so no drops
        }

        // Sort ammo types by lowest percentage to highest
        var list = ammoCounts.ToList();
        list.Sort((a, b) => ((float)a.Value.current / a.Value.max).CompareTo((float)b.Value.current / b.Value.max));

        // Check which wins. This is done by breaking upon first selection, which acts as weighting the drop
        AmmoType droppedType = AmmoType.None;
        float random = Random.value * (isMelee ? meleeDropMultiplier : 1f);
        foreach (var m in list)
        {
            if (random > (float)m.Value.current / m.Value.max)
            {
                droppedType = m.Key;
                break;
            }
        }

        //Debug.Log("Ammo drop probabilities are as follows: " + string.Join(", ", list.Select(m => m.Key + ": " + (1f - ((float)m.Value.current / m.Value.max)) * 100f + "%")));

        if(droppedType == AmmoType.None)
        {
            return; // No drop
        }

        AmmoData ammoData = ammoDatabase.Ammo.First(a => a.Type == droppedType);

        // 3. Calculate ammount to drop
        int ammountToDrop = Mathf.RoundToInt(ammoData.BaseDropAmount * Random.Range(1f - ammoData.DropVariance, 1f + ammoData.DropVariance) * (1f + dropMultiplier));

        // 4. Spawn the item
        GameObject drop = Instantiate(ammoData.PickupPrefab, position, Quaternion.identity);
        AmmoPickup pickup = drop.GetComponent<AmmoPickup>();
        pickup.Type = droppedType;
        pickup.Amount = ammountToDrop; 
    }
}