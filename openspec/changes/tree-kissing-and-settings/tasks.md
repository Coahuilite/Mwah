## Work state

The current slice repairs existing paired/wall runtime behavior and researches Ideology tree content in parallel. Tree implementation, the settings redesign, and 1.7 packaging have not started. The scope is approved in [proposal.md](proposal.md); source-derived gaps and technical choices are in [design.md](design.md). This file owns implementation progress and verification evidence. No new unit tests or unit-test cases are to be written; use existing checks and real calling paths. Installation and in-game execution belong to the maintainer.

## 1. Shared runtime and version boundaries

- [ ] 1.1 Reconfirm affected 1.6/1.7 API/build inputs against the target; retain shared gameplay sources and the existing support contract.
- [ ] 1.2 Implement cross-target occupancy, paired readiness and matching identity, and actual dispatch-admission reporting.
- [ ] 1.3 Repair departure/fallback ownership and cleanup idempotency; preserve damage interruption and newer player commands.
- [ ] 1.4 Reconcile ambient preprocessing with the specified cooldown exemption; retain cooldown checks for manual requests when exemption is off.

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

- [ ] 4.1 Run OpenSpec strict validation and the existing mandatory offline gates. Adapt existing artifact inventories to new declared Defs/settings without weakening checks; record any unrelated baseline failure separately.
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
