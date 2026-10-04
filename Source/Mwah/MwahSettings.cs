using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 玩家配置（存储与生命周期半边；渲染在 MwahSettingsUI.cs，partial 同类）。
/// 生命周期：immediate canonical + coalesced persistence。
/// 控件改动立刻成为唯一权威值并即时生效；磁盘写入由 <see cref="MwahMod"/> 防抖合并，关窗时强制 flush。
/// 所有时长按 tick 存储，界面同时给出 tick / 现实秒 / 游戏小时三种读法。
///
/// 命名四方格（2026-10-05 裁定）：字段名 = Scribe key = Restore 目标 = 语言键词根
/// （`MWAH.Settings.&lt;Pascal(字段)&gt;`[Desc]，由 UI 侧 CallerArgumentExpression 派生，
/// verify-local 门展开核对双语）。新增一项设置：字段 + Scribe + Restore + 一行 UI 调用
/// （零字符串）+ 双语各一键，全部同词，grep 一词贯穿。
///
/// 页面分四段（2026-10-05 维护者裁定）：核心 / 自主撮合 / 附加功能 / 系统。
/// 附加功能段按 <see cref="KissThingAddons.All"/> 注册表生长——新 addon 自动长出
/// 自己的开关与思想时长行，设置类零改动；两份按 Id 索引的字典
/// （<see cref="addonSwitches"/>、<see cref="addonThoughtDurations"/>）就是它的存储。
/// </summary>
public partial class MwahSettings : ModSettings
{
    public bool modEnabled = Constants.ModEnabled;
    public int pairScope = Constants.PairScopeDefault;
    public bool changeOpinion = Constants.ChangeOpinion;
    public bool returnHomeAfterKiss = Constants.ReturnHomeAfterKiss;
    public int kissDurationTicks = Constants.KissDurationTicks;
    public int heartFleckIntervalTicks = Constants.HeartFleckIntervalTicks;
    public int pawnCooldownTicks = Constants.PawnCooldownTicks;
    public int pairCooldownTicks = Constants.PairCooldownTicks;
    public bool directorButton = Constants.DirectorButton;
    public bool noCooldowns = Constants.NoCooldowns;
    public bool autonomousKissing = Constants.AutonomousKissing;
    public int autonomousIntervalTicks = Constants.AutonomousIntervalTicks;
    public int autonomousRadiusCells = Constants.AutonomousRadiusCells;
    public int thoughtDurationTicks = Constants.ThoughtDurationTicks;
    public float moodMultiplier = Constants.MoodMultiplier;
    public int diagnosticLevel = Constants.DiagnosticLevelDefault;

    /// <summary>
    /// addon 心情记忆时长，按 addon 稳定 Id 索引（"wall" → tick）。字典而不是散字段：
    /// 新 addon（树/工作台/机器）零设置类改动——注册表长出它的滑条，这里长出它的槽。
    /// 读侧一律 clamp；缺 Id = 用该 addon 声明的出厂值（<see cref="Constants.WallThoughtDurationTicks"/> 一类）。
    /// </summary>
    public Dictionary<string, int> addonThoughtDurations = new();

    /// <summary>
    /// addon 开关，按稳定 Id 索引（"wall" → 开/关）。与时长字典同一套理由：
    /// 开关是 addon 的事，不是设置类的事；缺槽 = 该 addon 的 DefaultActive。
    /// 它取代了旧的 wallKissing 裸字段（从未有版本发布，不留兼容）。
    /// </summary>
    public Dictionary<string, bool> addonSwitches = new();

    // 读侧统一 canonicalize：脏配置或手改的 XML 不能把运行时带进非法区间。
    // 所有时长项存的都是 tick（硬边界），界面用 MwahTime.FormatTicks 一次给出三种读法。
    public bool Enabled => modEnabled;
    public KissScope Scope => KissScopeUtility.Clamp(pairScope);
    public bool OpinionAffected => changeOpinion;
    public bool ReturnsHome => returnHomeAfterKiss;
    public int DurationTicks => Mathf.Clamp(kissDurationTicks, Constants.DurationTicksRange.min, Constants.DurationTicksRange.max);
    public int FleckIntervalTicks => Mathf.Clamp(heartFleckIntervalTicks, Constants.FleckIntervalTicksRange.min, Constants.FleckIntervalTicksRange.max);
    public int PawnCooldown => Mathf.Clamp(pawnCooldownTicks, Constants.CooldownTicksRange.min, Constants.CooldownTicksRange.max);
    public int PairCooldown => Mathf.Clamp(pairCooldownTicks, Constants.CooldownTicksRange.min, Constants.CooldownTicksRange.max);
    public int ThoughtDurationTicks => Mathf.Clamp(thoughtDurationTicks, Constants.ThoughtDurationTicksRange.min, Constants.ThoughtDurationTicksRange.max);
    public float MoodMult => Mathf.Clamp(moodMultiplier, Constants.MoodMultiplierRange.min, Constants.MoodMultiplierRange.max);
    public bool DirectorEnabled => directorButton;
    public bool CooldownsIgnored => noCooldowns;
    public bool AutonomousEnabled => autonomousKissing;
    public int AutonomousIntervalTicks => Mathf.Clamp(autonomousIntervalTicks, Constants.AutonomousIntervalTicksRange.min, Constants.AutonomousIntervalTicksRange.max);
    public int AutonomousRadius => Mathf.Clamp(autonomousRadiusCells, Constants.AutonomousRadiusRange.min, Constants.AutonomousRadiusRange.max);
    /// <summary>诊断档位原始值（收进合法档）；生效级别由 MwahLog.Level 解析 Auto，采样器共用。</summary>
    public MwahDiag DiagSetting => (MwahDiag)Mathf.Clamp(diagnosticLevel, (int)MwahDiag.Off, (int)MwahDiag.Verbose);

