# TODO

## Current goal

- **发布后维护态**（0.2.2 已双渠道发布，证据见 `docs/release_review/release-0.2.2-review-zh.md`）。无进行中开发任务；下一条指令来自维护者（bug 报告 / 功能请求 / 下一版本整备）。

## Entry point for the next session

- 先读 `MEMORY.md` 耐久状态；动某子系统前读 `docs/design-decisions.md` 对应节；发布相关只认 `docs/release-runbook-zh.md`。当前 hash 用 `git log --oneline -1`，本文不钉 hash。
- 工坊页面文案唯一维护源 = `docs/steam-workshop-page.md`；版本历史 = `docs/CHANGELOG*.md`（发布时把 Unreleased 头替换为 UTC+8 时间）。工坊上传与页面维护永远是维护者本人。
- 新 clone 先跑 `pwsh scripts/install-hooks.ps1`（`core.hooksPath` 是本机设置，不入库；不装则 hook 缺席，CI 兜底仍在）。

## Regression checklist（下一轮发布模板；0.2.2 已按此矩阵于 2026-10-05 以 dev 包全量验收，记录见 OBLIVIONIS 与 release review）

- [ ] 启动面：banner 身份正确（dev=`版本+完整sha`，发行=裸版本号）、双语齐备、天意表解析自证（无 `is not a Def type`）、无红字。
- [ ] 核心流：右键 / 导演台两段点选 / 快速发配 / 心形半填；双人 `stage→begin×2→perform×2→return×2` 全链无 abort。
- [ ] 墙与槽：五档短讯 + 逐实例旁白；"与墙的关系"任意时刻仅一条、新签就地顶替；天意表文件编辑回路（改权重/删行/删全表回落）。
- [ ] 无心情面：动物/机械族/无人机/实体照亲、消息通道各归各表、心情零入账；同表双结算不放回（镜像复读即回归）。
- [ ] 停火：敌对者 job 全程免战、回程结束下一 tick 恢复敌对、一击即碎并反击。
- [ ] 门禁：七档逐档恰好掉一类；灰项真因；addon 关闭 = 全可选面静默消失零提示。
- [ ] 心情数值：+5 × 自己 SocialImpact × 倍率；精神变态归零（读取时归零，面板无条目不是 bug）；同对象刷新单条、跨对象独立无封顶。
- [ ] 设置页：固定标题区不裁切、四段式、addon 组生长、tick 三读法、数量框量化回弹、诊断四档（Auto=渠道）、恢复默认清空双字典。
- [ ] 存档卫生：job 中途存/读干净；关模残留行为已知且记录；跨局静态不泄漏。

（明确不做的清单不在本文——永久立场只活在 `AGENTS.md` 硬边界与 `MEMORY.md` 宪法行，TODO 不复制。）
