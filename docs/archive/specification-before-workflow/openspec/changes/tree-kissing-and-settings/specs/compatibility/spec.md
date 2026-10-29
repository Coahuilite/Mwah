> 历史快照：迁移前的英文规格／变更资料，来源为 deb427d。已由[现行规格入口](../../../../../../../specification.md)替代；不再定义现行合同，也不表示关联实施变更已经完成。

## MODIFIED Requirements

### Requirement: Supported environment
The mod SHALL support RimWorld 1.6 and 1.7 with Core alone and no required DLC, Harmony, or framework mod. Gameplay SHALL remain live-map and single-player only. World-map actions, caravans, multiplayer synchronization, true body-contact kiss animation, and downed initiators SHALL remain outside the product scope. Basic tree kissing SHALL remain available without Ideology. Optional cultural definitions and execution SHALL be isolated so their absence cannot prevent paired, wall, or basic tree kissing from loading.

#### Scenario: Core-only installation
- **WHEN** the mod is loaded without DLC or other mods on either supported game minor
- **THEN** paired kissing, wall kissing, basic tree kissing, the director, and settings can load without an optional-content dependency error.

#### Scenario: The same gameplay request on supported minors
- **WHEN** equivalent valid inputs and settings are used on supported game minors
- **THEN** admission, cooldown, addon-switch, and settlement semantics remain the same despite version-specific game adapters.

### Requirement: Safe delivery and verification
Build and packaging tools SHALL NOT install or link the mod into a game installation. Packages SHALL retain the established channel shapes and one top-level Mwah directory where archived. Repository content and distributed artifacts SHALL exclude personal local paths, credentials, and Workshop publication identifiers. Existing mandatory offline gates SHALL be retained; their success SHALL NOT be presented as in-game verification.

#### Scenario: An offline package build
- **WHEN** a developer builds or packages the mod
- **THEN** the operation produces its build or distribution output without altering a game installation or claiming a game session passed.

#### Scenario: Runtime owners are refactored
- **WHEN** an approved refactor replaces an internal runtime owner without changing published entry points
- **THEN** the existing assembly inventory check verifies the current owner and retained entry points rather than requiring the removed implementation type.
