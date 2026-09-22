using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 亲墙的天意结算：五档互斥、按权重掷一次，弹一条带墙名的消息并把对应心情记忆塞给发起方。
/// 墙不吃门禁也不乘 SocialImpact（设计决定，理由写在 KissWallUtility 头注释）。
/// 数值阶梯锚点（与 MWAH_ThoughtDefs.xml 同一份依据）：滚床单 +8/3日 > 美言 +5/2日 >
/// 被蹭 +4/1日；双人吻 +5/1日。亲墙是"没有回应的单向多情"，期望值刻意压在双人吻之下：
/// 五档加权期望 ≈ +0.84/次，但方差是全部社交行为里最大的 —— 这正是笑点所在。
/// "毫无感觉"档也发 thought（moodOffset 0）：心情列表里挂着一条"亲了一堵墙（毫无感觉）"
/// 比什么都不加更有戏，也让五档在日志与面板里口径一致。
/// </summary>
public static class KissWallReward
{
    private static readonly ThoughtDef[] TierDefs =
    {
        MWAH_ThoughtDefOf.MWAH_KissedWall_Weird,
        MWAH_ThoughtDefOf.MWAH_KissedWall_Nothing,
        MWAH_ThoughtDefOf.MWAH_KissedWall_Slight,
        MWAH_ThoughtDefOf.MWAH_KissedWall_Moved,
        MWAH_ThoughtDefOf.MWAH_KissedWall_Devoted,
    };

    /// <summary>权重和 = 100：怪 20 / 无感 30 / 小暖 25 / 中动 17 / 真爱 8。</summary>
    private static readonly int[] TierWeights = { 20, 30, 25, 17, 8 };

    /// <summary>
    /// 每档 10 条叙述式旁白（墙全程不开口，只被观察）。键 = MWAH.KissWall.Line.{档}.{0..9}，
    /// 中英各 50 条逐条对译；结算时从命中档的池子里随机播一条。
    /// 全部写成字面量：反向键门要求"每个被定义的键都能在代码里找到引用"，拼接出来的键名它看不见。
    /// </summary>
    private static readonly string[][] TierLineKeys =
    {
        new[]
        {
            "MWAH.KissWall.Line.Weird.0", "MWAH.KissWall.Line.Weird.1", "MWAH.KissWall.Line.Weird.2",
            "MWAH.KissWall.Line.Weird.3", "MWAH.KissWall.Line.Weird.4", "MWAH.KissWall.Line.Weird.5",
            "MWAH.KissWall.Line.Weird.6", "MWAH.KissWall.Line.Weird.7", "MWAH.KissWall.Line.Weird.8",
            "MWAH.KissWall.Line.Weird.9",
        },
        new[]
        {
            "MWAH.KissWall.Line.Nothing.0", "MWAH.KissWall.Line.Nothing.1", "MWAH.KissWall.Line.Nothing.2",
            "MWAH.KissWall.Line.Nothing.3", "MWAH.KissWall.Line.Nothing.4", "MWAH.KissWall.Line.Nothing.5",
            "MWAH.KissWall.Line.Nothing.6", "MWAH.KissWall.Line.Nothing.7", "MWAH.KissWall.Line.Nothing.8",
            "MWAH.KissWall.Line.Nothing.9",
        },
        new[]
        {
            "MWAH.KissWall.Line.Slight.0", "MWAH.KissWall.Line.Slight.1", "MWAH.KissWall.Line.Slight.2",
            "MWAH.KissWall.Line.Slight.3", "MWAH.KissWall.Line.Slight.4", "MWAH.KissWall.Line.Slight.5",
            "MWAH.KissWall.Line.Slight.6", "MWAH.KissWall.Line.Slight.7", "MWAH.KissWall.Line.Slight.8",
            "MWAH.KissWall.Line.Slight.9",
        },
        new[]
        {
            "MWAH.KissWall.Line.Moved.0", "MWAH.KissWall.Line.Moved.1", "MWAH.KissWall.Line.Moved.2",
            "MWAH.KissWall.Line.Moved.3", "MWAH.KissWall.Line.Moved.4", "MWAH.KissWall.Line.Moved.5",
            "MWAH.KissWall.Line.Moved.6", "MWAH.KissWall.Line.Moved.7", "MWAH.KissWall.Line.Moved.8",
            "MWAH.KissWall.Line.Moved.9",
        },
        new[]
        {
            "MWAH.KissWall.Line.Devoted.0", "MWAH.KissWall.Line.Devoted.1", "MWAH.KissWall.Line.Devoted.2",
            "MWAH.KissWall.Line.Devoted.3", "MWAH.KissWall.Line.Devoted.4", "MWAH.KissWall.Line.Devoted.5",
            "MWAH.KissWall.Line.Devoted.6", "MWAH.KissWall.Line.Devoted.7", "MWAH.KissWall.Line.Devoted.8",
            "MWAH.KissWall.Line.Devoted.9",
        },
    };

    private static readonly MessageTypeDef[] TierMessageTypes =
    {
        MessageTypeDefOf.NegativeEvent, MessageTypeDefOf.NegativeEvent,
        MessageTypeDefOf.PositiveEvent, MessageTypeDefOf.PositiveEvent, MessageTypeDefOf.PositiveEvent,
    };

    public static void Settle(Pawn pawn, Thing wall)
    {
        if (pawn.needs?.mood?.thoughts?.memories == null)
        {
            return;
        }

        int roll = Rand.RangeInclusive(0, 99);
        int tier = 0;
        for (int cumulative = 0; tier < TierWeights.Length; tier++)
        {
            cumulative += TierWeights[tier];
            if (roll < cumulative)
            {
                break;
            }
        }
        if (tier >= TierDefs.Length)
        {
            tier = 2; // 权重和不为 100 时的兜底：落在"小暖"，不落在越界
        }

        if (ThoughtMaker.MakeThought(TierDefs[tier]) is Thought_Memory memory)
        {
            pawn.needs.mood.thoughts.memories.TryGainMemory(memory, null);
        }
        string line = TierLineKeys[tier][Rand.Range(0, TierLineKeys[tier].Length - 1)]
            .Translate(pawn.Named("PAWN"), wall.Named("WALL"));
        Messages.Message(line, new LookTargets(wall), TierMessageTypes[tier], historical: false);
        MwahLog.Dev("wall kiss settle: " + pawn.LabelShort + " -> " + wall.LabelCap + " tier=" + tier);
    }
}
