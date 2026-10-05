using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 一次"这个吻行不行、谁亲谁、不行是因为什么"的完整判定结果。
/// Visible=false 表示连灰项都不给（结构层就不成立：总开关关着、人已经不在了），
/// 免得每次右键殖民地都糊一屏灰色的亲吻；Allowed=false 才给灰项，括号里写真实原因。
/// 三层判定的顺序与依据见 <see cref="KissBoundary"/>。
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

    /// <summary>
    /// Allowed 为真时双方必然都在（Allow 是唯一置真入口）。把这条不变式收在这里，
    /// 调用点就只写一次判断，不必每处再补两个 `== null`。
    /// </summary>
    public bool TryGetPair(out Pawn doer, out Pawn receiver)
    {
        doer = Doer!;
        receiver = Receiver!;
        return Allowed && Doer != null && Receiver != null;
    }
}

/// <summary>
/// 亲吻的发起：判定顺序是 结构层 → 门禁层 → 参与层 → 主动层（谁去亲）→ 即时可用性。
/// 前四层的定义与源码依据都在 <see cref="KissBoundary"/>；表演与结算在 <see cref="JobDriver_Kiss"/>。
/// </summary>
public static class KissUtility
{
    /// <summary>
    /// 谁当发起方：被选中的那位优先主动；它主动不了（动不了 / 没嘴 / 生命阶段不允许）
    /// 而对方能，就让对方过来亲它。总开关与"已经不在了" → 连灰项都不给；门禁越界与即时状态 → 灰项带原因。
    /// <paramref name="allowRoleSwap"/> 只对右键路径为真：那是 About.xml 明文承诺的"选中方走不动
    /// 就换对方走过来"。代发路径（导演台点名、自主派发）必须传 false —— 导演台的消息要按点选
    /// 顺序说话；自主派发更不能借 swap 把玩家侧的小人拉起来当发起方。
    /// <paramref name="allowCombatInterrupt"/> 为假表示这一路不许打断交战中的单位（自主派发）；
    /// 玩家亲自下令的两条路（右键、导演台）恒为真 —— 超凡智能想让谁亲嘴，谁就得亲嘴。
    /// </summary>
    public static KissProposal Propose(Pawn selected, Pawn target, bool allowRoleSwap, bool allowCombatInterrupt)
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
            if (!allowRoleSwap)
            {
                return KissProposal.Blocked(initiator.Reason);
            }
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

