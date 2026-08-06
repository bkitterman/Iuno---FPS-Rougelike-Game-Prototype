using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
public class MemoryStorageMenu : MonoBehaviour
{
    [Header("Drawer Components")]
    [SerializeField] private RectTransform drawerRect;
    [SerializeField] private GameObject clickBlockerAbility;
    [SerializeField] private GameObject clickBlockerWeapon;
    [SerializeField] private GameObject clickBlockerPower;
    [SerializeField] private GameObject clickBlockerCores;
    [SerializeField] private GameObject clickBlockerMemory;
    [SerializeField] private GameObject clickBlockerStorage;
    [SerializeField] private GameObject clickBlockerCoresBackground;
    [SerializeField] private float animationSpeed = 15f;
    [SerializeField] private Transform allAbilitiesGrid;

    [Header("Equipped Holders")]
    [SerializeField] private MemoryStorageHolder_UI MemoryHolder;
    [SerializeField] private MemoryStorageHolder_UI StorageHolder;

    [Header("Prefabs & Data")]
    [SerializeField] private GameObject drawerIconPrefab;
    [SerializeField] private StorageDatabase storageDB;
    [SerializeField] private MemoryDatabase memoryDB;
    [SerializeField] private Player player;
    [SerializeField] private PlayerHardwareManager hardwareManager;
    [SerializeField] private PowerMenu powerMenu;

    private bool isDrawerOpen = false;
    private bool isMemory = true;
    private AbilityHolderUI selectedHolder;
    private Coroutine animationCoroutine;
    private float drawerOpenY = 0f;
    private float drawerClosedY;

    private Image abilityBlockerImage;
    private Image weaponBlockerImage;
    private Image powerBlockerImage;
    private Image coresBlockImage;
    private Image memoryBlockImage;
    private Image storageBlockImage;
    private Image coresBackgroundBlockerImage;

    void Start()
    {
        // Calculate the closed position based on the drawer's height
        drawerClosedY = drawerRect.rect.height; // Change neg to pos
        drawerRect.anchoredPosition = new Vector2(drawerRect.anchoredPosition.x, drawerClosedY);

        abilityBlockerImage = clickBlockerAbility.GetComponent<Image>();
        weaponBlockerImage = clickBlockerWeapon.GetComponent<Image>();
        powerBlockerImage = clickBlockerPower.GetComponent<Image>();
        coresBlockImage = clickBlockerCores.GetComponent<Image>();
        memoryBlockImage = clickBlockerMemory.GetComponent<Image>();
        storageBlockImage = clickBlockerStorage.GetComponent<Image>();
        coresBackgroundBlockerImage = clickBlockerCoresBackground.GetComponent<Image>();

        clickBlockerAbility.SetActive(false);
        clickBlockerWeapon.SetActive(false);
        clickBlockerPower.SetActive(false);
        clickBlockerCores.SetActive(false);
        clickBlockerMemory.SetActive(false);
        clickBlockerStorage.SetActive(false);
        clickBlockerCoresBackground.SetActive(false);

        // --- Wire up all the buttons ---

        Button holderButton = MemoryHolder.GetComponent<Button>();
        if (holderButton == null) holderButton = MemoryHolder.gameObject.AddComponent<Button>();
        holderButton.onClick.AddListener(() => {
            OnMemoryClicked();
        });
        holderButton = StorageHolder.GetComponent<Button>();
        if (holderButton == null) holderButton = StorageHolder.gameObject.AddComponent<Button>();
        holderButton.onClick.AddListener(() => {
            OnStorageClicked();
        });


        // 2. Wire up the CLICK BLOCKER
        clickBlockerAbility.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerWeapon.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerPower.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerCores.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerMemory.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerStorage.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerCoresBackground.GetComponent<Button>().onClick.AddListener(CloseDrawer);
    }

    public void OnEnable()
    {
        MemoryHolder.Name.text = "Memory - " + hardwareManager.EquippedMemory.Name;
        MemoryHolder.Description.text = hardwareManager.EquippedMemory.Description;
        MemoryHolder.Data = hardwareManager.EquippedMemory;

        StorageHolder.Name.text = "Storage - " + hardwareManager.EquippedStorage.Name;
        StorageHolder.Description.text = hardwareManager.EquippedStorage.Description;
        StorageHolder.Data = hardwareManager.EquippedStorage;
    }

    public void OnStorageClicked()
    {
        if (!isDrawerOpen)
        {
            // --- DRAWER IS CLOSED: Open it ---
            isMemory = false;
            StorageHolder.IsSelected = true; // Add a visual cue (e.g., highlight)
            OpenDrawer();
        }
        else
        {
            CloseDrawer();
        }
    }

    public void OnMemoryClicked()
    {
        if (!isDrawerOpen)
        {
            // --- DRAWER IS CLOSED: Open it ---
            isMemory = true;
            MemoryHolder.IsSelected = true; // Add a visual cue (e.g., highlight)
            OpenDrawer();
        }
        else
        {
            // Clicked the same one: Close the drawer
            CloseDrawer();
        }
    }

