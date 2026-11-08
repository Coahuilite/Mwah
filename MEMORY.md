# MEMORY

## Working protocol

### Local map

- **Task state:** [TODO.md](TODO.md) owns project-level goals, authorization and cross-change routing. Each linked OpenSpec `tasks.md` owns only its change's work, evidence and unmet acceptance. Release/channel state is owned by the linked release record. No STATE.md or START.md is adopted here; do not create a competing ledger. [LESSONS.md](LESSONS.md) owns active lessons and the scoped review checkpoint.
- **Standards and verification:** [AGENTS.md](AGENTS.md) fixes project constraints; [docs/specification.md](docs/specification.md) routes to current contracts and the local workflow. [docs/release-runbook-zh.md](docs/release-runbook-zh.md), `scripts/verify-local.ps1` and `scripts/privacy-audit.ps1` own existing procedures/checks. Use relevant scoped evidence; native installation and game testing belong to the maintainer. New unit tests and equivalent renamed cases remain prohibited.
- **Lesson promotion:** The maintainer owns adoption, or explicitly authorizes the project executor. Promote stable project constraints to AGENTS, operational verification/release hooks to the existing runbook/procedure, and actual behavior guarantees through the specification adoption workflow. Merge existing equivalents; these destinations grant no extra testing, publishing or cross-owner authority.
- **Review timezone:** Asia/Hong_Kong. Use an actual clock, ISO 8601 completion time with offset, and that local calendar date; no backdated Seen counts.

Project state owns global goals, authorization and cross-change responsibilities; scoped records, such as OpenSpec changes, retain their work and evidence. Each fact has one authority at its proper scope: link details instead of maintaining copies. Current focus does not enlarge a record's scope. Preserve existing authorities, including START; use STATE.md only when a needed project-level authority is absent, through authorized adoption/migration. MEMORY / Knowledge holds cross-task knowledge; LESSONS holds active lessons. Other optional todos are plans/views; the adopted TODO above retains its project-level authority. Workers report through responsible owners, not new ledgers.

### Resume, update and close

Keep the agreed goal, scope, authorization and completion conditions distinct from findings and plans. State identifies usable artifacts/evidence, unmet conditions, dependencies, owner and next action. Separate facts from hypotheses. Use existing fields or compact prose; retain detailed plans only when order matters to safety, permission or coordination.

Before consequential retries, reconcile uncertain records against actual work. An active label proves neither a live process nor permission; a worker report is not acceptance. Check only uncertainties affecting the next action. Missing permission or unresolved conflict stops the affected operation, not independent authorized work. Explicitly deferred work stays deferred.

Update the responsible source on material changes, not every command. Before interruption/handoff, fill only gaps preventing safe continuation. Defects stay in task state; LESSONS does not replace repair. Blockers name affected work, resolver and resumption condition. Continue while authorized action can advance the goal; otherwise return the blocker without polling unchanged state. The host or existing goal runner supplies activation.

At substantive closeout, compare results with original acceptance and relevant checks; never weaken the contract to fit implementation. Label evidence static, build, runtime, not run or unobservable, with release separate when needed. Reuse it while artifact, inputs and conditions remain applicable; changed premises reopen only affected conclusions. Distinguish implementation, delivery and acceptance. Close satisfied scope, retain outstanding acceptance at its owner, and remove completed activity under retention permissions once results/evidence are accessible. Closing or archiving a scoped record must leave project routing and other obligations reachable. No permanent Done diary or mandatory per-session handoff document.

### Knowledge maintenance

Route work to task state, established specifications/decisions to their existing sources, and reusable failure rules to LESSONS. Admit to Knowledge only facts whose loss could change a future decision or cause costly rediscovery and which no better source sufficiently holds. Keep conclusion, scope/exceptions and necessary evidence; omit cheaply recoverable environment facts and routine success logs.

Merge or replace affected items rather than append session notes. Reassess when relevant versions, implementation, decisions or counterevidence change; age alone does not expire knowledge. Hypotheses stay with investigations. Mature knowledge moves to an existing specification or procedure, leaving a useful pointer only; a small independent fact needs no separate document.

Compact affected Knowledge on update; review remaining content at phase closure or when duplication, staleness or reading cost warrants it. A size budget triggers review, not truncation. Preserve identifiers, values, units, version boundaries, user decisions/exceptions, evidence limits, unmet acceptance and recovery; route obligations to task state rather than discard them. Consult original sources when unclear. Before removing sole necessary conclusions/evidence, establish an accessible responsible source and repair references. Retain history only for concrete evidence, recovery or accountability needs, not an overflow archive. Read the result for the next task; add no compaction diary.

### Review and lessons

Review uncovered work before substantive delivery, handoff or waiting. At start/resume, recover interrupted reviews and uncovered earlier workdays. Across midnight, use the next natural checkpoint, not clock polling. Inspect actual changes, artifacts, existing verification, corrections and rework for avoidable failures or thin verification; reassess relevant lessons. Reuse evidence; no new work means no review or marker rewrite. Issues and comments are optional inputs.

Use one owner and reviewed-through checkpoint per scope, with actual covered work/evidence and reviewed_at. Store it in LESSONS if present, otherwise existing task state; transfer it with a pointer when LESSONS is created. Include uncommitted work and relevant corrections; date or Git HEAD alone is insufficient. Start from the authorized baseline, advance only through reviewed work and expose gaps. Coverage is not acceptance or repair. Create no file solely for a marker without a need for persistent continuation.

Read an actual clock at start/resume and review checkpoints. Use the declared named Review timezone, not worker machine settings; record reviewed_at at completion in ISO 8601 with UTC offset. Count its local calendar date; cross-midnight reviews count only on completion, delayed reviews are not backdated. Record timezone changes without reinterpreting old dates. If clock/timezone is unknown, retain findings and defer date-based counting.

Active lessons need a usable rule, why, evidence and scope from real errors, rework, corrections or verification omissions; apply them immediately within permissions. Existing requirements need no Seen threshold. Merge equivalents. Seen counts distinct completed-review dates finding fresh supporting evidence, at most once per date; rereading one incident, duplicate reports or elapsed days do not count. At three such dates, move a valid, actionable rule into its mapped stable entry; merge existing equivalents or correct/retire invalid rules instead. If authority or destination blocks promotion, retain an owned pending action in task state; do not claim completion.

Promotion connects the rule to its work/review trigger, action and necessary passing evidence in the entry's existing format, without adding testing or other permissions. Keep only useful migration pointers and needed evidence in LESSONS. Later violations attach to the existing rule: check its reach, clarity or validity, without resetting Seen or duplicating it. Lack of recurrence alone does not justify retirement. Maintain lessons within retention permissions; review and ordinary success require no new lesson or report.

## Knowledge

- Version-anchored engine ceilings and their evidence: [vanilla capability boundaries](docs/vanilla-capability-boundaries.md). Recheck only APIs affected by a target-build change; these observations do not override current product contracts.
- For a concrete historical design conflict, consult [design decisions](docs/design-decisions.md) or the existing `OBLIVIONIS.md` cold record as needed, not at every resume. Diagnostics and instrumented engine seams are explained in [trace logging design](docs/kiss-trace-logging-design-zh.md). None is a parallel current specification.
