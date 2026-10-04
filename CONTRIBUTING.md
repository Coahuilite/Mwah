# 贡献指南 · Contributing

欢迎 PR。先读 [`README.md`](./README.md) 的「硬边界」——它们是这个模组的全部性格，绕过边界的贡献不会被合并。

## 规则 / Rules

1. Fork 后从 `dev` 分支开工作分支；PR 目标 `dev`（维护者 squash merge 进 `main`）。
2. 提交信息用 Conventional Commits（`feat` / `fix` / `docs` / `refactor` / `chore` / `style` / `test`，模板见 `.gitmessage`）；一个 commit 只做一件事。
3. **零 Harmony**：不引入 `Lib.Harmony`、不做运行时补丁。需要补丁才能做到的功能，请直接判定为不做。
4. 仅游戏内地图、仅单机：不碰世界地图、商队、多人同步。
5. 玩家可见文字必须同时提供 English 与 ChineseSimplified 键（`1.6/Languages/`，键集合一致是构建门，漏一个键 CI 就红）。
6. 时长一律以 tick 为存储单位；界面同时呈现 tick / 现实秒 / 游戏小时。
7. 仓库里不写绝对本地路径、凭据、工坊 ID——`scripts/privacy-audit.ps1` 会替你记住这件事。

## 验证 / Verification

```powershell
pwsh scripts/verify-local.ps1
```

25 道离线门（三渠道构建、双语键一致、DefOf 解析、天意表完整性、DLL 符号审计、零 Harmony 反证等）必须全绿。实机测试由维护者本人执行（不建 junction、不代跑游戏），贡献者只需保证离线门与代码可读性。

## 代码口味 / Taste

面向人类的代码优先：命名说人话，注释写"为什么"而不是"是什么"，判定逻辑集中在 `KissBoundary`，表演逻辑集中在 job driver。新"亲非 pawn 目标"内容请继承 `KissThingAddon`（注册表加一行 = 全通路接通），不要另起管线。
