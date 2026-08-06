using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class CrosshairController : MonoBehaviour
{
    [Header("Crosshair Elements")]
    public RawImage crosshair;
    public RawImage hitMarker;

    [Header("Settings")]
    public float hipfireSpread = 30f;
    public float adsSpread = 5f;
    public float moveSpread = 20f;
    public float hitFlashTime = 0.1f;
    public float killFlashTime = 0.1f;

    [Header("Spread -> UI")]
    public float minSpreadDeg = 0.5f;
    public float maxSpreadDeg = 10f;
    public float minScale = 1f;
    public float maxScale = 1.6f;
    public float spreadLerpSpeed = 12f;

    private float currentSpread;
    private float targetSpread;
    private float hitTimer;

    public bool isAiming;
    private bool hitMarkerShown;
    private float hitMarkerTimeLeft;
    private float killMarkerTimeLeft;

    RectTransform crossRT;

    void Awake()
    {
        crossRT = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        GameEvents.OnDamageDealt += ShowHitMarker;
        GameEvents.OnEnemyKilled += ShowKillMarker;
    }

    void OnDisable()
    {
        GameEvents.OnDamageDealt -= ShowHitMarker;
        GameEvents.OnEnemyKilled -= ShowKillMarker;
    }

    /// <summary>
    /// Set the spread of the reticle. This is done by scaling up to match the recoil, compared to internal
    /// states of minimum and maximum spread.
    /// 
    /// </summary>
    /// <param name="spreadDeg">The amount of spread of the weapon</param>
    /// <param name="recoilInfluence">Increase the spread by the recoil amount.</param>
    public void SetSpread(float spreadDeg, float recoilInfluence = 0f)
    {
        float t = Mathf.InverseLerp(minSpreadDeg, maxSpreadDeg, Mathf.Abs(spreadDeg));
        t = Mathf.Clamp01(t + recoilInfluence);

        float targetScale = Mathf.Lerp(minScale, maxScale, t);
        crossRT.localScale = Vector3.Lerp(crossRT.localScale, Vector3.one * targetScale, Time.deltaTime * spreadLerpSpeed);
    }

    void Update()
    {
        // If hitmarker is showing, but time is up, turn it off
        if (hitMarkerShown && hitMarkerTimeLeft <= 0 && killMarkerTimeLeft <= 0)
        {
            hitMarker.color = new Color(1f, 0f, 0f, 0f);
            hitMarkerShown = false;
            hitMarkerTimeLeft = 0;
            killMarkerTimeLeft = 0;
        }
        
        // If hitmarker is not showing but timer is active, turn it on
        if (!hitMarkerShown && (hitMarkerTimeLeft > 0 || killMarkerTimeLeft > 0))
        {
            hitMarker.color = new Color(1f, 0f, 0f, 1f);
            hitMarkerShown = true;
        }

        if (!hitMarkerShown) return;

        // If Killmarker is 
        if(killMarkerTimeLeft <= 0)
        {
            // float result = Mathf.Lerp(hitMarker.transform.localScale.x, 1f, Time.deltaTime / spreadLerpSpeed);
            hitMarker.transform.localScale = new Vector3(1f, 1f, 1f);
        }

        // If hitmarker is showing, count down timer
        if (hitMarkerShown)
        {
            if (hitMarkerTimeLeft > 0) hitMarkerTimeLeft -= Time.deltaTime;
            if (killMarkerTimeLeft > 0) killMarkerTimeLeft -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Show the hitmarker.
    /// 
    /// This is done by reseting hit marker time.
    /// </summary>
    public void ShowHitMarker(GameObject target, float amount, bool isCrit, bool onHitEnabled)
    {
        hitMarkerTimeLeft = hitFlashTime;
    }

    public void ShowKillMarker(GameObject target)
    {
        killMarkerTimeLeft = killFlashTime;

        if (hitMarker == null) Debug.LogWarning("Hit marker");

        hitMarker.transform.localScale = new Vector3(1.5f, 1.5f, 1f);
    }
}
