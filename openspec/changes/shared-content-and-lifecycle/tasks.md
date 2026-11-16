# 根布局与文档收敛：当前任务

## 目标与授权

维护者于 2026-11-16 确认：共同内容移到模组根，保留双原生构建及 VNPE 条件隔离；随后整理规格、记忆、README 和 docs，最后处理 lessons 与今日 changelog。追加要求按所给规格工作流与 LTS-0002 校准职责，独立审阅，承接必要事实后把退役文件放 docs/archive，由维护者手动删除。

本次是既有单源／加载保证下的目录及文档整理，不改变玩法、保存含义、编译引用或公开身份，不新增规则 delta。禁止新增单元测试和等价换名用例，不运行或安装游戏，不代删历史资料。2026-11-17 先追加授权检查、推送、main 合入／主分支设置及干净渠道构建，随后授权正式 GitHub 发布和 changelog 更新；Steam 上传与工坊页面／标签操作继续由维护者手动完成。

## 实施与证据

- [x] 共同 Def／语言 25 文件移至根 Defs／Languages；直接逐文件对账无意外正文变化，仅两条 XML 路径注释更新。C# 修改均为路径／历史运行时说明注释，未改业务逻辑。
- [x] LoadFolders 每版保留 `/`＋当前 minor＋条件 VNPE 根；暂存按内容白名单，不复制整个仓库；源与包均拒绝旧 Common／minor 内容副本。
- [x] 既有 verify-local 双目标 × Dev／Steam／GitHub 全过、零警告／错误；语言、Def／天意、精确三出口、条件隔离、DLL 与框架、设置及版本门保留。
- [x] 真实 pack-dev -Zip 产出 35 文件的根布局包，ZIP 仅一个 Mwah 顶层；已检查有效内容选择及实际包隐私。包内无 art、Source、docs、archive、缓存或引用 DLL。最新文档与新增归档的最终检查另见收尾项。
- [x] 现行规格入口、MEMORY Local map、双语 README／changelog、架构／诊断／发布指引及渠道记录收敛为自持来源；changelog 日期 2026-11-16，仍标未发布。
- [x] 旧调研、过程、已采用 delta、执行 brief、历史检查点与 OBLIVIONIS 退役；当前所需的身份、状态选择、发布证据和未尽加载确认先留在现行来源，日常入口不依赖 archive。
- [x] 独立审阅及基于原材料的纠正完成：修复失效证据路径、发布事实唯一存档、残留归档读取方；撤回额外的保存字段禁令，区分 1.6 Mono 与 1.7 Waylith，纠正日志静默和未选择 Common 的解释。
- [x] 最后收敛 LESSONS，保留两条活跃经验与一个当前覆盖点；338 条本地 Markdown 引用可达，现行正文无指向 archive 的链接，源码／脚本无归档读取依赖。全部新增文件纳入暂存检查，cached diff 与工作树／实际 35 文件包隐私审计通过；未为纯文档重跑未受影响的完整构建。

实际包身份及维护者对 Common／根布局候选的确认以 [native-acceptance](native-acceptance.md)为准。正式渠道状态由 docs/release-status.md 负责，不以开发出包推定发布。

## 2026-11-17：上云自检与工坊兼容清单

- [x] 维护者已手动删除 docs/archive；已核对实际删除并同步暂存范围，不恢复已删除的档案。
- [x] 收到维护者“实际测试内容，可用”的反馈，关闭根布局待启动事项并更新直接读者。
- [x] `privacy-audit.ps1 -FullHistory -PackageDir dist/dev/Mwah` 通过：工作树、提交信息、178 个可达 revision、身份、媒体元数据及实际 35 文件包均已检查。仅有既已接受的两处历史 UNC 项（MEMORY.md、scripts/verify-local.ps1），没有新增命中、豁免或历史改写；未变图像复用目检依据。
- [x] 中英工坊兼容性板块仅列 RimWorld 1.6／1.7 Core、五款官方 DLC 及 VNPE 三出口；移除功能、供电、资源和运行范围说明，编辑约定同步采用该口径。版本与代码未改，未操作线上页面。

## 协作与审阅依据

主协调者独占运行内容、脚本、规格、记忆、Git 和最终集成。文档初稿由 Herdr `t_354d5eb4659f`／`docs-root-cleanup-01` 提供；观察到完成回读后仍重复收尾检查，主协调者停止该轮，终端确认停止写入，以仅交付请求 `docs-root-handoff-02` 接收已有成果。没有重放实施；报告与文件分开接受，初稿中的不当限制和跨版本诊断表述未被当作合同。

独立只读审阅者为未参与编写的 `t_edb7b7f17732`，请求 `spec-memory-review-01`。两者均用 OMP `bailian/qwen3.8-flash`、effort medium、group `mwah-04-plan`，均已停止写入，实际回报已消费。此次独立审阅是维护者追加的本次范围，不形成以后每次改文档都须重审的流程。

校准依据为维护者指定的 `RimWorld_Mod_Specification_Workflow_zh.md` 与 `LTS-0002-context-owned-memory-system.md` 完整原材料；它们的方法已本地化，不是日常外部依赖。LTS-0002 在来源中仍标 Candidate，本地协议沿已获授权的采用范围校准，不自动改变上游状态或项目权限。规格唯一正文仍为 openspec/specs；TODO 保留项目级权威，MEMORY 只承载协议及必要知识，LESSONS 保存活跃经验与当前覆盖点，不另建 STATE／START 或强制冷记录。

## 2026-11-17：main 集成与干净渠道构建

本地整理、候选确认及上云隐私／兼容文案工作已完成。清理提交 da944de 包含 25 条共同内容迁移、69 条退役文件删除及其现行替代资料，不只是追加文件。main 独有的 0.3.0 发布收尾已对照：工作流保留 `target_commitish: main`，仍需的渠道／采用事实已由现行发布记录和候选记录承接；合并未恢复已退役文件。

主协调者通过 [PR #3](https://github.com/Coahuilite/Mwah/pull/3)完成现有发布门、版本分支推送、PR squash 集成及构建。最终 main 树已与获准源分支对账一致，旧路径离开、新路径加入；干净 main 的 GitHub／Steam flavor 分别构建并通过实际包审计。旧玩法与识智观察继续有效。

## 正式发布与收尾

在维护者后续明确授权后，v0.4.0 固定于已验收 main 提交 ce2afac。Release CI 成功并创建正式 GitHub Release；实际下载 ZIP、版本／DLL 身份、完整清单及隐私已核对，双语更新说明已应用。Steam flavor 以同一提交构建的包已固定交付，页面与变更文案位于运行目录外；维护者于 2026-11-17 确认已手动上传并完成工坊页面／标签更新。

实际发布时间、渠道身份、完整交付位置和 CI 证据由 [release-status](../../../docs/release-status.md)唯一负责。双语 changelog、README 及当前工作状态已按实际发布结果收尾；此后仅有文档变更，不改发布 tag、包或既有原生采用结论。双渠道发布与本轮项目整理均已完成，不保留 Steam 待上传责任。

按维护者收队要求，已核对 Herdr `mwah-04-plan` 的六个执行终端均为空闲，关闭其独占的 `w33` 工作区及三个标签页；后续工作区清单已确认该工作区离开。既有报告已消费，不重放任务；保留原 relay 记录作为历史依据。
