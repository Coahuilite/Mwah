using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 导演台的两步点选：底栏按钮进入，地图上先点发起方、再点对象，立刻派发。
///
/// 用原版 Find.Targeter，不自造输入：Targeter 自带跟随指针的目标高亮
/// （GenDraw.DrawTargetHighlight）、鼠标挂件图标、左键取点、右键/Esc 取消，
/// 并且在地图输入管线里于窗口之后处理，所以任何常驻窗口（Dev palette 之类）
/// 都不会像手搓 GameComponentOnGUI 那样把整个模式掐掉。
/// caster 传 null：ConfirmStillValid 只在 caster 非空时才要求"发起方被选中"，
/// 传 null 即跳过该检查 —— 这正是"没有选中任何小人也能进入选点"的官方入口。
/// 两次选点靠链式 BeginTargeting 串起来：BeginTargeting 会把 needsStopTargetingCall
/// 重置为 false，所以在第一次的 action 里发起第二次选点，不会被随后的 StopTargeting 抹掉
/// （原版 DestinationSelector 就是这个套路）。
///
/// 但链式起跳有代价：原版的事件顺序是 action → StopTargeting → actionWhenFinished，
/// 所以第一跳的 actionWhenFinished 会在第二跳已经开跑**之后**才执行。若两跳都直接
/// Active = false，第二跳的整个窗口里 Active 是假的 —— KissTicker 的"设置关闭即收点"
/// 和按钮的 Toggle 取消全都失灵。因此用 targeting 代号区分：每跳自增，cleanup 只在
/// "代号仍是自己"时清位；被链式后续抢走的 cleanup 静默让位。
/// </summary>
public static class KissDirector
{
    /// <summary>是否有我们发起的选点正在进行，供底栏按钮做开/关切换。</summary>
    public static bool Active;

    /// <summary>当前链式选点的代号；每跳自增，cleanup 凭代号识别"这次收尾属于自己的那一跳"。</summary>
    private static int targetingGeneration;

    public static void Toggle()
    {
        if (Active)
        {
            Stop();
            return;
        }
        BeginFirstPick();
    }

    /// <summary>结束选点（按钮再点一次、或设置里关掉导演台时调用）。</summary>
    public static void Stop()
    {
        if (Active)
        {
            Find.Targeter.StopTargeting();
        }
    }

    private static void BeginFirstPick()
    {
        int generation = ++targetingGeneration;
        MwahLog.Dev("director on (generation " + generation + ")");
        Find.Targeter.BeginTargeting(
            targetParams: PawnParams(),
            action: delegate (LocalTargetInfo ti)
            {
                if (ti.Thing is Pawn a)
                {
                    MwahLog.Dev("pick 1: " + a.LabelShort);
                    BeginSecondPick(a);
                }
            },
            highlightAction: null,
            targetValidator: delegate (LocalTargetInfo ti) { return ti.Thing is Pawn; },
            caster: null,
            actionWhenFinished: delegate { CleanupPick(generation); },
            mouseAttachment: null,
            playSoundOnAction: false,
            onGuiAction: delegate (LocalTargetInfo ti)
            {
                Widgets.MouseAttachedLabel("MWAH.Director.PickFirst".Translate());
            });
    }

    private static void BeginSecondPick(Pawn first)
    {
        int generation = ++targetingGeneration;
        Find.Targeter.BeginTargeting(
            targetParams: PawnParams(),
            action: delegate (LocalTargetInfo ti)
            {
                if (ti.Thing is Pawn b)
                {
                    MwahLog.Dev("pick 2: " + b.LabelShort);
                    Dispatch(first, b);
                }
            },
            highlightAction: null,
            targetValidator: delegate (LocalTargetInfo ti) { return ti.Thing is Pawn p && p != first; },
            caster: null,
            actionWhenFinished: delegate { CleanupPick(generation); },
            mouseAttachment: null,
            playSoundOnAction: false,
            onGuiAction: delegate (LocalTargetInfo ti)
            {
                Widgets.MouseAttachedLabel("MWAH.Director.PickSecond".Translate(first.Named("PAWN")));
            });
    }

    /// <summary>一跳的收尾。链式起跳后，第一跳的 cleanup 会晚于第二跳的 BeginTargeting 执行；
    /// 此时代号已换代 ⇒ 这次收尾不属于仍在进行的选点，静默让位。</summary>
    private static void CleanupPick(int generation)
    {
        if (generation != targetingGeneration)
        {
            return;
        }
        if (Active)
        {
            Active = false;
            MwahLog.Dev("director off (generation " + generation + ")");
        }
    }

    private static TargetingParameters PawnParams()
    {
        // 与原版右键菜单同一套取点：任何 spawn 的 pawn 都能选，敌人也不例外
        // （能不能亲交给 KissUtility.BeginDirected 的完整门禁判定，这里只管"是个 pawn"）。
        return TargetingParameters.ForPawns();
    }

    private static void Dispatch(Pawn a, Pawn b)
    {
        if (KissUtility.BeginDirected(a, b))
        {
            Messages.Message("MWAH.Director.Started".Translate(a.Named("PAWN"), b.Named("OTHER")),
                new LookTargets(a, b), MessageTypeDefOf.PositiveEvent, historical: false);
            return;
        }
        // 选点模式没有"灰按钮"，可行性只能在点完之后用消息告知。
        Messages.Message(KissUtility.DirectPreview(a, b) ?? "MWAH.Fail.Busy".Translate(),
            new LookTargets(a, b), MessageTypeDefOf.RejectInput, historical: false);
    }
}
