using UnityEngine;

public class PlayerState : MonoBehaviour
{
    public bool IsSprinting { get; private set; }
    public bool IsAiming { get; private set; }
    public bool IsGrounded { get; private set; }
    public bool IsSliding { get; private set; }
    public bool IsFiring { get; private set; }
    public bool IsCrouching { get; private set; }
    public bool IsReloading { get; private set; }
    public bool IsBlocking { get; private set; }

    private CharacterController controller;

    void Awake()
    {
        IsSprinting = false;
        IsAiming = false;
        IsGrounded = false;
        IsSliding = false;
        IsCrouching = false;
        IsSliding = false;
        IsBlocking = false;

        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        IsGrounded = controller.isGrounded;
    }

    public void SetSprinting(bool isSprinting) {  IsSprinting = isSprinting; }
    public void SetAiming(bool isAiming) { IsAiming = isAiming; }
    public void SetSliding(bool isSliding) { IsSliding = isSliding; }
    public void SetFiring(bool isFiring) { IsFiring = isFiring; }
    public void SetCrouching(bool isCrouching) { IsCrouching = isCrouching; }
    public void SetReloading(bool isReloading) { IsReloading = isReloading; }
    public void SetBlocking(bool isBlocking) { IsBlocking = isBlocking; }
}
