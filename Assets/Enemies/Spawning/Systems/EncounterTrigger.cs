using UnityEngine;
using TMPro;

public class EncounterTrigger : MonoBehaviour, Interactable
{
    [SerializeField] private EnemySpawnManager spawnManager;
    [SerializeField] private TextMeshPro text;
    public bool CanInteract { get; set; }

    public string InteractionText { get; set; }
    public bool IsHoldInteract { get; set; }
    public float HoldDuration { get; set; }

    [SerializeField] private bool canInteract;
    [SerializeField] private bool isHoldInteract;
    [SerializeField] private float holdDuration;

    void Awake()
    {
        CanInteract = canInteract;
        IsHoldInteract = isHoldInteract;
        HoldDuration = holdDuration;
    }

    void FixedUpdate()
    {
        if (spawnManager.EncounterInProgress == true)
        {
            text.text = "Stop Encounter";
            InteractionText = "Stop Encounter";
        }
        else
        {
            text.text = "Start Encounter";
            InteractionText = "Start Encounter";
        }
    }


    /// <summary>
    /// Called by the player's interaction script when they press the interact key.
    /// </summary>
    public void Interact()
    {
        if (CanInteract)
        {
            if(spawnManager.EncounterInProgress == true)
            {
                spawnManager.StopEncounter();

            } else
            {
                spawnManager.StartEncounter();
            }
        }
        else if (spawnManager == null)
        {
            Debug.LogError("EncounterTrigger cannot find EnemySpawnManager!", this);
        }
    }

    public void OnLookEnter() { /* Highlight effect */ }
    public void OnLookExit() { /* Remove highlight */ }
}