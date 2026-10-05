# Mwah! · Every Pawn Kisses Each Other

**English** | [中文](./README.zh-CN.md)

Mwah! lets anyone kiss anyone on the live map. Select a pawn, right-click another, and order a kiss — the one that can walk goes over, both turn to face each other, and vanilla lovemakin' hearts float up. Pawns with a mood get a mood bonus scaled by their own social impact; pawns without one (animals, mechanoids, drones, entities) get nothing at all. It is a joke mod with serious boundaries: **live maps only, single-player only, zero Harmony**.

This README describes the current feature set and is not a release announcement; the single manually maintained version is `<Version>` in `Source/Mwah/Mwah.csproj` (`About/About.xml <modVersion>` follows it).

## Requirements

- RimWorld **1.6** (Core only — no DLC or mod dependencies; DLC pawns like mechanoids, xenotypes and anomalies participate per their own nature).
- No Harmony, no framework mods, nothing to load before this mod.

Install a published package into `RimWorld/Mods/`.

## What you can do

- **Right-click a kiss.** Select any orderable pawn, right-click another pawn. If the selected one cannot move, the other comes over instead.
- **Kiss Director panel.** A bottom-bar button opens a non-modal panel: two avatar slots pick any two pawns on the map — including ones you cannot order — via the vanilla targeter, and the heart fills half by half as sides are chosen. The right slot also accepts kissable things (a wall). Quick-chain mode dispatches in two map clicks.
- **Ambient kissing (off by default).** The game itself pairs up pawns you cannot control (visitors, traders, raiders, wild animals, mechanoids) on an interval and radius you set. It never touches your colonists.
- **Kiss a wall.** Right-click any wall, natural rock or buried vein and lean in. The outcome is rolled against an editable XML fate table (five tiers, 50 narration rows) and lands as exactly one "relationship with walls" memory — the next kiss replaces it in place.
- **Anyone is kissable, mood or not.** Pawns with a mood need gain +5 × their own `SocialImpact` × multiplier (charming colonists profit more; psychopaths feel nothing, per vanilla nullify lists). Animals, mechanoids, drones and Anomaly entities (shambler/ghoul/awoken corpse) have no mood system: they get the animation, the hearts and a fate-table message from their own channel — and nothing else. They can also be the initiator via the director.
- **A truce window.** A commanded kiss pacifies both participants for the whole job — approach, performance and walk-home — because vanilla job ownership suspends the think tree. Any violent hit breaks it instantly, and the pair cooldown plus per-pawn cooldown rate-limit the trick. The system never pulls a fighting unit into a kiss; you may.

## Settings

The settings page is organized in four sections — core, autonomous matchmaking, add-ons, system — under a pinned identity header (subtitle + build watermark, visible at any scroll depth). The gate slider ("who kisses whom", seven tiers, factory default = everything) never hides an add-on — each has its own switch in the add-ons block, and switching it off makes its targets vanish from every pick surface without a single nag. All durations are stored in ticks and shown as tick / real seconds / game hours at once; every quantity slider carries an exact number field (tier sliders deliberately do not). Diagnostics are a four-rung slider (Off / Auto / Simple / Verbose; Auto follows the build channel), and the startup banner plus error lines print regardless.

## Development, packaging, and versioning

The only manually maintained product version is `<Version>` in `Source/Mwah/Mwah.csproj`. Builds never install into RimWorld.

```powershell
pwsh scripts/pack-dev.ps1                           # dev: Release build + stage dist/dev/Mwah (folder; -Zip optional)
pwsh scripts/pack-github.ps1 -Version v0.2.1        # GitHub flavor -> dist/github/Mwah-vX.Y.Z.zip
pwsh scripts/pack-steam.ps1                         # Steam flavor -> dist/steam/Mwah (clean tree required)
pwsh scripts/verify-local.ps1                       # all offline gates: 3 flavor builds, keyed parity,
                                                    # fate-table completeness, DLL symbol audit, zero-Harmony proof
pwsh scripts/privacy-audit.ps1 -FullHistory -PrePush # the pre-push ritual
```

CI (`.github/workflows/ci.yml`) runs the same gates plus the privacy default scan on every push and PR to any version branch (`[0-9].[0-9].x`) or `main`. Commit discipline: Conventional Commits (see `.gitmessage`), work on the current version branch, squash-merge into `main` which is release-only, history is append-only — no amend, no rebase. The full flow lives in [`docs/release-runbook-zh.md`](./docs/release-runbook-zh.md); the release copy for the Workshop page is maintained in [`docs/steam-workshop-page.md`](./docs/steam-workshop-page.md) and version history in [`docs/CHANGELOG.md`](./docs/CHANGELOG.md) / [`docs/CHANGELOG.zh-CN.md`](./docs/CHANGELOG.zh-CN.md).

## Hard boundaries

Zero Harmony (no runtime patches, ever, unless the maintainer rewrites this line). No world map, no caravans, no multiplayer sync. No junctions or symlinks into a game install. Player-facing text ships in both languages or it does not ship. No absolute local paths, credentials or Workshop IDs anywhere in the repo or its reachable history.

## License

Source code is under the [Mozilla Public License 2.0](./LICENSE).
