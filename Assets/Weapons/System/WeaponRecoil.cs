using UnityEngine;

public class WeaponRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [SerializeField] private float recoilKickBackMin = 0.02f;
    [SerializeField] private float recoilKickBackMax = 0.06f;
    [SerializeField] private float recoilAnglePerShot = 1.8f;
    [SerializeField] private float recoilRecoverSpeed = 8f;

    [Header("ADS Damping")]
    [SerializeField] private float adsMultiplier = 0.2f;

    public Vector3 positionOffset { get; private set; }
    public Quaternion rotationOffset { get; private set; }

    // Internal smoothing states
    private Vector3 currentPosOffset;
    private Vector3 posSmoothVel;

    private float currentRecoilAngle;
    private float rotSmoothVel;

    private PlayerState playerState;

    void Awake()
    {
        playerState = GetComponentInParent<PlayerState>();
    }

    void Update()
    {
        bool isAiming = playerState.IsAiming;
        float adsFactor = isAiming ? adsMultiplier : 1f;
        float smoothTime = 1f / (recoilRecoverSpeed);

        // Smoothly dampen the position offset back to zero
        currentPosOffset = Vector3.SmoothDamp(currentPosOffset, Vector3.zero, ref posSmoothVel, smoothTime);

        // Smoothly dampen the rotation offset back to zero
        currentRecoilAngle = Mathf.SmoothDampAngle(currentRecoilAngle, 0f, ref rotSmoothVel, smoothTime);

        positionOffset = currentPosOffset;
        rotationOffset = Quaternion.Euler(-currentRecoilAngle, 0, 0);
    }

    /// <summary>
    /// Adds a "kick" to the recoil, which Update() will then smooth away.
    /// </summary>
    public void OnFire()
    {
        // --- 1. Get Player State ---
        bool isAiming = playerState.IsAiming;

        // --- 2. Calculate ADS Damping ---
        float adsFactor = isAiming ? adsMultiplier : 1f;

        // --- 3. Apply Positional Kick ---
        float kick = Random.Range(recoilKickBackMin, recoilKickBackMax);
        currentPosOffset += Vector3.back * kick * adsFactor;

        // --- 4. Apply Rotational Kick ---
        float angle = recoilAnglePerShot * adsFactor;
        currentRecoilAngle += angle;
    }
}