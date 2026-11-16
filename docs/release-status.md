# 发布与渠道状态

本页是已采用发布决定与渠道结果的现行记录；发布操作方法在 release-runbook-zh.md。日期及证据不构成下一版本的发布授权。

## 0.4.0：GitHub 发布完成，Steam 待手动上传

维护者接受玩法与根布局候选，先授权上云集成及干净渠道构建，随后于 2026-11-17 授权正式 GitHub 发布与 changelog 更新。发布提交为 **ce2afac**，tag 为 **v0.4.0**；`main` 已设为 GitHub 主／默认分支。Steam 上传及工坊页面／标签操作明确由维护者手动完成。

| 渠道 | 已确认结果 | 交付与依据 |
| --- | --- | --- |
| GitHub | 正式 Release，非 draft／prerelease，设为 latest；公开时间 **2026-11-17 00:57:53 UTC+8** | [v0.4.0](https://github.com/Coahuilite/Mwah/releases/tag/v0.4.0)，目标 main，双语更新说明已应用 |
| Steam | 正式渠道包与中英页面稿／更新说明已交付，尚未确认上传及线上页面更新 | 固定交付目录 `dist/steam-v0.4.0-ce2afac/Mwah/`，`build=steam / commit=ce2afac`；维护者手动上传 |

[PR #3](https://github.com/Coahuilite/Mwah/pull/3)经检查 squash 合入 main；合并树与获准源树一致，旧文件删除及新内容迁移均保留。[main CI](https://github.com/Coahuilite/Mwah/actions/runs/43911572113)与 [Release CI](https://github.com/Coahuilite/Mwah/actions/runs/43912906488)全部步骤成功，包含版本／main 血统、双原生三渠道门、全历史隐私及实际发布包审计。

实际下载 [Mwah-v0.4.0.zip](https://github.com/Coahuilite/Mwah/releases/download/v0.4.0/Mwah-v0.4.0.zip)核对：**689607 bytes、35 文件、唯一 Mwah 顶层**，清单与 Steam 包一致。GitHub 标签为 `Mwah 0.4.0 / build=github / commit=ce2afac`；两版 DLL 的产品版本均为 `0.4.0+ce2afacc136c0b0ab89b5263a8d885bbf526523a`。下载副本保存在 `dist/released-v0.4.0/`，实际解包内容另经隐私审计。

Steam 包在同一发布提交上独立构建自己的 flavor，双原生编译零警告／错误，不复用 GitHub DLL。交付父目录 `dist/steam-v0.4.0-ce2afac/` 另附 `Workshop.zh-CN.txt`、`Workshop.en.txt`、`Change-notes.zh-CN.txt`、`Change-notes.en.txt` 及 GitHub 双语说明；这些文件在 `Mwah/` 外，不是上传的游戏内容。整个交付目录的隐私检查已通过。全历史门仅保留既已接受的两处历史 UNC 项，没有新增豁免或历史改写。

玩法、天意、识智和根布局的原生采用依据见 [候选确认](../openspec/changes/shared-content-and-lifecycle/native-acceptance.md)；本轮未增加游戏运行或渠道包独立实测声明。双语 changelog 按 GitHub 实际公开时间登记，README 与工坊稿状态同步；后续文档收尾不回写发布 tag 或资产。下一责任方为维护者手动完成 Steam 上传、页面与标签检查，确认后再更新本页。

## 0.3.0：双渠道发布完成

维护者于 2026-10-30 接受候选并授权发布；发布提交为 **c764757**，tag 为 **v0.3.0**。

| 渠道 | 已确认结果 | 依据 |
| --- | --- | --- |
| GitHub | 正式 Release，非 draft／prerelease；公开时间 2026-10-30 02:16:52 UTC+8 | [v0.3.0](https://github.com/Coahuilite/Mwah/releases/tag/v0.3.0) |
| Steam | 维护者于 2026-10-30 确认上传及页面更新完成 | 交付身份 `dist/steam-v0.3.0-c764757/Mwah/`，Steam flavor |

GitHub 资产为 [Mwah-v0.3.0.zip](https://github.com/Coahuilite/Mwah/releases/download/v0.3.0/Mwah-v0.3.0.zip)，记录大小 710977 bytes、53 文件、唯一 Mwah 顶层目录；两版 DLL 为 0.3.0／c764757。与 Steam 包文件清单一致，两渠道分别构建并检查实际内容和隐私，未混用 flavor。

发布通过 [PR #1](https://github.com/Coahuilite/Mwah/pull/1)合入 main；[main CI](https://github.com/Coahuilite/Mwah/actions/runs/40929041123)和 [Release CI](https://github.com/Coahuilite/Mwah/actions/runs/40929074365)成功。发布检查包含版本／main 血统、双原生三渠道离线门、全历史隐私和实际包审计。已发布 tag 与资产不回写。

原生采用依据为维护者对 1.6.4871 rev591 常规／倒地接收的确认、1.7.4990 rev1069 派发成功反馈，以及此前社交压缩和表演结束点确认。维护者据此接受整版并发布；未逐项记录的文化、UI、记忆变体和存读档不扩写为全部实测通过。

## 后续如何更新

发布完成后在本页补充版本、渠道、实际候选身份与完成依据；未知渠道保持未确认。更新双语 changelog 的实际发布时间和 TODO 路由。日常运行与维护所需的发布事实已在本页承接，不依赖旧发布档案；更早版本的变化保留在双语 changelog。
