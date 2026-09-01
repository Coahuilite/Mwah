using RimWorld;
using Verse;

namespace Mwah;

[DefOf]
public static class MWAH_ThoughtDefOf
{
    /// <summary>纯心情记忆，不改动好感度（默认使用）。</summary>
    public static ThoughtDef MWAH_Kissed = null!;

    /// <summary>Thought_MemorySocial 变体，仅在"亲吻留下痕迹"打开时使用。</summary>
    public static ThoughtDef MWAH_KissedBond = null!;

    static MWAH_ThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MWAH_ThoughtDefOf));
    }
}
