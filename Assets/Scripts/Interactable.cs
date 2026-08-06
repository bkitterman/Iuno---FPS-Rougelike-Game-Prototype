using UnityEngine;

public interface Interactable
{
    bool CanInteract { get; set; }
    string InteractionText { get; set; }
    bool IsHoldInteract { get; set; }
    float HoldDuration { get; set; }


    /// <summary>
    /// Called by the player's interaction script when they press the interact key.
    /// </summary>
    public void Interact();

    public void OnLookEnter();
    public void OnLookExit();
}
