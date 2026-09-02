# AGENTS.md — Mwah! (Every Pawn Kisses Each Other)

> 本文件供 AI agent 使用；人类读者请读 `About/About.xml` 与 `docs/`（尚未建立）。
> 这里只保存每次会话都必须知道的项目身份、记忆协定与边界；不保存会话叙事。

## Project identity

- 项目：RimWorld **1.6** 模组 **Mwah! - Every Pawn Kisses Each Other**。永久品牌短名 **`Mwah!`**，全称 **`Every Pawn Kisses Each Other`**（注意 `-es`：2026-09-02 定名时顺手修掉了旧写法 `Every Pawn Kiss` 的语法瑕疵）。两者不得翻译、归一化或改写。
- 中文显示名**未定**：Keyed 分类名暂用「每个小人都互相亲吻」，未经维护者确认不得升格为正式品牌（见 `TODO.md`）。
- 工作区目录名保留历史值 `every_pawn_kiss_each_other/`；工程身份一律是 `Mwah`（命名空间/AssemblyName/`Mwah.slnx`/`Source/Mwah/`）。
- 永久 `packageId`：`coahuilite.mwah`（发布后不可改；`Constants.ModId` 与 `About/About.xml <packageId>` 由 `scripts/verify-local.ps1` 强制一致）。
- C# 命名空间：`Mwah`（单层，不按功能开子命名空间）。
- Def 前缀：`MWAH_`；Keyed 键前缀：`MWAH.`（点分层，PascalCase 段）。
- 日志前缀：`[MWAH] `，正文硬编码英文，不本地化、不用占位键。
- 版本主源：`Source/Mwah/Mwah.csproj <Version>`；`About/About.xml <modVersion>` 跟随；dev 包标签加 `-EXP`。

## Project philosophy

- **纯娱乐、零门槛**：能选中就能亲。不限阵营、不限种族、不要求可控、不要求征召。
- **零 Harmony 是硬约束**：不引用 `Lib.Harmony`、不声明 `brrainz.harmony` 依赖、不做任何运行时补丁。触发面用原版 `FloatMenuOptionProvider` 反射发现，行为用自定义 `JobDriver`。需要补丁才能解锁的能力（星图右键、Downed 者当发起方）一律视为 out of scope，不为此引入 Harmony。
- **单机专用**：不实现 RimWorld Multiplayer 同步；跨平台联机一致性不是本模组的责任面。
- **仅 live map**：世界地图/商队途中不做（未 spawn 的 pawn 已被移除 `pather`/`rotationTracker`/`jobs` 组件，物理上无法亲吻）。
- **收益保守**：默认只给心情，不改好感度、不接浪漫链；`changeOpinion` 为显式 opt-in。

## Memory protocol

每个非平凡会话：

- 先读 `MEMORY.md` 再声称了解项目上下文：只存已确认的耐久事实、决定、约束与证据指针。
- 先读 `TODO.md` 再继续工作：只存当前目标、开放行动、阻塞与明确延后。
- 仅在历史冲突或明确请求时读 `OBLIVIONIS.md`：冷归档证据不能覆盖现行合同。

维护边界：

- `MEMORY.md` 只增耐久事实，保持紧凑；已完成工作的细节留在提交信息与脚本输出里，不扩容记忆。
- 文档编辑本身不构成记忆事件；只有耐久事实、任务面、阻塞或归档状态变化才更新。
- 不存会话叙事、原始日志、已完成的验证矩阵、提交链或发布检查表。

## Boundaries and red lines

