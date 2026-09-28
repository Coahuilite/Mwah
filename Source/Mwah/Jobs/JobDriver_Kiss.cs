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
/// 不需要任何额外的停火机制。旧写法被动方要等发起方走到台位才被拉进 job，
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

    /// <summary>同一段路重复发第 3 次即判定走不动（门被关、被人堵住），结束 job，不再原地打转。</summary>
    private const int WalkAttemptBudget = 3;

    private readonly TargetIndex partnerInd = TargetIndex.A;

    /// <summary>发起方的亲吻倒计时（被动方不倒数，见 ToilKiss）。</summary>
    private int ticksLeft;

    /// <summary>倒计时真的走完过。只有走完才发心情：中途散场不该有奖励。</summary>
    private bool completed;

    /// <summary>finish action 幂等闸：收尾栈被重入时只结算一次。</summary>
    private bool finished;

    /// <summary>被动方 = 不是发起方的那个（按 C 端判，开场定一次，之后不再改）。</summary>
    private bool isPassivePartner;

    private int homeX = InvalidCell;
    private int homeZ = InvalidCell;

    private IntVec3 lastWalkTarget = IntVec3.Invalid;
    private int walkAttempts;

    private Pawn Partner => (Pawn)job.GetTarget(partnerInd).Thing;

    private IntVec3 HomeCell => homeX == InvalidCell ? IntVec3.Invalid : new IntVec3(homeX, 0, homeZ);

    /// <summary>本方台位；无效表示"没找到台"，走贴脸兜底。</summary>
    private IntVec3 StageCell => job.GetTarget(TargetIndex.B).Cell;

    /// <summary>对方的 driver（可能已结束/换人），被动方靠它读发起方的时钟。</summary>
    private JobDriver_Kiss? PartnerDriver => Partner?.jobs?.curDriver as JobDriver_Kiss;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref completed, "completed");
        Scribe_Values.Look(ref isPassivePartner, "isPassivePartner");
        Scribe_Values.Look(ref homeX, "homeX", InvalidCell);
        Scribe_Values.Look(ref homeZ, "homeZ", InvalidCell);
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
    /// "回原位"兜底的登记点。JobDriver 没有"job 收尾"钩子（1.6 的 Cleanup 挂在 ThingComp
    /// 一类，driver 上没有可重写的方法），所以登记放在 kiss 段的 finish action：它覆盖
    /// "到过表演段之后的一切出口"（正常走完、受击散场、对方死亡），与旧写法同一覆盖面；
    /// 走位段就失败的人本来也没走到需要送回家的地方。入队而非当场下令：finish action 跑在
    /// job 收尾栈里，同步起新 job 就是上局的死循环（见 KissReturnQueue 类注释）。
    /// 正常路径里 job 内回程先把人送回家，队列下一 tick 发放时被"已站在原位"的过滤消化；
    /// job 内回程中途止损的，这条队列 Goto 正好接着送。
    /// </summary>
    private void EnqueueReturnFallback()
    {
        KissReturnQueue.Enqueue(base.pawn, HomeCell);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(partnerInd);
        this.FailOn(() => Partner.Dead);

        yield return ToilBegin();
        yield return ToilGotoStage();
        yield return ToilKiss();
        yield return KissReturn.MakeToil("return", base.pawn, () => HomeCell, () => isPassivePartner, ReadyForNextToil);
    }

    /// <summary>
    /// 开场：认角色、记原位、记冷却。定台已经挪到 KissUtility.Start（双方 job 同帧起来，
    /// 台位必须在 job 存在之前就定好）。冷却仍在这里记而不是 StartJob 之前：
    /// 预订失败时 job 根本没落地，提前记账就是空罚一轮冷却。
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
            if (!isPassivePartner)
            {
                MwahSettings settings = MwahMod.Settings;
                KissCooldown.Mark(base.pawn, Partner, settings.PawnCooldown, settings.PairCooldown);
            }
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
            // 对方已经不在亲吻 job 里（走位失败、受击散场）就别继续走向空台。
            return Partner.CurJob == null || Partner.CurJob.def != MWAH_JobDefOf.MWAH_Kiss;
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
            // 每 tick 钉回原地：台位是算好的，被别的 job 插一步就走开就前功尽弃。
            base.pawn.pather?.StopDead();
            // 时钟只归发起方倒数；被动方读发起方的旗标离场。两条离场路都进 **job 内的回程段**
            // 而不是直接结束 job —— 敌对者的 think tree 只在 job 结束后接管，留在 job 里
            // 走完离开段才有停火；真被攻击时 checkOverrideOnDamage 会把 job 掐掉，退回队列兜底。
            // （不各自数各自的：同一 tick 里谁先被处理谁先结束，结算归属会随机丢一半。）
            if (!isPassivePartner)
            {
                ticksLeft -= delta;
                KissTrace.Ticks(false, ticksLeft);
                if (ticksLeft <= 0)
                {
                    completed = true;
                    ReadyForNextToil();
                    return;
                }
                // 对方中途掉出这场亲（被拉走、受击）：独角戏没有意义，直接进回程。
                if (Partner.CurJob == null || Partner.CurJob.def != MWAH_JobDefOf.MWAH_Kiss)
                {
                    ReadyForNextToil();
                    return;
                }
            }
            else
            {
                KissTrace.Ticks(true, ticksLeft);
                // 发起方走完（completed）或已经离场（driver 没了/换人）⇒ 被动方跟进回程段。
                JobDriver_Kiss? driver = PartnerDriver;
                if (driver == null || driver.completed)
                {
                    ReadyForNextToil();
                    return;
                }
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
            // "回原位"兜底在这里登记（覆盖面与理由见 EnqueueReturnFallback 的注释）。
            EnqueueReturnFallback();
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
        if (!KissUtility.CanMoveNow(base.pawn))
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
            lastWalkTarget = Partner.Position;
            base.pawn.pather.StartPath(Partner, PathEndMode.Touch);
            return true;
        }
        if (!PathProgressing(stage) || !base.pawn.CanReach(stage, PathEndMode.OnCell, Danger.Deadly))
        {
            return false;
        }
        base.pawn.pather.StartPath(stage, PathEndMode.OnCell);
        return true;
    }

    /// <summary>
    /// 走位止损：路径被截断（门被关、被人堵住）时 pawn 会停在半路，此时目标格不变，
    /// 每 tick 重发同一段路就是原地打转且 job 永不结束。同一格连发三次即判定走不动。
    /// 计数器不进存档：读档后允许多试一次，比永久卡死便宜。
    /// </summary>
    private bool PathProgressing(IntVec3 c)
    {
        if (c == lastWalkTarget)
        {
            walkAttempts++;
        }
        else
        {
            lastWalkTarget = c;
            walkAttempts = 1;
        }
        return walkAttempts <= WalkAttemptBudget;
    }

    /// <summary>
    /// 双方各按自己的位置转向对方；并排时必然是 East/West。
    /// 这里也顺手把对方的朝向摆正：被动方可能已经先一步结束并交回控制权。
    /// </summary>
    private void FaceEachOther()
    {
        SetFacing(base.pawn, Partner);
        SetFacing(Partner, base.pawn);
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
