using Verse;

namespace Mwah.Compatibility;

/// <summary>Def-load lifetime adapter; no external types or production API calls.</summary>
[StaticConstructorOnStartup]
internal static class VanillaNutrientPasteExpandedCompatibility
{
    private const string PackageId = "VanillaExpanded.VNutrientE";
    private static readonly ThingDef? tap;
    private static readonly ThingDef? feeder;
    private static readonly ThingDef? dripper;

    static VanillaNutrientPasteExpandedCompatibility()
    {
        if (!ModsConfig.IsActive(PackageId)) return;
        tap = Resolve("VNPE_NutrientPasteTap");
        feeder = Resolve("VNPE_NutrientPasteFeeder");
        dripper = Resolve("VNPE_NutrientPasteDripper");
    }

    private static ThingDef? Resolve(string defName)
    {
        ThingDef? definition = DefDatabase<ThingDef>.GetNamedSilentFail(defName);
        if (definition == null)
            Log.Warning("[MWAH] Optional VNPE kiss target missing: " + defName);
        return definition;
    }

    internal static bool IsOutlet(ThingDef definition) => definition == tap || IsTouchOutlet(definition);

    internal static bool IsTouchOutlet(ThingDef definition) => definition == feeder || definition == dripper;
}
