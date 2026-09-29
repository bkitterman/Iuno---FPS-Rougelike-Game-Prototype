using UnityEngine;

public class WeaponSway : MonoBehaviour
{
    [Header("Sway Settings")]
    [SerializeField] private float lookSwayAmount = 0.03f;
    [SerializeField] private float lookSwaySmooth = 12f;
    [SerializeField] private float moveSwayAmount = 0.06f;
    [SerializeField] private float moveSwaySmooth = 8f;

    [Header("Rotational Sway")]
    [SerializeField] private float maxRoll = 10f;
    [SerializeField] private float lookYawMultiplier = -1.5f;

    [Header("Positional Sway")]
    [SerializeField] private float movePosAmount = 0.01f;
    [SerializeField] private float maxPosOffset = 0.06f;

    [Header("ADS Damping")]
    [SerializeField] private float adsMultiplier = 0.15f;


    public Vector3 positionOffset { get; private set; }
    public Quaternion rotationOffset { get; private set; }

    // Internal smoothing states
    private Vector2 smoothedLook;
    private Vector2 smoothLookVel;

    private Vector3 smoothedPosOffset;
    private Vector3 smoothPosVel;

    private Quaternion smoothedRotOffset;
    private float smoothRotVelX;
    private float smoothRotVelY;
    private float smoothRotVelZ;

    private PlayerMovement playerMovement;
    private PlayerLook playerLook;
    private PlayerState playerState;

    void Awake()
    {
        // Fallback in case references aren't set in Inspector
        playerMovement = GetComponentInParent<PlayerMovement>();
        playerLook = GetComponentInParent<PlayerLook>();
        playerState = GetComponentInParent<PlayerState>();
    }

    void Update()
    {
        // --- 1. Get Player State ---
        // Ensure your PlayerLook script has a public "LookInput" property
        Vector2 rawLook = playerLook != null ? playerLook.LookInput : Vector2.zero;
        Vector3 horizontalVel = playerMovement.HorizontalVelocity;
        Vector3 localVel = transform.InverseTransformDirection(horizontalVel);
        bool isAiming = playerState.IsAiming;

        // --- 2. Calculate ADS Damping ---
        float adsFactor = isAiming ? adsMultiplier : 1f;

        // --- 3. Calculate Target Position Offset ---

        // A. Look Sway: Positional offset from mouse movement.
        // We use SmoothDamp for frame-rate independent smoothing (FIX from original)
        smoothedLook = Vector2.SmoothDamp(smoothedLook, rawLook, ref smoothLookVel, 1f / lookSwaySmooth);
        Vector3 lookSwayPos = new Vector3(-smoothedLook.x * lookSwayAmount * 0.02f,
                                          -smoothedLook.y * lookSwayAmount * 0.01f, 0f);

        // B. Move Sway: Positional offset from A/D strafing.
        Vector3 movePosOffset = transform.right * (-localVel.x) * movePosAmount;

        // C. Combine and clamp positional offsets
        Vector3 targetPosOffset = (lookSwayPos + movePosOffset) * adsFactor;
        if (targetPosOffset.magnitude > maxPosOffset)
        {
            targetPosOffset = targetPosOffset.normalized * maxPosOffset;
        }

        // --- 4. Calculate Target Rotation Offset ---

        // A. Look Yaw: Rotational offset from mouse X.
        float lookYaw = Mathf.Clamp(smoothedLook.x * lookYawMultiplier, -5f, 5f) * adsFactor;

        // B. Move Roll: Rotational offset (roll) from A/D strafing.
        float leanRoll = Mathf.Clamp(-localVel.x * moveSwayAmount * 40f, -maxRoll, maxRoll) * adsFactor;

        // C. Move Pitch: Rotational offset (pitch) from W/S movement.
        float leanPitch = Mathf.Clamp(-localVel.z * moveSwayAmount * 0.2f, -0.05f, 0.05f) * 40f * adsFactor;

        // D. Combine rotations using stable Quaternion multiplication (FIX from original)
        Quaternion targetRotOffset = Quaternion.Euler(leanPitch, lookYaw, leanRoll);

        // --- 5. Smooth Final Outputs ---
        // We smooth the final offsets so they feel responsive and springy.
        float smoothTime = 1f / moveSwaySmooth;

        positionOffset = Vector3.SmoothDamp(positionOffset, targetPosOffset, ref smoothPosVel, smoothTime);

        // Quaternion.Slerp is tricky to smooth correctly. It's often easier
        // to SmoothDamp the Euler angles and recompose.
        Vector3 currentRotEuler = rotationOffset.eulerAngles;
        Vector3 targetRotEuler = targetRotOffset.eulerAngles;

        float x = Mathf.SmoothDampAngle(currentRotEuler.x, targetRotEuler.x, ref smoothRotVelX, smoothTime);
        float y = Mathf.SmoothDampAngle(currentRotEuler.y, targetRotEuler.y, ref smoothRotVelY, smoothTime);
        float z = Mathf.SmoothDampAngle(currentRotEuler.z, targetRotEuler.z, ref smoothRotVelZ, smoothTime);

        rotationOffset = Quaternion.Euler(x, y, z);
    }
}