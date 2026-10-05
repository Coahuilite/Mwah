# MEMORY

## Current durable state

- RimWorld 1.6 mod, `packageId` `coahuilite.mwah`, product version **0.2.2** (the csproj `<Version>` is the repository's only version literal — the old `<VersionPrefix>` duplicate was removed 2026-10-05; every product-visible follower — About `<modVersion>`, the workshop page's target/version lines, both changelog top entries — is machine-locked to it by gate 7b). **0.1.x archived without release** (2026-10-05 maintainer ruling), development runs on `0.2.x`; the repository is on the cloud: `github.com/Coahuilite/Mwah` (public, MPL 2.0, default branch `main` = release-only; process: `docs/release-runbook-zh.md`). **Released (2026-10-05/06)**: 0.2.2 shipped to both channels — initial Steam Workshop upload by the maintainer (steam flavor, 2026-10-05) and GitHub Release `v0.2.2` (main squash commit, Release CI green, asset `Mwah-v0.2.2.zip`, 2026-10-06 00:00 UTC+8); neither 0.2.0 nor 0.2.1 was ever tagged. The live matrix was accepted on the pre-release draft (record in OBLIVIONIS; slim regression checklist in TODO). Post-release maintenance mode; channel observation supports page-level conclusions only.
- Feature surface: see `README.md` / `README.zh-CN.md` "What you can do" — three initiation paths (right-click, Kiss Director panel, ambient matchmaking off by default), an addon layer for non-pawn targets (wall today), fate tables driving all settlement copy, mood only for pawns with `needs.mood`.
- Design stance: **pure entertainment, gate as low as possible** — no faction, species, or draft restriction. This is the maintainer's position, explicitly reaffirmed 2026-09-02: player requests to "only matchmake the colony" are absorbed by the **gate slider** (factory far-right = unrestricted), **never by changing the default**. But "zero gate" was never true and must not be faked: vanilla `CanTakeOrder` and `ShouldGenerateFloatMenuForPawn` gate-keep before any provider — animals and neutrals cannot initiate, downed pawns get no menu at all. After the 2026-09-02 boundary consolidation, this mod's own preconditions are written as `KissBoundary`'s layers, never approximating "capability" with "species". The reward side stays conservative (see engineering decisions). The workspace folder name `every_pawn_kiss_each_other/` is a historical value and carries no identity.
- Explicitly never (not a to-do): world map / caravan tiles, RimWorld Multiplayer sync, true body-contact kiss animation, downed pawns as initiators (would need Harmony to open vanilla gates).
- **Never junction**: nothing is created, verified, or assumed under `Mods/`; copying the mod and live testing are the maintainer's own actions (2026-09-02 maintainer order).

## Naming decisions

- Identity lives in `AGENTS.md` (brand, packageId, prefixes, the Pawn noun rule, version source). History kept here: the gate's far-right label decided on 2026-09-02 as the brand name is now ordinary copy (`所有 Pawn 都互相亲吻`); the workspace folder name is a historical value (see design stance).
- Candidate elimination rationale and the vanilla-token occupancy audit (`KISS_/PECK_/CHUU_/XOXO_/SMOOCH_` death causes, six candidates all unoccupied) are archived in `OBLIVIONIS.md` (2026-10-04 section); the settled answer stands: MWAH is onomatopoeia, never an initialism.
- **Copy ruling (2026-09-22 maintainer order, supersedes the 2026-09-02 prompt decree)**: "啵嘴" may appear only in the Chinese display name `所有Pawn都给我啵嘴！` (the single `MWAH.SettingsCategory`); **all in-game copy uses the 亲吻/吻 register** — bottom bar `亲吻导演台`, picking prompts `谁要发起亲吻？` / `{PAWN} 想要和谁亲吻？`, gate greyed item `…不想和 …亲吻`, thought entry `被{0}亲吻过` / opinion `我们的那一吻` / description `有人吻了我，我也吻了回去。`, report string `亲吻TargetA。`, settings `自主亲吻` / `亲吻冲动间隔` / `无视亲吻冷却` / `亲吻导演台按钮`. The English side is unchanged (always kiss wording). Matrix rows and this file that once quoted the old prompts are synced; the "维护者要求『所有 pawn 都啵嘴』" quote elsewhere in this file is a verbatim requirement quote and stays.

## Vanilla capability boundaries