    private void OpenDrawer()
    {
        if (animationCoroutine != null) StopCoroutine(animationCoroutine);

        PopulateDrawer();

        clickBlockerAbility.SetActive(true);
        clickBlockerWeapon.SetActive(true);
        clickBlockerPower.SetActive(true);
        clickBlockerCores.SetActive(true);
        if(isMemory) clickBlockerStorage.SetActive(true);
        else clickBlockerMemory.SetActive(true);

        animationCoroutine = StartCoroutine(AnimateDrawer(drawerOpenY, true));
        isDrawerOpen = true;
    }

    public void CloseDrawer()
    {
        if (isMemory) MemoryHolder.IsSelected = false;
        else StorageHolder.IsSelected = false;

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

            abilityBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            weaponBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            powerBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            coresBlockImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            if(isMemory) storageBlockImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                    !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));
            else memoryBlockImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                    !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            coresBackgroundBlockerImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));

            yield return null;
        }
        pos.y = targetY;
        drawerRect.anchoredPosition = pos;

        abilityBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        weaponBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        powerBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        coresBlockImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);

        if(isMemory) storageBlockImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        else memoryBlockImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        coresBackgroundBlockerImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);

        clickBlockerAbility.SetActive(isDrawerOpening);
        clickBlockerWeapon.SetActive(isDrawerOpening);
        clickBlockerPower.SetActive(isDrawerOpening);
        clickBlockerCores.SetActive(isDrawerOpening);
        if(isMemory) clickBlockerStorage.SetActive(isDrawerOpening);
        else clickBlockerMemory.SetActive(isDrawerOpening);
        clickBlockerCoresBackground.SetActive(isDrawerOpening);
    }

    void PopulateDrawer()
    {
        // Clear any old icons first
        foreach (Transform child in allAbilitiesGrid)
        {
            Destroy(child.gameObject);
        }

        if(isMemory)
        {
            if (memoryDB == null) return;

            foreach (MemoryData data in memoryDB.MemoryList)
            {
                // Create the icon prefab
                GameObject iconGO = Instantiate(drawerIconPrefab, allAbilitiesGrid);
                MemoryStorageHolder_UI iconUI = iconGO.GetComponent<MemoryStorageHolder_UI>();

                // Build Icon
                iconUI.Name.text = data.Name;
                iconUI.Data = data;

                // Add a button and make it clickable
                Button iconButton = iconGO.GetComponent<Button>();
                if (iconButton == null) iconButton = iconGO.AddComponent<Button>();

                iconButton.onClick.AddListener(() => {
                    OnDrawerIconClicked(data);
                });
            }
        }
        else
        {
            if (storageDB == null) return;

            foreach (StorageData data in storageDB.StorageList)
            {
                // Create the icon prefab
                GameObject iconGO = Instantiate(drawerIconPrefab, allAbilitiesGrid);
                MemoryStorageHolder_UI iconUI = iconGO.GetComponent<MemoryStorageHolder_UI>();

                // Build Icon
                iconUI.Name.text = data.Name;
                iconUI.Data = data;

                // Add a button and make it clickable
                Button iconButton = iconGO.GetComponent<Button>();
                if (iconButton == null) iconButton = iconGO.AddComponent<Button>();

                iconButton.onClick.AddListener(() => {
                    OnDrawerIconClicked(data);
                });
            }
        }
        
    }

    private void OnDrawerIconClicked(HardwareData newData)
    {
        if (hardwareManager.GetPowerUtilization(false) + newData.PowerDraw_W > hardwareManager.EquippedPSU.PowerOutput_W)
        {
            powerMenu.AlertPowerOverdraw();
            return;
        }

        if (isMemory)
        {
            newData = (MemoryData)newData;

            if (MemoryHolder.Data == newData) return; // Prevent same
            if (hardwareManager.EquippedMemory == newData) return; // Prevent duplicates


            // --- 1. Tell the PlayerHardwareManager to Swap the memory ---
            hardwareManager.SwapHardware(newData);

            // --- 3. Update the UI Holder ---
            MemoryHolder.Name.text = "Memory - " + newData.Name;
            MemoryHolder.Description.text = newData.Description;
            MemoryHolder.Data = newData;

            // --- 4. Close the Drawer ---
            CloseDrawer();
        }
        else
        {
            newData = (StorageData)newData;

            if (StorageHolder.Data == newData) return; // Prevent same
            if (hardwareManager.EquippedStorage == newData) return; // Prevent duplicates


            // --- 1. Tell the PlayerHardwareManager to Swap the memory ---
            hardwareManager.SwapHardware(newData);

            // --- 3. Update the UI Holder ---
            StorageHolder.Name.text = "Storage - " + newData.Name;
            StorageHolder.Description.text = newData.PowerDraw_W.ToString();
            StorageHolder.Data = newData;

            // --- 4. Close the Drawer ---
            CloseDrawer();
        }
    }
}
