using System.Collections;

using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public enum FireMode { Semi, Auto }
public class Gun : MonoBehaviour, IWeapon
{
    [Header("Gun Data")]
    public WeaponData Data { get; set; }
    [SerializeField] private WeaponData _privateData;
    public FireMode FireMode = FireMode.Semi;
    public float spreadRecoverDelay = 0.12f;

    [Header("References")]
    public Player Player { get; set; }
    public Camera FpsCam { get; set; }
    public PlayerState PlayerState { get; set; }
    public LayerMask hitMask;
    public AudioSource gunAudio;
    public AudioClip shootSound;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public Transform ADSPoint;

    public CrosshairController Crosshair;
    public GameObject CrosshairObject;

    //WeaponSwayAndRecoil sway;
    WeaponRecoil sway;
    PlayerMovement playerMovement;

    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private Transform muzzleTransform;
    [SerializeField] public GameObject ScopeLens;
    [SerializeField] private float tracerDuration = 0.1f;

    // Private States
    public int CurrentAmmo { get; set; }
    public int AmmoReserve { get; set; }
    public float CurrentSpread { get; private set; }
    int shotsFiredInBurst = 0;
    bool onCooldown = false;
    float spreadVelocity;
    float lastFireTime;
    float timeSinceLastShot = 999f;

    private PlayerLook playerLook;
    Coroutine reloadRoutine;

    void Awake()
    {
        Data = _privateData;
    }

    void Start()
    {
        PlayerState = GetComponentInParent<PlayerState>();

        // Set up controls
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerLook = FindAnyObjectByType<PlayerLook>();

        try
        {
            AmmoReserve = (int)Player.Stats.AmmoReserve.GetValue();
        } 
        catch (System.Exception)
        {
            
        }

        sway = GetComponentInParent<WeaponRecoil>();

        muzzleFlash.Stop(); // Needed to prevent auto play of particle

        if (Crosshair == null)
        {
            Crosshair = FindAnyObjectByType<CrosshairController>();
            CrosshairObject = Crosshair.gameObject;
        }
    }

    void OnEnable()
    {
        if(sway == null) sway = GetComponentInParent<WeaponRecoil>();

        muzzleFlash.Stop();
    }

