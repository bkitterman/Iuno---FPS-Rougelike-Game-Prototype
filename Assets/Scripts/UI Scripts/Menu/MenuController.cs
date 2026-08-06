using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject softwareContent;
    public GameObject hardwareContent;
    public GameObject armoryContent;
    public GameObject osContent;
    public GameObject playerContent;
    public GameObject codexContent;

    // Armory Optimizations
    public GameObject weaponStudio;

    void Start()
    {
        // Start with the software tab visible by default
        ShowSoftwareTab();
    }

    void OnEnable()
    {
        weaponStudio.SetActive(true);
    }

    void OnDisable()
    {
        weaponStudio.SetActive(false);
    }

    private void DeactivateAllTabs()
    {
        softwareContent.SetActive(false);
        hardwareContent.SetActive(false);
        armoryContent.SetActive(false);
        osContent.SetActive(false);
        playerContent.SetActive(false);
        codexContent.SetActive(false);
    }

    public void ShowSoftwareTab()
    {
        DeactivateAllTabs();
        softwareContent.SetActive(true);
    }

    public void ShowHardwareTab()
    {
        DeactivateAllTabs();
        hardwareContent.SetActive(true);
    }

    public void ShowArmoryTab()
    {
        DeactivateAllTabs();
        armoryContent.SetActive(true);
    }

    public void ShowOperatingSystemTab()
    {
        DeactivateAllTabs();
        osContent.SetActive(true);
    }

    public void ShowPlayerTab()
    {
        DeactivateAllTabs();
        playerContent.SetActive(true);
    }

    public void ShowCodexTab()
    {
        DeactivateAllTabs();
        codexContent.SetActive(true);
    }
}