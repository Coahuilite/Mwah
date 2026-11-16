# 发布与渠道状态

本页是已采用发布决定与渠道结果的现行记录；发布操作方法在 release-runbook-zh.md。日期及证据不构成下一版本的发布授权。

## 0.4.0：未发布

维护者于 2026-11-17 授权检查、推送 `0.4.x`、以 PR squash 合入 `main`、将 `main` 作为主／默认分支，以及构建干净的双渠道包。此次不打 tag、不创建 GitHub Release、不上传工坊；合入 main 或出包不等于发布。

[PR #3](https://github.com/Coahuilite/Mwah/pull/3)是本次合入、CI 及最终交付结果的执行记录，以实际提交和 Actions 状态为准。GitHub 包入口为 `dist/github/Mwah-v0.4.0.zip`，Steam 为 `dist/steam/Mwah/`；正式渠道构建的准确提交／flavor 看各包 `version.txt`，不能把已实测的 dirty 开发包改标签充当正式包。历史候选身份及维护者覆盖由 [候选确认](../openspec/changes/shared-content-and-lifecycle/native-acceptance.md)负责；下一次正式发布仍需维护者决定，项目授权见 [TODO](../TODO.md)。

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
