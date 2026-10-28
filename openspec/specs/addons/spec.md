# Thing-kiss addons

## Purpose

Define independently controlled non-Pawn targets and their single-actor kissing behavior.

## Requirements

### Requirement: Independent target controls
Each addon SHALL have a stable identity, its own enable preference and thought duration, and an effective enable state subordinate to the master switch. Addons SHALL be available through right-click, director slots, and quick-chain picking, but not ambient matchmaking. The Pawn-pair scope slider SHALL NOT gate non-Pawn targets.

#### Scenario: Disabling an addon while a menu is open
- **WHEN** an addon is disabled after its menu item was generated and that stale item is activated
- **THEN** no job, rejection notification, or disabled replacement item is produced.

#### Scenario: A target is unavailable
- **WHEN** a target is disabled, destroyed, despawned, or fogged
- **THEN** it disappears from applicable picking surfaces, and stale dispatch after disabling or target loss remains silent.

#### Scenario: An addon is disabled during an admitted job
- **WHEN** an addon is disabled after its job has been admitted
- **THEN** no new request is admitted, but the existing job can finish normally.

### Requirement: Shared single-actor execution
Thing-kisses SHALL use the common participation, initiation, cooldown, facing, heart-effect, damage-interruption, and return-home rules. Only the Pawn SHALL run a job. A target SHALL be reserved against concurrent kisses, and a reachable stand cell SHALL be selected near it. Completion SHALL settle once; cancellation before completion SHALL not grant the addon reward.

#### Scenario: A target disappears during performance
- **WHEN** a non-Pawn target is destroyed before the performance completes
- **THEN** its job is canceled without granting a completion outcome.

### Requirement: Wall target and outcomes
The wall addon SHALL identify game-marked walls and natural rock, including eligible mineable rock. Its stable settings identity SHALL remain `wall`. It SHALL default enabled and use one wall-relationship slot with five stages. Wall outcomes SHALL NOT multiply by SocialImpact or add the paired-kiss psychopath nullifier. Empty fate tables SHALL use the default wall distribution.

#### Scenario: A new wall outcome replaces an old stage
- **WHEN** a Pawn with a wall memory completes another wall kiss
- **THEN** its existing wall slot receives the new stage, narration, and renewed configured duration instead of adding another wall slot.

#### Scenario: Default wall fate content
- **WHEN** default wall content or its empty-table fallback selects an outcome
- **THEN** mood values -2, 0, 1, 3, and 6 have respective probabilities 20, 30, 25, 17, and 8 percent.

### Requirement: Editable fate content
Addon outcomes SHALL be defined by weighted XML fate content with localized narration and optional messages. Missing mood SHALL suppress thought grants, not the action or an applicable message. Addon thought groups SHALL remain independent of the paired-kiss group and other target classes.

#### Scenario: A moodless Pawn kisses a wall
- **WHEN** a moodless Pawn completes an admitted wall kiss
- **THEN** the applicable fate message can be emitted without creating a thought or another substitute resource reward.
