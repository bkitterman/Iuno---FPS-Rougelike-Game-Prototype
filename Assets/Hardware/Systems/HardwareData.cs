using UnityEngine;

public abstract class HardwareData : ScriptableObject
{
    [Header("Common Info")]
    public string Name = "New Hardware";
    [TextArea] public string Description = "A component.";
    public string Version = "1.0.0";
    public Sprite Icon;

    [Header("System Stats")]
    [Tooltip("How much power this component requires to function.")]
    public float PowerDraw_W = 10f;
}