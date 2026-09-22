using UnityEngine;
using System.Collections.Generic;
using Verse;

namespace Mwah;

/// <summary>
/// 亲吻冷却。进程内内存态，每局由 KissTicker 构造器清零（与 Let Me Gnaw On You 的
/// CooldownManager 同一取舍）：不落盘、不占 tick，只在结算时顺带清过期项，避免字典无界增长。
/// 读档后冷却归零是可接受的取舍：这是一个纯娱乐动作，不是经济系统。
/// </summary>
public static class KissCooldown
{
    /// <summary>表大到这个条数才开始扫过期项；再小直接跳过，扫的成本比省下来的还贵。</summary>
    private const int PruneThreshold = 64;

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

    /// <summary>
    /// 每局清零。GameComponent 构造器在每次建局与读档时都会重新跑（Game.FillComponents），
    /// 而本表是 static：TicksGame 每局从 0 重新计数，thingIDNumber 也从小号重新分配，
    /// 上局残留的键值会把新局的 pawn 按上局的 until 判成超长冷却。
    /// </summary>
    public static void Reset()
    {
        PawnReadyAtTick.Clear();
        PairReadyAtTick.Clear();
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

    /// <summary>
    /// 顺带清过期项：只在写入时跑，且要过 64 条才开始扫 —— 冷却条目活不过几游戏时，
    /// 正常情况下表本来就长不了，这个门槛是给"玩家连点导演台"那种场面兜底的。
    /// </summary>
    private static void PruneExpired(int now)
    {
        Prune(PawnReadyAtTick, ExpiredPawnKeys, now);
        Prune(PairReadyAtTick, ExpiredPairKeys, now);
    }

    /// <summary>复用同一个 scratch 列表：这里每 Mark 一次就跑一遍，没必要再各分配一个。</summary>
    private static void Prune<T>(Dictionary<T, int> map, List<T> scratch, int now)
    {
        if (map.Count <= PruneThreshold)
        {
            return;
        }
        scratch.Clear();
        foreach (KeyValuePair<T, int> entry in map)
        {
            if (entry.Value <= now)
            {
                scratch.Add(entry.Key);
            }
        }
        for (int i = 0; i < scratch.Count; i++)
        {
            map.Remove(scratch[i]);
        }
        scratch.Clear();
    }
}
