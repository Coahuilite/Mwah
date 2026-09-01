using RimWorld;
using UnityEngine;
using Verse;

namespace EveryPawnKissEachOther;

/// <summary>
/// tick ↔ 现实秒 ↔ 游戏小时 的唯一换算入口。
/// 依据：GenTicks.TicksPerRealSecond = 60（1 倍速），GenDate.TicksPerDay = 60000，1 日 = 24 游戏小时。
/// 设置界面只允许以 tick 为存储单位，显示时同时给出三种单位，避免玩家猜数量级。
/// </summary>
public static class EPKTime
{
    public const int TicksPerRealSecond = GenTicks.TicksPerRealSecond; // 60
    public const int TicksPerGameHour = GenDate.TicksPerDay / 24;      // 2500

    public static float ToRealSeconds(int ticks) => ticks / (float)TicksPerRealSecond;

    public static float ToGameHours(int ticks) => ticks / (float)TicksPerGameHour;

    public static int FromRealSeconds(float seconds) => Mathf.CeilToInt(seconds * TicksPerRealSecond);

    public static int FromGameHours(float hours) => Mathf.CeilToInt(hours * TicksPerGameHour);

    /// <summary>以 tick 为主显示值，同时括出现实秒与游戏小时。</summary>
    public static string FormatTicks(int ticks)
    {
        return EPKStrings.Get("EPK.Settings.TimingFormat",
            ticks.ToString(),
            ToRealSeconds(ticks).ToString("0.##"),
            ToGameHours(ticks).ToString("0.###"));
    }

    /// <summary>以游戏小时为主显示值（心情时长这类玩家直觉按小时想）。</summary>
    public static string FormatGameHours(float hours)
    {
        return EPKStrings.Get("EPK.Settings.TimingFormat",
            FromGameHours(hours).ToString(),
            ToRealSeconds(FromGameHours(hours)).ToString("0.##"),
            hours.ToString("0.##"));
    }
}
