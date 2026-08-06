using UnityEngine;

public class PlayerLook : MonoBehaviour
{
    [Header("Camera Settings")]
    public Transform playerCamera;
    public float mouseSensitivityX = 2f;
    public float mouseSensitivityY = 1f;
    public float mouseADSReduction = 0.7f;
    public float recoilRecovery = 10f;

    [Header("Recoil Settings")]
    public float recoilVisualSpeed = 14f;   // how fast the camera visually follows accumulator
    public float recoilRecoverRate = 6f;    // deg per sec linear recovery of accumulator
    public float maxRecoilPitch = 40f;
    public float maxRecoilYaw = 25f;

    [Header("Head Sway and Bob")]
    [SerializeField] private float walkBobSpeed = 8f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 12f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 4f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    [SerializeField] private float idleSwayAmount = 0.01f;
    [SerializeField] private float idleSwaySpeed = 1.5f;

    [Header("ADS settings")]
    public float hipFOV = 60f;
    public float adsFOV = 45f;
    public float fovSmoothSpeed = 10f;
    public float fadeSpeed = 10f;

    [Header("Aim Assist Settings")]
    [SerializeField] private float aimAssistSlowdown = 0.5f; 
    [SerializeField] private LayerMask enemyHitboxLayer;

    [Header("Flinch Settings")]
    [SerializeField] private float flinchAmount = 4f;       
    [SerializeField] private float flinchVisualSpeed = 10f;  
    [SerializeField] private float flinchRecoverRate = 12f;   
    [SerializeField] private float maxFlinchPitch = 15f;    
    [SerializeField] private float maxFlinchYaw = 10f;

    public Vector2 LookInput;
    public Camera PlayerView;
    public Vector2 RecoilAccumulator;

    private Player player;
    private Vector2 flinchAccumulator;
    private Vector2 flinchVisual;

    private Vector2 recoilOffset = Vector2.zero;
    private Vector2 recoilVisual = Vector2.zero;
    private float pitch;
    private float yaw;
    private float bobTimer;

