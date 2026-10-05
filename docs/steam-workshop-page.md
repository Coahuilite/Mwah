# 工坊页面文案维护源（Workshop page copy, single source）

> **目标：0.2.2（未发布）。** 首次上传时整块粘贴对应语言，勾选后按页面预览核对一次。发布状态以本页头部与 `MEMORY.md` 耐久状态为准，不以本文件历史版本为准。

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
[b]模组版本：[/b]0.2.2
[b]适用版本：[/b]RimWorld 1.6

[b]Mwah! · Every Pawn Kisses Each Other[/b]

让任何 Pawn 亲任何 Pawn，容忍一下这份唐突的多情。

[h2]怎么亲[/h2]
[list]
[*][b]右键就亲。[/b]任何你指挥得动的Pawn，右键地图上的任何 Pawn。
[*][b]亲吻导演台。[/b]底栏按钮打开一个小面板，两个头像框可以点选地图上[b]任意[/b]两个 Pawn，包括你指挥不了的袭击者、商人、野生动物和机械族，还有两段式快速发配模式可用。
[*][b]自主撮合（默认关）。[/b]游戏按你设的间隔和半径，替你管不着的 Pawn 自己配对。它一次都不碰你的殖民者。
[/list]

[h2]兼容性[/h2]
[list]
[*]RimWorld 1.6，仅需 Core；无前置模组、无 Harmony、无框架依赖。
[*]DLC 可选：机械族、异象实体、Odyssey 无人机等按各自本性参与，没有 DLC 也完整可用。
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
[b]Mod version:[/b] 0.2.2
[b]For:[/b] RimWorld 1.6

Let any Pawn kiss any Pawn — tolerate the audacity of affection.

[h2]How to kiss[/h2]
[list]
[*][b]Right-click.[/b] Any pawn you can order, right-clicked onto any pawn on the map.
[*][b]Kiss Director.[/b] A bottom-bar button opens a small panel; two avatar slots pick [b]any[/b] two pawns on the map — including raiders, traders, wild animals and mechanoids you cannot command — plus a two-click quick-chain mode.
[*][b]Ambient matchmaking (off by default).[/b] The game pairs up pawns you cannot command on an interval and radius you set. It never touches your colonists.
[/list]

[h2]Compatibility[/h2]
[list]
[*]RimWorld 1.6, Core only. No Harmony, no framework mods, nothing to load before this mod.
[*]DLC optional: mechanoids, anomalies and Odyssey drones participate per their own nature; everything works without any DLC.
[/list]

[h2]Download & feedback[/h2]
[list]
[*][url=https://github.com/Coahuilite/Mwah/releases]GitHub Releases[/url]
[*][url=https://github.com/Coahuilite/Mwah/issues]GitHub Issues[/url]
[/list]

[h2]License[/h2]
Code is MPL-2.0. No vanilla RimWorld assets are redistributed.
```
