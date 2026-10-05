# Changelog

## Changelog Template

This file is the canonical English changelog for Mwah! (Every Pawn Kisses Each Other).

Rules:
- Keep released entries in chronological order, oldest first and newest last; place an Unreleased entry at the top.
- Use local release time in the heading: `[YYYY-MM-DD HH:MM UTC+8] Version X.Y.Z`.
- An unreleased top entry may use `Unreleased — X.Y.Z`; replace it with the actual UTC+8 release time when publishing.
- Use `Initial Workshop Upload` only for the first Steam Workshop upload; subsequent updates use ordinary version entries.
- Keep items short and visible. Prefer one change per bullet.
- Write precisely and describe behavior; jokes belong to the in-game copy, not to the changelog.
- Separate feature additions, changes, fixes, packaging notes, and release notes when useful.
- Bug fixes should be concise unless the fix changes player-facing behavior.
- Mention both GitHub and Steam only when the entry affects both release surfaces.
- Do not put unaccepted plans here. An accepted, not-yet-released version may use the explicit Unreleased entry above.
- Keep the Simplified Chinese version synchronized in `docs/CHANGELOG.zh-CN.md`.

Recommended entry shape:

```text
## [YYYY-MM-DD HH:MM UTC+8] Version X.Y.Z

One-line release summary.

### Added
- ...

### Changed
- ...

### Fixed
- ...

### Packaging
- ...
```

## Unreleased — 0.2.2

First public release: any pawn kisses any pawn on the live map — single-player only, zero Harmony, Core only.

### Added
- Added right-click kissing: select a pawn, right-click another; the one that can move walks over, the kiss happens, hearts float above both, and each returns to their original spot.
- Added the Kiss Director: a bottom-bar panel that orders any two pawns on the map to kiss, including pawns you cannot order, with a two-click quick-dispatch mode.
- Added wall kissing: built walls, natural rock and buried ore veins are valid targets; the outcome is rolled on an editable XML fate table (five mood tiers, fifty narration lines) and stored as a single "relationship with walls" memory that the next kiss replaces.
- Added animals, mechanoids and anomaly entities as kissable participants: they have no mood system, gain no mood, and receive one fate-table message from their own channel instead.
- Added a truce window: pawns ordered to kiss do not attack each other until it ends; any effective damage interrupts the kiss immediately.
- Added a mood bonus scaled by the receiver's own social impact; traits on the vanilla nullify list (e.g. psychopath) cancel it entirely.
- Added the "who kisses whom" gate slider: seven cumulative tiers, factory default fully open (no faction or species limit).
- Added ambient kissing (off by default): matchmakes pawns the player cannot order, on a configurable interval and radius; never orders player-controlled pawns.
- Added the four-section settings page (core / autonomous matchmaking / add-ons / system); every duration displays ticks, real seconds and game hours together.
- Added four diagnostic levels (off / auto / simple / verbose); auto follows the build channel, while the startup banner and error lines are never gated.

### Packaging
- Packages carry a version.txt identity label (version, build flavor, commit).
- GitHub release zips are deterministic (single top-level folder, commit-stamped entries).
- Bilingual key sets are verified by the offline gate suite, with a full-history privacy audit before any push.

### Notes
- The 0.1.x development line was archived without ever shipping, and 0.2.0 / 0.2.1 were never tagged; everything above ships together in 0.2.2, the first public release.
