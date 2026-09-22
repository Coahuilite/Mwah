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
    public bool noCooldowns = Constants.NoCooldowns;
    public bool autonomousKissing = Constants.AutonomousKissing;
    public int autonomousIntervalTicks = Constants.AutonomousIntervalTicks;
    public int autonomousRadiusCells = Constants.AutonomousRadiusCells;
    public int thoughtDurationTicks = Constants.ThoughtDurationTicks;
    public float moodMultiplier = Constants.MoodMultiplier;

    private Vector2 scrollPos;

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
        noCooldowns = Constants.NoCooldowns;
        autonomousKissing = Constants.AutonomousKissing;
        autonomousIntervalTicks = Constants.AutonomousIntervalTicks;
        autonomousRadiusCells = Constants.AutonomousRadiusCells;
        thoughtDurationTicks = Constants.ThoughtDurationTicks;
        moodMultiplier = Constants.MoodMultiplier;
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        bool changed = false;
        var viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height);
        // 自动测高：先用窗口高度开滚动，list 结束后用真实内容高度回写 viewRect。
        // 固定 ContentHeight 的写法在控件增删或译文变长后会截断底部控件。
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
        changed |= Checkbox(list, ref directorButton, "MWAH.Settings.DirectorButton", "MWAH.Settings.DirectorButtonDesc");
        changed |= Checkbox(list, ref noCooldowns, "MWAH.Settings.NoCooldowns", "MWAH.Settings.NoCooldownsDesc");
        changed |= IntSlider(list, ref pairScope, "MWAH.Settings.PairScope", "MWAH.Settings.PairScopeDesc",
            Constants.PairScopeRange, raw => KissScopeUtility.Label(KissScopeUtility.Clamp(raw)));
        changed |= Checkbox(list, ref autonomousKissing, "MWAH.Settings.Autonomous", "MWAH.Settings.AutonomousDesc");
        changed |= IntSlider(list, ref autonomousIntervalTicks, "MWAH.Settings.AutonomousInterval",
            "MWAH.Settings.AutonomousIntervalDesc", Constants.AutonomousIntervalTicksRange, MwahTime.FormatTicks);
        changed |= IntSlider(list, ref autonomousRadiusCells, "MWAH.Settings.AutonomousRadius",
            "MWAH.Settings.AutonomousRadiusDesc", Constants.AutonomousRadiusRange, cells => cells.ToString());
        changed |= Checkbox(list, ref changeOpinion, "MWAH.Settings.ChangeOpinion", "MWAH.Settings.ChangeOpinionDesc");
        changed |= Checkbox(list, ref returnHomeAfterKiss, "MWAH.Settings.ReturnHome", "MWAH.Settings.ReturnHomeDesc");

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
    /// 整数滑条。值的显示口径由 <paramref name="showAs"/> 决定：tick 项给三读法，
    /// 格数项给裸数字，门禁项给档位名 —— 控件形状相同，不必一份份抄。
    /// </summary>
    private static bool IntSlider(Listing_Standard list, ref int value, string labelKey, string tipKey,
        IntRange range, Func<int, string> showAs, int step = 1)
    {
        int before = value;
        string label = labelKey.Translate() + ": " + showAs(value);
        float raw = list.SliderLabeled(label, value, range.min, range.max, SliderLabelWidth, tipKey.Translate());
        // step 是允许的刻度：滑条本身是连续的，落值量化到 step 的整数倍才进存档。
        value = Mathf.RoundToInt(raw / step) * step;
        return before != value;
    }

    /// <summary>
    /// 小数滑条，落值量化到 <paramref name="step"/> 的整数倍：小于这个跨度的改动体感上没有区别，
    /// 却会在配置文件里留下无意义的长小数（0.25 游戏时、0.05 倍都是这么定的）。
    /// </summary>
    private static bool FloatSlider(Listing_Standard list, ref float value, string labelKey, string tipKey,
        FloatRange range, float step, Func<float, string> showAs)
    {
        float before = value;
        string label = labelKey.Translate() + ": " + showAs(value);
        float raw = list.SliderLabeled(label, value, range.min, range.max, SliderLabelWidth, tipKey.Translate());
        value = Mathf.Round(raw / step) * step;
        return !Mathf.Approximately(before, value);
    }
}
