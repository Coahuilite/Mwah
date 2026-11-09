# 隐式状态机调查与改造判断

依据：主协调者基于 `bedea41` 的静态源码调查，改造范围已获维护者核对采用。实际实施／编译／原生证据分别由 [tasks](tasks.md)记录；原生效果仍由维护者确认。

## 不用一个 enum 取代所有事实

原生 Toil 拥有执行进度；模组拥有完成事实、绑定身份和收尾幂等闸；角色及保存结果不是同一个维度。推荐只读逻辑阶段和集中转移入口，不引入一套与原生 Job 并行推进的 FSM。

| 状态域 | 调查基线的状态及所有者／寿命 | 影响与采用方向 |
| --- | --- | --- |
| 双方入场与绑定 | `KissUtility.Start` 先启动被动 Job，再启动发起 Job；双方确认后绑定 loadID、登记冷却。`JobDriver_Kiss.MatchingPartner` 还会补写旧档双方绑定 | 高优先级：保持双入场／失败回滚顺序；把旧档补绑显式化，查询不要隐式修改对方 Job；保留启动帧宽限 |
| 接近／等待／表演／完成 | 两 JobDriver 的 `CurToilIndex`、`completed`、`finished`、`ended` 共同决定结果；配对由发起方推进倒计时 | 高优先级：给出统一只读阶段／占用语义，集中完成和收尾边界；不重排 Toil，不把角色与成功混成单状态 |
| 返程与释放 | `KissReturn` 的 `returnTicks` 持久化；`ReleaseOnce` 是重建后的闭包局部闸；`WalkTally` 不保存 | 与 Job 边界一起整理：完成后不占用、不重结算；读档首 tick 仍能释放旧预订；保留重试／时限和原生 finalizer，不引入返程全局队列 |
| 导演台点选会话 | `KissDirector.Active`、generation、原生 Targeter 与窗口 `PickSlot`／头像引用分属不同 owner | 优先核对会话归属：旧 cleanup 不清新一跳；关窗只取消本窗口拥有的选点，不误停别的 Targeter |
| 数值编辑及设置持久化 | `MwahSettingsView` 拥有编辑缓冲和 pendingCommit；`MwahMod` 拥有规范值保存代次、savePending、queuedWrite、dueAt | 两个状态机而非一个 dirty：保留提交→内存生效→防抖保存→失败待重试。只收口命名与入口，不顺手换保存机制 |
| 冷却／自主调度／跨局重置 | 冷却按 readyAt tick 推导，自主撮合按 nextCheckTick 门控；`KissTicker` 在建局／读档清理约定的静态残留 | 记录状态和寿命即可，不添加 Ready/Cooling 实例状态，不改成持久化冷却或新调度系统 |
| Def 登记及诊断投影 | 扩展 registry／VNPE 缓存在 Def 加载期建立；KissTrace 的阶段字符串是观测，不是 Job 权威 | 随共享内容迁移明确有效登记集合；不让诊断日志控制执行或把缓存升级成第二套业务状态 |

## Job 的真实转移

```text
准入检查 → 启动双方／单方 Job → 确认入场及身份 → 接近
配对提前到位：在原表演 Toil 内等待另一方，不先扣时
双方在位／物体台位持续合法 → 表演倒计时 → completed
表演收尾：finished 幂等闸 → 释放预订 → 合格时结算
返程 → 原工作结束；必要时由既有原生 finalizer 安排普通 Goto
```

入场失败需要撤销已入场的本次 Job，不登记成功冷却。完成前失效沿既有失败路径离场或进入未完成返程，不发完成奖；暴力／新命令不能被清理逻辑插入返程抢走。已完成返程不再依赖原目标，也不继续占用亲吻资格，新请求仍检查冷却。

### completed 和 finished 不可合并

源码中，`completed` 表示倒计时走完；`finished` 在表演 finish callback 入口即置真，即使随后因 `completed=false` 而不发奖。它不是“奖励已经发出”。若把二者合并成 Success／Done，会把取消和完成混淆。

