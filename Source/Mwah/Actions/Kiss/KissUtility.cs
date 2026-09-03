using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 一次"要不要在右键菜单里出现"的完整判定结果。
/// Visible=false 表示连灰项都不给（结构层不可能：总开关关闭、对象已不在、
/// 有一方没有心情系统且设置未放行、敌对且设置未放行），
/// 避免每次右键殖民地都糊一屏灰色亲吻。
/// </summary>
public readonly struct KissProposal
{
    public readonly bool Visible;
    public readonly bool Allowed;
    public readonly Pawn? Doer;
    public readonly Pawn? Receiver;
    public readonly string? BlockedReason;

    private KissProposal(bool visible, bool allowed, Pawn? doer, Pawn? receiver, string? blockedReason)
    {
        Visible = visible;
        Allowed = allowed;
        Doer = doer;
        Receiver = receiver;
        BlockedReason = blockedReason;
    }

    /// <summary>
    /// 菜单里不出现，但仍带上原因：点下已经开了几秒的菜单项时，
    /// 状态可能变了，那时这条原因就是给玩家的解释。
    /// </summary>
    public static KissProposal Hidden(string? reason = null) => new(false, false, null, null, reason);

    public static KissProposal Allow(Pawn doer, Pawn receiver) => new(true, true, doer, receiver, null);

    public static KissProposal Blocked(string reason) => new(true, false, null, null, reason);
}

/// <summary>
/// 亲吻的发起：判定顺序是 结构层 → 参与层 → 主动层（谁去亲）→ 即时可用性。
/// 前三层的定义与源码依据都在 <see cref="KissBoundary"/>；表演与结算在 <see cref="JobDriver_Kiss"/>。
/// </summary>
public static class KissUtility
{
    /// <summary>
    /// 谁当发起方：被选中的那位优先主动；它主动不了（动不了 / 没嘴 / 生命阶段不允许）
    /// 而对方能，就让对方过来亲它。总开关与"已经不在了" → 连灰项都不给；门禁越界与即时状态 → 灰项带原因。
    /// </summary>
    public static KissProposal Propose(Pawn selected, Pawn target)
    {
        if (selected == null || target == null || selected == target)
        {
            return KissProposal.Hidden("MWAH.Fail.Self".Translate());
        }
        AcceptanceReport structure = KissBoundary.CheckStructure(selected, target);
        if (!structure.Accepted)
        {
            return KissProposal.Hidden(structure.Reason);
        }

        AcceptanceReport scope = KissBoundary.CheckScope(selected, target);
        if (!scope.Accepted)
        {
            return KissProposal.Blocked(scope.Reason);
        }

        AcceptanceReport selectedState = KissBoundary.CanParticipate(selected);
        if (!selectedState.Accepted)
        {
            return KissProposal.Blocked(selectedState.Reason);
        }
        AcceptanceReport targetState = KissBoundary.CanParticipate(target);
        if (!targetState.Accepted)
        {
            return KissProposal.Blocked(targetState.Reason);
        }

        bool selectedImmobile;
        AcceptanceReport initiator = KissBoundary.CanInitiate(selected, out selectedImmobile);
        Pawn doer = selected;
        Pawn receiver = target;
        if (!initiator.Accepted)
        {
            bool targetImmobile;
            AcceptanceReport other = KissBoundary.CanInitiate(target, out targetImmobile);
            if (!other.Accepted)
            {
                return KissProposal.Blocked(selectedImmobile && targetImmobile
                    ? "MWAH.Fail.ImmobilePair".Translate()
                    : (!selectedImmobile ? initiator.Reason : other.Reason));
            }
            doer = target;
            receiver = selected;
        }

        AcceptanceReport availability = CheckAvailability(doer, receiver);
        return availability.Accepted
            ? KissProposal.Allow(doer, receiver)
            : KissProposal.Blocked(availability.Reason);
    }

