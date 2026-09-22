using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 全部可配置项的代码层兜底默认值。运行时的真实来源是 MwahSettings（玩家可改），
/// 这里只定义"出厂默认"，不要在此处直接读改。
/// </summary>
public static class Constants
{
    public const string ModId = "coahuilite.mwah";
    public const string LogPrefix = "[MWAH] ";

    // 时长类默认值一律以 tick 为准（硬边界：tick 是唯一的存储单位），界面上再换算成现实秒与游戏小时。
    public const int KissDurationTicks = 150;          // ≈ 2.5 现实秒 ≈ 0.06 游戏小时
    public const int HeartFleckIntervalTicks = 100;    // 与原版 JobDriver_Lovin 的 TicksBetweenHeartMotes 一致
    public const int PawnCooldownTicks = 2500;         // 1 游戏小时
    public const int PairCooldownTicks = 6000;         // 2.4 游戏小时

    /// <summary>
    /// 心情记忆存活出厂默认 = 1 游戏日，与 MWAH_Kissed 的 durationDays 1 对齐。
    /// 整条数值阶梯（为什么是 +5 而不是 +8、为什么是一日而不是六小时）的依据写在
    /// 1.6/Defs/Kiss/MWAH_ThoughtDefs.xml 的注释里。
    /// </summary>
    public const int ThoughtDurationTicks = 60000;

    public const float MoodMultiplier = 1f;
    /// <summary>心情倍率粒度 0.05 倍。</summary>
    public const float MoodMultiplierStep = 0.05f;

    // 允许性开关的出厂默认：不限阵营、不限种族，门禁滑条停在最右档。
    public const bool ModEnabled = true;
    public const int PairScopeDefault = (int)KissScope.Everything;
    public const bool ChangeOpinion = false;           // 默认不接入原版恋爱/好感链
    public const bool ReturnHomeAfterKiss = true;
    public const bool DirectorButton = true;             // 底栏「亲吻导演台」按钮
    public const bool WallKissing = true;                // 亲墙：默认开（内容开关，刻意不挨着门禁滑条）
    public const bool NoCooldowns = false;                 // 超凡智能的大手：无视冷却
    public const bool AutonomousKissing = false;      // 第三优先级，先不默认开
    public const int AutonomousIntervalTicks = 250;      // 1 游戏小时促成一桩
    public const int AutonomousRadiusCells = 10;         // 超出这个距离就不去追

    public static readonly IntRange DurationTicksRange = new(30, 2400);
    public static readonly IntRange FleckIntervalTicksRange = new(20, 1200);
    public static readonly IntRange CooldownTicksRange = new(0, 60000);
    // 心情持续的跨度 0.5 游戏时 ~ 10 游戏日，粒度 1/4 游戏时：更细的跨度体感上没有区别，
    // 却会往配置里写没意义的数字。
    public static readonly IntRange ThoughtDurationTicksRange = new(1250, 600000);
    public const int ThoughtDurationStepTicks = MwahTime.TicksPerGameHour / 4;
    public static readonly FloatRange MoodMultiplierRange = new(0f, 5f);
    public static readonly IntRange PairScopeRange = new((int)KissScope.FreeColonists, (int)KissScope.Everything);
    public static readonly IntRange AutonomousIntervalTicksRange = new(30, 60000);
    public static readonly IntRange AutonomousRadiusRange = new(2, 40);
}

