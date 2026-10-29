# 0.3.0 发布记录

日期：2026-10-30（UTC+8）。维护者已确认 1.7 派发亲吻成功、功能正常，接受本候选验收，并授权 **GitHub 正式 Release、Steam 渠道打包、更新日志及工坊文案更新**。版本保持 **0.3.0**，开发分支 `0.3.x`，沿[既有流程](../release-runbook-zh.md)从 `main` 发布。本轮发布流程已完成。

## 验收采用

- 1.6.4871 rev591 的常规／倒地接收及本次修复日志见[配对记录](../development/mobility-and-kiss-completion.md#16-实机确认2026-10-30)。
- 维护者在既定 1.7.4990 rev1069 目标上确认派发亲吻成功、功能正常；此次采用依据是维护者反馈，未额外采集日志或逐项步骤。
- 社交压缩、表演结束点已有维护者确认。维护者据以上结果接受整个 0.3.0 候选并进入发布，不再把原逐项覆盖清单作为本次发布待办；未逐项记录的文化、UI、记忆变体和存读档场景仍保留其证据状态，不写成全部实测通过。
- 产品行为合同没有缩减或改变；本轮只更新发布状态、文案及维护者提供的 `About/Preview.png`。预览图已实际查看，包含 1.6／1.7、配对、墙和树主题，无私人界面、姓名或存档信息。

## 渠道状态

| 渠道 | 当前状态 |
| --- | --- |
| GitHub | **已正式发布**：[v0.3.0](https://github.com/Coahuilite/Mwah/releases/tag/v0.3.0)，非 draft、非 prerelease；实际公开时间为 2026-10-30 02:16:52 UTC+8 |
| Steam | **已发布**：维护者于 2026-10-30 确认上传及工坊页面更新完成；交付包为 `dist/steam-v0.3.0-c764757/Mwah/`，渠道完成依据为维护者确认 |
| 工坊文案 | [中英 BBCode](../steam-workshop-page.md)及[中文更新日志](../CHANGELOG.zh-CN.md)／[英文更新日志](../CHANGELOG.md)为交付源 |

## 发布与交付证据

- [PR #1](https://github.com/Coahuilite/Mwah/pull/1) 经双版本离线门及隐私审计后 squash 合入 main。tag `v0.3.0` 指向发布提交 **c764757**；发布前确认合并树与获准候选一致。main 的 0.2.2 squash 树与既有开发祖先完全相同，已补齐发布祖先关系，未改变已验收候选内容。
- main [CI](https://github.com/Coahuilite/Mwah/actions/runs/40929041123)及 [Release CI](https://github.com/Coahuilite/Mwah/actions/runs/40929074365)均成功；tag 版本／main 血统、双版本三渠道构建、全历史与实际 GitHub 包审计通过。
- 实际下载 [Mwah-v0.3.0.zip](https://github.com/Coahuilite/Mwah/releases/download/v0.3.0/Mwah-v0.3.0.zip)：710977 bytes，唯一 `Mwah/` 根、53 文件、两份本模组 DLL；与 Steam 包的文件清单一致。两渠道版本标签及两份 DLL 产品版本均为 0.3.0／c764757，框架及完整内容检查通过。
- 实际下载内容另经隐私审计；新预览图与已目视检查的源图逐字节一致。Steam 在同一发布提交上以自身 flavor 构建，零警告／零错误，实际包隐私审计通过。为保留旧候选，Steam 正式交付使用上表独立目录。
- 工坊 BBCode 与更新说明从唯一维护源导出到 Steam 交付目录旁的文本文件，位于 `Mwah/` 外，不混入游戏内容。维护者已确认完成线上上传及页面更新，发布交接待办关闭。
- 更新日志按 GitHub API 的实际公开时间登记，采用最新版本置顶；同步纠正与既有版本跟随门相反的旧排序说明。发布工作流明确 `target_commitish: main`，当前 Release 目标元数据也已对齐 main。tag 和已发布资产不回写。

[发布前准备](release-0.3.0-preparation-zh.md)保留历史候选及当时未覆盖项。源代码、原版 DLL、私人日志、凭据、工坊 ID 和未选用的美术草稿不进入分发包。
