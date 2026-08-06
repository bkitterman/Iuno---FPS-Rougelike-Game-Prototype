using UnityEngine;

[CreateAssetMenu(fileName = "ProgramData", menuName = "Programs/Programs")]
public class ProgramData : ScriptableObject
{
    [Header("Item Descriptions")]
    public string ProgramName;      // THIS NEEDS TO ALIGN WITH CLASS NAME
    public string Version = "1.0.0";
    [TextArea]  public string TagLine;
    [TextArea]  public string PlainTextDescription;
    [TextArea]  public string InDepthDescription;
    
    [Header("Tuning")]
    public float Multiplier;
    public StatModType RoutineScaling;
    public ScalingMethod ScaleMethod = ScalingMethod.Linear;
    public bool IsBuff = true;
    public float Storage = 32f;
    public float Memory = 2f;
    public float RoutineSize = 0.5f;

    [Header("Status Effects")]
    public StatusEffectData EffectData;

    [Header("Optimization")]
    [Tooltip("Can this program drop as an Optimized version?")]
    public bool canBeOptimized = true;

    [Header("Stat Overrides (0 = no override)")]
    public float OptimizedMultiplier = 0;
    public float OptimizedMemoryCost = 0;
    public float OptimizedRoutineCost = 0;
    public float OptimizedStorageCost = 0;

    [Header("Developer Items")]
    public int id; // UNQIUE REQUIRED
    public Sprite IconSprite;
    public RarityData Rarity;
    public ApplicationSuiteData ApplicationSuite;
}


