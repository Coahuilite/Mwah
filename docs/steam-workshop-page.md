# 工坊页面文案维护源（Workshop page copy, single source）

> **0.3.0 发布文案。** 维护者已接受 1.6／1.7 验收并授权本次更新；渠道实际状态见[发布记录](release_review/release-0.3.0-review-zh.md)。旧版文案保留于 docs/archive/steam-workshop-page-0.2.2.md。

本文是中英工坊描述的唯一维护源，不是 changelog 也不是发布记录。页面编辑、双预览、上传由维护者执行；agent 只做公开页面只读核验。

## 编辑约定

- 品牌：英文 `Mwah!` / 全称 `Every Pawn Kisses Each Other`；中文显示名 `所有Pawn都给我啵嘴！`。三者不翻译、不归一。
- 游戏内文案 register 是「亲吻/吻」；「啵嘴」只允许出现在中文显示名里（与语言文件同一条纪律）。
- 披露置顶：中文「3A 大作声明」（AI 规划/编程/维护），英文 `AI-Generated Work Disclosure`。
- 两份 BBCode 独立粘贴，只用保守标签（`h1/h2/b/i/list/url`）；正文不写 packageId、不重复依赖栏、不写开发者入口与制作步骤。
- 页面从简（2026-10-05 维护者审阅定稿）：开场一句 tagline，玩法三条带过，不铺陈机制长文；边界表述只保留兼容性段的 Core-only / 零 Harmony，单机与地图范围不再单独成条。
- 8000 字符是编辑目标而非已核实硬上限；改完重算长度，发布前核对中英对称、版本号、链接。
- 更新页面时同步 `docs/CHANGELOG*.md` 对应段落（工坊 Change Notes 从 changelog 取段）。

## 中文 BBCode

```bbcode
[h2]⚠ 3A 大作声明[/h2]
本模组由 AI 规划、AI 编程、AI 维护，并由人类维护者审查、测试、打包和发布。

[h1]所有Pawn都给我啵嘴！[/h1]
[b]模组版本：[/b]0.3.0
[b]适用版本：[/b]RimWorld 1.6 / 1.7

[b]Mwah! · Every Pawn Kisses Each Other[/b]

让任何 Pawn 亲任何 Pawn，容忍一下这份唐突的多情。

[h2]怎么亲[/h2]
[list]
[*][b]右键就亲。[/b]任何你指挥得动的 Pawn，右键地图上的任何 Pawn。清醒但倒地的接收方留在原地，能走的一方过去。
[*][b]亲吻导演台。[/b]底栏按钮打开一个小面板，两个头像框可以点选地图上[b]任意[/b]两个 Pawn，包括你指挥不了的袭击者、商人、野生动物和机械族；右槽也可选墙和树，另有快速发配。扩展可从设置首页管理，亲树可按自己的文化选择结果。
[*][b]自主撮合（默认关）。[/b]游戏按你设的间隔和半径，替不由你管理的 Pawn 发起配对；不会让玩家管理的 Pawn 发起，也不选择征召中的玩家 Pawn 接收。
[/list]

[h2]兼容性[/h2]
[list]
[*]RimWorld 1.6 / 1.7，仅需 Core；无前置模组、无 Harmony、无框架依赖。
[*]1.7 适配目标为 4990 rev1069。DLC 可选：亲树可按文化选择结果，关闭文化结果或没有文化 DLC 时仍保留基础亲树；机械族、异象实体、Odyssey 无人机等按各自本性参与。
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
[b]Mod version:[/b] 0.3.0
[b]For:[/b] RimWorld 1.6 / 1.7

Let any Pawn kiss any Pawn — tolerate the audacity of affection.

[h2]How to kiss[/h2]
[list]
[*][b]Right-click.[/b] Any pawn you can order, right-clicked onto any pawn on the map. A conscious downed receiver stays put while the mobile pawn comes over.
[*][b]Kiss Director.[/b] A bottom-bar button opens a small panel; two avatar slots pick [b]any[/b] two pawns on the map — including raiders, traders, wild animals and mechanoids you cannot command — or walls and trees in the right slot. Manage extensions from the settings home page; trees offer optional outcomes based on the acting pawn’s beliefs.
[*][b]Ambient matchmaking (off by default).[/b] The game pairs up pawns you do not manage on an interval and radius you set. Player-managed pawns never initiate; drafted player-controlled pawns are not selected as receivers.
[/list]

[h2]Compatibility[/h2]
[list]
[*]RimWorld 1.6 / 1.7, Core only. No Harmony, no framework mods, nothing to load before this mod.
[*]1.7 targets 4990 rev1069. DLC is optional: tree outcomes can follow the acting pawn's beliefs, while basic tree kissing remains available with culture switched off or without Ideology. Mechanoids, anomalies and Odyssey drones participate per their own nature.
[/list]

[h2]Download & feedback[/h2]
[list]
[*][url=https://github.com/Coahuilite/Mwah/releases]GitHub Releases[/url]
[*][url=https://github.com/Coahuilite/Mwah/issues]GitHub Issues[/url]
[/list]

[h2]License[/h2]
Code is MPL-2.0. No vanilla RimWorld assets are redistributed.
```
