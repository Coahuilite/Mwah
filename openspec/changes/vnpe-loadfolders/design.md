# VNPE 的 LoadFolders 隔离设计

后续路由：[全模组新设计](../shared-content-and-lifecycle/design.md)已提出 Common／顶层 Compatibility 的统一候选布局。本页继续保存 Wiki／源码观察和登记消费研究；下文 minor 内目录树是此前草案，不与新计划并行实施。

状态：候选技术设计，2026-11-09。范围和实施顺序见 [proposal](proposal.md)；现行玩家保证仍在 [addons](../../specs/addons/spec.md)。

## Wiki 使用方式与源码核对

Wiki 说明：显式 LoadFolders 替代默认目录选择；需要的根／Common 必须列出；`IfModActive` 可条件加载可选内容；相同相对文件路径由后列目录覆盖，路径大小写必须一致。[RimWorld Wiki：Mod Folder Structure](https://rimworldwiki.com/wiki/Modding_Tutorials/Mod_Folder_Structure#LoadFolders.xml_(Optional))。

本轮另核对原始实现：远端 RimSage 的 `Source/Verse/ModLoadFolders.cs`、`LoadFolder.cs`，以及本地 **1.7.5000 rev1275** 的 `Source/Assembly-CSharp/Verse/` 同名文件。本地快照完整且与配置源一致，相关 Assembly-CSharp 无反编译警告；远端结果未提供 build 标识，不据此声称某个精确 1.6 build 已实机通过。项目编译引用继续固定 1.6.4871／1.7.4990-unstable，不随调研数据集升级。

从 `LoadDataFromXmlCustom` 与 `ShouldLoad` 可确认条件语义：

- `IfModActive="A,B"`：至少一个启用；`IfModActiveAll="A,B"`：全部启用。
- `IfModNotActive="A,B"`：列表中任意一个启用都会拒绝该目录，即要求全部未启用；不要误读成“至少一个没启用”。
- 同一 li 上不同条件必须同时通过。当前只需单包 `IfModActive`，不组合多余条件。
- 判断使用无来源后缀的包身份查询；不为当前 1.6／1.7 额外复制 `_steam` 分支。

条件目录只选择实际文件；编入 Mwah.dll 的类型不会因为把 `.cs` 源文件放到 Compatibility 目录而变成条件载入。若将来真的引用第三方程序集类型，再考虑由相同条件控制独立适配程序集；现在没有这个依赖压力。

## 保留版本目录边界，条件化内容

继续遵守当前“运行内容只在 minor 目录内”的项目边界。单一编辑源拟为：

```text
1.6/
  Defs/ ...                            基础机器／原版 Paste 内容不搬
  Languages/ ...
  Compatibility/VanillaNutrientPasteExpanded/
    Defs/MWAH_VNPE_PasteTargets.xml
    Languages/English/Keyed/MWAH_VNPE.xml
    Languages/ChineseSimplified/Keyed/MWAH_VNPE.xml
```

暂存继续从唯一编辑源派生两版内容；`1.7/` 不新增另一份手工编辑源。拟定加载选择：

```xml
<loadFolders>
  <v1.6>
    <li>/</li>
    <li>1.6</li>
    <li IfModActive="VanillaExpanded.VNutrientE">1.6/Compatibility/VanillaNutrientPasteExpanded</li>
  </v1.6>
  <v1.7>
    <li>/</li>
    <li>1.7</li>
    <li IfModActive="VanillaExpanded.VNutrientE">1.7/Compatibility/VanillaNutrientPasteExpanded</li>
  </v1.7>
</loadFolders>
```

兼容根位于 minor 内，但不嵌套在基础 `Defs/` 或 `Languages/` 下，避免基础递归扫描绕过条件。所有声明目录在分发包中存在，条件只决定是否生效。

这与把全模组迁入 Common 是两件事：这里只增加有明确可选依赖原因的内容接缝；不搬基础内容、不改变两版 DLL，也不另立通用目录重构。

## 最小数据接缝，不造第二个机器系统

推荐一个小型 Mwah 自有 Def：登记目标 DefName 字符串、前向／接触站位策略，以及需要的 tooltip key。类型留在主程序集，三个实例仅在条件内容根。

```text
IfModActive 决定兼容登记文件是否存在于本次加载
  → Def 加载完成后，共享登记消费者解析目标
  → worker 判断目标及站位策略
  → 原 TargetB／JobDriver／Paste 天意／唯一机器记忆槽
```

- 三个第三方目标名从当前专有 adapter 移到可选 XML；基础 DLL 不再维护 VNPE 包名及目标名。
- 目标名用字符串显式解析：启用包但某个 Def 被其他模组移除时，按现有合同诊断并跳过该条，不让缺失的强 XML 引用拖累基础内容。
- 不往对方 ThingDef 注入 ModExtension 或 PatchOperation，继续遵守“不改对方 Def”。
- 注册缓存只拥有当前 Def 加载期的解析结果，不拥有 Pawn、管网、Food 或 Job 完成标志。无任何登记时为空，基础机器和原版营养膏机照常。
- 重复目标登记须明确诊断并排除歧义，不靠 XML 文件枚举顺序选胜者；内部类名在实施时按项目惯例确定，不是新公开接口。

## 文案和验证随真实消费者走

Paste 共用天意、饱腹短讯和原版机所需文案仍留基础内容；只迁 VNPE 接触提示等专用键。基础机器设置说明改为不依赖可选内容，避免复制整份语言文件作覆盖。登记携带专用 tooltip key，C# 不强制引用未加载组合的可选键。

现有验证需要按新前提调整，而非删掉失败项：

- `stage-package.ps1` 目前枚举全部 li，没有解释条件；新增条件后须区分包内完整内容和本次生效内容，并检验实际所选根中的程序集／Def／语言。
- `verify-local.ps1` 当前语言全集与 C# 引用检查基于基础语言目录。迁出专用键后，按登记的实际消费者及加载组合验证双语存在、重复键和引用，不把所有可选语言永久并入基础集合来掩盖漏载。
- DLL 清单验证真实保留类和入口；旧专有 adapter 删除后更新其条目，保留共享 Job／奖励入口，不把 XML-only 登记强塞进 ASCII 符号清单。
- 复用既有双目标／三渠道构建、XML、Def、零 Harmony 和包隐私门。Wiki 提供的开发菜单 **Loaded Files For Mod** 可由维护者观察最终文件选择；离线检查不代替该原生结果。

## 为什么不选其他路线

- **只把 `.cs` 移入条件目录**：编译后仍在主 DLL，不能满足条件内容隔离。
- **为三个名字另建双版本 DLL**：增加程序集引用、加载次序和分发责任；当前数据足以表达，不值得。
- **把同一份基础文件和可选文件放成同一路径覆盖**：容易遮掉基础 Def／语言；用独立、唯一文件路径与键。
- **保留硬编码查询再加空兼容目录**：没有迁移真实生产者／消费者，只增加装饰性布局。

实施完成条件在 proposal；本设计未改变当前 LoadFolders 或运行产物。
