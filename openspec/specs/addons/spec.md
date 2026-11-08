# 非 Pawn 亲吻扩展

状态：现行开发合同。维护责任：物体吻与内容扩展维护者。
适用条件：已加载的游戏地图、单机；文化扩展按下述可选条件采用。

## Purpose

约定物体目标资格、独立开关、共享单 Pawn 执行及内容结果。扩展通过窄合同组合到亲吻功能，不使配对吻依赖具体墙／树实现。

2026-11-05 采用机器吻 ADD-13～18，依据见 [采用记录](../../changes/machine-kissing/adoption.md)；实施与验收状态由该变更的 tasks 管理。

## Requirements

### Requirement: ADD-01 独立目标控制
每个扩展 SHALL 有稳定身份、独立启用偏好及思想时长；实际启用状态受总开关约束。右键、导演台槽和快速点选 SHALL 支持扩展，自主撮合不支持。配对范围 SHALL NOT 限制非 Pawn 目标。

#### Scenario: 菜单打开后关闭扩展
- **WHEN** 菜单项生成后关闭扩展，再点击旧菜单项
- **THEN** 不派工作、不显示拒绝通知，也不生成灰色替代项。

#### Scenario: 目标不可用
- **WHEN** 目标对应扩展关闭，或目标被毁、离开地图、处于迷雾
- **THEN** 目标从相应点选面消失；关闭或目标失效后的旧派发静默结束。

#### Scenario: 已入场工作中关闭扩展
- **WHEN** 扩展工作已经入场后关闭扩展
- **THEN** 不接纳新请求，但原工作可以正常完成。

### Requirement: ADD-02 共享单 Pawn 执行
物体吻 SHALL 使用 [KISS-03](../kissing/spec.md) 的 Pawn 参与／发起资格、KISS-04 的冷却、KISS-05 的受伤中断／回程所有权及 KISS-09 的占用规则，并共享朝向和爱心效果；目标范围由 ADD-01 负责。只有 Pawn 运行工作；目标 SHALL 在接近／表演中被预订以阻止并发亲吻，并选择附近可达站位；表演结束 SHALL 释放该工作持有的目标／台位预订，返程不继续锁定物体目标，报告及占用遵循 KISS-05／09。完成 SHALL 只结算一次，未完成取消不发扩展奖励。

#### Scenario: 表演中目标消失
- **WHEN** 非 Pawn 目标在表演完成前被毁
- **THEN** 取消工作，不发完成结果。

#### Scenario: 原版可下令的 mutant 发起方
- **WHEN** 可下令的 mutant Pawn 通过共同参与和发起资格
- **THEN** 物体吻采用与配对吻相同的发起方策略，不另加 mutant 白名单。

### Requirement: ADD-03 亲墙资格与结果
亲墙 SHALL 识别游戏标记的墙、天然岩石及合格可采矿岩石；稳定设置身份为 `wall`，默认开启。墙关系 SHALL 只有一个五档槽；不乘 SocialImpact，不添加配对吻的 psychopath 抵消。默认及空表回退 SHALL 对心情值 -2、0、1、3、6 使用 20、30、25、17、8 的百分比。

#### Scenario: 新墙结果替换旧档
- **WHEN** 已有墙记忆的 Pawn 完成另一次亲墙
- **THEN** 原槽替换档位、旁白并续用配置时长，不添加第二个墙槽。

#### Scenario: 默认墙结果分布
- **WHEN** 默认表或空表回退选择墙结果
- **THEN** 使用上述五档心情值和概率。

### Requirement: ADD-04 可编辑结果与无心情通道
扩展结果 SHALL 使用带权 XML 天意内容，提供本地化旁白及可选短讯。缺少心情 SHALL 只抑制思想发放，不抑制行动或适用短讯。扩展思想组 SHALL 独立于配对吻及其他目标类别。

#### Scenario: 无心情 Pawn 亲墙
- **WHEN** 无心情 Pawn 完成已经入场的亲墙
- **THEN** 可发送适用短讯，不创建思想或其他替代资源奖励。

