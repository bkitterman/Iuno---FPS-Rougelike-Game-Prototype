using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;

using TMPro;

public class AbilityHolderUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image Icon;
    public Image Border;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI PowerDraw;
    public AbilityData Data;

    public bool IsSelected;
    [SerializeField] private Vector2 offset;

    private string tipName;
    private string type;
    private string desc;

    public void OnPointerEnter(PointerEventData eventData)
    {
        tipName = Data.AbilityName;
        type = $"Ability - {Data.PowerDraw_W}";
        desc = $"{Data.PlainTextDescription}\nCooldown: {Data.Cooldown}\nCharges: {Data.charges}";
        
        TooltipManager.Instance.ShowTooltip(tipName, type, desc, transform, TooltipPosition.Above, offset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the tooltip
        TooltipManager.Instance.HideTooltip();
    }
}
