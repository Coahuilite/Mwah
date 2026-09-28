using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 亲吻边界的唯一判定入口。四层顺序，判据全部取自原版源码，不引入任何运行时补丁：
/// <list type="table">
///   <item><term>结构层</term><description>这一局里不会变、又不该在菜单里刷屏的条件：总开关、死活与在场。不满足就连灰项都不给。</description></item>
///   <item><term>门禁层</term><description>七档 <see cref="KissScope"/>（出厂最右档 = 万物互亲）：双方都要在档内；越界只说"谁不想亲谁"，不提设置。</description></item>
///   <item><term>参与层</term><description>原版 SocialInteractionUtility 对"社交对象"的通用要求：清醒、没在烧、不是社交无能的亚人、没被仪式 hediff 锁住。</description></item>
///   <item><term>主动层</term><description>谁去亲：在参与层之上还要求能走、有嘴（Talking 容量）、生命阶段被原版允许发起社交。</description></item>
/// </list>
/// "有没有心情"不是准入条件而是收益条件，且刻意读运行时的 <c>needs.mood</c> 而不是
/// <c>RaceProps.Humanlike</c>：Anomaly 的蹒跚者/尸鬼/唤醒尸体种族就是 Human（Humanlike 为真），
/// 但 MutantDef 的 <c>disableNeeds</c> 让它们连 needs 都没有 —— 用种族判会误纳成"能拿到心情收益"。
/// </summary>
public static class KissBoundary
{
    /// <summary>有没有心情系统（= 亲吻能不能产生收益）。以运行时 need 实例为准。</summary>
    public static bool HasMood(Pawn pawn) =>
        pawn != null && pawn.needs?.mood?.thoughts?.memories != null;

    /// <summary>
    /// "归玩家管"的口径。原版 IsPlayerControlled 不含殖民地动物（它们过不了 CanTakeOrder），
    /// 但自家动物按维护者的口径算玩家侧，所以这里显式并上 IsColonyAnimal。
    ///
    /// 代价要写清楚：自家动物因此既不能被右键下令（原版限制）、也不参与自主派发（这条口径），
    /// 于是它们只能被亲，不会主动去亲。要它们也能主动，只能把它们留在自主派发里。
    /// </summary>
    public static bool UnderPlayerManagement(Pawn pawn) =>
        pawn != null && (pawn.IsPlayerControlled || pawn.IsColonyAnimal);

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
    /// 主动层：在参与层之上还要求的三条 —— 走得动、有嘴、生命阶段允许发起社交。
    /// <paramref name="mobilityOnly"/> 为真表示"只是这会儿动不了"，用于两个都失败时改说"两个都动不了"。
    ///
    /// 前提：调用方已经跑过 <see cref="CanParticipate"/>。这三层是按顺序定义的（见类注释），
    /// 主动层不再回头重跑参与层，否则一次判定里同一个 pawn 的清醒/燃烧/仪式检查要做两遍；
    /// 走完整链的入口只有 <see cref="KissUtility.Propose"/> 与
    /// <see cref="KissUtility.PairLooksKissable"/>，它们都已经按顺序查过。
    /// </summary>
    public static AcceptanceReport CanInitiate(Pawn pawn, out bool mobilityOnly)
    {
        mobilityOnly = false;
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
        if (!MwahMod.Settings.Enabled)
        {
            return new AcceptanceReport("MWAH.Fail.Disabled".Translate());
        }
        if (a.Dead || b.Dead || !a.Spawned || !b.Spawned)
        {
            return new AcceptanceReport("MWAH.Fail.Gone".Translate());
        }
        return AcceptanceReport.WasAccepted;
    }

    /// <summary>
    /// 门禁层：越界不解释设置，只给一句"谁不想"。
    /// 主语取越界的那一方；两边都越界时让被点的那位拒绝（玩家点的是它）。
    /// 阵营与种族本来都不设限，收到哪一档是玩家自己的选择，不该由系统出面说话。
    /// </summary>
    public static AcceptanceReport CheckScope(Pawn a, Pawn b)
    {
        KissScope scope = MwahMod.Settings.Scope;
        bool aIn = KissScopeUtility.InScope(a, b, scope);
        bool bIn = KissScopeUtility.InScope(b, a, scope);
        if (aIn && bIn)
        {
            return AcceptanceReport.WasAccepted;
        }
        Pawn unwilling = bIn ? a : b;
        Pawn other = unwilling == a ? b : a;
        return new AcceptanceReport("MWAH.Fail.Unwilling".Translate(unwilling.Named("PAWN"), other.Named("OTHER")));
    }

    /// <summary>
    /// 这会儿正在交战：手里挂着原版三条攻击性 job 之一（近身追杀 AttackMelee、
    /// 站桩输出 AttackStatic、互殴 SocialFight）。给**自主派发**的战斗闸用 —— 系统不许把
    /// 单位硬拽去亲嘴（那是免费点穴器）；玩家亲自下令的两条路不受此限（见
    /// <see cref="KissUtility.BeginDirected"/> 的 playerIssued）。
    /// 只认 job，不查 mindState.enemyTarget：那个旗标在交火结束后还会滞留很久，拿它当闸
    /// 会把"刚打完、正溜达"的机械体永久排除在自主亲吻之外。
    /// </summary>
    public static bool InCombatNow(Pawn pawn)
    {
        JobDef def = pawn.CurJobDef;
        return def == JobDefOf.AttackMelee || def == JobDefOf.AttackStatic || def == JobDefOf.SocialFight;
    }

    /// <summary>菜单标签用的提示：这一对里有人没有心情系统，亲了也只有动作没有收益。</summary>
    public static bool AnyMoodless(Pawn a, Pawn b) => !HasMood(a) || !HasMood(b);
}
