using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 4f;
    public float sprintSpeed = 6f;
    public float jumpHeight = 1.2f;
    public float gravity = -9.81f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("Movement Multipliers")]
    public float groundControl = 1f;
    public float airControl = 0.25f;

    [Header("Camera Settings")]
    public Transform playerCamera;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float crouchTransitionSpeed = 10f;
    public float cameraCrouchOffset = 0.5f;
    public float crouchSpeed = 2f;

    [Header("Slide Settings")]
    public float slideSpeedBoost = 1.2f; 
    public float slideDecay = 2f;        
    public float minSlideSpeed = 2f;    
    
    [Header("Jump Assist Settings")]
    public float coyoteTime = 0.15f;
    public float jumpBufferTime = 0.15f;
    public float variableJumpSoftMultiplier = 1.2f;

    // Objects
    private PlayerState playerState;
    private PlayerInput playerInput;
    private Player player;
    private CharacterController controller;

    // Public States
    public Vector3 Velocity;
    public Vector2 MoveInput;
    public Vector3 HorizontalVelocity;

    // Private States
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private Vector3 slideVelocity;
    private Vector3 camDefaultLocalPos;

    // TEMP
    private bool isMomentumActive = false;
    private Vector3 momentumVelocity;
    [Header("Momentum")]
    public float momentumDrag = 3f;

    void Awake()
    {
        playerState = GetComponent<PlayerState>();
        playerInput = GetComponent<PlayerInput>();
        player = GetComponent<Player>();
        controller = GetComponent<CharacterController>();

        camDefaultLocalPos = playerCamera.transform.localPosition;
    }

    void Start()
    {
        player.Stats.MoveSpeed.BaseValue = moveSpeed;
    }

    void Update()
    {
        HandleMovement();
        HandleCrouch();
    }

    /// <summary>
    /// Handle player movement, including jumping, sprinting, sliding, movement, and falling.
    /// </summary>
    void HandleMovement()
    {
        // Track coyote time
        if (playerState.IsGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Track jump buffering
        if (playerInput.Controls.Player.Jump.triggered)
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        if (playerState.IsGrounded && Velocity.y < 0)
            Velocity.y = -10f;

        // Desired move from input
        Vector3 inputDir = transform.right * MoveInput.x + transform.forward * MoveInput.y;



        Vector3 slopeMoveDir = inputDir;
        if (playerState.IsGrounded)
        {
            slopeMoveDir = GetSlopeMoveDirection(inputDir);
        }

        if (inputDir == Vector3.zero && playerState.IsSprinting) ToggleSprint();

        // Handle sliding if active
        if (playerState.IsSliding)
        {
            // Apply sliding movement only
            Vector3 groundNormal = Vector3.up;
            if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height + 0.5f))
            {
                groundNormal = hit.normal;
            }
            float slopeAngle = Vector3.Angle(Vector3.up, groundNormal);

            // 2. Apply Gravity/Slope Acceleration
             if (slopeAngle > 0 && slopeAngle < controller.slopeLimit)
            {
                // This vector points DOWN the slope
                Vector3 slopeDownDirection = Vector3.ProjectOnPlane(Vector3.down, groundNormal).normalized;

                // Add speed based on how steep it is.
                // Steeper slope = more force.
                slideVelocity += slopeDownDirection * 1f * Time.deltaTime;
            }

            // Apply move
            controller.Move(slideVelocity * Time.deltaTime);

            // Decay slide
            slideVelocity -= slideVelocity * slideDecay * Time.deltaTime;

            // Stop if too slow or conditions break
            if (playerState.IsSprinting && slideVelocity.magnitude < minSlideSpeed + 1f)
            {
                EndSlide();
                ToggleCrouch();
            }
            if (slideVelocity.magnitude < minSlideSpeed || !playerState.IsCrouching)
            {
                EndSlide();
            }
        }
        else if (isMomentumActive)
        {
            // Apply drag
            momentumVelocity = Vector3.Lerp(momentumVelocity, Vector3.zero, momentumDrag * Time.deltaTime);

            // Blend momentum with input
            // If player is inputting movement, blend that in, but keep the high speed
            Vector3 inputVelocity = slopeMoveDir * moveSpeed;

            // If momentum has slowed down to walk speed, switch back to normal control
            if (momentumVelocity.magnitude <= moveSpeed)
            {
                isMomentumActive = false;
            }

            // While momentum is active, use the momentum vector
            controller.Move((momentumVelocity + (inputVelocity * 0.2f)) * Time.deltaTime);
        }
        else
        {

            // Sprinting only allowed on ground
            float speed = player.Stats.MoveSpeed.GetValue();
            
            // Regular grounded/air control
            if (playerState.IsGrounded)
                HorizontalVelocity = slopeMoveDir * speed;
            else
            {
                float airSpeed = Mathf.Min(speed, moveSpeed); // Clamp air speed
                HorizontalVelocity = Vector3.Lerp(HorizontalVelocity, inputDir * airSpeed, airControl * Time.deltaTime * 5f);
            }
            controller.Move(HorizontalVelocity * Time.deltaTime);
        }

        // Jump if within coyote + buffer windows
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            Velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferCounter = 0f;
        }

        // Variable jump height
        if (Velocity.y > 0 && !playerInput.Controls.Player.Jump.IsPressed())
            Velocity.y += gravity * (variableJumpSoftMultiplier - 1f) * Time.deltaTime;

        // Gravity & Better Jumps
        if (Velocity.y < 0)
        {
            Velocity.y += gravity * (fallMultiplier - 1f) * Time.deltaTime;
        }
        else if (Velocity.y > 0 && !playerInput.Controls.Player.Jump.IsPressed()) // released early
        {
            Velocity.y += gravity * (lowJumpMultiplier - 1f) * Time.deltaTime;
        }

        // Always apply base gravity
        Velocity.y += gravity * Time.deltaTime;
        controller.Move(Velocity * Time.deltaTime);
    }

    /// <summary>
    /// Toggle the sprint state. End crouch if needed.
    /// </summary>
    public void ToggleSprint()
    {
        if(playerState.IsSprinting)
        {
            // End Spring
            if (playerState.IsCrouching) ToggleCrouch();

            playerState.SetSprinting(false);

            player.Stats.MoveSpeed.BaseValue = moveSpeed;
        }
        else
        {
            // Start Sprint
            playerState.SetSprinting(true);
            player.Stats.MoveSpeed.BaseValue = sprintSpeed;
        }
        
    }
    
    /// <summary>
    /// Toggle the crouch state, and trigger a slide is sprinting and crouching down. If sliding, end the slide and stand.
    /// </summary>
    public void ToggleCrouch()
    {
        if (!playerState.IsCrouching && playerState.IsSprinting && playerState.IsGrounded)
        {
            StartSlide();
            playerState.SetSprinting(false);
            player.Stats.MoveSpeed.BaseValue = moveSpeed;
        }
        else
        {
            if (playerState.IsCrouching)
            {
                playerState.SetCrouching(false);

                if(playerState.IsSprinting) player.Stats.MoveSpeed.BaseValue = sprintSpeed;
                else player.Stats.MoveSpeed.BaseValue = moveSpeed;
            }
            else
            {
                playerState.SetCrouching(true);
                player.Stats.MoveSpeed.BaseValue = crouchSpeed;
                if (playerState.IsSprinting) 
                    EndSlide();
            }
        }
    }

    /// <summary>
    /// Handle the crouch state, and move camera to desired position.
    /// </summary>
    void HandleCrouch()
    {
        float targetHeight = playerState.IsCrouching ? crouchHeight : standingHeight;
        controller.height = Mathf.Lerp(controller.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        controller.center = new Vector3(0,controller.height / 2f,0);

        // Move camera up/down
        Vector3 camLocalPos = playerCamera.localPosition;
        float targetY = playerState.IsCrouching ? camDefaultLocalPos.y - cameraCrouchOffset : camDefaultLocalPos.y;
        camLocalPos.y = Mathf.Lerp(camLocalPos.y, targetY, Time.deltaTime * crouchTransitionSpeed);
        playerCamera.localPosition = camLocalPos; 
    }

    /// <summary>
    /// Start the slide state and compute the initial slide speed.
    /// </summary>
    void StartSlide()
    {
        playerState.SetSliding(true);
        playerState.SetCrouching(true);

        float currentSpeed = HorizontalVelocity.magnitude;
        float targetSpeed = currentSpeed > moveSpeed ? currentSpeed * slideSpeedBoost : minSlideSpeed;

        // Use input direction if pressing keys, otherwise forward
        Vector3 slideDir = (MoveInput.magnitude > 0)
            ? (transform.right * MoveInput.x + transform.forward * MoveInput.y).normalized
            : transform.forward;

        slideVelocity = slideDir * targetSpeed;
    }

    /// <summary>
    /// End the slide state
    /// </summary>
    void EndSlide()
    {
        HorizontalVelocity = slideVelocity;
        //isMomentumActive = true;
        //momentumVelocity = slideVelocity;

        playerState.SetSliding(false);

    }

    /// <summary>
    /// Get the current slope direction
    /// </summary>
    /// <param name="direction"></param>
    /// <returns></returns>
    private Vector3 GetSlopeMoveDirection(Vector3 direction)
    {
        // Cast a ray down to find the ground normal
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, controller.height / 2 + 0.5f))
        {
            // Project our horizontal move direction onto the slope's normal
            return Vector3.ProjectOnPlane(direction, hit.normal).normalized;
        }
        return direction;
    }
}
