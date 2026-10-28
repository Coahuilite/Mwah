using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 单人亲"非 pawn 目标"（墙，将来树等）的共享 job —— 全 addon 族共用 driver，由各自 JobDef 固定身份：
/// 走到贴目标格、面向它、闭眼若干 tick、按间隔冒爱心，然后**在 job 内走回原位**收尾。
/// 与双人镜像 job 的差别全部来自"目标不进 job"：没有定台（只挑自己的一格）、没有 lock。
/// 回程段与双人版同一个理由：敌对者的 think tree 只在 job 结束后接管，离开段留在 job 内，
/// An unfinished normal departure can use a vanilla finalizer; interruptions never insert one.
/// 朝向沿用双人版验证过的路子：toil 打 handlingFacing 让 Pawn_RotationTracker 早退，
/// 每 tick 用 rotationTracker.Face(目标的 DrawPos) 钉住 —— 与站着聊天同一套。
/// </summary>
public class JobDriver_KissThing : JobDriver
{

    private const int InvalidCell = -999999;
    private const int PerformanceToilIndex = 2;
    private const int ReturnToilIndex = 3;

    private int homeX = InvalidCell;

    private int homeZ = InvalidCell;

    private int ticksLeft;

    private bool completed;

    private bool finished;

    private readonly WalkTally walkTally = new();
    private readonly KissReturn departure = new();

    // JobDef metadata is saved activity identity, including legacy wall jobs.
    private KissThingAddon? Addon => KissThingAddons.ForJob(job);

    private Thing Target => job.GetTarget(TargetIndex.A).Thing;

    private IntVec3 HomeCell => homeX == InvalidCell ? IntVec3.Invalid : new IntVec3(homeX, 0, homeZ);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref completed, "completed");
        Scribe_Values.Look(ref finished, "finished", completed && CurToilIndex > PerformanceToilIndex);
        Scribe_Values.Look(ref homeX, "homeX", InvalidCell);
        Scribe_Values.Look(ref homeZ, "homeZ", InvalidCell);
        departure.ExposeData();
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        // 独占这堵墙：同一时间两个人排着队亲同一面墙，画面过于凄凉。
        return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
    }

    /// <summary>伤害自毁闸：与双人版同判据、同时机（伤害事件本身，同步，零延迟）——
    /// 被发配亲墙的机械体挨一枪就散场反打，playerForced 挡不住这个钩子（见 JobDriver_Kiss 的详注）。</summary>
    public override void Notify_DamageTaken(DamageInfo dinfo)
    {
        base.Notify_DamageTaken(dinfo);
        if (dinfo.Def.ExternalViolenceFor(base.pawn) && dinfo.Def.canInterruptJobs)
        {
            MwahLog.Note("thing kiss broken by damage: " + base.pawn.LabelShort + " (dmg=" + dinfo.Def.defName + ")");
            EndJobWith(JobCondition.Incompletable);
        }
    }

    public override bool CanBeginNowWhileLyingDown() => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => Target.Destroyed);
        SetFinalizerJob(condition => KissReturn.Finalizer(pawn, HomeCell, condition,
            CurToilIndex > ReturnToilIndex)!);
        AddFinishAction(_ => KissTrace.Set(false, "thing-end"));

        yield return ToilBegin();
        yield return ToilWalkToTarget();
        yield return ToilKiss();
        yield return departure.MakeToil("thing-return", base.pawn, () => HomeCell, () => false, ReadyForNextToil);
    }

    /// <summary>
    /// Record home before movement. Dispatch commits cooldowns after admission succeeds.
    /// </summary>
    private Toil ToilBegin()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilBegin));
        toil.initAction = delegate
        {
            homeX = base.pawn.Position.x;
            homeZ = base.pawn.Position.z;
            MwahLog.Dev("thing kiss start: " + pawn.LabelShort + " -> " + Target.LabelCap);
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private Toil ToilWalkToTarget()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilWalkToTarget));
        toil.initAction = delegate
        {
            KissTrace.Set(false, "thing-walk");
            TryWalk();
        };
        toil.tickIntervalAction = delegate
        {
            if (pawn.pather != null && pawn.pather.Moving)
            {
                return;
            }
            TryWalk();
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>到位即推进；迈不开步（被封死、被人堵）就止损结束，不在原地反复起步。</summary>
    private void TryWalk()
    {
        IntVec3 stand = KissThingAddon.FindStandCell(pawn, Target);
        if (stand == pawn.Position)
        {
            pawn.pather?.StopDead();
            ReadyForNextToil();
            return;
        }
        if (stand == IntVec3.Invalid || !KissBoundary.CanMoveNow(pawn))
        {
            EndJobWith(JobCondition.Incompletable);
            return;
        }
        if (!walkTally.KeepTrying(stand) || !pawn.CanReach(stand, PathEndMode.OnCell, Danger.Deadly))
        {
            EndJobWith(JobCondition.Incompletable);
            return;
        }
        pawn.pather.StartPath(stand, PathEndMode.OnCell);
    }

    private Toil ToilKiss()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilKiss));
        toil.handlingFacing = true;
        toil.initAction = delegate
        {
            pawn.pather?.StopDead();
            KissTrace.Set(false, "thing-perform");
            ticksLeft = MwahMod.Settings.DurationTicks;
            FaceTarget();
        };
        toil.AddPreTickIntervalAction(delegate (int delta)
        {
            pawn.pather?.StopDead();
            FaceTarget();
            ticksLeft -= delta;
            KissTrace.Ticks(false, ticksLeft);
            if (ticksLeft <= 0)
            {
                completed = true;
                ReadyForNextToil();
                return;
            }
            int interval = MwahMod.Settings.FleckIntervalTicks;
            if (interval > 0 && pawn.IsHashIntervalTick(interval, delta))
            {
                FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, FleckDefOf.Heart);
            }
        });
        toil.AddFinishAction(delegate
        {
            if (finished)
            {
                return;
            }
            finished = true;
            // 只有倒计时真的走完才结算：中途散场（墙被拆、被征召）不该有天意。
            if (completed)
            {
                Addon?.Settle(pawn, Target);
            }
        });
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    private void FaceTarget()
    {
        if (pawn.rotationTracker != null && Target != null)
        {
            pawn.rotationTracker.Face(Target.DrawPos);
        }
    }
}
