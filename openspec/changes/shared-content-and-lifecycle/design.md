# 共享内容、原生构建与交付设计

状态：2026-11-09 已按维护者采用方案实施，原生消费待维护者确认；交付及证据见 tasks。完整范围见 [proposal](proposal.md)，生命周期取舍另见 [state-machines](state-machines.md)。

## 实施结构：一套实际内容，两套原生输出

```text
Source/Mwah/                         唯一 C# 源码及 csproj
Common/
  Defs/                              唯一共同 Def 编辑源／运行根
  Languages/                         唯一共同语言编辑源／运行根
Compatibility/VanillaNutrientPasteExpanded/
  Defs/                              三个 Mwah 自有目标登记
  Languages/                         VNPE 专用文案
1.6/Assemblies/Mwah.dll               net472，固定 1.6 引用
1.7/Assemblies/Mwah.dll               net10.0，固定 1.7 引用
About/
LoadFolders.xml
```

这是同一仓库的实际布局，不是为 `0.4.x` 再复制一个项目目录。分发包复制 Common／Compatibility 各一份、两版原生输出各一份。旧 `1.6/Defs` 和 `1.6/Languages` 已移除，不再充当编辑或分发来源，不能残留成隐式覆盖层。

选择 Common 而非仅改名为 Content 后照旧复制两份，是为了同时明确编辑源和实际运行根。两版共同内容的一次修改只需维护一处，Loader 组合版本差异；不再依赖某个旧 minor 代表整个模组。

## 加载组合

两个目标版本都显式列出根、Common、自己的 minor 及满足条件的兼容根，例如：

```xml
<v1.6>
  <li>/</li>
  <li>Common</li>
  <li>1.6</li>
  <li IfModActive="VanillaExpanded.VNutrientE">Compatibility/VanillaNutrientPasteExpanded</li>
</v1.6>
```

1.7 仅将原生版本根改为 `1.7`。Common 与 Compatibility 均不放程序集；根也不放 Mwah.dll。完整有效输入始终为“共同内容＋当前 minor 原生 DLL＋符合条件的兼容内容”。

显式 LoadFolders 需要列出所需 Common，且同相对路径存在覆盖优先级；迁移采用唯一文件路径，不靠覆盖整份 Def／语言文件拼接功能。[Wiki：Mod Folder Structure](https://rimworldwiki.com/wiki/Modding_Tutorials/Mod_Folder_Structure)。更完整的条件语义及源码观察保留在[前期研究](../vnpe-loadfolders/design.md#wiki-使用方式与源码核对)。

采用根→Common→minor→条件兼容的优先级，表示共同基线、明确原生差异及兼容内容。当前包内 minor 只允许原生程序集，不保留旧 Def／语言。不能仅把 Common 排最后来掩盖旧安装残留：不同文件名的旧内容仍可能载入。

## VNPE 的内容接缝

沿用已研究的数据驱动方向，但使用本页的新共享布局：三个外部目标名、前向／接触策略和专用 tooltip key 留在可选登记 Def；共享消费者在 Def 加载后解析登记。包未启用则登记文件根本未进入有效内容；某个目标名失效时诊断并跳过该项。

不修改对方 ThingDef，不引用 VNPE／VEF／PipeSystem 类型，不把其生产条件引入 PowerOn；不新增兼容 DLL。原版营养膏机与其他机器的共同内容留 Common，Food 奖励、天意、单槽和 TargetB 都复用原责任方。可选语言不能被基础代码无条件引用，也不能通过永久合并全部可选语言让漏载静默通过。

## 必须一起完成的生产者—消费者连接

| 生产者／权威 | 消费者 | 迁移要求 |
| --- | --- | --- |
| Common 的 Def／语言 | Loader、源码 Keyed 引用、DefOf／天意引用 | 所有既有身份和键含义保留；基础组合完整，不从旧 minor 找回丢失内容 |
| 条件兼容登记／语言 | 机器识别、站位、tooltip | 条件成立才采用；无登记时基础能力正常，缺一目标只影响该项 |
| csproj 单一源码及版本轴 | 两原生 DLL、About、changelog／渠道说明 | 保留 net472／net10.0 和固定引用；0.4.0 用未发布身份，不修改既有 0.3.0 发布事实 |
| LoadFolders 声明 | 暂存验证及原生 Loader | 区分“包里存在”与“本组合生效”，不得把所有 li 当无条件加载 |
| stage-package 输出 | Dev／GitHub／Steam 及隐私审计 | 一次拷贝共享内容、各版本 DLL；原渠道顶层形状不变，不打入源码、工具、缓存或引用程序集 |

具体受影响点已确认包括：`scripts/stage-package.ps1` 的 `versionedSource`／复制／内容清单；`scripts/verify-local.ps1` 的 `1.6/Defs`、语言路径和有效引用集合；LoadFolders；README、CONTRIBUTING、发布流程及直接使用旧路径的说明。原 csproj 已是双目标选择，不因内容搬迁拆成两份工程。

## 施工边界

- 内容、兼容登记、LoadFolders 和打包／验证脚本由一个 owner 串成完整切片；不把搬文件、修扫描、修打包分给互相等待的不同人。
- 生命周期 owner 不改共同 Def／语言及这些脚本；若需要修改真实程序集清单，交给内容 owner 同步，不保留无用旧类维持检查。
- 当前双目标构建仍串行，避免同时改写同一项目的中间还原状态。不为了未来可能的并发先造构建平台。
- 迁移和生命周期整理分开提交，保留可定位的行为边界；主协调者统一集成、版本和 Git 操作。

## 验证与恢复

现有离线门继续检查同一保证，只调整新的内容来源与条件采用。必须检查声明目录确实存在、共享／兼容根没有程序集、minor 没有旧内容副本，以及有效输入遗漏、重复 Def／语言、同相对路径遮盖、选错 native DLL、可选内容在基础组合漏入／缺失；不新增单元测试或换名用例。内容搬迁、扫描路径、暂存规则和加载声明在同一完整提交落地，避免可构建却缺内容或检查入口直接失效的中间版本。

最终对稳定候选跑既有双目标／三渠道及实际包检查，复用仍适用证据。原生由维护者确认两版基础／VNPE 条件加载和实际入口；生命周期改动的消费验证见状态清单。未覆盖的场景留在各责任方，不把编译写成实机通过。

若共享根迁移失败，按共同源→Loader→暂存→消费者定位并修复，必要时以普通修复提交恢复整个迁移切片；不留下半份旧目录作无声兜底，不改写历史。候选包保留可辨别版本／提交身份，禁止混用两次构建的 DLL 或把源码根当分发包。

升级安装须由维护者**完整替换模组安装副本，不叠加覆盖旧目录**，避免旧 minor 内容继续加载；存档和配置保持原位置。该说明进入 runbook／玩家安装指引，实施者不代为删除游戏目录。旧公开包／已保留候选仍提供恢复来源，不建立额外备份链。