        AcceptanceReport availability = CheckAvailability(doer, receiver, allowCombatInterrupt);
        return availability.Accepted
            ? KissProposal.Allow(doer, receiver)
            : KissProposal.Blocked(availability.Reason);
    }

    /// <summary>即时可用性：正在亲、（自主派发时）正打着、冷却中、够不到。</summary>
    private static AcceptanceReport CheckAvailability(Pawn doer, Pawn receiver, bool allowCombatInterrupt)
    {
        if (IsKissing(doer) || IsKissing(receiver))
        {
            return new AcceptanceReport("MWAH.Fail.Busy".Translate());
        }
        // 战斗闸只拦自主派发：正在交战的一方被系统硬拽去亲嘴，等于给 AI 发了个免费点穴器。
        // 它排在"无视亲吻冷却"的豁免**之前** —— 那个开关豁免的是冷却两条，不是战斗闸。
        if (!allowCombatInterrupt)
        {
            Pawn? fighting = KissBoundary.InCombatNow(doer) ? doer
                : KissBoundary.InCombatNow(receiver) ? receiver : null;
            if (fighting != null)
            {
                return new AcceptanceReport("MWAH.Fail.InCombat".Translate(fighting.Named("PAWN")));
            }
        }
        if (MwahMod.Settings.CooldownsIgnored)
        {
            // 超凡智能的大手：冷却两条都跳过。"正在亲"不跳 —— 镜像 job 不允许三方。
            return AcceptanceReport.WasAccepted;
        }

        int pawnRemaining = KissCooldown.PawnRemaining(doer);
        if (pawnRemaining > 0)
        {
            return new AcceptanceReport("MWAH.Fail.CooldownPawn".Translate(
                doer.Named("PAWN"), MwahTime.FormatTicks(pawnRemaining).Named("TIME")));
        }

        int pairRemaining = KissCooldown.PairRemaining(doer, receiver);
        if (pairRemaining > 0)
        {
            return new AcceptanceReport("MWAH.Fail.CooldownPair".Translate(
                doer.Named("PAWN"), receiver.Named("OTHER"), MwahTime.FormatTicks(pairRemaining).Named("TIME")));
        }

        if (!doer.CanReach(receiver.Position, PathEndMode.Touch, Danger.Deadly))
        {
            return new AcceptanceReport("MWAH.Fail.CannotReach".Translate(receiver.Named("TARGET")));
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>点击菜单项后重新判定一次再发起：菜单可能已经开着好几秒，状态会变。</summary>
    public static void BeginKiss(Pawn selected, Pawn target)
    {
        KissProposal proposal = Propose(selected, target, allowRoleSwap: true, allowCombatInterrupt: true);
        if (!proposal.TryGetPair(out Pawn doer, out Pawn receiver))
        {
            Messages.Message(proposal.BlockedReason ?? "MWAH.Fail.Stale".Translate(),
                new LookTargets(selected, target), MessageTypeDefOf.RejectInput, historical: false);
            return;
        }
        Start(doer, receiver, forced: false);
    }

    /// <summary>
    /// 代发亲吻：由 <see cref="KissDirector"/> 两步点选点名，或由 <see cref="KissAmbient"/> 定时促成。
    /// 与右键那条的差别只有两点：不弹拒绝提示（没人点东西），以及 job 打 playerForced ——
    /// 否则 pawn 自己的 think tree 会在下一个 override 检查点把它拽回去，变成"起步即取消"。
    /// 走这条路的前提正是"原版不给玩家下令权"，所以它天然绕开 CanTakeOrder，判定仍全复用 Propose，
    /// 但**不做角色互换**：点谁就是谁去亲（<see cref="KissDirector"/> 的消息按点选顺序说话），
    /// 自主派发也绝不把玩家侧小人拉起来当发起方。<paramref name="playerIssued"/> 区分两种代发：
    /// 导演台是超凡智能亲手点的名，战斗闸放行；自主派发是系统在撮合，交战中的单位不许被它拽走。
    /// </summary>
    /// <returns>这次判定的完整结果：Allowed 为真时 job 已经起来，为假时 BlockedReason 就是给玩家的理由。
    /// 结果必须交回调用方 —— 早先这里只回 null，导演台想知道"为什么不行"只能把 Propose 连着
    /// 寻路再跑一遍（<c>CanReach</c> 是 A*，白烧一次），还可能两次结论不一致。</returns>
    public static KissProposal BeginDirected(Pawn first, Pawn second, bool playerIssued)
    {
        KissProposal proposal = Propose(first, second, allowRoleSwap: false, allowCombatInterrupt: playerIssued);
        if (proposal.TryGetPair(out Pawn doer, out Pawn receiver))
        {
            Start(doer, receiver, forced: true);
        }
        return proposal;
    }

    /// <summary>
    /// 便宜的成对预筛：只跑静态判定与状态判定，不做寻路。
    /// 给 <see cref="KissAmbient"/> 在一堆候选人里筛目标用；真正发起前仍会走完整的 <see cref="Propose"/>。
    /// 战斗闸也在这里过一遍：省的是随后那次完整 Propose（含 A*）。
    /// </summary>
    public static bool PairLooksKissable(Pawn a, Pawn b)
    {
        return KissBoundary.CheckStructure(a, b).Accepted
            && KissBoundary.CheckScope(a, b).Accepted
            && KissBoundary.CanParticipate(a).Accepted
            && KissBoundary.CanParticipate(b).Accepted
            && !KissBoundary.InCombatNow(a) && !KissBoundary.InCombatNow(b);
    }

    /// <summary>
    /// 共用的落地动作：**双方 job 同一帧起来**。被动方先起 —— 它的 job 起不来就整个取消，
    /// 发起方不该一个人跑去亲空气；发起方的 job 起不来（同 tick 被抢）就把刚起的被动 job
    /// 干净收掉。接近段停火的全部来源就是"被亲的一方从第 0 tick 起也在 job 里"。
    /// 台位在这里同步定好（<see cref="KissStage"/>，成本与旧写法定台 toil 的第一步同 tick 同量）。
    /// 冷却与"亲完回哪儿"都不在这里：冷却由发起方的 ToilBegin 记（预订失败时 job 没落地，
    /// 发起瞬间记账等于空罚一轮）；回程走 job 内收尾段，兜底在 kiss 段 finish action 排队。
    /// </summary>
    private static void Start(Pawn doer, Pawn receiver, bool forced)
    {
        KissStage.TryFind(doer, receiver, out IntVec3 mine, out IntVec3 theirs);
        MwahLog.Dev("stage: " + doer.LabelShort + "=" + (mine.IsValid ? mine.ToString() : "touch")
            + " " + receiver.LabelShort + "=" + (theirs.IsValid ? theirs.ToString() : "touch")
            + (forced ? " forced" : ""));
        var doerJob = JobMaker.MakeJob(MWAH_JobDefOf.MWAH_Kiss, receiver);
        var passiveJob = JobMaker.MakeJob(MWAH_JobDefOf.MWAH_Kiss, doer);
        if (mine.IsValid)
        {
            doerJob.SetTarget(TargetIndex.B, new LocalTargetInfo(mine));
        }
        if (theirs.IsValid)
        {
            passiveJob.SetTarget(TargetIndex.B, new LocalTargetInfo(theirs));
        }
        // C 端永远是发起方：两个镜像 job 靠这一个记号认角色，不再"看 B 填没填"猜。
        doerJob.SetTarget(TargetIndex.C, new LocalTargetInfo(doer));
        passiveJob.SetTarget(TargetIndex.C, new LocalTargetInfo(doer));
        if (forced)
        {
            // 双方都要：被拉进镜像 job 的敌对单位若不带这个记号，它的 think tree 会在
            // 下一个 override 检查点把 job 抢走 —— 旧写法被动方没这个旗标是因为它晚起，
            // 现在它从第 0 tick 就在场了。
            doerJob.playerForced = true;
            passiveJob.playerForced = true;
        }
        // 1.6 的 StartJob 不回 bool：起没起来看 CurJob 是不是刚做的那个 job。
        receiver.jobs.StartJob(passiveJob, JobCondition.InterruptForced);
        if (receiver.CurJob != passiveJob)
        {
            return;
        }
        doer.jobs.StartJob(doerJob, JobCondition.InterruptForced);
        if (doer.CurJob != doerJob)
        {
            receiver.jobs.EndCurrentJob(JobCondition.Incompletable);
        }
    }

    public static bool IsKissing(Pawn pawn) => pawn.CurJobDef == MWAH_JobDefOf.MWAH_Kiss;

}
