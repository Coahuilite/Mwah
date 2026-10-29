# MEMORY

## Current durable state

- RimWorld 1.6 / 1.7 mod, `packageId` `coahuilite.mwah`. **0.3.0 is published on GitHub and Steam Workshop**; the maintainer confirmed Workshop upload and page updates complete on 2026-10-30. Development and the remote default remain `0.3.x`; `main` is release-only. Repository: `github.com/Coahuilite/Mwah` (public, MPL-2.0). The csproj `<Version>` is primary; the follower gate locks About, Workshop copy and both changelogs to it. Channel evidence: `docs/release_review/release-0.3.0-review-zh.md`; process: `docs/release-runbook-zh.md`.
- Feature surface: see `README.md` / `README.zh-CN.md` "What you can do" — three initiation paths (right-click, Kiss Director panel, ambient matchmaking off by default), a descriptor-backed addon layer for non-pawn targets (walls and trees, with optional per-Pawn Ideology outcomes), fate tables driving all settlement copy, mood only for pawns with `needs.mood`.
- Product behavior and permissive defaults are specified in the current Markdown contracts rather than repeated here. The workspace folder name `every_pawn_kiss_each_other/` is historical and carries no engineering identity.
- Explicitly never (not a to-do): world map / caravan tiles, RimWorld Multiplayer sync, true body-contact kiss animation, downed pawns as initiators (would need Harmony to open vanilla gates).
- **Never junction**: nothing is created, verified, or assumed under `Mods/`; copying the mod and live testing are the maintainer's own actions (2026-09-02 maintainer order).

## Naming decisions

- Identity lives in `AGENTS.md` (brand, packageId, prefixes, the Pawn noun rule, version source). History kept here: the gate's far-right label decided on 2026-09-02 as the brand name is now ordinary copy (`所有 Pawn 都互相亲吻`); the workspace folder name is a historical value (see design stance).
- Candidate elimination rationale and the vanilla-token occupancy audit (`KISS_/PECK_/CHUU_/XOXO_/SMOOCH_` death causes, six candidates all unoccupied) are archived in `OBLIVIONIS.md` (2026-10-04 section); the settled answer stands: MWAH is onomatopoeia, never an initialism.
- **Copy ruling (2026-09-22 maintainer order, supersedes the 2026-09-02 prompt decree)**: "啵嘴" may appear only in the Chinese display name `所有Pawn都给我啵嘴！` (the single `MWAH.SettingsCategory`); **all in-game copy uses the 亲吻/吻 register** — bottom bar `亲吻导演台`, picking prompts `谁要发起亲吻？` / `{PAWN} 想要和谁亲吻？`, gate greyed item `…不想和 …亲吻`, thought entry `被{0}亲吻过` / opinion `我们的那一吻` / description `有人吻了我，我也吻了回去。`, report string `亲吻TargetA。`, settings `自主亲吻` / `亲吻冲动间隔` / `无视亲吻冷却` / `亲吻导演台按钮`. The English side is unchanged (always kiss wording). Matrix rows and this file that once quoted the old prompts are synced; the "维护者要求『所有 pawn 都啵嘴』" quote elsewhere in this file is a verbatim requirement quote and stays.

## Vanilla capability boundaries

- Version-anchored engine observations live in `docs/vanilla-capability-boundaries.md`; its section names remain stable for older references. Read observations relevant to the affected integration, and recheck changed APIs against the target build. Product requirements are linked from `docs/specification.md`.

## Specification and design authority

- `docs/specification.md` is the single human entry; `openspec/specs/` contains the current Chinese contracts, including already accepted rules formerly split across baseline and delta. Candidates do not define another current contract.
- Pure restoration does not require a delta. Real rule changes are specified before implementation; adoption, implementation acceptance and release are distinct. Workflow and ownership are defined by the entry and AGENTS.md.
- Acceptance stays with the existing change task lists: `openspec/changes/tree-kissing-and-settings/`, `openspec/changes/social-impact-soft-cap/`, and `openspec/changes/mobility-and-kiss-completion/`. Their `docs/development/` records preserve actual evidence and unrecorded coverage; the maintainer accepted this release without claiming every scenario was separately run. Historical spec/tool documents are under `docs/archive/specification-before-workflow/`.
- `docs/design-decisions.md` and `docs/vanilla-capability-boundaries.md` preserve dated reasoning and engine observations, not parallel specifications. Release operations remain governed by `docs/release-runbook-zh.md` and AGENTS.md.
- Attribution discipline: trust authoritative engine output; never default-explain a failure with an assumed old installation. Preserve useful actor/context identifiers in diagnostics.

## Repository structure and navigation

