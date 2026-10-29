> 历史快照：迁移前的英文规格／变更资料，来源为 deb427d。已由[现行规格入口](../../../specification.md)替代；不再定义现行合同，也不表示关联实施变更已经完成。

# Read the contract before changing Mwah!

## Where each kind of information belongs

| Location | Owns |
| --- | --- |
| `specs/<capability>/spec.md` | Baseline behavior requirements and acceptance scenarios |
| `changes/<change>/specs/<capability>/spec.md` | Added, modified, or removed requirements for that change |
| `changes/<change>/proposal.md` | Scope, approval decisions, and reasons for the change |
| `changes/<change>/design.md` | Technical choices, source evidence, risks, and unresolved design questions |
| `changes/<change>/tasks.md` | Implementation work, completion state, verification evidence, and blockers |
| `../TODO.md` | Entry point to active work, not a second task checklist |

Specification files contain contracts only: no implementation status, progress checkboxes, defect inventories, build results, or task plans. A normative requirement does not establish that its implementation passes.

The development contract is the baseline specification plus the relevant **approved** change deltas. Read the change proposal to establish approval; a draft does not supersede the baseline. Completed changes merge into the baseline through archive. Do not maintain a separate copy of the same requirements in memory or design notes.

## Spec-first workflow

1. Read the relevant baseline specs and active change deltas before changing behavior, code, Defs, settings, localization, or packaging.
2. Update the affected requirements or acceptance scenarios **before** implementation. For a bug fix or behavior-preserving refactor, express the required invariant and the concrete affected scenario; do not redefine correct behavior to match a defect. Small follow-ups can update the same coherent change.
3. Record scope decisions in the proposal and technical choices in the design. Ask the maintainer before changing a contract outside the authorized scope.
4. Run `openspec validate --all --strict --no-interactive`, then implement against those requirements. If implementation exposes a necessary contract change, return to the specs first.
5. Verify the relevant contract through existing builds/checks and maintainer-run game scenarios. Record actual results and remaining gaps in `tasks.md`; syntax validation is not gameplay verification. Do not write new unit tests or add cases to existing unit tests.
6. Archive only after the applicable work and verification are complete. After archive, read the merged specs and check that no requirement or scenario was lost. Do not weaken assertions or requirements to obtain a pass.

## Read by capability

- [Core kissing](specs/kissing/spec.md): initiation, admission, cooldowns, paired jobs, and settlement.
- [Thing-kiss addons](specs/addons/spec.md): independent switches, shared object behavior, and walls.
- [Settings](specs/settings/spec.md): controls, persistence, units, navigation, and diagnostics.
- [Compatibility](specs/compatibility/spec.md): supported environments, save identity, packaging, and hard boundaries.

List current work with `openspec list`; inspect a change with `openspec show <change>` or `openspec status --change <change>`.

`status` reports planning-artifact readiness. The task list, not artifact readiness, establishes implementation progress.

## Why this arrangement

OpenSpec keeps requirements in ordinary Markdown while separating baseline contracts from change deltas. Its existing CLI provides structural validation, so a second home-grown specification checker is unnecessary. Plain Markdown without a change convention would be simpler initially, but would leave the distinction between established requirements and a proposed change informal.

This project uses the standard `spec-driven` layout and the installed OpenSpec CLI, without custom schemas, tool-generated instruction packs, a new runtime dependency, or a second planning platform. Product guidance lives here and in `AGENTS.md`; no slash command is required.

References: [official concepts](https://github.com/Fission-AI/OpenSpec/blob/main/docs/concepts.md), [CLI reference](https://openspec.dev/docs/cli).
