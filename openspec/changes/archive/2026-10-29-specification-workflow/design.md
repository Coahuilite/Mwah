# Design

## Context

本轮为文档结构和采用流程迁移；产品语义不变，故没有 delta。方法依据为维护者提供的《RimWorld_Mod_Specification_Workflow_zh.md》。原状态来自 deb427d 上四份英文基线及已批准 delta；源码、Defs、设置、语言包、构建和 CI 不在改动范围。

## Decisions

- 保留 root openspec 下既有能力路径；以 docs/specification.md 作为唯一人类入口。已批准内容合并成中文现行开发合同，不再叠加旧基线／delta。结构标记及精确标识保留英文。
- settings-use 只是将原“内存立即权威／保存寿命”连接独立归位；设置能力保留编辑与呈现。两条连接要求来自旧 Value editing and persistence，不是新增玩法，也不要求新连接类。新增的具体场景只展开既有即时采用、失败保留与关闭刷新保证。
- 现行合同的 1.7 支持目标不以 About 仍为 1.6 为由缩减。入口标明开发合同与发布／实现的区别；尚未交付和待补证在实现证据与原 tasks 中记录，不混入 spec。
- 原变更的实施／验收任务继续开放。采用规范正文不表示结束实施；本轮仅归档文档迁移记录。旧正文与工具元数据移入 docs/archive/specification-before-workflow，保留历史标记及有效相对链接。
- 无需继续维护项目 CLI 配置或安装 OpenSpec-cn。旧 config 和机器元数据保留于历史；已装 CLI 不卸载、不升级。CONTRIBUTING／AGENTS／README／MEMORY／TODO 及研究资料一同切换入口；现有构建和 CI 不变。

## Requirement and scenario reconciliation

对账同时检查旧基线与批准 delta，不把整块文本的机械替换当作语义裁定。原设置页的“可滚动单列”和滚动时身份“可读”并未被双页方向撤销；它们在旧 delta 正文里被省略，本次据旧 Settings organization 补回 SET-02，而不从现实现反推新预期。其余规则按既定变化采用，未改变的入口、前提、负面要求及失败场景保留。

旧有效合同共 31 个要求、67 个场景；现行 33 个要求、70 个场景。计数只用于清点，语义以逐条对照为准；新增连接场景不代表已执行测试。原单页设置被先前已批准双页要求替换，不恢复旧布局；数值、单位、默认及合法调用条件不随这次结构迁移改变。

