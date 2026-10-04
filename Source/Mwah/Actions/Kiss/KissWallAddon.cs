using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 墙 = 第一个 addon。管线全在 <see cref="KissThingAddon"/> 基类，这里只剩墙的四件事：
/// 开关、谓词、标签口径、兜底分布。
///
/// 墙的口径（rimsage 对 Core XML 实证）：def.IsWall（人造墙，含一切继承 Wall 或自写
/// isWall 的 mod 墙）∨ building.isNaturalRock（天然墙、岩壁、坍塌岩 —— 压埋矿物 Mineable*
/// 全部 ParentName="RockBase"，一并落进这个谓词）。
///
/// 墙没有心情、没有嘴、不会回应：结算走天意表（scope KissWall，权重与文案全在
/// MWAH_FateDefs.xml），**不乘 SocialImpact**（那是"对方如何接住你"的量，墙的回应恒为零）、
/// **不挂 nullifyingTraits**（Psychopath 归零的是双向社交温度；墙无人回应，
/// 反社会人格恰恰最适合亲墙）。数值阶梯依据钉在 MWAH_ThoughtDefs.xml 的注释里。
/// </summary>
public sealed class KissWallAddon : KissThingAddon
{
    public static readonly KissWallAddon Instance = new();

    public override string Scope => ScopeConst;

    public override bool Active => MwahMod.Settings.Enabled && MwahMod.Settings.WallKissingEnabled;

    public override bool Accepts(Thing thing)
    {
        if (thing is not { Spawned: true } || thing.Destroyed || thing.def.category != ThingCategory.Building)
        {
            return false;
        }
        return thing.def.IsWall || (thing.def.building?.isNaturalRock ?? false);
    }

    /// <summary>
    /// 人形发起方（RaceProps.Humanlike，覆盖智人/har/任何 mod 人形种）用迟疑版
    /// 「亲吻{0}…?!」；机械族等非人形下令亲墙时不迟疑，用普通「亲吻{0}」。
    /// </summary>
    public override string OptionLabel(Pawn doer, Thing target)
    {
        string key = doer.RaceProps.Humanlike
            ? "MWAH.KissWall.OptionHumanlike"
            : "MWAH.KissWall.Option";
        return key.Translate(target.LabelCap);
    }

    /// <summary>
    /// 兜底分布（表被删空时）：与出厂 MWAH_FateDefs.xml 同权重、无旁白 ——
    /// 20/30/25/17/8 掷五档，心情照发、短讯照弹，只是心情条目退回静态描述。
    /// </summary>
    public override void SettleWithoutTable(Pawn doer, Thing target)
    {
        int[] weights = { 20, 30, 25, 17, 8 };
        string[] messageKeys =
        {
            "MWAH.KissWall.Result.Weird",
            "MWAH.KissWall.Result.Nothing",
            "MWAH.KissWall.Result.Slight",
            "MWAH.KissWall.Result.Moved",
            "MWAH.KissWall.Result.Devoted",
        };
        int roll = Rand.RangeInclusive(0, 99);
        int tier = 0;
        for (int cumulative = 0; tier < weights.Length; tier++)
        {
            cumulative += weights[tier];
            if (roll < cumulative)
            {
                break;
            }
        }
        if (tier >= weights.Length)
        {
            tier = 2; // 权重和不为 100 时的兜底：落在"小暖"，不落在越界
        }
        // 兜底行只在内存里存在（不进 DefDatabase）：五档 = 单 def 的 stageIndex，
        // 字段与出厂表的档位语义一一对应。
        var row = new MWAH_FateDef
        {
            defName = "MWAH_Fate_Fallback_Wall",
            scope = ScopeConst,
            thought = MWAH_ThoughtDefOf.MWAH_KissedWall,
            stageIndex = tier,
            weight = weights[tier],
            messageKey = messageKeys[tier],
            messageType = tier <= 1 ? MessageTypeDefOf.NegativeEvent : MessageTypeDefOf.PositiveEvent,
        };
        KissFate.GrantRow(doer, target, row);
    }

    private const string ScopeConst = "KissWall";
}
