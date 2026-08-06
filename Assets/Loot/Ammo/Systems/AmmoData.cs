using UnityEngine;

[CreateAssetMenu(fileName = "Ammo", menuName = "Loot/AmmoData")]
public class AmmoData : ScriptableObject
{
    public AmmoType Type;
    public int BaseDropAmount;

    [Tooltip("Variance in the amount of ammo dropped, represented as a percentage of the base drop amount. Considered to be a +/-.")]
    [Range(0f, 1f)] public float DropVariance = 0.5f;

    public GameObject PickupPrefab;
}