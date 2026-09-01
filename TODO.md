# TODO

## 当前目标

- 0.1.0 离机面已完成，等待维护者实机验收；验收通过后才是第一个可分发版本。

## 实机验证矩阵（维护者本人执行，agent 不建 junction、不代跑游戏）

- [ ] 装载：复制模组到 `Mods/` → 启用 → 启动无红字；开新档或读档后 `1.6/Defs/Kiss/*` 两文件被解析。
- [ ] 基本流：选中殖民者 A 右键殖民者 B → 菜单出现 `kiss B`（社交段位置）→ A 走到 B 贴脸 → 两人面对面 → 双方各冒爱心 → 各自回原位。
- [ ] 心情数值：A/B 心情面板出现「kissed by X」类条目，剩余时长与设置里"心情持续"一致；社交 10 级的人条目数值明显高于社交 0 级；哑巴/失聪者数值下降（`SocialImpact` 容量因子生效）。
- [ ] 无心情单位：对动物、机械族、Odyssey 无人机、异象实体执行 → 爱心与动作正常，心情面板**无任何新增**，Player.log 无异常。
- [ ] 角色互换：选中一个缺腿/倒地者，右键能动的 → 变成对方走过来亲；两个都动不了 → 菜单出现灰项并写明原因。
- [ ] 敌对与派系：默认设置下对袭击者下令 → 观察对方反应（可能立即反击，属预期）；关闭"允许敌对"后菜单不再出现该项。
- [ ] 冷却：连亲同一人 → 灰项显示剩余时间且三单位可读；把成对冷却设 0 → 可连续亲。
- [ ] 设置页：所有时长项显示 `tick / 秒 / 游戏时` 三读法；改值即时生效、重开设置仍保持；`恢复默认值` 回到 `Constants`。
- [ ] 存档兼容：亲吻进行中存档 → 读档 → job 正常继续或干净结束，无 `ticksLeft`/`homeX` 相关红字。
- [ ] 卸载残留：存档含 `MWAH_Kissed`/`MWAH_KissedBond` 记忆与 `MWAH_Kiss` job 后关闭模组 → 确认是静默丢弃还是报错，据此决定是否在 About 里加卸载提示。

## 待决策

- [ ] 是否有稳定中文显示名（现仅 Keyed 分类名「每个小人都互相亲吻」，未升格为品牌）。
- [ ] 是否建远端仓库与 `.github/workflows` CI（外部操作，需逐次授权）。
- [ ] LICENSE 选型（当前仓库无 LICENSE；`Coahuilite` 各仓库口径待统一）。

## 明确延后 / 不做

- [ ] 二期候选（延后，无排期）：AI 自主亲吻 —— 需 `InteractionDef` + `InteractionWorker.RandomSelectionWeight`，且只对 flesh + humanlike 生效，与本模组"全覆盖"取向不同。
- [ ] 不做：世界地图/商队途中右键；RimWorld Multiplayer 同步；真·贴合亲吻姿势（需自定义 `PawnRenderNodeWorker` + 美术，非 API 限制）。
- [ ] 不做：Downed 者当发起方（需 Harmony 放开 `FloatMenuMakerMap.ShouldGenerateFloatMenuForPawn`，与零 Harmony 约束冲突）。
