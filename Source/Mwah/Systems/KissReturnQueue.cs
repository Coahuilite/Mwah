using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 亲吻结束后的"回原位"队列：表演收尾时只登记，发放交给下一个 tick 的 <see cref="KissTicker"/>。
///
/// 为什么要排队，而不是在 toil 的 finish action 里直接下令：finish action 跑在 job 收尾的调用栈里，
/// 而 <c>TryTakeOrderedJob</c> 在 pawn 空闲时会**同步**起新 job，新 job 的 StartJob 又会回头结束
/// 正在收尾的旧 job ⇒ 旧 job 的 finish action 再发一次 ⇒ 同一 tick 内无限递归。
/// 2026-09-04 的实测现场就是：日志里同一条 kiss end 刷到 Unity 的 99 条折叠上限、
/// <c>Mwah-trace.log</c> 停行、主线程卡死数秒、进程没有"未响应"直接消失。
/// 隔一个 tick 发，栈早就退干净了，递归链就断了。
///
/// 队列是 static，所以寿命是进程而不是游戏局：跨局既留住旧局 pawn 的引用，又会在新一局的
/// 第一个 tick 对已经没有地图的对象发 Goto ⇒ 必须 <see cref="Reset"/>（由 KissTicker 的
/// (Game) 构造器调用，那是"每局必跑"的钩子）。
/// </summary>
public static class KissReturnQueue
{
    private static readonly List<(Pawn pawn, IntVec3 cell)> Pending = new();

    /// <summary>亲吻收尾时登记。这里只做最便宜的准入判断，真发放在 <see cref="Drain"/>。</summary>
    public static void Enqueue(Pawn pawn, IntVec3 homeCell)
    {
        if (MwahMod.Settings.ReturnsHome && pawn != null && homeCell.IsValid && pawn.Position != homeCell)
        {
            Pending.Add((pawn, homeCell));
        }
    }

    /// <summary>
    /// 发放上一 tick 攒下的请求。先把队列整个换出来再遍历：<c>TryTakeOrderedJob</c> 可能当场
    /// 结束别人的 job，被结束那方的 finish action 又会往队列里追加 —— 拿着活列表边遍历边加会炸。
    /// </summary>
    public static void Drain()
    {
        if (Pending.Count == 0)
        {
            return;
        }
        List<(Pawn pawn, IntVec3 cell)> batch = new(Pending);
        Pending.Clear();
        for (int i = 0; i < batch.Count; i++)
        {
            Request(batch[i].pawn, batch[i].cell);
        }
    }

    /// <summary>每局清空（见类注释里的跨局泄漏）。</summary>
    public static void Reset()
    {
        Pending.Clear();
    }

    /// <summary>
    /// 回到被下令时站的那一格。只是礼貌请求：紧急需求与排班仍会插队。
    /// 判据看着与 <see cref="Enqueue"/> 重复，但不是冗余：登记时"回原位"开关可能是开着的，
    /// 一个 tick 后玩家可能已经关掉它；这一遍还要看"这会儿还站不站得起来"。
    /// </summary>
    private static void Request(Pawn pawn, IntVec3 homeCell)
    {
        if (!MwahMod.Settings.ReturnsHome || !homeCell.IsValid || pawn.Position == homeCell)
        {
            return;
        }
        if (!KissUtility.CanMoveNow(pawn) || pawn.jobs == null)
        {
            return;
        }
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Goto, homeCell), JobTag.Misc);
    }
}
