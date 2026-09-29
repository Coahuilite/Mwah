using Verse;

namespace Mwah;

/// <summary>
/// 日志门面：硬编码英文 + [MWAH] 前缀，便于全局检索，不做本地化。
/// 三档口径（2026-09-30 维护者裁定）：
/// <list type="bullet">
///   <item><description>横幅（Banner）：**永远播报**，不受任何开关影响 —— 启动时一行给出
///   模组名、版本、构建渠道与 OK/FAILED，关掉诊断也要能确认"模组活着/死了"。</description></item>
///   <item><description>诊断（Dev）：受设置页「诊断日志」开关管，关 ⇒ 此后完全静默。
///   旧写法是 [Conditional("MWAH_DEV")] 编译期剔除，发行档永远无诊断可开；改成运行时判定后
///   三个构建渠道同一能力，dev 包默认开、发行档默认关（Constants.DiagnosticLogs）。</description></item>
///   <item><description>故障（Warn/Error）：不受开关管 —— 它们不是诊断噪音而是"模组自己出事了"，
///   把它静音等于制造静默失败（本项目最恨的东西）。</description></item>
/// </list>
/// </summary>
public static class MwahLog
{
    /// <summary>启动横幅专用：无条件、醒目、可全局检索。</summary>
    public static void Banner(string message) => Log.Message(Constants.LogPrefix + message);

    public static void Info(string message) => Log.Message(Constants.LogPrefix + message);

    public static void Warn(string message) => Log.Warning(Constants.LogPrefix + message);

    public static void Error(string message) => Log.Error(Constants.LogPrefix + message);

    /// <summary>诊断事件。开关关着就直接返回 —— 但注意参数在调用点已求值，
    /// 所以这里只挂事件级打点（一桩吻几条），逐 tick 的采样走 KissTrace 的字段写入。</summary>
    public static void Dev(string message)
    {
        if (!MwahMod.Settings.DiagnosticsEnabled)
        {
            return;
        }
        Log.Message(Constants.LogPrefix + "dev: " + message);
    }
}
