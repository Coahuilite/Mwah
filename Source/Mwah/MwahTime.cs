using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// tick ↔ 现实秒 ↔ 游戏小时 的唯一换算入口。
/// 依据：GenTicks.TicksPerRealSecond = 60（1 倍速），GenDate.TicksPerDay = 60000，1 日 = 24 游戏小时。
/// 设置界面只允许以 tick 为存储单位，显示时同时给出三种单位，避免玩家猜数量级。
///
/// 反方向（秒/游戏时 → tick）刻意不提供：存储单位只有 tick。让某个配置项先按别的单位表达、
/// 再换算回来，正是上一轮"浮点小时进配置文件"的来路 —— 少一个入口就少一类漂移。
/// </summary>
public static class MwahTime
{
    public const int TicksPerRealSecond = GenTicks.TicksPerRealSecond; // 60
    public const int TicksPerGameHour = GenDate.TicksPerDay / 24;      // 2500

    public static float ToRealSeconds(int ticks) => ticks / (float)TicksPerRealSecond;

    public static float ToGameHours(int ticks) => ticks / (float)TicksPerGameHour;

    /// <summary>以 tick 为主显示值，同时括出现实秒与游戏小时。设置页与冷却提示共用这一种读法。</summary>
    public static string FormatTicks(int ticks)
    {
        return "MWAH.Settings.TimingFormat".Translate(
            ticks.ToString(),
            ToRealSeconds(ticks).ToString("0.##"),
            ToGameHours(ticks).ToString("0.###"));
    }
}
