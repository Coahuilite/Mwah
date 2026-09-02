using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 自主亲吻：**只服务非玩家控制的 pawn**。玩家控制的小人仍然由你右键下令，这条路不碰它们。
///
/// 为什么需要它：右键菜单对非己方 pawn 永远不通 ——
/// FloatMenuContext 构造时先做 selectedPawns.RemoveAll(!CanTakeOrder)，
/// 而 CanTakeOrder 只放行玩家殖民者、己方机械族与可征召 subhuman；敌人的 pawn 连"被选中"都做不到。
/// 零 Harmony 下没法把下令权发给非己方单位，所以改由游戏自己发起。
///
/// 注册方式：GameComponent 由原版自动发现并实例化（1.6 的 GameComponent 无构造参数，
/// 也不需要 Def、XML 或补丁）。GameComponentTick 只在地图内运行，符合"仅游戏内地图"的边界。
/// </summary>
public class KissAmbient : GameComponent
{
    /// <summary>每个周期最多促成一桩；0 = 下次 tick 就可以试一次。</summary>
    private int nextCheckTick;

    public override void GameComponentTick()
    {
        MwahSettings? settings = MwahMod.Settings;
        if (settings == null || !settings.Enabled || !settings.AutonomousEnabled)
        {
            return;
        }
        int now = Find.TickManager.TicksGame;
        if (now < nextCheckTick)
        {
            return;
        }
        nextCheckTick = now + settings.AutonomousIntervalTicks;

        Map map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }
        TryOne(map, settings);
    }

    /// <summary>随机起点挑一个"玩家管不着"的当发起方，就近找对象。</summary>
    private static void TryOne(Map map, MwahSettings settings)
    {
        IReadOnlyList<Pawn> pawns = map.mapPawns.AllPawnsSpawned;
        if (pawns.Count < 2)
        {
            return;
        }
        // 从随机位置起扫，避免永远从同一个 pawn 开始试。
        int start = Rand.Range(0, pawns.Count - 1);
        for (int i = 0; i < pawns.Count; i++)
        {
            Pawn doer = pawns[(start + i) % pawns.Count];
            // 玩家控制得过来的，就不替他做主。
            if (doer.IsPlayerControlled)
            {
                continue;
            }
            if (!KissUtility.CanMoveNow(doer) || doer.jobs == null)
            {
                continue;
            }
            Pawn? receiver = NearestWilling(doer, pawns, settings);
            if (receiver != null && KissUtility.BeginAutonomous(doer, receiver))
            {
                return;
            }
        }
    }

    /// <summary>
    /// 就近找一个"判定通过"的对象。被亲的一方不做玩家控制限制——
    /// 袭击者来亲你的殖民者正是这模组的看点。
    /// 距离上限是刻意的：不设上限的话袭击者会穿过整张地图去亲另一端的人，
    /// 那不是搞笑，那是把战斗 AI 打断成观光团。
    /// </summary>
    private static Pawn? NearestWilling(Pawn doer, IReadOnlyList<Pawn> candidates, MwahSettings settings)
    {
        float best = settings.AutonomousRadius * settings.AutonomousRadius;
        Pawn? found = null;
        for (int i = 0; i < candidates.Count; i++)
        {
            Pawn other = candidates[i];
            if (other == doer || other.Dead || !other.Spawned)
            {
                continue;
            }
            float dist = doer.Position.DistanceToSquared(other.Position);
            if (dist >= best)
            {
                continue;
            }
            if (!KissUtility.PairLooksKissable(doer, other))
            {
                continue;
            }
            best = dist;
            found = other;
        }
        return found;
    }
}
