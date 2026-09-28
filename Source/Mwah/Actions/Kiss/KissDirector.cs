using System;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 点选的原语层：面板（Dialog_KissDirector）与「快速发配」链式流程共用的三段 —— 单段拾取 BeginPick、
/// 链式 QuickChain、派发 Dispatch。底栏按钮本身只开合面板，不再直接持有输入模式。
///
/// 用原版 Find.Targeter，不自造输入：Targeter 自带跟随指针的目标高亮
/// （GenDraw.DrawTargetHighlight）、鼠标挂件图标、左键取点、右键/Esc 取消，
/// 并且在地图输入管线里于窗口之后处理 —— 面板开着也不掐模式：点面板归面板，点地图归取点。
/// caster 传 null：ConfirmStillValid 只在 caster 非空时才要求"发起方被选中"，
/// 传 null 即跳过该检查 —— 这正是"没有选中任何小人也能进入选点"的官方入口。
/// 链式靠连续 BeginTargeting 串起来：BeginTargeting 会把 needsStopTargetingCall 重置为 false，
/// 所以在第一跳的 action 里发起第二跳，不会被随后的 StopTargeting 抹掉（原版 DestinationSelector 同套路）。
///
/// 链式起跳有代价：原版事件顺序是 action → StopTargeting → actionWhenFinished，第一跳的
/// actionWhenFinished 会在第二跳已开跑**之后**才执行。若两跳都直接 Active=false，第二跳整个
/// 窗口里 Active 是假的 —— KissTicker 的"设置关闭即收点"和面板的高亮复位全都失灵。因此每跳
/// 自增代号，cleanup 只在"代号仍是自己"时清位；被链式后续抢走的 cleanup 静默让位。
/// </summary>
public static class KissDirector
{
    /// <summary>是否有我们发起的选点正在进行。面板拿它复位高亮，KissTicker 拿它做"设置关了收点"。</summary>
    public static bool Active;

    /// <summary>当前链式选点的代号；每跳自增，cleanup 凭代号识别"这次收尾属于自己的那一跳"。</summary>
    private static int targetingGeneration;

    /// <summary>结束选点（面板关窗时还挂在拾取里、或设置里关掉导演台时调用）。</summary>
    public static void Stop()
    {
        if (Active)
        {
            Find.Targeter.StopTargeting();
        }
    }

    /// <summary>
    /// 一段地图拾取（只收 pawn）：头像框左槽与快速发配第一跳用。
    /// <paramref name="prompt"/> 是已经算好参数的鼠标挂件文案。
    /// </summary>
    public static void BeginPick(Action<Pawn> onPicked, Func<Pawn, bool>? extraAccept, string prompt)
    {
        BeginPickThing(
            t => { if (t is Pawn p) { onPicked(p); } },
            extraAccept == null ? null : new Func<Thing, bool>(th => th is Pawn pp && extraAccept(pp)),
            prompt,
            pawnsOnly: true);
    }

    /// <summary>
    /// 一段地图拾取（pawn 或墙）：右槽与快速发配第二跳用。墙只在"允许亲吻墙壁"开着时可选，
    /// 且必须不在迷雾里 —— 未知的部分不能提前选中（维护者裁定）。能不能亲仍不在这里判，
    /// 交给派发时的完整门禁。
    /// </summary>
    public static void BeginPickThing(Action<Thing> onPicked, Func<Thing, bool>? extraAccept, string prompt)
    {
        BeginPickThing(onPicked, extraAccept, prompt, pawnsOnly: false);
    }

