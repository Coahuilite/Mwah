using System;
using RimWorld;
using UnityEngine;
using Verse;

namespace EveryPawnKissEachOther;

/// <summary>
/// 玩家配置。生命周期：immediate canonical + coalesced persistence。
/// 控件改动立刻成为唯一权威值并即时生效；磁盘写入由 <see cref="EPKMod"/> 防抖合并，关窗时强制 flush。
/// 所有时长按 tick 存储，界面同时给出 tick / 现实秒 / 游戏小时三种读法。
/// </summary>
public class EPKSettings : ModSettings
{
    private const float ContentHeight = 980f;

    // 出厂默认来自 Constants；玩家改动只落在这些字段上，Scribe key 与字段名一致。
    public bool modEnabled = Constants.ModEnabled;
    public bool allowHostileTargets = Constants.AllowHostileTargets;
    public bool allowNonHumanlike = Constants.AllowNonHumanlike;
    public bool changeOpinion = Constants.ChangeOpinion;
    public bool returnHomeAfterKiss = Constants.ReturnHomeAfterKiss;
    public int kissDurationTicks = Constants.KissDurationTicks;
    public int heartFleckIntervalTicks = Constants.HeartFleckIntervalTicks;
    public int pawnCooldownTicks = Constants.PawnCooldownTicks;
    public int pairCooldownTicks = Constants.PairCooldownTicks;
    public float thoughtDurationGameHours = Constants.ThoughtDurationGameHours;
    public float moodMultiplier = Constants.MoodMultiplier;

    private Vector2 scrollPos;

