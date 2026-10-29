# Mwah! · Every Pawn Kisses Each Other

**English** | [中文](./README.zh-CN.md)

Mwah! lets anyone kiss anyone on the live map. Select a pawn, right-click another, and order a kiss — the one that can walk goes over, both turn to face each other, and vanilla lovemakin' hearts float up. Pawns with a mood get a mood bonus scaled by their own social impact; pawns without one (animals, mechanoids, drones, entities) still participate and can receive fate messages, without mood rewards. It is a joke mod with serious boundaries: **live maps only, single-player only, zero Harmony**.

This README describes the current feature set and is not a release announcement; the single manually maintained version is `<Version>` in `Source/Mwah/Mwah.csproj` (`About/About.xml <modVersion>` follows it).

## Requirements

- RimWorld **1.6 / 1.7** (Core only — no DLC or mod dependencies; DLC pawns like mechanoids, xenotypes and anomalies participate per their own nature).
- No Harmony, no framework mods, nothing to load before this mod.

Install a published package into `RimWorld/Mods/`.

## What you can do

- **Right-click a kiss.** Select any orderable pawn, right-click another pawn. A conscious but immobile receiver stays put while the mobile pawn comes over. The director also falls back when its first pick cannot move. If vanilla disallows orders from a downed selection, select the mobile pawn and right-click the receiver, or use the director.
- **Kiss Director panel.** A bottom-bar button opens a non-modal panel: two avatar slots pick any two pawns on the map — including ones you cannot order — via the vanilla targeter, and the heart fills half by half as sides are chosen. The right slot also accepts kissable things such as walls and trees. Quick-chain mode dispatches in two map clicks.
- **Ambient kissing (off by default).** The game itself pairs up pawns you cannot control (visitors, traders, raiders, wild animals, mechanoids) on an interval and radius you set. It never selects player-managed pawns as initiators, and drafted player-controlled pawns are excluded as receivers.
- **Kiss a wall.** Right-click any wall, natural rock or buried vein and lean in. The outcome is rolled against an editable XML fate table (five tiers, 50 narration rows) and lands as exactly one "relationship with walls" memory — the next kiss replaces it in place.
- **Kiss a tree.** Right-click a tree or select it in the director, including loaded mod trees marked as trees. Enabled by default, with a 30000-tick memory. Optional Ideology outcomes follow the acting pawn’s beliefs, favoring positive results and recognizing their own connected Gauranlen tree. Without the DLC, a matching belief, or with culture switched off, basic tree kissing still works. All tree outcomes renew one slot, separate from walls, without altering native pruning, connections or rituals.
- **Anyone is kissable, mood or not.** Pawns with a mood need gain +5 × their own effective social impact × multiplier (charming colonists profit more; psychopaths feel nothing, per vanilla nullify lists). Animals, mechanoids, drones and Anomaly entities (shambler/ghoul/awoken corpse) have no mood system: they get the animation, the hearts and a fate-table message from their own channel — and nothing else. They can also be the initiator via the director.
- **A truce window.** A commanded kiss pacifies both participants for the whole job — approach, performance and walk-home — because vanilla job ownership suspends the think tree. Any violent hit breaks it instantly, and the pair cooldown plus per-pawn cooldown rate-limit the trick. The system never pulls a fighting unit into a kiss; you may. The kiss itself ends with the performance: departure reports a return, releases kiss targets and no longer counts as occupied. New requests still check cooldowns and may replace the return when otherwise eligible.

## Settings

Settings open on **Gameplay & addons**: manage wall/tree extensions first, then the pairing scope, director, ambient kissing and other common switches. Choose **Configure** beside an addon for its own memory duration; tree culture controls sit inside the tree addon. Without Ideology, its availability is shown and basic trees remain configurable. Disabling the master switch or an addon retains its preferences.

**Parameters & misc.** holds timing, cooldowns, paired-mood tuning, ambient parameters, diagnostics and **Restore all defaults**. Identity, master switch and navigation stay pinned; pages scroll independently. Durations show ticks / real seconds / game hours. Numeric edits commit on Enter, focus loss, page changes, collapsing or closing; invalid text retains the previous value. Pairing scope applies only to Pawn pairs, not addons.

**Compress excessive social impact** sits beside mood strength and is on by default, including when older settings lack the new key. Impact up to 137% stays unchanged; excess gains diminish toward 150%, so one base reward approaches +7.5 at the default 1x multiplier. Off restores the original formula. It also applies to enabled opinion effects, not walls or trees. Existing memories keep their values until the same partner is kissed again; that settlement replaces the old pair slot with current strength, duration and narration.

**0.3.0** supports RimWorld 1.6 and 1.7; the maintainer has accepted both versions. See the [release record](./docs/release_review/release-0.3.0-review-zh.md) for channel status. Local test packages include 1.6 and 1.7 (4990 rev1069); the game selects only its matching directory. Use the whole `dist/dev/Mwah` produced by packaging, not the source checkout or a mixture of DLLs. Installation and in-game acceptance remain the maintainer’s actions; see the [acceptance notes](./docs/development/tree-kissing-and-settings.md#实机验收与反馈).

## Development, packaging, and versioning

The only manually maintained product version is `<Version>` in `Source/Mwah/Mwah.csproj`. Builds never install into RimWorld.

Read the [current specifications and change workflow](./docs/specification.md) before development. Specify real rule changes before code; a pure restoration can have zero delta. Implementation progress and evidence stay outside specification files.

```powershell
pwsh scripts/pack-dev.ps1                           # dev: Release build + stage dist/dev/Mwah (folder; -Zip optional)
pwsh scripts/pack-github.ps1 -Version v0.3.0        # GitHub flavor -> dist/github/Mwah-vX.Y.Z.zip
pwsh scripts/pack-steam.ps1                         # Steam flavor -> dist/steam/Mwah (clean tree required)
pwsh scripts/verify-local.ps1                       # all offline gates: 2 game targets × 3 flavors, keyed parity,
                                                    # fate-table completeness, DLL symbol audit, zero-Harmony proof
pwsh scripts/privacy-audit.ps1 -PackageDir dist/dev/Mwah # inspect the actual package; the hook audits history on push
```

CI (`.github/workflows/ci.yml`) runs the same gates and full-history privacy audit on every push and PR to any version branch (`[0-9].[0-9].x`) or `main`; push artifacts also pass a staged-payload audit before upload. Commit discipline: Conventional Commits (see `.gitmessage`), work on the current version branch, squash-merge into `main` which is release-only, history is append-only — no amend, no rebase. The full flow lives in [`docs/release-runbook-zh.md`](./docs/release-runbook-zh.md); the release copy for the Workshop page is maintained in [`docs/steam-workshop-page.md`](./docs/steam-workshop-page.md) and version history in [`docs/CHANGELOG.md`](./docs/CHANGELOG.md) / [`docs/CHANGELOG.zh-CN.md`](./docs/CHANGELOG.zh-CN.md).

## Hard boundaries

Zero Harmony (no runtime patches, ever, unless the maintainer rewrites this line). No world map, no caravans, no multiplayer sync. No junctions or symlinks into a game install. Player-facing text ships in both languages or it does not ship. No absolute local paths, credentials or Workshop IDs anywhere in the repo or its reachable history.

## License

Source code is under the [Mozilla Public License 2.0](./LICENSE).
