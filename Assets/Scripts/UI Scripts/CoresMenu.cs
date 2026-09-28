using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CoresMenu : MonoBehaviour
{
    [Header("Drawer Components")]
    [SerializeField] private RectTransform drawerRect;
    [SerializeField] private GameObject clickBlockerMemory;
    [SerializeField] private GameObject clickBlockerStorage;
    [SerializeField] private GameObject clickBlockerAbilities;
    [SerializeField] private GameObject clickBlockerWeapon;
    [SerializeField] private GameObject clickBlockerPower;
    [SerializeField] private GameObject clickBlockerCoresBackground;
    [SerializeField] private float animationSpeed = 15f;
    [SerializeField] private Transform allCoresGrid;

    [Header("Equipped Holders")]
    [SerializeField] private List<CoreHolderUI> equippedHolders;
    private List<CoreHolderUI> drawerUI = new List<CoreHolderUI>(); 

    [Header("Prefabs & Data")]
    [SerializeField] private GameObject coreIconPrefab;
    [SerializeField] private CoreDatabase allCoresDB;
    [SerializeField] private Player player;
    [SerializeField] private PlayerHardwareManager hardwareManager;
    [SerializeField] private PowerMenu powerMenu;
    [SerializeField] private Color highlightColor;
    [SerializeField] private Color deactivatedHighlightColor = new Color(0, 0, 0, 0);

    private bool isDrawerOpen = false;
    private CoreHolderUI selectedHolder;
    private Coroutine animationCoroutine;
    private float drawerOpenScale = 1f;
    private float drawerClosedScale;

    private Image memoryBlockerImage;
    private Image storageBlockerImage;
    private Image abilitiesBlockerImage;
    private Image weaponBlockerImage;
    private Image powerBlockerImage;
    private Image coresBackgroundBlockerImage;


    void Start()
    {
        // Calculate the closed position based on the drawer's height
        drawerClosedScale = drawerRect.localScale.x;

        memoryBlockerImage = clickBlockerMemory.GetComponent<Image>();
        storageBlockerImage = clickBlockerStorage.GetComponent<Image>();
        abilitiesBlockerImage = clickBlockerAbilities.GetComponent<Image>();
        weaponBlockerImage = clickBlockerWeapon.GetComponent<Image>();
        powerBlockerImage = clickBlockerPower.GetComponent<Image>();
        coresBackgroundBlockerImage = clickBlockerCoresBackground.GetComponent<Image>();

        clickBlockerMemory.SetActive(false);
        clickBlockerStorage.SetActive(false);
        clickBlockerAbilities.SetActive(false);
        clickBlockerWeapon.SetActive(false);
        clickBlockerPower.SetActive(false);
        clickBlockerCoresBackground.SetActive(false);

        // --- Wire up all the buttons ---
        foreach (var holder in equippedHolders)
        {
            Button holderButton = holder.GetComponent<Button>();
            if (holderButton == null) holderButton = holder.gameObject.AddComponent<Button>();

            holderButton.onClick.AddListener(() => {
                OnCoreHolderClicked(holder);
            });
        }

        // 2. Wire up the CLICK BLOCKER
        clickBlockerMemory.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerStorage.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerAbilities.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerWeapon.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerPower.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerCoresBackground.GetComponent<Button>().onClick.AddListener(CloseDrawer);
    }

    public void OnEnable()
    {
        if (hardwareManager.EquippedCores.Count != 0)
        {
            for (int i = 0; i < 4; i++)
            {
                if (hardwareManager.EquippedCores[i] != null)
                {

                    equippedHolders[i].Icon.color = new Color(1f, 1f, 1f, 1f);
                    equippedHolders[i].Icon.sprite = hardwareManager.EquippedCores[i].Icon;
                    equippedHolders[i].Name.text = hardwareManager.EquippedCores[i].Name;
                    equippedHolders[i].Data = hardwareManager.EquippedCores[i];
                    equippedHolders[i].Border.color = deactivatedHighlightColor;
                }
                else
                {
                    equippedHolders[i].Icon.color = new Color(0f, 0f, 0f, 0f);
                    equippedHolders[i].Name.text = "Empty";
                    equippedHolders[i].Border.color = deactivatedHighlightColor;
                }
            }
        }

        PopulateDrawer();
    }

    public void OnCoreHolderClicked(CoreHolderUI holder)
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
        clickBlockerAbilities.SetActive(true);
        clickBlockerWeapon.SetActive(true);
        clickBlockerPower.SetActive(true);
        clickBlockerCoresBackground.SetActive(true);

        animationCoroutine = StartCoroutine(AnimateDrawer(drawerOpenScale, true));
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

        animationCoroutine = StartCoroutine(AnimateDrawer(drawerClosedScale, false));

        isDrawerOpen = false;
    }

    private IEnumerator AnimateDrawer(float target, bool isDrawerOpening)
    {
        float t = 0f;
        float start = drawerRect.localScale.x;
        Vector2 pos = drawerRect.localScale;

        while (t < 1f)
        {
            t += Time.deltaTime * animationSpeed;
            var next = Mathf.Lerp(start, target, t);
            pos.x = next;
            pos.y = next;
            drawerRect.localScale = pos;

            memoryBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            storageBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            abilitiesBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            weaponBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            powerBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            coresBackgroundBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            yield return null;
        }
        pos.x = target;
        pos.y = target;
        drawerRect.localScale = pos;

        memoryBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        storageBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        abilitiesBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        weaponBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        powerBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        coresBackgroundBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);

        clickBlockerMemory.SetActive(isDrawerOpening);
        clickBlockerStorage.SetActive(isDrawerOpening);
        clickBlockerAbilities.SetActive(isDrawerOpening);
        clickBlockerWeapon.SetActive(isDrawerOpening);
        clickBlockerPower.SetActive(isDrawerOpening);
        clickBlockerCoresBackground.SetActive(isDrawerOpening);
    }

    void PopulateDrawer()
    {
        // Clear any old icons first
        foreach (Transform child in allCoresGrid)
        {
            Destroy(child.gameObject);
        }

        if (allCoresDB == null) return;

        drawerUI.Clear();

        int currentIndex = 0;
        int currentCore = 0;

        int[] skipIndexes = { 6,7,8, 11,12,13, 16,17,18 };
        while (currentCore < allCoresDB.CoreList.Count)
        {
            if (skipIndexes.Contains(currentIndex))
            {
                Instantiate(coreIconPrefab, allCoresGrid);
                currentIndex++;
                continue;
            }

            CoreData data = allCoresDB.CoreList[currentCore];

            // Create the icon prefab
            GameObject iconGO = Instantiate(coreIconPrefab, allCoresGrid);
            CoreHolderUI iconUI = iconGO.GetComponent<CoreHolderUI>();

            iconUI.Icon.sprite = data.Icon;
            iconUI.Name.text = data.Name;
            iconUI.Description.text = data.PowerDraw_W.ToString() + "w";
            iconUI.Data = data;
            iconUI.Index = currentIndex;

            // If currently selected, highlight.
            if (equippedHolders.FirstOrDefault(p => p.Data == data) != null)
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

            currentIndex++;
            currentCore++;
            drawerUI.Add(iconUI);
        }
    }

    private void OnDrawerIconClicked(CoreData newData, Image border)
    {
        if (selectedHolder == null) return;

        if (selectedHolder.Data == newData) return; // Prevent same
        if (equippedHolders.FirstOrDefault(p => p.Data == newData) != null) return; // Prevent duplicates
        
        if(hardwareManager.GetPowerUtilization(false) + newData.PowerDraw_W > hardwareManager.EquippedPSU.PowerOutput_W)
        {
            powerMenu.AlertPowerOverdraw();
            return;
        }

        // --- 1. Find the Slot Index ---
        CoreData oldData = selectedHolder.Data;
        int slotIndex = equippedHolders.IndexOf(selectedHolder);
        if (slotIndex == -1) return;

        // --- 2. Tell the PlayerHardwareManager to Swap the Core ---
        hardwareManager.SwapHardware(newData, slotIndex);

        // --- 3. Update the UI Holder ---
        selectedHolder.Icon.sprite = newData.Icon;
        selectedHolder.Name.text = newData.Name;
        selectedHolder.Data = newData;
        selectedHolder.Border.color = highlightColor;

        border.color = highlightColor;

        CoreHolderUI temp = drawerUI.FirstOrDefault((p => p.Data == oldData));
        if (temp != null)
        {
            temp.Border.color = deactivatedHighlightColor;
        }

        // --- 4. Close the Drawer ---
        //CloseDrawer();
    }
}