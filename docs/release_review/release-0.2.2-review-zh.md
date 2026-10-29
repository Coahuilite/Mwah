# Release 0.2.2 观察记录

观察日期/时区：2026-10-06（UTC+8）。本次批准渠道：**Steam Workshop 首发（维护者本人上传）+ GitHub 正式 Release**；agent 负责门禁、合入、tag、Release 面与下载级核验，Workshop 上传动作本身归维护者。证据范围：本地门全跑 + GitHub CI/资产（下载级）+ steam 暂存包 + 本地实机试玩（dev 包，维护者执行）。本文不含绝对本地路径、日志摘录、凭据或工坊 item ID。

## 发布事实

| 项 | 证据 |
| --- | --- |
| 版本、tag、源码提交 | `0.2.2`；轻量 tag `v0.2.2` → `32a9d94`（= `origin/main`，squash 自 `0.2.x@720e411`，合并后 `git diff 720e411 main` 为空）；tag 血统校验通过（Release CI 强制） |
| 发布周期 | `8f6ce79..d7cf943` 共 18 笔（0.2.x 分支上从文案整备到发布收尾的全部提交） |
| 构建 | dev 包 26 文件（`build=dev commit=a8760dd` 试玩版）；steam 暂存 `dist/steam/Mwah/` 26 文件（`build=steam commit=6316079`）；GitHub 资产由 CI 在 tag 提交上构建（`build=github commit=32a9d94`） |
| CI/资产 | main push CI `37337292496` success；Release CI `37337297968` success；资产 `Mwah-v0.2.2.zip` = 863,763 B，**下载级实测** SHA256 `73daa758…79c31b87` 与 API digest 一致；解包 = 唯一顶层 `Mwah/`、26 文件、`version.txt` 三行身份正确；文件清单与 steam 暂存包逐名一致（DLL 字节不同属三渠道 flavor 设计，非差异项） |
| 本地验证 | `verify-local` 全绿（35 条断言，含三渠道构建、双语键一致、天意表完整、版本跟随面 7b、名词纪律 3b、DLL 符号与零 Harmony 反证）；`privacy-audit -FullHistory -PrePush` 发布周期内三次 CLEAN（134 revisions，未接受命中 0，`[known-debt]` 2 条接受——均为正则字面量形状误报，非路径泄漏） |
| GitHub | **完整**：Release `v0.2.2` 已发布（非 prerelease、非 draft）+ 单资产；`published_at` 2026-10-06 00:00:22 UTC+8（draft `created_at` 23:59:11，发布时刻恰跨日） |
| Workshop | **已上传（2026-10-05）且页面级核验完成（2026-10-06，见下节）**；游戏内行为由维护者上传前以 steam 包开游戏实测背书 |
| 流程变更（本周期落地） | 版本轴收口为 csproj 单字面量 + 门 7b 锁全部产品可见跟随面；门 3b 锁 Pawn 名词纪律；隐私审计扩到五向量（二进制元数据 vector4、自匿名 vector5）；changelog 规则对齐 Keep a Changelog 类型制 + 影响优先条目写法；CONTRIBUTING 双语化并解除分支绑定 |

## 本周期关键修正与守卫

- **发配不放回抽取**（维护者实机定罪：鸸鹋×象同抽"闻口袋"镜像复读）：同一事件内两份结算共享天意表时第二抽排除第一行（`KissFate.Roll(scope, exclude)`）；模拟 10 万桩双结算撞车率 12.5%→0，第二抽均匀性无偏；单行表仍出句（沉默比复读糟）。
- **术语对齐原版官方译名**（对照本地原版 zh-CN 翻译镜像逐 Def 核实）：机械族/古代人/人类(humanlike)/想法/蹒跚怪/食尸鬼/苏醒的死尸/被埋藏的矿脉；"滚床单"确认为 `GotSomeLovin` 官方译名保留。
- **代码审查卫生**：走位止损三处拷贝合一 `WalkTally`；`CanMoveNow` 迁入 `KissBoundary`（依赖图转为严格单向）；`"KissPawn"` 字面量收进 `KissFateScopes`。
- **文案去 AI 味**：语言文件删操作手册与实现细节（"镜像 job""1Hz 采样器""这是不打补丁的唯一路子"）；changelog 双语重写为影响优先、戏谑留给游戏内。