### Requirement: ADD-05 稳定扩展解析
扩展注册 SHALL 使用稳定身份和确定目标优先级，诊断重复身份或同优先级歧义。工作 SHALL 跨存读档保留选定扩展身份，不在结算时重新分类目标。共享菜单和点选 SHALL 支持注册目标类别，不硬编码墙文案；新增目标族 SHALL NOT 改动配对吻行为。

#### Scenario: 两个扩展接受同一目标
- **WHEN** 注册目标规则重叠
- **THEN** 按明确优先级选择，或在派发前诊断同优先级歧义，不依赖偶然注册顺序。

#### Scenario: 已注册植物目标类别
- **WHEN** 启用的亲树扩展声明植物目标
- **THEN** 右键、导演台槽和快速点选通过扩展元数据选择合格树木，不修改配对吻。

#### Scenario: 可亲对象不是攻击目标
- **WHEN** 扩展接受未标记为可自动攻击地图物体的目标
- **THEN** 导演台不对亲吻施加攻击专属目标过滤。

### Requirement: ADD-06 基础亲树资格
亲树 SHALL 接受已经生成、未被毁且由游戏标记为树的 Plant，包括合格已加载内容；独立控制启用及思想时长，并复用共享物体吻。没有 Ideology 时 SHALL 仍能行动、显示旁白及结算适用心情。SHALL NOT 另加成熟度、文化、阵营或树种门槛。

#### Scenario: Core-only 请求亲树
- **WHEN** 未启用 Ideology 时请求合格树目标
- **THEN** 可完成基础亲树，不产生可选内容依赖错误。

#### Scenario: 亲树关闭
- **WHEN** 亲树扩展开关关闭
- **THEN** 树从地图点选消失，旧派发静默失效；设置入口仍可用于重新开启。

### Requirement: ADD-07 亲树默认及身份
亲树 SHALL 默认开启，思想时长为 30000 ticks，文化偏好默认开启但仅在适用时生效。身份 SHALL 保持：扩展偏好 `tree`、文化偏好 `treeCulturalIntegration`、工作 `MWAH_KissTree`、记忆 `MWAH_KissedTree`。新偏好 SHALL NOT 改写墙偏好。

#### Scenario: 配置尚无亲树条目
- **WHEN** 既有配置没有亲树项
- **THEN** 基础亲树以开启和 30000-tick 记忆为默认，仅匹配 DLC 与 Pawn 条件时采用文化策略。

### Requirement: ADD-08 亲树分布
亲树 SHALL 共用心情值 -2、0、1、3、6 五档，不乘 SocialImpact，不加独立 psychopath 抵消。默认相对权重 SHALL 使用下表；所选表没有正权重行时 SHALL 以同策略分布回退到同一树槽。

| 策略 | -2 | 0 | 1 | 3 | 6 |
| --- | --- | --- | --- | --- | --- |
| 基础 | 10 | 30 | 35 | 20 | 5 |
| 爱树文化 | 3 | 17 | 35 | 30 | 15 |
| 自己的已连接 Gauranlen | 0 | 10 | 25 | 40 | 25 |

#### Scenario: 文化提高正面机会而非奖励上限
- **WHEN** 使用默认基础、爱树、自己的连接策略
- **THEN** 正面档概率分别为 60%、80%、90%，最高心情值均为 6。

#### Scenario: 文化结果表为空
- **WHEN** 选定文化表没有正权重行
- **THEN** 使用该文化策略的规定权重，不使用墙分布或额外奖励。

### Requirement: ADD-09 树关系槽
基础和文化结果 SHALL 共用一个树关系槽，与墙槽独立；后续亲树 SHALL 替换该槽档位、旁白及持续时间，不新增另一树槽。

#### Scenario: 从文化转为基础结果
- **WHEN** 已有文化树记忆的 Pawn 完成基础亲树
- **THEN** 用基础结果更新同一槽，不叠加基础／文化两条记忆。

