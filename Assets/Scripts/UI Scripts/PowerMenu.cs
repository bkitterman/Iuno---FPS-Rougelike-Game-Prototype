using UnityEngine;
using UnityEngine.UI;

using TMPro;

using System.Collections;

public class PowerMenu : MonoBehaviour
{
    [Header("Drawer Components")]
    [SerializeField] private RectTransform drawerRect;
    [SerializeField] private float animationSpeed = 15f;
    [SerializeField] private float openWidth = 350f;
    [SerializeField] private PSUHolder_UI holder;

    [Header("Click-Away Blocker")]
    [SerializeField] private GameObject clickBlockerMain;
    [SerializeField] private GameObject clickBlockerWeapon;

    [Header("Drawer Population")]
    [SerializeField] private PSUDatabase psuDatabase;
    [SerializeField] private GameObject psuIconPrefab;
    [SerializeField] private Transform allPSUsGrid;
    [SerializeField] private Player player;
    [SerializeField] private PlayerHardwareManager hardwareManager;

    // Drawer
    private bool isDrawerOpen = false;
    private Coroutine animationCoroutine;
    private float openXPos;

    private Image mainBlockImage;
    private Image weaponBlockImage;

    [Header("Feedback Components")]
    [SerializeField] private TextMeshProUGUI powerText;
    [SerializeField] private Slider powerBar;
    [SerializeField] private Slider powerHighlight;
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private Image PowerBarFill;
    [SerializeField] private Color errorColor = Color.red;

    [Header("Animation Settings")]
    [SerializeField] private float barLerpSpeed = 5f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseMinAlpha = 0.5f;
    [SerializeField] private float pulseMaxAlpha = 1f;
    [SerializeField] private Color addPreviewColor = new Color(1f, 0.9f, 0f, 0.5f); // Yellow

    // Private targets for animation
    private Image powerHighlightFill;

    private float powerHighlightValue;
    private float powerHighlightVel;

    private float targetPowerValue;
    private float powerAnimVel;

    private Color defaultBarColor;


    void Start()
    {
        // Set the width and closed position
        drawerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, openWidth);
        openXPos = -openWidth + -200;
        drawerRect.anchoredPosition = new Vector2(-200, drawerRect.anchoredPosition.y);

        clickBlockerMain.SetActive(false);
        clickBlockerWeapon.SetActive(false);

        mainBlockImage = clickBlockerMain.GetComponent<Image>();
        weaponBlockImage = clickBlockerWeapon.GetComponent<Image>();

        clickBlockerMain.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerWeapon.GetComponent<Button>().onClick.AddListener(CloseDrawer);

        PopulateDrawer();

        // Animation of Power Bar
        if (PowerBarFill != null)
        {
            defaultBarColor = PowerBarFill.color;
        }

        // Get the Fill Image from the highlight sliders
        if (powerHighlight != null)
            powerHighlightFill = powerHighlight.fillRect.GetComponent<Image>();

        ClearCostPreview(); // Initialize highlights

