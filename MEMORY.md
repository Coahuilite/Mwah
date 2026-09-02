# MEMORY

## 当前耐久状态

- RimWorld 1.6 模组，`packageId` `coahuilite.mwah`，产品版本 **0.1.0**（csproj `<Version>` 为主源，About `<modVersion>` 跟随）。尚未发布、无远端仓库、无创意工坊条目。
- 功能面：**仅 live map、仅单机、零 Harmony**。选中**可下令的** pawn 右键另一 pawn → `kiss`（三层判定见「谁能亲：三层边界」）；能走的一方走过去，双方 `FaceTarget` 相对、贴脸（`PathEndMode.Touch`）、按间隔抛原版爱心，结束后各自 `JobDefOf.Goto` 回原位。有 `needs.mood` 的按**自己**的 `SocialImpact` 拿心情，没有的什么都不加。
- 设计取向：**纯娱乐、门槛尽量低**——不限阵营、不限种族、不要求征召。这是维护者的立场，2026-09-02 明确重申过：玩家反馈"想要只撮合殖民地"用**门禁滑条**承接（出厂停在最右档 = 双不限），**不改默认值**。但"零门槛"从来不是事实，也不该假装是：原版 `CanTakeOrder` 与 `ShouldGenerateFloatMenuForPawn` 在 provider 之前就把关，动物与中立者当不了发起方、倒地者连菜单都不生成。2026-09-02 边界收敛后，本模组自己的前提写成 `KissBoundary` 三层，不再用"种族"近似"能力"。收益面保守（见「工程决定」）。工作区目录名 `every_pawn_kiss_each_other/` 是历史值，不构成身份。
- 明确不做（不是待办）：世界地图/商队途中、RimWorld Multiplayer 同步、真·贴合亲吻动画、Downed 者当发起方（需 Harmony 放开原版闸门）。
- **不做 junction**：`Mods/` 与 `Mods/*` 一律不建、不校验、不假设存在；复制模组与实机测试由维护者本人执行（2026-09-02 维护者指令）。

## 命名决定（2026-09-02 定稿）

- 品牌短名 **`Mwah!`**，全称 **`Every Pawn Kisses Each Other`**，中文显示名 **`所有Pawn都给我啵嘴！`**（2026-09-02 定名；英文侧一律照旧。1.6 `ModMetaData.Init` 只读 `About/About.xml`，无按语言改名机制 ⇒ `About.xml <name>` 保持英文，中文名落在中文 Keyed 的分类名与门禁最右档），`packageId` = `coahuilite.mwah`，前缀 `MWAH_` / 键前缀 `MWAH.` / 日志 `[MWAH]`。工作区目录名保留 `every_pawn_kiss_each_other/`，工程身份一律 `Mwah`。
- 候选与淘汰理由（已核实部分标注依据）：`KISS_` 全称最直白但作为通用前缀撞车概率与工坊检索噪音最高（**注意：并非原版占用**——rimsage 核实原版无任何含 `kiss` 的 defName，仅 `Tales_DoublePawn_Relationships.xml` 有 "deeply kissing" 文案）；`PECK_` 是真词"轻吻"但被"鸟啄/轻敲"次要义稀释，且 `PECK_` 无法由全称首字母正向拼出；`CHUU_`（ちゅっ）对中文/ACG 受众有效但英文玩家不直觉；`XOXO_` 的 X 与"处决/取消"视觉混淆且含我们未实现的拥抱语义；`SMOOCH_` 前缀过长。
- 原版 token 占用核查结论：`Mwah / Peck / Chuu / Snog / Xoxo / Smooch` 在 1.6+Odyssey 的 defName、`Defs/Core/Names` 人名部件中**均无占用**（rimsage 检索为空）。
- `MWAH` 明确**不**做全称首字母展开，它是拟声词；全称负责"指示所有小人互亲"的语义，缩写负责菜单与日志里的嘴声。

## 原版能力边界（RimSage 1.6 + Odyssey 源码核验，2026-09-02）

改实现前必读：这几条是本模组形态成立的依据。

