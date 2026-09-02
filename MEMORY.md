# MEMORY

## 当前耐久状态

- RimWorld 1.6 模组，`packageId` `coahuilite.mwah`，产品版本 **0.1.0**（csproj `<Version>` 为主源，About `<modVersion>` 跟随）。尚未发布、无远端仓库、无创意工坊条目。
- 功能面：**仅 live map、仅单机、零 Harmony**。选中任意 pawn 右键另一 pawn → `kiss`；能走的一方走过去，双方 `FaceTarget` 相对、贴脸（`PathEndMode.Touch`）、按间隔抛原版爱心，结束后各自 `JobDefOf.Goto` 回原位。有 `needs.mood` 的按**自己**的 `SocialImpact` 拿心情，没有的什么都不加。
- 明确不做（不是待办）：世界地图/商队途中、RimWorld Multiplayer 同步、AI 自主亲吻（`InteractionDef` 二期候选）、真·贴合亲吻动画、Downed 者当发起方（需 Harmony 放开原版闸门）。
- **不做 junction**：`Mods/` 与 `Mods/*` 一律不建、不校验、不假设存在；复制模组与实机测试由维护者本人执行（2026-09-02 维护者指令）。

## 命名决定（2026-09-02 定稿）

- 品牌短名 **`Mwah!`**，全称 **`Every Pawn Kisses Each Other`**，`packageId` = `coahuilite.mwah`，前缀 `MWAH_` / 键前缀 `MWAH.` / 日志 `[MWAH]`。工作区目录名保留 `every_pawn_kiss_each_other/`，工程身份一律 `Mwah`。
- 候选与淘汰理由（已核实部分标注依据）：`KISS_` 全称最直白但作为通用前缀撞车概率与工坊检索噪音最高（**注意：并非原版占用**——rimsage 核实原版无任何含 `kiss` 的 defName，仅 `Tales_DoublePawn_Relationships.xml` 有 "deeply kissing" 文案）；`PECK_` 是真词"轻吻"但被"鸟啄/轻敲"次要义稀释，且 `PECK_` 无法由全称首字母正向拼出；`CHUU_`（ちゅっ）对中文/ACG 受众有效但英文玩家不直觉；`XOXO_` 的 X 与"处决/取消"视觉混淆且含我们未实现的拥抱语义；`SMOOCH_` 前缀过长。
- 原版 token 占用核查结论：`Mwah / Peck / Chuu / Snog / Xoxo / Smooch` 在 1.6+Odyssey 的 defName、`Defs/Core/Names` 人名部件中**均无占用**（rimsage 检索为空）。
- `MWAH` 明确**不**做全称首字母展开，它是拟声词；全称负责"指示所有小人互亲"的语义，缩写负责菜单与日志里的嘴声。

## 原版能力边界（RimSage 1.6 + Odyssey 源码核验，2026-09-02）

改实现前必读：这几条是本模组形态成立的依据。

- **右键入口自动注册**：`FloatMenuMakerMap.Init()` 对 `typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract()` 反射实例化 → MOD 的 provider 无需 Def/XML/patch 即进菜单链。基类闸门：`Drafted/Undrafted/Multiselect/RequiresManipulation/MechanoidCanDo/CanSelfTarget/CanTargetDespawned/IgnoreFogged` + `SelectedPawnValid/TargetThingValid/TargetPawnValid/Applies/GetOptionsFor(Thing|Pawn)`。
- **`SelectedPawnValid` 的坑**：base 首句用 `MutantDef.whitelistedFloatMenuProviders` 静默屏蔽未登记的 mutant/亚人 → 本模组 override 时**故意不调 base**；`MechanoidCanDo => true` 因此只是声明意图。
- **原版硬闸门（provider 层绕不过）**：`ShouldGenerateFloatMenuForPawn` 在遍历 provider 前剔除「不在当前地图 / `Downed` / `Deathresting` / `Lord.AllowsFloatMenu` 否决」；`GetOptions` 首行要求点击落在 `Find.CurrentMap` 内 → **倒地者不能当发起方**，但可当目标。
- **`ability` 路线被否决的依据**：`PawnComponentsUtility.AddComponentsForSpawn` 只在 `Humanlike || IsMechanoid` 时建 `pawn.abilities`；Odyssey 的 `FleshType.Drone`（`isOrganic=false`）也没有 `interactions` tracker。故 Ability/`InteractionDef` 都不能覆盖"所有 pawn"，只有 FloatMenu provider 可以。
- **贴近/相对**：`pather.StartPath(target, PathEndMode.Touch)`、`SocialInteractionUtility.BestInteractableCell/IsGoodPositionForInteraction(≤6 格 + LineOfSight)`、`rotationTracker.FaceTarget(...)`、`GenAdj.AdjacentTo8WayOrInside(Thing,Thing)`（多格体型正确）。1.6 **已无 `Pawn.CanMove`**，移动能力读法是 `health.capacities.CapableOf(PawnCapacityDefOf.Moving)` + `RaceProps.doesntMove` 特例。
- **爱心特效**：就是 `FleckMaker.ThrowMetaIcon(cell, map, FleckDefOf.Heart)`（`JobDriver_Lovin` 常量 100 tick）。`Heart` 是 **FleckDef**（`Things/Mote/Heart`，`MetaOverlays`），不存在 `Mote_Heart`。
- **双人锁定**：`JobDriver_Lovin` 的镜像 job 手法（给对方 `StartJob(同 def, 自己, InterruptForced)`）；`TryTakeOrderedJob(job, JobTag.Misc)` 用于回原位这类礼貌请求；`Pawn_JobTracker` 在 job 成功后自动接 `Wait_MaintainPosture`。
- **心情缩放**：`Thought_Memory.MoodOffset() = stage.baseMoodEffect × moodPowerFactor + moodOffset`；`Thought_Memory.durationTicksOverride` 可逐实例改时长；`ThoughtDef.DurationTicks = durationDays × 60000`（0.25 日 = 6 游戏时）。原版 `Pawn_InteractionsTracker.AddInteractionThought` 是 public static 但乘的是**对方**的 `SocialImpact`，且前置要求 `Talking` 容量与 `interactions` tracker → 本模组自己写，取**自己**的 `SocialImpact`。
- **不炸的边界**：`SkillNeed_BaseBonus.ValueFor` 对 `pawn.skills == null` 返回 `1f`，且 `SocialImpact` 标了 `neverDisabled` → 无技能单位取该属性安全。
- **社会语义副作用开关**：`Thought_MemorySocial.Init()` 在 `ThoughtMaker.MakeThought` 内被调用并写入 `opinionOffset = stage.baseOpinionOffset` → MakeThought 之后再乘倍率才有效；`ShouldDiscard` 要求 `otherPawn != null && opinionOffset != 0`。
- **单位常量**：`GenTicks.TicksPerRealSecond = 60`、`GenDate.TicksPerDay = 60000` → 1 游戏小时 = 2500 tick。
- 灰项惯例：`new FloatMenuOption(label, null, priority…)`（action 传 null 即禁用），原因写进 label 括号；可用 `FloatMenuUtility.DecoratePrioritizedTask` 标"会抢占"。菜单优先级有现成的 `MenuOptionPriority.InitiateSocial`。

