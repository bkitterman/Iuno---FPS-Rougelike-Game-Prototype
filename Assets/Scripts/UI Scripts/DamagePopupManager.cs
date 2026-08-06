using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager Instance;

    [SerializeField] private GameObject damagePopupPrefab; // Assign your NEW UI prefab here
    [SerializeField] private Transform canvasContainer;    // Assign your "Container" object from Bite 1

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /// <summary>
    /// Creates a damage popup that follows a specific transform (for DoTs).
    /// </summary>
    public void CreatePopup(Transform target, float damage, bool isPrecision)
    {
        GameObject popupGO = Instantiate(damagePopupPrefab, canvasContainer);
        DamagePopup popupScript = popupGO.GetComponent<DamagePopup>();
        popupScript.Init(target, damage, isPrecision);
    }

    /// <summary>
    /// Creates a damage popup at a static world location (for initial hits).
    /// </summary>
    public void CreatePopup(Vector3 worldPosition, float damage, bool isPrecision)
    {
        GameObject popupGO = Instantiate(damagePopupPrefab, canvasContainer);
        DamagePopup popupScript = popupGO.GetComponent<DamagePopup>();
        popupScript.Init(worldPosition, damage, isPrecision);
    }
}