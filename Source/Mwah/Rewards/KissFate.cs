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

    /// <summary>按 scope 抽一行；null = 该表没有任何有效行。</summary>
    public static MWAH_FateDef? Roll(string scope)
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
        int roll = Rand.RangeInclusive(0, total - 1);
        for (int i = 0; i < Scratch.Count; i++)
        {
            roll -= Scratch[i].weight;
            if (roll < 0)
            {
                return Scratch[i];
            }
        }
        return Scratch[Scratch.Count - 1]; // 理论不可达（总和已按同一集合算），防御空转
    }

    /// <summary>
    /// 抽一行并落地。返回 false = 表空，调用方兜底。
    /// 心情的有无、旁白的有无、短讯的有无全部由行数据决定，代码不做任何"应该怎样"的假设。
    /// </summary>
    public static bool Grant(Pawn doer, Thing target, string scope)
    {
        MWAH_FateDef? row = Roll(scope);
        if (row == null)
        {
            return false;
        }
        GrantRow(doer, target, row);
        return true;
    }

    /// <summary>把一行结果发出去 —— 兜底路径也复用这一步，保证两条路落地形状一致。</summary>
    public static void GrantRow(Pawn doer, Thing target, MWAH_FateDef row)
    {
        if (row.thought != null && doer.needs?.mood?.thoughts?.memories != null)
        {
            GrantFatedMemory(doer, target, row);
        }
        if (!row.messageKey.NullOrEmpty())
        {
            Messages.Message(row.messageKey.Translate(doer.Named("PAWN"), target.Named("WALL")),
                new LookTargets(target), row.messageType ?? MessageTypeDefOf.NeutralEvent, historical: false);
        }
        MwahLog.Dev("fate " + row.scope + ": " + doer.LabelShort + " -> " + target.LabelCap + " row=" + row.defName);
    }

    /// <summary>
    /// 关系槽语义（2026-10-05 裁定，见 MEMORY "0.2.x mood architecture rulings"）：
    /// 这类"单主体、无对象"的心情记忆按 (pawn, def) 各占一个槽，新抽签**就地顶替**——
    /// 换档、换旁白、重计时。vanilla 的组满行为是"刷新最旧、丢弃新条"，旧档会骑在新抽签
    /// 头上常驻，所以顶替由结算侧显式做；找不到槽才新建。
    /// </summary>
    private static void GrantFatedMemory(Pawn doer, Thing target, MWAH_FateDef row)
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
            return;
        }
        if (ThoughtMaker.MakeThought(row.thought, row.stageIndex) is Thought_Memory memory)
        {
            if (memory is IFatedNarration fated)
            {
                fated.NarrationKey = row.narrationKey;
                fated.NarrationSubject = target.LabelCap;
            }
            memories.TryGainMemory(memory, null);
        }
    }
}
