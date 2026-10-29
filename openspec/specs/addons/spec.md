# 非 Pawn 亲吻扩展

状态：现行开发合同。维护责任：物体吻与内容扩展维护者。
适用条件：已加载的游戏地图、单机；文化扩展按下述可选条件采用。

## Purpose

约定物体目标资格、独立开关、共享单 Pawn 执行及内容结果。扩展通过窄合同组合到亲吻功能，不使配对吻依赖具体墙／树实现。

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
物体吻 SHALL 使用 [KISS-03](../kissing/spec.md) 的 Pawn 参与／发起资格、KISS-04 的冷却、KISS-05 的受伤中断／回程所有权及 KISS-09 的占用规则，并共享朝向和爱心效果；目标范围由 ADD-01 负责。只有 Pawn 运行工作；目标 SHALL 被预订以阻止并发亲吻，并选择附近可达站位。完成 SHALL 只结算一次，未完成取消不发扩展奖励。

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
