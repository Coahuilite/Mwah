# Design

## Context

合同从[现行入口](../../../docs/specification.md)读取；范围由 [proposal](proposal.md)负责。下述是 `deb427d` 候选的技术映射和取舍，不是另一份产品规格；实际结果及差距只在[实现证据](../../../docs/development/tree-kissing-and-settings.md)和 [tasks](tasks.md)记录。

原生参考来自本地 RimSage 的 `1.7.4990 rev1069` 完整保留快照，维护者确认使用该 build。配置安装回到 1.6 后自动刷新失败，不使该已确认快照变成 1.6 证据。版本观察同时参见[原版边界](../../../docs/vanilla-capability-boundaries.md)和[树文化研究](../../../docs/research/tree-kissing-ideology.md)。

## Decisions

### 直接组合，不新增总管或引擎接口

| 合同责任 | 实际入口与消费者 | 事实权威／寿命 |
| --- | --- | --- |
| 配对请求、入场与表演 | `KissUtility` → `JobDriver_Kiss` → `KissMoodReward` | 单场活动的匹配工作、角色和发起方时钟；driver 保存必要状态 |
| 非 Pawn 扩展 | `KissThingAddons`／各 worker → `JobDriver_KissThing` → `KissFate`／记忆实例 | 已解析 Def 元数据；目标类别／优先级属于描述，活动身份由保存的 JobDef 固定 |
| 设置编辑到运行采用 | `MwahSettingsView` 提交 → `MwahSettings` → 请求入口及适用结算消费者 | 规范值属于设置；缓冲、导航、滚动和展开属于视图，不写玩法配置 |
| 设置到磁盘 | `RequestSettingsSave` → 防抖写入；原版 `Dialog_ModSettings.PreClose` → `MwahMod.WriteSettings` | 内存代次／待保存状态由 mod 入口负责；关闭先提交，防抖写入不抢先提交活跃文本 |
| 会话派生状态 | `KissTicker` 的 GameComponent 生命周期重置冷却及点选／面板状态 | 游戏会话；不沿用上一游戏的静态 Pawn 身份 |

[请求与扩展源码](../../../Source/Mwah/Actions/Kiss/KissThingAddons.cs)、[配对 driver](../../../Source/Mwah/Jobs/JobDriver_Kiss.cs)、[物体 driver](../../../Source/Mwah/Jobs/JobDriver_KissThing.cs)、[设置视图](../../../Source/Mwah/MwahSettingsView.cs)、[规范设置](../../../Source/Mwah/MwahSettings.cs)与[mod 入口](../../../Source/Mwah/Mod.cs)是实际映射。连接页不要求对应的新连接类。

### 工作所有权与原版安全退出

`KissActivity` 统一识别配对和物体 driver，不依赖具体墙／树 worker。派发检查实际当前工作及 `loadID`，双方存活入场后绑定身份并登记冷却。旧存档只从相互指向且 `startTick` 相同的镜像工作重建对应 ID。

正常离开留在亲吻工作内；原静态回位队列已经移除。finalizer 同时检查 `JobCondition.Succeeded` 和越过最后回程 toil，不只检查表演完成。1.7.4990 的 `ReadyForNextToil` 先递增索引，`EndCurrentJob` 先求 finalizer、cleanup 后才安全启动；不在 cleanup 同步开始新工作。兜底仍受原版 opportunistic-job gate 限制；它不替代所有参与者的正常工作内回程。

`partnerJobId`、`finished`、`returnTicks` 保存匹配、结算幂等及有界回程。现行 toil 读档可能不重跑 initAction，因此每 tick 读取保存的 home，而非依赖初始化闭包。A/B/C 目标、角色、剩余时间、home 和旧旁白字段不重新解释。

### 扩展身份由内容描述，文化只影响结算

`MWAH_KissAddonDef` 描述身份、类别、默认、JobDef、范围及呈现键；worker 处理目标特殊性。初始化检查重复身份和文化规则优先级；实际目标的同优先级歧义在派发前诊断。共享 targeter 从描述获取 building／plant／item 标志，并关闭 `mapObjectTargetsMustBeAutoAttackable`，避免非攻击交互继承攻击专属限制。

墙继续使用 `MWAH_KissThing`，树使用 `MWAH_KissTree`；两者通过 ModExtension 绑定描述，复用单 Pawn driver，不在结算时按目标重新分类。共享文案增加 `TARGET`，保留 `WALL` 别名及 `MWAH_narrationKey / MWAH_narrationSubject`。

文化是树 worker 的 `ResolveFateScope` 策略，不是另一个抢目标的扩展。它在结算检查偏好、DLC、该 Pawn 的 Ideo 及可选 XML 规则；原生 precept 引用仅在 `MayRequire` 条件条目里。读取 `CompTreeConnection.ConnectedPawn` 识别连接，不调用 Prune、仪式或目击变更。结果范围及数值引用 ADD-07 至 ADD-12，不在 design 再维护一份分布。

### 设置的提交与保存分开

活动编辑保留提交回调和范围，切页／收起无需重画旧字段也能解析。Enter 在原生 TextField 消费事件之前捕获；无效和非有限文本不写规范值。视图对象由 mod 入口持有；关闭提交并重置首页，不对其他瞬态状态承诺持久化。

使用原版 `TabDrawer / TabRecord / TooltipHandler`；按字体和宽度测量标签、水印及行高，不引入 UI 框架。防抖写入与外部关闭写入分路；保存失败不清 dirty，沿用既有重试路径。

### 共享玩法，仅隔离真实版本差异

共享两种 driver 及业务规则，不散布 minor 判断或为迁移重建通用引擎层。编译引用、加载元数据、菜单／输入和真实 UI API 差异才是版本适配面。目标快照的 `GUI.tooltip` 已存在，旧 1.7.4976 缺成员发现不再作为当前迁移依据。程序集能否共用及 1.7 包如何加载仍由版本／原生证据决定，不能从共享源码推导交付完成。

## Verification boundary

这份映射只说明实际责任和来源判断，不声称用户交互或原生存读档已经成功。未完成项及补证责任只在 [tasks](tasks.md) 与[实现证据](../../../docs/development/tree-kissing-and-settings.md)维护。原始设计的前置缺陷调查保存在[历史版本](../../../docs/archive/specification-before-workflow/openspec/changes/tree-kissing-and-settings/design.md)，不作为当前缺陷清单。