## 工程决定

- 冷却（单人 + 成对）为**会话内内存态**，不落盘、不占 tick，只在结算时顺带清过期项；读档后归零是接受取舍（与 `let_me_gnaw_on_you` 的 `CooldownManager` 同一口径）。
- 默认 `changeOpinion=false`：用普通 `Thought_Memory`，不写好感度、不喂原版恋爱链；开启后切换到 `Thought_MemorySocial` 变体。
- 心情发放只由**发起方**结算一次，被动方只负责自己的回原位；`isPassivePartner` 是这条不变量的载体。
- 对象倒地/无 jobs 接不下镜像 job 时，发起方仍独自完成亲吻（爱心照冒、心情照发）；主动方不设"对方必须在亲吻 job 中"的失败条件，只有被动方设。
- 设置生命周期：内存即时生效 + 磁盘 0.35 s 防抖合并 + 关窗强制 flush + 失败保留 dirty 并 2 s 重试（依据 `modding_documents/draft/modsettings-value-lifecycle-decision-tree-zh.md`）。
- 时长存储单位统一 tick；`thoughtDurationGameHours` 按 1/4 游戏小时量化，避免脏小数进配置文件。
- 本仓库的骨架、四件套记忆文件与四份脚本已抽象上收为通用指南：`modding_documents/RimWorld_Mod_RepoInit_AgentMemory_And_Packaging_Guide_zh.md`（占位符版，无项目身份）。同类新工程先读它，不要重新发明。注意 `modding_documents/` 本身不在任何 git 仓库下，改它不产生提交。
- 骨架照 `RimWorld_Mod_Skeleton_Guide_bilingual.md` Level 2 + `let_me_gnaw_on_you`/`squeaky_ratkin` 现行做法：版本化 `1.6/`、`.slnx` only、`net472` + `Krafs.Rimworld.Ref 1.6.*`、语言目录用 `ChineseSimplified`（`LanguageDatabase` 硬编码名录里的真实名字）。

## 验证状态

- 已绿（离线）：`dotnet build` Debug/Release 均 **0 警告 0 错误**；`scripts/verify-local.ps1` 14 项全 `[ok]`（8 个 XML 良构、Keyed 中英各 38 键且集合一致、C# 引用的 38 个键双语齐备、DefOf↔defName 无孤儿、`driverClass` 与 `namespace.type` 一致、DLL 含 12 个关键符号、DLL 无 Harmony 符号、版本与 packageId 三处一致、无绝对本地路径）。
- 「定义了却没人引用」的反向键检查是有价值的闸门：它在开发过程中抓到 `Mod.cs` 丢失 `SettingsCategory()` override —— 该方法返回非空是设置页出现在「模式选项」里的唯一条件，丢了就等于整个设置面不可达。删掉这条检查前必须先想清楚。
- 产物：`scripts/build-dev.ps1` 出 `dist/dev/Mwah-dev-v<VERSION>-EXP-<shortsha>[-dirty].zip`（当前 commit `89bf4ee`），包内 `version.txt` 三行 = 名称+标签 / build / commit；`dist/` 与 DLL 全 gitignored。
- 未做（阻塞在维护者实机）：游戏内右键、心情数值、非人单位、倒地对象、敌对反应、设置界面换算、存读档含 `MWAH_Kiss` job。矩阵见 `TODO.md`。
- 已知待确认：卸载本模组后，存档里残留的 `MWAH_Kissed*` 记忆与 `MWAH_Kiss` job 会成为未知 Def；具体表现（静默丢弃 or 红字）尚未实测，见 `TODO.md`。

当前目标、开放行动与延后项只记在 `TODO.md`；冷证据在 `OBLIVIONIS.md`，不能覆盖本文件与源码。
