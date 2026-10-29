# Tree kissing: reuse the Thing job, specialize the outcome

## Recommendation

Treat trees as a second non-Pawn target family alongside walls. Reuse the single-actor admission, reservation, movement, performance, interruption, and departure pipeline. Keep Ideology inside the tree outcome policy, evaluated once at settlement. It should select a different Mwah! fate table, not impersonate a native tree interaction.

The useful first cultural rules are `Trees_Desired` and a more specific own-Gauranlen rule requiring `GauranlenConnection_Strong` plus the target's actual connection to the acting Pawn. Tree-cutting disapproval, a ritual named after a tree, and another DLC's special tree are not interchangeable cultural signals.

Product contracts are defined only by [the current addon specification](../../openspec/specs/addons/spec.md), linked from [the specification entry](../specification.md). This document records research and implementation recommendations, not a second specification or an implementation checklist.

## Evidence and build boundary

Inspected on 2026-10-29 through **local RimSage**, using its full validated **RimWorld 1.7.4990 rev1069** snapshot. Automatic refresh had failed; the retained snapshot is the build explicitly confirmed by the maintainer. The queried `Assembly-CSharp` source has no reported decompilation warnings. Official XML inheritance merging does not establish the final Def database of an arbitrary mod list.

Mwah!'s inspected baseline is 0.2.2, targeting `net472` with `Krafs.Rimworld.Ref` 1.6; project-code observations below refer to that baseline. No game session or 1.7 mod execution was performed for this research. These integration points are ordinary game/Def APIs; none establishes that tree kissing needs a 1.7-only gameplay core.

Source locators below are relative to the RimSage snapshot. They identify game material without redistributing it.

## Which cultural content means what

| Native content | Observed meaning | Consequence for tree kissing |
| --- | --- | --- |
| `MemeDef TreeConnection` | Its `requireOne` groups require `Trees_Desired`, `RoughLiving_Welcomed`, `Temperature_Tough`, and `GauranlenConnection_Strong`. | Match the relevant precept rather than award an additional bonus for its meme. |
| `PreceptDef Trees_Desired` | Adds situational `TreesDesired` and `TreeDensityReduced` thoughts. | Strongest general signal for tree-affirming Mwah! narration and a more positive fate distribution. |
| `TreesDesired` | `ThoughtWorker_TreesDesired` reads the Pawn's recent tree sightings by category and time window. | Proximity during a kiss can naturally affect vanilla surroundings processing. Do not manufacture a sighting or grant this situational thought as a kiss memory. |
| `TreeDensityReduced` | `ThoughtWorker_Precept_TreeDensity` reads the map's player-responsible tree-destruction count. | A kiss neither repairs nor forgives tree destruction. Do not clear the tracker or offset this thought by invoking its worker. |
| `GauranlenConnection_Strong` | Requires the tree-connection meme, supplies `GauranlenConnectionDesired`, a `PruningSpeed` offset, and development points for `PlantedGauranlenTree`. It is not normally generated for NPC factions. | Use the acting Pawn's actual precept. A target-specific special outcome also needs `CompTreeConnection.ConnectedPawn == doer`. Do not impose the native precept's NPC-generation restriction on kiss admission. |
| `GauranlenConnectionDesired` | A situational worker examines native connected things and expectation/join-time conditions. | A pleasant kiss does not satisfy the native desire for an actual connection. Keep Mwah!'s memory separate. |
| `TreeCutting_Disapproved`, `TreeCutting_Horrible`, `TreeCutting_Prohibited` | Nature-primacy-associated responses to `CutTree` and `MinifiedTreeDied`; the prohibited variant also refuses cutting. | These describe harm, not the value of kissing. Do not infer an automatic positive kiss bonus, a new prohibition, or an atonement mechanic. |
| `PreceptDef TreeConnection` | A hidden, classic Gauranlen connection ritual; it shares its name with the **MemeDef**, but is a different Def type. | Its presence is not a suitable culture predicate. Never dispatch it as the implementation of a kiss. |
| `DateRitualConsumable` / `ChristmasTree` | A tree-party ritual centered on a consumable **Building**, not a living `Plant`. | Outside the approved game-marked-tree target predicate. A future ceremonial-object addon would be a separate decision. |

