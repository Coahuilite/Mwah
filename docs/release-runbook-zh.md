# 发布流程

> 唯一流程入口（2026-10-04 建立，学自 SqueakyRatkin 并按本项目规模瘦身）。本地 commit 自由执行；push / PR / merge / tag / GitHub Release / Workshop 上传均须对应的维护者授权。本文不是授权。

## 三命令契约

| 用途 | 命令 | 覆盖 |
| --- | --- | --- |
| 日常开发 | `pwsh scripts/verify-local.ps1` | 25 道离线门：三渠道构建、XML 良构、双语键一致、DefOf 解析、天意表完整性、DLL 符号与零 Harmony 反证、版本轴、设置项三处锁死 |
| 出包 | `pwsh scripts/build-dev.ps1` / `pack-github.ps1 -Version v…` / `pack-steam.ps1` | 三个渠道各建各的 flavor，**打包必构建**（2026-10-04 事故纪律：只 stage 旧 DLL 会把上一渠道产物装进新标签的包） |
| push 时（自动） | pre-push hook 跑 `privacy-audit.ps1 -FullHistory`（装法：`pwsh scripts/install-hooks.ps1`，每 clone 一次） | 全向量 + 身份面机械拦截，失败即拒推；CI 在全量 clone 上复跑同一 `-FullHistory` 审计兜底（`--no-verify` 无处藏） |

已有还原缓存或离线环境给构建类命令加 `-NoRestore` 不适用（本项目脚本自带 restore）；浮动版本 `1.6.*` 的隐式 restore 在无网时会 NU1301。不要在发布检查里手工重复脚本已覆盖的逐项复核——**新增检查先做成脚本，再进本文**。

## 分支模型（2026-10-04 裁定，对齐兄弟仓；2026-10-05 起对版本分支无关）

- **`main` 只做 release**：只在发布时前进；tag 必须落在 `origin/main` 的历史上（release CI 强制校验血统）。
- **开发在当前版本分支 `<minor>.x` 上**（取代 `dev`；每个 minor 开自己的版本分支，`dev` 不再复活；已归档的版本分支冻结、不再收新提交，当前是哪条看 `git branch --show-current` 与 `MEMORY.md` 耐久状态）。日常提交落当前版本分支，授权后 push → CI（CI 触发用 `[0-9].[0-9].x` 通配，切分支不改工作流）。
- 发布时：当前版本分支上的批准提交 → PR/squash merge 进 `main` → 在 main 的合并提交上打 tag → tag push 触发 Release CI。
- 历史 **append-only**：不 amend、不 rebase、不回写。

## 发布顺序

1. **候选准备**。版本主源 = csproj `<Version>`，`About/About.xml <modVersion>` 跟随（verify-local 门钉死三处一致）；实机矩阵全部验收通过；文案与双语键齐备。试玩用 `build-dev.ps1` 产物；dirty 包只作测试证据。
2. **最小仪式（仅发布）**：`verify-local` 全绿 → `privacy-audit -FullHistory -PrePush` 全绿（`-PrePush` 的干净树 / main 存在 / 待推提交与 tag 集合自检只在发布时有意义）→ 维护者裁决版本、渠道与工坊文案。日常 push 无人工仪式、无逐次授权：隐私拦截已由 hook + CI 机械化（2026-10-06 简化：同日两次人工漏跑证明靠记忆的门禁不是门禁）。
3. **合入 main**：squash merge（受保护 main 走 PR），合并后 `git diff --stat <当前版本分支> main` 应为空。
4. **GitHub**：在 main 的发布提交上 `git tag vX.Y.Z && git push origin vX.Y.Z`（需授权）。Release CI 校验 tag/版本/血统 → 25 门 → GitHub flavor → `dist/github/Mwah-vX.Y.Z.zip` → 自动建 Release（pre-release 由 `-` 后缀自动判定）。
5. **Steam（若获准）**：`pack-steam.ps1`（要求干净树）产出 `dist/steam/Mwah/`；维护者复制到本地上传副本，item ID 只写该副本的 `About/PublishedFileId.txt`（永不入库）。上传与页面维护由维护者本人执行。
6. **收尾**：更新 `MEMORY.md` 的耐久状态（版本号、已发布渠道）与 `TODO.md`；渠道观察只支持页面级结论，一个渠道不证明另一个渠道。

## 证据边界

- 工作树/提交信息/历史 blob/身份四面的结论互不推定；CI 默认模式 ≠ push 前 FullHistory。
- 仓库证据、CI 输出、一个渠道的产物不能证明其他渠道的外部状态；未知的外部状态记 unverified。
- 真实凭据若进入可达历史：先撤销/轮换，再谈清理；历史重写需单独授权。
