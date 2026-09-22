using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

[DefOf]
public static class MWAH_JobDefOf
{
    public static JobDef MWAH_Kiss = null!;

    /// <summary>单人亲墙：走到墙边面向它闭眼若干 tick，天意结算。</summary>
    public static JobDef MWAH_KissWall = null!;

    static MWAH_JobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MWAH_JobDefOf));
    }
}