Primary XML: `Defs/Ideology/MemeDefs/Memes_Misc.xml` (`TreeConnection`, line 1458); `Defs/Ideology/PreceptDefs/Precepts_Trees.xml` (precepts at lines 26 and 56; thought Defs at lines 96, 135, and 220); `Defs/Ideology/PreceptDefs/Precepts_TreeCutting.xml`; `Defs/Ideology/PreceptDefs/Precepts_Ritual.xml` (party at line 81, connection ritual at line 198); `Defs/Ideology/ThingDefs_Buildings/Buildings_Ideo.xml` (`ChristmasTree`, line 673); `Defs/Ideology/MemeDefs/Memes_Primacy.xml` (`NaturePrimacy`, line 96).

Primary behavior: `Source/Assembly-CSharp/RimWorld/ThoughtWorker_TreesDesired.cs`, `ThoughtWorker_Precept_TreeDensity.cs`, `ThoughtWorker_GauranlenConnectionDesired.cs`, and `TreeDestructionTracker.cs`.

## Target the game's tree category, not a species list

Use a spawned, non-destroyed `Plant` whose `def.plant.IsTree` is true. `PlantProperties.IsTree` recognizes the `Wood` harvest tag or `forceIsTree`. This admits loaded mod trees using the same marker without adding a whitelist, a growth threshold, or an Ideology gate.

Representative native targets:

| Target | Content owner | Relevant distinction |
| --- | --- | --- |
| Oak | Core | Ordinary trees use the game tree marker. |
| Bonsai | Ideology | A miniature tree still uses the game tree marker. |
| Gauranlen | Ideology | A `Plant` with `CompTreeConnection`; this is the target for the own-connection rule. |
| Anima | Royalty | A tree with meditation/psylink systems, not an Ideology Gauranlen connection. Basic tree kissing does not need Royalty. |
| Polux | Biotech | A tree with a pollution-pump comp. A kiss does not perform pollution removal. |
| Harbinger | Anomaly | `HarbingerTree : Plant`; its feeding, nutrition, growth, and propagation are separate mechanics. No dedicated cultural kiss rule is justified by the inspected tree precepts. |

The latter examples remain eligible when their content is loaded because of the generic predicate, not because Mwah! takes a required dependency on those DLCs.

Primary locators: `Source/Assembly-CSharp/RimWorld/PlantProperties.cs` (`IsTree`, line 207); `Defs/Core/ThingDefs_Plants/Plants_Wild_Temperate.xml` (`Plant_TreeOak`, line 144); `Defs/Ideology/ThingDefs_Plants/Plants_Special.xml` (bonsai at line 44, Gauranlen at line 123); `Defs/Royalty/ThingDefs_Plants/Plants_Wild.xml`; `Defs/Biotech/ThingDefs_Plants/Plants_Special.xml`; `Defs/Anomaly/ThingDefs_Plants/Plants_Special.xml`; `Source/Assembly-CSharp/RimWorld/HarbingerTree.cs`.

## Native seams to read, and mechanics to leave alone

