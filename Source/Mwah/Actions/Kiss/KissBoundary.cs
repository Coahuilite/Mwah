using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 亲吻边界的唯一判定入口。三层，全部取自原版源码，不引入任何运行时补丁：
/// <list type="table">
///   <item><term>结构层</term><description>这一局里都不会变的条件（总开关、死活、有没有心情系统、敌对放行）。不满足就连灰项都不给。</description></item>
///   <item><term>参与层</term><description>原版 SocialInteractionUtility 对"社交对象"的通用要求：清醒、没在烧、不是社交无能的亚人、没被仪式 hediff 锁住。</description></item>
///   <item><term>主动层</term><description>谁去亲：在参与层之上还要能走、有嘴（Talking 容量）、生命阶段被原版允许发起社交。</description></item>
/// </list>
/// 心情判定刻意读运行时的 <c>needs.mood</c> 而不是 <c>RaceProps.Humanlike</c>：
/// Anomaly 的蹒跚者/尸鬼/唤醒尸体种族就是 Human（Humanlike 为真），但 MutantDef 的
/// <c>disableNeeds</c> 让它们连 needs 都没有 —— 用种族判会误纳成"能拿到心情收益"。
/// </summary>
public static class KissBoundary
{
    /// <summary>有没有心情系统（= 亲吻能不能产生收益）。以运行时 need 实例为准。</summary>
    public static bool HasMood(Pawn pawn) =>
        pawn != null && pawn.needs?.mood?.thoughts?.memories != null;

    /// <summary>参与层：任何一方的即时状态门槛，对齐原版 SocialInteractionUtility。</summary>
    public static AcceptanceReport CanParticipate(Pawn pawn)
    {
        if (pawn == null || pawn.Dead || !pawn.Spawned)
        {
            return new AcceptanceReport("MWAH.Fail.Gone".Translate());
        }
        // RestUtility.Awake()：意识容量不足（昏迷/麻醉）或当前 job 标记为 asleep（睡觉/隐眠）。
        if (!pawn.Awake())
        {
            return new AcceptanceReport("MWAH.Fail.Unconscious".Translate(pawn.Named("PAWN")));
        }
        if (pawn.IsBurning())
        {
            return new AcceptanceReport("MWAH.Fail.Burning".Translate(pawn.Named("PAWN")));
        }
        if (pawn.IsMutant && pawn.mutant.Def.incapableOfSocialInteractions)
        {
            return new AcceptanceReport("MWAH.Fail.SociallyIncapable".Translate(pawn.Named("PAWN")));
        }
        // interaction 传 null：mental state 不参与判定（玩家强行下令的动作，且发起方已在
        // CanTakeOrder 处被精神状态排除），只认 HediffDef.blocksSocialInteraction（仪式沉浸态）。
        if (pawn.IsInteractionBlocked(null, isInitiator: false, isRandom: false))
        {
            return new AcceptanceReport("MWAH.Fail.RitualAbsorbed".Translate(pawn.Named("PAWN")));
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>
    /// 主动层：能不能由 <paramref name="pawn"/> 走过去亲。
    /// <paramref name="mobilityOnly"/> 为真表示"只是这会儿动不了"，用于两个都失败时改说"两个都动不了"。
    /// </summary>
    public static AcceptanceReport CanInitiate(Pawn pawn, out bool mobilityOnly)
    {
        mobilityOnly = false;
        AcceptanceReport participation = CanParticipate(pawn);
        if (!participation.Accepted)
        {
            return participation;
        }
        if (!KissUtility.CanMoveNow(pawn))
        {
            mobilityOnly = true;
            return new AcceptanceReport("MWAH.Fail.Immobile".Translate(pawn.Named("PAWN")));
        }
        // "有没有能亲的嘴"在原版里没有独立容量，最贴近的是 Talking：它由 TalkingSource 提供
        // （人形是下颌，动物是颌/喙）。机械族只有 TalkingPathway 没有 TalkingSource，
        // 一律要求会把 MechanoidCanDo 变成死代码，所以只对肉体 race 判这条。
        if (pawn.RaceProps.IsFlesh
            && (pawn.health?.capacities == null || !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Talking)))
        {
            return new AcceptanceReport("MWAH.Fail.NoMouth".Translate(pawn.Named("PAWN")));
        }
        // 原版 LifeStageDef 自带的社交发起门槛（人类婴儿为 false，且 alwaysDowned 已先挡掉）。
        LifeStageDef? stage = pawn.ageTracker?.CurLifeStage;
        if (stage != null && !stage.canInitiateSocialInteraction)
        {
            return new AcceptanceReport("MWAH.Fail.TooYoung".Translate(pawn.Named("PAWN")));
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>结构层：这一局里不会变、且不该在右键菜单里刷屏的条件。</summary>
    public static AcceptanceReport CheckStructure(Pawn a, Pawn b)
    {
        MwahSettings? settings = MwahMod.Settings;
        if (settings == null || !settings.Enabled)
        {
            return new AcceptanceReport("MWAH.Fail.Disabled".Translate());
        }
        if (a == null || b == null || a.Dead || b.Dead || !a.Spawned || !b.Spawned)
        {
            return new AcceptanceReport("MWAH.Fail.Gone".Translate());
        }
        if (!settings.MoodlessAllowed && (!HasMood(a) || !HasMood(b)))
        {
            return new AcceptanceReport("MWAH.Fail.NoMood".Translate());
        }
        if (!settings.HostileAllowed && a.HostileTo(b))
        {
            return new AcceptanceReport("MWAH.Fail.Hostile".Translate());
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>菜单标签用的提示：这一对里有人没有心情系统，亲了也只有动作没有收益。</summary>
    public static bool AnyMoodless(Pawn a, Pawn b) => !HasMood(a) || !HasMood(b);
}
