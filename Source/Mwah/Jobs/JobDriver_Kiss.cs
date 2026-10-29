using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 站立版"滚床单"双人镜像 job（骨架照原版 JobDriver_Lovin，去掉床）。
/// TargetIndex.A = 对方；TargetIndex.B = 自己要走上的"台位格"；TargetIndex.C = 发起方本人。
///
/// **双方 job 在发起的那一帧同时起来**（定台与建 job 都在 <see cref="KissUtility.Start"/>）。
/// 这不只是省一个 lock toil：敌对单位的 think tree 只在 job 结束后接管，所以
/// "接近 + 表演 + 离开"全程都在 job 所有权之内 ⇒ 谁都不攻击谁 —— **停火窗口就是 job 生命期本身**，
/// 不需要任何额外的停火机制。亲吻的逻辑占用只到表演结束，返程保留 job 并不继续占用。旧写法被动方要等发起方走到台位才被拉进 job，
/// 接近段里被亲的那位是自由的，可以边射边把这场亲打断。
/// 角色不再靠"目标 B 是否已被填好"猜：C 端永远是发起方，我不是他就是被动方。
///
/// 朝向本身不用发明：toil 打 handlingFacing 后 Pawn_RotationTracker.UpdateRotation 会整段早退，
/// 不再按 job 目标或 Drafted 覆盖朝向，剩下的只是每 tick 调 rotationTracker.Face ——
/// 与原版 JobDriver_StandAndBeSociallyActive 站着聊天用的是同一套。
///
/// "左右对向"靠**定台**：开场就选定一对左右相邻的静态格子（<see cref="KissStage"/>），
/// 一人一格各走各的，谁也不追谁。找不到台就退回原版贴脸相邻（PathEndMode.Touch，允许上下）。
/// </summary>
public class JobDriver_Kiss : JobDriver
{
    private const int InvalidCell = -999999;
    private const int PerformanceToilIndex = 2;
    private const int ReturnToilIndex = 3;

    private readonly TargetIndex partnerInd = TargetIndex.A;

    /// <summary>发起方的亲吻倒计时（被动方不倒数，见 ToilKiss）。</summary>
    private int ticksLeft;

    /// <summary>倒计时真的走完过。只有走完才发心情：中途散场不该有奖励。</summary>
    private bool completed;

    /// <summary>finish action 幂等闸：收尾栈被重入时只结算一次。</summary>
    private bool finished;

    // Native Job.loadID survives save/load and distinguishes replacement jobs, including pooled Jobs.
    private int partnerJobId = -1;

    /// <summary>被动方 = 不是发起方的那个（按 C 端判，开场定一次，之后不再改）。</summary>
    private bool isPassivePartner;

    private int homeX = InvalidCell;
    private int homeZ = InvalidCell;

    private readonly WalkTally walkTally = new();
    private readonly KissReturn departure = new();

    private Pawn Partner => (Pawn)job.GetTarget(partnerInd).Thing;

    private IntVec3 HomeCell => homeX == InvalidCell ? IntVec3.Invalid : new IntVec3(homeX, 0, homeZ);

    /// <summary>本方台位；无效表示"没找到台"，走贴脸兜底。</summary>
    private IntVec3 StageCell => job.GetTarget(TargetIndex.B).Cell;

    public bool IsKissing => !ended && !completed && CurToilIndex <= PerformanceToilIndex;

    public override string GetReport()
    {
        if (!IsKissing) return "MWAH.Job.Returning".Translate();
        return base.GetReport();
    }

    /// <summary>Both admitted jobs bind before the next simulation tick.</summary>
    internal void BindPartner(int loadId) => partnerJobId = loadId;

