using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 亲吻奖励：只有带心情系统（needs.mood）的 pawn 才拿得到 —— 判据是运行时 need 实例，
/// 不是种族：动物、机械族、无人机、异象实体本来就没有 mood，而 Anomaly 的蹒跚者/尸鬼/唤醒尸体
/// 种族是 Human（Humanlike 为真）却被 MutantDef.disableNeeds 摘掉了全部需求，同样拿不到。
/// 拿不到就是什么都不加，不做任何替代收益（结构层的开关会提前挡掉这种配对，
/// 见 <see cref="KissBoundary.CheckStructure"/>；这里的判空只兜住菜单打开后的状态变化）。
/// 强度 = ThoughtStage.baseMoodEffect × (自身 SocialImpact) × 设置倍率，
/// 由 Thought_Memory.moodPowerFactor 承载（原版 AddInteractionThought 同源机制）。
/// </summary>
public static class KissMoodReward
{
    /// <summary>由发起方统一结算，保证一次亲吻只发一份心情。</summary>
    public static void Settle(Pawn initiator, Pawn receiver)
    {
        GiveTo(initiator, receiver);
        GiveTo(receiver, initiator);
    }

    private static void GiveTo(Pawn pawn, Pawn other)
    {
        if (pawn.needs?.mood?.thoughts?.memories == null)
        {
            return;
        }

        MwahSettings settings = MwahMod.Settings;
        ThoughtDef def = settings.OpinionAffected ? MWAH_ThoughtDefOf.MWAH_KissedBond : MWAH_ThoughtDefOf.MWAH_Kissed;
        if (ThoughtMaker.MakeThought(def) is not Thought_Memory memory)
        {
            return;
        }

        // SocialImpact 对"有心情的 pawn"必然取到有效值：该 StatDef 标了 neverDisabled，且
        // SkillNeed_BaseBonus.ValueFor 在 pawn.skills == null 时直接返回 1f（依据见 MEMORY
        // 「原版能力边界」）。所以这里不兜 NaN/0 —— 兜不住的东西不存在。
        memory.moodPowerFactor = pawn.GetStatValue(StatDefOf.SocialImpact) * settings.MoodMult;
        if (settings.OpinionAffected && memory is Thought_MemorySocial socialMemory)
        {
            socialMemory.opinionOffset *= settings.MoodMult;
        }
        memory.durationTicksOverride = settings.ThoughtDurationTicks;

        pawn.needs.mood.thoughts.memories.TryGainMemory(memory, other);
    }
}
