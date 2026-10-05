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
/// 心情由 Thought_Memory.moodPowerFactor 承载、意见由 opinionOffset 承载（原版 AddInteractionThought
/// 对同一个倍率两边都乘，这里照做）。数值阶梯的取值依据见 1.6/Defs/Kiss/MWAH_ThoughtDefs.xml 的注释。
/// </summary>
public static class KissMoodReward
{
    /// <summary>由发起方统一结算，保证一次亲吻只发一份心情。
    /// 同一事件内的两份结算各抽一行旁白/短讯：**同表时第二抽排除第一抽已用的行**。
    /// 独立随机本身允许撞车（八行均匀表 ≈ 1/8），但撞了就会把同一句以两人视角镜像
    /// 复读一遍，玩家眼里这是发配器坏了（2026-10-05 实机定罪：鸸鹋×象同抽"闻口袋"）。</summary>
    public static void Settle(Pawn initiator, Pawn receiver)
    {
        // 类人旁白表先抽好再分发：谁有没有心情由 GiveTo 自己判，抽了不用无害。
        MWAH_FateDef? narrationA = KissFate.Roll(KissFateScopes.PawnNarrationScope);
        MWAH_FateDef? narrationB = KissFate.Roll(KissFateScopes.PawnNarrationScope, narrationA);
        GiveTo(initiator, receiver, narrationA);
        GiveTo(receiver, initiator, narrationB);
        // 没有心情系统的参与者改走天意表的消息通道：机械族/无人机的冷笑话、动物的
        // 嗅觉叙事、实体的空壳叙事各查各的表；同为一族时（动物×动物）第二抽排除第一行。
        TellMoodlessPair(initiator, receiver);
    }

    private static void TellMoodlessPair(Pawn a, Pawn b)
    {
        string? scopeA = ScopeForMoodless(a);
        string? scopeB = ScopeForMoodless(b);
        MWAH_FateDef? rowA = scopeA == null ? null : KissFate.Roll(scopeA);
        MWAH_FateDef? rowB = scopeB == null ? null : KissFate.Roll(scopeB, scopeA == scopeB ? rowA : null);
        if (rowA != null)
        {
            KissFate.GrantRow(a, b, rowA);
        }
        if (rowB != null)
        {
            KissFate.GrantRow(b, a, rowB);
        }
    }

    /// <summary>无心情参与者归哪张天意表；null = 不发（有心情的，或认不出族属的——
    /// 认不出留一行 dev 日志暴露缺口，不借别人的嘴说话，类别与优先级在 KissFateScopes）。</summary>
    private static string? ScopeForMoodless(Pawn pawn)
    {
        if (pawn.needs?.mood?.thoughts?.memories != null)
        {
            return null;
        }
        string? scope = KissFateScopes.ScopeFor(pawn);
        if (scope == null)
        {
            MwahLog.Note("moodless pawn matched no fate scope: " + pawn.LabelShortCap + " (" + pawn.def.defName + ")");
        }
        return scope;
    }

    private static void GiveTo(Pawn pawn, Pawn other, MWAH_FateDef? narration)
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
        // 原版 Pawn_InteractionsTracker.AddInteractionThought 对同一个倍率既乘心情也乘意见，
        // 这里照做：社交达人的吻在两边都更值钱，哑巴/失聪者在两边都更不值钱。
        float impact = pawn.GetStatValue(StatDefOf.SocialImpact) * settings.MoodMult;
        memory.moodPowerFactor = impact;
        if (memory is Thought_MemorySocial socialMemory)
        {
            socialMemory.opinionOffset *= impact;
        }
        memory.durationTicksOverride = settings.ThoughtDurationTicks;

        // 类人双人吻的旁白：用 Settle 抽好的那一行挂到这条记忆实例上（行空则维持静态描述，
        // 心情数值与时长完全不受表影响 —— 表只管"这句话怎么说"）。
        if (narration != null && memory is IFatedNarration fated && !narration.narrationKey.NullOrEmpty())
        {
            fated.NarrationKey = narration.narrationKey;
            fated.NarrationSubject = other.LabelShort;
            // 这条路径不经 GrantRow，补一行同形打点：抽中哪一行在 note 层可见（发行包也看得到）。
            MwahLog.Note("fate KissPawn: " + pawn.LabelShort + " -> " + other.LabelShort + " row=" + narration.defName);
        }

        pawn.needs.mood.thoughts.memories.TryGainMemory(memory, other);
    }
}
