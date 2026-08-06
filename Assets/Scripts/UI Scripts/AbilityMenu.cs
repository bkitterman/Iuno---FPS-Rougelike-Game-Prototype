using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class AbilityMenu : MonoBehaviour
{
    [Header("Drawer Components")]
    [SerializeField] private RectTransform drawerRect;
    [SerializeField] private GameObject clickBlockerMemory;
    [SerializeField] private GameObject clickBlockerStorage;
    [SerializeField] private GameObject clickBlockerCores;
    [SerializeField] private GameObject clickBlockerWeapon;
    [SerializeField] private GameObject clickBlockerPower;
    [SerializeField] private GameObject clickBlockerCoresBackground;
    [SerializeField] private float animationSpeed = 15f;
    [SerializeField] private Transform allAbilitiesGrid;

    [Header("Equipped Holders")]
    [SerializeField] private List<AbilityHolderUI> equippedHolders;

    [Header("Prefabs & Data")]
    [SerializeField] private GameObject abilityIconPrefab; 
    [SerializeField] private AbilityDatabase allAbilitiesDB; 
    [SerializeField] private Player player; 
    [SerializeField] private AbilityManager abilityManager;
    [SerializeField] private PlayerHardwareManager hardwareManager;
    [SerializeField] private PowerMenu powerMenu;

    [SerializeField] private Color highlightColor;
    [SerializeField] private Color deactivatedHighlightColor = new Color(0, 0, 0, 0);

    private bool isDrawerOpen = false;
    private AbilityHolderUI selectedHolder;
    private List<AbilityHolderUI> drawerUI = new List<AbilityHolderUI>();
    private Coroutine animationCoroutine;
    private float drawerOpenY = 0f;
    private float drawerClosedY;

    private Image memoryBlockerImage;
    private Image storageBlockerImage;
    private Image coresBlockerImage;
    private Image weaponBlockerImage;
    private Image powerBlockerImage;
    private Image coresBackgroundBlockerImage;

    void Start()
    {
        // Calculate the closed position based on the drawer's height
        drawerClosedY = -drawerRect.rect.height;
        drawerRect.anchoredPosition = new Vector2(drawerRect.anchoredPosition.x, drawerClosedY);

        memoryBlockerImage = clickBlockerMemory.GetComponent<Image>();
        storageBlockerImage = clickBlockerStorage.GetComponent<Image>();
        coresBlockerImage = clickBlockerCores.GetComponent<Image>();
        weaponBlockerImage = clickBlockerWeapon.GetComponent<Image>();
        powerBlockerImage = clickBlockerPower.GetComponent<Image>();
        coresBackgroundBlockerImage = clickBlockerCoresBackground.GetComponent<Image>();

        clickBlockerMemory.SetActive(false);
        clickBlockerStorage.SetActive(false);
        clickBlockerCores.SetActive(false);
        clickBlockerWeapon.SetActive(false);
        clickBlockerPower.SetActive(false);
        clickBlockerCoresBackground.SetActive(false);

        // --- Wire up all the buttons ---
        foreach (var holder in equippedHolders)
        {
            Button holderButton = holder.GetComponent<Button>();
            if (holderButton == null) holderButton = holder.gameObject.AddComponent<Button>();

            holderButton.onClick.AddListener(() => {
                OnAbilityHolderClicked(holder);
            });
        }

        // 2. Wire up the CLICK BLOCKER
        clickBlockerMemory.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerStorage.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerCores.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerWeapon.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerPower.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerCoresBackground.GetComponent<Button>().onClick.AddListener(CloseDrawer);

        // 3. Populate the drawer
        PopulateDrawer();
    }

    public void OnEnable()
    {
        for (int i = 0; i < 3; i++)
        {
            if (abilityManager.ActiveAbilities[i] != null)
            {

                equippedHolders[i].Icon.color = new Color(1f, 1f, 1f, 1f);
                equippedHolders[i].Icon.sprite = abilityManager.ActiveAbilities[i].Data.Icon;
                equippedHolders[i].Name.text = abilityManager.ActiveAbilities[i].Data.AbilityName;
                equippedHolders[i].PowerDraw.text = abilityManager.ActiveAbilities[i].Data.PowerDraw_W + "w";
                equippedHolders[i].Description.text = abilityManager.ActiveAbilities[i].Data.PlainTextDescription;
                equippedHolders[i].Data = abilityManager.ActiveAbilities[i].Data;
                equippedHolders[i].Border.color = deactivatedHighlightColor;
            }
            else
            {
                equippedHolders[i].Icon.color = new Color(0f, 0f, 0f, 0f);
                equippedHolders[i].PowerDraw.text = "0w";
                equippedHolders[i].Name.text = "No Ability Selected";
                equippedHolders[i].Description.text = "Click here to select an ability!";
                equippedHolders[i].Border.color = deactivatedHighlightColor;
            }
        }
    }

    public void OnAbilityHolderClicked(AbilityHolderUI holder)
    {
        if (!isDrawerOpen)
        {
            // --- DRAWER IS CLOSED: Open it ---
            selectedHolder = holder;
            holder.IsSelected = true;
            holder.Border.color = highlightColor;
            OpenDrawer();
        }
        else
        {
            // --- DRAWER IS OPEN ---
            if (holder == selectedHolder)
            {
                // Clicked the same one: Close the drawer
                CloseDrawer();
            }
            else
            {
                // Clicked a different slot: Swap selection
                if (selectedHolder != null)
                {
                    selectedHolder.IsSelected = false;
                    selectedHolder.Border.color = deactivatedHighlightColor;

                }

                selectedHolder = holder;
                selectedHolder.IsSelected = true;
                selectedHolder.Border.color = highlightColor;
            }
        }
    }

    private void OpenDrawer()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        clickBlockerMemory.SetActive(true);
        clickBlockerStorage.SetActive(true);
        clickBlockerCores.SetActive(true);
        clickBlockerWeapon.SetActive(true);
        clickBlockerPower.SetActive(true);
        animationCoroutine = StartCoroutine(AnimateDrawer(drawerOpenY, true));
        isDrawerOpen = true;
    }

    public void CloseDrawer()
    {
        if (selectedHolder != null)
        {
            selectedHolder.IsSelected = false;
            selectedHolder.Border.color = deactivatedHighlightColor;
        }

        selectedHolder = null;

        if (animationCoroutine != null) StopCoroutine(animationCoroutine);
        animationCoroutine = StartCoroutine(AnimateDrawer(drawerClosedY, false));
        isDrawerOpen = false;
    }

    private IEnumerator AnimateDrawer(float targetY, bool isDrawerOpening)
    {
        float t = 0f;
        float startY = drawerRect.anchoredPosition.y;
        Vector2 pos = drawerRect.anchoredPosition;

        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            pos.y = Mathf.Lerp(startY, targetY, t);
            drawerRect.anchoredPosition = pos;

            memoryBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            storageBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            coresBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            weaponBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            powerBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            coresBackgroundBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));
            yield return null;
        }
        pos.y = targetY;
        drawerRect.anchoredPosition = pos;

        memoryBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        storageBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        coresBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        weaponBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        powerBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        coresBackgroundBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);

        clickBlockerMemory.SetActive(isDrawerOpening);
        clickBlockerStorage.SetActive(isDrawerOpening);
        clickBlockerCores.SetActive(isDrawerOpening);
        clickBlockerWeapon.SetActive(isDrawerOpening);
        clickBlockerPower.SetActive(isDrawerOpening);
        clickBlockerCoresBackground.SetActive(isDrawerOpening);
    }

    void PopulateDrawer()
    {
        // Clear any old icons first
        foreach (Transform child in allAbilitiesGrid)
        {
            Destroy(child.gameObject);
        }

        if (allAbilitiesDB == null) return;

        drawerUI.Clear();

        foreach (AbilityData data in allAbilitiesDB.AbilityList)
        {
            // Create the icon prefab
            GameObject iconGO = Instantiate(abilityIconPrefab, allAbilitiesGrid);
            AbilityHolderUI iconUI = iconGO.GetComponent<AbilityHolderUI>();

            iconUI.Icon.sprite = data.Icon;
            iconUI.Name.text = data.AbilityName;
            iconUI.Data = data;

            // Add highlight if selected
            if (equippedHolders.FirstOrDefault((p => p.Data == data)) != null)
            {
                iconUI.Border.color = highlightColor;
            }
            else
            {
                iconUI.Border.color = deactivatedHighlightColor;
            }

            // Add a button and make it clickable
            Button iconButton = iconGO.GetComponent<Button>();
            if (iconButton == null) iconButton = iconGO.AddComponent<Button>();

            iconButton.onClick.AddListener(() => {
                OnDrawerIconClicked(data, iconUI.Border);
            });

            drawerUI.Add(iconUI);
        }
    }

    private void OnDrawerIconClicked(AbilityData newData, Image border)
    {
        if (selectedHolder == null) return;

        if (selectedHolder.Data == newData) return; // Prevent same
        if (equippedHolders.FirstOrDefault(p => p.Data == newData) != null) return; // Prevent duplicates

        if (hardwareManager.GetPowerUtilization(false) + newData.PowerDraw_W > hardwareManager.EquippedPSU.PowerOutput_W)
        {
            powerMenu.AlertPowerOverdraw();
            return;
        }

        // --- 1. Find the Slot Index ---
        AbilityData oldData = selectedHolder.Data;
        int slotIndex = equippedHolders.IndexOf(selectedHolder);
        if (slotIndex == -1) return;

        // --- 2. Tell the AbilityManager to Swap the Ability ---
        abilityManager.SwapAbility(slotIndex, newData);

        // --- 3. Update the UI Holder ---
        selectedHolder.Icon.sprite = newData.Icon;
        selectedHolder.Name.text = newData.AbilityName;
        selectedHolder.PowerDraw.text = newData.PowerDraw_W + "w";
        selectedHolder.Description.text = newData.PlainTextDescription;
        selectedHolder.Data = newData;

        selectedHolder.Border.color = highlightColor;

        border.color = highlightColor;

        AbilityHolderUI temp = drawerUI.FirstOrDefault((p => p.Data == oldData));
        if (temp != null)
        {
            temp.Border.color = deactivatedHighlightColor;
        }

        // --- 4. Close the Drawer ---
        //CloseDrawer();
    }
}