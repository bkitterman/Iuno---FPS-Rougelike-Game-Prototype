using UnityEngine;
using UnityEngine.UI;

using System.Collections.Generic;
using System.Linq;

public class ProgramDisplayController : MonoBehaviour
{
    [Header("References")]
    public Player player;
    public GameObject programIconPrefab;
    public GameObject suiteGroupPrefab;
    public Transform mainIconContainer;

    [Header("Bar Scaling")]
    [SerializeField] private float maxBarWidth = 800f;
    private HorizontalLayoutGroup _layoutGroup;
    private float _iconWidth = -1f;
    private float originalScale;

    private Dictionary<string, SuiteGroupUI> _activeSuiteUIs = new Dictionary<string, SuiteGroupUI>();
    private Dictionary<ProgramData, ProgramIcon_PrefabUI> _activeProgramIcons = new Dictionary<ProgramData, ProgramIcon_PrefabUI>();

    private List<ProgramData> _iconsToRemove = new List<ProgramData>();
    private List<string> _suitesToRemove = new List<string>();
    

    void Start()
    {
        GameEvents.OnProgramEnabled += UpdateDisplay;
        GameEvents.OnProgramDisabled += UpdateDisplay;
        GameEvents.OnProgramPickedUp += UpdateStacksDisplay;

        _layoutGroup = suiteGroupPrefab.GetComponent<HorizontalLayoutGroup>();

        if (programIconPrefab != null)
            _iconWidth = programIconPrefab.GetComponent<RectTransform>().rect.width;
        originalScale = mainIconContainer.localScale.x;
        
    }

    public void UpdateDisplay(ProgramData kljlkjlk)
    {
        // === Step 1: Cleanup ===
        foreach (ProgramData displayedData in _activeProgramIcons.Keys)
        {
            if (!player.ActivePrograms.Any(p => p.Data == displayedData))
            {
                _iconsToRemove.Add(displayedData);
            }
        }

        foreach (ProgramData dataToRemove in _iconsToRemove)
        {
            Destroy(_activeProgramIcons[dataToRemove].gameObject);
            _activeProgramIcons.Remove(dataToRemove);
        }
        _iconsToRemove.Clear();

        // Find and remove empty suite groups.
        foreach (string suiteName in _activeSuiteUIs.Keys)
        {
            if (!player.ActivePrograms.Any(p => p.Data.ApplicationSuite.Name == suiteName))
            {
                _suitesToRemove.Add(suiteName);
            }
        }
        foreach (string suiteToRemove in _suitesToRemove)
        {
            Destroy(_activeSuiteUIs[suiteToRemove].gameObject);
            _activeSuiteUIs.Remove(suiteToRemove);
        }
        _suitesToRemove.Clear();

        // --- Step 2: Update, Create, and Parent Icons ---
        string currentSuiteName = null;
        SuiteGroupUI currentSuiteGroup = null;

        foreach (var programInstance in player.ActivePrograms)
        {
            ProgramData data = programInstance.Data;
            string suiteName = data.ApplicationSuite.Name;

            if (suiteName != currentSuiteName)
            {
                currentSuiteName = suiteName;
                if (!_activeSuiteUIs.ContainsKey(suiteName))
                {
                    // Create Icon
                    GameObject suiteObj = Instantiate(suiteGroupPrefab, mainIconContainer);
                    currentSuiteGroup = suiteObj.GetComponent<SuiteGroupUI>();
                    currentSuiteGroup.SuiteIcon.sprite = data.ApplicationSuite.SuiteSprite;
                    _activeSuiteUIs.Add(suiteName, currentSuiteGroup);
                }
                else
                {
                    currentSuiteGroup = _activeSuiteUIs[suiteName];
                }
            }

            // --- Pretty Stuff ---
            // 1. Set the border color from the suite's data
            currentSuiteGroup.Border.color = data.ApplicationSuite.SuiteColor;

            // 2. Gray out the suite icon if the suite bonus is not active
            bool suiteIsActive = player.IsSuiteActive(data.ApplicationSuite);
            currentSuiteGroup.SuiteIcon.color = suiteIsActive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.75f);

            // Now, handle the individual program icon
            ProgramIcon_PrefabUI programIcon;
            if (!_activeProgramIcons.ContainsKey(data))
            {
                GameObject iconObj = Instantiate(programIconPrefab, currentSuiteGroup.IconContainer);
                programIcon = iconObj.GetComponent<ProgramIcon_PrefabUI>();
                programIcon.iconArtImage.sprite = data.IconSprite;
                programIcon.borderImage.color = data.Rarity.DisplayColor;
                programIcon.backgroundImage.color = data.ApplicationSuite.SuiteColor;
                _activeProgramIcons.Add(data, programIcon);
            }
            else
            {
                programIcon = _activeProgramIcons[data];
            }

            // Crucially, ensure the icon is in the correct parent container
            programIcon.transform.SetParent(currentSuiteGroup.IconContainer);
            currentSuiteGroup.SuiteIcon.transform.SetAsLastSibling();
        }

        // --- Step 3: Reorder the Suite Groups Themselves ---
        foreach (var suiteName in player.ActivePrograms.Select(p => p.Data.ApplicationSuite.Name).Distinct())
        {
            if (_activeSuiteUIs.ContainsKey(suiteName))
            {
                _activeSuiteUIs[suiteName].transform.SetAsLastSibling();
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(mainIconContainer as RectTransform);

        // --- STEP 4: SCALE CONTAINER TO FIT ---
        int iconCount = player.ActivePrograms.Count;
        if (iconCount == 0)
        {
            return;
        }

        // Get all the layout properties that affect width
        float iconSpacing = _layoutGroup.spacing;
        float padding = _layoutGroup.padding.left + _layoutGroup.padding.right;

        // Calculate the width the bar wants to be at full scale
        float preferredWidth = (iconCount * _iconWidth) + (Mathf.Max(0, iconCount - 1) * iconSpacing) + padding;
        if (preferredWidth <= maxBarWidth)
        {
            mainIconContainer.localScale = new Vector3(originalScale, originalScale, 1f);
        }
        else
        {
            float requiredScale = maxBarWidth / preferredWidth;
            if (requiredScale > originalScale) requiredScale = originalScale;
            mainIconContainer.localScale = new Vector3(requiredScale, requiredScale, 1f);
        }

        UpdateStacksDisplay(null);
    }

    public void UpdateStacksDisplay(ProgramData unneededData)
    {
        foreach (var programInstance in player.ActivePrograms)
        {
            ProgramData data = programInstance.Data;

            if (!_activeProgramIcons.ContainsKey(data))
            {
                continue;
            }

            if (programInstance.ActiveStacks > 1)
            {
                _activeProgramIcons[data].stackCountText.text = programInstance.ActiveStacks.ToString();
                _activeProgramIcons[data].stackCountText.gameObject.SetActive(true);
                _activeProgramIcons[data].stackCountPopup.gameObject.SetActive(true);
            }
            else
            {
                _activeProgramIcons[data].stackCountText.gameObject.SetActive(false);
                _activeProgramIcons[data].stackCountPopup.gameObject.SetActive(false);
            }

            if (player.CheckIfProgramOptimized(data)) _activeProgramIcons[data].Animator.SetBool("isOptimized", programInstance.IsOptimized);
        }
    }
}
