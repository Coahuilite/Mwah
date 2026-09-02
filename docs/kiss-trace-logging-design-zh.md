# Mwah 日志设计：保留意见、依据与更优雅的做法

> 状态：**设计文档，未实现**。代码里的实际打点仍然只有 `Mod.cs` 的三条（装载横幅、`dev: settings saved`、设置落盘失败）。
> 本文记录 2026-09-02 与维护者达成的一条保留意见（一次亲吻的日志行形状），以及为"更优雅"而探索出的备选路线与代价。
> 判据始终是维护者给的那句：**日志以 agent 能还原路径为准**。
> 引擎侧事实（上限、折叠、路径、持久化）逐条经 RimSage 1.6 源码核验，标了文件与行号；本文不复制 `MEMORY.md` 的结论，只写日志这一面。

## 1. 为什么需要日志，以及需要它证明什么

矩阵里有一类行**没有别的证据源**：负证据。"选中动物时菜单里没有亲吻项"、"对睡着的人下令出现灰项并写明原因"——肉眼只能看见"没有"，看不见"哪道门挡的、为什么"。

其余部分都有更强的离线证据源，日志不该重复记账：

| 要证明的东西 | 权威证据源 | 日志的角色 |
|---|---|---|
| 生效设置 | `Config/Mod_<目录名>_MwahMod.xml` + `Constants` 默认 + 读侧 clamp | 只记"何时落盘、改了哪个字段"，不记快照 |
| 心情是否发出、强度、对象、时刻 | **存档 XML**：`Thought_Memory.ExposeData` 持久 `otherPawn` / `moodPowerFactor` / `moodOffset` / `age` / `durationTicksOverride` | 不记结果，只记"我们做了什么决定、依据哪些输入" |
| job 状态跨读档连续 | 存档 XML：`JobDriver_Kiss.ExposeData` 的 `ticksLeft` / `isPassivePartner` / `homeX` / `homeZ` | 一行 `load` 便于对齐时间轴，非必需 |
| 判定链与卡住的位置 | **只有日志** | 主战场 |

## 2. 引擎侧硬约束（决定格式，全部核验过）

| 约束 | 依据 | 后果 |
|---|---|---|
| Player.log **无逐行时间戳**（Unity 只在文件头写时间） | Unity 日志行为；RimWorld `Verse.Log.Message` 只是转发 `Debug.Log(text)`（`Source/Verse/Log.cs:81-90`） | 每行必须自带 `t=<GenTicks.TicksGame>`，否则同类事件无法排序 |
| 全局 **10000 条**上限：`Notify_MessageReceivedThreadedInternal` 对所有 Unity 日志计数，到顶打 `Reached max messages limit. Stopping logging to avoid spam.` 并 `Debug.unityLogger.logEnabled = false` | `Source/Verse/Log.cs:204-213` | 之后 `Log.Message/Warning/Error` 被 `PreventLogging` 全吞（`Log.cs:44-51`）；抓日志先 grep 这句判断是否截断；游戏内日志窗 Clear 会 `ResetMessageCount()` 恢复 |
| **连续相同文本折叠**：`LogMessageQueue.Enqueue` 对 `CanCombineWith(lastMessage)` 累加 `repeats`，到 99 后 `repeatsCapped=true` → 不再写 Player.log | `Source/Verse/LogMessageQueue.cs:19-38` | 行内必须带唯一字段（tick / cid）；且不能靠重复行数统计次数 |
| `Log.Error` 在 DevMode 弹日志窗，且 `DebugSettings.pauseOnError` 会**暂停游戏** | `Source/Verse/Log.cs:128-136` | 运行时异常路径用 Warning，别用 Error |
| 设置文件**只写非默认值**：`Scribe_Values.Look(..., defaultValue, forceSave = false)`，等于默认直接 return | `Source/Verse/Scribe_Values.cs:6-40` | 缺字段=没改过；文件是 diff，不是快照 |
| 设置读失败自带证据：`Log.Warning("Caught exception while loading mod settings data for {folder}. Generating fresh settings. ...")` | `Source/Verse/LoadedModManager.cs:521-546` | 不需要为此自己打点 |
| Player.log 的路径可在运行时取得：`Application.consoleLogPath` | `Source/RimWorld/Dialog_Options.cs:316` | 可以在它旁边写我们自己的文件 |
| `GenFilePaths.FolderUnderSaveData` 是 **private** | `Source/Verse/GenFilePaths.cs:345` | 要数据目录只能从 public 的 `ConfigFolderPath` 取父级，或直接用 `consoleLogPath` |
| `[DebugAction]` 静态方法被 `GenTypes.AllTypes` 全类型扫描发现，**无需 Harmony** | `Source/LudeonTK/DebugTabMenu_Actions.cs:27-45`、`Source/LudeonTK/DebugActionAttribute.cs` | mod 可以往游戏内 Debug 菜单加按钮/地图工具（需 DevMode 打开菜单） |

