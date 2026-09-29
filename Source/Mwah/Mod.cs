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

    private bool savePending;
    private float saveDueAt;
    private long requestedGeneration;
    private long persistedGeneration;
    private long failedGeneration = -1;

#if MWAH_STEAM
    private const string BuildFlavor = "steam";
#elif MWAH_GITHUB
    private const string BuildFlavor = "github";
#else
    private const string BuildFlavor = "dev";
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
            MwahLog.Banner($"Mwah! (Every Pawn Kisses Each Other) build={BuildFlavor} startup FAILED: {ex.GetBaseException().Message}");
            throw;
        }
    }

    /// <summary>返回非空字符串是设置页在"模式选项"里出现的唯一条件。</summary>
    public override string SettingsCategory() => "MWAH.SettingsCategory".Translate();


    public override void DoSettingsWindowContents(Rect inRect)
    {
        TickQueuedSave();
        Settings.DoSettingsWindowContents(inRect);
    }

    /// <summary>设置页在拖 slider 时被高频调用，这里只在静默窗口期结束后真正落盘一次。</summary>
    public void RequestSettingsSave()
    {
        requestedGeneration++;
        savePending = true;
        saveDueAt = Time.realtimeSinceStartup + SaveDebounceSeconds;
    }

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

    private void FlushSettingsSave()
    {
        try
        {
            WriteSettings();
            persistedGeneration = requestedGeneration;
            savePending = false;
            MwahLog.Dev($"settings saved (generation {persistedGeneration})");
        }
        catch (Exception ex)
        {
            // 保存失败不能清掉 dirty：留着下次重试，并把原因告诉开发者。
            failedGeneration = requestedGeneration;
            saveDueAt = Time.realtimeSinceStartup + SaveRetrySeconds;
            MwahLog.Error($"settings save failed (generation {requestedGeneration}): {ex}");
        }
    }

    public override void WriteSettings()
    {
        base.WriteSettings();
        persistedGeneration = requestedGeneration;
        savePending = false;
    }

    private static string VersionString()
    {
        string? informational = typeof(MwahMod).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        return string.IsNullOrEmpty(informational) ? "unknown" : informational!;
    }
}
