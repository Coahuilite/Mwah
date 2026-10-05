# 所有Pawn都给我啵嘴! · Mwah!

[English](./README.md) | **中文**

Mwah! 让游戏内地图上的任何单位亲任何单位。选中一个 pawn，右键另一个，下令亲吻——能走的那位走过去，两人面对面，飘起原版滚床单同款爱心。有心情的按自己的 `SocialImpact` 拿心情加成；没心情的（动物、机械族、无人机、异象实体）什么都不加。这是个有严肃边界的娱乐模组：**仅游戏内地图、仅单机、零 Harmony**。

本文描述当前功能范围，不是发布声明；唯一人工维护的产品版本是 `Source/Mwah/Mwah.csproj` 的 `<Version>`（`About/About.xml <modVersion>` 跟随）。

## 依赖

- RimWorld **1.6**（仅 Core——不依赖任何 DLC 或模组；机械族、异型、异象实体等 DLC 单位按自身天性参与）。
- 无 Harmony、无前置框架，加载顺序无所谓。

把发布包放进 `RimWorld/Mods/` 即可。

## 能做什么

- **右键亲吻。** 选中任意可下令的 pawn，右键另一个。选中的那位走不动？对方走过来亲它。
- **亲吻导演台。** 底栏按钮开合一个不暂停游戏的面板：左/右两个头像框走原版点选，可指定地图上任意两个 pawn（包括你管不着的），右槽还能选墙等可亲之物；两边选好，心形填红。「快速发配」两段点选直接派发。
- **自主亲吻（默认关）。** 游戏自己撮合你管不着的单位（访客、商人、袭击者、野生动物、机械族），间隔与半径可调，绝不碰你的殖民者。
- **亲墙。** 右键任意墙、天然岩壁或被埋藏的矿脉，凑上去亲。结果按**天意表**（`MWAH_FateDef`，出厂亲墙五档 50 行旁白）掷出，落成**唯一一条**"与墙的关系"记忆——下一次亲吻就地顶替。
- **谁都能被亲，有心情没有都一样。** 有心情的 pawn 按**自己**的社交影响吃加成（+5 × SocialImpact × 倍率；社交达人更赚，哑巴缩水，精神变态毫无感觉）。动物、机械族、无人机和异象实体（蹒跚怪/食尸鬼/苏醒的死尸）没有心情系统：动画、爱心照给，外加各自通道的天意短讯，心情零入账；它们也能经导演台当发起方。
- **停火窗口。** 被发配的吻在 job 生命期内让双方"相安无事"——接近、表演、走回原位全程，因为原版 job 所有权天然挂起 think tree。有效伤害一击即碎，成对/单人冷却限速。系统永远不会把交火中的单位拽去亲嘴；你可以。

## 设置

设置页四段式——核心 / 自主撮合 / 附加功能 / 系统——顶部常驻标题区（副标题 + 构建水印，滚到哪截图都带着）。门禁滑条（「谁能亲谁」七档，出厂最右 = 万物互亲）与附加功能互不干涉：每个 addon 在附加功能段有自己的开关和想法时长，关掉即从一切可选面消失且零提示。所有时长以 tick 存储，界面同时给出 tick / 现实秒 / 游戏小时三种读法；数量滑条配精确数值框，档位滑条刻意不带（档不是量）。诊断面是四档滑条：关闭 / 自动 / 简化 / 详细（自动跟随构建渠道）；启动横幅与报错行无条件播报。中英双语键集合严格一致，由离线门把关。

## 开发、打包与版本

唯一人工维护的产品版本在 `Source/Mwah/Mwah.csproj`。构建从不写入游戏目录。

```powershell
pwsh scripts/pack-dev.ps1                           # dev：Release 构建 + stage dist/dev/Mwah（文件夹；-Zip 可选）
pwsh scripts/pack-github.ps1 -Version v0.2.1        # GitHub flavor -> dist/github/Mwah-vX.Y.Z.zip
pwsh scripts/pack-steam.ps1                         # Steam flavor -> dist/steam/Mwah（要求干净树）
pwsh scripts/verify-local.ps1                       # 全部离线门：三渠道构建、双语键一致、天意表完整、
                                                    # DLL 符号审计、零 Harmony 反证
pwsh scripts/privacy-audit.ps1 -FullHistory -PrePush # push 前仪式
```

CI（`.github/workflows/ci.yml`）在任何版本分支（`[0-9].[0-9].x`）与 `main` 的每次 push/PR 跑同一套门加隐私默认扫描。提交纪律：Conventional Commits（见 `.gitmessage`），在当前版本分支上工作，squash merge 进只做发布的 `main`，历史 **append-only**——不 amend、不 rebase。完整流程见 [`docs/release-runbook-zh.md`](./docs/release-runbook-zh.md)；工坊页面文案唯一维护源见 [`docs/steam-workshop-page.md`](./docs/steam-workshop-page.md)；版本历史见 [`docs/CHANGELOG.md`](./docs/CHANGELOG.md) / [`docs/CHANGELOG.zh-CN.md`](./docs/CHANGELOG.zh-CN.md)。

## 硬边界

零 Harmony（不引用、不声明依赖、不做运行时补丁）。不做世界地图、商队途中、多人联机同步。不向游戏安装目录建任何链接。玩家可见文字必须中英双语齐备。仓库与可达历史中零绝对本地路径、零凭据、零工坊 ID 文件。

## 许可证

源代码采用 [Mozilla Public License 2.0](./LICENSE)。