## 3. 保留意见：一次亲吻的真实 tick 形状

不是 7 个时刻。按代码里的触发链，只有 3 个：

| 时刻 | 同一 tick 内跑完的东西 | 原本误标 |
|---|---|---|
| **T0 下令** | `BeginKiss` → `jobs.StartJob` → `TryMakePreToilReservations` → `SetupToils()` → `ReadyForNextToil()` → `ToilGotoTouch.initAction`（记 home；已贴脸则直接 `ReadyForNextToil()`，否则 `StartPath`） | `order` + `start` + `toil:goto` |
| **T1 到达** | `pather` 停止 → `ToilLockPartner`（`defaultCompleteMode = Instant`，同 tick 过）→ `ToilKiss.initAction` 面对面、`ticksLeft = DurationTicks` | `toil:lock` + `toil:kiss begin` |
| **T2 = T1 + 时长** | `ticksLeft <= 0` → 末 toil → `EndCurrentJob(Succeeded)` → `AddFinishAction` 里 `Settle()` + `RequestReturnHome()` | `end` + `settle×2` |

补充：`Settle` 一次调用算两人 ⇒ 该是一行两列；双方通常同 tick 结束但是两次独立调用 ⇒ 被动方另有一行 `role=passive`（对方接不下 job 时只有主动方一行，`partner=solo`）；已贴脸时 T1 == T0。

**为什么 T0 必须单独一行**：只留终止汇总的话，"下令了但 job 从没结束"（卡寻路、被征召打断、对象被移除）和"什么都没发生"在日志里长得一样。有 order 无 end = 卡在哪一段是明确可读的。

## 4. 更优雅的做法：五条备选与代价

### A. T1 折进 T2（缓冲式 2 行）

`walk=<t1-t0>`、`partner=accepted|solo|already-kissing` 在 T2 全都已知 ⇒ 不必单独占一行。常规 2 行/次（order + 全路径汇总），被动方 1 行。

代价：job 若卡在 kiss 阶段不结束，T1 的信息随汇总一起丢失 ⇒ 缓解：卡住时由 `end`/warn 路径补一条，或接受"order 无 end"这一信号本身已足够定位。

**评价：纯赚，应采纳。**

### B. 闭集事件 + 人类句 + 机器后缀（继承 sibling 协议）

`squeaky_ratkin/docs/logging-protocol.md` 已把这套定型：事件闭集（业务代码不能自由拼串、不能自选级别/可见性）、一行内先英文人类句再 `srdiag fmt=N k=v ...` 固定字段序、可见性与级别独立、once-key 去重、值走 invariant culture、**禁止把本地化文本/pawn 标签/文件系统路径/裸 `Exception.ToString()` 写进协议**（异常只留 type/inner/site/sanitized msg，路径统一替换为 `<path>`）。

对我们的收益：日志可被机器解析（我能直接算出门分布），且"路径不进日志"这条正好覆盖我们的隐私红线。

代价：约 150–200 行样板（注册表 + 格式化器 + once 键），以及"字段序是兼容面"的长期纪律。

**评价：值得，但可以在第二轮做；第一轮先把事件集合定对，格式留 `fmt=1` 余地。**

### C. 环形缓冲 + 按需导出（`[DebugAction]`）

常态**零 Player.log 写入**：事件进内存环形缓冲（例如 512 条），维护者要排查时点 Debug 菜单里的 `Mwah: Dump kiss trace`，一次性写文件并只打一条"已导出到 <path>"。

收益：完全绕开 10k 上限与 99 折叠（导出是文件写，不是 Unity 日志）；玩家日志不被我们污染；trace 档可以放心开。

代价：崩溃/强退丢现场 ⇒ 需要 write-through 的兜底档；依赖 DevMode；"去哪拿文件"要显示给维护者（但按 B 的纪律不能进日志，只能在设置页/Debug 菜单显示）。

**评价：trace 档（菜单判定，量最大价值最低）应该走这条，而不是往 Player.log 里灌。**

### D. 独立文件 `Mwah-trace.log`，与 Player.log 同目录

