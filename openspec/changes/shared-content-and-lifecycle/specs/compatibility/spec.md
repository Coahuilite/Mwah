# 共享内容加载根采用差量

状态：2026-11-09 维护者核对后采用，见[采用记录](../../adoption.md)。原生加载位置的保证确有变化，故使用 MODIFIED；本文保留采用差量，唯一现行规则为 `openspec/specs/compatibility/spec.md`。其他玩法和持久身份不变。

## MODIFIED Requirements

### Requirement: NATIVE-02 持久与包身份
模组 SHALL 保留 `coahuilite.mwah`、Mwah 命名空间／程序集、已发布 Def 名、设置键、关系槽及旁白存档字段的含义。产品版本 SHALL 源于项目版本属性并一致传播至元数据及发布表面。运行内容 SHALL 仅从 LoadFolders 显式声明的加载根采用；共同 Def／语言 SHALL 来自单一共享内容根，可选兼容内容 SHALL 按声明条件加载。原生程序集 SHALL 按当前游戏 minor 互斥选择，不重复加载程序集。

#### Scenario: 保留配置及记忆更新模组
- **WHEN** 更新时保留配置和存档
- **THEN** 既有偏好及有效记忆引用保留含义，不悄悄变成其他功能。

#### Scenario: 两个目标 minor 消费共享内容
- **WHEN** 在任一受支持 minor 中加载完整分发包
- **THEN** 采用同一共享内容根和该 minor 的原生 Mwah.dll，不加载另一 minor 的程序集或旧复制内容。

#### Scenario: 可选兼容未启用
- **WHEN** VNPE 未启用
- **THEN** 不采用 VNPE 兼容根，保留共同内容与当前 minor 原生程序集的唯一有效加载。

#### Scenario: 可选兼容启用
- **WHEN** VNPE 已启用
- **THEN** 只采用一次其兼容根，共同内容与当前 minor 原生程序集仍唯一有效加载。