- **右键入口自动注册**：`FloatMenuMakerMap.Init()` 对 `typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract()` 反射实例化 → MOD 的 provider 无需 Def/XML/patch 即进菜单链。基类闸门：`Drafted/Undrafted/Multiselect/RequiresManipulation/MechanoidCanDo/CanSelfTarget/CanTargetDespawned/IgnoreFogged` + `SelectedPawnValid/TargetThingValid/TargetPawnValid/Applies/GetOptionsFor(Thing|Pawn)`。
- **`SelectedPawnValid` 的坑**：本模组 override 时**故意不调 base**（base 会按 mutant 白名单一刀切，机制见「谁能亲：三层边界」末条）；`MechanoidCanDo => true` 因此只是声明意图，不是能力。
- **原版硬闸门（provider 层绕不过）**：`GetOptions` 首行要求点击落在 `Find.CurrentMap` 内；`ShouldGenerateFloatMenuForPawn` 的剔除清单见「谁能亲：三层边界」表 → **倒地者不能当发起方**，但可当目标。
- **`ability` 路线被否决的依据**：`PawnComponentsUtility.AddComponentsForSpawn` 只在 `Humanlike || IsMechanoid` 时建 `pawn.abilities`；Odyssey 的 `FleshType.Drone`（`isOrganic=false`）也没有 `interactions` tracker。故 Ability/`InteractionDef` 都不能覆盖"所有 pawn"，只有 FloatMenu provider 可以。
- **贴近/相对**：`pather.StartPath(target, PathEndMode.Touch)`、`SocialInteractionUtility.BestInteractableCell/IsGoodPositionForInteraction(≤6 格 + LineOfSight)`、`rotationTracker.FaceTarget(...)`、`GenAdj.AdjacentTo8WayOrInside(Thing,Thing)`（多格体型正确）。1.6 **已无 `Pawn.CanMove`**，移动能力读法是 `health.capacities.CapableOf(PawnCapacityDefOf.Moving)` + `RaceProps.doesntMove` 特例。
- **爱心特效**：就是 `FleckMaker.ThrowMetaIcon(cell, map, FleckDefOf.Heart)`（`JobDriver_Lovin` 常量 100 tick）。`Heart` 是 **FleckDef**（`Things/Mote/Heart`，`MetaOverlays`），不存在 `Mote_Heart`。
- **双人锁定**：`JobDriver_Lovin` 的镜像 job 手法（给对方 `StartJob(同 def, 自己, InterruptForced)`）；`TryTakeOrderedJob(job, JobTag.Misc)` 用于回原位这类礼貌请求；`Pawn_JobTracker` 在 job 成功后自动接 `Wait_MaintainPosture`。
- **心情缩放**：`Thought_Memory.MoodOffset() = stage.baseMoodEffect × moodPowerFactor + moodOffset`；`Thought_Memory.durationTicksOverride` 可逐实例改时长；`ThoughtDef.DurationTicks = durationDays × 60000`（0.25 日 = 6 游戏时）。原版 `Pawn_InteractionsTracker.AddInteractionThought` 是 public static 但乘的是**对方**的 `SocialImpact`，且前置要求 `Talking` 容量与 `interactions` tracker → 本模组自己写，取**自己**的 `SocialImpact`。
- **不炸的边界**：`SkillNeed_BaseBonus.ValueFor` 对 `pawn.skills == null` 返回 `1f`，且 `SocialImpact` 标了 `neverDisabled` → 无技能单位取该属性安全。
- **社会语义副作用开关**：`Thought_MemorySocial.Init()` 在 `ThoughtMaker.MakeThought` 内被调用并写入 `opinionOffset = stage.baseOpinionOffset` → MakeThought 之后再乘倍率才有效；`ShouldDiscard` 要求 `otherPawn != null && opinionOffset != 0`。
- **日志基础设施的上限**：Player.log 无逐行时间戳、全局 10000 条 Unity 日志上限（到顶后 `Debug.unityLogger.logEnabled = false`，我们的 `Log.*` 全被吞）、连续相同文本折叠到 99 次后不再写入 ⇒ 任何打点必须自带 `t=<tick>` 与唯一字段。设置文件只写非默认值（是 diff 不是快照），结算结果由存档 XML 自带证据。**细节与取舍见 `docs/kiss-trace-logging-design-zh.md`。**
- **单位常量**：`GenTicks.TicksPerRealSecond = 60`、`GenDate.TicksPerDay = 60000` → 1 游戏小时 = 2500 tick。
- 灰项惯例：`new FloatMenuOption(label, null, priority…)`（action 传 null 即禁用），原因写进 label 括号；可用 `FloatMenuUtility.DecoratePrioritizedTask` 标"会抢占"。菜单优先级有现成的 `MenuOptionPriority.InitiateSocial`。

