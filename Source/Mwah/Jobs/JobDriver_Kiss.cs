using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 站立版"滚床单"双人镜像 job（骨架照原版 JobDriver_Lovin，去掉床）。
/// TargetIndex.A = 对方；TargetIndex.B = 自己要走上的"台位格"。
///
/// 朝向本身不用发明：toil 打 handlingFacing 后 Pawn_RotationTracker.UpdateRotation 会整段早退，
/// 不再按 job 目标或 Drafted 覆盖朝向，剩下的只是每 tick 调 rotationTracker.Face ——
/// 与原版 JobDriver_StandAndBeSociallyActive 站着聊天用的是同一套。
///
/// 所以"做不到左右对向"是站位问题，不是朝向问题。旧写法让被动方用 PathEndMode.Touch
/// 贴到主动方身上：Touch 不区分方向，它往往先停在对方正上/正下方；而主动方挑的"侧面格"
/// 又随被动方的移动每 tick 重算。两人于是经常变成前后站位，朝向只能是 North/South。
/// 现在改成**定台**：开场就选定一对左右相邻的静态格子，一人一格各走各的，谁也不追谁。
/// </summary>
public class JobDriver_Kiss : JobDriver
{
    private const int InvalidCell = -999999;

    /// <summary>台位格离各自原位的最大容忍距离；超出就当找不到台，退回贴脸兜底。</summary>
    private const int StageRadius = 8;

    /// <summary>GenRadial 是无限发散的，这里只扫中点附近这么多格，保证开场只付一次小代价。</summary>
    private const int StageSearchLimit = 140;

    /// <summary>同一段路重复发第 3 次即判定走不动（门被关、被人堵住），结束 job，不再原地打转。</summary>
    private const int WalkAttemptBudget = 3;

    private readonly TargetIndex partnerInd = TargetIndex.A;

    /// <summary>本方剩余的亲吻 tick 数（双方各自倒数，倒计时归零即结束）。</summary>
    private int ticksLeft;

    /// <summary>倒计时真的走完过。只有走完才发心情：中途散场不该有奖励。</summary>
    private bool completed;

    /// <summary>被动方 = 被对方拉进 job 的那个；结算只由发起方做一次，避免发两份心情。</summary>
    private bool isPassivePartner;

    private int homeX = InvalidCell;
    private int homeZ = InvalidCell;

    /// <summary>发起方为对方选定的台位。写进对方的 job 之后只有对方读它，这里存着是为了读档后仍自洽。</summary>
    private int partnerStageX = InvalidCell;

    private int partnerStageZ = InvalidCell;

    /// <summary>job 起步时目标 B 是否已被填好：被填好说明自己是被动方（发起方写进来的）。</summary>
    private bool bornPassive;

#if MWAH_DEV
    private static void TraceSet(bool passive, string phase) => KissTrace.Set(passive, phase);

    private static void TraceTicks(bool passive, int ticks)
    {
        if (passive)
        {
            KissTrace.TicksB = ticks;
        }
        else
        {
            KissTrace.TicksA = ticks;
        }
    }
#else
    private static void TraceSet(bool passive, string phase)
    {
    }

    private static void TraceTicks(bool passive, int ticks)
    {
    }
#endif

    private IntVec3 lastWalkTarget = IntVec3.Invalid;
    private int walkAttempts;

    private Pawn Partner => (Pawn)job.GetTarget(partnerInd).Thing;

    private IntVec3 HomeCell => homeX == InvalidCell ? IntVec3.Invalid : new IntVec3(homeX, 0, homeZ);

    /// <summary>本方台位；无效表示"没找到台"，走贴脸兜底。</summary>
    private IntVec3 StageCell => job.GetTarget(TargetIndex.B).Cell;

