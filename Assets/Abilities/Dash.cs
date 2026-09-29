using UnityEngine;

public class Dash : AbilityInstance
{
    // Interface Properties
    public AbilityData Data { get; set; }
    public bool IsOnCooldown => CurrentCharges < Data.charges;
    public float CooldownRemaining { get; private set; }
    public int CurrentCharges { get; private set; }

    // Dash state
    private bool isDashing = false;
    private float dashTimer = 0f;
    private float dashDuration = 0.2f;
    private Vector3 dashVelocity;

    // Player references
    private CharacterController controller;
    private Transform playerTransform;
    private PlayerMovement playerMovement;

    /// <summary>
    /// Called every frame by ability manager
    /// </summary>
    public void AbilityUpdate()
    {
        // Tick cooldown timer
        if (CurrentCharges < Data.charges)
        {
            CooldownRemaining -= Time.deltaTime;

            // If cooldown finished, gain a charge
            if (CooldownRemaining <= 0f)
            {
                CurrentCharges++;
                // If still missing charges, start the cooldown for the next one
                if (CurrentCharges < Data.charges)
                {
                    CooldownRemaining = Data.Cooldown;
                }
                else
                {
                    CooldownRemaining = 0f;
                }
            }
        }

        // Handle dash movement
        if (isDashing)
        {
            if (dashTimer > 0)
            {
                // Apply the dash movement using the CharacterController
                controller.Move(dashVelocity * Time.deltaTime);
                dashTimer -= Time.deltaTime;
            }
            else
            {
                isDashing = false;
            }
        }
    }

    /// <summary>
    /// Called when the ability is first equipped to the player.
    /// </summary>
    public void OnEquip(GameObject owner)
    {
        controller = owner.GetComponent<CharacterController>();
        playerTransform = owner.transform;
        playerMovement = owner.GetComponent<PlayerMovement>();
        CurrentCharges = Data.charges;
        CooldownRemaining = 0f;        
    }

    /// <summary>
    /// Called when the player presses the ability button.
    /// </summary>
    public void TryActivate()
    {
        if (isDashing || CurrentCharges <= 0) return;

        if (CurrentCharges == Data.charges)
        {
            CooldownRemaining = Data.Cooldown;
        }

        // 2. Set Dash State
        isDashing = true;
        dashTimer = dashDuration;
        CurrentCharges--;


        // 3. Get Dash Direction
        Vector3 inputDir = playerTransform.right * playerMovement.MoveInput.x + playerTransform.forward * playerMovement.MoveInput.y;
        Vector3 dashDirection;

        if (inputDir.magnitude > 0.1f)
            dashDirection = inputDir.normalized; // Dash in move direction
        else
            dashDirection = playerMovement.playerCamera.forward; // Dash in look direction

        // 4. Calculate Dash Velocity
        float dashSpeed = Data.Value / dashDuration; // Data.Value is distance
        dashVelocity = dashDirection * dashSpeed;

        // Add a small upward boost to prevent snagging on the floor
        dashVelocity.y = 2f;
    }

    /// <summary>
    /// Called when the ability is unequipped.
    /// </summary>
    public void OnUnequip()
    {

    }
}
