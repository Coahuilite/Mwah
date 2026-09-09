using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 右键菜单入口。FloatMenuMakerMap.Init() 用 AllSubclassesNonAbstract() 反射发现所有
/// FloatMenuOptionProvider 子类，所以这个类不需要 Def、不需要 XML、也不需要 Harmony 补丁。
/// 取向仍是"能下令就能亲"，但边界收敛后写清了前提：发起方得是原版允许下令的 pawn
/// （CanTakeOrder + 未倒地 + 无 Lord 限制，这三条在 provider 之前就把关，绕不过），
/// 并且双方都满足 <see cref="KissBoundary"/> 的参与层。
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
    /// 故意不调用 base：base 开头用 MutantDef.whitelistedFloatMenuProviders（原版三个 mutant 都是空表）
    /// 把所有 mutant 一刀切屏蔽。本模组改由参与层逐条判：只有
    /// <c>incapableOfSocialInteractions</c> 的（蹒跚者/尸鬼/唤醒尸体）被挡，别的照旧放行。
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
        KissProposal proposal = KissUtility.Propose(selected, clickedPawn, allowRoleSwap: true);
        if (!proposal.Visible)
        {
            return null;
        }

        string label = "MWAH.FloatMenu.Kiss".Translate(clickedPawn.Named("TARGET"));
        // 有一方没有心情系统时写进标签：亲是亲了，收益为零 —— 不让人事后猜为什么没弹心情。
        if (KissBoundary.AnyMoodless(selected, clickedPawn))
        {
            label += " " + "MWAH.FloatMenu.KissNoMood".Translate();
        }
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
            tooltip = new TipSignal("MWAH.FloatMenu.KissTooltip".Translate(clickedPawn.Named("TARGET")))
        };

        // 会打断对方当前工作的提示（爱心图标 + "会抢占"标记），复用原版装饰器。
        return FloatMenuUtility.DecoratePrioritizedTask(option, doer, receiver);
    }
}
