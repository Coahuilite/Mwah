using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 玩家配置。生命周期：immediate canonical + coalesced persistence。
/// 控件改动立刻成为唯一权威值并即时生效；磁盘写入由 <see cref="MwahMod"/> 防抖合并，关窗时强制 flush。
/// 所有时长按 tick 存储，界面同时给出 tick / 现实秒 / 游戏小时三种读法。
/// </summary>
public class MwahSettings : ModSettings
{

    // 出厂默认来自 Constants；玩家改动只落在这些字段上，Scribe key 与字段名一致。
    public bool modEnabled = Constants.ModEnabled;
    public int pairScope = Constants.PairScopeDefault;
    public bool changeOpinion = Constants.ChangeOpinion;
    public bool returnHomeAfterKiss = Constants.ReturnHomeAfterKiss;
    public int kissDurationTicks = Constants.KissDurationTicks;
    public int heartFleckIntervalTicks = Constants.HeartFleckIntervalTicks;
    public int pawnCooldownTicks = Constants.PawnCooldownTicks;
    public int pairCooldownTicks = Constants.PairCooldownTicks;
    public bool directorButton = Constants.DirectorButton;
    public bool wallKissing = Constants.WallKissing;
    public bool noCooldowns = Constants.NoCooldowns;
    public bool autonomousKissing = Constants.AutonomousKissing;
    public int autonomousIntervalTicks = Constants.AutonomousIntervalTicks;
    public int autonomousRadiusCells = Constants.AutonomousRadiusCells;
    public int thoughtDurationTicks = Constants.ThoughtDurationTicks;
    public float moodMultiplier = Constants.MoodMultiplier;
    public bool diagnosticLogs = Constants.DiagnosticLogs;

