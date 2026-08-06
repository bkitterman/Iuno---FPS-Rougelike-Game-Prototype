using UnityEngine;

public class WeaponSwayAndRecoil : MonoBehaviour
{
    [Header("References")]
    public Transform weaponPivot;
    public Gun gun;
    public PlayerMovement playerMovement;

    [Header("Sway")]
    public float adsSwayMultiplier = 0.15f;
    public float lookSwayAmount = 0.03f;
    public float moveSwayAmount = 0.06f; 
    public float swaySmooth = 8f;
    public float lookSwaySmooth = 12f;
    public float lookYawMultiplier = -1.5f;
    public float movePosAmount = 0.01f;
    public float maxPosOffset = 0.06f; 
    public float maxRoll = 10f;

    [Header("Jump/land sway")]
    public float jumpTiltAmount = 8f;       
    public float jumpPosAmount = 0.05f; 
    public float landBumpAmount = 0.1f; 
    public float jumpSwaySmooth = 6f;

    [Header("Slide sway")]
    public float slideTiltAmount = 10f;
    public float slidePosAmount = 0.1f;
    public float slideSmooth = 8f;

    [Header("Bob")]
    public float bobFrequency = 8f; 
    public float bobAmount = 0.02f;  

    [Header("Recoil")]
    public float recoilKickBackMin = 0.02f;  
    public float recoilKickBackMax = 0.06f;   
    public float recoilAnglePerShot = 1.8f; 
    public float recoilAccumulationMax = 12f; 
    public float recoilRecoverSpeed = 8f; 
    public float recoilAccumDecay = 6f;        

    [Header("Fire Vibration")]
    public float fireVibrationAmount = 0.008f; 
    public float fireVibrationDuration = 0.06f;

    // Private States
    Quaternion baseLocalRot;
    Quaternion restLocalRot;
    Quaternion adsLocalRot;
    Vector2 smoothedLook;
    Vector3 posVelocitySmooth;
    Vector3 baseLocalPos;
    Vector3 restLocalPos;
    Vector3 adsLocalPos;
    Vector3 currentPos;
    Vector3 jumpOffset = Vector3.zero;
    Vector3 slideOffset = Vector3.zero;
    float slideTilt = 0f;
    float jumpTilt = 0f;
    bool wasGrounded = true;
    Quaternion currentRot;

    // Recoil states
    Vector3 recoilPosOffset = Vector3.zero;
    float recoilAngle = 0f;
    float recoilAccum = 0f;
    float fireVibTimer = 0f;

    PlayerLook playerLook;
    PlayerState playerState;

    // Bob.
    float bobTimer = 0f;

    void Awake()
    {
        if (weaponPivot == null) weaponPivot = transform;
        baseLocalPos = weaponPivot.localPosition;
        baseLocalRot = weaponPivot.localRotation;
        //weaponPivot = transform;
        //restLocalPos = weaponPivot.localPosition;
        //restLocalRot = weaponPivot.localRotation;

        //adsLocalPos = gun.adsPos;
        //adsLocalRot = gun.adsRot;

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerState = FindAnyObjectByType<PlayerState>();
    }

    void Start()
    {
        playerLook = FindAnyObjectByType<PlayerLook>();
    }