## 实机验收（dev 包，维护者执行）

| 面 | 观察 |
| --- | --- |
| 发配 | 维护者核对动物×动物双结算："发配正常"（不放回抽取生效） |
| 试玩载体 | dev flavor（`build=dev`，含完整 sha 与详细诊断默认）；steam flavor 由维护者上传前开游戏实测（2026-10-06 追记：发布时即测，本次确认背书）；github flavor 未单独跑，行为面与 steam 同源，仅 banner 与诊断默认档位不同 |

边界：实机矩阵的完整通过记录在 `OBLIVIONIS.md`（2026-10-05 节）；发行 flavor 不再排期专门验收（维护者 2026-10-06 裁决：pass）。

## Workshop 页面级核验（2026-10-06 追记，agent 只读）

方法：匿名只读抓取（服务器渲染页 + `l=schinese` 语言参数），零登录零编辑动作；item ID 按红线不入库。

- **正文**：中英两份与唯一维护源 `docs/steam-workshop-page.md` **逐字一致**——披露置顶（3A 大作声明 / AI-Generated Work Disclosure）、版本 0.2.2、适用 1.6、三条发起路径、Core-only / 零 Harmony、GitHub Releases/Issues 链接、MPL-2.0 许可段，全部在场且无漂移。
- **元数据**：标题英文 `Mwah! - Every Pawn Kisses Each Other`、中文语言位 `Mwah! - 所有Pawn都给我啵嘴！`（品牌三名纪律满足）；标签 `Mod` + `1.6`；大小 963.632 KB；发布时刻 Oct 5 8:55am（Steam 服务器时区 = 23:55 UTC+8）；讨论 0、留言 0、"评价数不足"——首发首日正常态。
- **改动说明**（1 条）：正文与 `docs/CHANGELOG.md` 0.2.2 节逐字一致；唯一偏差 = 条目内 "GitHub Release went out at 23:57 UTC+8" 为撰写时预估值，实际 `published_at` 为 00:00:22 UTC+8（差 3 分钟、跨日）。公开页面编辑归维护者，是否修订由其裁决（不修亦无读者决策性影响）。

## 内容摘要

首个公开版本。任何 Pawn 亲任何 Pawn：右键 / 亲吻导演台 / 自主撮合三条发起路径；停火窗口 = job 生命期；心情按被亲方自身 `SocialImpact` 缩放；亲墙 addon（五档天意 × 五十行旁白，XML 可编辑）；无心情参与者的分类消息通道（动物/机械族/实体）；四段式设置页 + 四档诊断；中英双语；仅游戏内地图、仅单机、零 Harmony、仅依赖 Core。0.1.x 整线未发布即归档，0.2.0/0.2.1 从未打 tag。

## 限制与未决

- ~~Workshop 页面级与订阅安装后的实机核验未做~~ — 已闭合：页面级核验 2026-10-06 完成（见上节）；游戏内行为由维护者上传前 steam 包实测背书。
- ~~Release 正文注入 changelog 条目~~ — 已裁决不做（维护者 2026-10-06：Full Changelog 超链接足够，乐意的人自己会去看）。
- main 上的 changelog 冻结在发布提交的 `23:57` 时间戳；修正版（`00:00` 跨日事实）在 `0.2.x`，随下一次 release 带上 main（append-only，不回写）。
- ~~发行 flavor 的 Player.log 级验收~~ — 已闭合（维护者 2026-10-06 裁决 pass：发布时即用 steam 包开游戏实测后上传）。
