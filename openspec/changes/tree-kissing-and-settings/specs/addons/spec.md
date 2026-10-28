## ADDED Requirements

### Requirement: Stable addon resolution
Addon registration SHALL provide stable identities, deterministic target resolution, and clear handling of duplicate identities or ambiguous equal-priority matches. An admitted Thing-kiss SHALL retain its selected addon identity across save/load rather than reclassifying its target at settlement. Shared menus and picking SHALL support the registered target categories without hard-coded wall text. Adding a target family SHALL NOT require changing paired-kiss behavior.

#### Scenario: Two addons accept a target
- **WHEN** registered target rules overlap
- **THEN** declared precedence selects the addon deterministically, or ambiguous equal precedence is diagnosed before dispatch rather than depending on incidental registration order.

### Requirement: Basic tree kissing
The tree addon SHALL accept spawned, non-destroyed game-marked tree Plants, including eligible loaded content. It SHALL have independent enable/duration preferences and use shared Thing-kiss execution. Basic action, narration, and applicable mood settlement SHALL work without Ideology. No minimum maturity, culture, faction, or extra species gate SHALL be imposed.

#### Scenario: Core-only tree request
- **WHEN** a player requests an eligible tree kiss without Ideology enabled
- **THEN** the action can complete and produce a basic tree outcome without optional-content errors.

#### Scenario: Tree kissing is disabled
- **WHEN** the tree addon's enable preference is off
- **THEN** tree targets disappear from map picking and stale dispatch remains silent, while the settings entry remains available for re-enabling.

### Requirement: Tree defaults and identity
Tree kissing SHALL default enabled, with a thought duration of 30000 ticks and cultural integration preferred on when available. Stable identities SHALL be `tree` for addon preferences, `treeCulturalIntegration` for culture preference, `MWAH_KissTree` for the job, and `MWAH_KissedTree` for the shared memory. New preferences SHALL NOT change stored wall preferences.

#### Scenario: New tree preferences
- **WHEN** an existing configuration has no tree entries
- **THEN** basic tree kissing is enabled with a 30000-tick memory; culture is used only when its effective DLC and Pawn predicates match.

### Requirement: Tree outcome distributions
Tree memories SHALL use five stages with mood values -2, 0, 1, 3, and 6, without SocialImpact scaling or a bespoke psychopath nullifier. Default relative stage weights SHALL be as follows; an empty selected table SHALL use the same distribution and tree slot.

| Policy | -2 | 0 | 1 | 3 | 6 |
| --- | --- | --- | --- | --- | --- |
| Basic | 10 | 30 | 35 | 20 | 5 |
| Tree-affirming | 3 | 17 | 35 | 30 | 15 |
| Own connected Gauranlen | 0 | 10 | 25 | 40 | 25 |

#### Scenario: Cultural likelihood without a higher reward ceiling
- **WHEN** default basic, tree-affirming, and own-connected tables are used
- **THEN** positive-stage probabilities are respectively 60, 80, and 90 percent, with the same maximum mood value of 6.

#### Scenario: Empty cultural table
- **WHEN** the selected cultural fate table contains no positive-weight rows
- **THEN** its fallback uses that policy's specified weights, not the wall distribution or an additional reward.

### Requirement: Tree relationship memory
Basic and cultural outcomes SHALL share one tree-relationship slot, independent of the wall slot. A later tree outcome SHALL replace the slot's stage, narration, and duration rather than adding a second tree slot.

#### Scenario: Switching from cultural to basic settlement
- **WHEN** a Pawn with a cultural tree memory completes a basic tree kiss
- **THEN** the same tree slot is renewed with the basic result, without adding a cultural/basic pair of memories.

### Requirement: Optional cultural tree settlement
Tree kissing SHALL offer a separate cultural-integration preference. Ideology absence, a disabled integration preference, or a Pawn with no matching culture SHALL select basic tree settlement without blocking the action. Matching SHALL use the acting Pawn's own culture.

#### Scenario: Integration is deliberately disabled
- **WHEN** Ideology is active and the player disables cultural integration while leaving tree kissing enabled
- **THEN** tree kisses continue with basic settlement.

#### Scenario: Different cultures in one colony
- **WHEN** two Pawns with different cultural precepts kiss the same tree
- **THEN** each outcome uses that Pawn's culture, not the colony's primary culture.

#### Scenario: Culture changes before settlement
- **WHEN** an admitted tree kiss completes after cultural integration was disabled
- **THEN** it settles once using the basic policy; existing memories are not deleted by the switch.

### Requirement: Cultural outcome selection
`Trees_Desired` SHALL select tree-affirming narration and a greater positive-outcome likelihood than basic settlement. A special own-Gauranlen outcome SHALL require `GauranlenConnection_Strong` and an actual connection to that Pawn. Overlapping meme/precept rules SHALL select one policy, not duplicate bonuses. Tree-cutting disapproval alone SHALL NOT determine kissing value.

#### Scenario: Matching both a meme and its precept
- **WHEN** a Pawn matches a tree-affirming precept and its associated meme
- **THEN** one cultural policy is selected rather than awarding the same cultural benefit twice.

#### Scenario: Both cultural predicates match
- **WHEN** the Pawn has `GauranlenConnection_Strong`, the target is connected to that Pawn, and `Trees_Desired` also matches
- **THEN** exactly the own-connected policy is selected, before the general tree-affirming policy.

#### Scenario: Another Pawn's Gauranlen
- **WHEN** a Gauranlen target is unconnected or connected to another Pawn
- **THEN** settlement falls through to the acting Pawn's `Trees_Desired` policy or basic policy without rejecting the kiss.

### Requirement: Cultural integration preserves native tree systems
Cultural tree settlement SHALL NOT manufacture tree sightings, grant a native situational thought as a memory, perform pruning or linking, change connection strength or production, or claim ritual/planting events. It SHALL NOT add resource substitutes for moodless Pawns. Turning integration off SHALL leave vanilla environmental culture effects intact.

#### Scenario: Kissing a connected Gauranlen tree
- **WHEN** a Pawn completes a culturally matched kiss of its connected tree
- **THEN** it can receive its tree-kiss outcome without changing connection strength, dryad production, or ritual progress.