    void Update()
    {
        // ------------------ Intials ------------------
        // read inputs/state
        Vector2 rawLook = playerLook != null ? playerLook.LookInput : Vector2.zero;
        Vector3 hv = playerMovement != null ? playerMovement.HorizontalVelocity : Vector3.zero;
        float moveSpeed = hv.magnitude;

        float adsFactor = 1f;
        if (playerState.IsAiming)
        {
            adsFactor = adsSwayMultiplier;
        } 

        //baseLocalPos = playerMovement.IsAiming ? adsLocalPos : restLocalPos;
        //baseLocalRot = playerMovement.IsAiming ? adsLocalRot : restLocalRot;

        smoothedLook = Vector2.Lerp(smoothedLook, rawLook, Time.deltaTime * lookSwaySmooth);

        // ------------------ Weapon Sway (Look leans) ------------------
        Vector3 lookSwayPos = new Vector3(-smoothedLook.x * lookSwayAmount * 0.02f,
                                          -smoothedLook.y * lookSwayAmount * 0.01f,
                                          0f);
        lookSwayPos *= adsFactor;
        Vector3 localVel = playerMovement.transform.InverseTransformDirection(hv);

        // Rotation lean (A/D)
        float leanRoll = Mathf.Clamp(-localVel.x * moveSwayAmount * 40f, -maxRoll, maxRoll);
        leanRoll *= adsFactor; 

        // Rotational Tilt (W/S)
        float leanX = Mathf.Clamp(-localVel.z * moveSwayAmount * 0.2f, -0.05f, 0.05f);
        leanX *= adsFactor;

        // Small offset
        Vector3 movePosOffset = transform.right * (-localVel.x) * movePosAmount;
        movePosOffset *= adsFactor;

        // ------------------ Weapon Bob ------------------
        Vector3 bobOffset;
        if (playerState.IsGrounded && !playerState.IsSliding)
        {
            bobTimer += Time.deltaTime * (1f + moveSpeed * 0.2f) * bobFrequency;
            bobOffset = Vector3.up * (Mathf.Sin(bobTimer) * bobAmount * Mathf.Clamp01(moveSpeed));
            bobOffset *= adsFactor;
        } else
            bobOffset = Vector3.zero;


        // ------------------ Weapon Vibration/Recoil ------------------
        // Recoil
        recoilPosOffset = Vector3.Lerp(recoilPosOffset, Vector3.zero, recoilRecoverSpeed * Time.deltaTime);
        recoilAngle = Mathf.Lerp(recoilAngle, 0f, recoilRecoverSpeed * Time.deltaTime) * adsFactor;
        recoilAccum = Mathf.MoveTowards(recoilAccum, 0f, recoilAccumDecay * Time.deltaTime);
        if (fireVibTimer > 0f) fireVibTimer -= Time.deltaTime;

        // Vibration from firing
        Vector3 vib = Vector3.zero;
        if (fireVibTimer > 0f)
        {
            vib = new Vector3(
                (Mathf.PerlinNoise(Time.time * 40f, 0f) - 0.5f) * fireVibrationAmount,
                (Mathf.PerlinNoise(0f, Time.time * 40f) - 0.5f) * fireVibrationAmount,
                0f
            );
        }

        // ------------------ Jump/Landing ------------------
        float tiltTarget = -playerMovement.Velocity.y * 0.1f * jumpTiltAmount;
        jumpTilt = Mathf.Lerp(jumpTilt, tiltTarget, Time.deltaTime * jumpSwaySmooth);

        // Dip while airborne
        float airborneDip = playerState.IsGrounded ? 0f : -jumpPosAmount;
        jumpOffset = Vector3.Lerp(jumpOffset, new Vector3(0f, airborneDip, 0f), Time.deltaTime * jumpSwaySmooth);

        // Landing bump
        if (!wasGrounded && playerState.IsGrounded)
            recoilPosOffset += Vector3.up * landBumpAmount;

        // For next frame
        wasGrounded = playerState.IsGrounded;

        // ------------------ Sliding ------------------
        if (playerState.IsSliding)
        {
            // Backward tilt and positional dip while sliding
            slideTilt = Mathf.Lerp(slideTilt, slideTiltAmount, Time.deltaTime * slideSmooth);
            slideOffset = Vector3.Lerp(slideOffset, new Vector3(0f, -slidePosAmount, 0f), Time.deltaTime * slideSmooth);
        }
        else
        {
            slideTilt = Mathf.Lerp(slideTilt, 0f, Time.deltaTime * slideSmooth);
            slideOffset = Vector3.Lerp(slideOffset, Vector3.zero, Time.deltaTime * slideSmooth);
        }

        // ------------------ FINAL COMPUTATION ---------------------
        // Compose target position
        Vector3 rawTargetPos = baseLocalPos + lookSwayPos + movePosOffset + bobOffset + recoilPosOffset + vib + jumpOffset + slideOffset;
        Vector3 offset = rawTargetPos - baseLocalPos;
        if (offset.magnitude > maxPosOffset) offset = offset.normalized * maxPosOffset;
        Vector3 targetPos = baseLocalPos + offset;

        // Compose target rotation
        float lookYaw = Mathf.Clamp(smoothedLook.x * lookYawMultiplier, -5f, 5f);
        Quaternion targetRot = Quaternion.Euler(
            baseLocalRot.eulerAngles.x - recoilAngle + (leanX * 40f) + jumpTilt + slideTilt,
            baseLocalRot.eulerAngles.y + lookYaw,                      
            baseLocalRot.eulerAngles.z + leanRoll                      
        );

        // Move the weapon
        float posSmoothTime = Mathf.Max(0.001f, 1f / Mathf.Max(0.0001f, swaySmooth));
        weaponPivot.localPosition = Vector3.SmoothDamp(weaponPivot.localPosition, targetPos, ref posVelocitySmooth, posSmoothTime);
        weaponPivot.localRotation = Quaternion.Slerp(weaponPivot.localRotation, targetRot, Mathf.Clamp01(Time.deltaTime * swaySmooth));
    }


    /// <summary>
    /// Add vibration and recoil to the gun movement.
    /// </summary>
    public void OnFire(float shotRecoil = 1f)
    {
        // random kickback
        float kick = Random.Range(recoilKickBackMin, recoilKickBackMax) * shotRecoil;
        recoilPosOffset += Vector3.back * kick;

        // angle push (accumulates with auto fire)
        float angle = recoilAnglePerShot * shotRecoil;
        recoilAngle += angle;
        recoilAccum = Mathf.Clamp(recoilAccum + angle, 0f, recoilAccumulationMax);

        // small vibration impulse
        fireVibTimer = fireVibrationDuration;
    }
}