## 谁能亲：三层边界（RimSage 1.6 源码核验，2026-09-02）

挂 job 与种族无关，心情与种族**不**完全同构，"能下令"才是发起方的真天花板。

| 层 | 判据（原版源码） | 结论 |
|---|---|---|
| 技术：能挂 job | `PawnComponentsUtility.CreateInitialComponents` 给**所有** pawn 建 `jobs`/`needs`/`stances`；只有 `RemoveComponentsOnKilled` 置空 | 活着的 spawned pawn 一律能跑自定义 `JobDriver`，与 `intelligence` 无关 |
| 发起：能被下令 | `FloatMenuContext` 构造里 `RemoveAll(!CanTakeOrder)`；`CanTakeOrder = IsColonistPlayerControlled ∨ IsColonyMech ∨ IsColonySubhumanPlayerControlled`，而 `IsColonist` 要 `RaceProps.Humanlike ∧ ¬IsSubhuman` | **动物永远当不了发起方**（`IsColonyAnimal` 不在集合里）；殖民地机械族、玩家的可征召 subhuman 可以 |
| 发起：菜单前置 | 1.6 新增 `FloatMenuMakerMap.ShouldGenerateFloatMenuForPawn` | 倒地→`IsIncapped`、隐眠中、被 Lord 管控 → 任何 provider 都不生成 |
| 收益：有心情 | `NeedDef Mood`：`minIntelligence=Humanlike` + `developmentalStageFilter=Baby,Child,Adult`，无阵营/囚奴限制 | 人形即有心情：殖民者、奴隶、囚犯、访客、商人、袭击者、古人、Creepjoiner |
| 收益：无心情 | `RaceProperties.intelligence` **无初始化器**→默认 `Animal`；显式声明的其余全是 `ToolUser`（机械族、Odyssey 无人机、Anomaly 血肉兽/虚空实体） | 动物/昆虫/机械/无人机/异象生物没有 mood，亲吻只有动作没有收益 |

- **陷阱**：`BaseMutantEntity`（Shambler / Ghoul / AwokenCorpse 三个 mutant 共享）有 `disableNeeds=true`。它们的种族就是 `Human`，`RaceProps.Humanlike` 为真，但 `needs.mood == null`。⇒ 心情判据必须读运行时 need 实例，读种族会误纳。（原版 Humanlike 种族只有 `Human`，外加 Anomaly `CreepJoiner` 继承它。）
- 原版**没有任何** hediff / gene / trait / precept 把 `Mood` 放进 `disablesNeeds`（被关的是 Rest/Comfort/Outdoors/Beauty/Food）；`Mood` 也没有 `nullifyingPrecepts`/`titleRequiredAny`/`hediffRequiredAny`。所以"人形必有心意"除 mutant 外无例外。
- 社交语义对齐 `SocialInteractionUtility`：`interactions != null`（只有 `RaceProps.IsFlesh` 才建）、`CapableOf(Talking)`、`Awake()`、非燃烧、非 `MutantDef.incapableOfSocialInteractions`、非 `IsInteractionBlocked`（后者认 `HediffDef.blocksSocialInteraction`，原版只有 Biotech/Odyssey 的仪式 hediff 置了它）。
- `Awake()` = `health.capacities.CanBeAwake` ∧ 当前 `curDriver.asleep == false`（`RestUtility`）。⇒ 昏迷/麻醉/睡觉/隐眠被挡；**倒地但清醒仍通过**，所以"亲倒地的人"这条功能保留。
- `LifeStageDef.canInitiateSocialInteraction` 默认 `true`，原版只有 `HumanBaby` 置 `false`（它同时 `alwaysDowned=true`，两道门重叠）。
- 容量来自 body part 的 **tags**：`TalkingSource`（人形下颌、动物颌/喙）+ `TalkingPathway`（颈部）。机械族有 `MechanicalNeck` 的 `TalkingPathway` 却**没有 `TalkingSource`** ⇒ `CapableOf(Talking)` 对它们恒假 —— "有嘴才能主动"只能对 `RaceProps.IsFlesh` 判，否则 `MechanoidCanDo` 变死代码。机械族的 `ArtificialBrain` 带 `ConsciousnessSource`，`Awake()` 正常为真。
- `MutantDef.whitelistedFloatMenuProviders` 在三个 mutant 上都是**空表**（非 null），原版 `SelectedPawnValid` 因此把它们整体挡在右键菜单外；本模组故意不调 base，改由参与层逐条判（只有 `incapableOfSocialInteractions` 的被挡）。