    /// <summary>即时可用性：正在亲、冷却中、够不到。</summary>
    private static AcceptanceReport CheckAvailability(Pawn doer, Pawn receiver)
    {
        if (IsKissing(doer) || IsKissing(receiver))
        {
            return new AcceptanceReport("MWAH.Fail.Busy".Translate());
        }

        int pawnRemaining = KissCooldown.PawnRemaining(doer);
        if (pawnRemaining > 0)
        {
            return new AcceptanceReport(MwahStrings.Get("MWAH.Fail.CooldownPawn",
                doer.Named("PAWN"), MwahTime.FormatTicks(pawnRemaining).Named("TIME")));
        }

        int pairRemaining = KissCooldown.PairRemaining(doer, receiver);
        if (pairRemaining > 0)
        {
            return new AcceptanceReport(MwahStrings.Get("MWAH.Fail.CooldownPair",
                doer.Named("PAWN"), receiver.Named("OTHER"), MwahTime.FormatTicks(pairRemaining).Named("TIME")));
        }

        if (!doer.CanReach(receiver.Position, PathEndMode.Touch, Danger.Deadly))
        {
            return new AcceptanceReport(MwahStrings.Get("MWAH.Fail.CannotReach", receiver.Named("TARGET")));
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>点击菜单项后重新判定一次再发起：菜单可能已经开着好几秒，状态会变。</summary>
    public static void BeginKiss(Pawn selected, Pawn target)
    {
        KissProposal proposal = Propose(selected, target);
        if (!proposal.Allowed || proposal.Doer == null || proposal.Receiver == null)
        {
            Messages.Message(proposal.BlockedReason ?? "MWAH.Fail.Stale".Translate(),
                new LookTargets(selected, target), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        Start(proposal, forced: false);
    }

    /// <summary>
    /// 代发亲吻：由 <see cref="KissPick"/> 两步点选点名，或由 <see cref="KissAmbient"/> 定时促成。
    /// 与右键那条的差别只有两点：不弹拒绝提示（没人点东西），以及 job 打 playerForced ——
    /// 否则 pawn 自己的 think tree 会在下一个 override 检查点把它拽回去，变成"起步即取消"。
    /// 走这条路的前提正是"原版不给玩家下令权"，所以它天然绕开 CanTakeOrder，判定仍全复用 Propose。
    /// </summary>
    public static bool BeginDirected(Pawn doer, Pawn receiver)
    {
        KissProposal proposal = Propose(doer, receiver);
        if (!proposal.Allowed || proposal.Doer == null || proposal.Receiver == null)
        {
            return false;
        }
        Start(proposal, forced: true);
        return true;
    }

    /// <summary>
    /// 导演台用的可行性预览：不发起，只返回挡住它的原因文本，可行时返回 null。
    /// 界面要靠它把按钮置灰并写明理由，所以不能只给 bool。
    /// </summary>
    public static string? DirectPreview(Pawn a, Pawn b)
    {
        KissProposal proposal = Propose(a, b);
        return proposal.Allowed ? null : proposal.BlockedReason;
    }

    /// <summary>
    /// 便宜的成对预筛：只跑静态判定与状态判定，不做寻路。
    /// 给 <see cref="KissAmbient"/> 在一堆候选人里筛目标用；真正发起前仍会走完整的 <see cref="Propose"/>。
    /// </summary>
    public static bool PairLooksKissable(Pawn a, Pawn b)
    {
        return KissBoundary.CheckStructure(a, b).Accepted
            && KissBoundary.CanParticipate(a).Accepted
            && KissBoundary.CanParticipate(b).Accepted;
    }

    /// <summary>共用的落地动作：消耗冷却 + 起 job。冷却在发起瞬间记，避免同一 tick 反复排队刷爱心。</summary>
    private static void Start(KissProposal proposal, bool forced)
    {
        MwahSettings settings = MwahMod.Settings;
        KissCooldown.Mark(proposal.Doer!, proposal.Receiver!, settings.PawnCooldown, settings.PairCooldown);

        var job = JobMaker.MakeJob(MWAH_JobDefOf.MWAH_Kiss, proposal.Receiver);
        if (forced)
        {
            job.playerForced = true;
        }
        proposal.Doer!.jobs.StartJob(job, JobCondition.InterruptForced);
    }

    public static bool IsKissing(Pawn pawn) => pawn.CurJobDef == MWAH_JobDefOf.MWAH_Kiss;

    /// <summary>
    /// 1.6 已无 Pawn.CanMove，移动能力唯一可靠读法是 Moving 容量 + pather 存在；
    /// doesntMove 的种族（树精一类）永远算"动不了"。
    /// </summary>
    public static bool CanMoveNow(Pawn pawn)
    {
        if (pawn == null || pawn.Downed || pawn.RaceProps.doesntMove || pawn.pather == null)
        {
            return false;
        }
        return pawn.health?.capacities != null && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Moving);
    }

    /// <summary>
    /// 待发放的"回原位"请求。
    /// 为什么排队而不是在 toil 的 finish action 里直接发：finish action 跑在 job 收尾的调用栈里，
    /// 而 TryTakeOrderedJob 在 pawn 空闲时会**同步**起新 job，新 job 的 StartJob 又会回头结束
    /// 正在收尾的旧 job ⇒ 旧 job 的 finish action 再发一次 ⇒ 同一 tick 内无限递归。
    /// 实测表现就是日志里同一条 kiss end 刷到折叠上限、主线程卡死数秒、进程直接消失。
    /// 排到下一个 tick 发，递归链就断了。
    /// </summary>
    private static readonly List<(Pawn pawn, IntVec3 cell)> PendingReturns = new();

    public static void QueueReturnHome(Pawn pawn, IntVec3 homeCell)
    {
        if (MwahMod.Settings.ReturnsHome && pawn != null && homeCell.IsValid && pawn.Position != homeCell)
        {
            PendingReturns.Add((pawn, homeCell));
        }
    }

    /// <summary>下一 tick 由 GameComponent 调用：此时不在任何 job 的收尾栈里，起 job 是安全的。</summary>
    public static void DrainReturns()
    {
        if (PendingReturns.Count == 0)
        {
            return;
        }
        List<(Pawn pawn, IntVec3 cell)> batch = new(PendingReturns);
        PendingReturns.Clear();
        for (int i = 0; i < batch.Count; i++)
        {
            RequestReturnHome(batch[i].pawn, batch[i].cell);
        }
    }

    /// <summary>结束后回被下令时站的位置。只是礼貌请求：紧急需求与排班仍会插队。</summary>
    public static void RequestReturnHome(Pawn pawn, IntVec3 homeCell)
    {
        if (!MwahMod.Settings.ReturnsHome || pawn == null || !homeCell.IsValid || pawn.Position == homeCell)
        {
            return;
        }
        if (!CanMoveNow(pawn) || pawn.jobs == null)
        {
            return;
        }
        pawn.jobs.TryTakeOrderedJob(JobMaker.MakeJob(JobDefOf.Goto, homeCell), JobTag.Misc);
    }
}
