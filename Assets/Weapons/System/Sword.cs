using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour, IWeapon
{
    public WeaponData Data { get; set; }
    [SerializeField] private WeaponData _prviateData;

    [Header("References")]
    public Player Player { get; set; }
    public Camera FpsCam { get; set; }
    public PlayerState PlayerState { get; set;  }
    public LayerMask hitMask;
    public AudioSource swordAudio;
    public AudioClip swordSound;
    public GameObject impactEffect;

    public CrosshairController Crosshair;
    public GameObject CrosshairObject;

    //WeaponSwayAndRecoil sway;
    WeaponRecoil sway;
    PlayerMovement playerMovement;

    // Private States
    public int CurrentAmmo { get; set; }
    public int AmmoReserve { get; set; }
    public float CurrentSpread { get; private set; }
    int nextSwingInCombo = 0;
    bool onCooldown = false;
    float gracePeriod = 0f; // Time after a swing where the next swing in the combo can be performed, even if the time between swings exceeds the combo window.

    private PlayerLook playerLook;

    Coroutine activeSwing;

    void Awake()
    {
        Data = _prviateData;
    }

    void Start()
    {
        PlayerState = GetComponentInParent<PlayerState>();

        // Set up controls
        playerMovement = FindAnyObjectByType<PlayerMovement>();
        playerLook = FindAnyObjectByType<PlayerLook>();
        sway = GetComponentInParent<WeaponRecoil>();

        if (Crosshair == null)
        {
            Crosshair = FindAnyObjectByType<CrosshairController>();
            CrosshairObject = Crosshair.gameObject;
        }
    }

    void Update()
    {
        if (Player == null) return;

        if (gracePeriod > 0f)
        {
            gracePeriod -= Time.deltaTime;

            if (gracePeriod <= 0f)
            {
                nextSwingInCombo = 0; // Reset combo
            }
        }

        // Swing canceling with Spring
        if (onCooldown && PlayerState.IsSprinting)
        {
            StopCoroutine(activeSwing);
            activeSwing = null;
            nextSwingInCombo = 0;
            onCooldown = false;
        }

        // Disable block is sprint start
        if (Player.state.IsSprinting && PlayerState.IsBlocking)
            SecondaryFire(false);
    }

    void OnEnable()
    {
        if (sway == null) sway = GetComponentInParent<WeaponRecoil>();
    }

    void OnDisable()
    {
        if(PlayerState == null) return;

        if(PlayerState.IsBlocking)
        {
            Player.Stats.Defense.RemoveAllModifiersFromSource(this);
            PlayerState.SetBlocking(false);
        }

        if (activeSwing != null)
        {
            StopCoroutine(activeSwing);
            activeSwing = null;
        }

        nextSwingInCombo = 0;
    }

    public void PrimaryFire(bool held)
    {
        if (onCooldown) return;

        if (Player.state.IsSprinting)
            playerMovement.ToggleSprint();

        if (nextSwingInCombo == 0)
        {
            // Perform first swing (Upper Left to bottom right)
            activeSwing = StartCoroutine(SwingOne());
        }
        else
        {
            // Perform second swing (Upper Right to bottom left)
            activeSwing = StartCoroutine(SwingTwo());
        }
    }

    public void SecondaryFire(bool performed)
    {
        if(Player.state.IsSprinting && performed)
            playerMovement.ToggleSprint();

        // Hold to Block
        if (performed)
        {
            PlayerState.SetBlocking(true);
            CrosshairObject.SetActive(false);
            Player.Stats.Defense.AddModifier(new StatModifier(0.5f, StatModType.PercentMult, this));
            Player.Stats.MoveSpeed.AddModifier(new StatModifier(-0.3f, StatModType.PercentMult, this));
        }
        else
        {
            CrosshairObject.SetActive(true);
            Player.Stats.Defense.RemoveAllModifiersFromSource(this);
            Player.Stats.MoveSpeed.RemoveAllModifiersFromSource(this);
            PlayerState.SetBlocking(false);
        }
    }

    public void Reload()
    {
        return;
    }

    public void SwitchMode()
    {
        return;
    }

    IEnumerator SwingOne()
    {
        onCooldown = true;
        yield return new WaitForSeconds(Data.RoundsPerMinute / 2f);

        swordAudio.PlayOneShot(swordSound);

        // Perform raycast and damage logic here (Upper Left to bottom right)

        // 1. Define the swing volume in front of the player
        Vector3 boxCenter = FpsCam.transform.position + FpsCam.transform.forward * (Data.Range / 2f);
        Vector3 halfExtents = new Vector3(2.5f, 2f, Data.Range / 2f); // Slightly wider for better "cleave"
        Quaternion boxRotation = FpsCam.transform.rotation;

        // 2. Find ALL colliders within that box
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, halfExtents, boxRotation, hitMask);

        if (hitColliders.Length > 0)
        {
            float totalDamage =
                Player.Stats.WeaponDamage.GetValue() *
                (!PlayerState.IsGrounded ? Player.Stats.AirborneDamageMultiplier.GetValue() : 1f) *
                Player.Stats.AllDamageMultiplier.GetValue();
            
            List<EnemyHealth> damagedEnemies = new(); // To track which enemies have already been damaged in this swing
            // 3. Apply Damage
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Hitbox hb = hitColliders[i].GetComponent<Hitbox>();
                if (hb != null && !damagedEnemies.Contains(hb.TargetHealth))
                {
                    hb.TakeDamage(Player.gameObject.transform, totalDamage, hitColliders[i].ClosestPoint(boxCenter), true);
                    damagedEnemies.Add(hb.TargetHealth);

                    // Reduce damage for subsequent hits in the same swing to simulate "cleave" damage falloff
                    // Reduce damage by 25% for each additional enemy hit in the same swing, modified by the player's Cleave stat
                    totalDamage *= (0.75f * Player.Stats.Cleave.GetValue());
                }
                else
                {
                    // Handle Feedback
                    if (impactEffect != null)
                    {
                        GameObject impact = Instantiate(impactEffect, hitColliders[i].ClosestPoint(boxCenter), Quaternion.LookRotation(hitColliders[i].ClosestPoint(boxCenter)));
                        Destroy(impact, 2f);
                    }
                }
            }
        }

        yield return new WaitForSeconds(Data.RoundsPerMinute / 2f);

        onCooldown = false;
        nextSwingInCombo = 1;

        gracePeriod = Data.ReloadTime;
    }

    IEnumerator SwingTwo()
    {
        onCooldown = true;
        yield return new WaitForSeconds(Data.RoundsPerMinute / 2f);

        swordAudio.PlayOneShot(swordSound);

        // Perform raycast and damage logic here (Upper Right to bottom left)

        // 1. Define the swing volume in front of the player
        Vector3 boxCenter = FpsCam.transform.position + FpsCam.transform.forward * (Data.Range / 2f);
        Vector3 halfExtents = new Vector3(2.5f, 2f, Data.Range / 2f); // Slightly wider for better "cleave"
        Quaternion boxRotation = FpsCam.transform.rotation;

        // 2. Find ALL colliders within that box
        Collider[] hitColliders = Physics.OverlapBox(boxCenter, halfExtents, boxRotation, hitMask);


        if (hitColliders.Length > 0)
        {
            float totalDamage =
                Player.Stats.WeaponDamage.GetValue() *
                Player.Stats.MagazineDamageBonusMultiplier.GetValue() *
                (!PlayerState.IsGrounded ? Player.Stats.AirborneDamageMultiplier.GetValue() : 1f) *
                Player.Stats.AllDamageMultiplier.GetValue();

            // 3. Apply Damage
            List<EnemyHealth> damagedEnemies = new(); // To track which enemies have already been damaged in this swing
            for (int i = 0; i < hitColliders.Length; i++)
            {
                Hitbox hb = hitColliders[i].GetComponent<Hitbox>();
                if (hb != null && !damagedEnemies.Contains(hb.TargetHealth))
                {
                    hb.TakeDamage(Player.gameObject.transform, totalDamage, hitColliders[i].ClosestPoint(boxCenter), true);
                    damagedEnemies.Add(hb.TargetHealth);

                    // Reduce damage for subsequent hits in the same swing to simulate "cleave" damage falloff
                    // Reduce damage by 25% for each additional enemy hit in the same swing, modified by the player's Cleave stat
                    totalDamage *= (0.75f * Player.Stats.Cleave.GetValue());
                }
                else
                {
                    // Handle Feedback
                    if (impactEffect != null)
                    {
                        GameObject impact = Instantiate(impactEffect, hitColliders[i].ClosestPoint(boxCenter), Quaternion.LookRotation(hitColliders[i].ClosestPoint(boxCenter)));
                        Destroy(impact, 2f);
                    }
                }
            }
        }

        yield return new WaitForSeconds(Data.RoundsPerMinute / 2f);
        onCooldown = false;
        nextSwingInCombo = 1;

        gracePeriod = Data.ReloadTime;
    }
}
