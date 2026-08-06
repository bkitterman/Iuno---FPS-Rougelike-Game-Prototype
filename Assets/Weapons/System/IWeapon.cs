using UnityEngine;

public interface IWeapon
{
    public WeaponData Data { get; set; }
    public Player Player {  get; set; }
    public Camera FpsCam {  get; set; }
    public PlayerState PlayerState { get; set; }

    public void PrimaryFire(bool held);
    public void SecondaryFire(bool performed);
    public void Reload();
    public void SwitchMode();

    // Monobehaviour shenanigans
    public GameObject gameObject { get; }
}