- **永不做 junction**：不把 `Mods/` 指向工作区，不校验 junction，也不假设游戏 Mods 目录存在。模组复制与实机测试由维护者本人执行。
- 外部生效操作（push、建仓远端、tag、发布、创意工坊上传）需逐次明确授权；本仓库当前无远端。
- 仓库、文档、产物与可达历史中不得出现个人本地状态、展开的绝对路径、日志片段、凭据、`PublishedFileId.txt` 值；`scripts/verify-local.ps1` 有绝对路径红线检查。
- 玩家可见文字必须同时进 `1.6/Languages/English/Keyed/` 与 `1.6/Languages/ChineseSimplified/Keyed/`，键集合一致；Def 文本走 `DefInjected`。
- 时长一律以 tick 存储，界面必须同时显示 tick / 现实秒 / 游戏小时（`MwahTime` 是唯一换算入口，不使用游戏分钟）。
- 未经维护者同意，不新增运行时依赖、不新增 Harmony、不改 `packageId`。

## CURRENT STRUCTURE

```text
every_pawn_kiss_each_other/
├── AGENTS.md / MEMORY.md / TODO.md / OBLIVIONIS.md
├── About/About.xml
├── LoadFolders.xml                     <li>/</li><li>1.6</li>
├── Mwah.slnx                           只用 .slnx，不建 .sln
├── 1.6/
│   ├── Assemblies/                     构建产物（gitignored，.gitkeep 占位）
│   ├── Defs/Kiss/                      MWAH_JobDefs.xml, MWAH_ThoughtDefs.xml
│   └── Languages/{English,ChineseSimplified}/
├── Source/Mwah/
│   ├── Mod.cs / MwahSettings.cs / Constants.cs / MwahTime.cs / MwahLog.cs
│   ├── DefOf/ Actions/Kiss/ Jobs/ Rewards/ Systems/
└── scripts/                            stage-package / pack-dev / build-dev / verify-local
```

## WHERE TO LOOK

| 任务 | 位置 | 备注 |
|---|---|---|
| 耐久事实与源码核验结论 | `MEMORY.md` | 含原版 API 依据，改实现前必读 |
| 当前任务面与延后项 | `TODO.md` | 实机矩阵未完成 |
| 右键入口 | `Source/…/Actions/Kiss/FloatMenuOptionProvider_Kiss.cs` | 自动发现，无需注册 |
| 亲吻表演与结算 | `Source/…/Jobs/JobDriver_Kiss.cs` | 双人镜像 job |
| 谁去亲 / 能不能亲 | `Source/…/Actions/Kiss/KissUtility.cs` | `KissProposal` 三态 |
| 心情与社交缩放 | `Source/…/Rewards/KissMoodReward.cs` | 无 `needs.mood` 者静默 |
| 冷却 | `Source/…/Systems/KissCooldown.cs` | 会话内内存态，不落盘 |
| 单位换算 | `Source/…/MwahTime.cs` | 60 t = 1 秒，2500 t = 1 游戏时 |
| 设置项与生命周期 | `Source/…/MwahSettings.cs` + `Mod.cs` | 即时生效 + 防抖落盘 |
| 打包与验证 | `scripts/` | 见 COMMANDS |
| 同类新工程的通用骨架（仓库/记忆/脚本三件事） | `../modding_documents/RimWorld_Mod_RepoInit_AgentMemory_And_Packaging_Guide_zh.md` | 占位符版，跨项目复用 |

## COMMANDS

```powershell
dotnet build Source/Mwah/Mwah.csproj -nologo
pwsh -NoProfile -File scripts/verify-local.ps1          # 14 项静态门
pwsh -NoProfile -File scripts/build-dev.ps1             # Release 构建 + dist/dev 包（zip + version.txt）
pwsh -NoProfile -File scripts/pack-dev.ps1              # 只打包（要求已有构建产物与 git HEAD）
```

无测试工程；`verify-local.ps1` 只做静态与产物检查，实机验证见 `TODO.md`。

## COMMIT PRACTICE

- Conventional Commits 1.0.0，主题用英文祈使句，正文可中文写动机与取舍。
- 原子提交：gameplay 代码、Defs、本地化、打包工具、记忆文档分开提交，能单独回滚。
- `dist/`、`1.6/Assemblies/*.dll|*.pdb`、`Source/**/obj|bin`、`About/PublishedFileId.txt` 不入库。
