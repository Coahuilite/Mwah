# OBLIVIONIS

冷归档。只在出现历史冲突、被复活的话题、或维护者明确要求时读取；本文件内容不能覆盖 `MEMORY.md`、`AGENTS.md` 与源码。

条目格式：`## <日期> — <主题>` + `source:` + `reason:` + 压缩后的结论。不存原始日志、会话叙事或已完成的任务清单。

## 2026-09-02 — 允许性双开关（allowHostileTargets / allowMoodless）

source: 提交 `01d0c44`（把 `allowNonHumanlike` 改名为 `allowMoodless`，判据从种族换成运行时 `needs.mood`）与其后的门禁滑条改动。
reason: 两个布尔开关各自表达"范围放宽到哪一步"的一个切面，组合起来仍然表达不了玩家真正想要的那条线（只撮合殖民地）。七档 `KissScope` 把同一维度收成一条单调阶梯后，`allowHostileTargets` 恰等于"档位 ≥ 3"、`allowMoodless` 恰等于"档位 ≥ 2/5"，两者成为真子集而被删除。改名那条结论（判据必须是运行时 need 而不是 `RaceProps.Humanlike`）**仍然有效**，只是搬进了 `KissBoundary.HasMood` 与门禁的档位定义里。
replaced by: `Source/Mwah/Actions/Kiss/KissScope.cs` 的七档门禁（`MwahSettings.pairScope`）。
status: 已替代。
evidence: 出厂默认 `KissScope.Everything` 下，门禁判断恒真，与被删前的两个开关同时为 true 的行为逐位一致（结构层不再有任何允许性过滤）。
复活条件：若将来需要表达"跨档组合"（例如要自家动物但不要袭击者），单滑条表达不了，届时按第二维度加控件，而不是复活这两个布尔项。
