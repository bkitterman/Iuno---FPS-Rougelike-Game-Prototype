using UnityEngine;

public class PlayerAudio : MonoBehaviour
{
    [Header("Footsteps")]
    [SerializeField] private AudioSource footstepSource;
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float baseStepInterval = 0.5f;
    [SerializeField] private float sprintStepMultiplier = 0.7f;
    [SerializeField] private float crouchStepMultiplier = 1.5f;

    float footstepTimer;
    private PlayerState playerState;
    private PlayerMovement playerMovement;

    void Awake()
    {
        playerState = GetComponent<PlayerState>();
        playerMovement = GetComponent < PlayerMovement>();
    }

    void Update()
    {
        HandleFootsteps();
    }

    /// <summary>
    /// Play footsteps when able, defined by clip speed.
    /// </summary>
    void HandleFootsteps()
    {
        if (!playerState.IsGrounded || playerState.IsSliding || playerMovement.MoveInput.magnitude < 0.1f) return;

        float interval = baseStepInterval;
        if (playerState.IsSprinting) interval *= sprintStepMultiplier;
        if (playerState.IsCrouching) interval *= crouchStepMultiplier;

        footstepTimer -= Time.deltaTime;
        if (footstepTimer <= 0f)
        {
            PlayFootstep();
            footstepTimer = interval;
        }
    }

    /// <summary>
    /// Play the footstep sound from a random clip in the array.
    /// </summary>
    void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;
        int i = Random.Range(0, footstepClips.Length);
        footstepSource.PlayOneShot(footstepClips[i]);
    }

}