- Verified engine facts moved 2026-10-06 to **`docs/vanilla-capability-boundaries.md`** — three sections keep their entry names: "Vanilla capability boundaries" (float-menu self-registration, Targeter contract, facing, GameComponent ctor, localization flat-key facts, bottom-bar ceilings, logging ceilings, unit constants), "Mood ladder, nullify lists, SocialImpact anatomy, and the player-HUD surface", "Panel and resolution" (UI scaling, non-modal trio, text measurement), and "Who can kiss: the three-layer boundary" (the admission table). Read before changing implementation; the rulings below still cite those entry names.

## Engineering decisions

Full design records live in **`docs/design-decisions.md`** (sections: initiation & ambient / director & picking / performance / addons / mood architecture / logging / settings & durations / scaling rules / process / packaging). Read the section before touching that subsystem. This file keeps only what still constrains edits, one line each:

- `AGENTS.md` is injected every session and costs context: protocol, identity, hard boundaries, pointers — nothing volatile. Structure sourced from UniversalSqueaker's skeleton (2026-10-04).
- Three initiation paths are independent and parallel, meeting only at `KissUtility.Propose`; ambient never touches player-commandable pawns; own animals can only be kissed, never initiate; role swap belongs to the right-click path only.
- **Never start a job synchronously inside a toil's finish action** — queue via `KissReturnQueue`, drained next tick; every finish action carries an idempotency gate (live-proven 2026-09-04 crash).
- **Gate-exemption switch** `noCooldowns` (default off) skips both cooldowns, never "already kissing" — the mirror job structurally admits no third party; the player is super-intelligence's hand.
- **Truce window = job lifetime** (approach + performance + return inside both mirror jobs, zero extra primitives); the combat gate binds ambient only; kiss clock belongs to the initiator alone; fixed stage = left-right cells, each job reserves its own cell + the partner.
- Non-pawn targets are the addon family: extend `KissThingAddon` + one registry line = every path wired; ambient never dispatches thing-kisses; silent invalidation is constitutional.
- Mood = relationship-keyed slots, no cross-group cap; wall = one def + 5 stages replaced in place; per-addon storage keyed by stable addon id; pair settlement draws **without replacement** from the shared fate table (one-row tables still speak); `changeOpinion=false` default keeps the vanilla romance chain untouched.
- Gate = seven-tier `KissScope` slider, factory rightmost, predicates all vanilla; cooldowns are in-session state, reset-on-load accepted; durations are ticks-only everywhere, no reverse conversions, no compat layers before a release.
- Logging: Player.log only, banner unconditional, four-rung slider with Auto=channel; KissTrace is a crash bypass, not a logging subsystem.
- **No custom trait/xenotype kiss-value table, ever**; the phase-two differentiation axis is relationships (`pawn.relations` + `ThoughtDef.stages`).
- **Attribution discipline (cross-project)**: trust the engine's authoritative output; never default-explain with "user installed an old package"; instrument lines must carry unique fields (`t=`/counters) or Unity's fold-at-99 hides the re-entry.
- Remote discipline: pre-push audit runs **standalone with the exit code checked — never piped** (a pipe masked a dirty-tree failure once, 2026-10-06); debts ledger lives in the script header; runbook is the process authority.
- Skeleton/packaging patterns abstracted into `modding_documents/RimWorld_Mod_RepoInit_AgentMemory_And_Packaging_Guide_zh.md`; sibling projects read it first; `modding_documents/` is not in git.

## Repository structure and navigation

Runtime content lives entirely under the version folder `1.6/` (`Defs/Kiss/`, `Languages/{English,ChineseSimplified}/`, `Assemblies/` as build output); `LoadFolders.xml` maps `/` and `1.6`; no solution file (see engineering decisions); C# sources under `Source/Mwah/` (`DefOf/ Defs/ Actions/Kiss/ Jobs/ Rewards/ Systems/`). Directory details follow `ls`; this section keeps no tree.

