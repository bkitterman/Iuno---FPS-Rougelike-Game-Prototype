using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    [SerializeField] private Slider fillAmount;
    [SerializeField] private float lerpSpeed = 5f;
    [SerializeField] private CanvasGroup canvasGroup;

    private Transform cam;
    private float targetValue;

    private void Awake()
    {
        cam = Camera.main.transform;

        fillAmount.value = 1f;
        targetValue = 1f;
        canvasGroup.alpha = 0f;
    }

    public void SetHealth(float current, float max)
    {
        targetValue = Mathf.Clamp01(current / max);
        canvasGroup.alpha = targetValue < 1f ? 1f : 0f;
    }

    private void Update()
    {
        if (!Mathf.Approximately(fillAmount.value, targetValue))
        {
            fillAmount.value = Mathf.Lerp(fillAmount.value, targetValue, Time.deltaTime * lerpSpeed);
        }
    }

    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.forward);
    }
}
