using UnityEngine;

[CreateAssetMenu(fileName = "Program Rarity", menuName = "Programs/Rarity ")]
public class RarityData : ScriptableObject
{
    [Header("Visual Properties")]
    [Tooltip("The color used for the border and UI text.")]
    public Color DisplayColor = Color.white;

    [Tooltip("The material or sprite to use for the border object.")]
    public Material BorderMaterial; // Use this for a MeshRenderer border

    [Tooltip("The particle system prefab to spawn (optional).")]
    public GameObject VfxPrefab;

    [Tooltip("The name of the Rarity (e.g., 'Rare', 'Legendary').")]
    public string Name = "Common";
}