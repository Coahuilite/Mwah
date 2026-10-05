# Changelog

## Changelog Template

This file is the canonical English changelog for Mwah! (Every Pawn Kisses Each Other).

Rules:
- Keep released entries in chronological order, oldest first and newest last; place an Unreleased entry at the top.
- Use local release time in the heading: `[YYYY-MM-DD HH:MM UTC+8] Version X.Y.Z`.
- An unreleased top entry may use `Unreleased — X.Y.Z`; replace it with the actual UTC+8 release time when publishing.
- Use `Initial Workshop Upload` only for the first Steam Workshop upload; subsequent updates use ordinary version entries.
- Keep items short and visible. Prefer one change per bullet.
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
- Added right-click kissing: select a pawn, right-click another; the one that can walk walks over, they kiss, vanilla lovemakin' hearts float up, and both head back.
- Added the Kiss Director: a bottom-bar panel that dispatches any two units on the map, including ones you cannot command.
- Added wall kissing: right-click a built wall, natural rock or a buried vein; the outcome rolls on an editable XML fate table (five tiers, fifty narration lines) and lands as a single "relationship with walls" memory.
- Added animals, mechanoids and anomaly entities as kissable participants: no mood for them, but each gets its own fate-table commentary line.
- Added a truce window: a commanded pair will not fight until the kiss ends; one real hit breaks it.
- Added a mood bonus scaled by the receiver's own social impact; psychopaths feel nothing (vanilla nullify lists).
- Added the seven-tier "who kisses whom" gate slider, fully open by default.
- Added ambient kissing (off by default): the game matchmakes pawns you cannot order around and never touches your colonists.
- Added the four-section settings page; every duration is shown in ticks, real seconds and game hours.
- Added four diagnostic levels: quiet during normal play, readable when something looks off.

### Packaging
- Packages carry a version.txt identity label (version, build flavor, commit).
- GitHub release zips are deterministic (single top-level folder, commit-stamped entries).
- Bilingual key sets are verified by the offline gate suite, with a full-history privacy audit before any push.

### Notes
- The 0.1.x development line was archived without ever shipping, and 0.2.0 / 0.2.1 were never tagged; everything above ships together in 0.2.2, the first public release.