实现使用 `KissActivity.Phase` 推导只读 `KissPhase`，两个 driver 的 `IsKissing` 从该视图判断；`MarkPerformanceCompleted` 收口完成事实，`TryEnterFinishAction` 先置幂等闸再执行原收尾。保存键保留原名，不另序列化 phase。角色由既有 TargetC／isPassivePartner 约定保留，双方 job loadID 不替换成模糊的“对方正忙”。

### 查询中的旧档修复是隐藏转移

基线的 `MatchingPartner` getter 在缺少 `partnerJobId` 时检查互指及相同 startTick，并写双方 ID；接近期 fail condition 和表演期都会读它。迁出到显式绑定／恢复入口时必须保留这两个调用阶段和准备条件，不能只在表演阶段恢复，也不能在 Scribe 引用尚未就绪时强行补绑。

本轮把写入移到 `RestoreLegacyBinding`，接近和表演两条调用链都先恢复再查询；`BindPartner` 幂等且拒绝覆盖已绑定的不同 ID。`MatchingPartner` 只读，不再暗中修改另一 Job。`KissReturn` 的持久化、首 tick 释放及 finalizer 保留实现。

现有 `finished` 缺省值依赖 `completed && CurToilIndex > PerformanceToilIndex`；`PerformanceToilIndex=2`／`ReturnToilIndex=3` 与保存的原生进度相关。第一轮整理保留这些索引和旧字段，不通过插入新 Toil 实现“等待搭档”。

## 导演台：已收口关窗遗漏，原生交互待确认

观测：窗口 `StartPick` 设置 `picking`，但 `QuickSend` 直接启动 `QuickChain`，没有登记相同的窗口点选状态；`PostClose` 只有 `picking != None` 才调用 Stop。源码因此存在“快速链式点选仍 Active，而关窗条件认为没有选点”的路径。

基线影响推断：关窗可能遗留没有面板入口的 Targeter 会话。调查时规格未单列快速链式关窗；本轮已按原关闭清理意图补录 KISS-01。原生输入回调的实际交互由维护者确认。

Herdr 核对确认了这条源码遗漏，实施采用“本窗口拥有的所有点选随关窗取消”，不改成独立后台续选。

当前只有一个导演台窗口，现有项目内点选入口均由它发起。实现中 PostClose 不再以局部 `picking` 作为是否收点的代理，而总是交给已经按 `Active` 短路的 `KissDirector.Stop()`；保留 generation 防止上一跳 cleanup 清掉新一跳。不引入 token／session 类、不禁用原本可用的点选按钮，也不直接无条件停止其他功能的全局 Targeter。跨局重置由 `ResetForNewGame` 清 Active，下一次 Begin 才递增 generation；将来确有独立会话来源再增加 owner token。

是否存在特定 GUI 事件上下文限制没有证据支持，不预先添加“等待 Repaint 再停”的队列或前提。实际关窗、连续点选和 Esc／右键效果由维护者按既有 UI 路径确认。

## 设置：保留两个边界

编辑缓冲只在 Enter、失焦、隐藏、切页或关闭等既有入口提交；普通防抖保存只写规范值。`queuedWrite` 区分保存调用来源／防止提前 Close 编辑，并不是 dirty；请求代次与持久代次决定未保存事实，失败保留待保存状态。

0.4.x 保留编辑缓冲和保存尝试／失败路径，仅以 `MarkSettingsPersisted` 收口原写入成功记账，去掉 Flush 中的重复记账；不改 tick／实时钟、防抖或失败重试含义，不引入后台保存任务。依据仍为 [USE-01／02](../../specs/settings-use/spec.md)。

## 改造边界与验收

优先实施 Job／绑定／返程的局部显式化和导演台会话归属；设置以保持原语义、清楚命名和入口为限。其他条目完成状态／寿命记录后保留实现。

验证集中在启动帧、第二方入场失败、等待搭档、完成与取消、完成后新请求、返程中替换、旧执行中存档及快速点选关窗。维护者负责原生结果；既有构建和源码／包检查由实施者负责。不新增单元测试／换名脚本，不重做已经通过的识智奖励数值观察。