### Requirement: ADD-10 可选文化采用
亲树 SHALL 提供独立文化偏好。DLC 缺失、主动关闭或 Pawn 无匹配文化时 SHALL 选基础结果，不阻断行动。匹配 SHALL 使用发起 Pawn 自己的文化，并在结算时采用。

#### Scenario: 主动关闭文化
- **WHEN** Ideology 已启用，玩家关闭文化但保留亲树
- **THEN** 继续基础亲树。

#### Scenario: 同殖民地不同文化
- **WHEN** 两个不同文化的 Pawn 亲同一棵树
- **THEN** 各按自身文化结算，不使用殖民地主文化代替。

#### Scenario: 完成前文化偏好改变
- **WHEN** 已入场亲树在文化被关闭后完成
- **THEN** 按基础策略结算一次，不删除既有记忆。

### Requirement: ADD-11 文化结果优先级
`Trees_Desired` SHALL 选择爱树旁白及比基础更高的正面机会。自己的 Gauranlen 特殊策略 SHALL 同时要求 `GauranlenConnection_Strong` 和目标实际连接到该 Pawn，并优先于一般爱树策略。重叠 meme／precept SHALL 只选一个策略；反对砍树本身 SHALL NOT 决定亲吻价值。

#### Scenario: 同时匹配 meme 和其 precept
- **WHEN** Pawn 同时匹配爱树 precept 及关联 meme
- **THEN** 只选一个文化策略，不重复发同一文化收益。

#### Scenario: 两个文化条件同时匹配
- **WHEN** Pawn 有强连接 precept、目标连接到它，且也有 `Trees_Desired`
- **THEN** 只采用自己的连接策略。

#### Scenario: 别人的或未连接的 Gauranlen
- **WHEN** 目标未连接或连接到其他 Pawn
- **THEN** 依发起方落入一般爱树或基础策略，不因此拒绝亲吻。

### Requirement: ADD-12 不替代原版树系统
文化结算 SHALL NOT 伪造树木目击、把环境思想发成记忆、修剪或连接树、改变连接强度或产出、冒充仪式／种植事件。无心情 Pawn SHALL NOT 获得替代资源。关闭整合 SHALL 保留原版环境文化效果。

#### Scenario: 亲吻自己的已连接 Gauranlen
- **WHEN** 文化匹配的 Pawn 完成该亲树
- **THEN** 可获得树吻结果，但不改变连接强度、树精产出或仪式进度。

### Requirement: ADD-13 机器目标与单一身份
机器吻 SHALL 按 `Building_WorkTable` 类型族接受目标，并精确接受原版 `SimpleResearchBench`、`HiTechResearchBench`、`MultiAnalyzer`、`NutrientPasteDispenser` 四个 ThingDef，并按 ADD-19 接受明确声明的可选兼容目标。特例 SHALL 先于一般类型族选择策略，不按名称、生产分类或其他模组的相似功能兜底匹配。稳定身份 SHALL 为设置 `machine`、工作 `MWAH_KissMachine`、记忆 `MWAH_KissedMachine`；默认开启、思想时长 30000 ticks。普通、研究、营养膏策略 SHALL 共用一个机器记忆槽，与墙／树独立，替换时同步档位、旁白、时长和倍率。

#### Scenario: 模组工作台与非工作台机器
- **WHEN** 模组目标继承 `Building_WorkTable`，或只是名称相似但不属于该族
- **THEN** 前者进入普通机器吻，后者不被认领，除非是上述精确特例或 ADD-19 明确声明的可选目标。

#### Scenario: 从研究设备转吻普通工作台
- **WHEN** Pawn 已有识智加成的机器记忆，再完成普通工作台吻
- **THEN** 更新同一个槽，并将倍率恢复为普通机器的值，不保留研究增益。

### Requirement: ADD-14 机器供电
具有 `CompPowerTrader` 的机器 SHALL 仅以其 `PowerOn` 判断供电资格，不追查电源、接线、开关、故障或获得开启状态的方式，不附加完整生产可用条件。没有该组件的手动工作台及简易研究台 SHALL 不要求电力。接近／表演中失去 PowerOn SHALL 取消未完成动作，不发完成奖励；完成后的返程 SHALL 不撤销结果。供电不足 SHALL 给出直接拒绝原因，不隐藏仍启用的目标；关闭扩展的静默规则不变。

