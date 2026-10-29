using System;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>Transient settings presentation, separate from serialized configuration.</summary>
public sealed class MwahSettingsView
{
    private readonly MwahSettings settings;
    private readonly Vector2[] scrollPositions = new Vector2[2];
    private readonly float[] contentHeights = new float[2];
    private readonly System.Collections.Generic.HashSet<string> expanded = new();
    private int page;
    private Action<float>? pendingCommit;
    private float editMin, editMax, editStep;

    public MwahSettingsView(MwahSettings settings) => this.settings = settings;

    // 数值输入框的编辑态：同一时刻最多一个框在编辑（IMGUI 的 keyboardControl 本来就互斥）。
    private string? editingField;
    private string editBuf = "";

    /// <summary>滑条行里右侧数值框的宽度与间距（逻辑像素，不随分辨率缩）。</summary>
    private const float FieldWidth = 74f;
    private const float FieldGap = 6f;

    public void DoSettingsWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Small;
        string watermark = "Mwah!  ·  v" + MwahMod.VersionString() + "  ·  build=" + MwahMod.BuildFlavor;
        float subtitleHeight = LineHeightWithPadding("MWAH.Settings.Header".Translate(), GameFont.Small, inRect.width);
        float headerHeight = subtitleHeight + LineHeightWithPadding(watermark, GameFont.Tiny, inRect.width) + 2f;
        DrawPinnedHeader(inRect.TopPartPixels(headerHeight), subtitleHeight, watermark);

