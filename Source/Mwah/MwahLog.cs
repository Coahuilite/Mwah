using Verse;

namespace Mwah;

/// <summary>
/// 诊断档位（2026-10-05 维护者裁定，取代旧的是/否开关）。设置页存原始值，
/// 生效级别由 <see cref="MwahLog.Level"/> 解析；滑条是"档"不是"量"，刻意不带数值框
/// （与门禁滑条同一先例）。
/// </summary>
public enum MwahDiag
{
    /// <summary>除横幅与 Warn/Error 外什么都不发。</summary>
    Off = 0,

    /// <summary>按构建渠道走：dev 包 = Verbose，Steam/GitHub 包 = Simple。</summary>
    Auto = 1,

    /// <summary>值得知道的事才说话：每桩吻的结算行（fate row）、一切异常出口、启动自证。</summary>
    Simple = 2,

    /// <summary>完整叙事：阶段流水（stage/begin/perform/return）、点选代际、设置持久化，外加 KissTrace 采样器。</summary>
    Verbose = 3,
}

/// <summary>
/// 日志门面：硬编码英文 + [MWAH] 前缀，便于全局检索，不做本地化。
/// 四档口径（2026-10-05 维护者裁定，继承 09-30 的三档纪律）：
/// <list type="bullet">
///   <item><description>横幅（Banner）：**永远播报**，不受任何档位影响 —— 启动时一行给出
///   模组名、版本、构建渠道与 OK/FAILED，关掉诊断也要能确认"模组活着/死了"。</description></item>
///   <item><description>要事（Note，<c>[MWAH] note:</c>）：Simple 与 Verbose 都发 ——
///   每桩完成吻的 fate 结算行、一切异常出口（受击散场/走位失败/中途离场/无人认领的表），
///   以及启动翻译自证。一吻至多几条，发行包常开也不吵。</description></item>
///   <item><description>叙事（Dev，<c>[MWAH] dev:</c>）：只有 Verbose 发 —— 阶段流水与点选代际，
///   是"看着它跑"的调试面；KissTrace 的 1 Hz 采样器同档。</description></item>
///   <item><description>故障（Warn/Error）：不受档位管 —— 它们不是诊断噪音而是"模组自己出事了"，
///   把它静音等于制造静默失败（本项目最恨的东西）。</description></item>
/// </list>
/// Auto 的渠道解析只改返回值、不改任何成员的存废 —— 三渠道编译面一致（09-21 渠道纪律）。
/// </summary>
public static class MwahLog
{
    /// <summary>生效档位：设置说 Auto 才看构建渠道，否则设置说了算。</summary>
    public static MwahDiag Level
    {
        get
        {
            MwahDiag setting = MwahMod.Settings.DiagSetting;
            if (setting != MwahDiag.Auto)
            {
                return setting;
            }
#if MWAH_DEV
            return MwahDiag.Verbose;
#else
            return MwahDiag.Simple;
#endif
        }
    }

    /// <summary>要事层是否播报（Simple 及以上）。</summary>
    public static bool Notable => Level >= MwahDiag.Simple;

    /// <summary>叙事层是否播报（仅 Verbose）；KissTrace 采样器共用此闸。</summary>
    public static bool Detail => Level >= MwahDiag.Verbose;

    /// <summary>启动横幅专用：无条件、醒目、可全局检索。</summary>
    public static void Banner(string message) => Log.Message(Constants.LogPrefix + message);

    public static void Info(string message) => Log.Message(Constants.LogPrefix + message);

    public static void Warn(string message) => Log.Warning(Constants.LogPrefix + message);

    public static void Error(string message) => Log.Error(Constants.LogPrefix + message);

    /// <summary>要事：结算与异常出口。参数在调用点已求值，所以这里只挂"一桩一次"级别的事件。</summary>
    public static void Note(string message)
    {
        if (!Notable)
        {
            return;
        }
        Log.Message(Constants.LogPrefix + "note: " + message);
    }

    /// <summary>叙事：阶段流水。开关没到 Verbose 就直接返回 —— 逐 tick 的采样走 KissTrace 的字段写入。</summary>
    public static void Dev(string message)
    {
        if (!Detail)
        {
            return;
        }
        Log.Message(Constants.LogPrefix + "dev: " + message);
    }
}
