# Mwah! · Every Pawn Kisses Each Other

**English** | [中文](./README.zh-CN.md)

Mwah! lets anyone kiss anyone on the live map. Select a pawn, right-click another, and order a kiss — the one that can walk goes over, both turn to face each other, and vanilla lovemakin' hearts float up. Pawns with a mood get a mood bonus scaled by their own social impact; pawns without one (animals, mechanoids, drones, entities) get nothing at all. It is a joke mod with serious boundaries: **live maps only, single-player only, zero Harmony**.

This README describes the current feature set and is not a release announcement; the single manually maintained version is `<Version>` in `Source/Mwah/Mwah.csproj` (`About/About.xml <modVersion>` follows it).

## Requirements

- RimWorld **1.6** (Core only — no DLC or mod dependencies; DLC pawns like mechanoids, xenotypes and anomalies participate per their own nature).
- No Harmony, no framework mods, nothing to load before this mod.

Install a published package into `RimWorld/Mods/`. Copying the mod folder and running the game is the maintainer's own workflow; no symlinks or junctions are assumed anywhere.

## What you can do

- **Right-click a kiss.** Select any orderable pawn, right-click another pawn. If the selected one cannot move, the other comes over instead.
- **Kiss Director panel.** A bottom-bar button opens a non-modal panel: two avatar slots pick any two pawns on the map — including ones you cannot order — via the vanilla targeter, and a heart fills as both sides are chosen. Quick-chain mode dispatches in two clicks and chains.
- **Ambient kissing (off by default).** The game itself pairs up pawns you cannot control (visitors, traders, raiders, wild animals, mechanoids) on an interval and radius you set. It never touches your colonists.
- **Kiss a wall.** Right-click any wall, natural rock or buried vein and lean in. The outcome is rolled against an editable fate table (`MWAH_FateDef`, shipped with 50 wall rows across five tiers) and lands as a thought with its own narration line. Addons are one class each; the wall is the first.
- **A truce window.** A commanded kiss pacifies both participants for the whole job — approach, performance and walk-home — because vanilla job ownership suspends the think tree. Any violent hit breaks it instantly, and the pair cooldown plus per-pawn cooldown rate-limit the trick. The system never pulls a fighting unit into a kiss; you may.

## Settings

Seventeen settings, live-map only, in English and Simplified Chinese with identical key sets. The gate slider ("who kisses whom", seven tiers, factory default = everything) never hides the wall addon — that has its own switch, and switching it off makes walls vanish from every pick surface without a single nag. All durations are stored in ticks and shown as tick / real seconds / game hours at once. Every slider has an exact number field. Diagnostics (dev log lines in `Player.log` plus a 1 Hz phase sampler in the save-data folder) are a runtime toggle; the startup banner prints regardless.

## Development, packaging, and versioning

The only manually maintained product version is `<Version>` in `Source/Mwah/Mwah.csproj`. Builds never install into RimWorld.

```powershell
pwsh scripts/build-dev.ps1                          # dev: Release build + stage dist/dev/Mwah + zip
pwsh scripts/pack-github.ps1 -Version v0.1.0        # GitHub flavor -> dist/github/Mwah-vX.Y.Z.zip
pwsh scripts/pack-steam.ps1                         # Steam flavor -> dist/steam/Mwah (clean tree required)
pwsh scripts/verify-local.ps1                       # 25 offline gates: 3 flavor builds, keyed parity,
                                                    # fate-table completeness, DLL symbol audit, zero-Harmony proof
pwsh scripts/privacy-audit.ps1 -FullHistory -PrePush # the pre-push ritual
```

CI (`.github/workflows/ci.yml`) runs the same gates plus the privacy default scan on every push and PR. Commit discipline: Conventional Commits (see `.gitmessage`), work on the version branch (`0.1.x`), squash-merge into `main` which is release-only, history is append-only — no amend, no rebase. The full flow lives in [`docs/release-runbook-zh.md`](./docs/release-runbook-zh.md).

## Hard boundaries

Zero Harmony (no runtime patches, ever, unless the maintainer rewrites this line). No world map, no caravans, no multiplayer sync. No junctions or symlinks into a game install. Player-facing text ships in both languages or it does not ship. No absolute local paths, credentials or Workshop IDs anywhere in the repo or its reachable history.

## License

Source code is under the [Mozilla Public License 2.0](./LICENSE).
