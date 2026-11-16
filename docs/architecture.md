# 架构与维护接缝

共同玩法由一套源码和内容维护，RimWorld 1.6／1.7 分别构建原生 DLL。本文说明实现责任与维护选择；行为合同在 `openspec/specs/`，身份与权限约束在 AGENTS.md。

## 单源内容、双原生输出

```text
Source/Mwah/                         唯一 C# 项目和源码
Defs/Kiss/                           共同 Def
Languages/                           English／ChineseSimplified 共同语言
Compatibility/VanillaNutrientPasteExpanded/
  Defs/ Languages/                   可选登记与专用文案
1.6/Assemblies/Mwah.dll               1.6 原生输出
1.7/Assemblies/Mwah.dll               1.7 原生输出
About/                              元数据及模组图像
LoadFolders.xml                     实际加载选择
```

| 游戏目标 | 框架 | Krafs.Rimworld.Ref 编译引用 |
| --- | --- | --- |
| 1.6 | net472 | 1.6.4871 |
| 1.7 | net10.0 | 1.7.4990-unstable |

`MWAHGameVersion` 默认 1.6，选择框架、引用与输出；`MWAHBuildFlavor` 选择 Dev／Steam／GitHub 渠道。业务源码不复制成两份，也不增一个所谓共享核心 DLL。只在实际不同的 API／框架处适配，旧调用的入口、数据含义与使用条件一起保留。

编译引用与实测游戏 build 分开：在 1.7.5011 rev24 上运行成功不等于引用包已升级。产品版本以 csproj 的 `<Version>` 为主源，About、工坊文案和双语 changelog 跟随。

## 根加载也要显式声明

```xml
<v1.7>
  <li>/</li>
  <li>1.7</li>
  <li IfModActive="VanillaExpanded.VNutrientE">Compatibility/VanillaNutrientPasteExpanded</li>
</v1.7>
```

1.6 使用自己的版本项。命中自定义列表后不再自动追加根或 Common，因此 `/` 必须保留。后列根在相同相对路径上优先；本模组用不同文件路径和身份避免靠整份覆盖拼接功能。

根不放 Assemblies，minor 目录不放共同 Def／语言副本；每个有效组合只选当前 minor 的一份 Mwah.dll。VNPE 未启用时不采用兼容根。未选中的旧 Common 不会自动生效；仍被选中的根内若有残留文件才可能参与加载，所以安装仍完整替换旧副本。

暂存入口只复制 `Defs`、`Languages` 和指定兼容目录，再分别取两版 DLL、About 与加载声明；**选择根目录不等于递归复制整个仓库**。Source、docs、archive、art、工具和缓存都不进运行包。实际内容清单、根选择、跨根重名和程序集框架由既有检查共同约束。

## 请求沿现有责任传递

```text
右键／导演台／自主撮合
  → 请求判定与可见性／拒绝原因
  → 双 Pawn Job 或共用单 Pawn 物体 Job
  → 天意行与适用奖励
```

| 责任 | 实现入口 | 维护要点 |
| --- | --- | --- |
| Pawn 右键、物体右键 | 两个 FloatMenuOptionProvider 子类 | 由原版发现；不加 Harmony 或第二套菜单系统 |
| 导演台 | MainButtonWorker_KissDirector、Dialog_KissDirector、KissDirector | 底栏按钮与非模态面板；地图输入交给原版 Targeter |
| 自主撮合 | KissAmbient | GameComponent 定期运行，默认关闭，只撮合 Pawn 配对 |
| 配对判定与派发 | KissBoundary、KissScope、KissUtility | 结构／门禁／参与／主动资格分开，结果携带可见性、Allowed 和原因，不为显示原因再跑一次寻路 |
| 非 Pawn 扩展 | MWAH_KissAddonDef、KissThingAddon、KissThingAddons | 统一接入右键、导演台、开关、站位和记忆，当前有 wall／tree／machine |
| 可选目标 | MWAH_KissTargetDef、KissTargets | 类型在主程序集，登记实例与专用文案在条件根；Def 加载后解析目标，缺失或歧义项诊断隔离 |
| 执行 | JobDriver_Kiss、JobDriver_KissThing | 原生 Job／Toil 推进，配对发起方统一结算；物体共用单 actor 管线 |
| 站位与返程 | KissStage、KissReturn | 保留不可移动接收者的位置；返程仍在同一 Job，不抢回后续命令 |
| 奖励 | KissMoodReward、KissFate、Thought_MemoryFated | 结构在 Def、句子在 Keyed；旁白随思想实例保存 |
| 设置 | MwahSettings、MwahSettingsView、MwahMod | 规范值、编辑缓冲与磁盘写入分开；Constants 提供默认，MwahTime 负责单位换算 |
| 诊断 | MwahLog、KissTrace | 观测执行，不拥有业务进度；详见 diagnostics.md |

