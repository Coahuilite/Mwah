# Changelog

Notable changes to Mwah! (Every Pawn Kisses Each Other), newest first. Dates on unreleased entries are update dates, not publication dates. Released entries retain their actual UTC+8 release time.

## [2026-11-17 00:57 UTC+8] Version 0.4.0

### Added
- Kiss worktables and vanilla research devices, the multi-analyzer and nutrient paste dispenser, sharing one machine memory.
- Gain stronger positive research-device moods with Intellectual, without advancing research.
- French kiss the dispenser or supported Vanilla Nutrient Paste Expanded outlets for good fate outcomes that fill Food without using feedstock.

### Changed
- Consolidate installation, architecture, diagnostics and release guidance into self-contained documents; retire cold-memory and completed change records without leaving archive dependencies.

### Fixed
- Stop the current director pick when closing the panel, including quick dispatch.

### Packaging
- Load shared Defs and languages once from the mod root while retaining separate native builds for 1.6 and 1.7; replace old installations as a whole folder.
- Load VNPE registrations and dedicated text only when VNPE is active, and keep local art originals and documentation archives out of runtime packages.

## [2026-10-30 02:16 UTC+8] Version 0.3.0

### Added
- Kiss trees with basic outcomes even without Ideology, with optional results based on each pawn’s beliefs and own Gauranlen connection.
- Build a native .NET 10 package for RimWorld 1.7.4990 rev1069 alongside the retained 1.6 package.

### Changed
- Compress paired-kiss social impact above 137% with bounded diminishing returns toward 150%; enabled by default, switchable back to the original formula. Enabled opinion effects share the compression; walls and trees do not.
- Refresh the same partner’s old paired memory using current strength, duration and narration instead of renewing an outdated multiplier; replace prior plain/social variants in that partner slot.
- Manage extensions first in the new two-page settings view; pending numeric edits are committed before hiding or closing controls.

### Fixed
- Keep crawl-capable downed receivers in passive kissing instead of interrupting them because of the kiss reservation; cancel unfinished pairs when either pawn loses participation eligibility.
- Keep an immobile receiver at its own cell even when paired-stage selection falls back to Touch; allow manual director mobility fallback and report the actual walking pawn.
- End kiss occupancy and reports at performance completion, release both jobs’ kiss reservations, and let eligible new orders replace departure without duplicate rewards or return hijacks.
- Wait for both matching participants before advancing paired performance and report actual job admission.
- Keep normal departure inside the kiss job without stealing a newer order; persist activity identity and settlement state.
- Apply cooldown exemption consistently while preserving reachability and cross-target occupancy.

### Packaging
- Isolate the 1.6 and 1.7 assemblies, generate both minors from one content source, and reject incomplete or mixed packages.
- Remove native-target compiler warnings without suppressions; retain the default 1.6 build entry.

### Notes
- The maintainer accepts 0.3.0 after confirming pairing in 1.6 and 1.7. Native support targets 1.7.4990 rev1069 alongside 1.6.

## [2026-10-06 00:00 UTC+8] Version 0.2.2

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
- This version is also the initial Steam Workshop upload (published 2026-10-05, same steam-flavor build). The GitHub Release went out at 2026-10-06 00:00 UTC+8.
