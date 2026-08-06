using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using TMPro;

public class MemoryStorageHolder_UI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI Name;
    public TextMeshProUGUI Description;
    public HardwareData Data;
    public bool IsSelected;
    [SerializeField] public Vector2 BottomOffset;
    [SerializeField] public Vector2 LeftOffset;
    private Vector2 offset;

    private string tipName;
    private string type;
    private string desc;


    public void OnPointerEnter(PointerEventData eventData)
    {
        tipName = Data.Name;
        if (Data is PSUData)
        {
            PSUData data = (PSUData)Data;
            type = $"Power - {data.PowerOutput_W} - {data.Certification}";
            offset = LeftOffset;
        }
        else if (Data is MemoryData)
        {
            type = $"Memory - {Data.PowerDraw_W}w";
            offset = BottomOffset;
        }
        else if (Data is StorageData)
        {
            type = $"Storage - {Data.PowerDraw_W}w";
            offset = BottomOffset;
        }
        desc = Data.Description;

        // Show the tooltip
        if (Data is PSUData)
        {
            TooltipManager.Instance.ShowTooltip(tipName, type, desc, transform, TooltipPosition.Left, offset);
        }
        else
        {
            TooltipManager.Instance.ShowTooltip(tipName, type, desc, transform, TooltipPosition.Below, offset);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Hide the tooltip
        TooltipManager.Instance.HideTooltip();
    }
}
