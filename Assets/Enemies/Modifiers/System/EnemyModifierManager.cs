using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(EnemyBrain))]
public class EnemyModifierManager : MonoBehaviour
{
    // --- Component References ---
    private EnemyBrain brain;
    private StatsController stats;
    // You'll need a reference to the renderer to change materials
    [SerializeField] private SkinnedMeshRenderer meshRenderer;

    // --- State ---
    private List<EnemyModifierInstance> activeLogic = new List<EnemyModifierInstance>();
    private List<GameObject> activeVfx = new List<GameObject>();

    void Awake()
    {
        brain = GetComponent<EnemyBrain>();
        stats = GetComponent<StatsController>();
    }

    /// <summary>
    /// This is the main public method. The SpawnManager will call this
    /// to add a modifier to this enemy instance.
    /// </summary>
    public void ApplyModifier(EnemyModifierData data)
    {
        if (data == null) return;

        // --- 1. Apply Visuals ---
        if (meshRenderer != null && data.EliteMaterial != null)
        {
            var materials = new List<Material>(meshRenderer.materials);
            materials.Add(data.EliteMaterial);
            meshRenderer.materials = materials.ToArray();
        }

        if (data.VfxPrefab != null)
        {
            // Spawn the VFX and parent it to the enemy
            GameObject vfx = Instantiate(data.VfxPrefab, transform.position, transform.rotation, transform);
            activeVfx.Add(vfx);
        }

        // --- 2. Apply Stat Modifiers ---
        if (data.StatModifiers != null && stats != null)
        {
            foreach (var modifier in data.StatModifiers)
            {
                PlayerStat stat = stats.GetStatModifierByType(modifier.TargetStat);
                if (stat != null)
                {
                    stat.AddModifier(new StatModifier(modifier.Value, modifier.Type, data));
                }
            }
        }

        // --- 3. Instantiate and Apply Logic ---
        if (!string.IsNullOrEmpty(data.Name))
        {
            System.Type logicType = System.Type.GetType(data.Name);
            if (logicType != null && typeof(EnemyModifierInstance).IsAssignableFrom(logicType))
            {
                EnemyModifierInstance logic = (EnemyModifierInstance)System.Activator.CreateInstance(logicType);
                logic.OnApply(brain, this);
                activeLogic.Add(logic);
            }
            else
            {
                Debug.LogError($"Could not find or invalid logic class: {data.Name}", this);
            }
        }
    }

    // --- HOOKS ---
    public float ProcessOnTakeDamage(float damageAmount, Vector3 damageSource)
    {
        float modifiedDamage = damageAmount;
        foreach (var logic in activeLogic)
        {
            modifiedDamage = logic.OnTakeDamage(modifiedDamage, damageSource);
        }
        return modifiedDamage;
    }

    public void ProcessOnDealDamage(PlayerHealth player, float damage)
    {
        foreach (var logic in activeLogic)
        {
            logic.OnDealDamage(player, damage);
        }
    }

    // --- LIFECYCLE ---
    void Update()
    {
        // Tick all active logic scripts
        foreach (var logic in activeLogic)
        {
            logic.Update();
        }
    }

    void OnDestroy()
    {
        // Clean up all logic scripts
        foreach (var logic in activeLogic)
        {
            logic.OnRemove();
        }
        activeLogic.Clear();

        // Clean up all VFX
        foreach (var vfx in activeVfx)
        {
            if (vfx != null) Destroy(vfx);
        }
        activeVfx.Clear();
    }
}