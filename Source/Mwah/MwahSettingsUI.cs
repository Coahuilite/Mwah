using System;
using System.Runtime.CompilerServices;
using RimWorld;
using UnityEngine;
using Verse;

namespace Mwah;

/// <summary>
/// 设置页的渲染半边（partial；存储与生命周期在 MwahSettings.cs）。
///
/// 键名派生纪律（2026-10-05）：**设置字段行的语言键不手写**——helper 用
/// CallerArgumentExpression 捕获字段名，派生 `MWAH.Settings.<Pascal(字段)>` 与 `…Desc`。
/// 于是"字段 = Scribe key = Restore = 语言键词根"四方对齐，一次 grep 从存储贯穿到文案；
/// 键打错这类事故在构造上消失，双语缺键由门 3/4 展开派生集合后代查。
/// 非字段行（addon 开关/时长、段标题、档位名）不在此列——它们没有字段可派生，
/// 走显式键（*Keyed 后缀的 helper），规则清晰："字段行派生，其余命名"。
/// </summary>
public partial class MwahSettings
{
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
        changed |= Checkbox(list, ref modEnabled);

        // ===== 段一：核心（双人吻的全部旋钮 + 全局入口开关）=====
        Section(list, "MWAH.Settings.Section.Core");
        // 范围档位是"档"不是"量"：TierSlider 刻意不带数值框。
        changed |= TierSlider(list, ref pairScope, Constants.PairScopeRange,
            raw => KissScopeUtility.Label(KissScopeUtility.Clamp(raw)));
        changed |= IntSlider(list, ref kissDurationTicks, Constants.DurationTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref heartFleckIntervalTicks, Constants.FleckIntervalTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref pawnCooldownTicks, Constants.CooldownTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref pairCooldownTicks, Constants.CooldownTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref thoughtDurationTicks, Constants.ThoughtDurationTicksRange, MwahTime.FormatTicks,
            Constants.ThoughtDurationStepTicks);
        changed |= FloatSlider(list, ref moodMultiplier, Constants.MoodMultiplierRange, Constants.MoodMultiplierStep,
            v => v.ToString("0.##") + "x");
        changed |= Checkbox(list, ref changeOpinion);
        changed |= Checkbox(list, ref returnHomeAfterKiss);
        changed |= Checkbox(list, ref noCooldowns);
        changed |= Checkbox(list, ref directorButton);

        // ===== 段二：自主撮合（系统替玩家点的鸳鸯）=====
        Section(list, "MWAH.Settings.Section.Autonomous");
        changed |= Checkbox(list, ref autonomousKissing);
        changed |= IntSlider(list, ref autonomousIntervalTicks, Constants.AutonomousIntervalTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref autonomousRadiusCells, Constants.AutonomousRadiusRange, cells => cells.ToString());

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
        // 档位是"档"不是"量"：输入 2 没有意义，同样走无框 TierSlider。
        changed |= TierSlider(list, ref diagnosticLevel, Constants.DiagnosticLevelRange,
            raw => DiagLabel((MwahDiag)Mathf.Clamp(raw, (int)MwahDiag.Off, (int)MwahDiag.Verbose)));
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

    /// <summary>诊断档位名：枚举拼出来的键（MWAH.Settings.Diag.&lt;Name&gt;），双语齐备由门核对。</summary>
    private static string DiagLabel(MwahDiag level) => ("MWAH.Settings.Diag." + level).Translate();

    /// <summary>addon 开关行：读写都走字典（缺槽落 DefaultActive），写只在真被改过时发生。</summary>
    private bool AddonSwitchRow(Listing_Standard list, KissThingAddon addon)
    {
        bool before = AddonSwitch(addon.Id, addon.DefaultActive);
        bool value = before;
        bool touched = CheckboxKeyed(list, ref value, addon.SwitchLabelKey, addon.SwitchDescKey);
        if (touched && value != before)
        {
            SetAddonSwitch(addon.Id, value);
        }
        return touched && value != before;
    }