    private JobDriver_Kiss? MatchingPartner
    {
        get
        {
            JobDriver_Kiss? other = Partner?.jobs?.curDriver as JobDriver_Kiss;
            if (other == null || other.ended || other.Partner != pawn
                || other.job.GetTarget(TargetIndex.C).Pawn != job.GetTarget(TargetIndex.C).Pawn)
            {
                return null;
            }
            // Older saves have no explicit pair IDs. Reconstruct only a reciprocal pair from
            // the same dispatch tick, then retain the concrete IDs rather than reclassifying it.
            if (partnerJobId < 0)
            {
                if (other.job.startTick != job.startTick
                    || (other.partnerJobId >= 0 && other.partnerJobId != job.loadID))
                {
                    return null;
                }
                partnerJobId = other.job.loadID;
                other.partnerJobId = job.loadID;
            }
            return other.job.loadID == partnerJobId && other.partnerJobId == job.loadID ? other : null;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref completed, "completed");
        Scribe_Values.Look(ref finished, "finished", completed && CurToilIndex > PerformanceToilIndex);
        Scribe_Values.Look(ref partnerJobId, "partnerJobId", -1);
        Scribe_Values.Look(ref isPassivePartner, "isPassivePartner");
        Scribe_Values.Look(ref homeX, "homeX", InvalidCell);
        Scribe_Values.Look(ref homeZ, "homeZ", InvalidCell);
        departure.ExposeData();
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        // 独占对方 + 自己那格台位。两个镜像 job 各自订对方、各站各的格，
        // 合起来正好是旧写法里发起方一次订下的全部（原版 Lovin 同样是互相预订）。
        if (!pawn.Reserve(Partner, job, 1, -1, null, errorOnFailed))
        {
            return false;
        }
        IntVec3 stage = StageCell;
        return !stage.IsValid || pawn.Reserve(stage, job, 1, -1, null, errorOnFailed);
    }

    /// <summary>允许躺着的对象接受这个 job，否则"亲倒地的人"会在起步阶段被判不可开始。</summary>
    public override bool CanBeginNowWhileLyingDown() => true;

    /// <summary>
    /// 伤害自毁闸 —— 停火窗口唯一可信的打断触发点。**触发时机 = 伤害事件本身**（同步调用，
    /// 零延迟；approach/perform/return 任何阶段吃到有效伤害都在下一拍之前散场），三条发起路径
    /// 同一时钟。为什么必须自己掐：原版"伤害→打断 job"链（Pawn_JobTracker.Notify_DamageTaken）
    /// 排在 `!curJob.playerForced` 与 180 tick 冷却之后 —— 而导演台/自主派发的 job 双方都带
    /// playerForced（不带的话 think tree 起步即抢回，见 KissUtility.BeginDirected），靠原版链
    /// "边挨枪边亲嘴"就是无敌点穴。driver 自己的这个钩子在两道闸**之前**被 job tracker 调用
    /// （原版 JobDriver_PredatorHunt / JobDriver_TendPatient 就是靠它掐自己的 job，有先例）。
    /// 判据照抄原版那两条闸：对外暴力（ExternalViolenceFor）且可中断 job（canInterruptJobs）——
    /// 摔伤、饿死、友军治疗不算挨打，不掐。
    /// </summary>
    public override void Notify_DamageTaken(DamageInfo dinfo)
    {
        base.Notify_DamageTaken(dinfo);
        if (dinfo.Def.ExternalViolenceFor(base.pawn) && dinfo.Def.canInterruptJobs)
        {
            MwahLog.Note((IsKissing ? "kiss broken by damage: " : "kiss return interrupted by damage: ")
                + base.pawn.LabelShort + " (dmg=" + dinfo.Def.defName + ")");
            EndJobWith(JobCondition.Incompletable);
        }
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        // Waiting without crawling must not exempt either actor from live participation rules.
        // Once performance ends, departure no longer depends on the former partner's state.
        this.FailOn(() => IsKissing &&
            (!KissBoundary.CanParticipate(pawn).Accepted || !KissBoundary.CanParticipate(Partner).Accepted));
        SetFinalizerJob(condition => KissReturn.Finalizer(pawn, HomeCell, condition,
            CurToilIndex > ReturnToilIndex)!);
        AddFinishAction(_ => KissTrace.Set(isPassivePartner, "end"));

        yield return ToilBegin();
        yield return ToilGotoStage();
        yield return ToilKiss();
        yield return departure.MakeToil("return", base.pawn, () => HomeCell, () => isPassivePartner, ReadyForNextToil, job);
    }

