using System.Diagnostics;
using Verse;

namespace EveryPawnKissEachOther;

/// <summary>
/// 开发者面向日志：硬编码英文 + [EPK] 前缀，便于全局检索，不做本地化。
/// </summary>
public static class EPKLog
{
    public static void Info(string message) => Log.Message(Constants.LogPrefix + message);

    public static void Warn(string message) => Log.Warning(Constants.LogPrefix + message);

    public static void Error(string message) => Log.Error(Constants.LogPrefix + message);

    [Conditional("EPK_DEV")]
    public static void Dev(string message) => Log.Message(Constants.LogPrefix + "dev: " + message);
}