    /// <summary>
    /// addon 思想时长行：标签带 addon 名（{0}），数值框的编辑态键必须逐 addon 唯一
    /// （fieldId=Id —— 否则两个 addon 共用一个输入框焦点，editBuf 会串台）。
    /// </summary>
    private bool AddonDurationRow(Listing_Standard list, KissThingAddon addon)
    {
        int before = AddonThoughtDuration(addon.Id, addon.DefaultThoughtDurationTicks);
        int value = before;
        bool touched = IntSliderKeyed(list, ref value, "MWAH.Settings.AddonDuration", "MWAH.Settings.AddonDurationDesc",
            Constants.AddonDurationTicksRange, MwahTime.FormatTicks, Constants.ThoughtDurationStepTicks,
            labelArg: addon.NameKey.Translate(), fieldId: addon.Id);
        if (touched && value != before)
        {
            SetAddonThoughtDuration(addon.Id, value);
        }
        return touched && value != before;
    }

    /// <summary>字段名 → 键词根：首字母大写（modEnabled → ModEnabled）。派生只有一条规则，规则本身不许长记性。</summary>
    private static string Pascal(string field) =>
        string.IsNullOrEmpty(field) ? field : char.ToUpperInvariant(field[0]) + field.Substring(1);

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
    private bool IntSlider(Listing_Standard list, ref int value, IntRange range, Func<int, string> showAs, int step = 1,
        [CallerArgumentExpression("value")] string field = "")
        => IntSliderCore(list, ref value, "MWAH.Settings." + Pascal(field), "MWAH.Settings." + Pascal(field) + "Desc",
            range, showAs, withField: true, step);

    /// <summary>无数值框的档位滑条（档不是量：输入 3 没有意义）。</summary>
    private bool TierSlider(Listing_Standard list, ref int value, IntRange range, Func<int, string> showAs,
        [CallerArgumentExpression("value")] string field = "")
        => IntSliderCore(list, ref value, "MWAH.Settings." + Pascal(field), "MWAH.Settings." + Pascal(field) + "Desc",
            range, showAs, withField: false);

    /// <summary>小数滑条（带数值框），落值量化到 step 的整数倍：更细的跨度只会往配置里写没意义的数字。</summary>
    private bool FloatSlider(Listing_Standard list, ref float value, FloatRange range, float step, Func<float, string> showAs,
        [CallerArgumentExpression("value")] string field = "")
    {
        float before = value;
        string labelKey = "MWAH.Settings." + Pascal(field);
        string label = labelKey.Translate() + ": " + showAs(value);
        Rect row = list.GetRect(46f);
        DrawSliderLabel(row, label, (labelKey + "Desc").Translate());
        Rect slider = new Rect(row.x, row.y + 24f, row.width - FieldWidth - FieldGap, 20f);
        float raw = Widgets.HorizontalSlider(slider, value, range.min, range.max);
        value = Mathf.Round(raw / step) * step;
        value = NumericField(labelKey, new Rect(row.xMax - FieldWidth, row.y + 22f, FieldWidth, 24f),
            value, range.min, range.max, step);
        return !Mathf.Approximately(before, value);
    }

    // ===== 显式键行（addon 与非字段文案专用） =====

    private static bool CheckboxKeyed(Listing_Standard list, ref bool value, string labelKey, string tipKey)
    {
        bool before = value;
        list.CheckboxLabeled(labelKey.Translate(), ref value, tipKey.Translate());
        return before != value;
    }

    private bool IntSliderKeyed(Listing_Standard list, ref int value, string labelKey, string tipKey,
        IntRange range, Func<int, string> showAs, int step = 1, string? labelArg = null, string? fieldId = null)
        => IntSliderCore(list, ref value, labelKey, tipKey, range, showAs, withField: true, step, labelArg, fieldId);

    private bool IntSliderCore(Listing_Standard list, ref int value, string labelKey, string tipKey,
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
