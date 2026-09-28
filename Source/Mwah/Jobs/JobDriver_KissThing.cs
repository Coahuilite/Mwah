using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 单人亲"非 pawn 目标"（墙，将来树等）的共享 job —— 全 addon 族就这一个 driver、一个 JobDef：
/// 走到贴目标格、面向它、闭眼若干 tick、按间隔冒爱心，然后交给认领它的 addon 结算。
/// 与双人镜像 job 的差别全部来自"目标不进 job"：没有定台（只挑自己的一格）、没有 lock、
/// 没有回原位（它是自己走过去的，原版对抵达目标干完活的人不送回）。
/// 朝向沿用双人版验证过的路子：toil 打 handlingFacing 让 Pawn_RotationTracker 早退，
/// 每 tick 用 rotationTracker.Face(目标的 DrawPos) 钉住 —— 与站着聊天同一套。
/// </summary>
public class JobDriver_KissThing : JobDriver
{
    /// <summary>同一段路重复发第 3 次即判定走不动（与双人版同一止损）。</summary>
    private const int WalkAttemptBudget = 3;

    private int ticksLeft;

    private bool completed;

    private bool finished;

    private IntVec3 lastWalkTarget = IntVec3.Invalid;

    private int walkAttempts;

    /// <summary>认领这个目标的 addon。不存盘：读档后按谓词重查（For 只看 Accepts，不看开关 ——
    /// 表演已经在跑了，中途关开关不打断它，与"已起的 job 跑完"的双人语义一致）。</summary>
    private KissThingAddon? Addon => KissThingAddons.For(Target);

    private Thing Target => job.GetTarget(TargetIndex.A).Thing;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref ticksLeft, "ticksLeft", 0);
        Scribe_Values.Look(ref completed, "completed");
    }

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        // 独占这堵墙：同一时间两个人排着队亲同一面墙，画面过于凄凉。
        return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
    }

    public override bool CanBeginNowWhileLyingDown() => true;

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => Target.Destroyed);

        yield return ToilMarkCooldown();
        yield return ToilWalkToTarget();
        yield return ToilKiss();
    }

    /// <summary>
    /// 冷却在 job 落地后、走位前记（与双人版同一时机：预订失败时 job 根本没落地）。
    /// 与双人版同样**无条件记账** —— "无视冷却"只豁免检查，不豁免记账。
    /// </summary>
    private Toil ToilMarkCooldown()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilMarkCooldown));
        toil.initAction = delegate
        {
            MwahSettings settings = MwahMod.Settings;
            KissCooldown.Mark(pawn, Target, settings.PawnCooldown, settings.PairCooldown);
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
        if (stand == IntVec3.Invalid || !KissUtility.CanMoveNow(pawn))
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
        if (walkAttempts > WalkAttemptBudget || !pawn.CanReach(stand, PathEndMode.OnCell, Danger.Deadly))
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
            KissTrace.Set(false, "thing-end");
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
