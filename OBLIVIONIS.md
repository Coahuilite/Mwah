# OBLIVIONIS

Cold archive. Read only on a historical conflict, a resurrected topic, or an explicit maintainer request; nothing in this file can override `MEMORY.md`, `AGENTS.md`, or the source.

Entry format: `## <date> — <topic>` + `source:` + `reason:` + compressed conclusions. No raw logs, session narratives, or completed task lists.

## 2026-09-02 — The permissiveness booleans (allowHostileTargets / allowMoodless)

source: commit `01d0c44` (renamed `allowNonHumanlike` to `allowMoodless`, moving the test from race to the runtime `needs.mood`) and the later gate-slider change.
reason: each boolean expressed one facet of "how far the range widens", and the combination still could not draw the line players actually asked for (matchmake only the colony). Once the seven-tier `KissScope` collapsed the same dimension into a monotone ladder, `allowHostileTargets` became exactly "tier ≥ 3" and `allowMoodless` exactly "tier ≥ 2/5" — true subsets, deleted. The renaming conclusion (the test must be the runtime need, never `RaceProps.Humanlike`) **remains valid**; it merely moved into `KissBoundary.HasMood` and the tier definitions.
replaced by: the seven-tier gate in `Source/Mwah/Actions/Kiss/KissScope.cs` (`MwahSettings.pairScope`).
status: superseded.
evidence: under the factory default `KissScope.Everything` the gate test is constantly true and behaves bit-identically to both old booleans being true (the structure layer holds no permissiveness filter at all).
revival condition: if a future need requires cross-tier combinations (e.g. own animals yes but raiders no), the single slider cannot express it — add a control along the second dimension then; do not resurrect these booleans.

## 2026-10-04 — 0.1.0 development-cycle narrative and retired matrix rows (noise-reduction archive)

source: the completion narrative rows in TODO "entry point" (2026-09-21 wrap-up / 09-22 panelization and its second correction / 09-28 addon+fate tables and the truce window / 09-30 logging surface and first-launch landmines / 10-04 seat-swap deadlock), the naming-candidate elimination rationale in MEMORY, the disproven `checkOverrideOnDamage` claim, and the matrix rows listed below.
reason: completed work narratives and obsolete checks no longer guide future behavior; their durable conclusions have all been absorbed into MEMORY/AGENTS/code comments — only compressed stubs remain here for historical verification.
status: archived.

### Completion narrative (details in git log)

- 09-21 wrap-up: deleted `Mwah.slnx` (no solution files since; the IDE opens the csproj); decision-chain cleanup (one evaluation returning the reason, dev instrumentation without #if in performance code, `MwahMod.Settings` never null, ambient per-cycle cap, cooldown sweep dedup, settings sliders collapsed into two generic helpers); fixed the **Steam / GitHub channel compile failures** and added the channel build gate; mood values re-anchored with the `Psychopath`/`Inhumanized` nullify lists; two research deliverables (mood ladder + linkage hooks; player HUD surfaces → `docs/hud-ui-surfaces-1.6.md`).
- 09-22: director panelized (non-modal, `UI.screen*` logical canvas, draggable, re-anchoring) + second correction (title row in the height formula, WordWrap frame-end contract, half-heart fill) + settings numeric fields + wall-kissing shipped (five tiers −2/0/+1/+3/+6, weights 20/30/25/17/8, own switch default on, outside the gate).
- 09-28: addon-layer rework + fate tables `MWAH_FateDef` + per-instance narration (zero patches); truce window trio (same-frame dual job start, in-job return leg, completed-flag exit) + combat gate + `Notify_DamageTaken` damage self-destruct gate.
- 09-30: logging overhaul (unconditional startup banner + runtime diagnostics switch); first-launch incidents fixed same round: the **fate table dropped whole** (Mwah-namespace class invisible to the def loader → moved to RimWorld + verify gate 6b) and the **settings page painting overflow outside the window** (broken auto-measure → cross-frame cached height + single-column Begin).
- 10-04: seat-swap deadlock fixed (`Usable` rejects the partner's current cell) + all silent exits instrumented; cloud onboarding (`Coahuilite/Mwah` public, MPL 2.0, CI/privacy gate/README/CONTRIBUTING).

### Retired matrix rows (with reasons)

- Crash discriminators A/B/C and the "recursion regression" row: the real culprit was settled on 2026-09-04 (finish-action synchronous StartJob recursion) and fixed (queue + idempotency gates); the Steam-overlay/Defender hypotheses were falsified the same day. Diagnostic rows have completed their mission.
- "Hang localization" row: an operating instruction, not a check — merged into MEMORY's debugger-choice entry and the logging-surface row.
- "Mood switch tightening (turn off 'include moodless creatures')": that setting was deleted with the seven-tier gate rework; the row pointed at a control that no longer exists; moodless visibility is covered by the gate tiers.
- "Hostile and faction: ordering a raider at tier 6 → may counterattack immediately, expected": directly contradicted by the truce-window ruling (no counterattack during approach+performance+return now); replaced by the "truce window" and "passive side no longer shoots during approach" rows.
- "No mutual circling: only the smaller-thingID side walks": a relic of the Touch-chase era (under the fixed stage both walk to their own cells); covered by "side-by-side facing" and "passerby steals the stage".

### Naming candidate elimination rationale (the settled decision lives in MEMORY "Naming decisions")

`KISS_`: most literal but highest collision noise in Workshop search (note: **not** occupied by vanilla — rimsage verified zero defNames containing "kiss"; only a "deeply kissing" string in `Tales_DoublePawn_Relationships.xml`); `PECK_`: a real word for a light kiss but diluted by "bird-peck/tap" senses and cannot be spelled forward from the full name; `CHUU_` (ちゅっ): works for CN/ACG audiences, unintuitive for English players; `XOXO_`: the X reads as execution/cancel and carries hug semantics we never implemented; `SMOOCH_`: prefix too long. Token-occupancy audit: `Mwah / Peck / Chuu / Snog / Xoxo / Smooch` are all unoccupied in 1.6+Odyssey defNames and `Defs/Core/Names` name parts (rimsage, empty results). MWAH is deliberately **not** an initialism — it is onomatopoeia; the full name carries the "every pawn kisses each other" meaning, the abbreviation carries the mouth noise in menus and logs.

### Damage-break stale claim (the corrected conclusion lives in MEMORY "truce window")

On 2026-09-28 MEMORY recorded "checkOverrideOnDamage=Always makes one shot break the kiss" — that holds only on the right-click path (and lags up to 180 ticks); for playerForced jobs the vanilla chain never fires at all. Kept as a stub against unverified mechanism claims.

### Branch lineage

`dev` created 2026-09-02 (the first 9 commits actually landed on main; dev was fast-forwarded onto main without touching history). On 2026-10-04 `dev` retired in favor of the per-minor branch `0.1.x` (identical content at retirement); `main` contracted to release-only.
