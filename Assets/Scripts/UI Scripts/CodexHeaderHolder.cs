using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CodexHeaderHolder : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image Border;
    public Image Arrow;
    public Image Icon;
    public TextMeshProUGUI Text;
    public bool IsSelected;

    [SerializeField] private Color highlightColor;
    [SerializeField] private Color deactivatedHighlightColor = new Color(0, 0, 0, 0); 

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsSelected) return;
        this.Border.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (IsSelected) return;
        this.Border.color = deactivatedHighlightColor;
    }
}
