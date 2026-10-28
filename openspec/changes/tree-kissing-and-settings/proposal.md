## Why

The maintainer approved optional Ideology-aware tree kissing and an extension-first settings surface. Shared runtime boundaries also need tightening so adding trees does not spread addon or game-version branches through paired kissing.

## What Changes

- Keep basic tree kissing usable without Ideology or with cultural integration disabled.
- Add per-Pawn cultural outcome selection without replacing tree connection, pruning, ritual, or environmental-thought mechanics.
- Generalize addon discovery, stable activity identity, shared occupancy, and target selection.
- Establish paired readiness, truthful dispatch results, and safe normal/fallback departure.
- Replace the long settings page with two navigation tabs, keeping addon management prominent.
- Support RimWorld 1.7 while preserving the 1.6 contract and existing persistent identities.

## Capabilities

### New Capabilities

None; tree kissing extends the existing addon capability.

### Modified Capabilities

- `kissing`: paired synchronization, cross-target occupancy, dispatch results, and departure safety.
- `addons`: stable resolution, basic trees, and optional cultural settlement.
- `settings`: two-page navigation, nested extension controls, and edit handling across views.
- `compatibility`: 1.7 support and optional-content isolation.

## Impact

Affected areas are the request entry points, paired and Thing job drivers, addon registration, fate/Thought Defs, localization, settings rendering, and version/build packaging. No new runtime mod dependency or Harmony patch is authorized. Publication, installation, and game execution remain the maintainer's operations.

## Decisions

- Scope and the two-page direction were approved by the maintainer on 2026-10-29.
- The target 1.7 reference build is `1.7.4990 rev1069`.
- The maintainer explicitly retained the released manual-command policy: manual requests check cooldowns unless `noCooldowns` is enabled. Player-issued status alone is not an exemption.
- Specifications contain contracts only. Implementation state and evidence are owned by [tasks.md](tasks.md); technical choices are owned by [design.md](design.md).
- The initial execution slice repaired existing paired/wall runtime behavior while researching Ideology-aware tree kissing in parallel. Subsequent tree/settings implementation and version-specific packaging are separate slices.
- The maintainer subsequently authorized tree content and the two-page settings implementation, with defaults, weights, and cultural rules specified first. Version-specific packaging and publication remain separate operations.
