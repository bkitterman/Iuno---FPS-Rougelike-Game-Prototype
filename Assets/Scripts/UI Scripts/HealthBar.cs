using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class HealthBar : MonoBehaviour
{
    [Header("Health Components")]
    [SerializeField] private Slider fillAmount;

    [Header("Status Effect Components")]
    [SerializeField] private GameObject statusIconPrefab;
    [SerializeField] private Transform statusIconContainer;

    private Transform cam;
    private StatusEffectManager statusManager;
    private bool hasIcons = false;
    private Canvas canvas;

    private Dictionary<StatusEffectData, StatusIconUI> _activeIcons = new Dictionary<StatusEffectData, StatusIconUI>();
    private List<StatusEffectData> _keysToRemove = new List<StatusEffectData>();

    void Awake()
    {
        canvas = GetComponent<Canvas>();
    }

    void Start()
    {
        cam = Camera.main.transform;

        fillAmount.value = 1f;
        canvas.enabled = false;

        // Find the StatusEffectManager on the parent object
        statusManager = GetComponentInParent<StatusEffectManager>();

        // Get the Canvas component on this GameObject
        canvas.worldCamera = Camera.main;
    }

    public void SetHealth(float current, float max)
    {
        fillAmount.value = Mathf.Clamp01(current / max);
        canvas.enabled = (fillAmount.value < 1f && fillAmount.value > 0f) ? true : false;
    }

    void LateUpdate()
    {
        // Billboard logic
        transform.LookAt(transform.position + cam.forward);

        // Update the status icons
        UpdateStatusIcons();

        float SCALE_CONSTNAT = 0.1f;
        if (hasIcons && _activeIcons.Count == 0) transform.position += SCALE_CONSTNAT * Vector3.down;
        if (!hasIcons && _activeIcons.Count > 0) transform.position += SCALE_CONSTNAT * Vector3.up;

        hasIcons = _activeIcons.Count > 0;
    }

    private void UpdateStatusIcons()
    {
        if (statusManager == null) return;

        // --- Step A: Remove icons for effects that have expired ---
        foreach (var key in _activeIcons.Keys)
        {
            if (!statusManager.HasEffect(key))
            {
                _keysToRemove.Add(key);
            }
        }

        // Safely remove the marked keys and destroy their GameObjects
        foreach (var key in _keysToRemove)
        {
            Destroy(_activeIcons[key].gameObject);
            _activeIcons.Remove(key);
        }
        _keysToRemove.Clear();


        // --- Step B: Add new icons and update existing ones ---
        foreach (var effect in statusManager.GetActiveEffects())
        {
            StatusEffectData data = effect.GetData();

            // If we don't have an icon for this effect yet, create one
            if (!_activeIcons.ContainsKey(data))
            {
                GameObject newIconObj = Instantiate(statusIconPrefab, statusIconContainer);
                StatusIconUI newIconUI = newIconObj.GetComponent<StatusIconUI>();

                newIconUI.iconArtImage.sprite = data.Icon; 
                _activeIcons.Add(data, newIconUI);
            }

            // Update the stack count
            int stackCount = statusManager.GetStackCount(data);
            if (stackCount > 1)
            { 
                _activeIcons[data].stackCountText.text = stackCount.ToString();
                _activeIcons[data].stackCountText.gameObject.SetActive(true);
            }
            else
            {
                _activeIcons[data].stackCountText.gameObject.SetActive(false);
            }
        }
    }
}