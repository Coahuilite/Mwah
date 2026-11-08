# 机器吻技术设计

定稿：2026-11-05；本设计先于运行实现。合同见 [现行 addons](../../specs/addons/spec.md#requirement-add-13-机器目标与单一身份)。

后续采用的 VNPE 三出口兼容使用同一 Paste 策略，额外目标和接触站位由[独立设计](../vnpe-paste-kissing/design.md)负责，不改写本页的原版目标设计快照。

## 一条共享工作，机器自己拥有策略

- 新增 `KissMachineAddon`、描述 Def `MWAH_MachineAddon`，身份 `machine`；Job `MWAH_KissMachine` 固定指回描述，复用 `JobDriver_KissThing`。默认开启／30000 ticks，设置页自动发现，无额外控制器或建筑组件。
- 新增自己的原版目标 DefOf 引用，按 ThingDef 对象精确认领四个特例，再使用 `is Building_WorkTable`。研究不按 `Building_ResearchBench` 族泛化。
- 基类只增加目标状态判定、选站位及固定台位策略的窄虚接口；默认保持墙／树旧路径。机器 Job 的 TargetB 是唯一台位权威，原版 Job 序列化它；不插入／重排旧 toils，不改变已有完成／返程字段。
- 准入重查 PowerOn、预订与可达；入场预订机器及台位／椅子，失败释放本工作部分预订。接近／表演检查目标与固定格有效性；完成前再检查 PowerOn。返程使用现有释放路径，不持有机器或台位。
- 电力只读 `TryGetComp<CompPowerTrader>()?.PowerOn`；组件不存在则不需电。不读电网、故障、开关、料斗、燃料、账单或供电来源，不调用完整生产可用函数。

## 站位就是预订的那一格

- 营养膏：原版 InteractionCell，随后 `InteractionCell ± Rotation.RighthandCell`。中心优先；侧格按 Pawn 距离排序，等距左优先。三格各自检查站立、地图可见、Touch、可达与台位可预订；不扩大集合。
- 工作台／研究台：只接受原版 InteractionCell，椅子与 Pawn 同格；未声明交互格的模组工作台采用原版该属性的计算语义。
- 多元分析仪：按距离搜索邻格，要求原版即时 Touch 可达，不穿角接触。
- 原版 `ReserveSittableOrSpot` 预订椅子或格子；同时预订台位格，防止台位与椅子成为两套互不相干的所有权。拒绝已有生产占用，不强制取消别的 Pawn 工作。原版取餐不完整预订机器，本方案不承诺全局禁止取餐。

## 先抽一次，再结算思想与效果

- 让基类 Settle 可覆写，墙／树仍走旧实现。机器选择 Basic／Research／Paste scope，`KissFate.Roll` 一次；空表生成同分布结果行，再进入同一个机器结算出口。
- `KissMachineOutcomeExtension.goodOutcome` 是机器行元数据，默认 false。只用它判断饱腹，绝不从心情或档位反推。
- 保留现有 GrantRow 签名，增加显式倍率重载；原调用不改倍率。机器始终写入倍率（研究正档使用识智系数，其余为 1），新建与替换路径相同，避免共享槽残留倍率。
- 识智在完成时采样，包含原版资质；缺技能或不可用按 0，封顶 20。既有记忆不动态跟随技能。不授予研究进度／经验，不乘 SocialImpact 或配对设置倍率。
- 饱腹只写实际 Food 的 CurLevelPercentage 满值；不创建餐、不使用 TryDispenseFood／Ingested，不改健康状态。无 Food 不发饱腹短讯；无心情但有 Food 的菜单使用仅说明无心情奖励的提示。
- 沿用完成／finished 标志及相同 toil 索引，结算在表演结束最多一次；后续返程存读档不再结算。

## 数值已定

五档共同基础：-2、0、1、3、6；权重 20、30、25、17、8，合计 100。各表只有最后两档显式标记好结果，营养膏饱腹概率 25%。研究正面乘 `1 + clamp(L, 0, 20) / 20`，负面／零档不缩放。

| 识智 | 五档实际心情 |
| --- | --- |
| 0 | -2、0、1、3、6 |
| 10 | -2、0、1.5、4.5、9 |
| 20 及以上 | -2、0、2、6、12 |

工具计算的每次期望为 0.84／1.46／2.08（分别为识智 0／10／20）；这是设计计算，不是运行测试。单机器槽、默认半日和满识智最高 +12 保持奖励可见，同时不靠叠机器刷多条思想。

## 中英天意文案定稿快照

运行文字实施后以版本内容的 Keyed 文件为准；下表保留先定文案再实现的设计依据。各行以 `MWAH.Fate.Machine.{策略}.{档位}` 为键。

| 策略／档位 | 中文 | English |
| --- | --- | --- |
| Basic.Awkward | 我吻了{TARGET}。它没有回应，倒是我的嘴唇替它完成了一次质检。 | I kissed {TARGET}. It did not respond, but my lips seem to have completed its quality inspection. |
| Basic.Quiet | 这一吻没有产出。至少也没有生成待办。 | The kiss produced nothing. At least it did not create another bill. |
| Basic.Warm | 我在{TARGET}冷硬的边角上，找到一点不合规的温柔。 | Along the cold edges of {TARGET}, I found a little unauthorized tenderness. |
| Basic.Answered | 吻过{TARGET}以后，它看起来仍是一台机器。只有我开始把它往后余生里摆。 | After our kiss, {TARGET} still looked like a machine. I had merely started making room for it in the rest of my life. |
| Basic.Devoted | 我已经想好了婚礼。{TARGET}负责站着，我负责向所有人解释。 | I have already planned the wedding. {TARGET} will stand there. I will explain it to everyone. |
| Research.Awkward | 我试图从{TARGET}的回应里提炼爱情。唯一经得起复现的结论，是我有些尴尬。 | I tried to extract love from the response of {TARGET}. The only reproducible finding was my embarrassment. |
| Research.Quiet | 假设：亲吻能推进研究。实验结果：研究进度没有变化。嘴唇也没有。 | Hypothesis: kissing advances research. Result: no change in research progress. None in my lips, either. |
| Research.Warm | 亲吻{TARGET}之后，我突然理解了一个问题。不是研究项目，是我为什么会亲它。 | After kissing {TARGET}, I understood something at last. Not the research project. Why I had kissed it. |
| Research.Answered | 它没有给我答案。我却从这一吻里，读出了一整篇关于相爱的论文。 | It gave me no answers. Somehow I read an entire paper on mutual affection into that kiss. |
| Research.Devoted | 我吻了{TARGET}，并证明我们相爱。推导过程有些跳步，但结论美得无法反驳。 | I kissed {TARGET} and proved that we were in love. The derivation skips a few steps, but the conclusion is too beautiful to dispute. |
| Paste.Awkward | 我把舌头交给{TARGET}。它回敬了一次难以忘怀的金属口感。 | I offered my tongue to {TARGET}. It returned an unforgettable metallic aftertaste. |
| Paste.Quiet | 我们深吻了。没有营养膏，也没有下文。 | We kissed deeply. No nutrient paste. No further developments. |
| Paste.Warm | 这个法式深吻略带营养膏风味。我现在对浪漫的定义变得十分具体。 | That French kiss had a hint of nutrient paste. My definition of romance has become remarkably specific. |
| Paste.Answered | 它吻了回来。原料一粒没少，我却忽然觉得，这世上没有比它更会照顾我的机器。 | It kissed me back. Not a grain of feedstock was lost, yet I suddenly felt that no machine could care for me better. |
| Paste.Devoted | 我们之间发生了料斗无法解释的事。我愿意称之为爱情，物质守恒可以晚点再谈。 | Something happened between us that no hopper can explain. I choose to call it love. Conservation of matter can wait. |

实际补满 Food 时另发短讯：“{PAWN} 在与 {TARGET} 的深吻中饱餐了一顿。”／“{PAWN} came away from a French kiss with {TARGET} completely fed.” 无 Food 不发送；其他旁白不谎报需求已增加。

## 版本、存档与验证

原版 API 在此前 1.6 源码及本地 1.7.4998 rev1299 快照核对；编译以项目固定引用 1.6.4871／1.7.4990-unstable 为准。沿用 net472／net10.0 和双版本内容打包；不升级依赖、不写游戏目录。

旧配置缺机器槽采用新默认，墙／树键及 JobDef 不迁移。旧 Job 没有 TargetB 时不增加固定台位门槛。允许带着新功能更新旧存档；卸载／降级不承诺清除新 Job／Thought Def 引用，应先结束新工作并处理新记忆。已有正式包不改写。

复用 verify-local 的双版本三渠道编译、XML、双语、Def、天意和零 Harmony 门；只扩充实际交付清单，不新增单元测试或等价脚本。维护者实机确认：三种站位、断电／特殊来源 PowerOn、识智与单槽替换、无 Food／无心情、取消、进行中／返程存读档，以及配对／墙／树回归。实际 UI／原生行为验收保持待办，不把编译当作实机通过。
