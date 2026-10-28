# Compatibility and delivery boundaries

## Purpose

Define supported environments, persistent identities, and packaging constraints.

## Requirements

### Requirement: Supported environment
The mod SHALL support RimWorld 1.6 with Core alone and no required DLC, Harmony, or framework mod. Gameplay SHALL remain live-map and single-player only. World-map actions, caravans, multiplayer synchronization, true body-contact kiss animation, and downed initiators SHALL remain outside the product scope.

#### Scenario: Core-only installation
- **WHEN** the mod is loaded without DLC or other mods
- **THEN** its paired kissing, wall kissing, director, and settings can load without an optional-content dependency error.

### Requirement: Persistent and package identity
The mod SHALL preserve package identity `coahuilite.mwah`, the Mwah engineering namespace and assembly identity, published Def names, settings keys, relationship-slot meanings, and saved narration fields. Product version SHALL originate in the project version property and propagate consistently to declared metadata and release surfaces. Runtime content SHALL be loaded from declared version folders without duplicate assemblies.

#### Scenario: Loading existing configuration and memories
- **WHEN** a user updates the mod while retaining settings and a save
- **THEN** existing preferences and valid memory references retain their meanings rather than silently becoming different features.

### Requirement: Safe delivery and verification
Build and packaging tools SHALL NOT install or link the mod into a game installation. Packages SHALL retain the established channel shapes and one top-level Mwah directory where archived. Repository content and distributed artifacts SHALL exclude personal local paths, credentials, and Workshop publication identifiers. Existing mandatory offline gates SHALL be retained; their success SHALL NOT be presented as in-game verification.

#### Scenario: An offline package build
- **WHEN** a developer builds or packages the mod
- **THEN** the operation produces its build or distribution output without altering a game installation or claiming a game session passed.
