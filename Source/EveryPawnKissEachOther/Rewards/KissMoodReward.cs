using RimWorld;
using Verse;

namespace EveryPawnKissEachOther;

/// <summary>
/// 亲吻奖励：只有带心情系统（needs.mood）的 pawn 才拿得到。
/// 动物、机械族、无人机、异象实体没有 mood —— 按设计"什么都不加"，不做任何替代收益。
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

        EPKSettings settings = EPKMod.Settings;
        ThoughtDef def = settings.OpinionAffected ? EPK_ThoughtDefOf.EPK_KissedBond : EPK_ThoughtDefOf.EPK_Kissed;
        if (ThoughtMaker.MakeThought(def) is not Thought_Memory memory)
        {
            return;
        }

        // SocialImpact 由社交技能 + 说话/听觉容量决定；没有 skills 的 pawn 取不到有效值时按 1 处理。
        float social = pawn.GetStatValue(StatDefOf.SocialImpact);
        memory.moodPowerFactor = social * settings.MoodMult;
        if (settings.OpinionAffected && memory is Thought_MemorySocial socialMemory)
        {
            socialMemory.opinionOffset *= settings.MoodMult;
        }
        memory.durationTicksOverride = settings.ThoughtDurationTicks;

        pawn.needs.mood.thoughts.memories.TryGainMemory(memory, other);
    }
}
