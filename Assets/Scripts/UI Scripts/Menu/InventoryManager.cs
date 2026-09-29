using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic; 
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Player player; 

    [Header("UI References")]
    [SerializeField] private Transform inventoryGridContent; 
    [SerializeField] private GameObject programIconPrefab;
    [SerializeField] private Canvas mainCanvas;

    [Header("Info Panel References")]
    [SerializeField] private Image infoIcon;
    [SerializeField] private TextMeshProUGUI infoName;
    [SerializeField] private TextMeshProUGUI infoTagLine;
    [SerializeField] private TextMeshProUGUI infoDescription;
    [SerializeField] private TextMeshProUGUI infoStats;

    [Header("Active Bar References")]
    [SerializeField] private Transform activeBarContent;  
    private List<GameObject> spawnedActiveIcons = new List<GameObject>();

    [Header("Stats Bars References")]
    [SerializeField] private Slider memoryBar;
    [SerializeField] private Slider storageBar;
    [SerializeField] private TextMeshProUGUI memoryText;
    [SerializeField] private TextMeshProUGUI storageText;
    [SerializeField] private Slider memoryHighlight;
    [SerializeField] private Slider storageHighlight;
    [SerializeField] private Color addPreviewColor = new Color(1f, 0.9f, 0f, 0.5f); // Yellow
    [SerializeField] private Color removePreviewColor = new Color(1f, 0.2f, 0f, 0.5f); // Red

    [Header("Suite Bar References")]
    [SerializeField] private Transform suiteBarContent; 
    [SerializeField] private GameObject suiteIconPrefab;

    [Header("Feedback Components")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip errorSound;
    [SerializeField] private Image memoryBarFill;
    [SerializeField] private Color errorColor = Color.red;

    [Header("Animation Settings")]
    [SerializeField] private float barLerpSpeed = 5f;
    [SerializeField] private float pulseSpeed = 3f;
    [SerializeField] private float pulseMinAlpha = 0.5f;
    [SerializeField] private float pulseMaxAlpha = 1f;

    // Private targets for animation
    private Image memoryHighlightFill;
    private Image storageHighlightFill;

    private float targetMemoryHighlightValue;
    private float targetStorageHighlightValue;
    private float memoryHighlightVel;
    private float storageHighlightVel;

    private float targetMemoryValue;
    private float targetStorageValue;
    private float memoryAnimVel;
    private float storageAnimVel; 

    private Color defaultBarColor;

    private List<GameObject> spawnedIcons = new List<GameObject>();

    private ProgramData prevData;

    
    void Start()
    {
        if (memoryBarFill != null)
        {
            defaultBarColor = memoryBarFill.color;
        }

        // Get the Fill Image from the highlight sliders
        if (memoryHighlight != null)
            memoryHighlightFill = memoryHighlight.fillRect.GetComponent<Image>();
        if (storageHighlight != null)
            storageHighlightFill = storageHighlight.fillRect.GetComponent<Image>();

        ClearCostPreview(); // Initialize highlights
    }

    void Update()
    {
        memoryBar.value = Mathf.SmoothDamp(memoryBar.value, targetMemoryValue, ref memoryAnimVel, 1f / barLerpSpeed);
        storageBar.value = Mathf.SmoothDamp(storageBar.value, targetStorageValue, ref storageAnimVel, 1f / barLerpSpeed);

        memoryHighlight.value = Mathf.SmoothDamp(memoryHighlight.value, targetMemoryHighlightValue, ref memoryHighlightVel, 1f / barLerpSpeed);
        storageHighlight.value = Mathf.SmoothDamp(storageHighlight.value, targetStorageHighlightValue, ref storageHighlightVel, 1f / barLerpSpeed);

        // Handle the pulsing alpha
        HandlePulse(memoryHighlightFill, targetMemoryHighlightValue);
        HandlePulse(storageHighlightFill, targetStorageHighlightValue);
    }

    void OnAwake()
    {
        this.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (this.gameObject.transform.parent.gameObject.activeSelf == false) return;

        RefreshInventory();
        RefreshActiveBar();
        UpdateStatBars();
        if (prevData != null) OnInventoryIconClicked(prevData);
        RefreshSuiteBar();
    }

    public void RefreshInventory()
    {
        // --- 1. Clear Old Icons ---
        foreach (GameObject icon in spawnedIcons)
        {
            Destroy(icon);
        }
        spawnedIcons.Clear();

        // --- 2. Spawn New Icons ---
        // Loop through every program in the player's Storage
        foreach (var programInstance in player.StoredPrograms)
        {
            // Create a new icon from the prefab
            GameObject iconGO = Instantiate(programIconPrefab, inventoryGridContent);
            spawnedIcons.Add(iconGO);

            // Get the icon's UI components
            ProgramIcon_PrefabInventoryUI iconUI = iconGO.GetComponent<ProgramIcon_PrefabInventoryUI>();
            iconUI.Initialize(programInstance.Data, this, mainCanvas, false, programInstance.IsOptimized);

            // --- ADD STACK DISPLAY LOGIC ---
            int storedStacks = programInstance.MaxStacks - programInstance.ActiveStacks;
            if (storedStacks > 1)
            {
                iconUI.stackCountText.text = storedStacks.ToString();
                iconUI.stackCountPopup.gameObject.SetActive(true);
            }
            else if (storedStacks == 0)
            {
                iconGO.SetActive(false);
                
            } else
            {
                iconUI.stackCountPopup.gameObject.SetActive(false);
            }
        }

        UpdateStatBars();
    }

    public void RefreshActiveBar()
    {
        // Clear old icons
        foreach (GameObject icon in spawnedActiveIcons)
        {
            Destroy(icon);
        }
        spawnedActiveIcons.Clear();

        // Draw new icons
        foreach (var programInstance in player.ActivePrograms)
        {
            GameObject iconGO = Instantiate(programIconPrefab, activeBarContent);
            spawnedActiveIcons.Add(iconGO);

            ProgramIcon_PrefabInventoryUI iconUI = iconGO.GetComponent<ProgramIcon_PrefabInventoryUI>();
            iconUI.Initialize(programInstance.Data, this, mainCanvas, true, programInstance.IsOptimized);

            // --- ADD STACK DISPLAY LOGIC ---
            if (programInstance.ActiveStacks > 1)
            {
                iconUI.stackCountText.text = programInstance.ActiveStacks.ToString();
                iconUI.stackCountPopup.gameObject.SetActive(true);
            }
            else
            {
                iconUI.stackCountPopup.gameObject.SetActive(false);
            }
        }
    }

    private void RefreshSuiteBar()
    {
        foreach (Transform child in suiteBarContent)
        {
            Destroy(child.gameObject);
        }

        List<ApplicationSuiteData> suites = new List<ApplicationSuiteData>();
        foreach (var programInstance in player.StoredPrograms)
        {
            var suiteData = programInstance.Data.ApplicationSuite;
            if (!suites.Contains(suiteData))
                suites.Add(suiteData);
            else continue;

            SuiteIcon_InventoryPrefab suiteIcon = Instantiate(suiteIconPrefab, suiteBarContent).GetComponent<SuiteIcon_InventoryPrefab>();

            suiteIcon.Name.text = suiteData.Name;
            suiteIcon.Icon.sprite = suiteData.SuiteSprite;

            if (player.ActiveSuites.FirstOrDefault(p => p.Data == suiteData) != null)
                suiteIcon.Icon.color = Color.white;
            else
                suiteIcon.Icon.color = new Color(1f, 1f, 1f, 0.3f);

            suiteIcon.Button.onClick.AddListener(() => {
                OnSuiteIconClicked(suiteData);
            });
        }
    }

    public bool ActivateProgram(ProgramData dataToActivate, int count)
    {
        ProgramInstance program = player.StoredPrograms.Find(p => p.Data == dataToActivate);

        float utilPercent = player.CurrentMemoryUtilization() / player.Stats.MaxMemory.GetValue();

        if (program != null)
        {
            if (player.ActivePrograms.Contains(program))
            {
                float val = utilPercent + ((dataToActivate.RoutineSize * count) / player.Stats.MaxMemory.GetValue());
                if (val > 1f)
                {
                    if(this.gameObject.transform.parent.gameObject.activeSelf != false)
                        StartCoroutine(FlashUIElement(memoryBarFill, errorColor, defaultBarColor));
                    return false;
                }
                int stacks = program.ActiveStacks;
                program.OnDeactivate();
                program.OnActivate(count == 0 ? program.MaxStacks : stacks + count);
            } 
            else
            {
                float val = utilPercent + ((dataToActivate.Memory + (dataToActivate.RoutineSize * (count == 0 ? program.MaxStacks : count))) / player.Stats.MaxMemory.GetValue());
                if(val > 1f)
                {
                    if (this.gameObject.transform.parent.gameObject.activeSelf != false)
                        StartCoroutine(FlashUIElement(memoryBarFill, errorColor, defaultBarColor));
                    return false;
                }

                player.ActivePrograms.Add(program);
                if (count == 1) program.OnActivate();
                else
                {
                    program.OnActivate(count == 0 ? program.MaxStacks : count);
                }
            }

            GameEvents.ReportProgramEnabled(dataToActivate);

            // Update UI
            RefreshUI();

            return true;
        }
        return false;
    }

    public bool DeactivateProgram(ProgramData dataToDeactivate, int count)
    {
        ProgramInstance program = player.ActivePrograms.Find(p => p.Data == dataToDeactivate);

        if (program != null)
        {
            // Remove all
            if (program.ActiveStacks - count == 0 || count == 0)
            {
                program.OnDeactivate();
                player.ActivePrograms.Remove(program);
                GameEvents.ReportProgramDisabled(dataToDeactivate);
            }
            else // Remove by amount but not all
            {
                int stacks = program.ActiveStacks;
                program.OnDeactivate();
                program.OnActivate(stacks - count);
            }


            RefreshUI();

            return true;
        }

        return false;
    }

    public void OnInventoryIconClicked(ProgramData data)
    {
        prevData = data;
        ProgramInstance instance = player.StoredPrograms.FirstOrDefault(p => p.Data == data);
        infoIcon.sprite = data.IconSprite;
        infoTagLine.text = data.TagLine;
        infoName.text = data.ProgramName;
        infoDescription.text = data.PlainTextDescription;
        infoStats.text = $"Multiplier: {data.Multiplier}\n" +
            $"Memory: {data.Memory} GB\nStorage: {data.Storage} \nRoutine Size: {data.RoutineSize}\n" +
            $"Routines: {instance.MaxStacks}\nActive Routines: {instance.ActiveStacks}";
    }

    public void OnSuiteIconClicked(ApplicationSuiteData data)
    {
        infoIcon.sprite = data.SuiteSprite;
        infoTagLine.text = data.TagLine;
        infoName.text = data.Name;
        infoDescription.text = data.PlainTextDescription;
    }

    // Status Bars
    private void UpdateStatBars()
    {
        // --- Get Max Values ---
        if (player.Stats.MaxMemory == null) return; // Need it for... Some reason?

        float maxMemory = player.Stats.MaxMemory.GetValue();
        float maxStorage = player.Stats.MaxStorage.GetValue();

        // --- Calculate Current Usage ---
        float currentMemoryUsed = player.CurrentMemoryUtilization();
        float currentStorageUsed = player.CurrentStorageUtilization();

        // --- Update Sliders ---
        float memPercent = Mathf.Clamp01(currentMemoryUsed / maxMemory);
        float storePercent = Mathf.Clamp01(currentStorageUsed / maxStorage);

        targetMemoryValue = memPercent;
        targetStorageValue = storePercent;

        // --- Update Text ---
        memoryText.text = "<align=\"left\"><size=\"24\">Memory\n<align=\"center\"><size=\"20\">" +
            $"{(memPercent * 100).ToString("N0")}% - {currentMemoryUsed.ToString("F1")} / {player.Stats.MaxMemory.GetValue().ToString("F1")} GB";
        storageText.text = "<align=\"left\"><size=\"24\">Storage\n<align=\"center\"><size=\"20\">" +
            $"{(storePercent * 100).ToString("N0")}% - {currentStorageUsed.ToString("F1")} / {player.Stats.MaxStorage.GetValue().ToString("F1")} GB";
    }

    public void ShowCostPreview(ProgramData data, bool isAdding)
    {
        ProgramInstance program = player.ActivePrograms.Find(p => p.Data == data) ?? player.StoredPrograms.Find(p => p.Data == data);
        if (program == null) return;

        //float memoryCost = 0;
        //float storageCost = 0;
        float maxMemory = player.Stats.MaxMemory.GetValue();
        float maxStorage = player.Stats.MaxStorage.GetValue();

        //if (isAdding) // Previewing cost of adding ONE stack
        //{
        //    memoryCost = program.Data.RoutineSize + program.Data.Memory;
        //    // Only preview storage cost if it's not already stored/active
        //    if (program.ActiveStacks == 0)
        //    {
        //        storageCost = program.Data.Storage;
        //    }

        //    memoryHighlight.color = addPreviewColor;
        //    storageHighlight.color = addPreviewColor;
        //}
        //else // Previewing removing ONE stack
        //{
        //    memoryCost = -program.Data.Memory;
        //    if (program.ActiveStacks == 1)
        //    {
        //        storageCost = -program.Data.Storage;
        //    }

        //    memoryHighlight.color = removePreviewColor;
        //    storageHighlight.color = removePreviewColor;
        //}

        //memoryHighlight.value = memoryBar.value + (memoryCost / maxMemory);
        //storageHighlight.value = storageBar.value + (storageCost / maxStorage);

        // Just show how much space its taking up
        if (memoryHighlightFill != null) memoryHighlightFill.color = addPreviewColor;
        if (storageHighlightFill != null) storageHighlightFill.color = addPreviewColor;



        //storageHighlight.value = program.Data.Storage / maxStorage;
        //if(program.ActiveStacks == 0) memoryHighlight.value = 0;
        //else memoryHighlight.value = (program.Data.Memory + program.Data.RoutineSize * program.ActiveStacks) / maxMemory;
        targetStorageHighlightValue = program.Data.Storage / maxStorage;
        
        if(program.ActiveStacks == 0) targetMemoryHighlightValue = 0;
        else targetMemoryHighlightValue = (program.Data.Memory + program.Data.RoutineSize * program.ActiveStacks) / maxMemory;



    }

    public void ClearCostPreview()
    {
        // Set colors to clear to hide them
        //if (memoryHighlight != null) memoryHighlight.color = Color.clear;
        //if (storageHighlight != null) storageHighlight.color = Color.clear;

        //memoryHighlight.value = 0;
        //storageHighlight.value = 0;
        targetMemoryHighlightValue = 0;
        targetStorageHighlightValue = 0;

        // Re-sync them to the main bar's value
        //if (memoryBar != null) memoryHighlight.fillAmount = memoryBar.value;
        //if (storageBar != null) storageHighlight.fillAmount = storageBar.value;
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
            //fade to zero
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