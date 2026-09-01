using RimWorld;
using Verse;
using Verse.AI;

namespace EveryPawnKissEachOther;

/// <summary>
/// 一次"要不要在右键菜单里出现"的完整判定结果。
/// Visible=false 表示连灰项都不给（例如敌对、物种被排除、总开关关闭），
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

    public static KissProposal Hidden => default;

    public static KissProposal Allow(Pawn doer, Pawn receiver) => new(true, true, doer, receiver, null);

    public static KissProposal Blocked(string reason) => new(true, false, null, null, reason);
}

/// <summary>
/// 亲吻的判定与发起：只决定"谁走过去"和"现在能不能亲"，
/// 表演（贴近、相对、爱心）与结算在 <see cref="JobDriver_Kiss"/>。
/// </summary>
public static class KissUtility
{
    /// <summary>
    /// 谁当发起方：被选中的那位优先主动；
    /// 选中的动不了（缺 Moving 容量、倒地、doesntMove、无 pather）而对方能动，就让对方过来亲它。
    /// 两人已经贴在一起时不做换位，保持"谁点谁主动"。
    /// </summary>
    public static KissProposal Propose(Pawn selected, Pawn target)
    {
        if (selected == null || target == null || selected == target)
        {
            return KissProposal.Hidden;
        }

        AcceptanceReport eligibility = CheckEligibility(selected, target);
        if (!eligibility.Accepted)
        {
            return KissProposal.Hidden;
        }

        Pawn doer = selected;
        Pawn receiver = target;
        if (!selected.AdjacentTo8WayOrInside(target) && !CanMoveNow(selected))
        {
            if (!CanMoveNow(target))
            {
                return KissProposal.Blocked("EPK.Fail.ImmobilePair".Translate());
            }
            doer = target;
            receiver = selected;
        }

        AcceptanceReport availability = CheckAvailability(doer, receiver);
        return availability.Accepted
            ? KissProposal.Allow(doer, receiver)
            : KissProposal.Blocked(availability.Reason);
    }

    /// <summary>静态资格：与"此刻忙不忙、够不够得到"无关的过滤。</summary>
    private static AcceptanceReport CheckEligibility(Pawn a, Pawn b)
    {
        EPKSettings? settings = EPKMod.Settings;
        if (settings == null || !settings.Enabled)
        {
            return new AcceptanceReport("EPK.Fail.Disabled".Translate());
        }
        if (a.Dead || b.Dead || !a.Spawned || !b.Spawned)
        {
            return new AcceptanceReport("EPK.Fail.Self".Translate());
        }
        if (!settings.NonHumanlikeAllowed && (!a.RaceProps.Humanlike || !b.RaceProps.Humanlike))
        {
            return new AcceptanceReport("EPK.Fail.NonHumanlike".Translate());
        }
        if (!settings.HostileAllowed && a.HostileTo(b))
        {
            return new AcceptanceReport("EPK.Fail.Hostile".Translate());
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>即时可用性：正在亲、冷却中、够不到。</summary>
    private static AcceptanceReport CheckAvailability(Pawn doer, Pawn receiver)
    {
        if (IsKissing(doer) || IsKissing(receiver))
        {
            return new AcceptanceReport("EPK.Fail.Busy".Translate());
        }

        int pawnRemaining = KissCooldown.PawnRemaining(doer);
        if (pawnRemaining > 0)
        {
            return new AcceptanceReport(EPKStrings.Get("EPK.Fail.CooldownPawn",
                doer.Named("PAWN"), EPKTime.FormatTicks(pawnRemaining).Named("TIME")));
        }

        int pairRemaining = KissCooldown.PairRemaining(doer, receiver);
        if (pairRemaining > 0)
        {
            return new AcceptanceReport(EPKStrings.Get("EPK.Fail.CooldownPair",
                doer.Named("PAWN"), receiver.Named("OTHER"), EPKTime.FormatTicks(pairRemaining).Named("TIME")));
        }

        if (!doer.CanReach(receiver.Position, PathEndMode.Touch, Danger.Deadly))
        {
            return new AcceptanceReport(EPKStrings.Get("EPK.Fail.CannotReach", receiver.Named("TARGET")));
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>点击菜单项后重新判定一次再发起：菜单可能已经开着好几秒，状态会变。</summary>
    public static void BeginKiss(Pawn selected, Pawn target)
    {
        KissProposal proposal = Propose(selected, target);
        if (!proposal.Allowed || proposal.Doer == null || proposal.Receiver == null)
        {
            Messages.Message(proposal.BlockedReason ?? "EPK.Fail.Busy".Translate(),
                new LookTargets(selected, target), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }

        EPKSettings settings = EPKMod.Settings;
        // 冷却在下令瞬间消耗，避免同一 tick 反复排队刷爱心。
        KissCooldown.Mark(proposal.Doer, proposal.Receiver, settings.PawnCooldown, settings.PairCooldown);

        var job = JobMaker.MakeJob(EPK_JobDefOf.EPK_Kiss, proposal.Receiver);
        proposal.Doer.jobs.StartJob(job, JobCondition.InterruptForced);
    }

    public static bool IsKissing(Pawn pawn) => pawn.CurJobDef == EPK_JobDefOf.EPK_Kiss;

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

    /// <summary>结束后回被下令时站的位置。只是礼貌请求：紧急需求与排班仍会插队。</summary>
    public static void RequestReturnHome(Pawn pawn, IntVec3 homeCell)
    {
        if (!EPKMod.Settings.ReturnsHome || pawn == null || !homeCell.IsValid || pawn.Position == homeCell)
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
