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
/// </summary>
public static class KissDirector
{
    /// <summary>是否有我们发起的选点正在进行，供底栏按钮做开/关切换。</summary>
    public static bool Active;

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
        Active = true;
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
            actionWhenFinished: delegate { Active = false; },
            mouseAttachment: null,
            playSoundOnAction: false,
            onGuiAction: delegate (LocalTargetInfo ti)
            {
                Widgets.MouseAttachedLabel("MWAH.Director.PickFirst".Translate());
            });
    }

    private static void BeginSecondPick(Pawn first)
    {
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
            actionWhenFinished: delegate { Active = false; },
            mouseAttachment: null,
            playSoundOnAction: false,
            onGuiAction: delegate (LocalTargetInfo ti)
            {
                Widgets.MouseAttachedLabel("MWAH.Director.PickSecond".Translate(first.Named("PAWN")));
            });
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
