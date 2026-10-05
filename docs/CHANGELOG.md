# Changelog

Canonical English changelog for Mwah! (Every Pawn Kisses Each Other).

Rules:
- Released entries in chronological order, oldest first, newest last; the Unreleased entry sits on top.
- Heading uses local release time: `[YYYY-MM-DD HH:MM UTC+8] Version X.Y.Z`; replace `Unreleased — X.Y.Z` at publish time.
- `Initial Workshop Upload` wording only for the first Steam Workshop upload.
- One change per bullet; player-facing wording; bug fixes stay brief unless behavior visible to players changed.
- Keep the Simplified Chinese version synchronized in `docs/CHANGELOG.zh-CN.md`.

## Unreleased — Version 0.2.2

First public release. The 0.1.x development line was archived without ever shipping (2026-10-05 maintainer ruling), so everything below ships together. Mwah! is a joke mod with serious boundaries: live maps only, single-player only, zero Harmony, no dependencies beyond Core.

### Added
- **Right-click a kiss.** Select any orderable pawn, right-click another pawn: the one that can walk goes over, both turn to face each other side by side, vanilla lovemakin' hearts float up, then both walk home. If the selected pawn cannot move, the other one comes over instead.
- **Kiss Director panel.** A bottom-bar button opens a non-modal panel — the map keeps panning, zooming and selecting while it is open. Two avatar slots pick any two pawns on the map (including ones you cannot command) via the vanilla targeter; the heart fills half by half as sides are chosen. Quick-chain mode dispatches in two map clicks.
- **Kiss a wall.** Right-click a built wall, natural rock or buried vein and lean in. The outcome is rolled on an editable, XML-shipped fate table (five tiers, −2 to +6 mood, 50 narration rows) and lands as a single "relationship with walls" memory that the next kiss replaces in place. Add-ons are a registry: future targets (trees, benches, machines) each bring their own switch, fate table and duration.
- **Entity, animal and mechanoid kissing.** Moodless participants get the animation, hearts and a fate-table message of their own channel (mech cold jokes, animal sniff narratives, entity empty-shell psychology) — and no mood, because they have no mood need. Shambler/ghoul/awoken-corpse entities are kissable both ways and admitted by the same gate slider as any humanlike pawn.
- **A truce window.** A commanded kiss pacifies both participants for the whole job — approach, performance and walk-home — because vanilla job ownership suspends the think tree. Any violent hit breaks it instantly; pair and per-pawn cooldowns rate-limit the trick. The ambient system never pulls a fighting unit into a kiss; you may.
- **Ambient kissing (off by default).** The game pairs up pawns you cannot command (visitors, traders, raiders, wild animals, mechanoids) on an interval and radius you set. It never touches your colonists.
- **Mood that scales with the pawn who receives it.** +5 base × the receiver's own `SocialImpact` × a settings multiplier: a charming colonist gets more out of a kiss than a mute one. Psychopaths feel nothing from it (vanilla nullify lists). An optional setting additionally writes opinions ("our kiss") without touching the vanilla romance chain by default.
- **Gate slider.** "Who kisses whom" in seven cumulative tiers, from free colonists only to everything (factory default). Greyed menu items always state the real reason; nothing is ever silently hidden except disabled add-ons, which vanish without a nag.
- **Settings page, four sections.** Core / Autonomous matchmaking / Add-ons / System, with a pinned identity header (subtitle + build watermark visible at any scroll depth). Every duration slider has an exact tick number field; all durations show tick / real seconds / game hours at once.
- **Four-rung diagnostics.** Off / Auto / Simple / Verbose. Auto follows the build channel (dev builds verbose, release builds simple: one fate line per settled kiss plus every abnormal exit). The startup banner and error lines always print.

### Packaging
- Dev rehearsal is a folder dropped into `Mods/` (no zip by default); GitHub release zips are deterministic (single top-level folder, commit-stamped entries).
- Bilingual (English / Simplified Chinese) key sets verified by the full offline gate suite, including fate-table completeness, zero-Harmony proof and a full-history privacy audit before any push.

## Notes
- 0.1.0 was never tagged, never uploaded, never shipped; 0.2.0 and 0.2.1 likewise never tagged — the version axis advanced during pre-release copy work, and the first public release is 0.2.2.
