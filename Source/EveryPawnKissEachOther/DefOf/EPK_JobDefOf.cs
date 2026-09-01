using RimWorld;
using Verse;
using Verse.AI;

namespace EveryPawnKissEachOther;

[DefOf]
public static class EPK_JobDefOf
{
    public static JobDef EPK_Kiss = null!;

    static EPK_JobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EPK_JobDefOf));
    }
}
