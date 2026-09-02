using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 站立版"滚床单"双人镜像 job（骨架照原版 JobDriver_Lovin，去掉床）：
/// TargetIndex.A = 对方；双方各自跑同名 job，互相把对方拉进 job 里锁住，
/// 期间彼此相对、按间隔抛原版爱心 Fleck，结束时由发起方统一结算心情。
///
/// 站位刻意只走 X 轴（一左一右）：3/4 视角下上下相邻会互相遮挡，看不出"面对面"。
/// 朝向刻意走 <see cref="Toil.handlingFacing"/>：Pawn_RotationTracker.UpdateRotation
/// 每个视觉 tick 都会重算朝向，而且 pawn.Drafted 那一支会把朝向强制成 Rot4.South
/// （它在 curJob 分支之后没有 return），手动 FaceTarget 必然被覆盖。
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

    /// <summary>
    /// 谁负责绕到对方侧面：固定让 thingIDNumber 小的一方走位，另一方原地不动。
    /// 两方都试图绕到对方侧面会互相追着走（A 去东边时 B 也正往 A 的东边挪），
    /// 用 ID 破这个对称，零状态、跨存档稳定。
    /// </summary>
    private bool ArrangesPosition => pawn.thingIDNumber < Partner.thingIDNumber;

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

    /// <summary>走到对方左/右侧的邻格；已经并排就直接进入下一步。</summary>
    private Toil ToilGotoTouch()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilGotoTouch));
        toil.initAction = delegate
        {
            homeX = base.pawn.Position.x;
            homeZ = base.pawn.Position.z;
            if (Settled())
            {
                ReadyForNextToil();
            }
            else if (!KissUtility.CanMoveNow(base.pawn) || !StartApproach())
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
            if (Settled())
            {
                base.pawn.pather.StopDead();
                ReadyForNextToil();
                return;
            }
            if (!KissUtility.CanMoveNow(base.pawn) || !StartApproach())
            {
                EndJobWith(JobCondition.Incompletable);
            }
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>
    /// 位置是否已经到位：并排即算完成。
    /// 不负责走位的那一方只要已经贴到对方身上（哪怕是上下方向）就停手，
    /// 让负责走位的那一方绕过去——这是"两人互相绕圈"的唯一解。
    /// </summary>
    private bool Settled()
    {
        if (SideBySide(base.pawn, Partner))
        {
            return true;
        }
        return !ArrangesPosition && base.pawn.AdjacentTo8WayOrInside(Partner);
    }

    /// <summary>起一段走向侧面格；侧面不可用时退回原版的贴脸寻路。</summary>
    private bool StartApproach()
    {
        if (ArrangesPosition)
        {
            IntVec3 cell = HorizontalCell(base.pawn, Partner);
            if (cell.IsValid)
            {
                base.pawn.pather.StartPath(cell, PathEndMode.OnCell);
                return true;
            }
        }
        if (!base.pawn.CanReach(Partner.Position, PathEndMode.Touch, Danger.Deadly))
        {
            return false;
        }
        base.pawn.pather.StartPath(Partner, PathEndMode.Touch);
        return true;
    }

    /// <summary>把对方拉进同一个 job；对方已经在亲别人/已在 job 里则本次仍由我方独自完成。</summary>
    private Toil ToilLockPartner()
    {
        var toil = ToilMaker.MakeToil(nameof(ToilLockPartner));
        toil.initAction = delegate
        {
            isPassivePartner = Partner.CurJob != null && Partner.CurJob.def == MWAH_JobDefOf.MWAH_Kiss;
            if (!isPassivePartner)
            {
                var partnerJob = JobMaker.MakeJob(MWAH_JobDefOf.MWAH_Kiss, base.pawn);
                Partner.jobs.StartJob(partnerJob, JobCondition.InterruptForced);
            }
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
            // 只约束被动方：对方走开了就散场。主动方不依赖对方是否接下了 job（躺着的对象可能接不下）。
            return isPassivePartner && (Partner.CurJob == null || Partner.CurJob.def != MWAH_JobDefOf.MWAH_Kiss);
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

    /// <summary>
    /// 双方各按自己的位置挑一个四向朝向；并排时必然是 East/West。
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
        Vector3 d = looked.DrawPos - looker.DrawPos;
        looker.Rotation = Mathf.Abs(d.x) >= Mathf.Abs(d.z)
            ? (d.x > 0f ? Rot4.East : Rot4.West)
            : (d.z > 0f ? Rot4.North : Rot4.South);
    }

    /// <summary>并排 = 站在对方占格矩形的左右两侧、同一排（z 落在矩形内）。叠格不算。</summary>
    private static bool SideBySide(Pawn a, Pawn b)
    {
        CellRect rect = b.OccupiedRect();
        if (rect.Contains(a.Position))
        {
            return false;
        }
        return a.Position.z >= rect.minZ && a.Position.z <= rect.maxZ
            && (a.Position.x == rect.minX - 1 || a.Position.x == rect.maxX + 1);
    }

    /// <summary>
    /// 对方占格矩形的左/右邻格里挑一个能站、能到、不迷雾的；
    /// 先挑与自己同排（z 相同）的那一侧，否则绕远路。都不可用时返回 Invalid。
    /// </summary>
    private static IntVec3 HorizontalCell(Pawn me, Pawn other)
    {
        CellRect rect = other.OccupiedRect();
        int z = Mathf.Clamp(me.Position.z, rect.minZ, rect.maxZ);
        IntVec3 west = new IntVec3(rect.minX - 1, 0, z);
        IntVec3 east = new IntVec3(rect.maxX + 1, 0, z);
        IntVec3 first = me.Position.x <= rect.minX ? west : east;
        IntVec3 second = first == west ? east : west;
        if (Usable(me, first))
        {
            return first;
        }
        return Usable(me, second) ? second : IntVec3.Invalid;
    }

    private static bool Usable(Pawn me, IntVec3 c)
    {
        return c.InBounds(me.Map) && c.Standable(me.Map) && !c.Fogged(me.Map)
            && me.CanReach(c, PathEndMode.OnCell, Danger.Deadly);
    }
}
