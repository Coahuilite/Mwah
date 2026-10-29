> 历史快照：迁移前的英文规格／变更资料，来源为 deb427d。已由[现行规格入口](../../../../../../../specification.md)替代；不再定义现行合同，也不表示关联实施变更已经完成。

## MODIFIED Requirements

### Requirement: Settings organization
The UI SHALL present a pinned identity header, an immediately accessible master switch, and two tabs: gameplay/addons and parameters/miscellaneous. The default gameplay/addons tab SHALL place addon management before common controls. Addon controls SHALL come from registered descriptions rather than a main-page list of target types. Each tab SHALL have an independent scroll position.

#### Scenario: Scrolling to addon controls
- **WHEN** a player scrolls to the addon controls
- **THEN** build identity remains visible and every registered addon retains an accessible enable control.

#### Scenario: Opening settings to manage extensions
- **WHEN** a player opens the settings surface
- **THEN** the default tab prominently exposes registered addon enable controls before detailed global numeric tuning, with identity and navigation outside the scrolling body.

#### Scenario: Navigating between pages
- **WHEN** a player changes tabs after scrolling
- **THEN** the destination page restores its own position without inheriting the other page's content height or scroll offset.

#### Scenario: Finding common controls and detailed parameters
- **WHEN** a player selects a settings tab
- **THEN** gameplay/addons contains Pawn-pair scope, director visibility, ambient enable, opinion effects, return-home behavior, and cooldown exemption.
- **AND** parameters/miscellaneous contains kiss and heart timing, cooldown durations, paired-thought duration and multiplier, ambient interval/radius, diagnostics, unit guidance, and a clearly global restore-defaults action.

### Requirement: Value editing and persistence
Settings SHALL become authoritative in memory immediately, use coalesced disk persistence, flush pending writes on close, and retain dirty state for retry after save failure. Numeric inputs SHALL commit on Enter or focus loss, quantize and clamp valid values, and discard invalid edits without replacing the stored value. Hiding a control SHALL resolve its pending edit first. Navigation and editing state SHALL NOT become gameplay configuration.

#### Scenario: Invalid numeric text
- **WHEN** a player leaves a numeric field containing invalid text
- **THEN** the previous value is displayed and remains the stored value.

#### Scenario: A valid edit is followed by a tab change
- **WHEN** a player types a valid value and changes tabs without pressing Enter
- **THEN** the value is committed with the existing quantization/clamping rules and included in settings persistence.

#### Scenario: An addon editor is collapsed or settings close
- **WHEN** the active numeric editor is hidden by collapsing its addon or closing settings
- **THEN** its pending edit is resolved with the same valid/invalid input rules before persistence.

#### Scenario: Non-finite numeric input
- **WHEN** numeric text parses as NaN or infinity
- **THEN** it is treated as invalid and cannot replace a stored value.

#### Scenario: Enter commits the active numeric field
- **WHEN** the player presses Enter after typing a valid value in the active numeric field
- **THEN** the canonical value is updated without requiring another click or leaving the field first.

#### Scenario: Debounced persistence while editing
- **WHEN** a prior setting change reaches its debounced save time while a numeric field is still being edited
- **THEN** persistence does not commit that active edit before Enter, focus loss, hiding the control, or closing settings.

## ADDED Requirements

### Requirement: Contextual addon management
Each addon row SHALL retain its enable control while disabled and provide an expandable area for its duration and addon-specific options. The tree row SHALL expose cultural integration as a subordinate option and show DLC availability. Disabling the master switch or an addon SHALL retain stored subordinate preferences.

#### Scenario: Ideology is unavailable
- **WHEN** settings are opened without Ideology enabled
- **THEN** the tree row remains configurable for basic kissing and the cultural option explains its availability without a map-level rejection notification.

#### Scenario: Temporarily disabling tree kissing
- **WHEN** a player disables and later re-enables tree kissing
- **THEN** its saved duration and cultural preference are retained.
