using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Title;
    [SerializeField] private TextMeshProUGUI Subtitle;
    [SerializeField] private TextMeshProUGUI Body;

    // Corner Images
    [SerializeField] private GameObject Top;
    [SerializeField] private GameObject Left;
    [SerializeField] private GameObject Right;
    [SerializeField] private GameObject Bottom;

    public void Awake()
    {
        Top.SetActive(false);
        Left.SetActive(false);
        Right.SetActive(false);
        Bottom.SetActive(false);
    }

    public void OnDisable()
    {
        Top.SetActive(false);
        Left.SetActive(false);
        Right.SetActive(false);
        Bottom.SetActive(false);
    }

    public void SetText(string title, string subtitle, string body)
    {
        Title.text = title;
        Subtitle.text = subtitle;
        Body.text = body;
    }

    public void SetDirection(TooltipPosition pos)
    {
        // Clear all previous
        Bottom.SetActive(false);
        Left.SetActive(false);
        Right.SetActive(false);
        Top.SetActive(false);

        switch (pos) 
        {
            case TooltipPosition.Above: Bottom.SetActive(true); break;
            case TooltipPosition.Below: Top.SetActive(true); break;
            case TooltipPosition.Left: Right.SetActive(true); break;
            case TooltipPosition.Right: Left.SetActive(true); break;
        }
    }
}