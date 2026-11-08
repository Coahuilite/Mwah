# VNPE 独立兼容设计

## 精确身份与隔离

- 支持包：`VanillaExpanded.VNutrientE`；仅精确注册 `VNPE_NutrientPasteTap`、`VNPE_NutrientPasteFeeder`、`VNPE_NutrientPasteDripper`。研磨机、储罐和管道不在范围内。
- 独立适配文件 `Source/Mwah/Compatibility/VanillaNutrientPasteExpandedCompatibility.cs` 只依赖 Verse API，独占 packageId、三个 DefName 和目标分类。Def-load 启动期先检查 ModsConfig.IsActive，未启用则立即返回；启用时解析并缓存实际 ThingDef 对象。每个缺失项只告警一次、独立不注册。
- 不扫描外部程序集、不引用 VNPE／VEF／PipeSystem／Harmony、不做补丁、不注入组件、不改对方 Def，不把它们加入 Mwah 的 modDependencies。对方模组自身的依赖仍由其维护者和玩家负责。
- 只给机器 worker 两个识别入口：IsOutlet／IsTouchOutlet；特例标识全部留在兼容文件。原版四个特例仍由原 DefOf 提供，不重新分类旧工作的 addon 身份。

## 同一个法式深吻，两种接近方式

- 原版营养膏机与 VNPE Tap 都是前向策略：自身领取格优先，再考虑自身旋转后的左右格。Tap 本机 Def 是 3×1，interactionCellOffset 为 (0,0,1)，不能复制原版的固定偏移。
- Feeder／Dripper 为接触策略：候选为占格＋八向邻格，按 Pawn 距离、x、z 排序；仅接受站立／可见、ReachabilityImmediate Touch、可达及原版预订均成立的位置。允许已站在合法占格上的 Pawn 原地表演，不专门安排坐下，不抢床或旁路预订。
- 复用 TargetB 固定台位、机器及台位／椅子预订、表演持续位置检查和完成／取消释放；不修改共享 JobDriver 或旧 toil 索引。
- 接触策略使用单独中英 tooltip，避免告诉 Feeder／Dripper 玩家只可站在领取格左右。

## 奖励和电力保持原合同

三个出口统一进入已有 Paste scope：同一法式深吻菜单和报告、一次抽签、显式好结果、Food 饱腹、消息与唯一机器记忆槽。默认分布、时长、冷却和好结果概率保持原机器规格，不增加独立识智倍率。

只读原版 CompPowerTrader.PowerOn，不追查来源。兼容不调用 CanDispenseNowOverride（包含管网存量门槛）、TryDispenseFoodOverride／DrawAmongStorage（消耗网络资源），不要求管网或床位连接。Dripper 自身可能继续进行它原有的自动喂食，Mwah 不暂停、接管或伪装它的生产行为。

## 本机证据与版本

2026-11-07 只读核对第三方 About、loadfolders、1.6/Defs/ThingDef_Buildings/Buildings_Production.xml、VNPE.dll 类型与相关 IL：Tap 继承 Building_NutrientPasteDispenser，Feeder 继承 Building_Storage，Dripper 继承 Building；三者均配置原版 CompPowerTrader。对方 1.6／1.7 均加载其 1.6 目录。研究记录不复制第三方代码或资产。

原版 API 对照本地 1.7.5000 rev1275；最终编译仍使用项目固定引用，不升级依赖。保持 Mwah 的 net472／net10.0 分开构建和版本内容布局。

## 有限验证与交接

复用 verify-local 的双版本／三渠道构建、XML、双语键、Def、真实符号与零 Harmony 门，并只扩充实际兼容类型的符号清单。编译不依赖第三方 DLL，验证其编译隔离。对有效包使用现有暂存内容检查及隐私审计。

不添加单元测试或等价脚本，不运行游戏。维护者用实际包确认未加载 VNPE 与正常加载 VNPE 两种组合；三目标可触达／不可触达、Tap 旋转、占格原地、断电／无管网、完成与存读档边界。结果记录在 tasks，不将离线通过写成原生联调成功。