Canonical editable content remains under `1.6/` (`Defs/Kiss/`, `Languages/{English,ChineseSimplified}/`). Builds output separate `1.6/Assemblies/` (net472) and `1.7/Assemblies/` (net10.0) DLLs. Packaging copies the canonical content into both minor folders; exact `LoadFolders.xml` selections activate only the matching minor. A source checkout is not a complete 1.7 installation: use the staged package. No solution file (see engineering decisions); C# sources under `Source/Mwah/` (`DefOf/ Defs/ Actions/Kiss/ Jobs/ Rewards/ Systems/`). Directory details follow `ls`; this section keeps no tree.

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
| Thing-kiss addons (pipeline/registry/content) | `Source/…/Actions/Kiss/KissThingAddons.cs` + `KissWallAddon.cs` / `KissTreeAddon.cs` + `Defs/MWAH_KissAddonDef.cs` + `Jobs/JobDriver_KissThing.cs`; fate table `1.6/Defs/Kiss/MWAH_FateDefs.xml` + `Defs/MWAH_FateDef.cs`; instance narration `Rewards/Thought_MemoryFated.cs` |
| Settings and lifecycle | `Source/…/MwahSettings.cs` + `MwahSettingsView.cs` + `Mod.cs` (canonical values, transient two-page view, debounced persistence) |
| Logging/instrumentation design and engine ceilings | `docs/kiss-trace-logging-design-zh.md` — landed: unconditional banner, diagnostic surface behind the `diagnosticLevel` four-rung slider (Off/Auto/Simple/Verbose) |
| Where else the player HUD can be hung (full 1.6 list) | `docs/hud-ui-surfaces-1.6.md` — zero-Harmony surface / Harmony-only surface / conclusion: bottom bar stays the main entry |
| Packaging and gates | `scripts/`; the single release-process entry `docs/release-runbook-zh.md`; cross-project skeleton `../modding_documents/RimWorld_Mod_RepoInit_AgentMemory_And_Packaging_Guide_zh.md`; native heap-crash triage `../modding_documents/RimWorld_NativeHeapCrash_Triage_Guide_zh.md`; this project's silent-failure corpus (engine contracts / input layer / job clock, incl. the DefInjected flat-key relapse) `../modding_documents/RimWorld_Mod_Silent_Failures_Engine_Contracts_zh.md` |
| Product specs, workflow and evidence | `docs/specification.md`; implementation design/tasks under `openspec/changes/tree-kissing-and-settings/`; historical reasoning in `docs/design-decisions.md` |
| Release evidence reviews | `docs/release_review/` |

## Commit and command practices

- Conventional Commits 1.0.0; imperative English subject; body may explain motives and trade-offs (any language, but memory files themselves are English). Atomic split: code travels with its Defs (splitting leaves an uncompilable middle state); localization may separate; rename-style changes follow as their own commit; never rewrite history.
- Branch model, release-only `main`, append-only history: `AGENTS.md` + `docs/release-runbook-zh.md` are the authorities; never-committed paths live in `.gitignore`.
- **Scratch never inside the repo root**: verify-local's privacy gate and the pre-push audit scan the working tree — a decompiled vanilla dump left at `./.tmp-decomp/` once carried absolute local paths and broke both rituals mid-run (2026-10-05; the archive agent relocated it non-destructively). Throwaway decompilation/output goes to a sibling folder outside the repository, never inside it.
- Commands: `dotnet build Source/Mwah/Mwah.csproj -nologo` remains the 1.6 entry; add `-p:MWAHGameVersion=1.7` for native 1.7, or use `pwsh scripts/build-runtime.ps1` for both; `pwsh scripts/verify-local.ps1` (all gates); three-channel packaging `build-dev.ps1` / `pack-github.ps1 -Version v…` / `pack-steam.ps1` (packaging always builds, each channel its own flavor; pack-dev is build's alias layer). No test project; the live surface is `TODO.md`; full flow `docs/release-runbook-zh.md`.


## Verification status

- Release evidence: `docs/release_review/` — `release-0.2.2-review-zh.md` carries the download-level asset verification, the audit runs, the dev-package live acceptance, and the 2026-10-06 read-only workshop page verification (zh+en) for the shipped 0.2.2. Offline standard = `verify-local` fully green (its own output is the authority on gate count); live testing is the maintainer's alone; unchecked = unverified.
- The reverse key check ("defined but never referenced") is a gate worth its keep: during development it caught `Mod.cs` losing its `SettingsCategory()` override — a non-empty return is the sole condition for the settings page appearing under "Mode options", and losing it makes the entire settings surface unreachable. Think hard before deleting this check.

Current work is linked from `TODO.md`; the active change task list owns implementation state and evidence. Cold records in `OBLIVIONIS.md` do not override product specifications.
