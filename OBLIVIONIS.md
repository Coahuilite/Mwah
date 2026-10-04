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

## 2026-10-04 — 0.1.0 开发周期叙事与作废矩阵行（收尾轮降噪归档）

source: TODO「下一会话入口」的完成叙事行（2026-09-21 收尾轮 / 09-22 面板化与二次修正 / 09-28 addon 天意表与停火窗口 / 09-30 日志面与首启爆雷 / 10-04 换座死锁），MEMORY 命名候选淘汰理由与"checkOverrideOnDamage 旧依据证伪"叙事，及下列实机矩阵行。
reason: 已完成的工作叙事与已作废的检查项不再指导未来行为；其耐久结论均已吸收进 MEMORY/AGENTS/代码注释，此处只留压缩存根供历史核验。
status: 已归档。

### 完成叙事（细节以 git log 为准）
- 09-21 收尾轮：删 `Mwah.slnx`（此后无解决方案文件，IDE 直开 csproj）；判定链整理（一次判定拿原因、Settings 永不 null、派发上限、边界不互跑）；修 Steam/GitHub 渠道编译并加三渠道构建门；心情重标 +5/1日/×0.5 + Psychopath/Inhumanized 归零；HUD 与心情阶梯两份调研。
- 09-22：导演台面板化（非模态、UI.screen 逻辑画布、可拖重摆）+ 二次修正（标题行高、WordWrap 帧末、心形半颗）+ 设置页数值框 + 亲墙首发。
- 09-28：addon 层重构 + 天意表 `MWAH_FateDef` + 实例旁白（零补丁）；停火窗口三件套（双 job 同帧起、job 内回程、completed 旗标离场）+ 战斗闸 + `Notify_DamageTaken` 伤害自毁闸。
- 09-30：日志面（无条件启动横幅 + 运行时诊断开关）；实机首启爆雷两处当日修复（FateDef 命名空间、设置页跨帧测高）。
- 10-04：换座死锁修复（`Usable` 拒绝对方脚下格）+ 静默出口全量补日志；上云（`Coahuilite/Mwah` public，MPL 2.0，CI/隐私门/README/CONTRIBUTING 齐备）。

### 作废矩阵行（含作废理由）
- 崩溃判别 A/B/C 与"递归回归"：真凶 2026-09-04 已定（finish action 同步起 job 递归）并修复（队列化 + 幂等闸）；Steam 覆盖层/Defender 假设当日已证伪。判别类行完成使命。
- "卡死定位"：操作指引而非检查项，已并入 MEMORY「调试器选型」与日志面条目。
- "心情开关收紧（关闭'纳入没有心情的生物'）"：该设置项随七档门禁改造删除，行指向不存在的控件；无心情单位的可见性由门禁档覆盖。
- "敌对与派系：对袭击者下令→可能立即反击属预期"：与停火窗口裁定直接矛盾（现在接近+表演+离开全程不反击），由「停火窗口」「接近段被亲者不反击」两行取代。
- "不互相绕圈：只有 thingIDNumber 较小的那方走位"：定台模型下双方各走自己的台格，thingID 单侧走位是 Touch 追逐时代遗物；由「左右对向」「路人挤台」覆盖。

### 命名候选淘汰理由（定稿见 MEMORY「命名决定」）
`KISS_` 检索噪音最高（原版 defName 实无占用，rimsage 核过）；`PECK_` 被"鸟啄"义稀释且拼不出全称；`CHUU_` 英文玩家不直觉；`XOXO_` 含未实现的拥抱语义且 X 视觉歧义；`SMOOCH_` 过长。`Mwah/Peck/Chuu/Snog/Xoxo/Smooch` 在 1.6+Odyssey 的 defName 与人名部件中均无占用。MWAH 不做首字母展开，是拟声词。

### 伤害打断旧依据（结论已修正进 MEMORY「停火窗口」条）
2026-09-28 曾记"`checkOverrideOnDamage=Always` 让打一枪即散场"——只对右键路径成立且带 180 tick 迟滞，对 playerForced 路径完全无效；当日查证改写为 driver 钩子方案。留此存根警示"未验证的机制断言"。

### 分支沿革
2026-09-02 建 `dev`（此前 9 条提交落 main，fast-forward 对齐，未动历史）；2026-10-04 起 `dev` 退役，由版本分支 `0.1.x` 接替（内容与退役时的 dev 相同），main 收缩为 release-only。