| What to look at | Where |
|---|---|
| Bottom-bar entry and panel | `Source/…/Actions/Kiss/MainButtonWorker_KissDirector.cs` → `Dialog_KissDirector.cs` (non-modal: avatar frames + heart + quick chain; picking primitives in `KissDirector.cs`) |
| Right-click entry | `Source/…/Actions/Kiss/FloatMenuOptionProvider_Kiss.cs` (vanilla reflection discovery, no registration) |
| Who goes / whether it may | `Source/…/Actions/Kiss/KissUtility.cs` (`KissProposal` tri-state) |
| Performance and settlement | `Source/…/Jobs/JobDriver_Kiss.cs` (two-person mirror job) |
| Mood and social scaling | `Source/…/Rewards/KissMoodReward.cs` (silent for units without `needs.mood`) |
| Who may kiss whom (tiers) | `Source/…/Actions/Kiss/KissScope.cs` — seven-tier cumulative disjunction, factory far right |
| Cooldowns | `Source/…/Systems/KissCooldown.cs` (in-session memory state) |
| Unit conversion, single entry | `Source/…/MwahTime.cs` |
| Thing-kiss addons (pipeline/registry/wall) | `Source/…/Actions/Kiss/KissThingAddons.cs` + `KissWallAddon.cs` + `Jobs/JobDriver_KissThing.cs`; fate table `1.6/Defs/Kiss/MWAH_FateDefs.xml` + `Defs/MWAH_FateDef.cs`; instance narration `Rewards/Thought_MemoryFated.cs` |
| Settings and lifecycle | `Source/…/MwahSettings.cs` + `Mod.cs` (immediate effect + debounced persistence) |
| Logging/instrumentation design and engine ceilings | `docs/kiss-trace-logging-design-zh.md` — landed: unconditional banner, diagnostic surface behind the `diagnosticLevel` four-rung slider (Off/Auto/Simple/Verbose) |
| Where else the player HUD can be hung (full 1.6 list) | `docs/hud-ui-surfaces-1.6.md` — zero-Harmony surface / Harmony-only surface / conclusion: bottom bar stays the main entry |
| Packaging and gates | `scripts/`; the single release-process entry `docs/release-runbook-zh.md`; cross-project skeleton `../modding_documents/RimWorld_Mod_RepoInit_AgentMemory_And_Packaging_Guide_zh.md`; native heap-crash triage `../modding_documents/RimWorld_NativeHeapCrash_Triage_Guide_zh.md`; this project's silent-failure corpus (engine contracts / input layer / job clock, incl. the DefInjected flat-key relapse) `../modding_documents/RimWorld_Mod_Silent_Failures_Engine_Contracts_zh.md` |
| Design decision compendium (moved from MEMORY 2026-10-06) | `docs/design-decisions.md` |
| Release evidence reviews | `docs/release_review/` |

## Commit and command practices

- Conventional Commits 1.0.0; imperative English subject; body may explain motives and trade-offs (any language, but memory files themselves are English). Atomic split: code travels with its Defs (splitting leaves an uncompilable middle state); localization may separate; rename-style changes follow as their own commit; never rewrite history.
- Branch model, release-only `main`, append-only history: `AGENTS.md` + `docs/release-runbook-zh.md` are the authorities; never-committed paths live in `.gitignore`.
- **Scratch never inside the repo root**: verify-local's privacy gate and the pre-push audit scan the working tree — a decompiled vanilla dump left at `./.tmp-decomp/` once carried absolute local paths and broke both rituals mid-run (2026-10-05; the archive agent relocated it non-destructively). Throwaway decompilation/output goes to a sibling folder outside the repository, never inside it.
- Commands: `dotnet build Source/Mwah/Mwah.csproj -nologo`; `pwsh scripts/verify-local.ps1` (all gates); three-channel packaging `build-dev.ps1` / `pack-github.ps1 -Version v…` / `pack-steam.ps1` (packaging always builds, each channel its own flavor; pack-dev is build's alias layer). No test project; the live surface is `TODO.md`; full flow `docs/release-runbook-zh.md`.


## Verification status

- Release evidence: `docs/release_review/` — `release-0.2.2-review-zh.md` carries the download-level asset verification, the audit runs, and the dev-package live acceptance for the shipped 0.2.2. Offline standard = `verify-local` fully green (its own output is the authority on gate count); live testing is the maintainer's alone; unchecked = unverified.
- The reverse key check ("defined but never referenced") is a gate worth its keep: during development it caught `Mod.cs` losing its `SettingsCategory()` override — a non-empty return is the sole condition for the settings page appearing under "Mode options", and losing it makes the entire settings surface unreachable. Think hard before deleting this check.

Current goals, open actions, and deferrals live only in `TODO.md`; cold evidence in `OBLIVIONIS.md`, which cannot override this file or the source.
