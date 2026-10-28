## Context

The source baseline is the released 0.2.2 implementation: `KissUtility` dispatches mirrored jobs; `KissThingAddon` hosts object admission and fixed-scope settlement; `KissThingAddons.All` registers only walls; `MwahSettingsUI` renders one long page. Sources and public copy are stronger evidence of implementation than historical comments.

The local RimSage reference inspected on 2026-10-29 is `1.7.4990 rev1069`, explicitly confirmed by the maintainer. The service retained a validated full snapshot after its configured installation reverted to 1.6; that snapshot matches this change's target. It is not evidence of game execution.

## Decisions

### Share gameplay, isolate real version differences

Keep common gameplay sources and the two cohesive drivers. Do not introduce a general engine abstraction or scatter minor-version branches through admission and outcome rules. Compile references, loading metadata, native-menu/input integration, and any actual UI API differences are version-facing boundaries. Whether assemblies can be shared is a build and game-runtime finding, not a prerequisite to using the same gameplay sources.

`GUI.tooltip` is present in the target snapshot, so the earlier 1.7.4976 missing-member finding is not a current migration blocker. Tooltip rendering still needs actual UI acceptance. New settings presentation can use vanilla `TabDrawer`, `TabRecord`, and `TooltipHandler` rather than a UI framework dependency.

### Make activity ownership explicit

Share occupancy detection between paired and Thing requests without making core code depend on wall/tree implementations. Pair jobs need matching partner identity and a ready barrier before the initiator clock starts. Dispatch returns actual admission, not only proposal eligibility. Keep normal departure inside the job; register fallback only for an appropriate final exit, preserving idempotency and newer player orders. Vanilla whole-job finish actions are available; starting a job synchronously inside cleanup remains prohibited.

Prefer the vanilla finalizer-job handoff over a static next-tick queue when its invocation conditions distinguish successful natural exit from interruption on both supported APIs. A return fallback is only for normal completion still away from home; violence and replacement orders must not acquire a return prefix. The existing queue is an implementation detail, not a required product capability.

The selected handoff checks both `JobCondition.Succeeded` and advancement beyond the last departure toil; performance completion alone cannot authorize it. In the 1.7.4990 implementation, `ReadyForNextToil` advances the index before natural end, and `EndCurrentJob` obtains the finalizer before cleanup and starts it afterward. The 1.6 reference confirms the same API signatures, not its method bodies; actual 1.6 ordering remains a native-runtime acceptance point. Normal departure remains inside the kiss job for every admitted Pawn. Only the best-effort Goto fallback after the bounded departure uses vanilla's opportunistic-job gate, which excludes drafted, downed, and certain exceptional states.

Partner identity uses saved `Job.loadID` values. Legacy valid mirrored jobs can reconstruct identity only through reciprocal participant/initiator targets and the same dispatch tick. New `finished` and `returnTicks` fields preserve settlement idempotency and bounded departure across load; existing target, role, remaining-time, home, and narration fields keep their meanings. Departure reads the saved home each tick because rebuilding current toils on load need not rerun their initialization closures.

### Resolve addons once, use data for content

Use small Def-backed addon descriptions for stable identity, targeting category, outcome scope, settings metadata, and explicit match precedence; keep special behavior in workers. Reuse the Thing driver. Preserve `MWAH_KissThing` as the wall job identity; a tree JobDef can point through a ModExtension to its addon definition, making the selected identity durable without repurposing unrelated Job fields. Validate registry identity and precedence at Def initialization.

The shared targeter must include plants, with the active addon predicate still filtering actual targets and fog. Replace wall-specific shared tooltip wording through addon-owned metadata. Do not add ThingComps to every tree or wall just to make it kissable.

### Cultural settlement is a tree policy, not another target addon

Add a contextual outcome resolver (for example, `ResolveFateScope(Pawn, Thing)`), defaulting to the addon's basic scope. The tree policy checks the integration preference, `ModsConfig.IdeologyActive`, the acting Pawn's `Ideo`, and declared matching rules at settlement. Keep cultural Def references optional; neither About metadata nor basic tree content requires Ideology.

