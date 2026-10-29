> 历史快照：迁移前的英文规格／变更资料，来源为 deb427d。已由[现行规格入口](../../../../../../../specification.md)替代；不再定义现行合同，也不表示关联实施变更已经完成。

## MODIFIED Requirements

### Requirement: Cooldown and command policy
Requests SHALL honor per-Pawn and per-pair cooldowns unless `noCooldowns` is enabled. Player-issued orders SHALL bypass the ambient combat gate, not cooldowns by virtue of being player-issued. Exemption SHALL bypass only cooldowns, not occupancy, participation, or reachable staging. Cooldowns SHALL be recorded only after job admission and remain session state that resets on game load. Ambient preprocessing SHALL honor the same exemption.

#### Scenario: A repeated manual request
- **WHEN** a player requests the same pair during cooldown with `noCooldowns` disabled
- **THEN** the request is rejected with the real cooldown reason.

#### Scenario: A busy participant with exemption enabled
- **WHEN** `noCooldowns` is enabled and a requested participant is already kissing
- **THEN** the new request is rejected rather than introducing a third participant.

#### Scenario: Ambient cooldown exemption
- **WHEN** an otherwise eligible ambient initiator has a remaining cooldown and `noCooldowns` is enabled
- **THEN** preprocessing does not discard that initiator solely because of cooldown.

#### Scenario: An unreachable receiver with exemption enabled
- **WHEN** `noCooldowns` is enabled but no reachable staging is available for a requested pair
- **THEN** the request is not admitted and is not reported as a started kiss.

### Requirement: Paired performance and departure
A paired kiss SHALL use mirrored participant jobs with a single initiator-owned clock and settlement. Participants SHALL remain under kiss-job ownership through approach, performance, and enabled departure; interrupting external violence SHALL break that ownership. Interrupted performance SHALL not grant completion rewards. Timing SHALL begin only when both matching participants are in position. Deferred return SHALL respect normal departure and newer player commands.

#### Scenario: Violence during the kiss job
- **WHEN** interrupting external violence reaches either participant during approach, performance, or departure
- **THEN** that participant's kiss job ends without a global faction or hostility rewrite.

#### Scenario: Interrupted departure releases ownership
- **WHEN** violence or a replacement order interrupts departure away from home
- **THEN** no automatic return job is inserted ahead of the Pawn's response or replacement order.

#### Scenario: Stage selection and fallback
- **WHEN** a pair is assigned positions
- **THEN** stage selection prefers separate horizontal cells, keeps an immobile receiver in place, rejects exchanging occupied cells, and permits a touch fallback.

#### Scenario: Save and load during a valid kiss
- **WHEN** a valid ongoing kiss is saved and loaded
- **THEN** participant roles, remaining performance time, and home positions retain their meanings, and completion is not settled twice.

#### Scenario: One participant arrives first
- **WHEN** the initiator reaches its position while the matching receiver is still approaching
- **THEN** the initiator waits without advancing the performance clock or settling a reward.

#### Scenario: The partner is replaced before completion
- **WHEN** a participant loses its matching mirrored job before performance completes, even if that Pawn starts another kiss job
- **THEN** the remaining job cannot count the replacement as its partner or grant completion rewards.

#### Scenario: Normal return is still moving
- **WHEN** performance ends away from home and normal departure is active
- **THEN** fallback processing leaves that departure under the kiss job rather than replacing it with Goto.

#### Scenario: A player gives a new order
- **WHEN** a deferred return remains after a player has replaced the originating kiss with another order
- **THEN** the deferred return does not take ownership back from the new order.

## ADDED Requirements

### Requirement: Truthful dispatch outcome
A dispatch SHALL report success only after its required participant jobs have actually been admitted. A failed second paired-job admission SHALL leave no orphaned newly admitted partner job. Proposal eligibility SHALL NOT be reported as successful dispatch on its own.

#### Scenario: Reservation prevents admission
- **WHEN** eligibility passes but reservation or job admission fails
- **THEN** the director does not announce that the kiss started, and no unmatched newly started partner job remains.

### Requirement: Cross-target occupancy
All kiss entry points SHALL treat participation in a paired or Thing kiss, including its normal departure, as occupied. Enabling a different addon or cooldown exemption SHALL NOT bypass that exclusion.

#### Scenario: Pairing a Pawn that is kissing a wall
- **WHEN** a new paired-kiss request names a Pawn already executing a wall kiss or its normal departure
- **THEN** it is rejected as occupied rather than interrupting the wall activity.

#### Scenario: Pairing a Pawn that is kissing a tree
- **WHEN** a new paired-kiss request names a Pawn already executing a tree kiss
- **THEN** it is rejected as occupied rather than interrupting the tree activity.