#### Scenario: 特殊方式保持通电
- **WHEN** 设备 PowerOn 为真，即使其他生产条件不满足或由其他模组／开发工具改变该状态
- **THEN** 不因电源来源、故障、开关、燃料、原料或账单条件另行拒绝。

#### Scenario: 中途断电
- **WHEN** 已入场的电动机器吻在表演完成前 PowerOn 变为假
- **THEN** 取消，不发机器思想或饱腹奖励，释放本工作预订。

### Requirement: ADD-15 机器站位
营养膏机 SHALL 优先使用原版领取交互格，其次尝试相对机器朝向的左／右各一格；两侧同等可用时按 Pawn 距离选择，等距保持确定顺序。三格都不可达／不可预订 SHALL 取消，不扩大到其他侧面。工作台及研究台 SHALL 使用其原版 InteractionCell，与该格的椅子重合，不要求必须摆椅子；无声明交互格的工作台采用原版计算的交互格。多元分析仪 SHALL 使用附近可达、满足原版 Touch 条件的格子。机器工作 SHALL 固定保存选定台位，并使预订、行走和表演使用同一格；台位失效 SHALL 取消。此规则 SHALL NOT 改写墙／树原有站位条件。

#### Scenario: 营养膏机正面不可达
- **WHEN** 中央领取格不可用，但旋转后的某个左右候选可达且可预订
- **THEN** 使用该侧格；三格均不可用时不派工作。

#### Scenario: 研究台前摆有椅子
- **WHEN** 交互格上有可用椅子
- **THEN** Pawn 在同格完成表演，不改到旁边；正在被占用或预订则拒绝。

### Requirement: ADD-16 机器天意与识智
机器吻 SHALL 使用基础心情 -2、0、1、3、6，默认权重 20、30、25、17、8；基础、研究、营养膏各用自己的旁白表，但共用机器记忆。研究特例 SHALL 仅将正面心情乘以 `1 + clamp(L, 0, 20) / 20`；L 为结算时原版 Intellectual 有效等级（含资质），技能缺失或不可用按 0。负面及零档 SHALL 不缩放，普通机器与营养膏 SHALL 不采用识智增益。不乘 SocialImpact 或配对心情倍率，不授予研究进度或识智经验。档位权重 SHALL 不受识智影响；空表 SHALL 按同分布及相同效果规则回退。

#### Scenario: 识智满级
- **WHEN** 有心情的 Pawn 在识智有效等级 20 或以上完成研究设备吻
- **THEN** 五档实际心情为 -2、0、2、6、12；0 级为 -2、0、1、3、6。

#### Scenario: 高低识智切换
- **WHEN** 再次亲吻时识智改变
- **THEN** 新结算按当前值覆盖同槽倍率；旧记忆不随之后技能变化自行改变。

### Requirement: ADD-17 显式好结果与营养膏饱腹
机器天意行 SHALL 用显式 `goodOutcome` 标记好结果，不从 stageIndex、心情值、思想是否存在或 Pawn 是否有心情推断。出厂各表仅最后两档标记好结果。原版营养膏机 SHALL 显示法式深吻动作；完成并抽到标记行时 SHALL 将存在的 Food 需求填至当前上限，不要求或消耗原料、不生成食物、不走进食链，不替代其他需求或直接移除营养不良。无心情 SHALL 不阻止饱腹；无 Food SHALL 不补充电量等资源，且不显示已饱腹提示。思想／短讯／饱腹 SHALL 消费同一次抽签；未完成不发奖，完成最多一次，返程／存读档不重放。缺心情提示 SHALL 不将仍可饱腹的动作称为只有姿态。

#### Scenario: 无心情但有 Food
- **WHEN** Pawn 深吻营养膏机并抽到标记行
- **THEN** 不发思想，但补满 Food 并显示适用饱腹提示。

