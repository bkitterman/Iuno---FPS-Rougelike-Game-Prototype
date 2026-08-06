using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneChangeTrigger : MonoBehaviour, Interactable
{
    [Header("Interaction Settings")]
    [SerializeField] private TextMeshPro text;
    public string altarText = "To Test Level";

    [Header("Scene")]
    [SerializeField] private string sceneToLoad;

    [Header("Interface")]
    public bool IsHoldInteract { get; set; }
    public string InteractionText { get; set; }
    public float HoldDuration { get; set; }
    public bool CanInteract { get; set; }

    [SerializeField] private bool canInteract;
    [SerializeField] private bool isHoldInteract;
    [SerializeField] private float holdDuration;
    [SerializeField] private string interactionText;

    void Awake()
    {
        CanInteract = canInteract;
        IsHoldInteract = isHoldInteract;
        HoldDuration = holdDuration;
        InteractionText = interactionText;
    }

    /// <summary>
    /// Called by PlayerInteraction when the action is complete.
    /// </summary>
    public void Interact()
    {
        if (!CanInteract) return;

        SceneManager.LoadScene(sceneToLoad);
    }

    public void OnLookEnter() { /* add highlight logic here */ }
    public void OnLookExit() { /* remove highlight logic here */ }
}