using System.Collections.Generic;
using Verse;

// Same XML short-name rule as MWAH_FateDef: a Def's XML root element resolves by class short name
// only inside the engine's whitelisted namespaces, so this type cannot live in the Mwah namespace.
namespace RimWorld;

/// <summary>
/// One optional-content kiss target registration (pure data; instances exist only in a conditional
/// load root). Target name, stand policy and presentation key are declared by content, never
/// compiled into the base assembly, and are resolved once after Def load by <c>KissTargets</c>.
/// </summary>
public class MWAH_KissTargetDef : Def
{
    /// <summary>Exact ThingDef defName resolved at runtime. Missing target skips this entry only.</summary>
    public string targetDefName = "";

    /// <summary>Forward = central interaction cell plus one cell per side; Touch = any legal vanilla Touch cell.</summary>
    public Mwah.KissTargetStandPolicy standPolicy;

    /// <summary>Dedicated Keyed tooltip consumed with this registration; empty = the shared machine copy.</summary>
    public string tooltipKey = "";

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (string error in base.ConfigErrors()) yield return error;
        if (targetDefName.NullOrEmpty()) yield return "A kiss target registration needs a targetDefName.";
    }
}
