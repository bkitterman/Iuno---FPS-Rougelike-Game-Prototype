using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerLook playerLook;
    [SerializeField] private CrosshairController crosshair;
    [SerializeField] private AbilityManager abilityManager;

    [Header("Ability References")]
    [SerializeField] private AbilityIcon_UIPrefab ability1;
    [SerializeField] private AbilityIcon_UIPrefab ability2;
    [SerializeField] private AbilityIcon_UIPrefab ability3;

    private AbilityIcon_UIPrefab[] abilities;

    void Start()
    {
        abilities = new AbilityIcon_UIPrefab[] { ability1, ability2, ability3};
     
    }

    void Update()
    {
        updateGunUI();
        updateAbilityUI();
    }

    void updateGunUI()
    {
        if (player.EquippedWeapon == null) return;

        if (player.EquippedWeapon is Gun gun)
        {
            int currentAmmo = gun.CurrentAmmo;
            int currentReserves = gun.AmmoReserve;

            ammoText.text = (currentAmmo == 0 ? "<color=\"red\">" : "") + currentAmmo + "<color=\"white\">  " +
                "<size=\"16\">" + (currentReserves <= player.Stats.MaxAmmo.GetValue() ? "<color=\"red\">" : "") + currentReserves + "<color=\"white\"> " +
                "\n<size=\"16\">" + (gun.FireMode == FireMode.Semi ? "Single" : "Auto") +
                (playerState.IsReloading ? " - Reloading" : "");

            // Use current spread and also a scaled recoil influence
            float recoilMag = playerLook != null ? playerLook.RecoilAccumulator.magnitude : 0f;
            float recoilInfluence = Mathf.InverseLerp(0f, 10f, recoilMag) * 0.5f;
            crosshair.SetSpread(gun.CurrentSpread, recoilInfluence);
        }
        else if (player.EquippedWeapon is Sword sword)
        {
            ammoText.text = "inf\n<size=\"16\">Melee" + (playerState.IsBlocking ? " - Blocking" : "") + "</size>";
        }
        else
        {
            ammoText.text = "null\n<size=\"16\">null - null</size>";
        }
    }
 
    void updateAbilityUI()
    {
        for (int i = 0; i<abilityManager.ActiveAbilities.Length; i++)
        {

            AbilityIcon_UIPrefab icon = abilities[i];
            AbilityInstance ability = abilityManager.ActiveAbilities[i];
            if (ability == null)
            {
                icon.gameObject.SetActive(false);
                continue;
            }


            if (ability.Data.Icon != null)
                icon.Icon.sprite = ability.Data.Icon;

            if (ability.Data.charges == 1)
            {
                if (ability.IsOnCooldown)
                {
                    icon.CooldownText.gameObject.SetActive(true);
                    icon.CooldownText.text = ((int)ability.CooldownRemaining).ToString();
                    icon.CooldownSlider.value = Mathf.Clamp01(ability.CooldownRemaining / ability.Data.Cooldown);
                    icon.Background.color = new Color(1f, 0f, 0f, 0.5f);
                }
                else
                {
                    icon.CooldownText.gameObject.SetActive(false);
                    icon.CooldownSlider.value = 0f;
                    icon.Background.color = new Color(0f, 1f, 0f, 0.5f);
                }
            } 
            else
            {
                if (ability.CurrentCharges < ability.Data.charges)
                {
                    if(ability.CurrentCharges < 1)
                    {
                        icon.Background.color = new Color(1f, 0f, 0f, 0.5f);
                        icon.CooldownText.text = ((int)ability.CooldownRemaining).ToString();
                    }
                    else
                    {
                        icon.Background.color = new Color(0f, 1f, 0f, 0.5f);
                        icon.CooldownText.text = $" \n<size=\"14\">{ability.CurrentCharges}";
                    }
                    icon.CooldownSlider.value = Mathf.Clamp01(ability.CooldownRemaining / ability.Data.Cooldown);
                }
                else
                {
                    icon.CooldownText.text = " \n<size=\"14\">" + ability.CurrentCharges;
                    icon.Background.color = new Color(0f, 1f, 0f, 0.5f); 
                    icon.CooldownSlider.value = 0f;
                }
            }
        }
    }
}
