using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class InteractionPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI keybindText; // Assign your "[E]" text
    [SerializeField] private TextMeshProUGUI actionText; // Assign your action text
    [SerializeField] private Slider holdProgressBar; // Assign your optional slider
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSpeed = 10f;

    private bool isVisible = false;

    void Awake()
    {
        // Ensure CanvasGroup exists if not assigned
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Start hidden
        canvasGroup.alpha = 0f;
        if (holdProgressBar != null) holdProgressBar.gameObject.SetActive(false);
    }

    void Update()
    {
        // Smoothly fade in/out
        float targetAlpha = isVisible ? 1f : 0f;
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
    }

    public void ShowPrompt(string keybind, string action, bool showHold = false)
    {
        keybindText.text = $"[{keybind}]"; // Display the keybind
        actionText.text = action;        // Display the custom action text
        isVisible = true;

        if (holdProgressBar != null)
        {
            holdProgressBar.gameObject.SetActive(showHold);
            holdProgressBar.value = 0f; // Reset progress
        }
    }

    public void HidePrompt()
    {
        isVisible = false;
        // Optionally, reset text immediately
        // keybindText.text = "";
        // actionText.text = "";
    }

    public void UpdateHoldProgress(float progress) // Progress from 0.0 to 1.0
    {
        if (holdProgressBar != null && holdProgressBar.gameObject.activeSelf)
        {
            holdProgressBar.value = progress;
        }
    }
}