    private PlayerMovement playerMovement;
    private PlayerState playerState;
    private Vector3 cameraDefaultLocalPos;

    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerState = GetComponent<PlayerState>();
        player = GetComponent<Player>();
    }
    
    void Start()
    {
        cameraDefaultLocalPos = playerCamera.localPosition;
        GameEvents.OnPlayerDamageTaken += DamageFlinch;
    }

    void Update()
    {
        HandleHeadSway();

        float targetFOV = playerState.IsAiming ? (player.EquippedWeapon.Data.AdsFov != 0 ? player.EquippedWeapon.Data.AdsFov : adsFOV) : hipFOV;
        PlayerView.fieldOfView = Mathf.Lerp(PlayerView.fieldOfView, targetFOV, Time.deltaTime * fovSmoothSpeed);
    }
    
    void LateUpdate()
    {
        HandleMouseLook();
    }

    /// <summary>
    /// Move the camera to match mouse movement, and add recoil if there is any present.
    /// </summary>
    void HandleMouseLook()
    {
        bool lookingAtEnemy = false;
        if (Physics.Raycast(playerCamera.position, playerCamera.forward, player.EquippedWeapon.Data.Range, enemyHitboxLayer))
        {
            lookingAtEnemy = true;
        }

        // Get Mouse movement
        float mx = LookInput.x * mouseSensitivityX * 
            (playerState.IsAiming ? mouseADSReduction : 1f) *
            (lookingAtEnemy ? aimAssistSlowdown : 1f);
        float my = LookInput.y * mouseSensitivityY * 
            (playerState.IsAiming ? mouseADSReduction : 1f) *
            (lookingAtEnemy ? aimAssistSlowdown : 1f);

        yaw += mx;
        pitch -= my;

        // Prevent camera from rotating over/under player
        pitch = Mathf.Clamp(pitch, -85f, 85f);

        // 1) Smoothly move the visual recoil toward the accumulator (fast lerp)
        recoilVisual = Vector2.Lerp(recoilVisual, RecoilAccumulator, Time.deltaTime * recoilVisualSpeed);

        // Auto-recovery
        if (!playerState.IsFiring && RecoilAccumulator != Vector2.zero)
        {
            // If no mouse movement, return to center
            if (LookInput.x == 0 && LookInput.y == 0)
                RecoilAccumulator = Vector2.MoveTowards(RecoilAccumulator, Vector2.zero, recoilRecoverRate * Time.deltaTime);
            else
            {
                // If mouse moves, cancel recovery in favor of player movement
                RecoilAccumulator = Vector2.zero;
                pitch -= recoilVisual.y;
                yaw += recoilVisual.x;
                recoilVisual = Vector2.zero;
            }
        }

        // Flinch
        flinchVisual = Vector2.Lerp(flinchVisual, flinchAccumulator, Time.deltaTime * flinchVisualSpeed);
        flinchAccumulator = Vector2.MoveTowards(flinchAccumulator, Vector2.zero, flinchRecoverRate * Time.deltaTime);
        
        float appliedPitch = pitch - recoilVisual.y - flinchVisual.y;
        float appliedYaw = yaw + recoilVisual.x + flinchVisual.x; 

        playerCamera.localRotation = Quaternion.Euler(appliedPitch, 0f, 0f);
        transform.rotation = Quaternion.Euler(0f, appliedYaw, 0f);
    }

    /// <summary>
    /// Add breathing/body sway to the player during movement.
    /// </summary>
    void HandleHeadSway()
    {
        Vector3 targetPos = cameraDefaultLocalPos;

        if (playerState.IsGrounded && playerMovement.MoveInput.magnitude > 0.1f)
        {
            float speed, amount;
            if (playerState.IsSprinting) { speed = sprintBobSpeed; amount = sprintBobAmount; }
            else if (playerState.IsCrouching) { speed = crouchBobSpeed; amount = crouchBobAmount; }
            else { speed = walkBobSpeed; amount = walkBobAmount; }

            bobTimer += Time.deltaTime * speed;
            targetPos += new Vector3(
                Mathf.Cos(bobTimer) * amount,
                Mathf.Abs(Mathf.Sin(bobTimer)) * amount,
                0f
            );
        }
        else
        {
            // idle breathing
            bobTimer += Time.deltaTime * idleSwaySpeed;
            targetPos += new Vector3(0f, Mathf.Sin(bobTimer) * idleSwayAmount, 0f);
        }

        playerCamera.localPosition = Vector3.Lerp(
            playerCamera.localPosition, targetPos, Time.deltaTime * 10f
        );
    }

    /// <summary>
    /// Add recoil to the player's camera.
    /// 
    /// </summary>
    /// <param name="recoilDeg">The amount of recoil to add, in degrees of X,Y. </param>
    public void AddRecoil(Vector2 recoilDeg)
    {
        // This assumes recoilDeg.y is positive to push view up.
        RecoilAccumulator += recoilDeg;

        // clamp so player can't flip view
        RecoilAccumulator.y = Mathf.Clamp(RecoilAccumulator.y, 0f, maxRecoilPitch);
        RecoilAccumulator.x = Mathf.Clamp(RecoilAccumulator.x, -maxRecoilYaw, maxRecoilYaw);
    }

    private void DamageFlinch(GameObject source, Vector3 hitPoint, float damage)
    {
        // Calculate the direction from the player to the damage source
        Vector3 dirToSource = hitPoint - transform.position;

        // Convert this world-space direction into the player's local space.
        // localDir.x > 0 means the hit was from the RIGHT.
        // localDir.x < 0 means the hit was from the LEFT.
        Vector3 localDir = transform.InverseTransformDirection(dirToSource.normalized);

        // --- Calculate the Flinch Kicks ---
        float verticalKick = flinchAmount * Mathf.Min(damage, 100) / 150f;
        float horizontalKick = localDir.x * (flinchAmount * 0.5f);

        // Add the kick to the accumulator
        flinchAccumulator += new Vector2(horizontalKick, verticalKick) * (playerState.IsAiming ? 0.5f : 1f);

        // Clamp the accumulator
        flinchAccumulator.y = Mathf.Clamp(flinchAccumulator.y, 0f, maxFlinchPitch);
        flinchAccumulator.x = Mathf.Clamp(flinchAccumulator.x, -maxFlinchYaw, maxFlinchYaw);
    }
}
