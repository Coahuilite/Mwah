# Changelog

The public release record for Mwah! Maintenance rules:

- Released entries in chronological order, oldest first; the Unreleased entry sits on top.
- Heading uses local release time: `[YYYY-MM-DD HH:MM UTC+8] Version X.Y.Z`; replace `Unreleased — Version X.Y.Z` at publish time.
- The "Initial Workshop Upload" wording is reserved for the first Steam upload.
- One change per bullet, written for players; technical fixes a player cannot feel get one line.
- Keep the Simplified Chinese version synchronized in `docs/CHANGELOG.zh-CN.md`.

## Unreleased — Version 0.2.2

The first public release. The 0.1.x development line was scrapped without ever shipping, so everything arrives at once.

The fine print up front: this is a joke mod. Live maps only, single-player only, no Harmony, no dependencies beyond Core.

### Added

- Right-click a kiss. Select a pawn, right-click another. The one that can walk walks over, they kiss, hearts float up, and both head home.
- Kiss Director. A new bottom-bar button opens a small panel: point at any two units on the map and send them to kiss. Raiders, traders, wild animals - anyone you cannot command can still be arranged.
- Kiss a wall. Right-click a wall, a slab of natural rock, or even a buried ore vein. Walls do not answer back, so the mood is a dice roll: five outcomes, fifty narration lines, all in editable XML.
- Animals, mechanoids and anomaly entities are kissable too. They have no mood to move, but each gets its own brand of commentary: mechs crack cold jokes, animals talk about smells.
- A truce while it lasts. A commanded pair will not fight each other until the kiss is over; one real hit breaks the spell immediately.
- Mood scales with the receiver. The bonus is multiplied by the kissed pawn's own social impact: charming pawns profit, quiet ones get less, psychopaths feel nothing.
- A gate slider. "Who kisses whom" in seven steps, from colonists-only to everything-kissing-everything. Fully open by default.
- Ambient kissing (off by default). Once enabled, the game matchmakes the pawns you cannot order around, on an interval and radius you set. It never touches your colonists.
- The settings page comes in four sections: core, autonomous matchmaking, add-ons, system. Every duration is shown in ticks, real seconds and game hours at once.
- Four diagnostic levels. Quiet during normal play, and worth reading when something looks off.

### Notes

- 0.1.0, 0.2.0 and 0.2.1 were never tagged; the first public release is 0.2.2.
