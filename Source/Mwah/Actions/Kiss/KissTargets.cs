using System;
using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Mwah;

/// <summary>Stand candidates a registered optional target accepts; both reuse the machine slot rules.</summary>
public enum KissTargetStandPolicy
{
    /// <summary>Central interaction cell plus one cell to either side, like the vanilla dispenser.</summary>
    Forward,
    /// <summary>Any standable, reachable cell that satisfies vanilla Touch, footprint included.</summary>
    Touch,
}

/// <summary>
/// Def-load consumer of optional-content kiss target registrations. Nothing here names a foreign
/// package or target: the registrations only exist when their conditional load root was adopted,
/// a missing target is diagnosed and skipped as a single entry, and duplicate target names are
/// diagnosed and excluded entirely instead of being resolved by XML enumeration order.
/// </summary>
[StaticConstructorOnStartup]
internal static class KissTargets
{
    private static readonly Dictionary<ThingDef, MWAH_KissTargetDef> registered = new();

    static KissTargets()
    {
        var byName = new Dictionary<string, MWAH_KissTargetDef>(StringComparer.Ordinal);
        var ambiguous = new HashSet<string>(StringComparer.Ordinal);
        foreach (MWAH_KissTargetDef registration in DefDatabase<MWAH_KissTargetDef>.AllDefs)
        {
            string name = registration.targetDefName;
            if (name.NullOrEmpty()) continue; // ConfigErrors already rejects the entry.
            if (ambiguous.Contains(name)) continue; // Already excluded; a third copy must not re-add it.
            if (byName.TryGetValue(name, out MWAH_KissTargetDef? earlier))
            {
                Log.Error("[MWAH] Duplicate optional kiss target registration: " + name
                    + " (" + earlier.defName + " / " + registration.defName + ")");
                byName.Remove(name); // Ambiguous: exclude both, never guess a winner by XML order.
                ambiguous.Add(name);
                continue;
            }
            byName[name] = registration;
        }

        foreach (KeyValuePair<string, MWAH_KissTargetDef> pair in byName)
        {
            ThingDef? definition = DefDatabase<ThingDef>.GetNamedSilentFail(pair.Key);
            if (definition == null)
            {
                Log.Warning("[MWAH] Optional kiss target missing: " + pair.Key);
                continue;
            }
            registered[definition] = pair.Value;
        }
    }

    /// <summary>Registration for this exact target, or null when the optional content is absent or unusable.</summary>
    internal static MWAH_KissTargetDef? For(ThingDef? definition)
        => definition != null && registered.TryGetValue(definition, out MWAH_KissTargetDef? registration) ? registration : null;

    internal static bool IsRegistered(ThingDef? definition) => definition != null && registered.ContainsKey(definition);
}