## 工程决定

- `AGENTS.md` 每轮注入，是成本：只放记忆协定、不可漂移的身份、漏看即做错的硬边界；**禁止易变内容**（目录结构、文件清单、命令行、版本目录字面量、进度矩阵）——那些写在本文件与 `TODO.md`。预算 ≤ 35 行（2026-09-02 维护者规则）。
- 自主亲吻（2026-09-02 维护者要求"所有 pawn 都啵嘴"）走 ' + BT + 'KissAmbient : GameComponent' + BT + ' 定时促成，**只对玩家管不着的 pawn 生效**（判据 ' + BT + 'KissBoundary.UnderPlayerManagement = IsPlayerControlled ∨ IsColonyAnimal' + BT + '，即自家动物算玩家侧），玩家能下令的照旧只能右键。
  两条路径**互不相干、并行存在**，只在 ' + BT + 'KissUtility.Propose' + BT + ' 汇合：右键那次不会派生后续，定时器也不因你右键过而多派。不存在"玩家下令后转交游戏继续下发"。
  自家动物的后果：原版 ' + BT + 'CanTakeOrder' + BT + ' 不含 ' + BT + 'IsColonyAnimal' + BT + '（右键下令不了它），加上现在也不算自主对象 ⇒ **只能被亲，不会主动亲**。要它也能主动，只能把它留在自主派发里（改判据一行）。
  理由：原版 ' + BT + 'FloatMenuContext' + BT + ' 先 ' + BT + 'RemoveAll(!CanTakeOrder)' + BT + '，非己方 pawn 连被选中都做不到，零 Harmony 无解。
  当初否决 ' + BT + 'InteractionDef' + BT + ' 路线的理由（只覆盖 flesh + humanlike、与全覆盖取向冲突）依然成立——正因如此才改用定时器；' + BT + 'GameComponent' + BT + ' 由原版自动实例化，不需要 Def/XML/补丁。
  自主发起的 job 打 ' + BT + 'playerForced = true' + BT + '，否则 pawn 自己的 think tree 会在下一个 override 检查点把它拽回去，变成"起步即取消"。
  三个新设置项：' + BT + 'autonomousKissing' + BT + '（默认开）、' + BT + 'autonomousIntervalTicks' + BT + '（默认 250 = 1 游戏时一次机会）、' + BT + 'autonomousRadiusCells' + BT + '（默认 10 格，防止穿越全图去亲、把战斗变成观光团）。