- **Culture:** `Pawn.Ideo` is nullable and belongs to that Pawn. `Ideo.HasPrecept(PreceptDef)` checks actual precept membership. Do not substitute the colony's primary ideology, a faction's ideology, or a meme-name string.
- **Connection identity:** `CompTreeConnection.ConnectedPawn` is a public read-only identity view. Read it for the special outcome. `ConnectionStrength` is mutable and `Prune(int delta)` changes both strength and the last-pruned tick; neither belongs in kiss settlement.
- **Ritual ownership:** `RitualOutcomeEffectWorker_ConnectToTree.Apply` grows moss and calls `ConnectToPawn`; `RitualOutcomeEffectWorker_AnimaTreeLinking.Apply` calls `CompPsylinkable.FinishLinkingRitual`. These are state-changing ritual completions, not reusable kiss reward helpers. Do not emit planting/linking history events either.
- **Surroundings:** `Pawn_SurroundingsTracker` periodically collects sightings for eligible awake Pawns. `IdeoUtility.TreeSightingsNearPawn` scans nearby unfogged, line-of-sight tree Plants. Its returned list is scratch data. Keep these systems and their tracking lists untouched. Disabling Mwah!'s cultural integration cannot disable the Pawn's vanilla environmental thoughts.
- **Map input:** `TargetingParameters.canTargetPlants` is the native plant-category gate. Enabling it in the existing shared targeter is necessary in addition to the addon validator; a buildings-only targeter will reject a tree before the predicate can make it eligible.
- **Metadata:** `Def.GetModExtension<T>()` and `DefModExtension.ResolveReferences` / `ConfigErrors` provide ordinary metadata and validation seams. No component needs to be injected into every tree or wall.

Primary locators: `Source/Assembly-CSharp/Verse/Pawn.cs` (line 582); `Source/Assembly-CSharp/RimWorld/Ideo.cs` (`HasPrecept`, line 1307); `CompTreeConnection.cs` (`ConnectedPawn`, line 54; `Prune`, line 414); `RitualOutcomeEffectWorker_ConnectToTree.cs`; `RitualOutcomeEffectWorker_AnimaTreeLinking.cs`; `Pawn_SurroundingsTracker.cs`; `IdeoUtility.cs` (`TreeSightingsNearPawn`, line 636); `TargetingParameters.cs` (field at line 27, plant branch at line 293); `Source/Assembly-CSharp/Verse/Def.cs` and `DefModExtension.cs`.

## Wall is the execution reference, not the cultural template

The 0.2.2 wall path is: shared right-click/director entry → addon proposal → reachable adjacent stand cell → `MWAH_KissThing` → cooldown/home capture → approach → timed performance → settlement → shared departure. Only the Pawn owns a job; the target is reserved rather than assigned a mirrored participant job. This is also the right tree execution model.

Reuse these mechanisms after the current runtime repairs. Keep the wall identity `wall`, its `MWAH_KissThing` JobDef, five-stage memory, duration preference, distribution, and narration fields intact. Do not branch paired-kiss staging or paired SocialImpact settlement to accommodate trees.

The small extension seams still needed for trees are:

1. **A Def-backed addon descriptor plus a worker.** Describe stable settings identity, target category, JobDef, basic fate scope, labels/tooltips, defaults, and explicit match precedence. Resolve duplicate identities and ambiguous equal-priority target matches during Def initialization. There is no need for a general expression/rule engine.
2. **Durable selected identity.** Use a new tree JobDef pointing to its addon descriptor through a ModExtension, while the existing wall JobDef remains wall-bound. Both use `JobDriver_KissThing`. Resolve the admitted activity from its saved JobDef, not by calling a first-match target classifier again at settlement. Preserve old wall jobs without reinterpreting their target fields.
3. **One contextual outcome hook.** An interface such as `ResolveFateScope(Pawn doer, Thing target)` defaults to the descriptor's basic scope. Only the tree worker evaluates cultural rules. Its output feeds the existing fate/memory pipeline once.
4. **Target-neutral presentation.** The shared menu tooltip, failure substitution, message translation, and memory-description translation currently use wall-specific wording or the `WALL` argument. Add a neutral target argument for new content while retaining the existing `WALL` alias and saved `MWAH_narrationKey` / `MWAH_narrationSubject` meanings. Do not require a saved-memory migration merely to rename an in-memory translation argument.
5. **Settings as descriptor consumers.** Existing `addonSwitches` and `addonThoughtDurations` already key by stable addon identity. Add a distinct tree identity and a separate cultural preference; turning culture off is not the same as turning tree targeting off. The future extension-first page can nest culture controls beneath trees without a second competing tree addon.

