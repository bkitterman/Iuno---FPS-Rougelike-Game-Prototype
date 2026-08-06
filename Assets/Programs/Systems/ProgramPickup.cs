using UnityEngine;

public class ProgramPickup : MonoBehaviour
{
    public ProgramData programData;

    [Header("Component References")]
    [SerializeField] private SpriteRenderer iconSpriteRenderer;
    [SerializeField] private SpriteRenderer borderRenderer;
    [SerializeField] private SpriteRenderer backgroundRenderer;
    [SerializeField] private ParticleSystem rarityParticles;
    [SerializeField] public bool IsPermanent = false;

    public bool isOptimized = false;

    /// <summary>
    /// Called by the LootManager right after this prefab is spawned.
    /// </summary>
    public void Initialize(ProgramData data)
    {
        this.programData = data;

        // 1. Set the Icon
        if (data.IconSprite != null)
        {
            iconSpriteRenderer.sprite = data.IconSprite;
        }
        else
        {
            Debug.LogWarning($"ProgramData '{data.ProgramName}' is missing an IconSprite!");
        }

        // 2. Set the Particle Color
        var mainParticleModule = rarityParticles.main;
        mainParticleModule.startColor = data.Rarity.DisplayColor;

        // 3. Set border color
        if (borderRenderer != null)
        {
            borderRenderer.color = data.Rarity.DisplayColor;
        }

        // 4. Set Background color
        if (backgroundRenderer != null)
        {
            backgroundRenderer.color = data.ApplicationSuite.SuiteColor;
        }
        
    }
}