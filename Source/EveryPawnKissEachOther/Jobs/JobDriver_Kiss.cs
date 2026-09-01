using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace EveryPawnKissEachOther;

/// <summary>
/// 站立版"滚床单"双人镜像 job（骨架照原版 JobDriver_Lovin，去掉床）：
/// TargetIndex.A = 对方；双方各自跑同名 job，互相把对方拉进 job 里锁住，
/// 期间彼此 FaceTarget 相对、按间隔抛原版爱心 Fleck，结束时由发起方统一结算心情。
/// </summary>
public class JobDriver_Kiss : JobDriver
{
    private const int InvalidHome = -999999;

    private readonly TargetIndex partnerInd = TargetIndex.A;

    /// <summary>本方剩余的亲吻 tick 数（双方各自倒数，倒计时归零即结束）。</summary>
    private int ticksLeft;

    /// <summary>被动方 = 被对方拉进 job 的那个；结算只由发起方做一次，避免发两份心情。</summary>
    private bool isPassivePartner;

    private int homeX = InvalidHome;
    private int homeZ = InvalidHome;

    private Pawn Partner => (Pawn)job.GetTarget(partnerInd).Thing;

    private IntVec3 HomeCell => homeX == InvalidHome ? IntVec3.Invalid : new IntVec3(homeX, 0, homeZ);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref isPassivePartner, "isPassivePartner");
        Scribe_Values.Look(ref homeX, "homeX", InvalidHome);
        Scribe_Values.Look(ref homeZ, "homeZ", InvalidHome);
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

        yield return ToilGotoTouch();
        yield return ToilLockPartner();
        yield return ToilKiss();
    }

    /// <summary>走到贴脸邻格；已经贴着就直接进入下一步。</summary>
    private Toil ToilGotoTouch()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilGotoTouch));
        toil.initAction = delegate
        {
            homeX = base.pawn.Position.x;
            homeZ = base.pawn.Position.z;
            if (base.pawn.AdjacentTo8WayOrInside(Partner))
            {
                ReadyForNextToil();
            }
            else if (KissUtility.CanMoveNow(base.pawn))
            {
                base.pawn.pather.StartPath(Partner, PathEndMode.Touch);
            }
            else
            {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.tickIntervalAction = delegate
        {
            if (base.pawn.pather == null)
            {
                return;
            }
            if (base.pawn.AdjacentTo8WayOrInside(Partner) && !base.pawn.pather.Moving)
            {
                base.pawn.pather.StopDead();
                ReadyForNextToil();
                return;
            }
            if (!base.pawn.pather.Moving)
            {
                IntVec3 cell = SocialInteractionUtility.BestInteractableCell(base.pawn, Partner);
                if (cell.IsValid)
                {
                    base.pawn.pather.StartPath(cell, PathEndMode.OnCell);
                }
                else
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            }
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>把对方拉进同一个 job；对方已经在亲别人/已在 job 里则本次仍由我方独自完成。</summary>
    private Toil ToilLockPartner()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilLockPartner));
        toil.initAction = delegate
        {
            isPassivePartner = Partner.CurJob != null && Partner.CurJob.def == EPK_JobDefOf.EPK_Kiss;
            if (!isPassivePartner)
            {
                var partnerJob = JobMaker.MakeJob(EPK_JobDefOf.EPK_Kiss, base.pawn);
                Partner.jobs.StartJob(partnerJob, JobCondition.InterruptForced);
            }
            ticksLeft = EPKMod.Settings.DurationTicks;
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    /// <summary>贴着、相对、冒爱心，直到本方倒计时结束。</summary>
    private Toil ToilKiss()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilKiss));
        toil.initAction = delegate
        {
            FaceEachOther();
        };
        toil.AddPreTickIntervalAction(delegate(int delta)
        {
            ticksLeft -= delta;
            if (ticksLeft <= 0)
            {
                ReadyForNextToil();
                return;
            }
            FaceEachOther();
            int interval = EPKMod.Settings.FleckIntervalTicks;
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
            // 只约束被动方：对方走开了就散场。主动方不依赖对方是否接下了 job（躺着的对象可能接不下）。
            return isPassivePartner && (Partner.CurJob == null || Partner.CurJob.def != EPK_JobDefOf.EPK_Kiss);
        });
        toil.AddFinishAction(delegate
        {
            if (!isPassivePartner)
            {
                KissMoodReward.Settle(base.pawn, Partner);
            }
            KissUtility.RequestReturnHome(base.pawn, HomeCell);
        });
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    private void FaceEachOther()
    {
        Pawn partner = Partner;
        if (base.pawn.rotationTracker != null)
        {
            base.pawn.rotationTracker.FaceTarget(new LocalTargetInfo(partner));
        }
        if (partner.rotationTracker != null)
        {
            partner.rotationTracker.FaceTarget(new LocalTargetInfo(base.pawn));
        }
    }
}
