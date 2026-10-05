# 贡献指南 · Contributing

欢迎 PR。先读 [`README.md`](./README.md) 的「硬边界」——它们是这个模组的全部性格，绕过边界的贡献不会被合并。

PRs welcome. Read the "Hard boundaries" section of [`README.md`](./README.md) first — they are this mod's entire personality, and contributions that route around them will not be merged.

## 规则 / Rules

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

全套离线门（三渠道构建、双语键一致、DefOf 解析、天意表完整性、版本跟随面一致、DLL 符号审计、零 Harmony 反证等；确切条目与数量以脚本自身输出为准）必须全绿。实机测试由维护者本人执行（不建 junction、不代跑游戏），贡献者只需保证离线门与代码可读性。

The full offline gate suite (three-channel builds, bilingual key parity, DefOf resolution, fate-table integrity, version-follower parity, DLL symbol audit, zero-Harmony proof, …; the script's own output is the authority on count) must pass. In-game testing is the maintainer's own workflow; contributors only owe green gates and readable code.

## 代码口味 / Taste

面向人类的代码优先：命名说人话，注释写"为什么"而不是"是什么"，准入四层判定集中在 `KissBoundary`、即时可用性编排收在 `KissUtility.Propose` 单入口，表演逻辑集中在 job driver。新"亲非 pawn 目标"内容请继承 `KissThingAddon`（注册表加一行 = 全通路接通），不要另起管线。

Human-facing code first: names that speak plainly, comments that say *why* rather than *what*, the four admission layers living in `KissBoundary` with immediate availability orchestrated by the single `KissUtility.Propose` entry, performance logic in the job drivers. New kissable non-pawn targets should extend `KissThingAddon` (one registry line wires up every path) instead of starting a second pipeline.
