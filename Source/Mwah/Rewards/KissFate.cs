using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Mwah;

/// <summary>
/// 天意掷骰器：按 scope 查 <see cref="MWAH_FateDef"/> 表、加权抽一行、把结果落地
/// （心情记忆 + 实例旁白 + 可选短讯）。表是数据、这里只是引擎 —— 增删档位、
/// 改概率、换文案都不碰这个类。
/// 兜底失效形式：某 scope 一行有效行都没有 ⇒ <see cref="Grant"/> 返回 false，
/// 由调用方（addon）落回代码内置分布；坏行（weight≤0）静默跳过而不是拖垮整表。
/// </summary>
public static class KissFate
{
    private static readonly List<MWAH_FateDef> Scratch = new();

    /// <summary>按 scope 抽一行；null = 该表没有任何有效行。
    /// <paramref name="exclude"/> = 同一事件里已被同伴结算用掉的行，优先跳过 ——
    /// 一桩吻的两份结算若同表独立抽，撞车就会把同一句镜像输出两遍（2026-10-05 实机：
    /// 两只动物同抽"闻口袋"）。排除是尽力而为：表里只剩被排除那一行时照抽它，
    /// 沉默比复读更糟。</summary>
    public static MWAH_FateDef? Roll(string scope, MWAH_FateDef? exclude = null)
    {
        Scratch.Clear();
        int total = 0;
        foreach (MWAH_FateDef row in DefDatabase<MWAH_FateDef>.AllDefs)
        {
            if (row.scope == scope && row.weight > 0)
            {
                Scratch.Add(row);
                total += row.weight;
            }
        }
        if (Scratch.Count == 0 || total <= 0)
        {
            return null;
        }
        // 排除只在"该行确实在表里且表不止一行"时生效；行只按 weight>0 收集，
        // 所以 count>1 时扣掉 exclude 的权重后 total 必然仍 >0。
        int excludeIndex = exclude == null ? -1 : Scratch.IndexOf(exclude);
        if (excludeIndex >= 0)
        {
            if (Scratch.Count <= 1)
            {
                excludeIndex = -1;
            }
            else
            {
                total -= Scratch[excludeIndex].weight;
            }
        }
        int roll = Rand.RangeInclusive(0, total - 1);
        for (int i = 0; i < Scratch.Count; i++)
        {
            if (i == excludeIndex)
            {
                continue;
            }
            roll -= Scratch[i].weight;
            if (roll < 0)
            {
                return Scratch[i];
            }
        }
        for (int i = Scratch.Count - 1; i >= 0; i--)
        {
            if (i != excludeIndex)
            {
                return Scratch[i]; // 理论不可达（总和按同一集合算），防御空转
            }
        }
        return null;
    }

    /// <summary>
    /// 抽一行并落地。返回 false = 表空，调用方兜底。
    /// 心情的有无、旁白的有无、短讯的有无全部由行数据决定，代码不做任何"应该怎样"的假设。
    /// <paramref name="thoughtDurationTicks"/> &gt; 0 时写进记忆实例的时长覆盖（addon 独立时长设置）；
    /// 0 = 不吃覆盖，走 def 自带时长（消息通道表恒传 0：那些行根本不发 thought）。
    /// </summary>
    public static bool Grant(Pawn doer, Thing target, string scope, int thoughtDurationTicks = 0)
    {
        MWAH_FateDef? row = Roll(scope);
        if (row == null)
        {
            return false;
        }
        GrantRow(doer, target, row, thoughtDurationTicks);
        return true;
    }

    /// <summary>把一行结果发出去 —— 兜底路径也复用这一步，保证两条路落地形状一致。</summary>
    public static void GrantRow(Pawn doer, Thing target, MWAH_FateDef row, int thoughtDurationTicks = 0)
    {
        if (row.thought != null && doer.needs?.mood?.thoughts?.memories != null)
        {
            GrantFatedMemory(doer, target, row, thoughtDurationTicks);
        }
        if (!row.messageKey.NullOrEmpty())
        {
            Messages.Message(row.messageKey.Translate(doer.Named("PAWN"), target.Named("WALL")),
                new LookTargets(target), row.messageType ?? MessageTypeDefOf.NeutralEvent, historical: false);
        }
        MwahLog.Note("fate " + row.scope + ": " + doer.LabelShort + " -> " + target.LabelCap + " row=" + row.defName);
    }

    /// <summary>
    /// 关系槽语义（2026-10-05 裁定，见 MEMORY "0.2.x mood architecture rulings"）：
    /// 这类"单主体、无对象"的心情记忆按 (pawn, def) 各占一个槽，新抽签**就地顶替**——
    /// 换档、换旁白、重计时、重取时长。vanilla 的组满行为是"刷新最旧、丢弃新条"，
    /// 旧档会骑在新抽签头上常驻，所以顶替由结算侧显式做；找不到槽才新建。
    /// </summary>
    private static void GrantFatedMemory(Pawn doer, Thing target, MWAH_FateDef row, int thoughtDurationTicks)
    {
        MemoryThoughtHandler memories = doer.needs.mood.thoughts.memories;
        List<Thought_Memory> list = memories.Memories;
        Thought_Memory? slot = null;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].def == row.thought && list[i].otherPawn == null)
            {
                slot = list[i];
                break;
            }
        }
        if (slot != null)
        {
            slot.SetForcedStage(row.stageIndex);
            if (slot is IFatedNarration fatedSlot)
            {
                fatedSlot.NarrationKey = row.narrationKey;
                fatedSlot.NarrationSubject = target.LabelCap;
            }
            slot.Renew();
            ApplyDuration(slot, thoughtDurationTicks);
            return;
        }
        if (ThoughtMaker.MakeThought(row.thought, row.stageIndex) is Thought_Memory memory)
        {
            if (memory is IFatedNarration fated)
            {
                fated.NarrationKey = row.narrationKey;
                fated.NarrationSubject = target.LabelCap;
            }
            ApplyDuration(memory, thoughtDurationTicks);
            memories.TryGainMemory(memory, null);
        }
    }

    /// <summary>时长覆盖只在 &gt;0 时写入；0 = 保留 def 自带时长（顶替路径同样尊重"不吃覆盖"）。</summary>
    private static void ApplyDuration(Thought_Memory memory, int thoughtDurationTicks)
    {
        if (thoughtDurationTicks > 0)
        {
            memory.durationTicksOverride = thoughtDurationTicks;
        }
    }
}
