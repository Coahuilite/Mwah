# 所有Pawn都给我啵嘴! · Mwah!

[English](./README.md) | **中文**

Mwah! 让游戏内地图上的任何单位亲任何单位。选中一个 pawn，右键另一个，下令亲吻——能走的那位走过去，两人面对面，飘起原版滚床单同款爱心。有心情的按自己的 `SocialImpact` 拿心情加成；没心情的（动物、机械族、无人机、异象实体）什么都不加。这是个有严肃边界的娱乐模组：**仅游戏内地图、仅单机、零 Harmony**。

本文描述当前功能范围，不是发布声明；唯一人工维护的产品版本是 `Source/Mwah/Mwah.csproj` 的 `<Version>`（`About/About.xml <modVersion>` 跟随）。

## 依赖

- RimWorld **1.6**（仅 Core——不依赖任何 DLC 或模组；机械族、异型、异象实体等 DLC 单位按自身天性参与）。
- 无 Harmony、无前置框架，加载顺序无所谓。

把发布包放进 `RimWorld/Mods/` 即可。复制模组与实机测试由维护者本人执行；仓库不假设、不创建任何符号链接。

## 能做什么

- **右键啵嘴。** 选中任意可下令的 pawn，右键另一个。选中的那位走不动？对方走过来亲它。
- **亲吻导演台。** 底栏按钮开合一个不暂停游戏的面板：左/右两个头像框走原版点选，可指定地图上任意两个 pawn（包括你管不着的），两边选好爱心填红；「快速发配」两段点选直接派发。
- **自主亲吻（默认关）。** 游戏自己撮合你管不着的单位（访客、商人、袭击者、野生动物、机械族），间隔与半径可调，绝不碰你的殖民者。
- **亲墙。** 右键任意墙、天然岩壁或压埋矿脉，凑上去亲。结果按**天意表**（`MWAH_FateDef`，出厂亲墙 50 行、五档权重）掷出，落成带逐行旁白的心情条目。addon 各一个类，墙是第一个。
- **停火窗口。** 被发配的吻在 job 生命期内让双方"相安无事"——接近、表演、走回原位全程，因为原版 job 所有权天然挂起 think tree。有效伤害一击即碎，成对/单人冷却限速。系统永远不会把交火中的单位拽去亲嘴；你可以。

## 设置

17 项设置，中英双语键集合严格一致。门禁滑条（「谁能亲谁」七档，出厂最右 = 万物互亲）管不着亲墙——它有独立开关，关掉后墙从一切可选面消失且零提示。所有时长以 tick 存储，界面同时给出 tick / 现实秒 / 游戏小时三种读法，每条滑条配精确数值框。诊断面（`Player.log` 的 dev 事件行 + 存档目录 1Hz 阶段采样）是运行时开关；启动横幅无条件播报。

## 开发、打包与版本

唯一人工维护的产品版本在 `Source/Mwah/Mwah.csproj`。构建从不写入游戏目录。

```powershell
pwsh scripts/build-dev.ps1                          # dev：Release 构建 + stage dist/dev/Mwah + zip
pwsh scripts/pack-github.ps1 -Version v0.1.0        # GitHub flavor -> dist/github/Mwah-vX.Y.Z.zip
pwsh scripts/pack-steam.ps1                         # Steam flavor -> dist/steam/Mwah（要求干净树）
pwsh scripts/verify-local.ps1                       # 25 道离线门：三渠道构建、双语键一致、天意表完整、
                                                    # DLL 符号审计、零 Harmony 反证
pwsh scripts/privacy-audit.ps1 -FullHistory -PrePush # push 前仪式
```

CI（`.github/workflows/ci.yml`）在每次 push/PR 跑同一套门加隐私默认扫描。提交纪律：Conventional Commits（见 `.gitmessage`），在版本分支（`0.1.x`）上工作，squash merge 进只做发布的 `main`，历史 **append-only**——不 amend、不 rebase。完整流程见 [`docs/release-runbook-zh.md`](./docs/release-runbook-zh.md)。

## 硬边界

零 Harmony（不引用、不声明依赖、不做运行时补丁）。不做世界地图、商队途中、多人联机同步。不向游戏安装目录建任何链接。玩家可见文字必须中英双语齐备。仓库与可达历史中零绝对本地路径、零凭据、零工坊 ID 文件。

## 许可证

源代码采用 [Mozilla Public License 2.0](./LICENSE)。
