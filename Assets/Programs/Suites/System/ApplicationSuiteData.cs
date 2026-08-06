using System.Collections.Generic;

using UnityEngine;

[CreateAssetMenu(fileName = "ApplicationSuiteData", menuName = "Programs/Application Suite")]
public class ApplicationSuiteData : ScriptableObject
{
    [Header("Suite Info")]
    public string Name;
    public string Version = "1.0.0";
    public string TagLine;
    [TextArea] public string PlainTextDescription;
    [TextArea] public string InDepthDescription;

    [Header("Tuning")]
    public List<SuiteBonusTier> BonusTiers;

    [Header("Dev Items")]
    public int Id; // UNQIUE REQUIRED
    public Sprite SuiteSprite;
    public Color SuiteColor;
}

[System.Serializable]
public class SuiteBonusTier
{
    public int RequiredPrograms;
    public float EffectValue;
    public StatModType ModifierType;
    public StatusEffectData Effect;
}