    /// <summary>addon 时长的唯一读法：字典有 Id 则 clamp 取出，没有则用该 addon 声明的出厂值。</summary>
    public int AddonThoughtDuration(string addonId, int factoryDefault) =>
        addonThoughtDurations.TryGetValue(addonId, out int ticks)
            ? Mathf.Clamp(ticks, Constants.AddonDurationTicksRange.min, Constants.AddonDurationTicksRange.max)
            : factoryDefault;

    public void SetAddonThoughtDuration(string addonId, int ticks) => addonThoughtDurations[addonId] = ticks;

    /// <summary>addon 开关的唯一读法：缺槽落出厂值。</summary>
    public bool AddonSwitch(string addonId, bool factoryDefault) =>
        addonSwitches.TryGetValue(addonId, out bool on) ? on : factoryDefault;

    public void SetAddonSwitch(string addonId, bool on) => addonSwitches[addonId] = on;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref modEnabled, "modEnabled", Constants.ModEnabled);
        Scribe_Values.Look(ref pairScope, "pairScope", Constants.PairScopeDefault);
        Scribe_Values.Look(ref changeOpinion, "changeOpinion", Constants.ChangeOpinion);
        Scribe_Values.Look(ref returnHomeAfterKiss, "returnHomeAfterKiss", Constants.ReturnHomeAfterKiss);
        Scribe_Values.Look(ref kissDurationTicks, "kissDurationTicks", Constants.KissDurationTicks);
        Scribe_Values.Look(ref heartFleckIntervalTicks, "heartFleckIntervalTicks", Constants.HeartFleckIntervalTicks);
        Scribe_Values.Look(ref pawnCooldownTicks, "pawnCooldownTicks", Constants.PawnCooldownTicks);
        Scribe_Values.Look(ref pairCooldownTicks, "pairCooldownTicks", Constants.PairCooldownTicks);
        Scribe_Values.Look(ref directorButton, "directorButton", Constants.DirectorButton);
        Scribe_Values.Look(ref noCooldowns, "noCooldowns", Constants.NoCooldowns);
        Scribe_Values.Look(ref autonomousKissing, "autonomousKissing", Constants.AutonomousKissing);
        Scribe_Values.Look(ref autonomousIntervalTicks, "autonomousIntervalTicks", Constants.AutonomousIntervalTicks);
        Scribe_Values.Look(ref autonomousRadiusCells, "autonomousRadiusCells", Constants.AutonomousRadiusCells);
        Scribe_Values.Look(ref thoughtDurationTicks, "thoughtDurationTicks", Constants.ThoughtDurationTicks);
        Scribe_Values.Look(ref moodMultiplier, "moodMultiplier", Constants.MoodMultiplier);
        Scribe_Values.Look(ref diagnosticLevel, "diagnosticLevel", Constants.DiagnosticLevelDefault);
        // addon 两张字典：整表入档（LookMode.Value；string→int / string→bool 都是值类型）。
        Scribe_Collections.Look(ref addonThoughtDurations, "addonThoughtDurations", LookMode.Value, LookMode.Value);
        Scribe_Collections.Look(ref addonSwitches, "addonSwitches", LookMode.Value, LookMode.Value);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            // 手改/损坏的 XML 可能把整表置 null：先补回空表再归一，读侧永不判空。
            addonThoughtDurations ??= new();
            addonSwitches ??= new();
            Normalize();
        }
    }

    private void Normalize()
    {
        kissDurationTicks = DurationTicks;
        heartFleckIntervalTicks = FleckIntervalTicks;
        pawnCooldownTicks = PawnCooldown;
        pairCooldownTicks = PairCooldown;
        thoughtDurationTicks = ThoughtDurationTicks;
        moodMultiplier = MoodMult;
        pairScope = (int)Scope;
        autonomousIntervalTicks = AutonomousIntervalTicks;
        autonomousRadiusCells = AutonomousRadius;
        diagnosticLevel = (int)DiagSetting;
        // 字典逐项 clamp：手改 XML 不能把 addon 时长带出合法区间。
        foreach (string key in new List<string>(addonThoughtDurations.Keys))
        {
            addonThoughtDurations[key] = Mathf.Clamp(addonThoughtDurations[key],
                Constants.AddonDurationTicksRange.min, Constants.AddonDurationTicksRange.max);
        }
    }

    public void RestoreDefaults()
    {
        modEnabled = Constants.ModEnabled;
        pairScope = Constants.PairScopeDefault;
        changeOpinion = Constants.ChangeOpinion;
        returnHomeAfterKiss = Constants.ReturnHomeAfterKiss;
        kissDurationTicks = Constants.KissDurationTicks;
        heartFleckIntervalTicks = Constants.HeartFleckIntervalTicks;
        pawnCooldownTicks = Constants.PawnCooldownTicks;
        pairCooldownTicks = Constants.PairCooldownTicks;
        directorButton = Constants.DirectorButton;
        noCooldowns = Constants.NoCooldowns;
        autonomousKissing = Constants.AutonomousKissing;
        autonomousIntervalTicks = Constants.AutonomousIntervalTicks;
        autonomousRadiusCells = Constants.AutonomousRadiusCells;
        thoughtDurationTicks = Constants.ThoughtDurationTicks;
        moodMultiplier = Constants.MoodMultiplier;
        diagnosticLevel = Constants.DiagnosticLevelDefault;
        // 字典没有"逐字段的 Constants.*"可回，恢复默认 = 清空整表 ⇒ 全部读侧落回各 addon 出厂值。
        addonThoughtDurations.Clear();
        addonSwitches.Clear();
    }
}
