## MODIFIED Requirements

### Requirement: Supported environment
The mod SHALL support RimWorld 1.6 and 1.7 with Core alone and no required DLC, Harmony, or framework mod. Gameplay SHALL remain live-map and single-player only. World-map actions, caravans, multiplayer synchronization, true body-contact kiss animation, and downed initiators SHALL remain outside the product scope. Basic tree kissing SHALL remain available without Ideology. Optional cultural definitions and execution SHALL be isolated so their absence cannot prevent paired, wall, or basic tree kissing from loading.

#### Scenario: Core-only installation
- **WHEN** the mod is loaded without DLC or other mods on either supported game minor
- **THEN** paired kissing, wall kissing, basic tree kissing, the director, and settings can load without an optional-content dependency error.

#### Scenario: The same gameplay request on supported minors
- **WHEN** equivalent valid inputs and settings are used on supported game minors
- **THEN** admission, cooldown, addon-switch, and settlement semantics remain the same despite version-specific game adapters.
