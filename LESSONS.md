# Lessons

## L-001 — Match a verification assertion to the layer that owns the identity

- **Rule / trigger:** When editing the DLL symbol inventory, require a symbol only if the implementation actually emits the managed type/member or literal being audited. An XML-only Def identity belongs to the existing XML and resolver checks; do not add dummy runtime fields to satisfy an unrelated ASCII inventory.
- **Why / scope:** A descriptor-backed JobDef may be resolved through metadata without its DefName appearing as an ASCII DLL symbol. This applies to the current `verify-local` symbol audit, not to removal of genuine runtime-entry or DefOf assertions.
- **Evidence:** During machine implementation on 2026-11-05, the newly added `MWAH_KissMachine` ASCII assertion failed for both targets. The JobDef existed in [XML](Common/Defs/Kiss/MWAH_JobDefs.xml) and was resolved by [KissThingAddons.ForJob](Source/Mwah/Actions/Kiss/KissThingAddons.cs); it had no matching managed symbol obligation. The invalid new assertion was removed, genuine class/Thought symbols retained, and the existing full offline gates passed as recorded in [machine tasks](openspec/changes/machine-kissing/tasks.md). The current [verification script](scripts/verify-local.ps1) retains the separate XML/Def and managed-symbol checks.
- **Seen:** 1 completed-review date, 2026-11-09. The older incident is not a backdated review or multiple observations.
- **Promotion destination / owner:** Existing verification procedure in `docs/release-runbook-zh.md`, under maintainer authorization; stable engineering constraints belong in AGENTS. Apply this lesson now; promotion does not authorize new tests or weaken existing requirements.

## Review checkpoint — project organization

- **Owner:** Codex executor for the maintainer-authorized documentation scope.
- **Reviewed through:** The documentation migration, state/readers and retained contracts previously covered here; the pending machine/VNPE runtime/content diff and its recorded evidence; Wiki plus ModLoadFolders/LoadFolder source observations; the conditional-content proposal and 0.4.0 assessment. Included the current double-target/three-flavor gates, dev staging, full-history/package privacy audit, staged scope and Markdown links. Staging exposed EOF blank lines in previously untracked files; normalized only their final newline and passed the cached diff check. Fixed stale source-comment MEMORY references. Actual evidence and local-commit scope are in [organization tasks](openspec/changes/project-organization/tasks.md).
- **reviewed_at:** 2026-11-09T01:15:39+08:00 (Asia/Hong_Kong).
- **Coverage limit / remaining owner:** Reused applicable implementation/acceptance evidence; no new game run or full audit of unchanged runtime source. New LoadFolders behavior remains a plan, with later native acceptance at the maintainer. Local commit history owns the delivered revisions; pushing/publication remain separate. L-001 stays at Seen 1 on this same review date; the whitespace correction applies the existing untracked-file rule rather than creating a duplicate lesson.

## Review checkpoint — 0.4 planning

- **Owner:** Main coordinator for the maintainer's planning/alignment checkpoint.
- **Reviewed through:** The uncommitted shared-content-and-lifecycle proposal, architecture design, state inventory, task plan and candidate NATIVE-02 block; redirects from the narrower VNPE plan and the current specification/TODO entry. Source observations cover Job admission/completion/binding, departure, director callbacks, edit/save ownership and clock/reset state. Consumed the two effective OMP/Herdr cross-checks at their actual narrow scope, including stale-install precedence and QuickChain/PostClose; excluded unsupported or unnecessary expansions. Exact task/request IDs and delivery reconciliation remain in the owning task record.
- **reviewed_at:** 2026-11-09T10:19:25+08:00 (Asia/Hong_Kong).
- **Evidence / limit:** Static sources, artifact links/headings, whitespace/diff and targeted new-document privacy checks; retained the old NATIVE-02 scenario in the candidate and left the current contract unchanged. No runtime changes, builds, native game execution, branch creation, commits or pushes in this planning phase. User alignment is the next activation condition; later native verification remains maintainer-owned.
- **Lessons:** No new active lesson or Seen increment. Existing scope, evidence and untracked-artifact requirements apply immediately; a transport failure stays with its task/recovery record.
