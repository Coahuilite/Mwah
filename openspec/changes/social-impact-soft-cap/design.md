# Design

## 直接责任与数值

KissMoodReward 读取每个 Pawn 自身最终 SocialImpact，137% 以内保持原值，以上按 B + R × (S − B) / (R + S − B) 压缩，B=1.37、R=0.13，再乘 MwahSettings.MoodMult。常量固定，不另加滑条。压缩作用于最终属性，原版装备超出基准的部分也会压缩；好感沿用同一倍率。关闭时直接使用原值，不对技能或 StatDef 做全局写入。

MwahSettings 是开关的权威，字段／Scribe 键 socialImpactCompression，默认及缺失值 true，恢复默认覆盖该字段。MwahSettingsView 在第二页心情强度旁使用既有 Checkbox；原防抖、关闭保存及双语键门继续管理它。

## 原版准入后的定向替换

1.7.4990 rev1069 的 MemoryThoughtHandler.TryGainMemory 先验证 CanGetThought 和社交目标生命阶段，再分配 pawn／otherPawn，调用新记忆的 TryMergeWithExistingMemory，最后加入记忆并应用原版堆叠约束。普通 Thought_Memory 满组只 Renew 旧实例，新的 moodPowerFactor 会被丢弃；社交变体又直接跳过合并。不能仅在发奖前给候选实例写倍率。

保留既有可存档的 Thought_MemoryFated／Thought_MemorySocialFated 类型，在其原生合并钩子仅为 MWAH_Kissed／MWAH_KissedBond 调用共享替换责任方：通过准入后，倒序用 RemoveMemory 删除同搭档两种配对 Def 的旧条目，再让原版加入已完成数值、时长和旁白配置的新实例。非配对 Def 继续走各自 base，不改变墙／树。使用原版公开钩子及移除方法，不直接改记忆列表、不绕过原版准入、不加 Harmony。

这种刷新同时恢复 KISS-07 已有的同搭档单槽保证。它只发生在新的合法结算；其他搭档、未再亲吻的旧记忆、旧存档字段不扫描迁移。原版既有总堆叠／衰减规则保留。

## 版本与验证边界

共享源码分别对 1.6.4871 net472 和 1.7.4990-unstable net10.0 编译；保留旧构建入口、类型／Def／Scribe 身份，无新运行依赖。复用 verify-local 的双版本三渠道构建、配置／语言／程序集库存，以及 pack-dev 暂存和实际包隐私审计；不新增单元测试或等价换名脚本。游戏加载、实际渲染、旧记忆替换和存读档由维护者实机验收。源提交 17bc175 的实现和已有证据保留历史对照，不把编译成功当作行为成功。
