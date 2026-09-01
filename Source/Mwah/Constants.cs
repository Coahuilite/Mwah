using System;
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

    // 时长类默认值一律以 tick 为准，界面上再换算成现实秒与游戏小时。
    public const int KissDurationTicks = 150;          // ≈ 2.5 现实秒 ≈ 0.06 游戏小时
    public const int HeartFleckIntervalTicks = 100;    // 与原版 JobDriver_Lovin 的 TicksBetweenHeartMotes 一致
    public const int PawnCooldownTicks = 2500;         // 1 游戏小时
    public const int PairCooldownTicks = 6000;         // 2.4 游戏小时
    public const float ThoughtDurationGameHours = 6f;  // 与 MWAH_Kissed 的 durationDays 0.25 对齐

    public const float MoodMultiplier = 1f;
    public const int MaxSelectionReach = 9999;

    // 允许性开关的出厂默认：胡逼模式全开。
    public const bool ModEnabled = true;
    public const bool AllowHostileTargets = true;
    public const bool AllowNonHumanlike = true;
    public const bool ChangeOpinion = false;           // 默认不接入原版恋爱/好感链
    public const bool ReturnHomeAfterKiss = true;

    public static readonly IntRange DurationTicksRange = new(30, 2400);
    public static readonly IntRange FleckIntervalTicksRange = new(20, 1200);
    public static readonly IntRange CooldownTicksRange = new(0, 60000);
    public static readonly FloatRange ThoughtHoursRange = new(0.5f, 240f);
    public static readonly FloatRange MoodMultiplierRange = new(0f, 5f);
}

/// <summary>本地化取值入口。玩家可见文字一律走 Keyed，不硬编码。</summary>
public static class MwahStrings
{
    public static string Get(string key) => key.Translate();

    public static string Get(string key, params NamedArgument[] args) => key.Translate(args);
}