    void OnDisable()
    {
        if (reloadRoutine != null)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
            PlayerState.SetReloading(false);
        }
    }

    void Update()
    {
        if(Player == null) return;
        // -------------------- Bloom Recovery --------------------
        timeSinceLastShot += Time.deltaTime;
        if (timeSinceLastShot > spreadRecoverDelay)
        {
            CurrentSpread = Mathf.MoveTowards(
                CurrentSpread, 
                Player.Stats.BulletSpread.GetValue(), 
                Data.SpreadRecoveryRate * Time.deltaTime);
        }

        // -------------------- Reloading --------------------
        if (PlayerState.IsReloading && PlayerState.IsSprinting)
        {
            StopCoroutine(reloadRoutine);
            reloadRoutine = null;
            PlayerState.SetReloading(false);
            return;
        }

        // Force Reloading if mag is empty
        if (CurrentAmmo <= 0 && !PlayerState.IsReloading && !PlayerState.IsSprinting)
        {
            reloadRoutine = StartCoroutine(ReloadMethod());
            return;
        }
    }

    /// <summary>
    /// Shoot the gun once.
    /// </summary>
    /// <param name="held">It true, the button is being held.</param>
    public void PrimaryFire(bool held)
    {
        if (PlayerState.IsReloading || onCooldown) return;
        if (CurrentAmmo <= 0)
        {
            Reload();
            return;
        }

        if (FireMode == FireMode.Semi && !held)
            Shoot();
        else if (FireMode == FireMode.Auto && held)
            Shoot();

    }

    public void SecondaryFire(bool performed)
    {

        if (Player.state.IsSprinting)
            playerMovement.ToggleSprint();

        PlayerState.SetAiming(!PlayerState.IsAiming);
        Crosshair.isAiming = PlayerState.IsAiming;
        CrosshairObject.SetActive(!PlayerState.IsAiming);
    }

    /// <summary>
    /// Switch the guns active fire mode to the next in line.
    /// </summary>
    public void SwitchMode()
    {
        FireMode = FireMode == FireMode.Semi ? FireMode.Auto : FireMode.Semi;
    }

    /// <summary>
    /// Try to reload the gun
    /// </summary>
    public void Reload()
    {
        if (Player.state.IsSprinting)
            playerMovement.ToggleSprint();

        float maxAmmo = Player.Stats.MaxAmmo.GetValue();
        if (!PlayerState.IsReloading && CurrentAmmo < maxAmmo && !PlayerState.IsSprinting && AmmoReserve > 0)
        {
            reloadRoutine = StartCoroutine(ReloadMethod());
        }
    }

    /// <summary>
    /// Reload the current wepaon.
    /// </summary>
    /// <returns></returns>
    IEnumerator ReloadMethod()
    {
        PlayerState.SetReloading(true);

        yield return new WaitForSeconds(Player.Stats.ReloadSpeed.GetValue());
        if(AmmoReserve < Player.Stats.MaxAmmo.GetValue())
        {
            CurrentAmmo = AmmoReserve;
            AmmoReserve = 0;
        }
        else
        {
            CurrentAmmo = (int)Player.Stats.MaxAmmo.GetValue();
            AmmoReserve -= CurrentAmmo;
        }

        PlayerState.SetReloading(false);
        GameEvents.ReportPlayerReload();
    }

    /// <summary>
    /// Set the timeframe of which the gun canno tbe fired.
    /// </summary>
    /// <returns></returns>
    IEnumerator Cooldown()
    {
        onCooldown = true;

        yield return new WaitForSeconds(Player.Stats.FireRate.GetValue());
        onCooldown = false;
    }

    /// <summary>
    /// Shoot the gun, apply recoil to the camera.
    /// </summary>
    void Shoot() 
    {
        if (Player.state.IsSprinting)
            playerMovement.ToggleSprint();

        lastFireTime = Time.time;
        timeSinceLastShot = 0f;
        shotsFiredInBurst++;
        RaycastHit hit;

        //GameEvents.ReportPlayerWeaponFired(CurrentAmmo);

        // ------------------ Player Feedback ------------------
        muzzleFlash.Emit(1);

        gunAudio.PlayOneShot(shootSound); 
        if (sway != null) sway.OnFire();

        for (int shots = 0; shots < Player.Stats.RoundsPerShot.GetValue(); shots++)
        {
            if (CurrentAmmo == 0) break;
            if (Random.value < Player.Stats.AmmoConsumptionChance.GetValue()) CurrentAmmo--;

            // ------------------ Bloom / Spread ------------------
            float adsMul = PlayerState.IsAiming ? Player.Stats.BulletSpread.GetValue() : 1f;
            float airborneMul = !PlayerState.IsGrounded ? Player.Stats.AirborneEffectiveness.GetValue() : 1f;
            float spreadDeg = CurrentSpread * adsMul * airborneMul;
            
            Vector2 rndUnit = Random.insideUnitCircle;

            // Map to yaw/pitch in degrees
            float yawDeg = rndUnit.x * spreadDeg;
            float pitchDeg = rndUnit.y * spreadDeg;

            // Build the shot direction by rotating camera.forward around camera's local axes
            Vector3 shotDir = FpsCam.transform.forward;
            shotDir = Quaternion.AngleAxis(yawDeg, FpsCam.transform.up) * shotDir;
            shotDir = Quaternion.AngleAxis(pitchDeg, FpsCam.transform.right) * shotDir;

            int bounces = (int) Player.Stats.BulletBounces.GetValue();
            bool isBouncing = true;
            bool directShot = true;

            Vector3 currentRayPoint = FpsCam.transform.position;
            Vector3 currentRayDir = shotDir;

            int bonusThresholdAmmo = (int) (
                Player.Stats.MagazineDamageBonusThreshold.GetValue() * 
                Player.Stats.MaxAmmo.GetValue());

            while (isBouncing)
            {
                // ------------------ Fire the Shot ------------------
                if (Physics.SphereCast(currentRayPoint, Data.BulletMagnetismRadius, currentRayDir, out hit, Data.Range, hitMask))
                {
                    // Apply damage
                    var hb = hit.collider.GetComponent<Hitbox>();

                    // Create tracer
                    CreateTracer(currentRayPoint, hit.point);

                    if (hb != null)
                    {
                        if (CurrentAmmo > bonusThresholdAmmo)
                            hb.TakeDamage(
                                Player.gameObject.transform,
                                Player.Stats.WeaponDamage.GetValue() *
                                (!PlayerState.IsGrounded ? Player.Stats.AirborneDamageMultiplier.GetValue() : 1f) *
                                (!directShot ? Player.Stats.SecondaryDamageMultiplier.GetValue() : 1f) *
                                Player.Stats.AllDamageMultiplier.GetValue(),
                                hit.point,
                                true);
                        else
                            hb.TakeDamage(
                                Player.gameObject.transform,
                                Player.Stats.WeaponDamage.GetValue() * 
                                Player.Stats.MagazineDamageBonusMultiplier.GetValue() *
                                (!directShot ? Player.Stats.SecondaryDamageMultiplier.GetValue() : 1f) *
                                (!PlayerState.IsGrounded ? Player.Stats.AirborneDamageMultiplier.GetValue() : 1f) *
                                Player.Stats.AllDamageMultiplier.GetValue(),
                                hit.point,
                                true);
                        isBouncing = false;
                    }
                    else
                    {
                        if (bounces == 0)
                        {
                            // Impact Effect
                            GameObject impactGO = Instantiate(impactEffect, hit.point, Quaternion.LookRotation(hit.normal));
                            Destroy(impactGO, 5f);
                            GameEvents.ReportPlayerMiss();
                            isBouncing = false;
                        }
                        else
                        {
                            directShot = false;
                            bounces--;
                            currentRayPoint = hit.point;
                            currentRayDir = Vector3.Reflect(currentRayDir, hit.normal);
                        }
                    }

                } 
                else
                {
                    // muzzle visual: compute target point (if no hit)
                    Vector3 targetPoint = currentRayPoint + currentRayDir * Data.Range;
                    CreateTracer(currentRayPoint, targetPoint);
                    GameEvents.ReportPlayerMiss();
                    isBouncing = false;
                }

            }

            // Add to bloom for next shot
            CurrentSpread = Mathf.Clamp(
                CurrentSpread + Data.SpreadPerShotDeg,
                Player.Stats.BulletSpread.GetValue(),
                Data.SpreadMaxDeg);
        }

        StartCoroutine(Cooldown());

        // ------------------ Recoil from Shot ------------------
        Vector2 perShotRecoil = Vector2.zero;
        float yawRand = Random.Range(
            -Player.Stats.RecoilHorizontalPerShot.GetValue(),
            Player.Stats.RecoilHorizontalPerShot.GetValue());

        perShotRecoil = new Vector2(yawRand,
            Player.Stats.RecoilVerticalPerShot.GetValue());

        // Reduce Recoil if ADS
        perShotRecoil *= PlayerState.IsAiming ? Data.AdsRecoilMultiplier : 1f;
        playerLook.AddRecoil(perShotRecoil);
    }

    /// <summary>
    /// Create a tracer at the muzzle to the hit point.
    /// </summary>
    /// <param name="hitPoint">The end of the tracer trail. </param>
    void CreateTracer(Vector3 startPoint, Vector3 hitPoint)
    {
        if(startPoint == FpsCam.transform.position) startPoint = muzzleTransform.position;

        GameObject tracer = Instantiate(tracerPrefab);
        LineRenderer lr = tracer.GetComponent<LineRenderer>();

        lr.SetPosition(0, startPoint);
        lr.SetPosition(1, hitPoint);

        StartCoroutine(FadeAndDestroyTracer(lr));
    }

    /// <summary>
    /// Handle visual of the tracer.
    /// </summary>
    /// <param name="lr"></param>
    /// <returns>The tracer's line renderer object</returns>
    IEnumerator FadeAndDestroyTracer(LineRenderer lr)
    {
        float t = 0f;
        float startWidth = lr.startWidth;

        while (t < tracerDuration)
        {
            t += Time.deltaTime;
            float alpha = 1f - (t / tracerDuration);

            // fade width
            lr.startWidth = Mathf.Lerp(startWidth, 0f, t / tracerDuration);
            lr.endWidth = Mathf.Lerp(startWidth, 0f, t / tracerDuration);

            yield return null;
        }

        Destroy(lr.gameObject);
    }
}
