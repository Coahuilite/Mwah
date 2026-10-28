## Work state

The existing paired/wall runtime repair is implemented, and Ideology tree research is complete. Offline verification and maintainer acceptance are recorded below. Tree implementation, the settings redesign, and 1.7 packaging have not started. The scope is approved in [proposal.md](proposal.md); source-derived gaps and technical choices are in [design.md](design.md). This file owns implementation progress and verification evidence. No new unit tests or unit-test cases are to be written; use existing checks and real calling paths. Installation and in-game execution belong to the maintainer.

## 1. Shared runtime and version boundaries

- [x] 1.1 Reconfirm affected repair APIs/build inputs against 1.6 references and the target 1.7 snapshot; retain shared gameplay sources and the existing support declaration. Native-runtime acceptance remains separate.
- [x] 1.2 Implement cross-target occupancy, paired readiness and matching identity, and actual dispatch-admission reporting.
- [x] 1.3 Repair departure/fallback ownership and cleanup idempotency; preserve damage interruption and newer player commands.
- [x] 1.4 Reconcile ambient preprocessing with the specified cooldown exemption; retain cooldown checks for manual requests when exemption is off.

## 2. Addons and tree content

- [x] 2.0 Research native Ideology tree content and wall-pipeline reuse; record source evidence and remaining content decisions without implementing trees.
- [ ] 2.1 Implement stable addon definitions/resolution, durable activity identity, plant targeting, and addon-owned menu text while preserving wall behavior.
- [ ] 2.2 Select and write tree numeric defaults and fate weights into the affected spec before authoring runtime content.
- [ ] 2.3 Implement basic tree content, the contextual outcome resolver, and optional per-Pawn Ideology rules; keep one tree thought slot and native tree systems unchanged.
- [ ] 2.4 Complete bilingual Def/localization content and stable new settings identities; retain existing wall keys and fallback behavior.

## 3. Settings presentation

- [ ] 3.1 Separate transient view state from saved settings and implement the two navigation tabs with extension-first default content.
- [ ] 3.2 Implement addon expansion, culture availability, independent page scrolling, and edit commit before hiding controls; preserve save retry/close semantics.
- [ ] 3.3 Update user documentation to describe the delivered UI and functionality, not merely the intended design.

## 4. Verification and acceptance evidence

- [x] 4.1 Run OpenSpec strict validation and the existing mandatory offline gates for the existing-runtime repair candidate. Adapt the existing artifact inventory to its replacement runtime owner without weakening checks. Future tree, settings, and packaging changes require their own applicable reruns.
- [ ] 4.2 Maintainer: verify startup identity, bilingual labels, fate Def resolution, all initiation paths, director slot/heart/quick-chain interactions, and normal paired phase progression on supported game minors.
- [ ] 4.3 Maintainer: verify staggered arrival, reservation failure, cross-target occupancy, long departure, violent interruption, and a replacement player order through their actual game paths.
- [ ] 4.4 Maintainer: verify wall stage replacement, narration and duration, empty-table fallback, moodless channels, non-repeating shared-table settlement, scope tiers, and silent invalidation.
- [ ] 4.5 Maintainer: verify Core-only basic trees, cultural integration off/on, different Pawn cultures, own/other Gauranlen trees, shared tree slots, and unchanged native tree mechanics.
- [ ] 4.6 Maintainer: inspect both settings pages at relevant UI scales, including pinned header/navigation, exact numeric inputs, tab/collapse commits, three-unit time display, diagnostics, restore defaults, and retained disabled preferences.
- [ ] 4.7 Maintainer: verify existing preferences/memories and mid-job save/load, including addon identity and per-game state reset; record actual package and game-build inputs with results.
- [ ] 4.8 Reconcile results against the specs, complete any required in-scope repairs, and archive only after applicable acceptance is satisfied. Public release operations still need their own authorization.

## Specification preparation evidence

2026-10-29: the installed OpenSpec CLI was upgraded from 1.2.0 to stable 1.15.0, and the standard layout was initialized with no generated tool integrations. Strict validation accepted the four baseline capabilities and this change, including preservation of baseline scenarios in modified requirements.

The existing `pwsh scripts/verify-local.ps1` passed on the unchanged runtime source, including Dev/Steam/GitHub builds with zero warnings and errors. No game session or 1.7 runtime acceptance was performed. These results do not complete the implementation tasks above; their checks apply again after the corresponding runtime changes.

## Tree research handoff

2026-10-29: [tree-kissing-ideology.md](../../../docs/research/tree-kissing-ideology.md) records local RimSage 1.7.4990 rev1069 Def/API observations, native-system boundaries, optional-DLC behavior, and the shared Thing-job extension seams. The research supports the approved cultural direction; tree defaults, stage values, fate weights, and new persistent IDs remain content decisions before tree implementation. No tree runtime content or settings redesign was implemented by this research.

## Existing-runtime repair evidence

2026-10-29: the repair candidate passed `pwsh scripts/verify-local.ps1`, including Dev/Steam/GitHub 1.6 builds with zero warnings and errors. The first run found one stale artifact-inventory expectation for removed `KissReturnQueue`; replacing that expected internal symbol with `KissActivity` retained the inventory assertion and all other symbols. The corrected existing gate suite passed in full. OpenSpec strict validation passed all four baseline capabilities and the active change; `git diff --check` passed.

Full shared sources also compiled against the retained 1.7.4990 snapshot's native assemblies under .NET 10: zero errors, six warnings. The warnings concern unchanged generic dictionary nullability (`CS8714`) and the existing `CallerArgumentExpressionAttribute` shim colliding with .NET 10 (`CS0436`). This is API-compilation evidence, not a packaged 1.7 build or game execution; native-target warning cleanup belongs with the later version/build slice.

Maintainer acceptance remains open for staggered arrivals, failed reservations and actual dispatch feedback, wall/paired occupancy, long departure, violent interruption, replacement orders, and mid-performance/mid-departure save/load. In particular, 1.6 finalizer invocation ordering has only matching reference signatures, not an inspected implementation body. Do not archive the full change or declare native-runtime acceptance from these compiler results.
