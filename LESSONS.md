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

## Review checkpoint — root layout and 0.4.0 publication

- **Owner:** Main coordinator for the maintainer-authorized cleanup, cloud integration and GitHub publication.
- **Reviewed through:** Previously covered root-content/documentation work and corrected findings; PR #3 integration and default-main result; channel builds, v0.4.0 publication and the downloaded asset; PR #4 documentation closeout and successful main CI; the maintainer's Steam upload/page/tag completion report; verified shutdown of the six idle Mwah Herdr executors; the current uncommitted completion-record diff. No runtime or content rule changed after the accepted layout.
- **Evidence:** Existing native acceptance, tree/inventory/identity inspection, successful dual-target/three-flavor CI and full-history gates, public Release metadata and actual package audits; the maintainer's channel confirmation and Herdr workspace read-back after closure. [Release status](docs/release-status.md) owns channel facts and locations; [tasks](openspec/changes/shared-content-and-lifecycle/tasks.md) owns scoped completion. The diff preserves evidence limits and the published tag's identity.
- **Coverage / next owner:** Both channel outcomes and this project's executor retirement are covered; no release obligation remains assigned. Normal documentation push/PR checks still run mechanically. Prior native acceptance is reused without claiming a new game run or a separately observed Steam upload time. The two accepted historical privacy entries remain unchanged; the successful CI's Node action-runtime deprecation annotation is not a mod-runtime failure.
- **reviewed_at:** 2026-11-17T01:14:29+08:00 (Asia/Hong_Kong).
- **Lessons:** L-001 and L-002 remain Seen 1; this work adds no fresh supporting incident and no new lesson.
