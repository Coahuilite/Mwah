using System;
using System.Collections.Generic;
using Mwah;
using Verse;
using Verse.AI;

namespace RimWorld;

/// <summary>Content metadata; workers own target-specific behavior, not another execution pipeline.</summary>
public class MWAH_KissAddonDef : Def
{
    public string id = "";
    public Type workerClass = null!;
    public JobDef job = null!;
    public ThingCategory targetCategory;
    public string fateScope = "";
    public int priority;
    public string nameKey = "";
    public string switchLabelKey = "";
    public string switchDescKey = "";
    public string tooltipKey = "";
    public bool defaultActive = true;
    public int defaultThoughtDurationTicks = 30000;
    public bool culturalOptions;
    public List<KissTreeCultureRule> cultureRules = new();

    private KissThingAddon? worker;
    public KissThingAddon Worker
    {
        get
        {
            if (worker == null)
            {
                worker = (KissThingAddon)Activator.CreateInstance(workerClass)!;
                worker.Definition = this;
            }
            return worker;
        }
    }

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors()) yield return error;
        if (id.NullOrEmpty() || fateScope.NullOrEmpty() || nameKey.NullOrEmpty()
            || switchLabelKey.NullOrEmpty() || switchDescKey.NullOrEmpty() || tooltipKey.NullOrEmpty())
            yield return "Addon identity, scope and presentation keys are required.";
        if (workerClass == null || workerClass.IsAbstract || !typeof(KissThingAddon).IsAssignableFrom(workerClass)
            || workerClass.GetConstructor(Type.EmptyTypes) == null)
            yield return "workerClass must be a concrete KissThingAddon with a public default constructor.";
        if (job == null || job.GetModExtension<KissAddonJobExtension>()?.addon != this)
            yield return "The addon job must point back to this descriptor.";
        if (targetCategory is not (ThingCategory.Building or ThingCategory.Plant or ThingCategory.Item))
            yield return "Target category must be a building, plant or item, not a Pawn.";
        if (defaultThoughtDurationTicks < Constants.AddonDurationTicksRange.min
            || defaultThoughtDurationTicks > Constants.AddonDurationTicksRange.max)
            yield return "Default thought duration is outside the addon range.";
        foreach (MWAH_KissAddonDef other in DefDatabase<MWAH_KissAddonDef>.AllDefsListForReading)
            if (other != this && other.id == id) yield return "Duplicate addon identity: " + id;
        var priorities = new HashSet<int>();
        foreach (KissTreeCultureRule rule in cultureRules)
        {
            if (rule.precept == null || rule.fateScope.NullOrEmpty()) yield return "Incomplete cultural rule.";
            if (!priorities.Add(rule.priority)) yield return "Cultural rule priorities must be distinct.";
        }
    }
}

/// <summary>Only optional XML entries hold DLC Def references.</summary>
public class KissTreeCultureRule
{
    public PreceptDef precept = null!;
    public bool requiresOwnConnection;
    public int priority;
    public string fateScope = "";
}

public class KissAddonJobExtension : DefModExtension
{
    public MWAH_KissAddonDef addon = null!;
    public override IEnumerable<string> ConfigErrors()
    {
        if (addon == null) yield return "An addon job requires its descriptor.";
    }
}