Use `Trees_Desired` for tree-affirming outcomes. A specialized Gauranlen rule also checks `GauranlenConnection_Strong` and `CompTreeConnection.ConnectedPawn`. Resolve overlapping rules once rather than stacking a meme bonus on its own precept bonus. Do not call `Prune`, link/ritual events, or mutate native sightings. `ThoughtWorker_TreesDesired` reads surroundings and is not a completed-kiss memory.

Basic and cultural outcomes use the same new tree ThoughtDef and refresh the same slot. Preserve `wall`, existing Def names, narration fields, setting names, and their meanings. Use distinct new identities for tree and cultural preferences. Numerical tree defaults and outcome weights must be specified before editing the corresponding runtime content; they are not inferred from the wall distribution.

Tree content uses the same five mood anchors as walls and the same 30000-tick default duration, with its own slot. The specified basic/affirming/own-connected weights increase the likelihood of a positive result rather than its reward ceiling. Addon descriptors own default enable/duration and job bindings; optional XML culture-rule entries reference native precepts only under `MayRequire` for Ideology. No optional native DefOf field is necessary.

The focused [tree/Ideology research](../../../docs/research/tree-kissing-ideology.md) records the native Def/API evidence and wall-pipeline comparison. Use own connected Gauranlen plus the strong-connection precept before the general tree-affirming precept, otherwise basic settlement. Shared presentation needs a neutral target argument for new content while retaining the existing `WALL` alias and saved narration meanings. Other DLC trees remain ordinary eligible tree targets, not evidence for a Gauranlen cultural rule.

### Separate settings data from presentation

Keep `MwahSettings` and its serialized keys. Move page, scrolling, expansion, and pending-field editing into a presentation owner invoked from the existing mod settings entry point. Preserve the existing measured header, numeric parsing/quantization, tick display, and coalesced save path. Resolve edits before hiding a control. Each page has its own measured height and scroll position. Render addon controls from their descriptors; keep cultural controls nested under tree kissing.

The view retains an active numeric edit's commit callback and range, so hiding or closing it can resolve the value without drawing the old control. Debounced writes persist canonical values only; the external settings-close write resolves the active edit before serialization. A descriptor flag exposes cultural options without branching on target identities in the shared settings view.

## Source findings and verification implications

| Finding | Consequence / relevant check |
| --- | --- |
| Pair drivers advance on their own arrival, not joint readiness | Observe staggered arrival and require the shared clock to remain stopped |
| Performance finish actions enqueue returns; the GameComponent drains after Pawn ticks, with no origin-job guard | Observe a long normal departure and a newer player order; neither may be stolen by fallback |
| Pair occupancy checks only `MWAH_Kiss`; Thing admission checks both families | Exercise a paired request against an ongoing wall/tree kiss |
| `BeginDirected` returns eligibility even when `Start` admission fails | Exercise reservation failure through the director, checking its success message and partner cleanup |
| Ambient preprocessing unconditionally checks the Pawn cooldown before `Propose` | Reconcile it with the cooldown-exemption contract without changing the maintainer's manual-command decision |
| Thing settlement re-runs first-match target recognition | Check saved activity identity with overlapping registered target rules |
| Pending numeric edits are processed only when that field draws | Inspect a valid edit followed immediately by tab change or addon collapse |

These are source-derived gaps, not reproduced game failures. Verification work and its state are recorded only in [tasks.md](tasks.md).

## Migration and delivery

Retain existing entry points, 1.6 support, save identities, and settings keys. Add target-specific loading/build inputs only where needed. Existing wall jobs continue resolving as walls. No schema change may silently reinterpret their fields. Do not change product version, declare 1.7 runtime acceptance, install into a game, publish, or archive this change merely because specification validation succeeds.

Historical engineering reasoning remains in `docs/design-decisions.md`; behavior authority is the baseline plus these approved deltas. Exact tree balance is a content decision to add to the relevant spec before content development, not an implementation-status note inside a spec.