        var master = new Listing_Standard(GameFont.Small);
        master.Begin(new Rect(inRect.x, inRect.y + headerHeight + 4f, inRect.width, 100000f));
        bool changed = Checkbox(master, ref settings.modEnabled);
        master.End();
        float bodyTop = inRect.y + headerHeight + master.CurHeight + 44f;
        var pane = new Rect(inRect.x, bodyTop, inRect.width, Mathf.Max(1f, inRect.yMax - bodyTop));
        Widgets.DrawMenuSection(pane);
        TabDrawer.DrawTabs(pane, new System.Collections.Generic.List<TabRecord>
        {
            new("MWAH.Settings.Tab.Gameplay".Translate(), () => SelectPage(0), page == 0),
            new("MWAH.Settings.Tab.Parameters".Translate(), () => SelectPage(1), page == 1),
        });
        Rect scrollRect = pane.ContractedBy(10f);
        float viewWidth = scrollRect.width - 16f;
        var viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(contentHeights[page], scrollRect.height));
        Widgets.DrawBoxSolid(new Rect(scrollRect.xMax - 15f, scrollRect.y, 15f, scrollRect.height), new Color(0f, 0f, 0f, 0.3f));
        Widgets.BeginScrollView(scrollRect, ref scrollPositions[page], viewRect);
        var list = new Listing_Standard(GameFont.Small) { ColumnWidth = viewWidth - 12f };
        list.Begin(new Rect(0f, 0f, viewWidth, 100000f));
        if (page == 0)
        {
            Section(list, "MWAH.Settings.Section.Addons");
            foreach (KissThingAddon addon in KissThingAddons.All) changed |= AddonRow(list, addon);
            Section(list, "MWAH.Settings.Section.Core");
            changed |= TierSlider(list, ref settings.pairScope, Constants.PairScopeRange,
                raw => KissScopeUtility.Label(KissScopeUtility.Clamp(raw)));
            changed |= Checkbox(list, ref settings.directorButton);
            changed |= Checkbox(list, ref settings.autonomousKissing);
            changed |= Checkbox(list, ref settings.changeOpinion);
            changed |= Checkbox(list, ref settings.returnHomeAfterKiss);
            changed |= Checkbox(list, ref settings.noCooldowns);
        }
        else
        {
            Section(list, "MWAH.Settings.Section.Core");
            changed |= IntSlider(list, ref settings.kissDurationTicks, Constants.DurationTicksRange, MwahTime.FormatTicks,
                v => settings.kissDurationTicks = v);
            changed |= IntSlider(list, ref settings.heartFleckIntervalTicks, Constants.FleckIntervalTicksRange, MwahTime.FormatTicks,
                v => settings.heartFleckIntervalTicks = v);
            changed |= IntSlider(list, ref settings.pawnCooldownTicks, Constants.CooldownTicksRange, MwahTime.FormatTicks,
                v => settings.pawnCooldownTicks = v);
            changed |= IntSlider(list, ref settings.pairCooldownTicks, Constants.CooldownTicksRange, MwahTime.FormatTicks,
                v => settings.pairCooldownTicks = v);
            changed |= IntSlider(list, ref settings.thoughtDurationTicks, Constants.ThoughtDurationTicksRange, MwahTime.FormatTicks,
                v => settings.thoughtDurationTicks = v, Constants.ThoughtDurationStepTicks);
            changed |= FloatSlider(list, ref settings.moodMultiplier, Constants.MoodMultiplierRange, Constants.MoodMultiplierStep,
                v => v.ToString("0.##") + "x", v => settings.moodMultiplier = v);
            changed |= Checkbox(list, ref settings.socialImpactCompression);
            Section(list, "MWAH.Settings.Section.Autonomous");
            changed |= IntSlider(list, ref settings.autonomousIntervalTicks, Constants.AutonomousIntervalTicksRange, MwahTime.FormatTicks,
                v => settings.autonomousIntervalTicks = v);
            changed |= IntSlider(list, ref settings.autonomousRadiusCells, Constants.AutonomousRadiusRange, cells => cells.ToString(),
                v => settings.autonomousRadiusCells = v);
            Section(list, "MWAH.Settings.Section.System");
            changed |= TierSlider(list, ref settings.diagnosticLevel, Constants.DiagnosticLevelRange,
                raw => DiagLabel((MwahDiag)Mathf.Clamp(raw, (int)MwahDiag.Off, (int)MwahDiag.Verbose)));
            list.Gap();
            list.Label("MWAH.Settings.TimingHint".Translate());
            list.Gap();
            if (list.ButtonText("MWAH.Settings.Reset".Translate()))
            {
                CommitPendingEdit();
                settings.RestoreDefaults();
                changed = true;
            }
        }
        list.End();
        contentHeights[page] = list.CurHeight + 16f;
        Widgets.EndScrollView();
        if (changed) MwahMod.Instance?.RequestSettingsSave();
    }

    private void SelectPage(int next)
    {
        CommitPendingEdit();
        page = next;
    }

    public void Close()
    {
        CommitPendingEdit(releaseFocus: false);
        page = 0;
        scrollPositions[0] = Vector2.zero;
    }

    private bool AddonRow(Listing_Standard list, KissThingAddon addon)
    {
        float height = Mathf.Max(32f, Text.CalcHeight(addon.SwitchLabelKey.Translate(), list.ColumnWidth - 132f) + 8f);
        Rect row = list.GetRect(height);
        var toggle = new Rect(row.x, row.y, row.width - 100f, row.height);
        bool before = settings.AddonSwitch(addon.Id, addon.DefaultActive);
        bool value = before;
        Widgets.CheckboxLabeled(toggle, addon.SwitchLabelKey.Translate(), ref value);
        TooltipHandler.TipRegion(toggle, addon.SwitchDescKey.Translate());
        if (value != before) settings.SetAddonSwitch(addon.Id, value);
        bool open = expanded.Contains(addon.Id);
        if (Widgets.ButtonText(new Rect(row.xMax - 88f, row.y + 2f, 88f, 28f),
            (open ? "MWAH.Settings.Collapse" : "MWAH.Settings.Expand").Translate()))
        {
            CommitPendingEdit();
            if (open) expanded.Remove(addon.Id); else expanded.Add(addon.Id);
            open = !open;
        }
        bool changed = value != before;
        if (open)
        {
            changed |= AddonDurationRow(list, addon);
            if (addon.Definition.culturalOptions)
            {
                bool enabled = GUI.enabled;
                GUI.enabled = enabled && ModsConfig.IdeologyActive;
                changed |= Checkbox(list, ref settings.treeCulturalIntegration);
                GUI.enabled = enabled;
                if (!ModsConfig.IdeologyActive) list.Label("MWAH.Settings.CultureUnavailable".Translate());
            }
            list.Gap(8f);
        }
        return changed;
    }

    /// <summary>单行文本的实测高度 + 8px 呼吸位（Text.CalcHeight 按当前字体与给定宽度算
    /// 行框，下伸部与行距都在里面；量完还原字体，不污染 IMGUI 状态）。</summary>
    private static float LineHeightWithPadding(string text, GameFont font, float width)
    {
        GameFont fontBefore = Text.Font;
        Text.Font = font;
        float height = Text.CalcHeight(text, width);
        Text.Font = fontBefore;
        return height + 8f;
    }

    /// <summary>固定标题区：副标题在上（Small，默认锚点）、身份水印在下（Tiny、半透明、
    /// 不可交互——它是水印不是控件）、底部 1px 分隔线。两行都读同一份真相。</summary>
    private static void DrawPinnedHeader(Rect header, float subtitleHeight, string watermark)
    {
        Widgets.Label(header.TopPartPixels(subtitleHeight), "MWAH.Settings.Header".Translate());
        Widgets.DrawBoxSolid(new Rect(header.x, header.yMax - 1f, header.width, 1f), new Color(1f, 1f, 1f, 0.12f));
        Rect row = new Rect(header.x, header.y + subtitleHeight, header.width, header.height - subtitleHeight);
        TextAnchor anchorBefore = Text.Anchor;
        Color colorBefore = GUI.color;
        Text.Anchor = TextAnchor.MiddleLeft;
        GUI.color = new Color(1f, 1f, 1f, 0.55f);
        GameFont fontBefore = Text.Font;
        Text.Font = GameFont.Tiny;
        Widgets.Label(row, watermark);
        Text.Font = fontBefore;
        GUI.color = colorBefore;
        Text.Anchor = anchorBefore;
    }

    /// <summary>段标题：一条分隔线 + 加粗感的裸标签（键双语齐备由门把关）。</summary>
    private static void Section(Listing_Standard list, string labelKey)
    {
        list.GapLine();
        list.Label(labelKey.Translate());
    }

    /// <summary>诊断档位名：枚举拼出来的键（MWAH.Settings.Diag.&lt;Name&gt;），双语齐备由门核对。</summary>
    private static string DiagLabel(MwahDiag level) => ("MWAH.Settings.Diag." + level).Translate();

    /// <summary>
    /// addon 思想时长行：标签带 addon 名（{0}），数值框的编辑态键必须逐 addon 唯一
    /// （fieldId=Id —— 否则两个 addon 共用一个输入框焦点，editBuf 会串台）。
    /// </summary>
    private bool AddonDurationRow(Listing_Standard list, KissThingAddon addon)
    {
        int before = settings.AddonThoughtDuration(addon.Id, addon.DefaultThoughtDurationTicks);
        int value = before;
        bool touched = IntSliderKeyed(list, ref value, "MWAH.Settings.AddonDuration", "MWAH.Settings.AddonDurationDesc",
            Constants.AddonDurationTicksRange, MwahTime.FormatTicks, v => settings.SetAddonThoughtDuration(addon.Id, v), Constants.ThoughtDurationStepTicks,
            labelArg: addon.NameKey.Translate(), fieldId: addon.Id);
        if (touched && value != before)
        {
            settings.SetAddonThoughtDuration(addon.Id, value);
        }
        return touched && value != before;
    }

    /// <summary>字段名 → 键词根：首字母大写（modEnabled → ModEnabled）。派生只有一条规则，规则本身不许长记性。</summary>
    private static string Pascal(string field)
    {
        field = field.Substring(field.LastIndexOf('.') + 1);
        return string.IsNullOrEmpty(field) ? field : char.ToUpperInvariant(field[0]) + field.Substring(1);
    }

    // ===== 派生键行（设置字段专用）：调用点零字符串，键名从字段名捕获 =====

    private bool Checkbox(Listing_Standard list, ref bool value,
        [CallerArgumentExpression("value")] string field = "")
    {
        bool before = value;
        list.CheckboxLabeled(("MWAH.Settings." + Pascal(field)).Translate(), ref value,
            ("MWAH.Settings." + Pascal(field) + "Desc").Translate());
        return before != value;
    }

    /// <summary>
    /// 整数滑条（带数值框）。tick 项的框编辑的就是 tick —— 存储单位即输入单位，
    /// 三读法只出现在标签上，符合"时长只以 tick 为存储单位"的硬边界。
    /// </summary>
    private bool IntSlider(Listing_Standard list, ref int value, IntRange range, Func<int, string> showAs, Action<int> commit, int step = 1,
        [CallerArgumentExpression("value")] string field = "")
        => IntSliderCore(list, ref value, "MWAH.Settings." + Pascal(field), "MWAH.Settings." + Pascal(field) + "Desc",
            range, showAs, withField: true, commit, step);

    /// <summary>无数值框的档位滑条（档不是量：输入 3 没有意义）。</summary>
    private bool TierSlider(Listing_Standard list, ref int value, IntRange range, Func<int, string> showAs,
        [CallerArgumentExpression("value")] string field = "")
        => IntSliderCore(list, ref value, "MWAH.Settings." + Pascal(field), "MWAH.Settings." + Pascal(field) + "Desc",
            range, showAs, withField: false, commit: null);

    /// <summary>小数滑条（带数值框），落值量化到 step 的整数倍：更细的跨度只会往配置里写没意义的数字。</summary>
    private bool FloatSlider(Listing_Standard list, ref float value, FloatRange range, float step, Func<float, string> showAs, Action<float> commit,
        [CallerArgumentExpression("value")] string field = "")
    {
        float before = value;
        string labelKey = "MWAH.Settings." + Pascal(field);
        string label = labelKey.Translate() + ": " + showAs(value);
        float labelHeight = Mathf.Max(20f, Text.CalcHeight(label, list.ColumnWidth));
        Rect row = list.GetRect(labelHeight + 26f);
        DrawSliderLabel(row, label, (labelKey + "Desc").Translate());
        Rect slider = new Rect(row.x, row.y + labelHeight + 4f, row.width - FieldWidth - FieldGap, 20f);
        float raw = Widgets.HorizontalSlider(slider, value, range.min, range.max);
        value = Mathf.Round(raw / step) * step;
        value = NumericField(labelKey, new Rect(row.xMax - FieldWidth, row.y + labelHeight + 2f, FieldWidth, 24f),
            value, range.min, range.max, step, commit);
        return !Mathf.Approximately(before, value);
    }

    // ===== 显式键行（addon 与非字段文案专用） =====

    private bool IntSliderKeyed(Listing_Standard list, ref int value, string labelKey, string tipKey,
        IntRange range, Func<int, string> showAs, Action<int> commit, int step = 1, string? labelArg = null, string? fieldId = null)
        => IntSliderCore(list, ref value, labelKey, tipKey, range, showAs, withField: true, commit, step, labelArg, fieldId);

    private bool IntSliderCore(Listing_Standard list, ref int value, string labelKey, string tipKey,
        IntRange range, Func<int, string> showAs, bool withField, Action<int>? commit, int step = 1, string? labelArg = null, string? fieldId = null)
    {
        int before = value;
        string label = (labelArg == null ? labelKey.Translate() : labelKey.Translate(labelArg)) + ": " + showAs(value);
        string tip = tipKey.Translate();
        float labelHeight = Mathf.Max(20f, Text.CalcHeight(label, list.ColumnWidth));
        Rect row = list.GetRect(labelHeight + 26f);
        DrawSliderLabel(row, label, tip);
        if (!withField)
        {
            float rawScope = Widgets.HorizontalSlider(new Rect(row.x, row.y + labelHeight + 4f, row.width, 20f), value, range.min, range.max);
            value = Mathf.RoundToInt(rawScope);
            return before != value;
        }
        Rect slider = new Rect(row.x, row.y + labelHeight + 4f, row.width - FieldWidth - FieldGap, 20f);
        float raw = Widgets.HorizontalSlider(slider, value, range.min, range.max);
        value = Mathf.RoundToInt(raw / step) * step; // 滑条连续、落值量化：step 才是允许的刻度
        value = (int)NumericField(labelKey + (fieldId ?? ""), new Rect(row.xMax - FieldWidth, row.y + labelHeight + 2f, FieldWidth, 24f),
            value, range.min, range.max, step, v => commit!((int)v));
        return before != value;
    }

    private static void DrawSliderLabel(Rect row, string label, string tip)
    {
        Rect labelRect = new Rect(row.x, row.y, row.width, row.height - 26f);
        Widgets.Label(labelRect, label);
        TooltipHandler.TipRegion(labelRect, tip);
    }

    /// <summary>
    /// 数值输入框：点进即编辑（IMGUI 自己管焦点），回车或失焦提交；提交按 step 量化并夹进量程，
    /// 解析失败（空串、乱码）则整次编辑作废、回显当前值。解析用不变文化（InvariantCulture），
    /// 与 Settings.xml 的写法同一口径，不受系统小数点逗号影响。
    /// </summary>
    private float NumericField(string key, Rect rect, float value, float min, float max, float step, Action<float> commit)
    {
        // Capture before TextField can consume the key event.
        bool enter = Event.current.type == EventType.KeyDown
            && (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter);
        if (editingField == key && Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
            value = CommitPendingEdit() ?? value;
        string controlName = "MWAH_Field_" + key;
        GUI.SetNextControlName(controlName);
        string shown = editingField == key ? editBuf : value.ToString(System.Globalization.CultureInfo.InvariantCulture);
        string text = Widgets.TextField(rect, shown, 12);
        bool focused = GUI.GetNameOfFocusedControl() == controlName;
        if (focused && editingField != key)
        {
            CommitPendingEdit(releaseFocus: false);
            editingField = key;
            editBuf = text;
        }
        if (editingField != key) return value;
        pendingCommit = commit;
        editMin = min; editMax = max; editStep = step;
        editBuf = text;
        if (enter || !focused) value = CommitPendingEdit() ?? value;
        return value;
    }

    private float? CommitPendingEdit(bool releaseFocus = true)
    {
        if (editingField == null) return null;
        Action<float>? commit = pendingCommit;
        editingField = null;
        pendingCommit = null;
        if (releaseFocus) GUI.FocusControl(null);
        if (commit == null || !float.TryParse(editBuf, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float parsed) || float.IsNaN(parsed) || float.IsInfinity(parsed))
            return null;
        float value = Mathf.Clamp(Mathf.Round(parsed / editStep) * editStep, editMin, editMax);
        commit(value);
        MwahMod.Instance?.RequestSettingsSave();
        return value;
    }
}