    private Vector2 scrollPos;

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
    public bool WallKissingEnabled => wallKissing;
    public bool CooldownsIgnored => noCooldowns;
    public bool AutonomousEnabled => autonomousKissing;
    public int AutonomousIntervalTicks => Mathf.Clamp(autonomousIntervalTicks, Constants.AutonomousIntervalTicksRange.min, Constants.AutonomousIntervalTicksRange.max);
    public int AutonomousRadius => Mathf.Clamp(autonomousRadiusCells, Constants.AutonomousRadiusRange.min, Constants.AutonomousRadiusRange.max);
    /// <summary>诊断总闸：MwahLog.Dev 与 KissTrace 采样共用；横幅与 Warn/Error 不归它管。</summary>
    public bool DiagnosticsEnabled => diagnosticLogs;

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
        Scribe_Values.Look(ref wallKissing, "wallKissing", Constants.WallKissing);
        Scribe_Values.Look(ref noCooldowns, "noCooldowns", Constants.NoCooldowns);
        Scribe_Values.Look(ref autonomousKissing, "autonomousKissing", Constants.AutonomousKissing);
        Scribe_Values.Look(ref autonomousIntervalTicks, "autonomousIntervalTicks", Constants.AutonomousIntervalTicks);
        Scribe_Values.Look(ref autonomousRadiusCells, "autonomousRadiusCells", Constants.AutonomousRadiusCells);
        Scribe_Values.Look(ref thoughtDurationTicks, "thoughtDurationTicks", Constants.ThoughtDurationTicks);
        Scribe_Values.Look(ref moodMultiplier, "moodMultiplier", Constants.MoodMultiplier);
        Scribe_Values.Look(ref diagnosticLogs, "diagnosticLogs", Constants.DiagnosticLogs);

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
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
        wallKissing = Constants.WallKissing;
        noCooldowns = Constants.NoCooldowns;
        autonomousKissing = Constants.AutonomousKissing;
        autonomousIntervalTicks = Constants.AutonomousIntervalTicks;
        autonomousRadiusCells = Constants.AutonomousRadiusCells;
        thoughtDurationTicks = Constants.ThoughtDurationTicks;
        moodMultiplier = Constants.MoodMultiplier;
        diagnosticLogs = Constants.DiagnosticLogs;
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        bool changed = false;
        var viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height);
        // 自动测高：先用窗口高度开滚动，list 结束后用真实内容高度回写 viewRect。
        // 固定 ContentHeight 的写法在控件增删或译文变长后会截断底部控件。
        // 可见轨道：BeginScrollView 只画滑块、滑块在暗底上近乎隐形（实机裁定"滚动条完全看不到"），
        // 先垫一条低透明黑轨，让"这里能滚"看得出来。
        Widgets.DrawBoxSolid(new Rect(inRect.xMax - 15f, inRect.y, 15f, inRect.height), new Color(0f, 0f, 0f, 0.3f));
        Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
        var list = new Listing_Standard(GameFont.Small);
        list.ColumnWidth = viewRect.width - 24f;
        list.Begin(viewRect);

        list.Label("MWAH.Settings.Header".Translate());
        list.GapLine();
        changed |= Checkbox(list, ref modEnabled, "MWAH.Settings.Enabled", "MWAH.Settings.EnabledDesc");
        list.Gap();

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

        list.GapLine();
        // 亲墙开关刻意放在门禁滑条（pairScope，在下面几行）之外的独立位置：
        // 它管的是"墙算不算可亲对象"，与"谁能亲谁"的七档范围是两回事。
        changed |= Checkbox(list, ref wallKissing, "MWAH.Settings.WallKissing", "MWAH.Settings.WallKissingDesc");
        changed |= Checkbox(list, ref directorButton, "MWAH.Settings.DirectorButton", "MWAH.Settings.DirectorButtonDesc");
        changed |= Checkbox(list, ref noCooldowns, "MWAH.Settings.NoCooldowns", "MWAH.Settings.NoCooldownsDesc");
        // 范围档位是"档"不是"量"：输入 3 没有意义，所以这一条滑条刻意不带数值框。
        changed |= IntSliderRow(list, ref pairScope, "MWAH.Settings.PairScope", "MWAH.Settings.PairScopeDesc",
            Constants.PairScopeRange, raw => KissScopeUtility.Label(KissScopeUtility.Clamp(raw)), withField: false);
        changed |= Checkbox(list, ref autonomousKissing, "MWAH.Settings.Autonomous", "MWAH.Settings.AutonomousDesc");
        changed |= IntSlider(list, ref autonomousIntervalTicks, "MWAH.Settings.AutonomousInterval",
            "MWAH.Settings.AutonomousIntervalDesc", Constants.AutonomousIntervalTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref autonomousRadiusCells, "MWAH.Settings.AutonomousRadius",
            "MWAH.Settings.AutonomousRadiusDesc", Constants.AutonomousRadiusRange, cells => cells.ToString());
        changed |= Checkbox(list, ref changeOpinion, "MWAH.Settings.ChangeOpinion", "MWAH.Settings.ChangeOpinionDesc");
        changed |= Checkbox(list, ref returnHomeAfterKiss, "MWAH.Settings.ReturnHome", "MWAH.Settings.ReturnHomeDesc");
        list.GapLine();
        changed |= Checkbox(list, ref diagnosticLogs, "MWAH.Settings.Diagnostics", "MWAH.Settings.DiagnosticsDesc");

        list.Gap();
        list.Label("MWAH.Settings.TimingHint".Translate());
        list.Gap();
        if (list.ButtonText("MWAH.Settings.Reset".Translate()))
        {
            RestoreDefaults();
            changed = true;
        }
        list.End();
        viewRect.height = list.CurHeight + 16f;
        Widgets.EndScrollView();
        if (changed)
        {
            MwahMod.Instance?.RequestSettingsSave();
        }
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
        IntRange range, Func<int, string> showAs, int step = 1)
        => IntSliderRow(list, ref value, labelKey, tipKey, range, showAs, withField: true, step);

    private bool IntSliderRow(Listing_Standard list, ref int value, string labelKey, string tipKey,
        IntRange range, Func<int, string> showAs, bool withField, int step = 1)
    {
        int before = value;
        string label = labelKey.Translate() + ": " + showAs(value);
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
        value = (int)NumericField(labelKey, new Rect(row.xMax - FieldWidth, row.y + 22f, FieldWidth, 24f),
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
