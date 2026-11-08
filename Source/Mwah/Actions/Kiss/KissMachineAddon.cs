using System;
using System.Collections.Generic;
using Mwah.Compatibility;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>One machine identity; exact vanilla exceptions supply local policies.</summary>
public sealed class KissMachineAddon : KissThingAddon
{
    private const string ResearchScope = "KissMachineResearch";
    private const string PasteScope = "KissMachinePaste";
    private static readonly int[] Weights = { 20, 30, 25, 17, 8 };
    private static readonly string[][] Narrations =
    {
        new[] { "MWAH.Fate.Machine.Basic.Awkward", "MWAH.Fate.Machine.Basic.Quiet", "MWAH.Fate.Machine.Basic.Warm", "MWAH.Fate.Machine.Basic.Answered", "MWAH.Fate.Machine.Basic.Devoted" },
        new[] { "MWAH.Fate.Machine.Research.Awkward", "MWAH.Fate.Machine.Research.Quiet", "MWAH.Fate.Machine.Research.Warm", "MWAH.Fate.Machine.Research.Answered", "MWAH.Fate.Machine.Research.Devoted" },
        new[] { "MWAH.Fate.Machine.Paste.Awkward", "MWAH.Fate.Machine.Paste.Quiet", "MWAH.Fate.Machine.Paste.Warm", "MWAH.Fate.Machine.Paste.Answered", "MWAH.Fate.Machine.Paste.Devoted" },
    };
    private static readonly string[] ResultMessageKeys =
    {
        "MWAH.KissMachine.Result.Awkward", "MWAH.KissMachine.Result.Quiet",
        "MWAH.KissMachine.Result.Warm", "MWAH.KissMachine.Result.Answered", "MWAH.KissMachine.Result.Devoted",
    };

    private static bool IsPaste(Thing target) => target.def == MWAH_MachineTargetDefOf.NutrientPasteDispenser
        || VanillaNutrientPasteExpandedCompatibility.IsOutlet(target.def);
    private static bool IsTouchPaste(Thing target) =>
        VanillaNutrientPasteExpandedCompatibility.IsTouchOutlet(target.def);
    private static bool IsAnalyzer(Thing target) => target.def == MWAH_MachineTargetDefOf.MultiAnalyzer;
    private static bool IsResearch(Thing target) => IsAnalyzer(target)
        || target.def == MWAH_MachineTargetDefOf.SimpleResearchBench
        || target.def == MWAH_MachineTargetDefOf.HiTechResearchBench;

    public override bool Accepts(Thing thing) =>
        thing is Building { Spawned: true, Destroyed: false }
        && (IsPaste(thing) || IsResearch(thing) || thing is Building_WorkTable);

    public override bool UsesFixedStandCell => true;

    public override AcceptanceReport CheckTarget(Pawn doer, Thing target) =>
        target.TryGetComp<CompPowerTrader>() is { PowerOn: false }
            ? new AcceptanceReport("MWAH.KissMachine.Fail.NoPower".Translate(target.Named("TARGET")))
            : AcceptanceReport.WasAccepted;

    public override string OptionLabel(Pawn doer, Thing target) =>
        (IsPaste(target) ? "MWAH.KissMachine.OptionPaste" : "MWAH.KissMachine.Option").Translate(target.LabelCap);

    public override string OptionTooltip(Thing target) => IsTouchPaste(target)
        ? (string)"MWAH.KissMachine.PasteTouchTooltip".Translate(target.Named("TARGET"))
        : IsPaste(target) ? (string)"MWAH.KissMachine.PasteTooltip".Translate(target.Named("TARGET"))
        : base.OptionTooltip(target);

    public override string? JobReport(Thing target) => IsPaste(target)
        ? (string)"MWAH.KissMachine.ReportPaste".Translate(target.Named("TARGET")) : null;

    public override string MoodlessLabel(Pawn doer, Thing target) =>
        IsPaste(target) && doer.needs?.food != null
            ? (string)"MWAH.KissMachine.NoMoodFood".Translate()
            : base.MoodlessLabel(doer, target);

    public override string ResolveFateScope(Pawn doer, Thing target) =>
        IsPaste(target) ? PasteScope : IsResearch(target) ? ResearchScope : Scope;

    private static bool PhysicalStandCell(Pawn pawn, Thing target, IntVec3 cell) =>
        pawn.Map != null && target.Map == pawn.Map && cell.IsValid && cell.InBounds(pawn.Map)
        && cell.Standable(pawn.Map) && !cell.Fogged(pawn.Map)
        && ReachabilityImmediate.CanReachImmediate(cell, target, pawn.Map, PathEndMode.Touch, pawn);