- 菜单被拒必须说真原因：`KissProposal` 即使 `Visible=false` 也带 `BlockedReason`，点下过期菜单项时回显该原因；没有原因的那条路径（自亲，已被 `CanSelfTarget` 挡）说"这个亲吻已经不成立了"，不再谎报"已经在亲了"。
- 2026-09-02 三次硬崩溃`**已归因到环境，不是本模组**`：`0xC0000005` 读 `0x1000000000000000` @ `ntdll!RtlDosApplyFileIsolationRedirection_Ustr+0x387`（Windows 正在惰性加载 `TextShaping.dll`），进程内同时注入 Steam 覆盖层与 Defender `MpOAV`。本模组零 DllImport、零原生加载，最多是"让新中文文本第一次上屏"的触发者。取证方法见跨项目排查指南 §6.5–6.6。
- 打点裁定（2026-09-02 维护者）：**只走 Player.log**，不建自定义文件、不做环形缓冲与按需导出（为"亲亲"引入这些是过度设计）；**每行必带双方身份**（`doer=` 与 `recv=`），不允许靠 cid 回查上一行。方案与依据见 `docs/kiss-trace-logging-design-zh.md`。
- 允许性面 = **七档 `KissScope` 滑条**（`pairScope`，0 只自由殖民者 → 6 万物互亲，出厂 6）。每档 = 前一档 ∨ 一类人群，双方都要在档内，谓词全取自原版（`IsFreeNonSlaveColonist`/`IsColonist`/`IsPrisoner`/`IsColony*`/`Humanlike`/`IsFlesh`/`HostileTo`）。它取代了 `allowHostileTargets` 与 `allowMoodless` 两个布尔开关（见 `OBLIVIONIS.md`）；档位名键由枚举拼接，靠 `verify-local` 的门反向核对双语与范围常量。
- 冷却（单人 + 成对）为**会话内内存态**，不落盘、不占 tick，只在结算时顺带清过期项；读档后归零是接受取舍（与 `let_me_gnaw_on_you` 的 `CooldownManager` 同一口径）。
- 默认 `changeOpinion=false`：用普通 `Thought_Memory`，不写好感度、不喂原版恋爱链；开启后切换到 `Thought_MemorySocial` 变体。
- 心情发放只由**发起方**结算一次，被动方只负责自己的回原位；`isPassivePartner` 是这条不变量的载体。
- 对象倒地/无 jobs 接不下镜像 job 时，发起方仍独自完成亲吻（爱心照冒、心情照发）；主动方不设"对方必须在亲吻 job 中"的失败条件，只有被动方设。
- 设置生命周期：内存即时生效 + 磁盘 0.35 s 防抖合并 + 关窗强制 flush + 失败保留 dirty 并 2 s 重试（依据 `modding_documents/draft/modsettings-value-lifecycle-decision-tree-zh.md`）。
- 时长存储单位统一 tick；`thoughtDurationGameHours` 按 1/4 游戏小时量化，避免脏小数进配置文件。
- 本仓库的骨架、四件套记忆文件与四份脚本已抽象上收为通用指南：`modding_documents/RimWorld_Mod_RepoInit_AgentMemory_And_Packaging_Guide_zh.md`（占位符版，无项目身份）。同类新工程先读它，不要重新发明。注意 `modding_documents/` 本身不在任何 git 仓库下，改它不产生提交。
- 骨架照 `RimWorld_Mod_Skeleton_Guide_bilingual.md` Level 2 + `let_me_gnaw_on_you`/`squeaky_ratkin` 现行做法：版本化 `1.6/`、`.slnx` only、`net472` + `Krafs.Rimworld.Ref 1.6.*`、语言目录用 `ChineseSimplified`（`LanguageDatabase` 硬编码名录里的真实名字）。

## 仓库结构与导航

运行内容全部在版本目录 `1.6/` 下（`Defs/Kiss/`、`Languages/{English,ChineseSimplified}/`、`Assemblies/` 为构建产物）；`LoadFolders.xml` 映射 `/` 与 `1.6`；解决方案只有 `Mwah.slnx`，不建 `.sln`；C# 源在 `Source/Mwah/`（`DefOf/ Actions/Kiss/ Jobs/ Rewards/ Systems/`）。目录细节以 `ls` 为准，本节不维护树状图。
| 要看什么 | 位置 |
|---|---|
| 右键入口 | `Source/…/Actions/Kiss/FloatMenuOptionProvider_Kiss.cs`（原版反射发现，无需注册） |
| 谁去亲 / 能不能亲 | `Source/…/Actions/Kiss/KissUtility.cs`（`KissProposal` 三态） |
| 亲吻表演与结算 | `Source/…/Jobs/JobDriver_Kiss.cs`（双人镜像 job） |
| 心情与社交缩放 | `Source/…/Rewards/KissMoodReward.cs`（无 `needs.mood` 者静默） |
| 谁能亲谁（范围档位） | `Source/…/Actions/Kiss/KissScope.cs` | 七档累积析取，出厂最右档 |
| 冷却 | `Source/…/Systems/KissCooldown.cs`（会话内内存态） |
| 单位换算唯一入口 | `Source/…/MwahTime.cs` |
| 设置项与生命周期 | `Source/…/MwahSettings.cs` + `Mod.cs`（即时生效 + 防抖落盘） |
| 日志/打点设计与引擎上限 | `docs/kiss-trace-logging-design-zh.md` | 未实现，含保留意见与待决策 |
| 打包与门 | `scripts/`；跨项目通用骨架见 `../modding_documents/RimWorld_Mod_RepoInit_AgentMemory_And_Packaging_Guide_zh.md`；原生崩溃（日志无栈的突发崩溃）排查见 `../modding_documents/RimWorld_NativeHeapCrash_Triage_Guide_zh.md` |

