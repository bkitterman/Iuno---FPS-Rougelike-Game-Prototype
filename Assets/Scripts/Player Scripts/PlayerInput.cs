using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float interactRange = 3f;
    [SerializeField] private LayerMask interactableMask;

    [Header("Interaction UI")]
    [SerializeField] private InteractionPopup promptUI;
    [SerializeField] private InputActionReference interactAction;  

    // Hold interaction state
    private float holdTimer = 0f;
    private bool isHoldingInteract = false;

    public Player Player;
    public PlayerControls Controls;
    public GameObject InventoryObject;
    private AbilityManager abilityManager;

    private PlayerMovement playerMovement;
    private PlayerMelee playerMelee;
    private PlayerLook playerLook;
    private PlayerState playerState;

    bool inventoryActive = false;
    bool cursorLocked = true;
    bool cursorVisible = false;

    private Interactable currentLookTarget;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Controls = new PlayerControls();
        playerMovement = GetComponent<PlayerMovement>();
        playerLook = GetComponent<PlayerLook>();
        playerMelee = GetComponent<PlayerMelee>();
        playerState = GetComponent<PlayerState>();
        abilityManager = GetComponent<AbilityManager>();
    }
     
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        InventoryObject.SetActive(false);
    }

    void OnEnable()
    {
        Controls.Player.Enable();
        Controls.Inventory.Enable();

        // Movement and Looks
        Controls.Player.Move.performed += ctx => playerMovement.MoveInput = ctx.ReadValue<Vector2>();
        Controls.Player.Move.canceled += ctx => playerMovement.MoveInput = Vector2.zero;

        Controls.Player.Look.performed += ctx => playerLook.LookInput = ctx.ReadValue<Vector2>();
        Controls.Player.Look.canceled += ctx => playerLook.LookInput = Vector2.zero;

        Controls.Player.Crouch.performed += ctx => playerMovement.ToggleCrouch();
        Controls.Player.Sprint.performed += ctx => playerMovement.ToggleSprint();

        // Gun
        Controls.Player.Fire.performed += ctx => Player.EquippedWeapon?.PrimaryFire(false);
        Controls.Player.Reload.performed += ctx => Player.EquippedWeapon?.Reload();
        Controls.Player.SwitchFireMode.performed += ctx => Player.EquippedWeapon?.SwitchMode();

        Controls.Player.SecondaryFire.performed += ctx => Player.EquippedWeapon?.SecondaryFire(true);
        Controls.Player.SecondaryFire.canceled += ctx => Player.EquippedWeapon?.SecondaryFire(false);

        Controls.Player.SwapWeapon.performed += ctx =>
        {
            Vector2 scrollDelta = ctx.ReadValue<Vector2>();

            if (scrollDelta.y > 0)
                Player.SwapWeapon(1, true);
            else if (scrollDelta.y < 0)
                Player.SwapWeapon(-1, true);
        };

        Controls.Player.SwapWeapon1.performed += ctx => Player.SwapWeapon(0, false);
        Controls.Player.SwapWeapon2.performed += ctx => Player.SwapWeapon(1, false);
        Controls.Player.SwapWeapon3.performed += ctx => Player.SwapWeapon(2, false);

        // Abilities
        Controls.Player.Melee.performed += ctx => playerMelee.TryMelee();

        Controls.Player.Ability1.performed += ctx => abilityManager.OnAbility1Pressed();
        Controls.Player.Ability2.performed += ctx => abilityManager.OnAbility2Pressed();
        Controls.Player.Ability3.performed += ctx => abilityManager.OnAbility3Pressed();

        // UI
        Controls.Inventory.Inventory.performed += ctx => ToggleInventory();
        Controls.Inventory.Menu.performed += ctx =>
        {
            if (inventoryActive) ToggleInventory();
            // Menu
        };

        // Interaction
        Controls.Player.Interact.performed += HandleInteractPerformed;
        Controls.Player.Interact.canceled += HandleInteractCanceled;
    }

    void OnDisable() 
    {
        Controls.Player.Disable(); 
        if (promptUI != null) promptUI.HidePrompt(); 
    }

    void Update()
    {
        // For fires auto flying.
        if (Player.EquippedWeapon != null && Controls.Player.Fire.IsPressed())
        {
            playerState.SetFiring(true);
            Player.EquippedWeapon?.PrimaryFire(true);
        }
        else playerState.SetFiring(false);

        CheckForInteractable();

        // Handle hold interaction logic
        if (isHoldingInteract && currentLookTarget != null && currentLookTarget.IsHoldInteract)
        {
            holdTimer += Time.deltaTime;
            // Update UI progress (already done in CheckForInteractable)
            // promptUI.UpdateHoldProgress(holdTimer / currentLookTarget.holdDuration);

            if (holdTimer >= currentLookTarget.HoldDuration)
            {
                // Hold complete! Trigger interaction
                currentLookTarget.Interact();
                isHoldingInteract = false; // Stop holding
                holdTimer = 0f;
                // Optionally hide prompt or give success feedback
            }
        }
    }


    public void ToggleInventory()
    {
        inventoryActive = !inventoryActive;
        cursorLocked = !cursorLocked;
        cursorVisible = !cursorVisible;

        if (inventoryActive) Controls.Player.Disable();
        else Controls.Player.Enable();

            Cursor.lockState = cursorLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = cursorVisible;
        InventoryObject.SetActive(inventoryActive);
    }

    void CheckForInteractable() //
    {
        Interactable hitTrigger = null;
        RaycastHit hit;

        if (Physics.Raycast(cameraTransform.position, cameraTransform.forward, out hit, interactRange, interactableMask))
        {
            hitTrigger = hit.collider.GetComponent<Interactable>();
        }

        if (hitTrigger != currentLookTarget)
        {
            currentLookTarget?.OnLookExit();
            // Reset hold state ONLY if we were actually holding
            if (isHoldingInteract)
            {
                isHoldingInteract = false;
                holdTimer = 0f;
            }

            currentLookTarget = hitTrigger;

            if (currentLookTarget != null && currentLookTarget.CanInteract)
            {
                currentLookTarget.OnLookEnter();
                string keybind = interactAction.action.GetBindingDisplayString();
                promptUI.ShowPrompt(keybind, currentLookTarget.InteractionText, currentLookTarget.IsHoldInteract);
            }
            else
            {
                promptUI.HidePrompt();
            }
        }

        // Update hold progress visualization
        if (currentLookTarget != null && currentLookTarget.IsHoldInteract)
        {
            // Update progress only if holding, otherwise show 0
            promptUI.UpdateHoldProgress(isHoldingInteract ? (holdTimer / currentLookTarget.HoldDuration) : 0f);
        }
    }

    // Called when the interact button is PRESSED DOWN
    private void HandleInteractPerformed(InputAction.CallbackContext c)
    {
        if (currentLookTarget != null && currentLookTarget.CanInteract)
        {
            if (currentLookTarget.IsHoldInteract)
            {
                isHoldingInteract = true;
                holdTimer = 0f; // Start timer
            }
            else
            {
                // It's a simple press interact
                currentLookTarget.Interact();
            }
        }
    }

    // Called when the interact button is RELEASED
    private void HandleInteractCanceled(InputAction.CallbackContext c)
    {
        // Reset hold state regardless of success
        isHoldingInteract = false;
        holdTimer = 0f;
        // Reset progress bar visually if needed (might already be handled by CheckForInteractable)
        if (currentLookTarget != null && currentLookTarget.IsHoldInteract) promptUI.UpdateHoldProgress(0f);
    }
}
