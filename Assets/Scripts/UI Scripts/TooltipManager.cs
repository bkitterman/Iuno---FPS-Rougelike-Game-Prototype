using UnityEngine;
using System.Collections;

public enum TooltipPosition { Above, Below, Left, Right }

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [SerializeField] private Tooltip tooltip; 
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform canvasRect; // The main UI Canvas RectTransform
    [SerializeField] private float fadeSpeed = 15f;
    [SerializeField] private float offset = 30f; // Pixel offset from the icon

    private Coroutine fadeCoroutine;
    private Transform followTarget;
    private TooltipPosition position;
    private Vector2 offsetPos;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Get components from the tooltip prefab
        canvasGroup = tooltip.GetComponent<CanvasGroup>();
        tooltip.gameObject.SetActive(false); // Start hidden
    }

    void Update()
    {
        if (tooltip.gameObject.activeSelf && followTarget != null)
        {
            // This makes the tooltip follow the icon
            UpdateTooltipPosition();
        }
    }

    public void ShowTooltip(string name, string type, string desc, Transform target, TooltipPosition pos, Vector2 offset = default)
    {
        tooltip.SetText(name, type, desc);
        tooltip.SetDirection(pos);
        followTarget = target;
        position = pos;
        offsetPos = offset;
        tooltip.gameObject.SetActive(true);
        UpdateTooltipPosition(); // Set initial position

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTooltip(1f));
    }

    public void HideTooltip()
    {
        followTarget = null;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeTooltip(0f));
    }

    private void UpdateTooltipPosition()
    {
        Vector2 targetPos = followTarget.position;
        Vector2 pivot;
        Vector2 newOffset;

        // Set pivot and offset based on desired position
        switch (position)
        {
            case TooltipPosition.Above:
                pivot = new Vector2(0.5f, 0f); // Pivot at bottom-center
                newOffset = new Vector2(0, offset);
                break;
            case TooltipPosition.Below:
                pivot = new Vector2(0.5f, 1f); // Pivot at top-center
                newOffset = new Vector2(0, -offset);
                break;
            case TooltipPosition.Left:
                pivot = new Vector2(1f, 0.5f); // Pivot at middle-right
                newOffset = new Vector2(-offset, 0);
                break;
            default: // Right
                pivot = new Vector2(0f, 0.5f); // Pivot at middle-left
                newOffset = new Vector2(offset, 0);
                break;
        }

        tooltip.GetComponent<RectTransform>().pivot = pivot;
        tooltip.transform.position = targetPos + newOffset + offsetPos;

        // Optional: Add logic to keep it on-screen
        // ... (check if tooltipRect is outside canvasRect and flip position)
    }

    private IEnumerator FadeTooltip(float targetAlpha)
    {
        while (!Mathf.Approximately(canvasGroup.alpha, targetAlpha))
        {
            canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
            yield return null;
        }

        if (targetAlpha == 0)
        {
            tooltip.gameObject.SetActive(false); // Fully hide
        }
    }
}