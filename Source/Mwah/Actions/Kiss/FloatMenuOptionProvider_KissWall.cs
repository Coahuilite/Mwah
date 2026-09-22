using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// "亲吻这堵墙"的右键入口。与双人 provider 一样靠 FloatMenuMakerMap 的反射发现，零 Def 零补丁。
/// 人形发起方的菜单文案是迟疑版「亲吻{0}…?!」——智人及其异种、har 及其异种、任何 mod 人形种
/// 都落在 RaceProps.Humanlike 这一个谓词里；机械族等非人形下令亲墙时不迟疑，用普通「亲吻{0}」。
/// </summary>
public class FloatMenuOptionProvider_KissWall : FloatMenuOptionProvider
{
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => true;

    protected override bool RequiresManipulation => false;

    protected override bool MechanoidCanDo => true;

    public override bool TargetThingValid(Thing thing, FloatMenuContext context)
    {
        return KissWallUtility.IsWallLike(thing);
    }

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Thing clickedThing, FloatMenuContext context)
    {
        if (clickedThing == null)
        {
            yield break;
        }
        foreach (Pawn selected in context.ValidSelectedPawns)
        {
            FloatMenuOption? option = BuildOption(selected, clickedThing);
            if (option != null)
            {
                yield return option;
            }
        }
    }

    private static FloatMenuOption? BuildOption(Pawn selected, Thing wall)
    {
        KissWallUtility.WallKissProposal proposal = KissWallUtility.Propose(selected, wall);
        if (!proposal.Visible)
        {
            return null;
        }

        string labelKey = selected.RaceProps.Humanlike
            ? "MWAH.KissWall.OptionHumanlike"
            : "MWAH.KissWall.Option";
        string label = labelKey.Translate(wall.LabelCap);
        if (selected.needs?.mood == null)
        {
            // 墙本来就不会给"对方"心情，这里的无心情标记只提醒发起方：这一亲不会有你的份。
            label += " " + "MWAH.FloatMenu.KissNoMood".Translate();
        }

        if (!proposal.Allowed || proposal.Doer == null)
        {
            return new FloatMenuOption(label + " (" + proposal.BlockedReason + ")", null,
                MenuOptionPriority.InitiateSocial, null, wall);
        }

        return new FloatMenuOption(label, delegate { KissWallUtility.BeginKiss(selected, wall); },
            MenuOptionPriority.InitiateSocial, null, wall)
        {
            tooltip = new TipSignal("MWAH.KissWall.OptionTooltip".Translate(wall.Named("WALL")))
        };
    }
}
