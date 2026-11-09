# 0.4.x 执行分工

2026-11-09 维护者已核对并要求用 goal 开始，当前分支 `0.4.x`。采用依据见 [adoption](adoption.md)，设计及不变量见本 change 的 design／state-machines。先读 AGENTS、MEMORY、TODO 和作用域内现行规格。下列是可写执行任务，不沿用早期只读任务的权限。

## 全体执行约束

- OMP、`bailian/qwen3.8-flash`、effort medium；只向主协调者汇报，不自行扩编、切模型、改 CLI 默认或服务。
- Windows shell 只能使用 `pwsh`，禁止 Windows PowerShell 5.x，包括间接调用。不安装依赖或工具，不取凭据，不写本机绝对路径、日志原文或工坊标识到仓库。
- 不运行游戏、不安装／链接模组、不修改第三方目录、不碰 `art/`；不新增单元测试或等价脚本／用例。
- 禁止 Git 写操作（包括 add、commit、branch、push、reset、checkout）；移动内容用本仓库内的原生文件操作，先确认源／目标在授权根内。保留其他 owner 的变化，不做整树清理。
- 主协调者独占 csproj、版本／About／工坊文案／changelog、AGENTS／规格采用／任务／记忆和所有 Git 操作。不要修改这些文件。
- 不并行运行 dotnet／完整验证或打包：同一工程的 obj 和原生输出共享，由主协调者在两 owner 交付稳定切片后执行。各 owner 负责必要的静态、XML／引用与直接调用检查；报告时明确编译／原生未运行，不能声称通过。

## A：共享内容、可选兼容与交付 owner

写范围：`1.6/Defs`、`1.6/Languages` 的迁移，`Common/`、顶层 `Compatibility/VanillaNutrientPasteExpanded/`、`LoadFolders.xml`、`scripts/stage-package.ps1`、`scripts/verify-local.ps1`；VNPE adapter／通用登记消费者、所需的新 Def 类型、`KissMachineAddon.cs`；README 双语、CONTRIBUTING、release-runbook 中直接涉及布局和安装的说明。其他历史文档及其活动路径由主协调者负责，A 不再扫描或改写。

主协调者已核对原始 `DefDatabase<T>`：本地 1.7.5000 与远端源码均将 defsList 静态初始化为 new List，AllDefsListForReading 直接返回它；自定义 Def 没有实例时读空列表合法。此疑点已解决，沿既定 StaticConstructorOnStartup 登记方案实施，不再为此准备替代平台。条件性由 LoadFolders 单独负责，登记无需再叠加相同 packageId 的 MayRequire。

完整交付：

1. Def／语言从旧 1.6 内容源迁到 Common；包中共同内容也只有一份，两 minor 只放自己的 native DLL。保持所有 Def、键和含义，不遗留旧内容作 fallback。共同／兼容根不得有 Assemblies。
2. 两版 LoadFolders 都显式采用根→Common→对应 minor→条件 VNPE 根；只有 `IfModActive="VanillaExpanded.VNutrientE"` 成立才采用可选登记及专用语言。
3. 用小型 Mwah 自有 Def 登记目标 DefName 字符串、前向／接触策略及 tooltip key，在 Def 加载后解析缓存。三个 VNPE 名字只在可选内容中；主 DLL 无 VNPE packageId／DefName 硬编码、无第三方类型／DLL引用。缺一目标只诊断并跳过；重复登记诊断并排除歧义。不改对方 ThingDef。
4. 保留原版四个精确目标与工作台类型族，只有匹配的数据目标复用 Paste。共同天意／Food／单机器槽／识智／PowerOn／站位／TargetB 原合同完全保留，不新建兼容 DLL或另一套结算。
5. 专用 tooltip key 随登记消费，不在基础 C# 无条件引用；共同文案留 Common。基础设置说明不靠可选键。中英键／DefInjected／Pawn 措辞规则在实际有效内容中都要受检。
6. 完整修改暂存及现有门：声明根存在；按基础／VNPE 的实际选择验证根／唯一 native DLL／Def／语言；禁止 old minor 内容副本；保持包整体形状、源文件清单、框架／零 Harmony 和隐私检查。只修实际变更前提，不能删／弱化有效断言来过门。不写新测试或专用验证框架。
7. 若旧 adapter 已被真实替换，更新它的实际程序集符号条目；不保留空类，不要求 XML-only 名称成为 ASCII DLL 符号。生命周期新类型的必要清单信息由另一 owner 提供，脚本只有你一人写。
8. 安装指引明确完整替换旧模组副本、不叠加覆盖旧目录；由维护者操作，保护原存档／配置，不代删安装目录。

交付主协调者：修改清单、布局／条件消费结论、静态检查结果、未运行项、必要直接读者修复；完成稳定切片后停止写入，等待集成结果。主协调者可能将真实编译／包错误返回原 owner 修复。

## B：Job 与界面生命周期 owner

写范围：`Source/Mwah/Jobs/`、`Actions/Kiss/KissUtility.cs`／`KissThingAddons.cs`／`KissDirector.cs`／`Dialog_KissDirector.cs`、必要的 `Systems/KissTicker.cs`；仅在有真实收益时最小整理 `Mod.cs`／`MwahSettingsView.cs` 的提交／保存入口。不得修改 MachineAddon、Def／语言、构建脚本或其他 owner 文件。

完整交付：

1. 以原生 Toil 作为执行进度权威，增加清楚的只读逻辑阶段／占用查询，收口表演完成与 finish-action 幂等入口；不要再序列化一个需要同步的 FSM。保持现有 IsKissing／KissActivity 语义、角色和入口。
2. completed 是倒计时完成；finished 是收尾入口已经运行，取消也可能为真。不能把它理解成奖励已成功发出。保留原 Scribe 键、默认值、Toil 序列和索引，尤其旧档 finished 默认由 completed 与索引推导。
3. 旧配对补绑移出有写副作用的 MatchingPartner getter，变成显式、幂等的绑定／恢复入口；接近和表演两个阶段都要保留原互指、相同 startTick、loadID 约束及启动帧宽限。不要移到引用尚未就绪的 ExposeData 强行修复。
4. 保留双方入场确认后才登记冷却、第二方失败回滚本次已入场 Job、不接管新工作；保留被动方先起、发起方扣时、完成标志同步到双方与即时释放预订。完成后返程不占用、不追原目标、不重发奖。
5. 保留 KissReturn 的 returnTicks、非持久 WalkTally、读档首 tick 再释放旧预订、原生 finalizer 的条件。不要加全局返程队列，也不让取消／暴力／新命令被返程抢回。
6. 修复 QuickSend／PostClose 的收口遗漏：在当前单窗口前提优先统一调用已有 Active 保护的 KissDirector.Stop，保留 generation，旧 cleanup 不清新一跳；不引入新 token/session 框架、不禁用按钮、不添加无证据的 Repaint 队列。KISS-01 已补录关闭当前点选场景。
7. 设置的编辑缓冲与规范值／保存代次分开；当前入口已清楚时允许保留实现并解释。不得改变防抖、提交、失败重试、冷却／自主／诊断寿命或玩家数值，不为所有 bool 强造 enum。

交付主协调者：真实状态／转移和保存兼容说明、直接调用检查、修改及必要程序集类型清单、未运行项。稳定交付后停止写入；集成发现的本切片缺陷回原 owner 处理。
