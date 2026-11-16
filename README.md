# Mwah! · Every Pawn Kisses Each Other

**English** | [简体中文](README.zh-CN.md)

Select a pawn, right-click another, and order a kiss. The mobile pawn approaches, both face each other, and vanilla hearts float up. Use the **Kiss Director** to pair pawns you cannot normally order, or send a pawn to kiss a wall, tree or machine.

Supports **RimWorld 1.6 and 1.7**, live maps and single-player. Core alone is enough: no required DLC, framework or Harmony. This page includes **unreleased 0.4.0** machine and VNPE features; published changes are listed in the [changelog](docs/CHANGELOG.md).

## Install and update

Install the complete packaged `Mwah` folder into `RimWorld/Mods/`, then enable one copy of `coahuilite.mwah`. Do not install the source checkout or mix DLLs from different builds.

**Replace the old mod folder completely when updating**, rather than copying over it. Keep your saves and settings. The package contains shared root content, a native DLL for each game minor, and optional compatibility content selected by `LoadFolders.xml`.

## Choose a kiss

- **Pawn pairs:** right-click an orderable pawn's partner, or select both sides in the director. A conscious immobile receiver stays in place while the mobile pawn approaches. The director can select non-player-controlled pawns; its quick-dispatch option uses two map clicks.
- **Walls:** walls, natural rock and mineral veins have their own fate table and one shared wall-memory slot.
- **Trees:** trees, including modded plants marked as trees, share a separate memory slot. Optional Ideology outcomes use the acting pawn's beliefs and own Gauranlen connection. Basic tree kisses work without the DLC and do not change pruning, connections or rituals.
- **Machines:** worktable subclasses, both vanilla research benches, the multi-analyzer and the nutrient paste dispenser share one machine-memory slot. A target with a power component must report `PowerOn`, whatever its power source; non-electric benches do not need electricity.
- **Research devices:** positive mood scales by `1 + Intellectual/20`, capped at skill level 20. Research progress is unchanged.
- **French-kissing paste machines:** a good fate outcome fills an existing Food need without feedstock, food consumption or ingestion side effects. The vanilla dispenser and supported VNPE outlets use the same special kiss.

Worktables and research benches use their work/chair interaction cell; no chair is required. The dispenser prefers its dispensing interaction cell, with one cell to either side allowed. The multi-analyzer only needs a legal touching position. An unreachable required position prevents the kiss.

### Optional Vanilla Nutrient Paste Expanded support

With VNPE enabled, its **tap, feeder and dripper** accept French kisses. The tap uses its interaction cell or either adjacent side cell; the feeder and dripper use a reachable, standable, reservable position satisfying vanilla Touch. They require power, not network stock or a linked bed, and consume no resources.

Their registrations and dedicated text load only with VNPE. Other features work without it; this compatibility does not alter VNPE's production or feeding rules.

## Rewards and cancellation

Paired-kiss mood rewards scale by each participant's **own** effective SocialImpact and the settings multiplier. Moodless pawns still participate and use their applicable narration/message channel without receiving mood rewards. Repeating a paired kiss refreshes that partner's memory; wall, tree and machine kisses each replace their own single slot.

Unfinished or interrupted kisses grant no completion reward. Damage can interrupt the job. After the performance, the kiss is complete: return travel releases kiss occupancy and does not settle again. A new eligible order may replace the return, while cooldowns still apply.

## Settings

**Gameplay & addons** contains extension switches, per-extension memory duration, tree culture options, pairing scope, director and ambient kissing. **Parameters & misc.** contains timing, cooldowns, paired-mood tuning, diagnostics and restore defaults. Turning a feature off preserves its preferences.

Durations display ticks, real seconds and game hours together and are stored in ticks. Numeric edits commit on Enter, focus loss, page change or close. Pairing scope affects Pawn pairs, not wall/tree/machine targets.

Ambient kissing is **off by default**. It only pairs pawns: it does not use player-managed initiators or drafted player-controlled receivers. Excessive SocialImpact compression is on by default, leaves values through 137% unchanged and diminishes toward 150%; it affects paired mood and enabled opinion effects, not addon rewards.

## Build and contribute

A dual-version build needs the .NET 10 SDK and available NuGet restore/cache. Build output stays outside the game installation:

```powershell
pwsh scripts/verify-local.ps1
pwsh scripts/pack-dev.ps1 -Zip
```

The packaged folder is `dist/dev/Mwah`. Read [CONTRIBUTING.md](CONTRIBUTING.md) for the source layout, compatibility constraints and allowed checks. [Documentation](docs/README.md) covers architecture, diagnostics, specifications and release operations. In-game testing belongs to the maintainer.

## License

[Mozilla Public License 2.0](LICENSE).