#### Scenario: 心情被调成零
- **WHEN** 标记行的心情因内容修改或原版机制归零
- **THEN** 仍按显式标记处理饱腹，不重新掷骰。

#### Scenario: 已完成后保存再加载
- **WHEN** 深吻已结算、Pawn 在返程中存读档
- **THEN** 不再次抽签、刷新思想或补充 Food。

### Requirement: ADD-18 双版本与既有行为
机器吻 SHALL 在 RimWorld 1.6／1.7 Core-only 环境使用同一规则，复用现有右键、导演台、冷却、受伤中断、扩展开关及返程边界，不加入自主撮合。新增设置／Job／记忆 SHALL 不重用墙／树身份，不改变旧存档中既有工作的含义。SHALL 不引入 Harmony、运行库依赖或对原版生产／研究／进食流程的补丁。

#### Scenario: 旧存档缺少机器设置
- **WHEN** 旧配置及存档升级
- **THEN** 机器采用默认值，墙／树及配对偏好与有效记忆保留含义，旧物体吻不要求新增台位字段。

### Requirement: ADD-19 可选 VNPE 营养膏出口兼容
当 `VanillaExpanded.VNutrientE` 启用时，机器吻 SHALL 额外精确接受 `VNPE_NutrientPasteTap`、`VNPE_NutrientPasteFeeder`、`VNPE_NutrientPasteDripper`。兼容 SHALL 按 packageId 与 Def 身份隔离，不按名称、类型子类或管网标签扩大到研磨机、储罐、管道或其他模组目标。未启用时 SHALL 不解析这些 Def；启用但某个 Def 缺失时 SHALL 诊断该项并只跳过该项，不影响其余目标及基础功能。

三者 SHALL 共用 ADD-17 的法式深吻文案、同一次天意抽签、显式 goodOutcome 饱腹、无心情／无 Food 语义，以及 ADD-13 的机器设置、Job 和单一记忆槽。兼容 SHALL 仅采用 ADD-14 的 PowerOn 判据，不检查管网连接、储量、床位连接、开关或原料，不调用出餐／喂食 API，不抽取管网资源，不增加识智收益。

Tap SHALL 使用自身 InteractionCell 及相对朝向的左右一格，遵守 ADD-15 的中央优先与限定候选。Feeder／Dripper SHALL 在自身占格及邻格内寻找可站立、可预订、可到达且满足原版 Touch 的位置，按距离优先，等距用稳定坐标顺序；没有合法位置就拒绝／取消，不强行挤占床位、移动建筑或使用隔墙目标。选定台位 SHALL 继续由 TargetB 保存，沿用持续有效性、中断及完成／返程预订边界。

兼容 SHALL 在 1.6／1.7 共用规则，不引入对方 DLL、框架或 Harmony 的 Mwah 强制依赖，不修改对方 Def、运行组件或原生生产行为。

#### Scenario: VNPE 未加载
- **WHEN** 仅加载 Mwah 的 Core-only 组合
- **THEN** 不解析 VNPE Def、不发缺失告警，基础机器、墙、树和配对仍可用。

#### Scenario: 三种出口的站位
- **WHEN** Tap 中央不可用但其左右候选合法，或 Feeder／Dripper 附近有合法 Touch 位置
- **THEN** 分别使用限定侧格或合法接触格；所有合法候选都不可达时不开始表演。

#### Scenario: 已站在小型出口占格
- **WHEN** Pawn 所在格与可穿过的 Feeder／Dripper 占格重合，且原版站立和预订条件通过
- **THEN** 允许原地表演，不强制先移动到邻格。

#### Scenario: 有电但无管网
- **WHEN** 任一受支持出口 PowerOn 为真，但没有管网／存量或床位连接
- **THEN** 不因这些生产条件拒绝，完成后仍消费同一次天意并发放适用饱腹；不调用真实出餐或自动喂食。

#### Scenario: 已入场出口失效
- **WHEN** 完成前断电、目标消失或固定台位失效
- **THEN** 取消并释放本工作预订，不发完成结果；完成后返程／存读档不重新结算。
