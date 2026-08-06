using System;
using System.Transactions;
using UnityEngine;

public class WeaponModelDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponStage;
    [SerializeField] private Player player;
    [SerializeField] private int weaponIndex = 0;
    //[SerializeField] private float rotateSpeed = 15f;

    private GameObject currentWeaponModel;
    private int weaponUILayer;

    void Awake()
    {
        // Get the integer value of our custom layer
        weaponUILayer = LayerMask.NameToLayer("WeaponUI");
    }

    void Update()
    {
        // Maybe add rotation
        //if (currentWeaponModel != null)
        //{
        //    weaponStage.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
        //}
    }

    private void OnEnable()
    {
        GameEvents.OnWeaponEquipped += ShowWeapon;
        ShowWeapon(player.WeaponInventory[weaponIndex]);
    }

    private void OnDisable()
    {
        GameEvents.OnWeaponEquipped -= ShowWeapon;
    }

    /// <summary>
    /// Spawns a weapon model on the UI stage.
    /// </summary>
    public void ShowWeapon(IWeapon weapon)
    {
        if(this.gameObject.activeInHierarchy == false || weapon == null) return;

        GameObject weaponPrefab = weapon.Data.WeaponPrefab;
        if (currentWeaponModel != null)
        {
            Destroy(currentWeaponModel);
        }

        if (weaponPrefab != null)
        {
            currentWeaponModel = Instantiate(weaponPrefab, weaponStage.position, weaponStage.rotation);
            currentWeaponModel.transform.SetParent(weaponStage);

            SetLayerRecursively(currentWeaponModel, weaponUILayer);
        }
    }

    /// <summary>
    /// Helper function to set the layer for an object and all its children.
    /// </summary>
    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
}