    /// <summary>
    /// Record role, home and duration. Dispatch commits cooldowns after both admissions succeed.
    /// </summary>
    private Toil ToilBegin()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilBegin));
        toil.initAction = delegate
        {
            isPassivePartner = job.GetTarget(TargetIndex.C).Pawn != base.pawn;
            homeX = base.pawn.Position.x;
            homeZ = base.pawn.Position.z;
            ticksLeft = MwahMod.Settings.DurationTicks;
            KissTrace.Set(isPassivePartner, "begin");
            MwahLog.Dev("kiss begin: " + base.pawn.LabelShort + " <-> " + Partner.LabelShort
                + (isPassivePartner ? " (passive)" : " (initiator)"));
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    /// <summary>
    /// 走到自己那一格。目标是静态格，所以中途不需要重算，也不存在互相追着走。
    /// 起步与每 tick 的检查是同一段判断，抽成 <see cref="TryReachStage"/>，免得两份逻辑各改各的。
    /// </summary>
    private Toil ToilGotoStage()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilGotoStage));
        toil.initAction = delegate
        {
            KissTrace.Set(isPassivePartner, "walk");
            TryReachStage();
        };
        toil.tickIntervalAction = delegate
        {
            // 还在路上就不插手；pather 没了（被别的 job 抢走或已经走完）才需要再看一眼。
            if (base.pawn.pather != null && base.pawn.pather.Moving)
            {
                return;
            }
            TryReachStage();
        };
        toil.AddFailCondition(delegate
        {
            // 启动帧宽限：双方 job 同帧起（被动方先、发起方后），而 vanilla 在 StartJob 调用处
            // **同步**跑 ReadyForNextToil —— Instant 的 ToilBegin 当场链进本 toil，入场即评估
            // 这条 endCondition（JobDriver.CheckCurrentToilEndOrFail 早于 initAction）。那一刻
            // 后起的一方 CurJob 还没落地，"对方离场"会被误读成"对方还没起 job"，双方当场互判
            // 散场（2026-09-28 停火窗口改造埋下的时序死锁，2026-10-04 实机才炸出）。落地当 tick
            // 一律不判离场：StartJob 已把 newJob.startTick 设为本 tick，故 TicksGame<=startTick
            // 恰覆盖启动帧；真实离场（死亡 / 被抢 job / 受击散场）都在后续 tick，照常捕获。
            if (Find.TickManager.TicksGame <= job.startTick)
            {
                return false;
            }
            // 对方已经不在亲吻 job 里（走位失败、受击散场）就别继续走向空台。
            if (MatchingPartner == null)
            {
                MwahLog.Note("kiss abort in walk: " + base.pawn.LabelShort + " partner " + Partner.LabelShort + " left the kiss");
                return true;
            }
            return false;
        });
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>到位就推进到下一个 toil；迈不开步（动不了、够不到、走位止损）就判这桩亲不成了。</summary>
    private void TryReachStage()
    {
        if (InPosition())
        {
            base.pawn.pather?.StopDead();
            ReadyForNextToil();
        }
        else if (!BeginWalk())
        {
            MwahLog.Note("kiss walk fail: " + base.pawn.LabelShort + " stage=" + StageCell
                + " canMove=" + KissBoundary.CanMoveNow(base.pawn) + " attempts=" + walkTally.Attempts);
            EndJobWith(JobCondition.Incompletable);
        }
    }

    /// <summary>贴着、相对、冒爱心，直到发起方的倒计时走完。</summary>
    private Toil ToilKiss()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilKiss));
        toil.handlingFacing = true;
        toil.initAction = delegate
        {
            base.pawn.pather?.StopDead();
            KissTrace.Set(isPassivePartner, "perform");
            MwahLog.Dev("kiss perform: " + base.pawn.LabelShort + " at " + base.pawn.Position + ", partner at " + Partner.Position + ", ticks=" + ticksLeft);
            FaceEachOther();
        };
        toil.AddPreTickIntervalAction(delegate (int delta)
        {
            base.pawn.pather?.StopDead();
            if (completed)
            {
                ReadyForNextToil();
                return;
            }
            JobDriver_Kiss? other = MatchingPartner;
            if (other == null)
            {
                // StartJob initializes toils synchronously, before the other job is installed.
                // No countdown or reward is possible during that admission window.
                if (Find.TickManager.TicksGame > job.startTick)
                {
                    MwahLog.Note("kiss cut short: " + base.pawn.LabelShort + " partner " + Partner.LabelShort + " gone at t-" + ticksLeft);
                    ReadyForNextToil();
                }
                return;
            }
            if (other.CurToilIndex > PerformanceToilIndex)
            {
                ReadyForNextToil();
                return;
            }
            // An early arrival waits inside its performance toil. Both matching jobs must be
            // performing at their assigned positions before the initiator advances the clock.
            if (other.CurToilIndex != PerformanceToilIndex)
            {
                return;
            }
            if (!InPosition() || !other.InPosition())
            {
                ReadyForNextToil();
                return;
            }
            if (!isPassivePartner)
            {
                ticksLeft -= delta;
                KissTrace.Ticks(false, ticksLeft);
                if (ticksLeft <= 0)
                {
                    completed = true;
                    // Completion belongs to the initiator, but both jobs retain the result even
                    // if the initiator finishes a zero-length departure before the next Pawn tick.
                    other.completed = true;
                    // Release both owners now: the passive Pawn may not tick before a new
                    // request arrives. Releasing just this Pawn would leave its partner locked.
                    KissReturn.ReleaseReservations(pawn, job);
                    KissReturn.ReleaseReservations(other.pawn, other.job);
                    ReadyForNextToil();
                    return;
                }
            }
            else
            {
                KissTrace.Ticks(true, other.ticksLeft);
            }
            FaceEachOther();
            int interval = MwahMod.Settings.FleckIntervalTicks;
            if (interval > 0 && base.pawn.IsHashIntervalTick(interval, delta))
            {
                // 每人只抛自己头顶的爱心：双方相位错开，同一间隔内两个位置各有一颗，
                // 观感不变、粒子总量减半（旧写法每人替对方也抛一次，总量是原版四倍）。
                FleckMaker.ThrowMetaIcon(base.pawn.Position, base.pawn.Map, FleckDefOf.Heart);
            }
        });
        toil.AddFinishAction(delegate
        {
            if (finished)
            {
                return;
            }
            finished = true;
            KissReturn.ReleaseReservations(pawn, job);
            if (completed && !isPassivePartner)
            {
                KissMoodReward.Settle(base.pawn, Partner);
            }
        });
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>位置是否已经到位：有台位就要求踩中它；没台位时贴到对方身上即算到位。</summary>
    private bool InPosition()
    {
        IntVec3 stage = StageCell;
        if (stage.IsValid)
        {
            return base.pawn.Position == stage;
        }
        return base.pawn.AdjacentTo8WayOrInside(Partner);
    }

    /// <summary>起一段路：优先走向台位格；没有台位时退回原版贴脸寻路（允许上下相邻）。</summary>
    private bool BeginWalk()
    {
        if (InPosition())
        {
            return true;
        }
        if (!KissBoundary.CanMoveNow(base.pawn))
        {
            return false;
        }
        IntVec3 stage = StageCell;
        if (!stage.IsValid)
        {
            if (!base.pawn.CanReach(Partner.Position, PathEndMode.Touch, Danger.Deadly))
            {
                return false;
            }
            // A moving touch target has no stable stage cell for the retry tally.
            // Native path failure, partner loss and damage remain the cancellation paths.
            base.pawn.pather.StartPath(Partner, PathEndMode.Touch);
            return true;
        }
        if (!walkTally.KeepTrying(stage) || !base.pawn.CanReach(stage, PathEndMode.OnCell, Danger.Deadly))
        {
            return false;
        }
        base.pawn.pather.StartPath(stage, PathEndMode.OnCell);
        return true;
    }

    /// <summary>
    /// Each driver owns its Pawn's facing; never rotate a partner after it leaves this job.
    /// </summary>
    private void FaceEachOther()
    {
        SetFacing(base.pawn, Partner);
    }

    private static void SetFacing(Pawn looker, Pawn looked)
    {
        if (looker == null || looked == null || looker.rotationTracker == null)
        {
            return;
        }
        // 用原版同一支 Face：它按 DrawPos 取角度并走 RotFromAngleBiased，
        // 与站着聊天、跳舞、听演讲的朝向完全一致。
        looker.rotationTracker.Face(looked.DrawPos);
    }
}
