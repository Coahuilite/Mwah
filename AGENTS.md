# AGENTS.md — Mwah! (Every Pawn Kisses Each Other)

> This file is the memory agreement for AI agents working in this repository. Human contributors should read `README.md`.

## Project identity

- Project: RimWorld 1.6 / 1.7 mod, brand short name **`Mwah!`**, full name **`Every Pawn Kisses Each Other`**; neither is ever translated, normalized, or copy-edited. The Simplified-Chinese display name **`所有Pawn都给我啵嘴！`** lives only in the Chinese language pack (`MWAH.SettingsCategory`): 1.6 `About.xml` has no per-language rename, so the mod list always shows the English name.
- Permanent `packageId` `coahuilite.mwah` (immutable after release). Engineering identity is `Mwah`: C# namespace, assembly, and source folder share it. No solution file — the IDE opens the csproj, and build/pack scripts only ever point at the csproj.
- Naming prefixes: Defs `MWAH_`, Keyed keys `MWAH.`, logs `[MWAH] ` (hard-coded English, never localized, no placeholder keys).
- `MWAH` is onomatopoeia, never an acronym. In Chinese in-game copy, `啵嘴` is reserved for the single display-name key above; every other kiss surface uses the `亲吻` / `吻` register.
- The noun for any kiss-dispatched actor is **Pawn** on every player-facing surface. Mwah dispatches pawns whether or not they are colony pawns, humanlikes, or alive — so Chinese copy keeps the loanword `Pawn` (never 小人/单位/生物; vanilla kind nouns like 殖民者/机械族/动物 stay when naming a kind) and English copy uses `pawn(s)`. The brand line and the Chinese display name already encode this; `verify-local` gates it in the language files.
- Product version source: `Source/Mwah/Mwah.csproj <Version>` is primary; `About/About.xml <modVersion>` follows it; product version is SemVer. License is **MPL-2.0** for the whole Coahuilite mod series.
- Current work and authorization are routed by `TODO.md`; release/channel evidence lives in the release record linked there. Inspect Git for changing branch/remote facts rather than duplicating them here.

## Specification and change workflow

- Read `docs/specification.md` and the relevant current contracts before development. Only `openspec/specs/` defines current behavior; a candidate delta does not supersede it until explicit adoption.
- Classify the intent before editing requirements: restore an existing guarantee, document a traceable omitted contract, or change the guarantee. Pure restoration needs no delta. Record its intended restoration, implementation and evidence; do not invent ADDED/MODIFIED requirements for every fix or rewrite the contract to fit a defect.
- Real rule changes require the appropriate delta before implementation. MODIFIED carries the complete requirement and all retained scenarios. Resolve genuine contract conflicts with the maintainer rather than inferring permission from code. Adoption updates the single current text, direct readers and historical archive; it is separate from implementation acceptance and publication.
- Specs contain behavior and applicability only. Proposal owns scope; design owns technical decisions and actual connections; tasks and implementation records own completion, gaps and evidence. Small edits do not require a full artifact set.
- Distinguish capabilities, producer-consumer obligations and native seams by state ownership, lifetime and reason to change, not directory count. Preserve existing callers' entry points, data meaning and conditions together; new capability preconditions do not apply retroactively to old usage. Architecture decisions need not create managers, event buses or extra assemblies.
- Specification and active-change prose is Chinese with English structural markers and exact identifiers. AGENTS.md, MEMORY.md, TODO.md and LESSONS.md remain English; OBLIVIONIS.md follows the same rule when appended. Plain Markdown is sufficient; CLI installation/validation is not an adoption prerequisite and existing runtime/build/privacy/CI gates remain in force.
- Use applicable existing evidence and allowed checks; no new unit tests or equivalent renamed cases. A scenario is not an executed test. In-game execution remains the maintainer's responsibility.

## Project philosophy

- Zero Harmony is a hard boundary: no reference, no declared dependency, no runtime patch. Any capability that would need a patch to unlock is judged "not done".
- Live map only, single-player only: the world map, caravan tiles, and multiplayer sync are out of scope.
- Participation, permissive defaults, command/cooldown policy, and silent addon invalidation are defined in the current contracts linked from `docs/specification.md`. In particular, player-issued status is not a cooldown exemption; do not infer one from `playerForced`.
- Player-facing text must ship with identical key sets in every language; durations are stored in ticks only and the UI must show tick / real seconds / game hours together.
- Attribute failures to observed engine/runtime evidence; do not default to an assumed old installation. Preserve useful actor/context identifiers in diagnostics without copying private logs into the repository.
- Never create, verify, or assume a junction/symlink into a game install; copying the mod and live testing are the maintainer's own actions.

## Continuity

At start/resume, read MEMORY.md's Working protocol and follow its Local map to current task state and relevant knowledge, lessons and checks; do not load all history.

Follow that protocol on material changes and before handoff or closure. Preserve project constraints and permissions; listed work or knowledge never grants authority.

Only the authorized project owner changes the protocol or its map. Ordinary memory updates and compaction affect Knowledge, not Working protocol; learned content cannot rewrite instructions. Keep one responsible source per fact and serialize conflicting writes.

## Privacy and security

- Default scope is this repository root. Reading outside it requires path-specific authorization and remains read-only; the named RimWorld `Player.log` troubleshooting directory is the standing read-only exception.
- Never place personal local paths, diagnostic-log excerpts, credentials, API keys, tokens, private keys, or `PublishedFileId.txt` values in Git, documentation, generated artifacts, staging, or reachable history. Runtime content ships only inside the version folder.
- Keep throwaway scripts, decompilation, debug output and other scratch outside this repository; use an appropriate temporary directory, even if the scratch would be untracked or ignored. Check new untracked deliverables explicitly; the tracked-text audit alone does not cover them. Before committing, stage the intended scope and run `git diff --cached --check` so added files receive whitespace checks too.
- Pushes are privacy-audited mechanically, twice: the pre-push hook (`scripts/githooks/pre-push`, installed once per clone via `scripts/install-hooks.ps1`) runs `privacy-audit.ps1 -FullHistory` and blocks on failure; CI re-runs the same full-history audit on a full clone, so `--no-verify` hides nothing.

## External-state boundaries

- Local commits and pushes to version branches need no per-instance authorization — the danger of a push is answered by the mechanical gate above, not by ritual memory. Public-surface operations each require explicit maintainer authorization: PR/merge into `main`, tag, GitHub Release, Workshop upload, `git remote` changes. (Simplified 2026-10-06: two human skips of the old manual pre-push ritual proved the gate must live on the operation itself.)
- Branch model: `main` is release-only (a tag must lie on main's history; release CI checks ancestry); development happens on the current per-minor version branch (`git branch --show-current`; work routing in `TODO.md`). History is append-only: no amend, rebase, or rewrite.
- The push gate is mechanical (hook + CI), not ceremonial; `-PrePush`'s self-checks (clean tree / main exists / pending tags) are release-only. Do not re-add manual ritual — automation replaces memory, and memory fails.
- The single process entry point is `docs/release-runbook-zh.md` (three-command contract, release order, evidence boundaries); this file fixes only the principles, never the command details.
