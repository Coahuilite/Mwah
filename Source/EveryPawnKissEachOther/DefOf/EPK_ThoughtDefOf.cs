using RimWorld;
using Verse;

namespace EveryPawnKissEachOther;

[DefOf]
public static class EPK_ThoughtDefOf
{
    /// <summary>纯心情记忆，不改动好感度（默认使用）。</summary>
    public static ThoughtDef EPK_Kissed = null!;

    /// <summary>Thought_MemorySocial 变体，仅在"亲吻留下痕迹"打开时使用。</summary>
    public static ThoughtDef EPK_KissedBond = null!;

    static EPK_ThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EPK_ThoughtDefOf));
    }
}
