# TODO

## Current goal

- **0.2.2 已发布**（2026-10-05/06）：工坊首发（维护者本人上传，steam flavor）+ GitHub Release `v0.2.2`（main 发布提交 `32a9d94`，Release CI 绿，资产 `Mwah-v0.2.2.zip`）。当前进入发布后维护态。
- 仓库：`Coahuilite/Mwah`，开发分支 `0.2.x`（已上云，CI 绿），`0.1.x` 冻结归档（从未发布），`main` release-only（现与 0.2.2 同步）。版本轴 0.2.2。

## Entry point for the next session

- 先读 `MEMORY.md` 耐久状态与 `docs/release-runbook-zh.md`；当前 hash 用 `git log --oneline -1`，本文不钉 hash。
- Agent 的下一步只在两种情况出现：维护者给出 bug 报告/功能请求的处理授权；或下一个版本的发布整备指令。工坊页面维护与上传永远是维护者本人。
- 已知改进候选（未排期）：Release CI 目前用 GitHub 自动生成的 "Full Changelog" 链接作为 Release 正文，未注入 `docs/CHANGELOG.md` 对应条目；若要注入，改 release.yml 的 body 生成步骤。
- 工坊页面文案唯一维护源 = `docs/steam-workshop-page.md`（中英 BBCode + 编辑约定）；版本历史 = `docs/CHANGELOG*.md`（发布时把 Unreleased 头替换为 UTC+8 时间）。

## Regression checklist（供未来发布复用；0.2.1 全量矩阵见 OBLIVIONIS）

- [ ] 启动面：banner 身份正确（dev=`版本+完整sha`，发行=裸版本号）、双语齐备、天意表解析自证（无 `is not a Def type`）、无红字。
- [ ] 核心流：右键 / 导演台两段点选 / 快速发配 / 心形半填；双人 `stage→begin×2→perform×2→return×2` 全链无 abort。
- [ ] 墙与槽：五档短讯 + 逐实例旁白；"与墙的关系"任意时刻仅一条、新签就地顶替；天意表文件编辑回路（改权重/删行/删全表回落）。
- [ ] 无心情面：动物/机械族/无人机/实体照亲、消息通道各归各表、心情零入账；实体可当发起方（导演台左槽）。
- [ ] 停火：敌对者 job 全程免战、回程结束下一 tick 恢复敌对、一击即碎并反击；战争女皇充能炮/产虫双静默。
- [ ] 门禁：七档逐档恰好掉一类；灰项真因；addon 关闭 = 全可选面静默消失零提示。
- [ ] 心情数值：+5 × 自己 SocialImpact × 倍率；精神变态归零（面板无条目是读取时归零，不是 bug）；同对象刷新单条、跨对象独立无封顶。
- [ ] 设置页：固定标题区（副标题+水印不裁切）、四段式、addon 组生长、tick 三读法、数量框量化回弹、诊断四档（Auto=渠道）、恢复默认清空双字典。
- [ ] 存档卫生：job 中途存/读干净；关模残留行为已知且记录；跨局静态不泄漏。

## 明确延后 / 不做

- [ ] 不做：世界地图/商队途中右键；RimWorld Multiplayer 同步；真·贴合亲吻姿势（需自定义 `PawnRenderNodeWorker` + 美术，非 API 限制）。
- [ ] 不做：Downed 者当右键发起方（需 Harmony 放开原版门，与零 Harmony 冲突；导演台代发不受此限）。
