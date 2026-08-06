using UnityEngine;
using UnityEngine.UI;

using TMPro;

public class StatusEffectUIHolder : MonoBehaviour
{
    public Image Icon;
    public Image BuffIcon;
    public Image DebuffIcon;

    [SerializeField] private Color InactiveColor = new(0, 0, 0, 0);

    public TextMeshProUGUI Text;
    public StatusEffect Effect;

    public bool IsBuff { get; private set; }
    public bool IsDebuff => !IsBuff;

    public int Priority = 0;

    // DEV SHIT
    private bool hasWarned = false;

    public void SetText(string text, float duration = 0f)
    {
        if(text.Length > 15)
        {
            if(hasWarned)
            {
                Debug.LogWarning($"Status Effect name '{text}' is too long and will be truncated to " + string.Concat(text[0..15], "..."));
                hasWarned = true;
            }

            text = string.Concat(text[0..15], "...");
        }

        if(duration > 0f)
        {
            Text.text = $"{text} - {duration / 60:0}:{duration %60 :00}";
        }
        else
        {
            Text.text = $"{text}";
        }
    }

    public void SetBuff(bool isBuff)
    {
        IsBuff = isBuff;
        
        if (isBuff)
        {
            DebuffIcon.color = InactiveColor;
        }
        else
        {
            BuffIcon.color = InactiveColor;
        }
    }
}
