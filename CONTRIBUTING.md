# 贡献指南 · Contributing

修改行为前，先从 [规格工作流](docs/specification.md) 找到 `openspec/specs/` 中负责该行为的现行合同。纯恢复不伪造 delta；真实规则变化先写完整差量。范围、技术选择和证据分别归 proposal、design、tasks。

Before changing behavior, use the [specification workflow](docs/specification.md) to find its current contract in `openspec/specs/`. Restoration needs no invented delta; real rule changes need a complete delta first. Keep scope, design and evidence in their responsible records.

## 修改正确的源 / Edit the source

- `Source/Mwah/`：一套 C# 项目，分别构建 1.6 `net472` 与 1.7 `net10.0`。只适配真实 API／框架差异，不复制玩法逻辑。
- 根 `Defs/`、`Languages/`：共同内容唯一编辑源。玩家文案同时维护 English／ChineseSimplified；时长以 tick 存储，界面同时显示 tick／现实秒／游戏小时。
- `Compatibility/<Mod>/`：条件兼容登记与专用文案。`LoadFolders.xml` 每版必须显式保留 `/`、当前 minor 和适用的条件根；版本目录只放各自原生程序集。
- `docs/`：自持的现行指引。历史归档不承担构建、运行或日常接续所需的唯一事实。

`Source/Mwah/` is one C# project with separate native targets; adapt actual API/framework differences without duplicating rules. Root `Defs/` and `Languages/` are the shared editing source, with matching English/Simplified-Chinese keys and tick-based durations. Optional registrations and dedicated copy belong in conditional compatibility roots. Each custom load selection explicitly retains `/` and its own native minor. Current guidance lives in `docs/`; required operating facts must not depend on a removable archive.

## 保留合同 / Preserve the contract

零 Harmony，仅游戏内地图与单机。保留 `coahuilite.mwah`、Mwah 程序集／命名空间、已发布 Def、设置键和保存字段的数据含义；新能力的前提不限制旧合法用法。新增保存数据须明确其默认、旧档读取和迁移语义，不将“保留身份”误写成禁止新增字段。

Zero Harmony; live maps and single-player only. Preserve published identities and saved-data meanings, and keep new-feature prerequisites off existing callers. New persisted data needs explicit defaults, legacy reads and migration semantics; identity preservation is not a blanket ban on new fields.

品牌为 `Mwah!`／`Every Pawn Kisses Each Other`；中文显示名为 `所有Pawn都给我啵嘴！`。参与者统一称 Pawn；其余中文亲吻界面使用「亲吻／吻」，日志以硬编码英文 `[MWAH] ` 开头。

Keep the brand names intact, call participants pawns, and use hard-coded English `[MWAH] ` log prefixes. New non-Pawn targets reuse the existing Thing-kiss pipeline. Native Toils own execution progress; derived phase views must not become a parallel state machine. See [architecture](docs/architecture.md) for the current seams.

## 验证与打包 / Verify and package

```powershell
pwsh scripts/verify-local.ps1
pwsh scripts/pack-dev.ps1 -Zip
pwsh scripts/privacy-audit.ps1 -PackageDir dist/dev/Mwah
```

复用仍适用的证据。实现／内容／打包改动运行受影响的现有门；纯文档检查语义、引用和隐私，不重跑无关构建。禁止新增单元测试、追加用例或等价换名脚本。实机验证由维护者执行，实施者不运行游戏、不安装模组、不建游戏目录链接。

Reuse applicable evidence. Run affected existing gates for implementation/content/packaging changes; documentation-only work checks semantics, references and privacy. Do not add unit tests, unit-test cases or equivalent renamed checks. The maintainer owns in-game testing; do not install, run or link into the game.

## 提交与隐私 / Commits and privacy

从当前开发版本分支建立贡献分支并向其提 PR，使用 Conventional Commits。历史只追加，不 amend／rebase；`main` 仅供发布。正式 merge／tag／Release／工坊操作须对应授权，具体步骤见 [发布流程](docs/release-runbook-zh.md)。

Branch and target PRs from the current development version branch, use Conventional Commits, and keep history append-only. `main` is release-only; merge, tag and publication require their own authorization. The [release runbook](docs/release-runbook-zh.md) owns the operating sequence.

不提交个人绝对路径、凭据、工坊发布标识或原始日志。临时脚本放系统临时目录；`art/` 本地原稿保留但不跟踪、不分发。新增文件须进入隐私检查范围；push hook 与 CI 负责全历史机械门禁。

Never commit personal paths, credentials, Workshop publication identifiers or raw logs. Keep temporary scripts outside the repository and local `art/` originals ignored and undistributed. Include new files in the privacy check; the push hook and CI enforce full-history auditing.
