# 0.3.0 发布前准备

更新日期：2026-10-30（UTC+8）。开发分支 `0.3.x`，产品版本 **0.3.0**；已发布基线仍为 0.2.2。本记录管理本地渠道候选和发布决定，不替代[发布流程](../release-runbook-zh.md)或现行规格。当前不合入 `main`、不打 tag、不建 Release、不上传／编辑工坊。

## 已确认的行为

维护者已确认社交压缩和表演结束点。2026-10-30 又确认常规及倒地接收亲吻正常；对应 **1.6.4871 rev591／dev／4c1ca99** 日志中的 6 轮配对均有双方结算，包含 5 次倒地等待接收方入场，原版记录到的 9 次亲吻工作退出均成功，旧被动方强制中断链未再出现。[配对记录](../development/mobility-and-kiss-completion.md#16-实机确认2026-10-30)负责详细覆盖边界。

这些结果可以继续复用：本轮只整理文档及渠道内容，不修改玩法、XML 或设置身份。`6dd0cc7` 的[开发 CI](https://github.com/Coahuilite/Mwah/actions/runs/40906774160)已通过双版本／三渠道离线门、全历史隐私及实际 dev 包审计。

## 渠道候选

本轮准备以下本地内容，不执行上传。各脚本必定重新构建对应 flavor；以包内 `version.txt` 和 DLL 产品版本确认实际打包提交。

| 消费方 | 本地交付 | 维护来源 |
| --- | --- | --- |
| GitHub Release | `dist/github/Mwah-v0.3.0.zip`，只含一个 `Mwah/` 根目录及双版本内容 | `scripts/pack-github.ps1 -Version v0.3.0`；正式 Release 仍由获准的 main／tag 流程重新构建 |
| Steam Workshop | `dist/steam/Mwah/`，不含工坊 ID | `scripts/pack-steam.ps1`；维护者获准后复制到自己的上传副本 |
| 工坊页面 | [中英 BBCode 草案](../steam-workshop-page.md) | 唯一页面文案来源，保留 1.7 待实机覆盖提示 |
| 更新说明与使用 | [英文变更](../CHANGELOG.md)、[中文变更](../CHANGELOG.zh-CN.md)、双语 README | 继续标为未发布，不预填发布时间 |

本地渠道产物的构建、身份和脱敏结果在实际完成后记录于此。

工坊草案同步澄清自主撮合：玩家管理的 Pawn 不作为自主发起方，征召中的玩家 Pawn 不作为接收方；删除了“完全不会碰殖民者”的过度表述，与既有 KISS-02 对齐，玩法未变。

## 发布前仍需的决定和证据

**建议保留双版本目标，先完成已有未覆盖验收，再授权正式发布。** 不把 1.6 单次会话推广为 1.7 或完整 DLC／存读档通过，也不临时缩减现行支持合同。

| 影响发布的未覆盖面 | 责任与继续条件 |
| --- | --- |
| 1.7.4990 rev1069、Core-only 启动和原生入口／状态 | 维护者按[原变更 4.2／4.3](../../openspec/changes/tree-kissing-and-settings/tasks.md)补实际结果；代理处理出现的合同偏差 |
| 基础树／文化关闭、不同文化及 Gauranlen，设置页实际操作，更新与存读档 | 维护者继续[原变更 4.4 至 4.7](../../openspec/changes/tree-kissing-and-settings/tasks.md)，不新建另一套验收清单 |
| 旧配对记忆刷新／好感变体及状态中断／返程替换 | 分别沿[压缩任务 5](../../openspec/changes/social-impact-soft-cap/tasks.md)和[移动任务 5.2](../../openspec/changes/mobility-and-kiss-completion/tasks.md)补余下覆盖 |
| 正式渠道与公开表面 | 维护者接受剩余结果并明确授权版本、渠道及页面文案后，才进入 main／tag／上传步骤 |

## 脱敏口径

仓库只保留版本、构建身份和去身份化结论；原始 Player.log、Pawn 名、坐标、存档、私人路径、凭据及工坊 ID 不进入 Git 或分发包。渠道包分别扫描实际内容；开发分支 push 由既有 hook 和 CI 检查可达历史。游戏安装、配置和存档不作为上云材料。
