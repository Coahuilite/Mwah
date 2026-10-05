# Changelog

All notable changes to Mwah! (Every Pawn Kisses Each Other) are documented in this file. The format follows [Keep a Changelog 1.1.0](https://keepachangelog.com/en/1.1.0/), and the project adheres to [Semantic Versioning](https://semver.org/).

## Changelog rules

Structure and categories:
- Released entries in chronological order, oldest first; the Unreleased entry sits on top.
- Heading uses local release time: `[YYYY-MM-DD HH:MM UTC+8] Version X.Y.Z`; an unreleased entry uses `Unreleased — X.Y.Z` and gets the actual time at publish.
- Sections use the Keep a Changelog types: **Added**, **Changed**, **Fixed**, **Deprecated**, **Removed**, **Security**; this project additionally allows **Packaging** and **Notes**. Never keep empty sections.
- Breaking changes (save compatibility, settings migration, incompatible behavior shifts) always come first, with the impact and the action the player must take spelled out.
- `Initial Workshop Upload` is reserved for the first Steam Workshop upload; later updates use ordinary version entries.

Writing entries:
- One entry = one notable change, phrased as what the user can now do or what problem is gone — never as a mechanism walkthrough. Operational detail belongs to the README; an entry may carry at most a one-line pointer.
- Start with a strong verb, present tense, roughly one line. Two short sentences beat one long manual.
- Be specific; "various fixes and improvements" is banned. Every version gets entries, but only for changes that actually happened.
- Write precisely and describe behavior; jokes belong to the in-game copy, not to the changelog.
- Do not record unaccepted plans; accepted, unreleased work lives in the top Unreleased entry.
- Mention GitHub and Steam together only when an entry affects both release surfaces.

Maintenance:
- Keep the Simplified Chinese version synchronized in `docs/CHANGELOG.zh-CN.md`, entry by entry.

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

First public release: any pawn kisses any pawn. Mechanics and details live in the README.

### Added
- Right-clicking now makes one pawn kiss another, including pawns you cannot order.
- Kiss Director: a bottom-bar panel that orders any two pawns on the map to kiss, with a two-click quick-dispatch mode.
- Walls are kissable: built walls, natural rock, and buried ore veins; the mood outcome is rolled on an editable fate table.
- Animals, mechanoids and anomaly entities can be kissed and can initiate; they gain no mood, but each receives a fate-table message from its own channel.
- A commanded pair will not attack each other while the kiss lasts; effective damage interrupts it immediately.
- The mood bonus scales with the receiver's own social impact; traits on the vanilla nullify list cancel it entirely.
- A seven-tier "who kisses whom" gate slider, unrestricted by default.
- Ambient kissing: the game matchmakes pawns you cannot order, on your interval and radius; player-controlled pawns are never ordered for you.
- Four-section settings page (core / autonomous / add-ons / system); every duration shows ticks, real seconds and game hours together.
- Four diagnostic levels, defaulting to the build channel; the startup banner and error lines are never gated.

### Packaging
- Packages carry a version.txt identity label (version, build flavor, commit).
- GitHub release zips are deterministic (single top-level folder, commit-stamped entries).
- Bilingual key sets are verified by the offline gate suite, with a full-history privacy audit before any push.

### Notes
- The 0.1.x line was archived without shipping, and 0.2.0 / 0.2.1 were never tagged; everything above ships together in 0.2.2, the first public release.
