using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerStatMathIconUIHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Transform Transform;
    public string TooltipHeader;
    public string TooltipBody;
    public string ToolTipSubtitle;

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        // Show tooltip
        TooltipManager.Instance.ShowTooltip(TooltipHeader, ToolTipSubtitle, TooltipBody, this.Transform, TooltipPosition.Left);
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }
}
