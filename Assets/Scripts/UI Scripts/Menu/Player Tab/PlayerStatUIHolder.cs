using TMPro;
using UnityEngine;

public class PlayerStatUIHolder : MonoBehaviour
{
    public GameObject Border;
    public GameObject BuffIcon;
    public GameObject DebuffIcon;

    public Stat Stat;

    public bool IsSelected = false;

    [SerializeField] private TextMeshProUGUI value;

    public bool DisplayAsPercentage = false;
    public string Unit = "";

    public void SetValue(float newValue)
    {
        if(DisplayAsPercentage)
        {
            value.text = (newValue * 100f).ToString() + "%";    
            return;
        }

        if(Stat == Stat.FireRate)
        {
            value.text = (60f / newValue).ToString("N0") + Unit;
            return;
        }

        float roundedValue = (newValue >= 10 ? Mathf.FloorToInt(newValue) : newValue);
        if ((Mathf.Abs(roundedValue) < 1000000f && Mathf.Abs(roundedValue) > 0.0001f) || roundedValue == 0f)
        {
            value.text = roundedValue.ToString("N") + Unit;
        }
        else
        {
            value.text = roundedValue.ToString("E") + Unit;
        }
    }


}
