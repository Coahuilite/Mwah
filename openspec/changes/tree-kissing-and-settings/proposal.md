# Proposal

## Why

维护者批准亲树、可选文化结果、扩展优先的设置页及保持共享玩法的 1.7 适配。它们的行为已采用到[唯一现行合同](../../../docs/specification.md)；本记录继续管理该既定范围的实现和验收，不再叠加一套现行 delta。

## What Changes

原授权涵盖共享运行恢复、物体扩展与亲树内容、文化结算、双页设置以及版本适配；未授权新增 Harmony、框架依赖、发布或代跑游戏。实现、剩余动作和验收见 [tasks](tasks.md)，技术责任及接缝见 [design](design.md)。

## Capabilities

### New Capabilities

相对现行合同无新增产品能力。

### Modified Capabilities

相对现行合同无新规则变化，因此当前不保留活动 delta。原先批准的 `kissing / addons / settings / compatibility` 规则及设置连接已并入现行页；1.7 支持目标没有因尚未交付而撤销。

## Impact

既定实施涉及请求、配对／单 Pawn driver、扩展 Def、天意／思想、本地化、设置和版本打包。此次文档迁移只调整合同归属与入口，不追加运行改动权限。今后若在收尾时真正改变保证，先提出明确 delta；纯恢复只记录问题、恢复路径和证据。

## Decisions

- 目标 1.7 参考 build 由维护者确认为 `1.7.4990 rev1069`。
- 维护者保留手动命令也检查冷却、仅 `noCooldowns` 豁免的既有政策，现归 KISS-04。
- 亲树默认／权重及文化规则先于实现确定，现归 ADD-07 至 ADD-12；两页方向归 SET-02、07。
- 文档迁移采用不代表游戏验收、1.7 打包或发布完成。原始授权和旧 delta 已保存在[历史快照](../../../docs/archive/specification-before-workflow/openspec/changes/tree-kissing-and-settings/proposal.md)。