    private IntVec3 PartnerStageCell => partnerStageX == InvalidCell ? IntVec3.Invalid : new IntVec3(partnerStageX, 0, partnerStageZ);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref completed, "completed");
        Scribe_Values.Look(ref isPassivePartner, "isPassivePartner");
        Scribe_Values.Look(ref homeX, "homeX", InvalidCell);
        Scribe_Values.Look(ref homeZ, "homeZ", InvalidCell);
        Scribe_Values.Look(ref partnerStageX, "partnerStageX", InvalidCell);
        Scribe_Values.Look(ref partnerStageZ, "partnerStageZ", InvalidCell);
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        // 独占对方：不会出现两个人同时排队亲同一个目标。
        return pawn.Reserve(Partner, job, 1, -1, null, errorOnFailed);
    }

    /// <summary>允许躺着的对象接受这个 job，否则"亲倒地的人"会在起步阶段被判不可开始。</summary>
    public override bool CanBeginNowWhileLyingDown() => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(partnerInd);
        this.FailOn(() => Partner.Dead);

        yield return ToilTakeStage();
        yield return ToilGotoStage();
        yield return ToilLockPartner();
        yield return ToilKiss();
    }

    /// <summary>
    /// 开场定台。只在发起方算一次并写进双方的 job 目标 B；被动方沿用，不再自己挑。
    /// 台位两格都由发起方占住，免得路人挤进两人中间。
    /// </summary>
    private Toil ToilTakeStage()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilTakeStage));
        toil.initAction = delegate
        {
            bornPassive = StageCell.IsValid;
            TraceSet(bornPassive, "stage");
            homeX = base.pawn.Position.x;
            homeZ = base.pawn.Position.z;
            if (StageCell.IsValid || !TryFindStage(out IntVec3 mine, out IntVec3 theirs))
            {
                return;
            }
            job.SetTarget(TargetIndex.B, new LocalTargetInfo(mine));
            partnerStageX = theirs.x;
            partnerStageZ = theirs.z;
            base.pawn.Reserve(mine, job, 1, -1, null, errorOnFailed: false);
            base.pawn.Reserve(theirs, job, 1, -1, null, errorOnFailed: false);
            MwahLog.Dev("stage " + mine + " for " + base.pawn.LabelShort + ", " + theirs + " for " + Partner.LabelShort);
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    /// <summary>走到自己那一格。目标是静态格，所以中途不需要重算，也不存在互相追着走。</summary>
    private Toil ToilGotoStage()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilGotoStage));
        toil.initAction = delegate
        {
            TraceSet(bornPassive, "walk");
            if (InPosition())
            {
                base.pawn.pather?.StopDead();
                ReadyForNextToil();
            }
            else if (!BeginWalk())
            {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.tickIntervalAction = delegate
        {
            if (base.pawn.pather == null || base.pawn.pather.Moving)
            {
                return;
            }
            if (InPosition())
            {
                base.pawn.pather.StopDead();
                ReadyForNextToil();
                return;
            }
            if (!BeginWalk())
            {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>把对方拉进同一个 job，并把它那一格交给它。</summary>
    private Toil ToilLockPartner()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilLockPartner));
        toil.initAction = delegate
        {
            isPassivePartner = Partner.CurJob != null && Partner.CurJob.def == MWAH_JobDefOf.MWAH_Kiss;
            TraceSet(isPassivePartner, "lock");
            if (!isPassivePartner)
            {
                var partnerJob = JobMaker.MakeJob(MWAH_JobDefOf.MWAH_Kiss, base.pawn);
                IntVec3 theirs = PartnerStageCell;
                if (theirs.IsValid)
                {
                    partnerJob.SetTarget(TargetIndex.B, new LocalTargetInfo(theirs));
                }
                Partner.jobs.StartJob(partnerJob, JobCondition.InterruptForced);
            }
            MwahLog.Dev("kiss lock: " + base.pawn.LabelShort + " -> " + Partner.LabelShort + (isPassivePartner ? " (passive)" : " (initiator)"));
            ticksLeft = MwahMod.Settings.DurationTicks;
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    /// <summary>贴着、相对、冒爱心，直到本方倒计时结束。</summary>
    private Toil ToilKiss()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilKiss));
        toil.handlingFacing = true;
        toil.initAction = delegate
        {
            base.pawn.pather?.StopDead();
            TraceSet(isPassivePartner, "perform");
            MwahLog.Dev("kiss perform: " + base.pawn.LabelShort + " at " + base.pawn.Position + ", partner at " + Partner.Position + ", ticks=" + ticksLeft);
            FaceEachOther();
        };
        toil.AddPreTickIntervalAction(delegate (int delta)
        {
            // 每 tick 钉回原地：台位是算好的，被别的 job 插一步就走开就前功尽弃。
            base.pawn.pather?.StopDead();
            ticksLeft -= delta;
            TraceTicks(isPassivePartner, ticksLeft);
            if (ticksLeft <= 0)
            {
                completed = true;
                ReadyForNextToil();
                return;
            }
            FaceEachOther();
            int interval = MwahMod.Settings.FleckIntervalTicks;
            if (interval > 0 && base.pawn.IsHashIntervalTick(interval, delta))
            {
                // 与原版滚床单完全同一支调用；两边各来一次，比原版更热闹。
                FleckMaker.ThrowMetaIcon(base.pawn.Position, base.pawn.Map, FleckDefOf.Heart);
                if (!Partner.Dead)
                {
                    FleckMaker.ThrowMetaIcon(Partner.Position, base.pawn.Map, FleckDefOf.Heart);
                }
            }
        });
        toil.AddFailCondition(delegate
        {
            // 对称判定：任何一方掉出这个 job，另一格上的独角戏立刻散场。
            return Partner.CurJob == null || Partner.CurJob.def != MWAH_JobDefOf.MWAH_Kiss;
        });
        toil.AddFinishAction(delegate
        {
            TraceSet(isPassivePartner, "end");
            MwahLog.Dev("kiss end: " + base.pawn.LabelShort + " completed=" + completed + " passive=" + isPassivePartner);
            if (completed && !isPassivePartner)
            {
                KissMoodReward.Settle(base.pawn, Partner);
            }
            KissUtility.RequestReturnHome(base.pawn, HomeCell);
        });
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>
    /// 找一对左右相邻的格子当台位：一格归我、一格归对方。
    /// 以两人中点为圆心按距离递增扫，第一个可用的组合即采用；动不了的一方钉在它当前格上
    /// （所以"亲倒地的人"也会得到正确的左右对向：站着的那个绕到它侧面）。
    /// </summary>
    private bool TryFindStage(out IntVec3 mine, out IntVec3 theirs)
    {
        mine = IntVec3.Invalid;
        theirs = IntVec3.Invalid;
        Pawn partner = Partner;
        Map map = base.pawn.Map;
        if (map == null || partner == null || partner.Map != map)
        {
            return false;
        }
        // 2x2 及以上（大象、部分机械族）放不下"两个 1x1 相邻格"这个模型，交给贴脸兜底。
        if (!SingleCell(base.pawn) || !SingleCell(partner))
        {
            MwahLog.Dev("stage skipped: multi-cell occupant " + base.pawn.LabelShort + " / " + partner.LabelShort);
            return false;
        }

        bool iMove = KissUtility.CanMoveNow(base.pawn);
        bool theyMove = KissUtility.CanMoveNow(partner);
        if (!iMove && !theyMove)
        {
            return false;
        }

        IntVec3 a = base.pawn.Position;
        IntVec3 b = partner.Position;
        IntVec3 mid = new IntVec3((a.x + b.x) / 2, 0, (a.z + b.z) / 2);
        int limit = Mathf.Min(StageSearchLimit, GenRadial.RadialPattern.Length);
        for (int i = 0; i < limit; i++)
        {
            IntVec3 west = mid + GenRadial.RadialPattern[i];
            IntVec3 east = west + IntVec3.East;
            // 两种分配都试：谁离哪格近就踩哪格，另一格归对方。
            if (west.DistanceTo(a) <= StageRadius && east.DistanceTo(b) <= StageRadius
                && Usable(west, base.pawn, partner, iMove) && Usable(east, partner, base.pawn, theyMove))
            {
                mine = west;
                theirs = east;
                return true;
            }
            if (east.DistanceTo(a) <= StageRadius && west.DistanceTo(b) <= StageRadius
                && Usable(east, base.pawn, partner, iMove) && Usable(west, partner, base.pawn, theyMove))
            {
                mine = east;
                theirs = west;
                return true;
            }
        }
        MwahLog.Dev("no stage within radius " + StageRadius + " for " + base.pawn.LabelShort + " / " + partner.LabelShort);
        return false;
    }

    /// <summary>
    /// 这一格能不能归这位用。能动的一方要求"能站、不迷雾、没有别人、到得了"；
    /// 动不了的一方只接受它自己当前那格 —— 它没法走过去，把它排到别处等于判这个 job 死刑。
    /// </summary>
    private static bool Usable(IntVec3 c, Pawn who, Pawn other, bool whoMoves)
    {
        if (!whoMoves)
        {
            return c == who.Position;
        }
        Map map = who.Map;
        if (map == null || !c.InBounds(map) || !c.Standable(map) || c.Fogged(map))
        {
            return false;
        }
        List<Thing> things = c.GetThingList(map);
        for (int i = 0; i < things.Count; i++)
        {
            // 当事人自己可以正站在这格上（台位常常就落在两人脚下），别人不行。
            if (things[i] is Pawn p && p != who && p != other && !p.Dead)
            {
                return false;
            }
        }
        return who.CanReach(c, PathEndMode.OnCell, Danger.Deadly);
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

    /// <summary>占格是不是 1x1：CellRect 只有 Width/Height，没有 CellCount。</summary>
    private static bool SingleCell(Pawn p)
    {
        CellRect r = p.OccupiedRect();
        return r.Width == 1 && r.Height == 1;
    }
}
