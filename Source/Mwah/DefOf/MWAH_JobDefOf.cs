using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

[DefOf]
public static class MWAH_JobDefOf
{
    public static JobDef MWAH_Kiss = null!;

    /// <summary>单人亲非 pawn 目标（addon 族共享：墙、将来的树……）：走到贴目标格面向它若干 tick，天意结算。</summary>
    public static JobDef MWAH_KissThing = null!;

    static MWAH_JobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MWAH_JobDefOf));
    }
}