## 提交与命令实践

- Conventional Commits 1.0.0；主题英文祈使句，正文可中文写动机与取舍。
- 原子划分：骨架/metadata → gameplay 代码 + Defs → 本地化 → 打包工具 → 记忆文档。Defs 与代码同一条（拆开会留下构建失败的中间态），本地化可分开。
- 改名类变更走"改动后新建提交"，不回写历史。
- 分支模型：**原子提交落在 `dev`**，`main` 只在发布时前进（对应指南里的"main 保护 / dev 原子"）。2026-09-02 之前的 9 条提交实际全落在 `main`，`dev` 停摆 7 条 —— 已用纯 fast-forward 把 `dev` 指到 `main` 并切到 `dev` 工作，未动任何提交。远端与分支保护建立前，`main` 只是发布锚点。
- 不入库：`dist/`、`1.6/Assemblies/*.{dll,pdb,xml}`、`Source/**/{obj,bin}`、`About/PublishedFileId.txt`、`.idea/`、`.vs/`。
- 命令：`dotnet build Source/Mwah/Mwah.csproj -nologo`；`pwsh -NoProfile -File scripts/verify-local.ps1`（15 项静态与产物门）；`scripts/build-dev.ps1`（构建 + 出包）；`scripts/pack-dev.ps1`（只打包，需已有产物与 HEAD）。无测试工程；实机面见 `TODO.md`。

## 验证状态

- 已绿（离线）：`dotnet build` Debug/Release 均 **0 警告 0 错误**；`scripts/verify-local.ps1` 15 项全 `[ok]`（8 个 XML 良构、Keyed 中英各 48 键且集合一致、C# 引用的 48 个键双语齐备、DefOf↔defName 无孤儿、`driverClass` 与 `namespace.type` 一致、DLL 含 13 个关键符号、DLL 无 Harmony 符号、版本与 packageId 三处一致、无绝对本地路径、设置项字段↔Scribe key↔`Constants` 三处锁死共 11 项）。
- 边界收敛（`KissBoundary` 三层）与七档门禁目前**只有离线证据**：编译期 API 存在性与静态门已绿，运行时行为（灰项文案、动物不可主动、机械族可主动、mutant 被参与层挡住）全部未在游戏内观测，见 `TODO.md` 矩阵新增项。
- 「定义了却没人引用」的反向键检查是有价值的闸门：它在开发过程中抓到 `Mod.cs` 丢失 `SettingsCategory()` override —— 该方法返回非空是设置页出现在「模式选项」里的唯一条件，丢了就等于整个设置面不可达。删掉这条检查前必须先想清楚。
- 产物：`scripts/build-dev.ps1` 出 `dist/dev/Mwah-dev-v<VERSION>-EXP-<shortsha>[-dirty].zip`（当前 commit `89bf4ee`），包内 `version.txt` 三行 = 名称+标签 / build / commit；`dist/` 与 DLL 全 gitignored。
- 未做（阻塞在维护者实机）：游戏内右键、心情数值、非人单位、倒地对象、敌对反应、设置界面换算、存读档含 `MWAH_Kiss` job。矩阵见 `TODO.md`。
- 已知待确认：卸载本模组后，存档里残留的 `MWAH_Kissed*` 记忆与 `MWAH_Kiss` job 会成为未知 Def；具体表现（静默丢弃 or 红字）尚未实测，见 `TODO.md`。

当前目标、开放行动与延后项只记在 `TODO.md`；冷证据在 `OBLIVIONIS.md`，不能覆盖本文件与源码。
