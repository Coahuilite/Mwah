using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 单人亲墙 job：走到墙边、面向它、闭眼若干 tick、按间隔冒爱心，然后天意结算。
/// 与双人镜像 job 的差别全部来自"墙不进 job"：没有定台（只挑自己的一格）、没有 lock、
/// 没有回原位（它是自己走过去的，原版对走到目标干完活的人不送回）。
/// 朝向沿用双人版验证过的路子：toil 打 handlingFacing 让 Pawn_RotationTracker 早退，
/// 每 tick 用 rotationTracker.Face(墙的 DrawPos) 钉住 —— 与站着聊天同一套。
/// </summary>
public class JobDriver_KissWall : JobDriver
{
    /// <summary>同一段路重复发第 3 次即判定走不动（与双人版同一止损）。</summary>
    private const int WalkAttemptBudget = 3;

    private int ticksLeft;

    private bool completed;

    private bool finished;

    private IntVec3 lastWalkTarget = IntVec3.Invalid;

    private int walkAttempts;

    private Thing Wall => job.GetTarget(TargetIndex.A).Thing;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref completed, "completed");
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        // 独占这堵墙：同一时间两个人排着队亲同一面墙，画面过于凄凉。
        return pawn.Reserve(Wall, job, 1, -1, null, errorOnFailed);
    }

    public override bool CanBeginNowWhileLyingDown() => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => Wall.Destroyed);

        yield return ToilMarkCooldown();
        yield return ToilWalkToWall();
        yield return ToilKissWall();
    }

    /// <summary>
    /// 冷却在 job 落地后、走位前记：双人版把记账推迟到定台 toil 是为了躲"预订失败空罚"，
    /// 这里预订已在 TryMakePreToilReservations 完成，走到第一格之前罚与不罚只差几 tick，
    /// 记在开场最便宜（墙不会中途消失来陷害这个记账）。
    /// </summary>
    private Toil ToilMarkCooldown()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilMarkCooldown));
        toil.initAction = delegate
        {
            MwahSettings settings = MwahMod.Settings;
            if (!settings.CooldownsIgnored)
            {
                KissCooldown.Mark(base.pawn, Wall, settings.PawnCooldown, settings.PairCooldown);
            }
            MwahLog.Dev("wall kiss start: " + base.pawn.LabelShort + " -> " + Wall.LabelCap);
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Instant;
        return toil;
    }

    private Toil ToilWalkToWall()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilWalkToWall));
        toil.initAction = delegate
        {
            KissTrace.Set(false, "wall-walk");
            TryWalk();
        };
        toil.tickIntervalAction = delegate
        {
            if (base.pawn.pather != null && base.pawn.pather.Moving)
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
        IntVec3 stand = KissWallUtility.FindStandCell(base.pawn, Wall);
        if (stand == base.pawn.Position)
        {
            base.pawn.pather?.StopDead();
            ReadyForNextToil();
            return;
        }
        if (stand == IntVec3.Invalid || !KissUtility.CanMoveNow(base.pawn))
        {
            EndJobWith(JobCondition.Incompletable);
            return;
        }
        if (stand == lastWalkTarget)
        {
            walkAttempts++;
        }
        else
        {
            lastWalkTarget = stand;
            walkAttempts = 1;
        }
        if (walkAttempts > WalkAttemptBudget || !base.pawn.CanReach(stand, PathEndMode.OnCell, Danger.Deadly))
        {
            EndJobWith(JobCondition.Incompletable);
            return;
        }
        base.pawn.pather.StartPath(stand, PathEndMode.OnCell);
    }

    private Toil ToilKissWall()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilKissWall));
        toil.handlingFacing = true;
        toil.initAction = delegate
        {
            base.pawn.pather?.StopDead();
            KissTrace.Set(false, "wall-perform");
            ticksLeft = MwahMod.Settings.DurationTicks;
            FaceWall();
        };
        toil.AddPreTickIntervalAction(delegate (int delta)
        {
            base.pawn.pather?.StopDead();
            FaceWall();
            ticksLeft -= delta;
            KissTrace.Ticks(false, ticksLeft);
            if (ticksLeft <= 0)
            {
                completed = true;
                ReadyForNextToil();
                return;
            }
            int interval = MwahMod.Settings.FleckIntervalTicks;
            if (interval > 0 && base.pawn.IsHashIntervalTick(interval, delta))
            {
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
            // 只有倒计时真的走完才结算：中途散场（墙被拆、被征召）不该有天意。
            if (completed)
            {
                KissWallReward.Settle(base.pawn, Wall);
            }
            KissTrace.Set(false, "wall-end");
        });
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    private void FaceWall()
    {
        if (base.pawn.rotationTracker != null && Wall != null)
        {
            base.pawn.rotationTracker.Face(Wall.DrawPos);
        }
    }
}