Project references: `Source/Mwah/Actions/Kiss/KissThingAddons.cs`, `KissWallAddon.cs`, `FloatMenuOptionProvider_KissThing.cs`, `KissDirector.cs`; `Source/Mwah/Jobs/JobDriver_KissThing.cs`; `Source/Mwah/Rewards/KissFate.cs`, `Thought_MemoryFated.cs`; `Source/Mwah/MwahSettings.cs`.

## Select one outcome policy, then grant one tree slot

Recommended precedence at **settlement**, consistent with the approved delta:

```text
tree kiss completed
  culture preference off / Ideology absent / Pawn.Ideo absent -> basic tree scope
  matching strong-connection precept + target connected to this Pawn -> own-Gauranlen scope
  matching Trees_Desired precept -> tree-affirming scope
  otherwise -> basic tree scope
```

Culture changes during approach/performance therefore affect settlement without reclassifying the activity or canceling it. An unconnected or somebody else's Gauranlen tree falls through to the Pawn's general tree policy; it is not a failed kiss.

Keep basic and both cultural tables on **one new Mwah! tree ThoughtDef**. `KissFate` already renews a non-Pawn relationship slot by `(Pawn, ThoughtDef, otherPawn == null)`, replacing its stage, narration, and duration. Sharing that ThoughtDef prevents basic/cultural toggling from stacking separate memories; a different ThoughtDef keeps the wall slot independent. Moodless Pawns continue through the applicable message channel without resource substitutes.

For DLC isolation, prefer cultural rule entries conditional on `MayRequire="Ludeon.RimWorld.Ideology"`; core tree descriptors and basic fate/Thought Defs stay unconditional. If C# DefOf fields are used, Mwah! must declare its own appropriately `[MayRequireIdeology]` fields: native `PreceptDefOf` has **no** `Trees_Desired` or `GauranlenConnection_Strong` field in this snapshot. `MemeDefOf.TreeConnection` does exist, but using it instead would change the rule's meaning. Check the effective DLC/preference gate before consulting optional rule references. An unavailable optional rule resolves to the basic policy, without a required-DLC error.

Primary locators for optional loading: `Source/Assembly-CSharp/RimWorld/PreceptDefOf.cs`, `MemeDefOf.cs`, `MayRequireIdeologyAttribute.cs`; native cross-DLC examples in `Defs/Core/CultureDefs/Cultures.xml`.

## Content decision handoff

These were the content decisions identified by the research. Their selected values now belong to the [current addon specification](../../openspec/specs/addons/spec.md); this list records the research handoff, not another current decision queue:

- Tree addon default enable state, default thought-duration ticks, and cultural-integration default preference.
- The new tree memory's stage meanings and mood values, including whether all policies share the wall's stage count or choose a different tree-specific ladder.
- Basic, tree-affirming, and own-Gauranlen relative fate weights and their empty-table fallback content. More narration rows must not accidentally increase a stage's aggregate probability. General tree-affirming content must demonstrably have greater positive-outcome likelihood than basic content.
- Stable new Def/settings identities and bilingual target-neutral text. These become migration-sensitive once shipped.

Prefer extending the current fixed-stage fate format over adding a second reward system. Give cultural content more fitting narration and a different distribution before considering a larger mood ceiling; vanilla tree culture already supplies independent situational effects. This is a balance recommendation, not an approved numerical choice.

The development order is: finish shared-runtime repairs; specify the outstanding content defaults; introduce descriptor/identity and generic presentation seams while preserving walls; deliver basic trees with Core alone; add the optional cultural policy and content; then integrate the settings presentation. Maintainer acceptance should use the existing real paths for Core-only trees, culture off/on, different Pawn ideologies, own/other/unconnected Gauranlen targets, memory replacement, and unchanged pruning/linking/sightings. Source inspection supports the design; it does not replace those game observations.
