using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// "亲吻这堵墙/这棵树/……"的右键入口 —— 全 addon 族共用这一个 provider
/// （FloatMenuMakerMap 反射发现，零 Def 零补丁）。每个开着的 addon 对点中的 Thing
/// 认领一次：认领成立才提案，提案 Hidden（开关中途关掉、目标已塌）连灰项都不给。
/// 文案口径归 addon 自己（墙在这里显示人形迟疑版）。
/// </summary>
public class FloatMenuOptionProvider_KissThing : FloatMenuOptionProvider
{
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => true;

    protected override bool RequiresManipulation => false;

    protected override bool MechanoidCanDo => true;

    public override bool TargetThingValid(Thing thing, FloatMenuContext context)
    {
        return KissThingAddons.ActiveFor(thing);
    }

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
    {
        if (clickedThing == null)
        {
            yield break;
        }
        KissThingAddon? addon = KissThingAddons.For(clickedThing);
        if (addon is not { Active: true })
        {
            yield break;
        }
        foreach (Pawn selected in context.ValidSelectedPawns)
        {
            FloatMenuOption? option = BuildOption(addon, selected, clickedThing, context);
            if (option != null)
            {
                yield return option;
            }
        }
    }

    private static FloatMenuOption? BuildOption(KissThingAddon addon, Pawn selected, Thing target, FloatMenuContext context)
    {
        ThingKissProposal proposal = addon.Propose(selected, target);
        if (!proposal.Visible)
        {
            return null;
        }

        string label = addon.OptionLabel(selected, target);
        if (selected.needs?.mood == null)
        {
            // 目标本来就不会给"对方"心情，这里的无心情标记只提醒发起方：这一亲不会有你的份。
            label += " " + "MWAH.FloatMenu.KissNoMood".Translate();
        }
        if (context.IsMultiselect)
        {
            label = selected.LabelShort + ": " + label;
        }

        if (!proposal.Allowed || proposal.Doer == null)
        {
            return new FloatMenuOption(label + " (" + proposal.BlockedReason + ")", null,
                MenuOptionPriority.InitiateSocial, null, target);
        }

        return new FloatMenuOption(label, delegate { addon.BeginKiss(selected, target); },
            MenuOptionPriority.InitiateSocial, null, target)
        {
            tooltip = new TipSignal("MWAH.KissWall.OptionTooltip".Translate(target.Named("WALL")))
        };
    }
}
