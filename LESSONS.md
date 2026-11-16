# Lessons

## L-001 — Audit an identity at the layer that owns it

- **Rule / trigger:** When changing the DLL symbol inventory, require a symbol only when the implementation emits that managed member or literal. XML-only Def identities belong to the XML/resolver checks; never add dummy fields to satisfy an unrelated ASCII inventory.
- **Evidence / scope:** The machine implementation's 2026-11-05 ASCII assertion for MWAH_KissMachine failed although the descriptor-backed JobDef and resolver were valid. The invalid assertion was removed while genuine runtime/DefOf checks remained. The current [JobDefs](Defs/Kiss/MWAH_JobDefs.xml), [resolver](Source/Mwah/Actions/Kiss/KissThingAddons.cs) and [verification entry](scripts/verify-local.ps1) retain that separation.
- **Seen:** 1 completed-review date: 2026-11-09. Reusing the evidence during this cleanup is not a fresh incident.
- **Promotion / owner:** The existing verification procedure, under maintainer authorization, after the protocol's distinct-date threshold. This lesson grants no new tests or weakened checks.

## L-002 — Do not promote historical assumptions into current contracts

- **Rule / trigger:** When rewriting from archived research, carry its runtime/version and original scope. Check normative statements against the adopted contract and actual caller before copying them into current guidance; a previous observation is not a new permanent restriction or acceptance rule.
- **Evidence / scope:** The 2026-11-16 documentation draft introduced a blanket ban on new save fields, applied the old Mono debugging premise to 1.7 Waylith and treated quiet logs as functional acceptance. Integration restored identity/data-meaning preservation, separated the runtimes, and tied acceptance to observed behavior plus relevant diagnostics. Applies to documentation migrations, not to a demand to re-audit unchanged runtime code.
- **Seen:** 1 completed-review date: 2026-11-16.
- **Promotion / owner:** The specification workflow's source/scope review guidance, under maintainer authorization if the rule remains useful at the protocol threshold. Existing project constraints apply immediately.

## Review checkpoint — root layout and cloud handoff preparation

- **Owner:** Main coordinator for the maintainer-authorized cleanup and method calibration.
- **Reviewed through:** The root move and documentation/method calibration previously covered here; the maintainer's archive deletion and root-layout usability confirmation; the reconciled staging scope, cloud-standard privacy audit and compatibility-only Chinese/English Workshop sections. The new cloud authorization, tracked additions/removals/renames, main-only 0.3.0 closeout facts and the existing PR/squash/channel-build path have also been reviewed. Earlier author and independent-review reports were reconciled with the actual files rather than treated as acceptance by themselves.
- **Evidence:** Direct content comparison, valid two-target/three-flavor build and real package evidence, source/contract reading, actual file-reference inventory and version followers. Detailed completion and final privacy results belong to [tasks](openspec/changes/shared-content-and-lifecycle/tasks.md), not another ledger. Source changes after the layout are comments only; no new unit tests, game execution or publication.
- **Coverage / next owner:** The maintainer confirmed the root-layout content works on 2026-11-17; [native acceptance](openspec/changes/shared-content-and-lifecycle/native-acceptance.md) owns that report. Privacy evidence retains the two previously accepted historical exceptions without adding exemptions. This checkpoint covers preparation before the commit/push; actual cloud execution and final artifact evidence are owned by [PR #3](https://github.com/Coahuilite/Mwah/pull/3). No new game run or online Workshop page update is claimed; tag/channel publication remains separately authorized.
- **reviewed_at:** 2026-11-17T00:45:11+08:00 (Asia/Hong_Kong).
- **Lessons:** L-001 and L-002 remain Seen 1; this work adds no fresh supporting incident and no new lesson.
