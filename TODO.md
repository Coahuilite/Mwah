# TODO

## Current goal

- **0.2.1 发布流程**。实机矩阵已于 2026-10-05 由维护者全量观测通过（含墙单槽顶替与诊断四档；完整矩阵记录已压缩进 `OBLIVIONIS.md` 2026-10-05 节）。当前停在：发布整备文档（CHANGELOG×2、steam-workshop-page、README×2、日志文档状态头）待维护者审阅 → 审阅通过后提交 → tag / GitHub Release / 工坊上传逐项授权（流程唯一入口 `docs/release-runbook-zh.md`）。
- 仓库：`Coahuilite/Mwah`，开发分支 `0.2.x`（已上云，CI 绿），`0.1.x` 冻结归档（从未发布），`main` release-only。版本轴 0.2.1。

## Entry point for the next session

- 先读 `MEMORY.md` 耐久状态与 `docs/release-runbook-zh.md`；当前 hash 用 `git log --oneline -1`，本文不钉 hash。
- Agent 的下一步只在两种情况出现：维护者对发布整备文档给出审阅意见/授权；或发布后维护请求。
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