    public override bool ValidStandCell(Pawn doer, Thing target, IntVec3 cell)
    {
        if (!PhysicalStandCell(doer, target, cell)) return false;
        if (IsPaste(target) && !IsTouchPaste(target))
        {
            IntVec3 center = target.InteractionCell;
            IntVec3 side = target.Rotation.RighthandCell;
            return cell == center || cell == center - side || cell == center + side;
        }
        return IsAnalyzer(target) || IsTouchPaste(target) || cell == target.InteractionCell;
    }

    public override IntVec3 SelectStandCell(Pawn doer, Thing target)
    {
        if (doer.Map == null || target.Map != doer.Map || !doer.CanReserve(target)) return IntVec3.Invalid;
        var candidates = new List<IntVec3>();
        if (IsPaste(target) && !IsTouchPaste(target))
        {
            IntVec3 center = target.InteractionCell;
            IntVec3 side = target.Rotation.RighthandCell;
            IntVec3 left = center - side;
            IntVec3 right = center + side;
            candidates.Add(center);
            if (left.DistanceToSquared(doer.Position) <= right.DistanceToSquared(doer.Position))
            {
                candidates.Add(left);
                candidates.Add(right);
            }
            else
            {
                candidates.Add(right);
                candidates.Add(left);
            }
        }
        else if (IsAnalyzer(target) || IsTouchPaste(target))
        {
            // Small pass-through outlets also allow a legal cell inside their footprint.
            if (IsTouchPaste(target)) candidates.AddRange(GenAdj.CellsOccupiedBy(target));
            candidates.AddRange(GenAdj.CellsAdjacent8Way(target));
            candidates.Sort((a, b) =>
            {
                int distance = a.DistanceToSquared(doer.Position).CompareTo(b.DistanceToSquared(doer.Position));
                if (distance != 0) return distance;
                int x = a.x.CompareTo(b.x);
                return x != 0 ? x : a.z.CompareTo(b.z);
            });
        }
        else candidates.Add(target.InteractionCell);

        foreach (IntVec3 cell in candidates)
            if (ValidStandCell(doer, target, cell) && doer.CanReserveSittableOrSpot(cell)
                && doer.CanReserve(cell) && doer.CanReach(cell, PathEndMode.OnCell, Danger.Deadly))
                return cell;
        return IntVec3.Invalid;
    }

    public override void Settle(Pawn doer, Thing target)
    {
        if (!CheckTarget(doer, target).Accepted) return;
        string scope = ResolveFateScope(doer, target);
        GrantOutcome(doer, target, KissFate.Roll(scope) ?? Fallback(scope));
    }

    public override void SettleWithoutTable(Pawn doer, Thing target, string scope)
    {
        if (CheckTarget(doer, target).Accepted) GrantOutcome(doer, target, Fallback(scope));
    }

    private MWAH_FateDef Fallback(string scope)
    {
        int roll = Rand.RangeInclusive(0, 99);
        int tier = 0;
        for (; tier < Weights.Length - 1; tier++)
        {
            roll -= Weights[tier];
            if (roll < 0) break;
        }
        int policy = scope == PasteScope ? 2 : scope == ResearchScope ? 1 : 0;
        return new MWAH_FateDef
        {
            defName = "MWAH_Fate_Fallback_Machine",
            scope = scope,
            thought = MWAH_ThoughtDefOf.MWAH_KissedMachine,
            stageIndex = tier,
            narrationKey = Narrations[policy][tier],
            messageKey = ResultMessageKeys[tier],
            messageType = tier == 0 ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.NeutralEvent,
            modExtensions = new List<DefModExtension>
            {
                new KissMachineOutcomeExtension { goodOutcome = tier >= 3 },
            },
        };
    }

    private void GrantOutcome(Pawn doer, Thing target, MWAH_FateDef row)
    {
        float strength = 1f;
        if (IsResearch(target) && row.thought != null
            && row.thought.stages[row.stageIndex].baseMoodEffect > 0f)
        {
            // Do not let GetSkill's missing-record fallback borrow an unrelated skill.
            SkillRecord? intellectual = doer.skills?.skills.Find(skill => skill.def == SkillDefOf.Intellectual);
            strength += Mathf.Clamp(intellectual?.Level ?? 0, 0, 20) / 20f;
        }
        KissFate.GrantRow(doer, target, row, ThoughtDurationTicks, strength);
        if (IsPaste(target) && row.GetModExtension<KissMachineOutcomeExtension>()?.goodOutcome == true
            && doer.needs?.food is { } food)
        {
            food.CurLevelPercentage = 1f;
            Messages.Message("MWAH.KissMachine.Result.Fed".Translate(doer.Named("PAWN"), target.Named("TARGET")),
                new LookTargets(target), MessageTypeDefOf.PositiveEvent, historical: false);
        }
    }
}

/// <summary>Explicit fate metadata, independent of mood value and need availability.</summary>
public sealed class KissMachineOutcomeExtension : DefModExtension
{
    public bool goodOutcome;
}
