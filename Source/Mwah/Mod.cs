using System;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 模组入口。零 Harmony：不 PatchAll、不依赖 brrainz.harmony，
/// 触发入口靠原版 FloatMenuOptionProvider 自动发现，行为靠自定义 JobDriver。
/// 设置持久化采用"内存即时生效 + 磁盘防抖合并"，失败时保留 dirty 并重试。
/// </summary>
public class MwahMod : Mod
{
    private const float SaveDebounceSeconds = 0.35f;
    private const float SaveRetrySeconds = 2f;

    /// <summary>
    /// 玩家配置。出厂就是一个装着默认值的实例，**永不为 null** —— 各判定入口因此不必
    /// 各写一遍"配置没读到就当禁用"。Mod 构造时它被 GetSettings&lt;MwahSettings&gt;()
    /// （已从 Settings.xml 恢复的真实配置）整个替换。
    /// </summary>
    public static MwahSettings Settings { get; private set; } = new MwahSettings();
    public static MwahMod? Instance { get; private set; }

    private MwahSettingsView? settingsView;
    private bool queuedWrite;
    private bool savePending;
    private float saveDueAt;
    private long requestedGeneration;
    private long persistedGeneration;
    private long failedGeneration = -1;

#if MWAH_STEAM
    internal const string BuildFlavor = "steam";
#elif MWAH_GITHUB
    internal const string BuildFlavor = "github";
#else
    internal const string BuildFlavor = "dev";
#endif

    public MwahMod(ModContentPack content) : base(content)
    {
        // 启动横幅：无条件播报（诊断开关关掉也要能确认模组活着），格式固定
        // "[MWAH] <模组名> v<版本> build=<渠道> startup OK/FAILED"，全局检索 [MWAH] 即得。
        // FAILED 的成因 = 入口自身炸了（设置反序列化、路径准备）；播完横幅把异常原样抛回，
        // 让原版照常记堆栈 —— 半初始化的模组不能装作活着。
        try
        {
            Instance = this;
            Settings = GetSettings<MwahSettings>();
            KissTrace.Start();
            MwahLog.Banner($"Mwah! (Every Pawn Kisses Each Other) v{VersionString()} build={BuildFlavor} startup OK");
        }
        catch (Exception ex)
        {
            MwahLog.Banner($"Mwah! (Every Pawn Kisses Each Other) v{VersionString()} build={BuildFlavor} startup FAILED: {ex.GetBaseException().Message}");
            throw;
        }
    }

    /// <summary>返回非空字符串是设置页在"模式选项"里出现的唯一条件。</summary>
    public override string SettingsCategory() => "MWAH.SettingsCategory".Translate();

    public override void DoSettingsWindowContents(Rect inRect)
    {
        TickQueuedSave();
        (settingsView ??= new MwahSettingsView(Settings)).DoSettingsWindowContents(inRect);
    }

    // ---- 提交／保存的四个动作，各有唯一入口；代次就是它们之间的账 ----
    // 提交 = RequestSettingsSave：规范值已经改在内存里，只登记待落盘代次并重置防抖窗口，不碰磁盘。
    // 尝试 = FlushSettingsSave：静默窗口结束后的一次真实写盘调用。
    // 成功 = MarkSettingsPersisted：唯一的持久代次记账点。
    // 失败 = 记 failedGeneration 并推迟重试，dirty 原样保留 —— 绝不冒称已保存（USE-02）。
    // 防抖时长、重试时长、提交时机与玩家数值全部照旧，这里只把已有的分工写成可辨认的入口。

    /// <summary>
    /// 提交入口：设置页已经把改动落进内存规范值（USE-01 的"立即生效"），
    /// 这里只登记"有新一代待落盘"并重置防抖窗口。
    /// </summary>
    public void RequestSettingsSave()
    {
        requestedGeneration++;
        savePending = true;
        saveDueAt = Time.realtimeSinceStartup + SaveDebounceSeconds;
    }

    /// <summary>防抖闸：由设置页每帧驱动，到点且确实还有未落盘代次时才发起一次尝试。</summary>
    private void TickQueuedSave()
    {
        if (!savePending || Time.realtimeSinceStartup < saveDueAt)
        {
            return;
        }
        // dirty 判定基于"是否还有未落盘的代次"，而不是"控件是否被碰过"。
        if (requestedGeneration == persistedGeneration && failedGeneration != requestedGeneration)
        {
            savePending = false;
            return;
        }
        FlushSettingsSave();
    }

    /// <summary>保存尝试：成功与否由代次记账，不靠"调用过 WriteSettings"自证。</summary>
    private void FlushSettingsSave()
    {
        try
        {
            // queuedWrite 只说明"这次 WriteSettings 来自内部防抖 flush"：
            // 原版入口会在关设置页时先提交编辑缓冲，而我们的自动保存绝不能替玩家提前提交
            // 还打在键盘上的数值框（USE-02）。
            queuedWrite = true;
            WriteSettings();
            MwahLog.Dev($"settings saved (generation {persistedGeneration})");
        }
        catch (Exception ex)
        {
            // 保存失败不能清掉 dirty：留着下次重试，并把原因告诉开发者。
            failedGeneration = requestedGeneration;
            saveDueAt = Time.realtimeSinceStartup + SaveRetrySeconds;
            MwahLog.Error($"settings save failed (generation {requestedGeneration}): {ex}");
        }
        finally { queuedWrite = false; }
    }

    public override void WriteSettings()
    {
        if (!queuedWrite) settingsView?.Close();
        base.WriteSettings();
        MarkSettingsPersisted();
    }

    /// <summary>
    /// 持久成功的唯一记账点：磁盘上现在就是最新请求的那一代。
    /// 两条路径（游戏自己的保存／设置页的防抖 flush）都汇到这里，不再各写一份同样的赋值。
    /// </summary>
    private void MarkSettingsPersisted()
    {
        persistedGeneration = requestedGeneration;
        savePending = false;
    }

    /// <summary>
    /// 横幅与设置页共用的版本身份（2026-10-05 维护者裁定，机制同 UniversalSqueaker）：
    /// dev 包带完整 commit hash——同版本号的不同构建必须能在日志里分辨（"打包必构建"
    /// 事故的账本）；Steam/GitHub 发行包只报版本号——+sha 是构建账本，不是玩家信息。
    /// #if 只改返回值不改成员存废，三渠道编译面一致（渠道纪律）。
    /// </summary>
    internal static string VersionString()
    {
        string? informational = typeof(MwahMod).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        if (string.IsNullOrEmpty(informational))
        {
            return "unknown";
        }
#if MWAH_DEV
        return informational!;
#else
        int plus = informational!.IndexOf('+');
        return plus < 0 ? informational : informational[..plus];
#endif
    }
}
