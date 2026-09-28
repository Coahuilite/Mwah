using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// "亲完走回原位"的 job 内收尾 toil —— 双人镜像 job 与亲物 job 共用。
///
/// 为什么它在 job 里：敌对单位的 think tree 只在 job 结束后接管。回程段留在 job 内，
/// 接近、表演、离开就全程都在 job 所有权之下 ⇒ **停火窗口 = job 生命期**，不需要任何额外机制。
/// 旧写法是 job 一结束就从队列发 Goto —— Goto 是随时可被 think tree 抢走的普通 job，
/// 袭击者亲完迈出第二步就重新锁定目标了。
///
/// 直接结束 job，兜底交给 kiss 段 finish action 登记的 KissReturnQueue ——
/// 殖民者"亲完还是走回家"的礼貌不丢，队列自己的准入过滤会消化掉不该发的。
/// 走位止损与双人版同一套：同一段路连发三次（被人堵、门被关）就地结束；
/// 另有阶段 tick 硬顶，回程是礼貌不是刑期，窗口可以短、不能长。
/// </summary>
public static class KissReturn
{
    /// <summary>同一段路重复发第 3 次即判定走不动（与定台走位的止损同值同义）。</summary>
    private const int WalkAttemptBudget = 3;

    /// <summary>回程段硬顶：1200 tick = 20 现实秒。到点就地结束，剩下的路交给队列兜底。</summary>
    private const int PhaseTicksCap = 1200;

    /// <summary>
    /// 造回程 toil。arrive 由调用方传 <c>ReadyForNextToil</c>（这是最后一个 toil，推进即正常结束）。
    /// 内部状态（重试计数、阶段计时）刻意不进存档：读档后重新起一段路即可，
    /// 与定台走位的 walkAttempts 同一取舍。
    /// </summary>
    public static Toil MakeToil(string traceTag, Pawn pawn, Func<IntVec3> homeCell, Func<bool> passive, Action arrive)
    {
        var toil = ToilMaker.MakeToil("ToilReturnHome");
        var walker = new Walker();
        IntVec3 home = IntVec3.Invalid;
        int ticks = 0;
        bool endImmediately = true;

        toil.initAction = delegate
        {
            KissTrace.Set(passive(), traceTag);
            ticks = 0;
            home = homeCell();
            endImmediately = !MwahMod.Settings.ReturnsHome || home == pawn.Position || !walker.Begin(pawn, home);
            if (!endImmediately)
            {
                MwahLog.Dev("kiss return: " + pawn.LabelShort + " -> " + home);
            }
        };
        toil.tickIntervalAction = delegate (int delta)
        {
            if (endImmediately)
            {
                arrive();
                return;
            }
            ticks += delta;
            if (ticks >= PhaseTicksCap)
            {
                arrive();
                return;
            }
            if (pawn.pather != null && pawn.pather.Moving)
            {
                return;
            }
            if (pawn.Position == home)
            {
                pawn.pather?.StopDead();
                arrive();
                return;
            }
            // 半路被堵停下来了：重发同一段路，预算用尽就地结束（finish action 登记的队列接着送）。
            if (!walker.Begin(pawn, home))
            {
                arrive();
            }
        };
        toil.socialMode = RandomSocialMode.Off;
        toil.defaultCompleteMode = ToilCompleteMode.Never;
        return toil;
    }

    /// <summary>一段回家路的记账本：目标格与重发次数。闭包捕获变量不能作 ref 实参，所以收进小类。</summary>
    private sealed class Walker
    {
        private IntVec3 lastWalkTarget = IntVec3.Invalid;
        private int walkAttempts;

        /// <summary>起（或重发）一段路。动不了、够不到、同一段路发到第 4 次，都算走不动。</summary>
        public bool Begin(Pawn pawn, IntVec3 dest)
        {
            if (!dest.IsValid || !KissUtility.CanMoveNow(pawn))
            {
                return false;
            }
            if (dest == lastWalkTarget)
            {
                walkAttempts++;
            }
            else
            {
                lastWalkTarget = dest;
                walkAttempts = 1;
            }
            if (walkAttempts > WalkAttemptBudget || !pawn.CanReach(dest, PathEndMode.OnCell, Danger.Deadly))
            {
                return false;
            }
            pawn.pather.StartPath(dest, PathEndMode.OnCell);
            return true;
        }
    }
}
