using RimWorld;
using Verse;

namespace Mwah;

public sealed class KissTreeAddon : KissThingAddon
{
    private const string AffirmingScope = "KissTreeAffirming";
    private const string ConnectedScope = "KissTreeConnected";

    public override bool Accepts(Thing thing) =>
        thing is Plant { Spawned: true, Destroyed: false } && thing.def.plant.IsTree;

    public override string OptionLabel(Pawn doer, Thing target) => "MWAH.KissTree.Option".Translate(target.LabelCap);

    public override string ResolveFateScope(Pawn doer, Thing target)
    {
        if (!MwahMod.Settings.treeCulturalIntegration || !ModsConfig.IdeologyActive || doer.Ideo == null)
            return Scope;

        KissTreeCultureRule? selected = null;
        foreach (KissTreeCultureRule rule in Definition.cultureRules)
        {
            if (rule.precept == null || !doer.Ideo.HasPrecept(rule.precept)
                || (selected != null && selected.priority >= rule.priority)) continue;
            if (rule.requiresOwnConnection && target.TryGetComp<CompTreeConnection>()?.ConnectedPawn != doer) continue;
            selected = rule;
        }
        return selected?.fateScope ?? Scope;
    }

    public override void SettleWithoutTable(Pawn doer, Thing target, string scope)
    {
        int[] weights = scope == ConnectedScope ? new[] { 0, 10, 25, 40, 25 }
            : scope == AffirmingScope ? new[] { 3, 17, 35, 30, 15 } : new[] { 10, 30, 35, 20, 5 };
        int roll = Rand.RangeInclusive(0, 99);
        int tier = 0;
        for (; tier < weights.Length - 1; tier++)
        {
            roll -= weights[tier];
            if (roll < 0) break;
        }
        string[] messages = { "MWAH.KissTree.Result.Awkward", "MWAH.KissTree.Result.Quiet",
            "MWAH.KissTree.Result.Calm", "MWAH.KissTree.Result.Rooted", "MWAH.KissTree.Result.Belonging" };
        string[] stages = { "Awkward", "Quiet", "Calm", "Rooted", "Belonging" };
        string policy = scope == ConnectedScope ? "Connected" : scope == AffirmingScope ? "Affirming" : "Basic";
        KissFate.GrantRow(doer, target, new MWAH_FateDef
        {
            defName = "MWAH_Fate_Fallback_Tree",
            scope = scope,
            thought = MWAH_ThoughtDefOf.MWAH_KissedTree,
            stageIndex = tier,
            narrationKey = "MWAH.Fate.Tree." + policy + "." + stages[tier],
            messageKey = messages[tier],
            messageType = tier == 0 ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.NeutralEvent,
        }, ThoughtDurationTicks);
    }
}
