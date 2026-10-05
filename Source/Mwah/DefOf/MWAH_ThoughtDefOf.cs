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

    // 亲墙天意：单 def 五档（stageIndex 0..4 = 越想越怪/毫无感觉/感觉不错/被回应了/爱上这堵墙）。
    public static ThoughtDef MWAH_KissedWall = null!;

    static MWAH_ThoughtDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MWAH_ThoughtDefOf));
    }
}
