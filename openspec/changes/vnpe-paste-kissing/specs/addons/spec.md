# VNPE 可选出口合同变更

2026-11-07 已采用，见 [采用依据](../../adoption.md)。现行唯一正文为 `openspec/specs/addons/spec.md`；本文件保留本次 delta，不是第二份活动规格。

## MODIFIED Requirements

### Requirement: ADD-13 机器目标与单一身份
机器吻 SHALL 按 `Building_WorkTable` 类型族接受目标，并精确接受原版 `SimpleResearchBench`、`HiTechResearchBench`、`MultiAnalyzer`、`NutrientPasteDispenser` 四个 ThingDef，并按 ADD-19 接受明确声明的可选兼容目标。特例 SHALL 先于一般类型族选择策略，不按名称、生产分类或其他模组的相似功能兜底匹配。稳定身份 SHALL 为设置 `machine`、工作 `MWAH_KissMachine`、记忆 `MWAH_KissedMachine`；默认开启、思想时长 30000 ticks。普通、研究、营养膏策略 SHALL 共用一个机器记忆槽，与墙／树独立，替换时同步档位、旁白、时长和倍率。

#### Scenario: 模组工作台与非工作台机器
- **WHEN** 模组目标继承 `Building_WorkTable`，或只是名称相似但不属于该族
- **THEN** 前者进入普通机器吻，后者不被认领，除非是上述精确特例或 ADD-19 明确声明的可选目标。

#### Scenario: 从研究设备转吻普通工作台
- **WHEN** Pawn 已有识智加成的机器记忆，再完成普通工作台吻
- **THEN** 更新同一个槽，并将倍率恢复为普通机器的值，不保留研究增益。


## ADDED Requirements

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
