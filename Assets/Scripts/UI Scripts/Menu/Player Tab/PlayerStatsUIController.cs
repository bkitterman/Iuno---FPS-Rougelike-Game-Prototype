using UnityEngine;
using UnityEngine.UI;
using TMPro;

using System.Linq;
using System.Collections.Generic;

public class PlayerStatsUIController : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject statMathUIHolder;

    [Header("References")]
    [SerializeField] private Transform mathPanel;
    [SerializeField] private Transform statsPanel;
    [SerializeField] private Transform hooksPanel;
    [SerializeField] private TextMeshProUGUI mathFinalStatText;
    [SerializeField] private TextMeshProUGUI mathInitialStatText;

    [SerializeField] private GameObject mathProgramsPanel;
    [SerializeField] private GameObject mathProgramsPanelHeader;
    [SerializeField] private GameObject mathSuitesPanel;
    [SerializeField] private GameObject mathSuitesPanelHeader;
    [SerializeField] private GameObject mathStatusEffectsPanel;
    [SerializeField] private GameObject mathStatusEffectsPanelHeader;

    [SerializeField] private StatsController controller;

    // States
    private Stat currentDisplayedStat = Stat.None;
    private PlayerStatUIHolder currentHolder;

    void Awake()
    {
        List<PlayerStatUIHolder> holderList = statsPanel.GetComponentsInChildren<PlayerStatUIHolder>().ToList();
        foreach (PlayerStatUIHolder holder in holderList)
        {
            holder.Border.SetActive(false);
            holder.BuffIcon.SetActive(false);
            holder.DebuffIcon.SetActive(false);

            holder.gameObject.AddComponent<Button>().onClick.AddListener(() => {
                holder.IsSelected = true;
                holder.Border.SetActive(true);

                if (currentHolder != null)
                {
                    currentHolder.IsSelected = false;
                    currentHolder.Border.SetActive(false);
                }

                currentHolder = holder;

                DisplayMath(holder.Stat);
            });
        }
        
        // Do above for Hooks
    }

    void OnEnable()
    {
        if(currentDisplayedStat == Stat.None)
        {
            DisplayMath(Stat.Health);
        }
        else
        {
            DisplayMath(currentDisplayedStat);
        }

        // Iteration through all stats buttons.
        List<PlayerStatUIHolder> holderList = statsPanel.GetComponentsInChildren<PlayerStatUIHolder>().ToList();
        foreach (PlayerStatUIHolder holder in holderList)
        {
            if (holder.Stat == Stat.None)
            {
                Debug.LogError("PlayerStatUIHolder has Stat.None, assign a stat to it in the inspector.");
                continue;
            }
            PlayerStat mod = controller.GetStatModifierByType(holder.Stat);

            if (mod == null)
            {
                Debug.LogError("No PlayerStat found for " + holder.Stat.ToString() + " in PlayerStatsUIController. Make sure the stat is added to the StatsController.");
                holder.SetValue(-100f);
                continue;
            }

            holder.SetValue(mod.GetValue());

            // Show Buff/Debuff arrows
            List<Stat> reverseArrows = new List<Stat> { Stat.ReloadSpeed, Stat.RecoilHorizontalPerShot, Stat.RecoilVerticalPerShot, Stat.AirborneEffectiveness, Stat.BulletSpread };

            if (reverseArrows.Contains(holder.Stat)) {
                if (mod.GetValue() > mod.BaseValue)
                {
                    holder.DebuffIcon.SetActive(true);
                    holder.BuffIcon.SetActive(false);
                }
                else if (mod.GetValue() < mod.BaseValue)
                {
                    holder.DebuffIcon.SetActive(false);
                    holder.BuffIcon.SetActive(true);
                }
                else
                {
                    holder.DebuffIcon.SetActive(false);
                    holder.BuffIcon.SetActive(false);
                }
            }
            else
            {
                if (mod.GetValue() < mod.BaseValue)
                {
                    holder.DebuffIcon.SetActive(true);
                    holder.BuffIcon.SetActive(false);
                }
                else if (mod.GetValue() > mod.BaseValue)
                {
                    holder.DebuffIcon.SetActive(false);
                    holder.BuffIcon.SetActive(true);
                }
                else
                {
                    holder.DebuffIcon.SetActive(false);
                    holder.BuffIcon.SetActive(false);
                }
            }
        }

        // Show active hooks and inactive hooks.

    }

    void DisplayMath(Stat stat)
    {
        // Clear all previous from panels
        foreach (Transform child in mathProgramsPanel.transform)
        {
            if(child.gameObject == mathProgramsPanelHeader)             
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        foreach (Transform child in mathSuitesPanel.transform)
        {
            if(child.gameObject == mathSuitesPanelHeader)             
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        foreach (Transform child in mathStatusEffectsPanel.transform)
        {
            if (child.gameObject == mathStatusEffectsPanelHeader)
            {
                continue;
            }
            Destroy(child.gameObject);
        }

        // Add new and updated rows for each panel.
        PlayerStat modifier = controller.GetStatModifierByType(stat);

        if (stat == Stat.FireRate)
        {
            mathFinalStatText.text = $"Final Value: {modifier.GetValue()}rps ({60f / modifier.GetValue()}rpm)";
            mathInitialStatText.text = $"Initial Value ({modifier.BaseValueSource}): {modifier.BaseValue}rps ({60f / modifier.GetValue()}rpm)";
        }
        else
        {
            mathFinalStatText.text = $"Final Value: {modifier.GetValue()}";
            mathInitialStatText.text = $"Initial Value ({modifier.BaseValueSource}): {modifier.BaseValue}";
        }


        // Programs
        List<StatModifier> programMods = modifier.StatModifiers.Where(mod => mod.Source is ProgramInstance).ToList();
        
        if (programMods.Count > 0)
        {
            List<ProgramInstance> tracker = new();
            foreach (StatModifier mod in programMods)
            {
                // Prevents duplicate rows from showing up
                if(tracker.Contains((ProgramInstance)mod.Source))
                {
                    continue;
                }
                else
                {
                    tracker.Add((ProgramInstance)mod.Source);
                }

                GameObject newRow = Instantiate(statMathUIHolder, mathProgramsPanel.transform);
                PlayerStatMathUIHolder holder = newRow.GetComponent<PlayerStatMathUIHolder>();
                ProgramInstance program = ((ProgramInstance)mod.Source);
                ProgramData data = program.Data;

                holder.Icon.sprite = data.IconSprite;  

                // Build text
                string calcValue = "";
                switch(data.ScaleMethod)
                {
                    case ScalingMethod.Linear:
                        if (program is Aquila aquila)
                            calcValue = $"{data.Multiplier} * {program.ActiveStacks} * {aquila.GetKillCount()}";
                        else
                            calcValue = $"{data.Multiplier} * {program.ActiveStacks}";
                        break;
                    case ScalingMethod.Exponential:
                        calcValue = $"{data.Multiplier} ^ {program.ActiveStacks}";
                        break;
                    case ScalingMethod.Logarithmic:
                        calcValue = $"log({data.Multiplier}) * {program.ActiveStacks}";
                        break;
                    case ScalingMethod.Hyperbolic:
                        calcValue = $"{program.ActiveStacks} / ({data.Multiplier} + {program.ActiveStacks})";
                        break;
                    default:
                        calcValue = "ERR: Scale Method";
                        break;
                }

                string finalEquation = "";
                switch (data.RoutineScaling)
                {
                    case StatModType.Flat:
                        finalEquation = $"{calcValue}";
                        break;
                    case StatModType.PercentAdd:
                        finalEquation = $"{modifier.BaseValue} + ({modifier.BaseValue} * {calcValue})";
                        break;
                    case StatModType.PercentMult:
                        finalEquation = $"{modifier.BaseValue} * ( 1 + {calcValue})";
                        break;
                    default:
                        finalEquation = "ERR: Stat Mod";
                        break;
                }

                string finalValue = "";

                switch(data.RoutineScaling)
                {
                    case StatModType.Flat:
                        finalValue = $"+{program.GetProgramMultiplier()}";
                        break;
                    case StatModType.PercentAdd:
                        finalValue = $"{modifier.GetPrePercentAddValue() * program.GetProgramMultiplier()} (+{program.GetProgramMultiplier() * 100}%)";
                        break;
                    case StatModType.PercentMult:
                        finalValue = $"{modifier.GetPrePercentMultValue() * program.GetProgramMultiplier()} (*{program.GetProgramMultiplier() * 100}%)";
                        break;
                    default:
                        finalValue = "ERR: Stat Mod Final";
                        break;
                }

                holder.Text.text = $"{data.ProgramName} [x{program.ActiveStacks}] => " +
                    $"{finalEquation} = {finalValue}";

                holder.IconHolder.TooltipHeader = data.ProgramName;
                holder.IconHolder.TooltipBody = data.InDepthDescription;
                holder.IconHolder.ToolTipSubtitle = $"Program - {data.ApplicationSuite.Name}";
            }

            mathProgramsPanel.SetActive(true);
        }
        else
        {
            mathProgramsPanel.SetActive(false);
        }

        // Suites
        List<StatModifier> suiteMods = modifier.StatModifiers.Where(mod => mod.Source is ApplicationSuiteInstance).ToList();
        if (suiteMods.Count > 0)
        {
            GameObject newRow = Instantiate(statMathUIHolder, mathSuitesPanel.transform);
            PlayerStatMathUIHolder holder = newRow.GetComponent<PlayerStatMathUIHolder>();
            holder.Text.text = "Active Suites: " + string.Join(", ", suiteMods.Select(mod => ((ApplicationSuiteInstance)mod.Source).Data.Name));


            mathSuitesPanel.SetActive(true);
        }
        else
        {
            mathSuitesPanel.SetActive(false);
        }



        // Status Effects
        List<StatModifier> statusEffectMods = modifier.StatModifiers.Where(mod => mod.Source is StatusEffect).ToList();
        if (statusEffectMods.Count > 0)
        {
            List<StatusEffect> tracker = new();
            foreach (StatModifier mod in statusEffectMods)
            {
                // Prevents duplicate rows from showing up
                if (tracker.Contains((StatusEffect)mod.Source))
                {
                    continue;
                }
                else
                {
                    tracker.Add((StatusEffect)mod.Source);
                }

                GameObject newRow = Instantiate(statMathUIHolder, mathStatusEffectsPanel.transform);
                PlayerStatMathUIHolder holder = newRow.GetComponent<PlayerStatMathUIHolder>();
                StatusEffect effect = ((StatusEffect)mod.Source);
                StatusEffectData data = effect.GetData();
                holder.Icon.sprite = data.Icon;

                int effectStacks = 1;
                if(data.StackingBehavior == StackingType.Independent)
                    effectStacks = statusEffectMods.Count(mod => mod.Source == effect);

                string finalValue = "";
                string finalEquation = "";
                string calcValue = "";

                if (data is StatModifierEffectData)
                {
                    StatModifierEffectData statModifierData = (StatModifierEffectData)data;

                    calcValue = $"{statModifierData.ModifierValue} * {effectStacks}";

                    // Build text
                    switch (statModifierData.ModifierType)
                    {
                        case StatModType.Flat:
                            finalEquation = $"{calcValue}";
                            break;
                        case StatModType.PercentAdd:
                            finalEquation = $"{modifier.BaseValue} + ({modifier.BaseValue} * {calcValue})";
                            break;
                        case StatModType.PercentMult:
                            finalEquation = $"{modifier.BaseValue} * ( 1 + {calcValue})";
                            break;
                        default:
                            finalEquation = "ERR: Stat Mod";
                            break;
                    }

                    switch (statModifierData.ModifierType)
                    {
                        case StatModType.Flat:
                            finalValue = $"+{statModifierData.ModifierValue * effectStacks}";
                            break;
                        case StatModType.PercentAdd:
                            finalValue = $"{modifier.GetPrePercentAddValue() * statModifierData.ModifierValue * effectStacks} (+{statModifierData.ModifierValue * effectStacks * 100}%)";
                            break;
                        case StatModType.PercentMult:
                            finalValue = $"{modifier.GetPrePercentMultValue() * statModifierData.ModifierValue * effectStacks} (*{statModifierData.ModifierValue * effectStacks * 100}%)";
                            break;
                        default:
                            finalValue = "ERR: Stat Mod Final";
                            break;
                    }
                }
                else
                {
                    Debug.LogWarning("No implementation for " + data.GetType().Name + " in status effects UI of stats tab in menu.");
                }


                holder.Text.text = $"{data.Name} {(data.StackingBehavior == StackingType.Independent ? $"x[{effectStacks}] " : "")}=> " +
                    $"{finalEquation} = {finalValue}";

                holder.IconHolder.TooltipHeader = data.Name;
                holder.IconHolder.TooltipBody = data.Description;
                holder.IconHolder.ToolTipSubtitle = $"Status Effect - {(data.IsBuff ? "Buff" : "Debuff")}";
            }

            mathStatusEffectsPanel.SetActive(true);
        }
        else
        {
            mathStatusEffectsPanel.SetActive(false);
        }



        currentDisplayedStat = stat;
    }



}