        GameEvents.OnHardwareEquipped += UpdatePowerBar;
        GameEvents.OnAbilityEquipped += UpdatePowerBar;
    }

    void OnEnable()
    {
        if (hardwareManager.EquippedPSU == null) return;
        holder.Name.text = hardwareManager.EquippedPSU.PowerOutput_W.ToString() + "w";
        holder.Certification.text = hardwareManager.EquippedPSU.Certification;
        holder.Description.text = hardwareManager.EquippedPSU.Description;
        holder.Data = hardwareManager.EquippedPSU;

        UpdatePowerBar(null);
    }

    void Update()
    {
        powerBar.value = Mathf.SmoothDamp(powerBar.value, targetPowerValue, ref powerAnimVel, 1f / barLerpSpeed);
        
        powerHighlight.value = Mathf.SmoothDamp(powerHighlight.value, powerHighlightValue, ref powerHighlightVel, 1f / barLerpSpeed);
        
        HandlePulse(powerHighlightFill, powerHighlightValue);
    }

    public void PopulateDrawer()
    {
        foreach (Transform child in allPSUsGrid)
        {
            Destroy(child.gameObject);
        }

        if (psuDatabase == null) return;

        // Loop through every ability in the database
        foreach (PSUData data in psuDatabase.PSUList)
        {
            // Create the icon prefab
            GameObject iconGO = Instantiate(psuIconPrefab, allPSUsGrid);
            MemoryStorageHolder_UI iconUI = iconGO.GetComponent<MemoryStorageHolder_UI>();

            // Set up the icon's visuals
            iconUI.Name.text = data.name;
            iconUI.Description.text = data.name;
            iconUI.Data = data;

            // Add a button and make it clickable
            Button iconButton = iconGO.GetComponent<Button>();
            if (iconButton == null) iconButton = iconGO.AddComponent<Button>();

            iconButton.onClick.AddListener(() => {
                OnDrawerButtonClicked(data);
            });
        }
    }

    private void OnDrawerButtonClicked(PSUData newData)
    {
        if (hardwareManager.EquippedPSU == newData) return; // Prevent same

        // --- 1. Tell the PlayerHardwareManager to Swap the Core ---
        hardwareManager.SwapHardware(newData);

        // --- 2. Update the UI Holder ---
        holder.Name.text = newData.PowerOutput_W.ToString() + "w";
        holder.Certification.text = newData.Certification;
        holder.Description.text = newData.Description;
        holder.Data = newData;

        // --- 3. Close the Drawer ---
        CloseDrawer();
    }

    public void ToggleDrawer()
    {
        if (isDrawerOpen) CloseDrawer();
        else OpenDrawer();
    }

    public void OpenDrawer()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        clickBlockerMain.SetActive(true);
        clickBlockerWeapon.SetActive(true);
        isDrawerOpen = true;
        animationCoroutine = StartCoroutine(AnimateDrawer(drawerRect.anchoredPosition.x, openXPos, true));
    }

    public void CloseDrawer()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateDrawer(drawerRect.anchoredPosition.x, -200, false));
        isDrawerOpen = false;

    }

    private IEnumerator AnimateDrawer(float startX, float endX, bool isDrawerOpening)
    {
        float t = 0f;
        Vector2 pos = drawerRect.anchoredPosition;

        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            pos.x = Mathf.Lerp(startX, endX, t);
            drawerRect.anchoredPosition = pos;

            mainBlockImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));
            weaponBlockImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            yield return null;
        }

        pos.x = endX;
        drawerRect.anchoredPosition = pos;
        mainBlockImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        weaponBlockImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);

        clickBlockerMain.SetActive(isDrawerOpening);
        clickBlockerWeapon.SetActive(isDrawerOpening);
    }

    // Status Bars
    private void UpdatePowerBar(ScriptableObject data)
    {
        // --- Get Max Values ---
        float maxPower = hardwareManager.EquippedPSU.PowerOutput_W;

        // --- Update Sliders ---
        targetPowerValue = Mathf.Clamp01(hardwareManager.GetPowerUtilization());

        // --- Update Text ---
        powerText.text = "<align=\"left\"><size=\"24\">Power\n<align=\"center\"><size=\"20\">" +
            $"{(targetPowerValue * 100).ToString("N0")}% - {hardwareManager.GetPowerUtilization(false).ToString()} / {maxPower} w";
    }

    public void ShowCostPreview(HardwareData data)
    {
        float maxPower = hardwareManager.EquippedPSU.PowerOutput_W;

        if (powerHighlightFill != null) powerHighlightFill.color = addPreviewColor;
        powerHighlightValue = data.PowerDraw_W / maxPower;
    }

    public void ClearCostPreview()
    {
        powerHighlightValue = 0;
    }

    public void AlertPowerOverdraw()
    {
        StartCoroutine(FlashUIElement(PowerBarFill, errorColor, defaultBarColor));
    }

    private void HandlePulse(Image fillImage, float targetValue)
    {
        if (fillImage == null) return;

        Color c = fillImage.color;
        float targetAlpha;

        if (targetValue > 0)
        {
            // If the bar is active, calculate the pulse
            float alpha = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f; // 0 to 1
            targetAlpha = Mathf.Lerp(pulseMinAlpha, pulseMaxAlpha, alpha); // min to max
        }
        else
        {
            // If the bar is not active, fade to zero
            targetAlpha = 0f;
        }

        // Smoothly animate the alpha
        c.a = Mathf.Lerp(c.a, targetAlpha, Time.deltaTime * barLerpSpeed);
        fillImage.color = c;
    }

    private IEnumerator FlashUIElement(Image element, Color flashColor, Color defaultColor)
    {
        if (uiAudioSource != null && errorSound != null)
        {
            uiAudioSource.PlayOneShot(errorSound);
        }

        for (int i = 0; i < 2; i++)
        {
            element.color = flashColor;
            yield return new WaitForSeconds(0.1f);
            element.color = defaultColor;
            yield return new WaitForSeconds(0.1f);
        }
    }
}