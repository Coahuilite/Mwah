# AGENTS.md — Mwah! (Every Pawn Kisses Each Other)

> This file is the memory agreement for AI agents working in this repository. Human contributors should read `README.md`.

## Project identity

- Project: RimWorld 1.6 mod, brand short name **`Mwah!`**, full name **`Every Pawn Kisses Each Other`**; neither is ever translated, normalized, or copy-edited. The Simplified-Chinese display name **`所有Pawn都给我啵嘴！`** lives only in the Chinese language pack (`MWAH.SettingsCategory`): 1.6 `About.xml` has no per-language rename, so the mod list always shows the English name.
- Permanent `packageId` `coahuilite.mwah` (immutable after release). Engineering identity is `Mwah`: C# namespace, assembly, and source folder share it. No solution file — the IDE opens the csproj, and build/pack scripts only ever point at the csproj.
- Naming prefixes: Defs `MWAH_`, Keyed keys `MWAH.`, logs `[MWAH] ` (hard-coded English, never localized, no placeholder keys).
- Product version source: `Source/Mwah/Mwah.csproj <Version>` is primary; `About/About.xml <modVersion>` follows it; product version is SemVer. License is **MPL-2.0** for the whole Coahuilite mod series.
- Repo, remote, and publication state are not pinned here — read `MEMORY.md`.

## Project philosophy

- Zero Harmony is a hard boundary: no reference, no declared dependency, no runtime patch. Any capability that would need a patch to unlock is judged "not done".
- Live map only, single-player only: the world map, caravan tiles, and multiplayer sync are out of scope.
- Pure entertainment with the lowest workable gate: no faction/species/draft restriction by default. The maintainer's stance (reaffirmed 2026-09-02): a player wish to "only matchmake the colony" is absorbed by the gate slider (factory far-right = unrestricted), never by changing the default. "Zero gate" was never true and is not faked — vanilla `CanTakeOrder` / `ShouldGenerateFloatMenuForPawn` pre-filter, so animals and neutrals cannot initiate and downed pawns get no menu.
- The player is super-intelligence's hand: a hand-issued order (right-click / director) overrides cooldowns and the combat gate — "if the super-intelligence wants two pawns to kiss, they kiss" (maintainer ruling). Only the ambient dispatcher obeys the combat gate.
- Silent invalidation is constitutional: when an addon switch is off or a target collapses, it vanishes from every pick surface with zero nag — no greyed item, no dispatched job, no "no longer valid" message.
- Player-facing text must ship with identical key sets in every language; durations are stored in ticks only and the UI must show tick / real seconds / game hours together.
- Never create, verify, or assume a junction/symlink into a game install; copying the mod and live testing are the maintainer's own actions.

## Memory protocol

At every non-trivial session:

- Read `MEMORY.md` before claiming project context; it stores confirmed durable facts, decisions, constraints, and evidence pointers.
- Read `TODO.md` before continuing work; it stores only current goals, open actions, blockers, and explicit deferrals.
- Read `OBLIVIONIS.md` only for a historical conflict or explicit request; it is cold archive evidence and cannot override current sources.
- The three active memory files (`AGENTS.md`, `MEMORY.md`, `TODO.md`) are maintained in accurate English; `OBLIVIONIS.md` is the cold archive and follows the same language rule when appended.

Maintain these boundaries:

- Update `MEMORY.md` only when durable facts or the open action surface changes; keep it compact.
- Compact by default: settled release/implementation detail and superseded decisions move to `OBLIVIONIS.md` (cold archive) or `docs/`; MEMORY keeps only durable facts and pointers. Do not grow MEMORY with finished work.
- Update `TODO.md` only when its current task surface changes.
- Do not store session narratives, transient artifacts, raw logs, completed test matrices, commit chains, or release checklists in either active memory file.
- Documentation edits alone are not memory events; external-state summaries never override their authoritative source.

## Privacy and security

- Default scope is this repository root. Reading outside it requires path-specific authorization and remains read-only; the named RimWorld `Player.log` troubleshooting directory is the standing read-only exception.
- Never place personal local paths, diagnostic-log excerpts, credentials, API keys, tokens, private keys, or `PublishedFileId.txt` values in Git, documentation, generated artifacts, staging, or reachable history. Runtime content ships only inside the version folder.
- Every push is preceded by a privacy review of the complete reachable range (`scripts/privacy-audit.ps1 -FullHistory`); CI runs the default scan on every push and PR.

## External-state boundaries

- Local commits are permitted without a separate step: reversible, never leave the machine, and how a release is assembled. Remote-facing operations — `git remote`, push, PR, merge, tag, GitHub Release, Workshop upload — each require explicit maintainer authorization.
- Branch model: `main` is release-only (a tag must lie on main's history; release CI checks ancestry); development happens on the per-minor branch (currently `0.1.x`). History is append-only: no amend, rebase, or rewrite.
- The pre-push ceremony is deliberately minimal: `scripts/privacy-audit.ps1 -FullHistory -PrePush` plus maintainer authorization. Everything else is automated by `verify-local` and the workflows — do not re-add manual ritual.
- The single process entry point is `docs/release-runbook-zh.md` (three-command contract, release order, evidence boundaries); this file fixes only the principles, never the command details.
