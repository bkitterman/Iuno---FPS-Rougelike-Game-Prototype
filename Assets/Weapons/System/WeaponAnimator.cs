using UnityEngine;

public class WeaponAnimator : MonoBehaviour
{
    [Header("Component References")]
    [SerializeField] private Transform weaponPivot;
    [SerializeField] private WeaponBob weaponBob;
    [SerializeField] private WeaponSway weaponSway;
    [SerializeField] private WeaponRecoil weaponRecoil;
    
    [Header("ADS Setup")]
    private Transform sightTransform; 
    [SerializeField] private Transform adsTarget;     

    // --- STATE VARIABLES ---
    private Vector3 baseLocalPos;
    private Quaternion baseLocalRot;
    private Vector3 adsPos;
    private Quaternion adsRot;

    private Vector3 currentBasePos;
    private Quaternion currentBaseRot;
    private Vector3 posSmoothVel;

    private PlayerState playerState;
    private Gun equippedGun;

    void Start()
    {
        playerState = GetComponentInParent<PlayerState>();

        // Store the gun's original position and rotation
        baseLocalPos = weaponPivot.localPosition;
        baseLocalRot = weaponPivot.localRotation;

        currentBasePos = baseLocalPos;
        currentBaseRot = baseLocalRot;

        GameEvents.OnWeaponEquipped += OnGunEquipEvent;
    }

    void OnDestroy()
    {
        GameEvents.OnWeaponEquipped -= OnGunEquipEvent;
    }


    void OnGunEquipEvent(IWeapon weapon)
    {
        if (weapon is not Gun) return;
        Gun gun = weapon as Gun;

        // --- 1. Store the pivot's current animated state ---
        Vector3 previousPos = weaponPivot.localPosition;
        Quaternion previousRot = weaponPivot.localRotation;

        // --- 2. Force the pivot to its base hip-fire pose ---
        weaponPivot.localPosition = baseLocalPos;
        weaponPivot.localRotation = baseLocalRot;

        // --- 3. Now, get references and perform calculations ---
        equippedGun = gun;
        sightTransform = gun.ADSPoint;

        Vector3 deltaPos = adsTarget.position - sightTransform.position;
        adsPos = baseLocalPos + weaponPivot.parent.InverseTransformVector(deltaPos);
        Quaternion deltaRot = adsTarget.rotation * Quaternion.Inverse(sightTransform.rotation);

        // 2. Get the hip-fire world rotation
        Quaternion hipWorldRot = weaponPivot.parent.rotation * baseLocalRot;

        // 3. Apply the delta to the hip-fire world rotation
        Quaternion targetWorldRot = deltaRot * hipWorldRot;

        // 4. Convert the target world rotation back into the pivot's *local* rotation
        adsRot = Quaternion.Inverse(weaponPivot.parent.rotation) * targetWorldRot;

        // --- Z-Axis Fix ---
        Vector3 euler = adsRot.eulerAngles;
        euler.z = 0f;
        adsRot = Quaternion.Euler(euler);

        // --- 4. Restore the pivot to its previous animated state ---
        weaponPivot.localPosition = previousPos;
        weaponPivot.localRotation = previousRot;
    }

    void LateUpdate()
    {
        if (equippedGun == null) return;

        // --- 0. Get Base Target Transform ---
        Vector3 targetPos;
        Quaternion targetRot;

        if (playerState.IsAiming)
        {
            targetPos = adsPos;
            targetRot = adsRot;
            if (equippedGun.ScopeLens != null) equippedGun.ScopeLens.SetActive(false);
        }
        else
        {
            targetPos = baseLocalPos;
            targetRot = baseLocalRot;
            if (equippedGun.ScopeLens != null) equippedGun.ScopeLens.SetActive(true);
        }

        // --- 1. Smooth the Base Transform ---
        Vector3 smoothedBasePos = Vector3.SmoothDamp(currentBasePos, targetPos, ref posSmoothVel, 1f / equippedGun.Data.AdsSpeed);
        Quaternion smoothedBaseRot = Quaternion.Slerp(currentBaseRot, targetRot, 1f - Mathf.Exp(-equippedGun.Data.AdsSpeed * Time.deltaTime));

        // --- 2. Combine all Positional Offsets ---
        Vector3 finalPos = smoothedBasePos +
                           weaponBob.positionOffset +
                           weaponSway.positionOffset +
                           weaponRecoil.positionOffset;

        // --- 3. Combine all Rotational Offsets ---
        Quaternion finalRot = smoothedBaseRot *
                              weaponBob.rotationOffset *
                              weaponSway.rotationOffset *
                              weaponRecoil.rotationOffset;

        // --- 4. Apply the Final Transformation ---
        weaponPivot.localPosition = finalPos;
        weaponPivot.localRotation = finalRot;

        // --- 5. Update Internal State for Next Frame ---
        currentBasePos = smoothedBasePos;
        currentBaseRot = smoothedBaseRot;
    }
}