心情需求是收益条件，不是参与条件；读取运行时 need，不用 Humanlike 替代。配对的 SocialImpact 缩放与机器的识智缩放是不同策略。营养膏饱腹只看天意行的显式 goodOutcome 标记及已有 Food need，不从最终心情正负推断。

## 状态由谁保存

| 状态 | 权威与寿命 | 保存方式 |
| --- | --- | --- |
| Job 执行进度 | 原生 JobDriver／Toil，该次 Job | 原版字段 |
| 完成事实、双方绑定、一次收尾 | 两个吻 driver，该次 Job | 保留既有字段、默认和旧档补绑含义 |
| 台位、角色与返程 | Job target 约定及 KissReturn，该次 Job | 既有 Job／driver 数据 |
| 当前是否占用亲吻 | KissActivity／KissPhase 的只读解释 | 不另存并行 FSM |
| 单 Pawn／配对冷却 | KissCooldown，局内使用的进程表 | 不保存，KissTicker 按建局／读档清零 |
| 导演台点选代际 | KissDirector，当前取点会话 | 不保存，过期回调无效，关窗清理当前取点 |
| 数值编辑文本 | MwahSettingsView，设置窗口 | 不作为玩法配置持久化 |
| 规范设置 | MwahSettings，跨局 | 保存非默认值及按扩展稳定 Id 索引的字典 |
| 保存代次记账 | MwahMod，进程内 | 成功写入后确认对应代次，失败保留待保存状态 |
| 可选目标缓存与天意表 | Def 加载期 | 从有效 Def 重建，不作为第二份业务权威 |
| 阶段样本 | KissTrace，共享观测字段 | 玩家机器上的诊断文件，不进入存档或仓库 |

原生 Toil 拥有进度；角色、成功完成与资源收尾是不同事实。不能为了统一形式再造一个与 Job 并行推进的状态机。

未完成取消不发完成奖励；完成时释放亲吻预订与占用；返程不重复结算，也不替换更新的玩家命令。旧档缺少绑定身份时经明确恢复入口补绑，查询视图本身不成为修复 owner。

七档 KissScope 表达单一累积范围。未来若确需跨档组合，应增加真正独立的选择维度，而不是恢复 allowHostileTargets／allowMoodless 两个重叠开关；当前不新增这种功能。

## 可选兼容不改变基础条件

工作台按类型族识别，原版研究设备与营养膏机按明确 Def 身份处理；不承诺任意自制机器自动兼容。VNPE 三出口只提供精确目标名、站位策略和 tooltip 键，不修改对方 ThingDef，不引用对方程序集或类型，也不检查管网库存、生产或床位。

有 CompPowerTrader 的目标只读取 PowerOn，不限制其供电来源；没有电力组件的非电力工作台保留原条件。所有机器共用一个记忆槽；好营养膏结果可以给已有饥饿需求饱腹，研究设备仅放大正面心情且识智封顶 20，不推进研究。

扩展通过 JobDef.modExtensions 绑定回描述；可选文化内容用 MayRequire 隔离。自定义 Def 类使用当前 XML 短名解析可达的 RimWorld 命名空间，保持 MWAH_ 前缀；不要为了配合无关的 DLL 字符串清单制造空运行字段。

## 修改与验证

保存身份和含义必须保持；新增保存数据要说明默认、旧档读取与迁移，不是一概禁止新增字段。运行内容与玩家文字维护双语，时长存 tick。新玩法先按 specification.md 判断是否改变合同，再完成真实消费者和退出路径。

代码／内容／打包改动沿用现有离线门和真实渠道入口，检查实际包；纯文档不重跑无关构建。原生确认由维护者执行，不操作或链接游戏安装目录。当前候选与实际覆盖在[候选确认](../openspec/changes/shared-content-and-lifecycle/native-acceptance.md)，发布渠道结果在 [release-status.md](release-status.md)；这些信息不依赖可删除的历史归档。
