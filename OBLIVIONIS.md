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

## 2026-10-05 — 0.2.x cycle: live-test findings, rulings, and the 0.2.1 acceptance record

source: live sessions 2026-10-04/05 (maintainer-executed matrix runs on dev packages `fc7ebe5`→`c8c4b60`), decompiled 1.6.4871 evidence, commits `7638cb6`…`8f6ce79` plus the uncommitted release-prep docs.
reason: the 0.2.1 live matrix served its purpose and is retired from `TODO.md` down to a regression checklist; the superseded artifacts of this cycle need one cold record so nobody resurrects them.
status: archived.

**What live testing caught** (all fixed and re-verified in-session): the same-frame dual-start startup race that made every two-person dispatch self-abort (mechanism and general law live in MEMORY — this is the ledger that the matrix exists to catch exactly this class); director portraits rendered `Rot4.North` = back of the head (South faces the camera; all six explicit-rotation vanilla call sites agree — MEMORY's 09-22 nail was wrong and is corrected); pinned settings header clipped twice because band heights were guessed, fixed by measuring with `Text.CalcHeight` (two-axis rule in MEMORY).

**Rulings of the cycle** (full text in MEMORY "0.2.x mood architecture rulings" and "Logging ruling"): entities = mech-equal treatment with a dedicated `KissPawnMutant` table and body-agnostic (psychology-only) copy; monolyn-style race mods clarified as ordinary humanlike colonists, not entities; no cross-group mood cap (relationship slots independent); per-addon settings dictionaries keyed by stable addon ids; four-section settings page with registry-grown add-on block; diagnostics as a four-rung slider (Off/Auto/Simple/Verbose); channel-tied build identity (dev = version+full sha, release = bare version) pinned in a measured-height header; dev rehearsal = folder, zip opt-in and deterministic; Chinese copy says "Pawn" where the referent is not human-only; FerriteLib considered and rejected as a dependency (measure-not-guess is a vanilla primitive).

**Superseded artifacts — do not resurrect**: five separate `MWAH_KissedWall_*` thought defs (now one def + 5 stages, `stageIndex` on fate rows); the `MWAH.Fail.SociallyIncapable` rejection key (deleted with the mutant gate); the `wallKissing` bool field (now `addonSwitches["wall"]`); the `diagnosticLogs` bool (now `diagnosticLevel`); hand-typed settings key literals (now derived from field names); old Keyed hand names `Enabled`/`Duration`/`FleckInterval`/`PawnCooldown`/`PairCooldown`/`ReturnHome`/`Autonomous`/`AutonomousInterval`/`AutonomousRadius`/`ThoughtDuration`/`DiagLevel` (renamed to field stems); the always-zipped dev package; the full 90-line 0.2.1 verification matrix (acceptance observed 2026-10-05, maintainer, dev package `c8c4b60` — including wall single-slot replacement and the four diagnostics rungs; future releases use the slim regression checklist in `TODO.md`).

**Release posture**: 0.1.x archived without release, 0.2.0 never tagged — the public axis starts at 0.2.1; tag/Release/Workshop remain separately gated per runbook.
