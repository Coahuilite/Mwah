# 贡献指南 · Contributing

欢迎 PR。先读 [`README.md`](./README.md) 的「硬边界」——它们是这个模组的全部性格，绕过边界的贡献不会被合并。

PRs welcome. Read the "Hard boundaries" section of [`README.md`](./README.md) first — they are this mod's entire personality, and contributions that route around them will not be merged.

## 规则 / Rules

开发前先读[现行规格与变更流程](./docs/specification.md)。先区分恢复、既有合同补录和规则变化；纯恢复无需制造 delta，真实规则变化先写完整差量再实现。候选明确采用前不替代现行正文。scope、技术取舍和完成依据分别放在 proposal、design、tasks／实现证据，不把它们混入规格。

Read the [current specifications and workflow](./docs/specification.md) first. Distinguish restoration, traceable baseline documentation and rule changes. Pure restoration needs no delta; specify real rule changes before implementation. Candidates do not replace current contracts until adopted. Scope, design and evidence belong outside the specs.

1. Fork 后从当前开发分支拉自己的分支，PR 对着它提即可；分支与合并模型由维护者在合并时决定，贡献者无需迁就。
   Fork, branch off the current development branch, and open your PR against it. The branching and merge model is the maintainer's call at merge time — contributors don't need to accommodate it.
2. 提交信息用 Conventional Commits（`feat` / `fix` / `docs` / `refactor` / `chore` / `style` / `test`，模板见 `.gitmessage`）；一个 commit 只做一件事。
   Use Conventional Commits (template in `.gitmessage`); one commit, one thing.
3. **零 Harmony / Zero Harmony**：不引入 `Lib.Harmony`、不做运行时补丁；需要补丁才能做到的功能，直接判定为不做。
   No `Lib.Harmony`, no runtime patches. If a feature needs a patch to exist, it is out of scope by definition.
4. **仅游戏内地图、仅单机 / Live maps, single-player only**：不碰世界地图、商队、多人同步。
   Don't touch the world map, caravans, or multiplayer sync.
5. **双语 / Bilingual**：玩家可见文字必须同时提供 English 与 ChineseSimplified 键（`1.6/Languages/`）；键集合一致是构建门，漏一个键 CI 就红。
   Player-visible text needs both English and ChineseSimplified keys (`1.6/Languages/`); matching key sets are a build gate — one missing key turns CI red.
6. **tick 单位 / Ticks only**：时长一律以 tick 存储，界面同时呈现 tick / 现实秒 / 游戏小时。
   Durations are stored in ticks and displayed as ticks / real seconds / game hours.
7. **隐私 / Privacy**：仓库里不写绝对本地路径、凭据、工坊 ID——`scripts/privacy-audit.ps1` 会替你记住这件事。
   No absolute local paths, credentials, or workshop IDs anywhere in the repo — `scripts/privacy-audit.ps1` remembers this for you.

## 验证 / Verification

```powershell
pwsh scripts/verify-local.ps1
```

涉及实现、运行内容或打包时，既有离线门必须通过（三渠道构建、双语键、DefOf、天意、版本、DLL 与零 Harmony 等）；复用对同一候选仍有效的证据。纯文档改动检查合同语义、链接、读取方及隐私，不机械重跑构建。实机测试仍由维护者执行，不建 junction、不代跑游戏。

Implementation, runtime-content and packaging changes retain the existing offline gates. Reuse valid evidence for the same candidate. Documentation-only work checks semantics, links, readers and privacy rather than rerunning unaffected builds. In-game execution remains the maintainer’s responsibility; do not create game-install links.

不新增单元测试，也不向现有单元测试添加用例；使用现有检查及维护者实机路径。普通 Markdown 不依赖 OpenSpec CLI；可选结构校验不能代替语义对账或运行验证。

Do not write new unit tests or extend unit-test cases. Use existing checks and maintainer-run game paths. Plain Markdown needs no OpenSpec CLI; optional structural validation does not establish semantics or runtime behavior.

## 代码口味 / Taste

面向人类的代码优先：命名说人话，注释写"为什么"而不是"是什么"。共用参与规则、请求判定和 job 表演保持清楚边界；新非 Pawn 目标复用物体吻管线。架构变更按已批准变更的设计推进，不把旧类名和注册方式当作永久接口。

Human-facing code first: clear names, comments that explain *why*, and explicit boundaries between shared participation rules, request evaluation, and job performance. New non-Pawn targets reuse the Thing-kiss pipeline. Follow the approved change design for architecture work; old class names and registry mechanics are not permanent interfaces.