| 原能力／要求 | 现行要求 | 归属说明 |
| --- | --- | --- |
| kissing: Independent initiation paths | KISS-01 相互独立的发起入口 | 全部场景保留于对应要求 |
| kissing: Ambient selection boundaries | KISS-02 自主撮合边界 | 全部场景保留于对应要求 |
| kissing: Participation and scope | KISS-03 参与资格与配对范围 | 全部场景保留于对应要求 |
| kissing: Cooldown and command policy | KISS-04 冷却与命令策略 | 全部场景保留于对应要求 |
| kissing: Paired performance and departure | KISS-05 配对表演与离开所有权 | 全部场景保留于对应要求 |
| kissing: Relationship-based settlement | KISS-06 配对奖励 | 全部场景保留于对应要求 |
| kissing: Relationship slots and narration | KISS-07 关系槽与实例旁白 | 全部场景保留于对应要求 |
| kissing: Truthful dispatch outcome | KISS-08 派发结果真实 | 全部场景保留于对应要求 |
| kissing: Cross-target occupancy | KISS-09 跨目标占用一致 | 全部场景保留于对应要求 |
| addons: Independent target controls | ADD-01 独立目标控制 | 全部场景保留于对应要求 |
| addons: Shared single-actor execution | ADD-02 共享单 Pawn 执行 | 全部场景保留于对应要求 |
| addons: Wall target and outcomes | ADD-03 亲墙资格与结果 | 全部场景保留于对应要求 |
| addons: Editable fate content | ADD-04 可编辑结果与无心情通道 | 全部场景保留于对应要求 |
| addons: Stable addon resolution | ADD-05 稳定扩展解析 | 全部场景保留于对应要求 |
| addons: Basic tree kissing | ADD-06 基础亲树资格 | 全部场景保留于对应要求 |
| addons: Tree defaults and identity | ADD-07 亲树默认及身份 | 全部场景保留于对应要求 |
| addons: Tree outcome distributions | ADD-08 亲树分布 | 全部场景保留于对应要求 |
| addons: Tree relationship memory | ADD-09 树关系槽 | 全部场景保留于对应要求 |
| addons: Optional cultural tree settlement | ADD-10 可选文化采用 | 全部场景保留于对应要求 |
| addons: Cultural outcome selection | ADD-11 文化结果优先级 | 全部场景保留于对应要求 |
| addons: Cultural integration preserves native tree systems | ADD-12 不替代原版树系统 | 全部场景保留于对应要求 |
| settings: Settings coverage | SET-01 设置覆盖与独立调节 | 全部场景保留于对应要求 |
| settings: Settings organization | SET-02 扩展优先的双页导航 | 全部场景保留于对应要求 |
| settings: Value editing and persistence | SET-03 数值编辑与隐藏前提交 | 编辑场景留 SET-03，防抖场景及持久化保证归 USE-01／02 |
| settings: Units and configuration identity | SET-04 单位及恢复默认 | 全部场景保留于对应要求 |
| settings: Diagnostics and localization | SET-05 诊断档位与本地化 | 全部场景保留于对应要求 |
| settings: Diagnostic channels | SET-06 诊断通道 | 全部场景保留于对应要求 |
| settings: Contextual addon management | SET-07 可展开的扩展管理 | 全部场景保留于对应要求 |
| compatibility: Supported environment | NATIVE-01 环境与可选内容隔离 | 全部场景保留于对应要求 |
| compatibility: Persistent and package identity | NATIVE-02 持久与包身份 | 全部场景保留于对应要求 |
| compatibility: Safe delivery and verification | NATIVE-03 安全交付与证据边界 | 全部场景保留于对应要求 |

### Scenario destinations

#### kissing / Independent initiation paths

- A directed non-orderable initiator → 指定不可由玩家下令的发起方
- Ambient matchmaking is independent → 自主撮合与手动命令独立

#### kissing / Ambient selection boundaries

- A nearby colony animal → 殖民地动物在附近

#### kissing / Participation and scope

- An awake downed receiver → 清醒但倒地的接收方
- A moodless participant → 无心情的参与者

#### kissing / Cooldown and command policy

- A repeated manual request → 冷却中的重复手动请求
- A busy participant with exemption enabled → 冷却豁免不解除占用
- Ambient cooldown exemption → 自主预筛采用冷却豁免
- An unreachable receiver with exemption enabled → 冷却豁免下仍不可达

#### kissing / Paired performance and departure

- Violence during the kiss job → 亲吻工作中受暴力伤害
- Interrupted departure releases ownership → 离开被中断后释放所有权
- Stage selection and fallback → 台位与贴近回退
- Save and load during a valid kiss → 有效亲吻中的存读档
- One participant arrives first → 一方提前到达
- The partner is replaced before completion → 完成前搭档被替换
- Normal return is still moving → 正常回程仍在移动
- A player gives a new order → 玩家已给出新命令

#### kissing / Relationship-based settlement

- Default mood-only settlement → 默认只结算心情

#### kissing / Relationship slots and narration

- A one-row narration table → 旁白表仅有一行

#### kissing / Truthful dispatch outcome

- Reservation prevents admission → 预订阻止入场

#### kissing / Cross-target occupancy

- Pairing a Pawn that is kissing a wall → 配对目标正在亲墙
- Pairing a Pawn that is kissing a tree → 配对目标正在亲树

#### addons / Independent target controls

