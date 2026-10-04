using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;

namespace Mwah;

/// <summary>
/// 定台：给两个人挑一对左右相邻的格子，一人一格各走各的，谁也不追谁。
/// 从 JobDriver_Kiss 挪出来是因为**双方 job 在发起的那一帧同时起来**，
/// 台位必须在两个 job 都存在之前就定好（被动方的 job 一出生就要有自己的格子可走）。
/// 搜索逻辑保留：以两人中点为圆心按距离递增扫，第一个可用的组合即采用；
/// 动不了的一方钉在它当前格上（所以"亲倒地的人"也会得到正确的左右对向：
/// 站着的那个绕到它侧面）。找不到台就返回 false，双方退回原版贴脸兜底。
/// </summary>
public static class KissStage
{
    /// <summary>台位格离各自原位的最大容忍距离；超出就当找不到台，退回贴脸兜底。</summary>
    private const int StageRadius = 8;

    /// <summary>
    /// 定台期间允许的寻路查询失败上限。Usable 的最后一步是 CanReach（A*），140 个径向候选
    /// 理论最坏能烧 ~280 次：密集废墟里这一 tick 就是可感卡顿。超过预算直接放弃定台走贴脸兜底
    /// ——失败路径本来就要走兜底，所以只省时间、不换行为。
    /// </summary>
    private const int ReachCheckBudget = 24;

    /// <summary>GenRadial 是无限发散的，这里只扫中点附近这么多格，保证开场只付一次小代价。</summary>
    private const int StageSearchLimit = 140;

    /// <summary>为 a、b 各挑一格（a 拿 cellA、b 拿 cellB）。返回值 false 时两个都是 Invalid。</summary>
    public static bool TryFind(Pawn a, Pawn b, out IntVec3 cellA, out IntVec3 cellB)
    {
        cellA = IntVec3.Invalid;
        cellB = IntVec3.Invalid;
        Map map = a.Map;
        if (map == null || b.Map != map)
        {
            return false;
        }
        // 2x2 及以上（大象、部分机械族）放不下"两个 1x1 相邻格"这个模型，交给贴脸兜底。
        if (!SingleCell(a) || !SingleCell(b))
        {
            MwahLog.Note("stage skipped: multi-cell occupant " + a.LabelShort + " / " + b.LabelShort);
            return false;
        }

        bool aMoves = KissUtility.CanMoveNow(a);
        bool bMoves = KissUtility.CanMoveNow(b);
        if (!aMoves && !bMoves)
        {
            return false;
        }

        IntVec3 pa = a.Position;
        IntVec3 pb = b.Position;
        IntVec3 mid = new IntVec3((pa.x + pb.x) / 2, 0, (pa.z + pb.z) / 2);
        int reachBudget = ReachCheckBudget;
        int limit = Mathf.Min(StageSearchLimit, GenRadial.RadialPattern.Length);
        for (int i = 0; i < limit; i++)
        {
            IntVec3 west = mid + GenRadial.RadialPattern[i];
            IntVec3 east = west + IntVec3.East;
            // 两种分配都试：谁离哪格近就踩哪格，另一格归对方。
            if (west.DistanceTo(pa) <= StageRadius && east.DistanceTo(pb) <= StageRadius
                && Usable(west, a, b, aMoves, ref reachBudget) && Usable(east, b, a, bMoves, ref reachBudget))
            {
                cellA = west;
                cellB = east;
                return true;
            }
            if (east.DistanceTo(pa) <= StageRadius && west.DistanceTo(pb) <= StageRadius
                && Usable(east, a, b, aMoves, ref reachBudget) && Usable(west, b, a, bMoves, ref reachBudget))
            {
                cellA = east;
                cellB = west;
                return true;
            }
        }
        MwahLog.Note("no stage within radius " + StageRadius + " for " + a.LabelShort + " / " + b.LabelShort);
        return false;
    }

    /// <summary>
    /// 这一格能不能归这位用。能动的一方要求"能站、不迷雾、没有别人、到得了"；
    /// 动不了的一方只接受它自己当前那格 —— 它没法走过去，把它排到别处等于判这个 job 死刑。
    /// **对方的脚下格永远不行**：双方 job 同帧起之后，"我的台=你的位置、你的台=我的位置"
    /// 是互换死锁 —— 原版寻路不会终结在别人占着的格上，两人各自停在旁边互等对方挪窝，
    /// 走位预算烧光双双静默散场（2026-10-04 实机定罪的"换座之舞"：两次发配都在 begin 后
    /// 一秒内无声死亡，trace 里连 walk 相位都没留下）。自己的脚下格可以：零成本到位。
    /// </summary>
    private static bool Usable(IntVec3 c, Pawn who, Pawn other, bool whoMoves, ref int reachBudget)
    {
        if (c == other.Position)
        {
            return false;
        }
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
        // 预算耗尽后不再做 A*，只接受"人已经在格上"这种零成本确认；这次定台多半会失败，
        // 但失败本来就走贴脸兜底，省下的是同一 tick 里几十次寻路。
        if (reachBudget <= 0)
        {
            return c == who.Position;
        }
        bool reachable = who.CanReach(c, PathEndMode.OnCell, Danger.Deadly);
        if (!reachable)
        {
            reachBudget--;
        }
        return reachable;
    }

    /// <summary>占格是不是 1x1：CellRect 只有 Width/Height，没有 CellCount。</summary>
    private static bool SingleCell(Pawn p)
    {
        CellRect r = p.OccupiedRect();
        return r.Width == 1 && r.Height == 1;
    }
}
