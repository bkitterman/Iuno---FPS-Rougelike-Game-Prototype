using UnityEngine;

public abstract class EnemyWeaponInstance : MonoBehaviour
{
    public abstract void Initialize(WeaponData data, EnemyBrain ownerBrain, EnemyModifierManager manager);
    public abstract void TryAttack(Transform target);
}
