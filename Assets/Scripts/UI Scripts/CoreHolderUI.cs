using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public class CoreHolderUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image Icon;
    public Image Border;
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public int Index;
    public CoreData Data;
    public bool IsSelected;
    [SerializeField] public Vector2 LeftOffset;
    [SerializeField] public Vector2 RightOffset;
    [SerializeField] public Vector2 TopOffset;
    [SerializeField] public Vector2 BelowOffset;
    private Vector2 offset;

    private string tipName;
    private string type;
    private string desc;

    public void OnPointerEnter(PointerEventData eventData)
    {
        tipName = Data.Name;
        type = $"Core - {Data.PowerDraw_W}";
        desc = Data.Description;

        TooltipPosition direction;

        if (Index < 5)
        {
            direction = TooltipPosition.Above;
            offset = TopOffset;
        }
        else if (Index == 5 || Index == 10 || Index == 15)
        {
            direction = TooltipPosition.Left;
            offset = LeftOffset;
        }
        else if (Index > 19)
        {
            direction = TooltipPosition.Below;
            offset = BelowOffset;
        }
        else
        {
            direction = TooltipPosition.Right;
            offset = RightOffset;
        }

        TooltipManager.Instance.ShowTooltip(tipName, type, desc, transform, direction, offset);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the tooltip
        TooltipManager.Instance.HideTooltip();
    }
}
