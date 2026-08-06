using UnityEngine;
using TMPro;

public class DamagePopup : MonoBehaviour
{
    public float floatSpeed = 2f;
    public float lifeTime = 1f;

    private float spawnTime;
    private TextMeshProUGUI text; // Changed to the UI version
    private RectTransform rectTransform;
    private Camera mainCamera;

    // --- We now have two "targets" ---
    private Transform followTarget;     // For moving enemies (DoTs)
    private Vector3 staticWorldPos;   // For static hit points
    private Vector3 randomOffset;

    private Vector3 horizontalDirection;

    // --- Init for a TRANSFORM (DoTs, active enemies) ---
    public void Init(Transform target, float damage, bool precision)
    {
        CommonInit(damage, precision);
        followTarget = target;
    }

    // --- Init for a static POSITION (Initial hit) ---
    public void Init(Vector3 worldPosition, float damage, bool precision)
    {
        CommonInit(damage, precision);
        staticWorldPos = worldPosition;
    }

    // Common setup logic
    private void CommonInit(float damage, bool precision)
    {
        text = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();
        mainCamera = Camera.main;
        spawnTime = Time.time;

        text.text = $"{(precision ? "<color=\"yellow\"><size=\"22\">" : "")}{damage:F0}";

        horizontalDirection = Random.value < 0.5f ? Vector2.left : Vector2.right;

        // Random offset for visual flair
        randomOffset = new Vector3(
            horizontalDirection.x < 0 ? Random.Range(-0.3f, -0.15f) : Random.Range(0.15f, 0.3f),
            Random.Range(-0.1f, 0.1f), 0f);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        // --- 1. Follow the Target ---
        Vector3 worldPos;
        if (followTarget != null)
        {
            worldPos = followTarget.position;
        }
        else
        {
            worldPos = staticWorldPos;
        }

        // Convert the target's world position to a screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos + randomOffset);

        // Check if the target is behind the camera
        if (screenPos.z <= 0)
        {
            // Target is behind the camera, make the popup invisible
            text.enabled = false;
        }
        else
        {
            text.enabled = true; 
            rectTransform.position = screenPos;

            // --- 2. Animate and Fade ---
            float elapsed = Time.time - spawnTime;
            float t = Mathf.Clamp01(elapsed / lifeTime);

            // Move
            float speedFactor = 1f - t;
            randomOffset += horizontalDirection * floatSpeed * speedFactor * Time.deltaTime * 0.1f;

            // Fade alpha
            Color c = text.color;
            c.a = 1f - Mathf.Pow(t, 2);
            text.color = c;
        }
    }
}
