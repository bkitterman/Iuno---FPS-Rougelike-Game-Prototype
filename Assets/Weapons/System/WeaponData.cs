using UnityEngine;

[CreateAssetMenu(fileName = "WeaponData", menuName = "Weapons/WeaponData")]
public class WeaponData : ScriptableObject
{
    [Header("Gun Information")]
    public string Name;
    [TextArea] public string Description;
    public WeaponType WeaponType = WeaponType.Rifle;
    public AmmoType AmmoType = AmmoType.Rifle;
    public bool IsAutomatic = true;
    public float RoundsPerMinute = 600f;

    public int Magazine = 30;
    public int AmmoReserve = 496;
    public float Damage = 30f;
    public float Range = 200f;
    public float ReloadTime = 2f;
    public float AdsSpeed = 10f;
    public float ProjectileSpeed = 1f; // 0 is instant
    public float AdsFov;
    public GameObject WeaponPrefab;
    public GameObject ProjectilePrefab; // Keep this for ranged weapons

    public bool IsMelee = false; // ADD THIS FLAG

    [Header("Spread (degrees)")]
    public float BulletSpreadAngle = 0.6f;
    public float SpreadPerShotDeg = 0.6f;
    public float SpreadMaxDeg = 6f;
    public float SpreadRecoveryRate = 4f; // deg/sec
    public float BulletMagnetismRadius = 0.2f;

    [Header("Recoil (deg)")]
    public float RecoilVerticalPerShot = 1.4f;
    public float RecoilHorizontalPerShot = 0.4f;   
    public float RecoilRecoverySpeed = 6f; 
    public float RecoilAccumulationMax = 18f;

    public float AdsSpreadMultiplier = 0.25f;
    public float AdsRecoilMultiplier = 0.6f;
    public float SwayAmount = 1f;
}
