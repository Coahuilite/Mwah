using UnityEngine;
using System.Collections.Generic;
using Verse;

namespace EveryPawnKissEachOther;

/// <summary>
/// 亲吻冷却。会话内内存态（与 Let Me Gnaw On You 的 CooldownManager 同一取舍）：
/// 不落盘、不占 tick，只在结算时顺带清过期项，避免字典无界增长。
/// 读档后冷却归零是可接受的取舍：这是一个纯娱乐动作，不是经济系统。
/// </summary>
public static class KissCooldown
{
    private static readonly Dictionary<int, int> PawnReadyAtTick = new();
    private static readonly Dictionary<long, int> PairReadyAtTick = new();
    private static readonly List<int> ExpiredPawnKeys = new();
    private static readonly List<long> ExpiredPairKeys = new();

    public static int PawnRemaining(Pawn pawn)
    {
        if (pawn == null)
        {
            return 0;
        }
        return Remaining(Find.TickManager.TicksGame, PawnReadyAtTick, pawn.thingIDNumber);
    }

    public static int PairRemaining(Pawn a, Pawn b)
    {
        if (a == null || b == null)
        {
            return 0;
        }
        return Remaining(Find.TickManager.TicksGame, PairReadyAtTick, PairKey(a, b));
    }

    public static void Mark(Pawn a, Pawn b, int pawnTicks, int pairTicks)
    {
        int now = Find.TickManager.TicksGame;
        if (pawnTicks > 0)
        {
            if (a != null)
            {
                PawnReadyAtTick[a.thingIDNumber] = now + pawnTicks;
            }
            if (b != null)
            {
                PawnReadyAtTick[b.thingIDNumber] = now + pawnTicks;
            }
        }
        if (pairTicks > 0 && a != null && b != null)
        {
            PairReadyAtTick[PairKey(a, b)] = now + pairTicks;
        }
        PruneExpired(now);
    }

    private static int Remaining<T>(int now, Dictionary<T, int> map, T key)
    {
        if (!map.TryGetValue(key, out int until))
        {
            return 0;
        }
        if (until <= now)
        {
            map.Remove(key);
            return 0;
        }
        return until - now;
    }

    /// <summary>成对键与单人键分表存放，这里只需要保证同一对人不论谁亲谁都同键。</summary>
    private static long PairKey(Pawn a, Pawn b)
    {
        int lo = Mathf.Min(a.thingIDNumber, b.thingIDNumber);
        int hi = Mathf.Max(a.thingIDNumber, b.thingIDNumber);
        return ((long)lo << 32) | (uint)hi;
    }

    private static void PruneExpired(int now)
    {
        if (PawnReadyAtTick.Count > 64)
        {
            ExpiredPawnKeys.Clear();
            foreach (KeyValuePair<int, int> entry in PawnReadyAtTick)
            {
                if (entry.Value <= now)
                {
                    ExpiredPawnKeys.Add(entry.Key);
                }
            }
            for (int i = 0; i < ExpiredPawnKeys.Count; i++)
            {
                PawnReadyAtTick.Remove(ExpiredPawnKeys[i]);
            }
        }
        if (PairReadyAtTick.Count > 64)
        {
            ExpiredPairKeys.Clear();
            foreach (KeyValuePair<long, int> entry in PairReadyAtTick)
            {
                if (entry.Value <= now)
                {
                    ExpiredPairKeys.Add(entry.Key);
                }
            }
            for (int i = 0; i < ExpiredPairKeys.Count; i++)
            {
                PairReadyAtTick.Remove(ExpiredPairKeys[i]);
            }
        }
    }
}
