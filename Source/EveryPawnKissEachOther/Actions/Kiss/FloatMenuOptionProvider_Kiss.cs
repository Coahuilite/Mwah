using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EveryPawnKissEachOther;

/// <summary>
/// 右键菜单入口。FloatMenuMakerMap.Init() 用 AllSubclassesNonAbstract() 反射发现所有
/// FloatMenuOptionProvider 子类，所以这个类不需要 Def、不需要 XML、也不需要 Harmony 补丁。
/// 目标是"能选中就能亲"：不限制种族、不要求是殖民者、也不要求征召。
/// </summary>
public class FloatMenuOptionProvider_Kiss : FloatMenuOptionProvider
{
    protected override bool Drafted => true;

    protected override bool Undrafted => true;

    protected override bool Multiselect => true;

    protected override bool RequiresManipulation => false;

    /// <summary>机械族也能亲（base 默认把它们整个排除在右键菜单之外）。</summary>
    protected override bool MechanoidCanDo => true;

    protected override bool CanSelfTarget => false;

    /// <summary>
    /// 故意不调用 base：base 开头就用 MutantDef.whitelistedFloatMenuProviders
    /// 把所有未登记的 mutant/亚人静默屏蔽，而"每个小人都能亲"正是本模组的存在理由。
    /// </summary>
    public override bool SelectedPawnValid(Pawn pawn, FloatMenuContext context)
    {
        return pawn != null && pawn.Spawned && !pawn.Dead;
    }

    public override bool TargetPawnValid(Pawn pawn, FloatMenuContext context)
    {
        if (!base.TargetPawnValid(pawn, context))
        {
            return false;
        }
        return pawn != null && pawn.Spawned && !pawn.Dead;
    }

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        if (clickedPawn == null)
        {
            yield break;
        }
        foreach (Pawn selected in context.ValidSelectedPawns)
        {
            FloatMenuOption? option = BuildOption(selected, clickedPawn, context);
            if (option != null)
            {
                yield return option;
            }
        }
    }

    private static FloatMenuOption? BuildOption(Pawn selected, Pawn clickedPawn, FloatMenuContext context)
    {
        KissProposal proposal = KissUtility.Propose(selected, clickedPawn);
        if (!proposal.Visible)
        {
            return null;
        }

        string label = "EPK.FloatMenu.Kiss".Translate(clickedPawn.Named("TARGET"));
        if (context.IsMultiselect)
        {
            // 多选时不写"谁去亲"，否则一屏"kiss 张三"分不清主体。
            label = selected.LabelShort + ": " + label;
        }

        if (!proposal.Allowed || proposal.Doer == null || proposal.Receiver == null)
        {
            // 原版灰项惯例：action 传 null 即禁用，原因写在括号里。
            return new FloatMenuOption(label + " (" + proposal.BlockedReason + ")", null,
                MenuOptionPriority.InitiateSocial, null, clickedPawn);
        }

        Pawn doer = proposal.Doer;
        Pawn receiver = proposal.Receiver;
        var option = new FloatMenuOption(label, delegate { KissUtility.BeginKiss(selected, clickedPawn); },
            MenuOptionPriority.InitiateSocial, null, clickedPawn)
        {
            tooltip = new TipSignal("EPK.FloatMenu.KissTooltip".Translate(clickedPawn.Named("TARGET")))
        };

        // 会打断对方当前工作的提示（爱心图标 + "会抢占"标记），复用原版装饰器。
        return FloatMenuUtility.DecoratePrioritizedTask(option, doer, receiver);
    }
}
