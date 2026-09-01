using RimWorld;
using Verse;
using Verse.AI;

namespace Mwah;

[DefOf]
public static class MWAH_JobDefOf
{
    public static JobDef MWAH_Kiss = null!;

    static MWAH_JobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(MWAH_JobDefOf));
    }
}
