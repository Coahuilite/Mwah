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
| GitHub | **完整**：Release `v0.2.2` 已发布（非 prerelease、非 draft）+ 单资产；发布时间 2026-10-06 00:00 UTC+8 |
| Workshop | **维护者报告已上传（2026-10-05）**；agent 未做页面级核验（无 item URL 可查，item ID 永不入库）。订阅安装后的游戏内行为未核验 |
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
| 试玩载体 | dev flavor（`build=dev`，含完整 sha 与详细诊断默认）；**发布资产（github/steam flavor）未做专门 Player.log 验收** |

边界：实机矩阵的完整通过记录在 `OBLIVIONIS.md`（2026-10-05 节）；本次发布后未以发行 flavor 重跑矩阵。

## 内容摘要

首个公开版本。任何 Pawn 亲任何 Pawn：右键 / 亲吻导演台 / 自主撮合三条发起路径；停火窗口 = job 生命期；心情按被亲方自身 `SocialImpact` 缩放；亲墙 addon（五档天意 × 五十行旁白，XML 可编辑）；无心情参与者的分类消息通道（动物/机械族/实体）；四段式设置页 + 四档诊断；中英双语；仅游戏内地图、仅单机、零 Harmony、仅依赖 Core。0.1.x 整线未发布即归档，0.2.0/0.2.1 从未打 tag。

## 限制与未决

- Workshop 页面级与订阅安装后的实机核验未做（渠道观察只支持页面级结论，一个渠道不证明另一个渠道）。
- Release 正文目前是 GitHub 自动生成的 "Full Changelog" 链接，未注入 changelog 条目（TODO 已记，未排期）。
- main 上的 changelog 冻结在发布提交的 `23:57` 时间戳；修正版（`00:00` 跨日事实）在 `0.2.x`，随下一次 release 带上 main（append-only，不回写）。
- 发行 flavor 的 Player.log 级验收未做；如需补，用 GitHub 资产装一次并开简化诊断即可。
