> 历史快照：迁移前的英文规格／变更资料，来源为 deb427d。已由[现行规格入口](../../../../../specification.md)替代；不再定义现行合同，也不表示关联实施变更已经完成。

# Core kissing

## Purpose

Define Pawn-to-Pawn kissing, its entry points, participation rules, and outcomes.

## Requirements

### Requirement: Independent initiation paths
The mod SHALL offer right-click orders, a non-modal Kiss Director with avatar-slot and quick-chain picking, and optional ambient matchmaking. Right-click SHALL respect vanilla order/menu gates and allow role swapping when necessary. Director and ambient requests SHALL keep the selected initiator.

#### Scenario: A directed non-orderable initiator
- **WHEN** a player names a physically capable, non-orderable Pawn in the director
- **THEN** the request is evaluated without requiring vanilla player order ownership and without swapping roles.

#### Scenario: Ambient matchmaking is independent
- **WHEN** a player orders a kiss while ambient matchmaking is disabled
- **THEN** only that request is dispatched and no automatic follow-up is created.

### Requirement: Ambient selection boundaries
Ambient matchmaking SHALL default off, consider only initiators outside player management (excluding colony animals), avoid drafted player-controlled receivers and fighting participants, and remain Pawn-to-Pawn only. Its configured interval and radius SHALL bound opportunities and target distance rather than guarantee a kiss.

#### Scenario: A nearby colony animal
- **WHEN** ambient matchmaking considers initiators near a colony-owned animal
- **THEN** it does not issue that animal an initiating order, while the director can still explicitly evaluate it under the normal action gates.

### Requirement: Participation and scope
The mod SHALL require distinct, alive, spawned participants that are awake, not burning, and not interaction-blocked by ritual state. Initiators SHALL also be mobile, meet the flesh-race Talking-capacity check, and have an eligible life stage. Mood SHALL affect reward eligibility, not admission. Both Pawns SHALL pass the cumulative seven-tier pairing scope, whose default is unrestricted; no additional faction or species whitelist SHALL be imposed.

#### Scenario: An awake downed receiver
- **WHEN** a mobile initiator targets an awake downed Pawn and all other gates pass
- **THEN** the receiver can participate without becoming an initiator.

#### Scenario: A moodless participant
- **WHEN** a moodless Pawn meets the applicable action gates
- **THEN** its lack of mood does not prevent participation.

### Requirement: Cooldown and command policy
Requests SHALL honor per-Pawn and per-pair cooldowns unless `noCooldowns` is enabled. Player-issued orders SHALL bypass the ambient combat gate, not cooldowns by virtue of being player-issued. Cooldown exemption SHALL never permit an already occupied participant. Cooldowns SHALL be recorded only after job admission and remain session state that resets on game load.

#### Scenario: A repeated manual request
- **WHEN** a player requests the same pair during cooldown with `noCooldowns` disabled
- **THEN** the request is rejected with the real cooldown reason.

#### Scenario: A busy participant with exemption enabled
- **WHEN** `noCooldowns` is enabled and a requested participant is already kissing
- **THEN** the new request is rejected rather than introducing a third participant.

### Requirement: Paired performance and departure
A paired kiss SHALL use mirrored participant jobs with a single initiator-owned clock and settlement. Participants SHALL remain under kiss-job ownership through approach, performance, and enabled departure; interrupting external violence SHALL break that ownership. Interrupted performance SHALL not grant completion rewards.

#### Scenario: Violence during the kiss job
- **WHEN** interrupting external violence reaches either participant during approach, performance, or departure
- **THEN** that participant's kiss job ends without a global faction or hostility rewrite.

#### Scenario: Stage selection and fallback
- **WHEN** a pair is assigned positions
- **THEN** stage selection prefers separate horizontal cells, keeps an immobile receiver in place, rejects exchanging occupied cells, and permits a touch fallback.

#### Scenario: Save and load during a valid kiss
- **WHEN** a valid ongoing kiss is saved and loaded
- **THEN** participant roles, remaining performance time, and home positions retain their meanings, and completion is not settled twice.

### Requirement: Relationship-based settlement
Only Pawns with a runtime mood-memory handler SHALL receive thoughts. The paired base mood value SHALL be 5, scaled by that Pawn's own SocialImpact and the configured multiplier, with vanilla thought nullification respected. Opinion changes SHALL default off. The mod SHALL NOT introduce bespoke trait or xenotype kiss-value tables.

#### Scenario: Default mood-only settlement
- **WHEN** a completed pair is settled with opinion changes disabled
- **THEN** eligible participants receive mood memories, no opinion offset is written, and moodless participants use their applicable message channel instead.

### Requirement: Relationship slots and narration
Memories SHALL group by partner without an additional mod-wide cross-partner cap; same-partner renewal SHALL not create duplicate relationship slots. Shared fate-table draws SHALL avoid the same row within the event when alternatives exist. Narration SHALL persist with the memory instance.

#### Scenario: A one-row narration table
- **WHEN** both participants use a fate table with only one valid row
- **THEN** both can receive that row rather than suppressing the second outcome.
