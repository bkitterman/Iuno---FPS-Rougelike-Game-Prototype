using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "Ability/Abilities/1 Value")]
public class AbilityData : ScriptableObject
{
    [Header("Info")]
    public string AbilityName;
    public string ClassName;
    [TextArea] public string PlainTextDescription;
    [TextArea] public string InDepthDescription;
    public string Version = "1.0.0";
    public int charges = 1;
    public Sprite Icon;

    [Header("Tuning")]
    public float Cooldown;
    public float Value;
    public float PowerDraw_W = 125;
}