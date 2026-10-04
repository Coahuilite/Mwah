using System;
using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 玩家配置。生命周期：immediate canonical + coalesced persistence。
/// 控件改动立刻成为唯一权威值并即时生效；磁盘写入由 <see cref="MwahMod"/> 防抖合并，关窗时强制 flush。
/// 所有时长按 tick 存储，界面同时给出 tick / 现实秒 / 游戏小时三种读法。
///
/// 页面分四段（2026-10-05 维护者裁定）：核心 / 自主撮合 / 附加功能 / 系统。
/// 附加功能段按 <see cref="KissThingAddons.All"/> 注册表生长——新 addon 在这里自动长出
/// 自己的开关与思想时长行，设置类零改动；两份按 Id 索引的字典
/// （<see cref="addonSwitches"/>、<see cref="addonThoughtDurations"/>）就是它的存储。
/// </summary>
public class MwahSettings : ModSettings
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
    /// 它取代了旧的 wallKissing 裸字段（0.2.0 首发前，不留兼容）。
    /// </summary>
    public Dictionary<string, bool> addonSwitches = new();

    private Vector2 scrollPos;

    /// <summary>上一帧量出的内容高度（跨帧缓存，见 DoSettingsWindowContents 顶注）。不存盘。</summary>
    private float scrollContentHeight;

    // 数值输入框的编辑态：同一时刻最多一个框在编辑（IMGUI 的 keyboardControl 本来就互斥）。
    private string? editingField;
    private string editBuf = "";

    /// <summary>滑条行里右侧数值框的宽度与间距（逻辑像素，不随分辨率缩）。</summary>
    private const float FieldWidth = 74f;
    private const float FieldGap = 6f;

    /// <summary>滑条标签/控件的分栏宽度（控件占 62%，标签占剩下的）。</summary>
    private const float SliderLabelWidth = 0.62f;

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

    /// <summary>档位名键：枚举拼出来的，语言文件键集合由 verify-local 反向核对。</summary>
    private static string DiagLabel(MwahDiag level) => ("MWAH.Settings.Diag." + level).Translate();

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

    public void DoSettingsWindowContents(Rect inRect)
    {
        bool changed = false;
        // 固定身份行（2026-10-05 维护者裁定）：vanilla 的标题行只画 SettingsCategory
        // （Dialog_ModSettings.DoWindowContents 反编译实锤），版本进不了那一行——除非 Harmony，
        // 那是硬边界。所以在自己区域顶部常驻一行"品牌 · 版本 · 渠道"，滚动区从它下面开始：
        // 滚到哪里截图都带着构建身份。字符串与启动横幅同源（MwahMod.VersionString()）。
        Rect identityRect = inRect.TopPartPixels(24f);
        Rect scrollRect = inRect.BottomPartPixels(inRect.height - 24f);
        DrawIdentityRow(identityRect);
        float viewWidth = scrollRect.width - 16f;
        // 滚动范围用**上一帧量出的内容高度**：viewRect 是局部变量，"End 之后回写高度"
        // 对本帧的滚动条毫无作用（2026-09-10 的自动测高就是这么坏的：范围恒等于窗口高，
        // 滚不动，溢出内容还被 Listing_Standard 换列画到窗外右侧——"恢复默认值"飘出窗口）。
        // 跨帧缓存后第 2 帧起范围即真实内容高度；首帧最多少滚一屏，无感。
        var viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(scrollContentHeight, scrollRect.height));
        // 可见轨道：BeginScrollView 只画滑块、滑块在暗底上近乎隐形（实机裁定"滚动条完全看不到"），
        // 先垫一条低透明黑轨，让"这里能滚"看得出来。
        Widgets.DrawBoxSolid(new Rect(scrollRect.xMax - 15f, scrollRect.y, 15f, scrollRect.height), new Color(0f, 0f, 0f, 0.3f));
        Widgets.BeginScrollView(scrollRect, ref scrollPos, viewRect);
        var list = new Listing_Standard(GameFont.Small);
        list.ColumnWidth = viewWidth - 24f;
        // 单列铁律：Begin 的 maxRect.height 给到近乎无穷，"装不下就换列"永不触发。
        // 换列 = 溢出内容画到右边窗外（实机截图里被挤到另一侧、再也点不到的设置项）。
        list.Begin(new Rect(0f, 0f, viewWidth, 100000f));

        list.Label("MWAH.Settings.Header".Translate());
        list.GapLine();
        changed |= Checkbox(list, ref modEnabled, "MWAH.Settings.Enabled", "MWAH.Settings.EnabledDesc");

        // ===== 段一：核心（双人吻的全部旋钮 + 全局入口开关）=====
        Section(list, "MWAH.Settings.Section.Core");
        // 范围档位是"档"不是"量"：输入 3 没有意义，所以这一条滑条刻意不带数值框。
        changed |= IntSliderRow(list, ref pairScope, "MWAH.Settings.PairScope", "MWAH.Settings.PairScopeDesc",
            Constants.PairScopeRange, raw => KissScopeUtility.Label(KissScopeUtility.Clamp(raw)), withField: false);
        changed |= IntSlider(list, ref kissDurationTicks, "MWAH.Settings.Duration", "MWAH.Settings.DurationDesc",
            Constants.DurationTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref heartFleckIntervalTicks, "MWAH.Settings.FleckInterval", "MWAH.Settings.FleckIntervalDesc",
            Constants.FleckIntervalTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref pawnCooldownTicks, "MWAH.Settings.PawnCooldown", "MWAH.Settings.PawnCooldownDesc",
            Constants.CooldownTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref pairCooldownTicks, "MWAH.Settings.PairCooldown", "MWAH.Settings.PairCooldownDesc",
            Constants.CooldownTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref thoughtDurationTicks, "MWAH.Settings.ThoughtDuration",
            "MWAH.Settings.ThoughtDurationDesc", Constants.ThoughtDurationTicksRange, MwahTime.FormatTicks,
            Constants.ThoughtDurationStepTicks);
        changed |= FloatSlider(list, ref moodMultiplier, "MWAH.Settings.MoodMultiplier", "MWAH.Settings.MoodMultiplierDesc",
            Constants.MoodMultiplierRange, Constants.MoodMultiplierStep, v => v.ToString("0.##") + "x");
        changed |= Checkbox(list, ref changeOpinion, "MWAH.Settings.ChangeOpinion", "MWAH.Settings.ChangeOpinionDesc");
        changed |= Checkbox(list, ref returnHomeAfterKiss, "MWAH.Settings.ReturnHome", "MWAH.Settings.ReturnHomeDesc");
        changed |= Checkbox(list, ref noCooldowns, "MWAH.Settings.NoCooldowns", "MWAH.Settings.NoCooldownsDesc");
        changed |= Checkbox(list, ref directorButton, "MWAH.Settings.DirectorButton", "MWAH.Settings.DirectorButtonDesc");

        // ===== 段二：自主撮合（系统替玩家点的鸳鸯）=====
        Section(list, "MWAH.Settings.Section.Autonomous");
        changed |= Checkbox(list, ref autonomousKissing, "MWAH.Settings.Autonomous", "MWAH.Settings.AutonomousDesc");
        changed |= IntSlider(list, ref autonomousIntervalTicks, "MWAH.Settings.AutonomousInterval",
            "MWAH.Settings.AutonomousIntervalDesc", Constants.AutonomousIntervalTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref autonomousRadiusCells, "MWAH.Settings.AutonomousRadius",
            "MWAH.Settings.AutonomousRadiusDesc", Constants.AutonomousRadiusRange, cells => cells.ToString());

        // ===== 段三：附加功能（注册表生长；每个 addon 一组开关+时长）=====
        Section(list, "MWAH.Settings.Section.Addons");
        for (int i = 0; i < KissThingAddons.All.Length; i++)
        {
            KissThingAddon addon = KissThingAddons.All[i];
            changed |= AddonSwitchRow(list, addon);
            changed |= AddonDurationRow(list, addon);
        }

        // ===== 段四：系统（诊断与恢复）=====
        Section(list, "MWAH.Settings.Section.System");
        // 档位是"档"不是"量"：输入 2 没有意义，滑条刻意不带数值框（与门禁滑条同一先例）。
        changed |= IntSliderRow(list, ref diagnosticLevel, "MWAH.Settings.DiagLevel", "MWAH.Settings.DiagLevelDesc",
            Constants.DiagnosticLevelRange, raw => DiagLabel((MwahDiag)Mathf.Clamp(raw, (int)MwahDiag.Off, (int)MwahDiag.Verbose)), withField: false);
        list.Gap();
        list.Label("MWAH.Settings.TimingHint".Translate());
        list.Gap();
        if (list.ButtonText("MWAH.Settings.Reset".Translate()))
        {
            RestoreDefaults();
            changed = true;
        }
        list.End();
        scrollContentHeight = list.CurHeight + 16f;
        Widgets.EndScrollView();
        if (changed)
        {
            MwahMod.Instance?.RequestSettingsSave();
        }
    }

    /// <summary>身份行：Tiny、半透明、不可交互——它是水印不是控件，读的是横幅同一份真相。</summary>
    private static void DrawIdentityRow(Rect row)
    {
        GameFont fontBefore = Text.Font;
        TextAnchor anchorBefore = Text.Anchor;
        Color colorBefore = GUI.color;
        Text.Font = GameFont.Tiny;
        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = new Color(1f, 1f, 1f, 0.55f);
        Widgets.Label(row, "Mwah!  ·  v" + MwahMod.VersionString() + "  ·  build=" + MwahMod.BuildFlavor);
        GUI.color = colorBefore;
        Text.Anchor = anchorBefore;
        Text.Font = fontBefore;
    }

    /// <summary>段标题：一条分隔线 + 加粗感的裸标签（键双语齐备由门把关）。</summary>
    private static void Section(Listing_Standard list, string labelKey)
    {
        list.GapLine();
        list.Label(labelKey.Translate());
    }

    /// <summary>addon 开关行：读写都走字典（缺槽落 DefaultActive），写只在真被改过时发生。</summary>
    private bool AddonSwitchRow(Listing_Standard list, KissThingAddon addon)
    {
        bool before = MwahMod.Settings.AddonSwitch(addon.Id, addon.DefaultActive);
        bool value = before;
        bool touched = Checkbox(list, ref value, addon.SwitchLabelKey, addon.SwitchDescKey);
        if (touched && value != before)
        {
            MwahMod.Settings.SetAddonSwitch(addon.Id, value);
        }
        return touched && value != before;
    }

    /// <summary>
    /// addon 思想时长行：标签带 addon 名（{0}），数值框的编辑态键必须逐 addon 唯一
    /// （fieldId=Id —— 否则两个 addon 共用一个输入框焦点，editBuf 会串台）。
    /// </summary>
    private bool AddonDurationRow(Listing_Standard list, KissThingAddon addon)
    {
        int before = MwahMod.Settings.AddonThoughtDuration(addon.Id, addon.DefaultThoughtDurationTicks);
        int value = before;
        bool touched = IntSlider(list, ref value, "MWAH.Settings.AddonDuration", "MWAH.Settings.AddonDurationDesc",
            Constants.AddonDurationTicksRange, MwahTime.FormatTicks, Constants.ThoughtDurationStepTicks,
            labelArg: addon.NameKey.Translate(), fieldId: addon.Id);
        if (touched && value != before)
        {
            MwahMod.Settings.SetAddonThoughtDuration(addon.Id, value);
        }
        return touched && value != before;
    }

    private static bool Checkbox(Listing_Standard list, ref bool value, string labelKey, string tipKey)
    {
        bool before = value;
        list.CheckboxLabeled(labelKey.Translate(), ref value, tipKey.Translate());
        return before != value;
    }

    /// <summary>
    /// 整数滑条（带数值框）。tick 项的框编辑的就是 tick —— 存储单位即输入单位，
    /// 三读法只出现在标签上，符合"时长只以 tick 为存储单位"的硬边界。
    /// </summary>
    private bool IntSlider(Listing_Standard list, ref int value, string labelKey, string tipKey,
        IntRange range, Func<int, string> showAs, int step = 1, string? labelArg = null, string? fieldId = null)
        => IntSliderRow(list, ref value, labelKey, tipKey, range, showAs, withField: true, step, labelArg, fieldId);

    private bool IntSliderRow(Listing_Standard list, ref int value, string labelKey, string tipKey,
        IntRange range, Func<int, string> showAs, bool withField, int step = 1, string? labelArg = null, string? fieldId = null)
    {
        int before = value;
        string label = (labelArg == null ? labelKey.Translate() : labelKey.Translate(labelArg)) + ": " + showAs(value);
        string tip = tipKey.Translate();
        if (!withField)
        {
            float rawScope = list.SliderLabeled(label, value, range.min, range.max, SliderLabelWidth, tip);
            value = Mathf.RoundToInt(rawScope);
            return before != value;
        }
        Rect row = list.GetRect(46f);
        DrawSliderLabel(row, label, tip);
        Rect slider = new Rect(row.x, row.y + 24f, row.width - FieldWidth - FieldGap, 20f);
        float raw = Widgets.HorizontalSlider(slider, value, range.min, range.max);
        value = Mathf.RoundToInt(raw / step) * step; // 滑条连续、落值量化：step 才是允许的刻度
        value = (int)NumericField(labelKey + (fieldId ?? ""), new Rect(row.xMax - FieldWidth, row.y + 22f, FieldWidth, 24f),
            value, range.min, range.max, step);
        return before != value;
    }

    /// <summary>
    /// 小数滑条（带数值框），落值量化到 <paramref name="step"/> 的整数倍：小于这个跨度的改动体感上没有区别，
    /// 却会在配置文件里留下无意义的长小数（0.25 游戏时、0.05 倍都是这么定的）。
    /// </summary>
    private bool FloatSlider(Listing_Standard list, ref float value, string labelKey, string tipKey,
        FloatRange range, float step, Func<float, string> showAs)
    {
        float before = value;
        string label = labelKey.Translate() + ": " + showAs(value);
        Rect row = list.GetRect(46f);
        DrawSliderLabel(row, label, tipKey.Translate());
        Rect slider = new Rect(row.x, row.y + 24f, row.width - FieldWidth - FieldGap, 20f);
        float raw = Widgets.HorizontalSlider(slider, value, range.min, range.max);
        value = Mathf.Round(raw / step) * step;
        value = NumericField(labelKey, new Rect(row.xMax - FieldWidth, row.y + 22f, FieldWidth, 24f),
            value, range.min, range.max, step);
        return !Mathf.Approximately(before, value);
    }

    private static void DrawSliderLabel(Rect row, string label, string tip)
    {
        Rect labelRect = new Rect(row.x, row.y, row.width, 20f);
        Widgets.Label(labelRect, label);
        TooltipHandler.TipRegion(labelRect, tip);
    }

    /// <summary>
    /// 数值输入框：点进即编辑（IMGUI 自己管焦点），回车或失焦提交；提交按 step 量化并夹进量程，
    /// 解析失败（空串、乱码）则整次编辑作废、回显当前值。解析用不变文化（InvariantCulture），
    /// 与 Settings.xml 的写法同一口径，不受系统小数点逗号影响。
    /// </summary>
    private float NumericField(string key, Rect rect, float value, float min, float max, float step)
    {
        string controlName = "MWAH_Field_" + key;
        GUI.SetNextControlName(controlName);
        string shown = editingField == key ? editBuf
            : value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string text = Widgets.TextField(rect, shown, 12);
        bool focused = GUI.GetNameOfFocusedControl() == controlName;
        if (focused && editingField != key)
        {
            editingField = key; // 用户点进来了：以当前值起编
            editBuf = shown;
        }
        if (editingField != key)
        {
            return value;
        }
        editBuf = text;
        bool enter = Event.current.type == EventType.KeyDown
            && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
        if (enter || !focused)
        {
            editingField = null;
            GUI.FocusControl(null);
            if (float.TryParse(editBuf, System.Globalization.NumberStyles.Float,
                    System.Globalization.CultureInfo.InvariantCulture, out float parsed))
            {
                value = Mathf.Round(parsed / step) * step;
                value = Mathf.Clamp(value, min, max);
            }
        }
        return value;
    }
}