用 `Application.consoleLogPath` 定位目录后自己追加写。不受 Unity 日志上限影响，格式完全自主，一行一事件。

代价：文件 I/O（3 写/次亲吻，可忽略但要 try/catch + 失败降级为一条 Warning）；多一个"要拿的文件"；写失败本身要可见。

**评价：与 C 组合——C 决定何时写，D 决定写到哪。**

### E. 用游戏内事件日志（`Find.PlayLog`）代替文件

`PlayLog.Add(LogEntry)` 是 public，条目**持久化进存档**（`Scribe_Collections.Look(ref entries, "entries", LookMode.Deep)`），容量 150，玩家在史历页能直接看。

收益：证据跟着存档走，我能从存档 XML 复原"谁亲了谁"，完全不需要日志文件。

代价：自定义 `LogEntry` 子类会成为**存档里的新类型** ⇒ 卸载残留面扩大（矩阵第 25 行本来就要测这个）；150 条与原版事件共享，会被冲掉；`LogEntry` 需要 `LabelReadable`/`Concerns` 等实现。

**评价：作为"结果证据"很优雅，但它替代不了负证据（菜单判定不进存档），所以是补充而非主路径。暂不做。**

## 5. 推荐落地形状（第一轮）

```
[MWAH] kiss.order t=<tick> cid=<doerID@tick> doer=<...> recv=<...> dur=<n>t cd=<a>/<b> reach=<ok|no>
[MWAH] kiss.end   t=<tick> cid=<cid> role=<initiator|passive> cond=<JobCondition> walk=<n>t motes=<n>
                     partner=<accepted|solo|already> settle=<A:+x(SI y) B:+z(SI w)> | skipped=no-mood
[MWAH] kiss.reject t=<tick> layer=<structure|participate|initiate|availability> code=<Key> pair=<idA,idB>
[MWAH] kiss.abnormal t=<tick> cid=<cid> site=<reservation|toil|load> why=<sanitized>
[MWAH] settings.saved t=<tick> changed=<field:old->new,...> gen=<n>
```

- 常规一次亲吻 **2 行**（A 案），被动方多 1 行；`kiss.reject` 只在点击执行后仍失败时出现。
- `code=<Key>` 用**稳定的原因键名**（`Unconscious` / `Burning` / `NoMouth` / `TooYoung` / `SociallyIncapable` / `RitualAbsorbed` / `NoMood` / `Hostile` / `Immobile` / `ImmobilePair` / `CannotReach` / `Busy` / `CooldownPawn` / `CooldownPair` / `Disabled` / `Gone`），译文只在 UI 侧生成 ⇒ 这是"原因码化"那条待决项，日志是它的硬需求方。
- 三档可见性：`off` / `play`（上面这些） / `trace`（菜单级判定，走 C+D，不进 Player.log）。
- 不记：设置快照、结算后的世界状态、DLC 列表（改由 `kiss.*` 行的字段隐含 + banner 一行）。

## 6. 待决策

- [ ] 原因码化（`KissBoundary` 返回 `(layer, code)`）：日志可解析的前提，约 30 行 + 一道"每个 code 都有双语键"的静态门。**建议做。**
- [ ] `MWAH.LogLevel` 三档设置项（进 Scribe ⇒ 多一个字段与一对双语键）vs 编译符号。**建议设置项**，理由：矩阵在 dev 包里跑，编译期那条路对可分发版本永远是哑的。
- [ ] 第一轮是否就引入 B 的闭集注册表。**建议不引入**，先用上面的固定行式跑一轮矩阵，字段序稳定后第二轮再升级为 `fmt=1` 协议，避免把未验证的格式锁成兼容面。
- [ ] 是否引入 C+D（环形缓冲 + Debug 菜单导出到独立文件）。**建议与第一轮同做**，因为 trace 档没有它就只能灌 Player.log。
- [ ] E（`PlayLog` 条目）留待卸载残留测试（矩阵第 25 行）有结论后再议。

## 7. 本文与仓库红线的关系

- 零 Harmony：`[DebugAction]` 与 `FloatMenuOptionProvider` 一样靠原版反射发现，不需要补丁。
- 日志文本硬编码英文、`[MWAH] ` 前缀、不本地化（`AGENTS.md` 硬边界）。
- 文件系统路径**不进日志**（B 的纪律 + 仓库零绝对路径红线一致）；导出路径只在设置页/Debug 菜单显示给本人。
- 玩家可见文案才走 Keyed；日志键名稳定优先。
