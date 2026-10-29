> 历史快照：迁移前的英文规格／变更资料，来源为 deb427d。已由[现行规格入口](../../../../../specification.md)替代；不再定义现行合同，也不表示关联实施变更已经完成。

# Settings

## Purpose

Define the settings surface, configuration identity, and value lifecycle.

## Requirements

### Requirement: Settings coverage
The settings UI SHALL expose the master enable preference; Pawn-pair scope; kiss duration; heart interval; per-Pawn and per-pair cooldowns; paired-thought duration and mood multiplier; opinion effects; return-home behavior; cooldown exemption; director visibility; ambient enable, interval, and radius; each registered addon's enable preference and thought duration; diagnostic level; and restore-defaults action.

#### Scenario: Adjusting an addon independently
- **WHEN** a player changes an addon's thought duration
- **THEN** that preference does not overwrite the paired-thought duration or another addon's preference.

### Requirement: Settings organization
The UI SHALL present a pinned identity header and a scrollable, single-column body organized into core, ambient matchmaking, addons, and system sections. The master switch SHALL precede the sections; addon controls SHALL be generated from registered addons. The identity header SHALL remain readable while scrolling.

#### Scenario: Scrolling to addon controls
- **WHEN** a player scrolls to the addon section
- **THEN** the build identity remains visible and every registered addon retains an accessible enable control.

### Requirement: Value editing and persistence
Settings SHALL become authoritative in memory immediately, use coalesced disk persistence, flush pending writes on close, and retain dirty state for retry after save failure. Numeric inputs SHALL commit on Enter or focus loss, quantize and clamp valid values, and discard invalid edits without replacing the stored value.

#### Scenario: Invalid numeric text
- **WHEN** a player leaves a numeric field containing invalid text
- **THEN** the previous value is displayed and remains the stored value.

### Requirement: Units and configuration identity
All durations SHALL be stored and edited in ticks and displayed with ticks, real seconds, and game hours. Tier controls SHALL not expose quantity inputs. Existing settings keys and stable addon keys SHALL retain their meanings. Restore defaults SHALL reset fields and addon overrides.

#### Scenario: Restoring addon defaults
- **WHEN** the player restores defaults after changing addon preferences
- **THEN** addon overrides and ordinary fields return to defaults without changing their persisted identities.

### Requirement: Diagnostics and localization
Diagnostics SHALL offer Off, Auto, Simple, and Verbose, defaulting to Auto. Auto SHALL resolve to Verbose for dev builds and Simple for distribution builds. Startup identity and errors SHALL remain visible regardless of that preference. Player-facing settings text SHALL ship with matching English and ChineseSimplified key sets, preserve the Mwah! brands, and use Pawn as the dispatched actor noun.

#### Scenario: Diagnostics are disabled
- **WHEN** the diagnostic setting is Off
- **THEN** ordinary diagnostic output is suppressed while startup identity and error reporting remain available.

### Requirement: Diagnostic channels
Ordinary diagnostic output SHALL use Player.log. The existing crash-phase sampler SHALL remain a distinct Verbose-only crash-diagnosis exception, not a second general logging system. The mod SHALL NOT introduce another ordinary log sink or export workflow.

#### Scenario: Verbose diagnostics are enabled
- **WHEN** a player selects Verbose diagnostics
- **THEN** ordinary logs remain in Player.log and the crash sampler can record its dedicated phase samples without changing the ordinary logging channel.