- Disabling an addon while a menu is open → 菜单打开后关闭扩展
- A target is unavailable → 目标不可用
- An addon is disabled during an admitted job → 已入场工作中关闭扩展

#### addons / Shared single-actor execution

- A target disappears during performance → 表演中目标消失
- A vanilla-orderable mutant initiator → 原版可下令的 mutant 发起方

#### addons / Wall target and outcomes

- A new wall outcome replaces an old stage → 新墙结果替换旧档
- Default wall fate content → 默认墙结果分布

#### addons / Editable fate content

- A moodless Pawn kisses a wall → 无心情 Pawn 亲墙

#### addons / Stable addon resolution

- Two addons accept a target → 两个扩展接受同一目标
- A registered plant target category → 已注册植物目标类别
- A kissable object is not an attack target → 可亲对象不是攻击目标

#### addons / Basic tree kissing

- Core-only tree request → Core-only 请求亲树
- Tree kissing is disabled → 亲树关闭

#### addons / Tree defaults and identity

- New tree preferences → 配置尚无亲树条目

#### addons / Tree outcome distributions

- Cultural likelihood without a higher reward ceiling → 文化提高正面机会而非奖励上限
- Empty cultural table → 文化结果表为空

#### addons / Tree relationship memory

- Switching from cultural to basic settlement → 从文化转为基础结果

#### addons / Optional cultural tree settlement

- Integration is deliberately disabled → 主动关闭文化
- Different cultures in one colony → 同殖民地不同文化
- Culture changes before settlement → 完成前文化偏好改变

#### addons / Cultural outcome selection

- Matching both a meme and its precept → 同时匹配 meme 和其 precept
- Both cultural predicates match → 两个文化条件同时匹配
- Another Pawn's Gauranlen → 别人的或未连接的 Gauranlen

#### addons / Cultural integration preserves native tree systems

- Kissing a connected Gauranlen tree → 亲吻自己的已连接 Gauranlen

#### settings / Settings coverage

- Adjusting an addon independently → 独立修改扩展时长

#### settings / Settings organization

- Scrolling to addon controls → 滚到扩展控制
- Opening settings to manage extensions → 打开设置管理扩展
- Navigating between pages → 页面间独立滚动
- Finding common controls and detailed parameters → 寻找常用控制与详细参数

#### settings / Value editing and persistence

- Invalid numeric text → 无效数值文本
- A valid edit is followed by a tab change → 输入后立即切页
- An addon editor is collapsed or settings close → 收起扩展或关闭设置
- Non-finite numeric input → 非有限数值
- Enter commits the active numeric field → Enter 提交活跃字段
- Debounced persistence while editing → 编辑时发生防抖保存

#### settings / Units and configuration identity

- Restoring addon defaults → 恢复扩展默认

#### settings / Diagnostics and localization

- Diagnostics are disabled → 关闭诊断

#### settings / Diagnostic channels

- Verbose diagnostics are enabled → 开启 Verbose

#### settings / Contextual addon management

- Ideology is unavailable → 未启用 Ideology
- Temporarily disabling tree kissing → 暂时关闭亲树

#### compatibility / Supported environment

- Core-only installation → Core-only 加载
- The same gameplay request on supported minors → 两个目标 minor 的等价请求

#### compatibility / Persistent and package identity

- Loading existing configuration and memories → 保留配置及记忆更新模组

#### compatibility / Safe delivery and verification

- An offline package build → 离线打包
- Runtime owners are refactored → 重构内部运行责任方

## Verification and handoff

从 README／CONTRIBUTING／AGENTS 的实际入口核对中文现行页；核对全部旧场景和有影响的默认、负面要求、状态转移及可选条件；检查 Markdown 链接、旧权威引用、文档范围及隐私。不开新验证框架，不跑未受影响的构建或游戏。实际检查结果见 tasks；先前运行证据仍由 docs/development/tree-kissing-and-settings.md 负责。
