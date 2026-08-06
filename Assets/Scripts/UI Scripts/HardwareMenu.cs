using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HardwareMenu : MonoBehaviour
{
    [Header("Drawer Components")]
    [SerializeField] private RectTransform drawerRect;
    [SerializeField] private float animationSpeed = 15f;
    [SerializeField] private float openWidth = 350f;

    [Header("Click-Away Blocker")]
    [SerializeField] private GameObject clickBlockerMain;
    [SerializeField] private GameObject clickBlockerPower;

    [Header("Drawer Population")]
    [SerializeField] private WeaponDatabase weaponDatabase;
    [SerializeField] private GameObject weaponButtonPrefab;
    [SerializeField] private Transform allGunsGrid;
    [SerializeField] private Player player;

    private bool isDrawerOpen = false;
    private Coroutine animationCoroutine; 
    private float closedXPos;

    private Image mainBlockImage;
    private Image powerBlockImage;

    void Start()
    { 
        // Set the width and closed position
        drawerRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, openWidth);
        closedXPos = -openWidth;
        drawerRect.anchoredPosition = new Vector2(closedXPos, drawerRect.anchoredPosition.y);

        clickBlockerMain.SetActive(false);
        clickBlockerPower.SetActive(false);

        mainBlockImage = clickBlockerMain.GetComponent<Image>();
        powerBlockImage = clickBlockerPower.GetComponent<Image>();

        clickBlockerMain.GetComponent<Button>().onClick.AddListener(CloseDrawer);
        clickBlockerPower.GetComponent<Button>().onClick.AddListener(CloseDrawer);

        PopulateDrawer();
    }

    public void PopulateDrawer()
    {
        foreach (Transform child in allGunsGrid)
        {
            Destroy(child.gameObject);
        }

        if (weaponDatabase == null) return;

        // Loop through every ability in the database
        foreach (WeaponData data in weaponDatabase.WeaponList)
        {
            // Create the icon prefab
            GameObject iconGO = Instantiate(weaponButtonPrefab, allGunsGrid);
            WeaponButtonUI_Prefab iconUI = iconGO.GetComponent<WeaponButtonUI_Prefab>();

            // Set up the icon's visuals
            iconUI.Name.text = data.name;

            // Add a button and make it clickable
            Button iconButton = iconGO.GetComponent<Button>();
            if (iconButton == null) iconButton = iconGO.AddComponent<Button>();

            iconButton.onClick.AddListener(() => {
                OnDrawerButtonClicked(data);
            });
        }
    }

    private void OnDrawerButtonClicked(WeaponData data)
    {
        player.EquipGun(player.WeaponInventory.FindIndex(p => p.Data == data));
    }

    public void ToggleWeaponDrawer()
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
        clickBlockerPower.SetActive(true);
        isDrawerOpen = true;
        animationCoroutine = StartCoroutine(AnimateDrawer(drawerRect.anchoredPosition.x, 0, true));

    }

    public void CloseDrawer()
    {
        if (animationCoroutine != null)
        {
            StopCoroutine(animationCoroutine);
        }
        animationCoroutine = StartCoroutine(AnimateDrawer(drawerRect.anchoredPosition.x, closedXPos, false));
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
            powerBlockImage.color = new Color(0f, 0f, 0f, Mathf.Lerp(
                !isDrawerOpening ? 0.5f : 0f, !isDrawerOpening ? 0f : 0.5f, t));
            yield return null;
        }

        pos.x = endX;
        drawerRect.anchoredPosition = pos;
        mainBlockImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        powerBlockImage.color = new Color(0f, 0f, 0f, isDrawerOpening ? 0.5f : 0f);
        clickBlockerMain.SetActive(isDrawerOpening);
        clickBlockerPower.SetActive(isDrawerOpening);
    }
}