    private static void BeginPickThing(Action<Thing> onPicked, Func<Thing, bool>? extraAccept, string prompt, bool pawnsOnly)
    {
        int generation = ++targetingGeneration;
        Active = true;
        MwahLog.Dev("pick start (generation " + generation + ")");
        Find.Targeter.BeginTargeting(
            targetParams: pawnsOnly ? TargetingParameters.ForPawns() : PawnAndThingParams,
            action: delegate (LocalTargetInfo ti)
            {
                if (ti.Thing is Thing picked && (extraAccept == null || extraAccept(picked)))
                {
                    MwahLog.Dev("pick ok (generation " + generation + "): " + picked.LabelShort);
                    onPicked(picked);
                }
            },
            highlightAction: null,
            targetValidator: delegate (LocalTargetInfo ti)
            {
                return ti.Thing is Thing t && (extraAccept == null || extraAccept(t));
            },
            caster: null,
            actionWhenFinished: delegate { CleanupPick(generation); },
            mouseAttachment: null,
            playSoundOnAction: false,
            onGuiAction: delegate (LocalTargetInfo ti)
            {
                Widgets.MouseAttachedLabel(prompt);
            });
    }

    /// <summary>pawn 全放行（能不能亲由派发判定）；非 pawn 交给 addon 注册表：
    /// 某个开着的 addon 认领、且不在迷雾里，才可选。</summary>
    private static TargetingParameters PawnAndThingParams => new()
    {
        canTargetPawns = true,
        canTargetBuildings = true,
        validator = delegate (TargetInfo ti)
        {
            return ti.Thing is Pawn || (ti.Thing != null && KissThingAddons.AnyPickable(ti.Thing));
        },
    };

    /// <summary>
    /// 快速发配：老版导演台的两段链 —— 第一点发起方、第二点对象（pawn 或墙），选完立即派发。
    /// <paramref name="onBoth"/> 只是给面板回显槽位用的钩子，派发不经过它。
    /// </summary>
    public static void QuickChain(Action<Pawn, Thing>? onBoth)
    {
        BeginPick(first =>
        {
            BeginPickThing(
                second =>
                {
                    onBoth?.Invoke(first, second);
                    Dispatch(first, second);
                },
                t => t != first,
                "MWAH.Director.PickSecond".Translate(first.Named("PAWN")));
        }, null, "MWAH.Director.PickFirst".Translate());
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
            MwahLog.Dev("pick end (generation " + generation + ")");
        }
    }

    // 取点参数只管"是不是个可选的东西"，能不能亲交给派发时的完整门禁：
    // pawn 走 core（KissUtility.BeginDirected），非 pawn 走 addon 注册表。

    public static void Dispatch(Pawn a, Thing b)
    {
        if (b is Pawn partner)
        {
            // 一次判定拿到结果与原因（早先失败时要再跑一遍 Propose，连 A* 寻路都白烧）。
            // 导演台是玩家亲手点的名：战斗闸放行（超凡智能想让谁亲嘴，谁就得亲嘴）。
            KissProposal proposal = KissUtility.BeginDirected(a, partner, playerIssued: true);
            if (proposal.Allowed)
            {
                Messages.Message("MWAH.Director.Started".Translate(a.Named("PAWN"), partner.Named("OTHER")),
                    new LookTargets(a, partner), MessageTypeDefOf.PositiveEvent, historical: false);
                return;
            }
            Messages.Message(proposal.BlockedReason ?? "MWAH.Fail.Busy".Translate(),
                new LookTargets(a, partner), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        KissThingAddon? addon = KissThingAddons.For(b);
        if (addon is not { Active: true })
        {
            return; // 没有 addon 认领、或认领它的功能刚被关掉：静默失效，不弹"为什么不行"
        }
        ThingKissProposal thingProposal = addon.BeginDirected(a, b);
        if (!thingProposal.Visible)
        {
            return;
        }
        if (thingProposal.Allowed)
        {
            Messages.Message("MWAH.Director.Started".Translate(a.Named("PAWN"), b.Named("OTHER")),
                new LookTargets(a, b), MessageTypeDefOf.PositiveEvent, historical: false);
            return;
        }
        Messages.Message(thingProposal.BlockedReason ?? "MWAH.Fail.Busy".Translate(),
            new LookTargets(a, b), MessageTypeDefOf.RejectInput, historical: false);
    }
}