    // 读侧统一 canonicalize：脏配置或手改的 XML 不能把运行时带进非法区间。
    public bool Enabled => modEnabled;
    public bool HostileAllowed => allowHostileTargets;
    public bool NonHumanlikeAllowed => allowNonHumanlike;
    public bool OpinionAffected => changeOpinion;
    public bool ReturnsHome => returnHomeAfterKiss;
    public int DurationTicks => Mathf.Clamp(kissDurationTicks, Constants.DurationTicksRange.min, Constants.DurationTicksRange.max);
    public int FleckIntervalTicks => Mathf.Clamp(heartFleckIntervalTicks, Constants.FleckIntervalTicksRange.min, Constants.FleckIntervalTicksRange.max);
    public int PawnCooldown => Mathf.Clamp(pawnCooldownTicks, Constants.CooldownTicksRange.min, Constants.CooldownTicksRange.max);
    public int PairCooldown => Mathf.Clamp(pairCooldownTicks, Constants.CooldownTicksRange.min, Constants.CooldownTicksRange.max);
    public float ThoughtDurationHours => Mathf.Clamp(thoughtDurationGameHours, Constants.ThoughtHoursRange.min, Constants.ThoughtHoursRange.max);
    public float MoodMult => Mathf.Clamp(moodMultiplier, Constants.MoodMultiplierRange.min, Constants.MoodMultiplierRange.max);
    public int ThoughtDurationTicks => EPKTime.FromGameHours(ThoughtDurationHours);

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref modEnabled, "modEnabled", Constants.ModEnabled);
        Scribe_Values.Look(ref allowHostileTargets, "allowHostileTargets", Constants.AllowHostileTargets);
        Scribe_Values.Look(ref allowNonHumanlike, "allowNonHumanlike", Constants.AllowNonHumanlike);
        Scribe_Values.Look(ref changeOpinion, "changeOpinion", Constants.ChangeOpinion);
        Scribe_Values.Look(ref returnHomeAfterKiss, "returnHomeAfterKiss", Constants.ReturnHomeAfterKiss);
        Scribe_Values.Look(ref kissDurationTicks, "kissDurationTicks", Constants.KissDurationTicks);
        Scribe_Values.Look(ref heartFleckIntervalTicks, "heartFleckIntervalTicks", Constants.HeartFleckIntervalTicks);
        Scribe_Values.Look(ref pawnCooldownTicks, "pawnCooldownTicks", Constants.PawnCooldownTicks);
        Scribe_Values.Look(ref pairCooldownTicks, "pairCooldownTicks", Constants.PairCooldownTicks);
        Scribe_Values.Look(ref thoughtDurationGameHours, "thoughtDurationGameHours", Constants.ThoughtDurationGameHours);
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
        thoughtDurationGameHours = ThoughtDurationHours;
        moodMultiplier = MoodMult;
    }

    public void RestoreDefaults()
    {
        modEnabled = Constants.ModEnabled;
        allowHostileTargets = Constants.AllowHostileTargets;
        allowNonHumanlike = Constants.AllowNonHumanlike;
        changeOpinion = Constants.ChangeOpinion;
        returnHomeAfterKiss = Constants.ReturnHomeAfterKiss;
        kissDurationTicks = Constants.KissDurationTicks;
        heartFleckIntervalTicks = Constants.HeartFleckIntervalTicks;
        pawnCooldownTicks = Constants.PawnCooldownTicks;
        pairCooldownTicks = Constants.PairCooldownTicks;
        thoughtDurationGameHours = Constants.ThoughtDurationGameHours;
        moodMultiplier = Constants.MoodMultiplier;
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        bool changed = false;
        var viewRect = new Rect(0f, 0f, inRect.width - 16f, ContentHeight);
        Widgets.BeginScrollView(inRect, ref scrollPos, viewRect);
        var list = new Listing_Standard(GameFont.Small);
        list.ColumnWidth = viewRect.width - 24f;
        list.Begin(viewRect);

        list.Label("EPK.Settings.Header".Translate());
        list.GapLine();
        changed |= Checkbox(list, ref modEnabled, "EPK.Settings.Enabled", "EPK.Settings.EnabledDesc");
        list.Gap();

        changed |= TickSlider(list, ref kissDurationTicks, "EPK.Settings.Duration", "EPK.Settings.DurationDesc",
            Constants.DurationTicksRange.min, Constants.DurationTicksRange.max);
        changed |= TickSlider(list, ref heartFleckIntervalTicks, "EPK.Settings.FleckInterval", "EPK.Settings.FleckIntervalDesc",
            Constants.FleckIntervalTicksRange.min, Constants.FleckIntervalTicksRange.max);
        changed |= TickSlider(list, ref pawnCooldownTicks, "EPK.Settings.PawnCooldown", "EPK.Settings.PawnCooldownDesc",
            Constants.CooldownTicksRange.min, Constants.CooldownTicksRange.max);
        changed |= TickSlider(list, ref pairCooldownTicks, "EPK.Settings.PairCooldown", "EPK.Settings.PairCooldownDesc",
            Constants.CooldownTicksRange.min, Constants.CooldownTicksRange.max);
        changed |= HoursSlider(list);
        changed |= MoodSlider(list);

        list.GapLine();
        changed |= Checkbox(list, ref allowHostileTargets, "EPK.Settings.AllowHostile", "EPK.Settings.AllowHostileDesc");
        changed |= Checkbox(list, ref allowNonHumanlike, "EPK.Settings.AllowNonHumanlike", "EPK.Settings.AllowNonHumanlikeDesc");
        changed |= Checkbox(list, ref changeOpinion, "EPK.Settings.ChangeOpinion", "EPK.Settings.ChangeOpinionDesc");
        changed |= Checkbox(list, ref returnHomeAfterKiss, "EPK.Settings.ReturnHome", "EPK.Settings.ReturnHomeDesc");

        list.Gap();
        list.Label("EPK.Settings.TimingHint".Translate());
        list.Gap();
        if (list.ButtonText("EPK.Settings.Reset".Translate()))
        {
            RestoreDefaults();
            changed = true;
        }

        list.End();
        Widgets.EndScrollView();
        if (changed)
        {
            EPKMod.Instance?.RequestSettingsSave();
        }
    }

    private static bool Checkbox(Listing_Standard list, ref bool value, string labelKey, string tipKey)
    {
        bool before = value;
        list.CheckboxLabeled(labelKey.Translate(), ref value, tipKey.Translate());
        return before != value;
    }

    private static bool TickSlider(Listing_Standard list, ref int ticks, string labelKey, string tipKey, int min, int max)
    {
        int before = ticks;
        string label = labelKey.Translate() + ": " + EPKTime.FormatTicks(ticks);
        ticks = Mathf.RoundToInt(list.SliderLabeled(label, ticks, min, max, 0.62f, tipKey.Translate()));
        return before != ticks;
    }

    private bool HoursSlider(Listing_Standard list)
    {
        float before = thoughtDurationGameHours;
        string label = "EPK.Settings.ThoughtDuration".Translate() + ": " + EPKTime.FormatGameHours(before);
        float raw = list.SliderLabeled(label, before, Constants.ThoughtHoursRange.min, Constants.ThoughtHoursRange.max,
            0.62f, "EPK.Settings.ThoughtDurationDesc".Translate());
        // 1/4 游戏小时粒度：小于这个跨度在体感上没有区别，却会让存档里出现无意义的长小数。
        thoughtDurationGameHours = Mathf.Round(raw * 4f) / 4f;
        return !Mathf.Approximately(before, thoughtDurationGameHours);
    }

    private bool MoodSlider(Listing_Standard list)
    {
        float before = moodMultiplier;
        string label = "EPK.Settings.MoodMultiplier".Translate() + ": " + before.ToString("0.##") + "x";
        float raw = list.SliderLabeled(label, before, Constants.MoodMultiplierRange.min, Constants.MoodMultiplierRange.max,
            0.62f, "EPK.Settings.MoodMultiplierDesc".Translate());
        moodMultiplier = Mathf.Round(raw * 20f) / 20f; // 0.05 粒度
        return !Mathf.Approximately(before, moodMultiplier);
    }
}
