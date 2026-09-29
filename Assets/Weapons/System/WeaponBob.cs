using UnityEngine;

public class WeaponBob : MonoBehaviour
{
    [Header("Bob Settings")]
    [SerializeField] private float walkBobSpeed = 6f;
    [SerializeField] private float walkBobAmount = 0.02f;
    [SerializeField] private float sprintBobSpeed = 10f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float crouchBobSpeed = 4f;
    [SerializeField] private float crouchBobAmount = 0.025f;

    [Header("Rotational Bob")]
    [SerializeField] private float rollAmount = 2f;
    [SerializeField] private float pitchAmount = 1f;

    [Header("ADS Damping")]
    [SerializeField] private float adsMultiplier = 0.15f;

    public Vector3 positionOffset { get; private set; }
    public Quaternion rotationOffset { get; private set; }

    // Private internal state
    private float bobTimer = 0f;
    private Vector3 lastPosOffset;
    private Quaternion lastRotOffset;

    private PlayerState playerState;
    private PlayerMovement playerMovement;

    void Start()
    {
        playerState = GetComponentInParent<PlayerState>();
        playerMovement = GetComponentInParent<PlayerMovement>();
    }

    void Update()
    {
        // --- 1. Get Player State ---
        Vector2 moveInput = playerMovement.MoveInput;
        bool isGrounded = playerState.IsGrounded;
        bool isSprinting = playerState.IsSprinting;
        bool isCrouching = playerState.IsCrouching;
        bool isAiming = playerState.IsAiming;

        // --- 2. Get Bob Settings ---
        float speed, amount;
        if (isSprinting)
        {
            speed = sprintBobSpeed;
            amount = sprintBobAmount;
        }
        else if (isCrouching)
        {
            speed = crouchBobSpeed;
            amount = crouchBobAmount;
        }
        else
        {
            speed = walkBobSpeed;
            amount = walkBobAmount;
        }

        // --- 3. Calculate Bob Offsets ---
        Vector3 pos = Vector3.zero;
        Quaternion rot = Quaternion.identity;

        if (isGrounded && moveInput.magnitude > 0.1f)
        {
            bobTimer += Time.deltaTime * speed;

            // Use Cos for horizontal (X) and Sin for vertical (Y)
            float xPos = Mathf.Cos(bobTimer) * amount;
            float yPos = Mathf.Abs(Mathf.Sin(bobTimer)) * amount;

            // Calculate rotational bob
            float xRot = Mathf.Abs(Mathf.Sin(bobTimer)) * pitchAmount;
            float zRot = -Mathf.Cos(bobTimer) * rollAmount; 

            pos = new Vector3(xPos, yPos, 0f);
            rot = Quaternion.Euler(xRot, 0f, zRot);
        }
        else
        {
            // Reset timer if not moving or airborne
            bobTimer = 0f;
        }

        // --- 4. Apply ADS Damping ---
        float adsFactor = isAiming ? adsMultiplier : 1f;

        // Smoothly interpolate the offsets
        lastPosOffset = Vector3.Lerp(lastPosOffset, pos * adsFactor, Time.deltaTime * 10f);
        lastRotOffset = Quaternion.Slerp(lastRotOffset, isAiming ? Quaternion.Euler(0f,0f,0f) : rot, Time.deltaTime * 10f);

        // --- 5. Set Public Properties ---
        positionOffset = lastPosOffset;
        rotationOffset = lastRotOffset;
    }
}