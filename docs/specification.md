# Mwah! 规格与变更入口

状态：**现行开发合同入口**，2026-10-29 采用。维护责任：项目维护者。

这里指向唯一现行正文，不复制行为规则。现行合同已合并维护者此前批准的配对修复、亲树、文化策略、双页设置及 1.7 支持目标；不再要求把英文基线和旧 delta 拼成另一套正文。

**合同采用与交付分开。** 规格约定行为；实现证据由[开发记录](development/tree-kissing-and-settings.md)负责，维护者的 0.3.0 验收采用与渠道状态见[发布记录](release_review/release-0.3.0-review-zh.md)。

## 从责任找到正文

| 正文 | 负责的事实与结果 | 维护责任 |
| --- | --- | --- |
| [kissing](../openspec/specs/kissing/spec.md) | Pawn 配对入口、资格、冷却、占用、配对状态及奖励 | 配对亲吻维护者 |
| [addons](../openspec/specs/addons/spec.md) | 非 Pawn 目标、扩展身份、共享单 Pawn 执行、墙／树及可选文化 | 扩展维护者 |
| [settings](../openspec/specs/settings/spec.md) | 页面、编辑、单位、控件及诊断呈现 | 设置界面维护者 |
| [settings-use](../openspec/specs/settings-use/spec.md) | 提交到规范值、运行消费和持久化的既有连接 | 设置连接维护者 |
| [compatibility](../openspec/specs/compatibility/spec.md) | 原生支持条件、可选内容隔离、持久身份及交付边界 | 原生适配与交付维护者 |

这些是维护责任，不要求新建团队、总管、事件总线、加载器或 DLL。实际类、方法、状态寿命及原版 API 映射见[设计](../openspec/changes/tree-kissing-and-settings/design.md)。

```text
右键／导演台／自主撮合 → kissing
右键／导演台的非 Pawn 请求 → addons → 共用单 Pawn 执行
settings → settings-use → 请求准入／适用结算 + 磁盘持久化
上述能力 → compatibility 的原生环境与身份边界
```

## 维护时走最短完整路径

1. **先读现行合同。** 找到入口、输入含义、适用条件和真实消费者；不能只看实现返回值。
2. **在 proposal 写意图。** 区分恢复旧保证、补录有来源的旧合同和改变保证；小型文字修正不必凑齐一套制品。
3. **有语义变化才写 delta。** 新增、改变、撤销或改名要求，才使用 `ADDED / MODIFIED / REMOVED / RENAMED Requirements`。纯恢复可以零 delta，不为每次修复强造新要求。遗漏合同可注明既有来源后补录；不能把错误实现改写成正确预期。
4. **把实现与证据放对位置。** design 解释取舍、直接连接和责任；tasks 记录获准动作及完成依据。按项目允许的方法验证到实际消费层，复用仍适用的证据，不从场景自动生成测试任务。
5. **明确采用和结束。** 候选注明状态及所属变更，在明确采用前不替代现行正文；`MODIFIED` 保留完整要求块和全部旧场景。采用时更新现行正文、入口和直接读取方，并保存旧文档。归档对应工作不代表发布，未完成验收不能靠归档关闭。

社交收益压缩的追加采用记录见[本次 delta](../openspec/changes/social-impact-soft-cap/adoption.md)，实施与验收采用由其 [tasks](../openspec/changes/social-impact-soft-cap/tasks.md)记录。

原地、非爬行被动接收及持续参与条件见[已采用修订](../openspec/changes/mobility-and-kiss-completion/stationary-reception-adoption.md)，与原移动回退变更共用任务与证据记录。

移动回退及表演／返程边界的追加修订见 [delta](../openspec/changes/mobility-and-kiss-completion/adoption.md)，修复与验收见其 [tasks](../openspec/changes/mobility-and-kiss-completion/tasks.md)。

当前只有 `openspec/specs/` 下的正文定义现行规则；change 可以包含候选 delta，不能自称第二套 current。原变更的实施及验收采用见[任务记录](../openspec/changes/tree-kissing-and-settings/tasks.md)。

## 写作与工具

- 现行规格及活动变更正文使用中文；保留 `Purpose / Requirements / Requirement / Scenario`、`WHEN / THEN`，按需用 `GIVEN / AND` 和代码、路径标识。
- 硬保证使用 `SHALL` 或 `MUST`，每条要求至少有一个具体场景。场景是合同说明，不是已经执行的测试。
- `spec.md` 只写行为及适用条件；未实现、缺陷、已执行检查、待补证均放实现／证据材料，不混进规范正文。
- 使用普通 Markdown 手工维护。无需安装 OpenSpec、OpenSpec-cn 或集成插件，不以 CLI 通过作为采用条件；若另行采用工具，不为解析器删场景、改语义或制造 delta。现有构建、打包、隐私和 CI 门禁不因此改变。
- 代码实施、运行游戏、真实配置及发布仍受各自授权约束。禁止新增单元测试及等价换名用例；实机验证由维护者进行。纯文档迁移检查合同、链接、直接读取方和隐私，不为安心重跑未受影响的游戏构建。

## 历史与采用依据

方法依据为维护者提供的《RimWorld_Mod_Specification_Workflow_zh.md》。本页已包含本项目需要执行的流程，不依赖工作区外文件或上游工具才能使用。

- [被替代英文正文与工具元数据](archive/specification-before-workflow/README.md)：保留历史，不是活动任务归档。
- [本轮迁移与语义对账](../openspec/changes/archive/2026-10-29-specification-workflow/design.md)：记录原要求／场景去向及单一入口切换。
- [实施结果、有效证据和真实缺口](development/tree-kissing-and-settings.md)：不把源码／离线编译写成实机成功。
