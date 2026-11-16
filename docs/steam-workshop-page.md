# 工坊页面文案维护源（Workshop page copy, single source）

> **目标：0.4.0（维护者已确认 Steam 上传、页面及标签更新完成）。** 已确认的渠道状态见 [release-status.md](release-status.md)。本文是中英工坊描述的唯一维护源。
>
> 上传与线上页面编辑由维护者执行；本页更新不构成发布授权。候选确认见[原生记录](../openspec/changes/shared-content-and-lifecycle/native-acceptance.md)，操作流程见[发布流程](release-runbook-zh.md)。

## 编辑约定

- 品牌：英文 `Mwah!` / 全称 `Every Pawn Kisses Each Other`；中文显示名 `所有Pawn都给我啵嘴！`。三者不翻译、不归一、不改写。
- 游戏内文案的 register 是「亲吻／吻」；「啵嘴」只允许出现在中文显示名里（与语言文件同一条纪律，`verify-local` 会检查语言文件）。
- 被发配者的名词一律是 **Pawn**：中文保留借词，不用「小人／单位／生物」；原版族名（殖民者、机械族、动物等）照常。
- 披露置顶：中文「3A 大作声明」（AI 规划／编程／维护），英文 `AI-Generated Work Disclosure`。
- 两份 BBCode 独立整块粘贴，只用保守标签（`h1/h2/b/i/list/url`）；正文不写 packageId、不重复依赖栏、不写开发者入口与制作步骤。
- 页面从简：开场一句 tagline，玩法三条带过。兼容性板块仅列游戏版本、官方 DLC 和明确支持的可选模组／目标，不混入玩法、供电、资源或运行范围说明。
- **版本行是同步面，不是自由文案。** 本页头部的 `目标：` 行、中文 `模组版本：` 行与英文 `Mod version:` 行必须等于 csproj `<Version>`；`verify-local` 把这三处连同双语 changelog 的置顶条目一起核对，五处不等即红。改版本轴时一起改，别只改一处。
- 8000 字符是编辑目标，不是已核实的工坊硬上限；改完重算长度，发布前核对中英对称、版本号与链接。
- 更新页面时同步 `docs/CHANGELOG*.md` 对应段落（工坊 Change Notes 从 changelog 取段）。
- 不写具体测试结论：验收边界（离线门／实机覆盖）归 [原生确认](../openspec/changes/shared-content-and-lifecycle/native-acceptance.md) 与发布记录，页面只写玩家能自己验证的东西。

## 中文 BBCode

```bbcode
[h2]⚠ 3A 大作声明[/h2]
本模组由 AI 规划、AI 编程、AI 维护，并由人类维护者审查、测试、打包和发布。

[h1]所有Pawn都给我啵嘴！[/h1]
[b]模组版本：[/b]0.4.0
[b]适用版本：[/b]RimWorld 1.6 / 1.7

[b]Mwah! · Every Pawn Kisses Each Other[/b]

让任何 Pawn 亲任何 Pawn，容忍一下这份唐突的多情。

[h2]怎么亲[/h2]
[list]
[*][b]右键就亲。[/b]任何你指挥得动的 Pawn，右键地图上的任何 Pawn。清醒但走不动的接收方留在原地，由能走的那位过去。
[*][b]亲吻导演台。[/b]底栏按钮打开一个小面板，两个头像框可以点选地图上[b]任意[/b]两个 Pawn，包括你指挥不了的袭击者、商人、野生动物和机械族；右槽也能选墙、树和机器，另有两段点选的快速发配。墙、树、机器各共用一个会刷新替换的记忆槽；研究设备的正面收益随识智提升，法式深吻营养膏机的好结果能把已有的饥饿补满，不消耗原料。扩展开关在设置首页管理。
[*][b]自主撮合（默认关）。[/b]游戏按你设的间隔和半径，替不由你管理的 Pawn 发起配对；不会让玩家管理的 Pawn 发起，也不选征召中的玩家 Pawn 接收。
[/list]

[h2]兼容性[/h2]
[list]
[*]RimWorld 1.6 / 1.7（Core）。
[*]官方 DLC：Royalty、Ideology、Biotech、Anomaly、Odyssey。
[*]Vanilla Nutrient Paste Expanded：营养膏排出口、供给器、滴喂器。
[/list]

[h2]下载与反馈[/h2]
[list]
[*][url=https://github.com/Coahuilite/Mwah/releases]GitHub Releases[/url]
[*][url=https://github.com/Coahuilite/Mwah/issues]GitHub Issues：问题反馈[/url]
[/list]

[h2]许可[/h2]
模组代码采用 MPL-2.0。RimWorld 原版资产不随本模组重新分发。

```

## English BBCode

```bbcode
[h2]⚠ AI-Generated Work Disclosure[/h2]
This mod is planned, coded and maintained by AI, and reviewed, tested, packaged and published by a human maintainer.

[h1]Mwah! — Every Pawn Kisses Each Other[/h1]
[b]Mod version:[/b] 0.4.0
[b]For:[/b] RimWorld 1.6 / 1.7

Let any Pawn kiss any Pawn — tolerate the audacity of affection.

[h2]How to kiss[/h2]
[list]
[*][b]Right-click.[/b] Any pawn you can order, right-clicked onto any pawn on the map. A conscious but immobile receiver stays put while the mobile one comes over.
[*][b]Kiss Director.[/b] A bottom-bar button opens a small panel; two avatar slots pick [b]any[/b] two pawns on the map — including raiders, traders, wild animals and mechanoids you cannot command — or walls, trees and machines in the right slot, plus a two-click quick dispatch. Walls, trees and machines each refresh their own shared memory slot; positive research-device moods scale with Intellectual, and a good French kiss at the nutrient paste dispenser can fill an existing Food need without using feedstock. Manage extensions from the settings home page.
[*][b]Ambient matchmaking (off by default).[/b] The game pairs up pawns you do not manage on an interval and radius you set. Player-managed pawns never initiate; drafted player-controlled pawns are not selected as receivers.
[/list]

[h2]Compatibility[/h2]
[list]
[*]RimWorld 1.6 / 1.7 (Core).
[*]Official DLC: Royalty, Ideology, Biotech, Anomaly, Odyssey.
[*]Vanilla Nutrient Paste Expanded: tap, feeder and dripper.
[/list]

[h2]Download & feedback[/h2]
[list]
[*][url=https://github.com/Coahuilite/Mwah/releases]GitHub Releases[/url]
[*][url=https://github.com/Coahuilite/Mwah/issues]GitHub Issues[/url]
[/list]

[h2]License[/h2]
Code is MPL-2.0. No vanilla RimWorld assets are redistributed.
```
