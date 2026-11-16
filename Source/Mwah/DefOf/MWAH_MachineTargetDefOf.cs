using RimWorld;
using Verse;

namespace Mwah;

[DefOf]
public static class MWAH_MachineTargetDefOf
{
    public static ThingDef SimpleResearchBench = null!;
    public static ThingDef HiTechResearchBench = null!;
    public static ThingDef MultiAnalyzer = null!;
    public static ThingDef NutrientPasteDispenser = null!;

    static MWAH_MachineTargetDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(MWAH_MachineTargetDefOf));
}
