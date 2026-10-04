# TODO

## Current goal

- **0.1.0 awaits the maintainer's live acceptance** (matrix below); only after acceptance do tag / GitHub Release / Workshop come into play (each separately authorized).
- The repository is on the cloud (`Coahuilite/Mwah`, development branch `0.1.x`; process: `docs/release-runbook-zh.md`); the offline surface is fully green. History and completed narratives live in `OBLIVIONIS.md` (2026-10-04 section); durable conclusions in `MEMORY.md`.

## Entry point for the next session

- Read `MEMORY.md`'s "Vanilla capability boundaries" and "Naming decisions" sections before touching code.
- Working branch `0.1.x` (local and remote share the name); `main` only advances at release time. Current hash: `git log --oneline -1` — this file pins no hash.
- The only in-flight item is the live matrix below, executed by the maintainer personally. The agent's next step appears in exactly two cases: the matrix reports a defect, or the maintainer authorizes release actions.

## Live verification matrix (maintainer executes; the agent builds no junctions and never runs the game)

### Loading and logging

- [ ] Load: copy the mod into `Mods/` → enable → start with no red lines; after a new game all four files under `1.6/Defs/Kiss/*` parse.
- [ ] Logging surface: (1) start with diagnostics off → Player.log still carries one `[MWAH] Mwah! (Every Pawn Kisses Each Other) v0.1.0 build=dev startup OK` line; (2) diagnostics on → any kiss shows `dev: stage/kiss begin/perform/return` lines and savedata's `Mwah-trace.log` advances every second; (3) turning it off mid-game stops both at once, the banner unaffected; (4) "restore defaults" puts the toggle back to this channel's default (dev package = on).
- [ ] Fate table live self-proof: startup Player.log carries **no** `MWAH_FateDef is not a Def type` line; after kissing a wall the mood entry shows a per-row narration (not the static fallback description) and the short message pops normally; the "fate table edit loop" row presupposes this one.
- [ ] Translation self-proof: after installing the new package, the bottom bar "亲吻导演台", panel title, thought "被{名字}亲吻过", report string "亲吻{名字}。", and right-slot placeholder "（先选发起方）" are all Chinese; re-save the translation report — MWAH entries must be gone from "missing".
- [ ] Clean startup log: no `Dialog_KissDirector probably needs a StaticConstructorOnStartup attribute` warning.

### Basic flow and performance

- [ ] Basic flow: select colonist A, right-click colonist B → menu shows "亲吻B…?!" → both walk to their stage cells, face each other side-by-side, each throws hearts, each walks back to where they stood.
- [ ] Seat-swap deadlock regression: dispatch two colonists standing **tight against each other** → both step aside one cell and kiss on the freed adjacent pair (the old build had them wait on each other until both silently scattered); the log sequence must be `stage: A=… B=…` → `kiss begin` ×2 → `kiss perform` → `kiss return`; any `kiss walk fail`/`kiss abort in walk` line means another pathing problem — report it verbatim.
- [ ] **Side-by-side facing**: any two mobile 1x1 pawns must kiss left-right facing each other, never front-back overlapping or stacked vertically.
- [ ] Kissing a downed pawn: the standing one circles to the downed one's left/right side — still side-by-side.
- [ ] Furniture squeeze fallback: when all side cells are taken by tables/beds/walls, falling back to vertical adjacency (North/South facing) is acceptable, but the kiss must complete rather than spin in place.
- [ ] Passerby steals the stage: another colonist walks into a stage cell mid-kiss → the side that cannot reach its cell gives up after 3 re-issues; it must never wedge permanently.
- [ ] No mutual chasing: each walks to their own stage cell — no circling each other, no seat-swapping waits.
- [ ] Sides unavailable: order in a 1-wide corridor / against a wall → fall back to vanilla Touch adjacency (vertical allowed), never deadlocks.
- [ ] Walk stop-loss: route blocked en route → gives up after ~2 re-issues and either kisses (vertically adjacent) or ends cleanly; `Player.log` must not spam.
- [ ] Drafted facing: order a kiss while drafted → both still face each other and do not both snap South (vanilla `Pawn_RotationTracker.UpdateRotation` forces South when drafted; `handlingFacing` blocks it).
- [ ] Interrupted kisses pay no mood: either side pulled out of the job (death, ordered away) → the other ends immediately and **no** mood settles.
- [ ] Passive-side settlement never lost: complete 10 consecutive full-length kisses (including slow-walking passive setups) → **each bout pays both sides exactly one mood entry** (clock-rework regression).
- [ ] Save compatibility (**new saves only** — old saves incompatible by design): save mid-kiss (especially mid-return-leg) → load → the job continues cleanly or ends cleanly, no `ticksLeft`/`homeX` red lines.
- [ ] Uninstall residue: disable the mod with `MWAH_Kissed*` memories and `MWAH_Kiss`/`MWAH_KissThing` jobs in the save → silent drop or error; decide from the result whether About.xml needs an uninstall note.

### Truce window and combat gate

- [ ] Truce window main case: director-dispatch an **idle** raider/mech to kiss your colonist → from the moment of the order it stops attacking anyone: walks over, kisses, walks back its original route, never re-acquiring targets; hostility resumes on the **tick after the return leg completes**. One shot mid-way → instant break and immediate counterattack (the anti-abuse self-destruct gate working, not a bug).
- [ ] Damage-break timing: dispatch via the director (playerForced path) → fire one shot at either participant at any phase (approach/face-to-face/return) → they break and fight back **on the next tick**, Player.log shows `kiss broken by damage`; control case: non-external-violence damage (falls, friendly heals) does **not** break.
- [ ] Passive side no longer shoots during approach: right-click a kiss on a raider → your pawn is not shot on its way over (the old late-start passive partner shot back mid-approach).
- [ ] Warqueen case: dispatch a colonist to kiss an idle warqueen → while in the job (approach/perform/return) she **neither fires the ChargeBlaster nor spawns war urchins** (both are think-tree job channels, proven by source; seeing either = a new bug). Her already-spawned urchins keep shooting your pawn — the first hit breaks the kiss (previous row); the moment her job ends she may instantly release a fresh litter (the carrier cooldown kept ticking inside the job) — clear the urchins first for a safe kiss.
- [ ] Combat gate: ambient on + a raider inside an attack job (in a firefight) → the **system** never pulls it into a kiss; at the same moment the director can name it → **it kisses** (player-issued overrides the gate). The gate still holds while "ignore kiss cooldowns" is on.
- [ ] Return leg: both walk home inside the job (with "return home" on); blocked en route → 3 re-issues then end in place and the queue keeps delivering; long route → hard cap at 20 real seconds, ends in place, never hangs. Wall-kissers and mechs also return inside the job.

### Gates and decisions

- [ ] Moodless units: right-click an animal / mechanoid / Odyssey drone / anomaly entity → menu label carries "(没有心情系统，只是做做样子)", hearts and animation normal, mood panel gains **nothing**, no exceptions in Player.log.
- [ ] Own animals only get kissed: select a colony animal and right-click anything → no kiss option (vanilla `CanTakeOrder`); it also never initiates via ambient; others kissing it works normally.
- [ ] Mechs can initiate: select a friendly mech, right-click another mech → kissable (the "has a mouth" test deliberately applies to flesh only).
- [ ] Role swap: select a legless/downed pawn, right-click a mobile one → the other walks over instead; both immobile → greyed item says "两个都动不了".
- [ ] Participation-layer greyed items: ordering a sleeping pawn → "在睡着或不清醒"; a downed-but-awake one → kissable as normal (a deliberate distinction); a burning one → greyed.
- [ ] Organs and life stages: a colonist without a jaw initiating → "没有能用来亲的嘴" (as a receiver no restriction); babies cannot initiate but can be kissed.
- [ ] Entities (shambler / ghoul / awoken corpse — **mech-equal treatment**, 2026-10-05 ruling): as receiver, ordering one → the kiss completes (menu label carries the "没有心情系统" suffix), an **entity fate-table message** pops (`MWAH.KissPawn.Entity.*`, log line `fate KissPawnMutant: ... row=MWAH_Fate_PawnEntity_Line_N`), mood panel gains **nothing**; **as initiator, director left slot = idle shambler → it walks over and kisses** (`BeginDirected` skips `CanTakeOrder`, and the 10-05 deletion removed our last semantic gate — right-click still can't, that's vanilla's subhuman rule); admission follows the gate slider (they are Humanlike: tier ≥3 non-hostile, tier ≥4 hostile) — below that expect the ordinary tier wording, the old "已经顾不上" rejection key is gone; spot-check the 10 entity messages: **no body/appearance words** (psychology only — entities may not be humanoid).
- [ ] Ritual absorbed: a target whose hediff sets `blocksSocialInteraction` (Biotech/Odyssey rituals) → "正沉浸在仪式里".
- [ ] Prisoners and slaves: initiating or receiving → mood entries appear normally (vanilla `Mood` gates on neither faction nor bondage).
- [ ] Gate narrowing, tier by tier: drag 6→0 and confirm **exactly one population class drops per notch** (6→5 mechs/drones/void entities, 5→4 wild animals and fleshbeasts, 4→3 hostile humanlikes, 3→2 visitors/traders/allies, 2→1 own animals and colony mechs, 1→0 slaves and prisoners).
- [ ] Gate factory value and wording: a fresh save shows "谁能亲谁: 所有 Pawn 都互相亲吻" / "every pawn kisses each other"; greyed items read "{the out-of-tier side} 不想和 {the other} 亲吻" with **no** system-speak about settings or ranges.
- [ ] Any combination: enemy×enemy, enemy×colonist, animal×mech, downed×standing all work (via panel or quick chain, one BeginDirected gate).
- [ ] Cross-session static reset: kiss a few times → back to main menu → new game/load an old save → ID-colliding pawns must not inherit last session's cooldowns (right-click kisses immediately); no Goto error on the new session's first tick.

### Director panel

- [ ] Panel basics (non-modal): click the bottom bar → a small panel appears centered above the bar, **no pause**, map keeps normal box-select; **arrow panning and wheel zoom must keep working** (the `preventCameraMotion=false` regression point — the first live version killed exactly this); left slot "谁要发起亲吻？", right slot "（先选发起方）", grey heart.
- [ ] Single-hop picking: click the left slot → map picking with the prompt "谁要发起亲吻？" → pick someone → avatar fills the slot, caption becomes "{名字} 要发起亲吻。", right slot becomes "{名字} 想要和谁亲吻？", **left half of the heart turns red**; click the right slot → pick a second pawn **or a wall** (walls show their uiIcon) → **right half red = whole heart red**, caption "{左} 想要和 {右/墙名} 亲吻。"; click the red heart → dispatch (walls take the wall-kiss chain, playerForced). With one side empty the heart does nothing.
- [ ] Heart dispatch failure: pair still gated/cooldown-unreachable → a message with the **real reason** (never silent); re-clicking inside the pair cooldown states the remaining time. Slots stay filled after dispatch.
- [ ] Secondary operations: left-click a filled avatar = re-pick that hop; right-click the left slot = clear it and the right slot with it (the referent changed); right-click the right slot = clear only the right; with the left empty, clicking the right slot does nothing.
- [ ] Quick chain: the panel's bottom button skips the avatar frames — two map hops directly (second hop also accepts a wall), dispatch on the second commit **and echo both into the avatar slots**.
- [ ] Wall fog filter: a fogged wall is unpickable and never highlights; with the wall-kissing switch off the right slot cannot pick walls either (pawns still selectable).
- [ ] Responsiveness: change resolution or UI scale (`Prefs.UIScale`) → the panel re-lays out proportionally; a dragged panel keeps its relative position mapped and clamped on-screen, an untouched one re-anchors above the bar; on the smallest canvas (1024×768) captions stay single-line, never poke the frame, never crowd "快速发配" (the fix: the height formula's missing title row `Margin+25f`); with the panel open the Dev palette shows no red `Word wrap was false at end of frame` (frame-end contract fixed). The vanilla settings window clipping at UIScale>1 is vanilla's fixed 650×600 — test at UIScale 1.0.
- [ ] Drag and close: any spot drags (base `GUI.DragWindow`); × or re-clicking the bar button closes; if picking is still live when closing, the Targeter is collected too (no invisible pick mode left behind); Esc only cancels picking, never closes the panel.
- [ ] Director off via settings: turning off "亲吻导演台按钮" → the bar cell vanishes, an open panel closes immediately, an in-flight pick ends immediately; factory default **on**.
- [ ] Stacked-cell disambiguation: with a dog standing on a person, which one a pick grabs must match what the right-click menu picks at the same spot (both use vanilla `Find.Targeter`).
- [ ] Bar position: the button sits left of the gear and right of all tabs (order=200).
- [ ] Swap boundaries (dispatch side): director-initiating a jawless raider → rejection message and **your colonist is never drafted as a proxy kisser**; with ambient on your drafted colonists never get pulled out of combat.
- [ ] Panel × Dev palette coexistence: with the Dev palette open the panel still pops, picks, and commits (clicks landing on windows never pick; on the map they do); `[MWAH] dev: pick start/ok/end (generation N)` lines pair up, and at a chain hand-off hop one's cleanup is suppressed by the generation token — no spurious end.

### Wall kissing and fate tables

- [ ] Wall basics (fate-table era): select a colonist, right-click a **man-made wall** → "亲吻{墙名}…?!" → walks to a touching cell, faces it, hearts at interval, duration completes → a **five-tier short message** (e.g. "{PAWN} 亲吻了 {WALL}。什么也没有发生——…") plus the matching mood entry (−2/0/+1/+3/+6); **open that memory and its description is the drawn 1-of-10 narration line** — the same pawn kissing several walls shows different descriptions per entry (per-instance, zero patches). Natural rock and buried veins also get the menu; doors/furniture/art do not.
- [ ] Silent invalidation (the addon constitution): with "允许亲吻墙壁" off — right-clicking a wall shows **not even a greyed item**; the director's right slot and quick-chain hop two cannot hit walls (no highlight); a wall already stored in the right slot clears silently; with the menu open, flip the switch off and click the stale item → **nothing pops at all**. No red lines anywhere in Player.log throughout.
- [ ] Fate table edit loop (file-driven acceptance): edit a row's weight in `MWAH_FateDefs.xml` (e.g. all Devoted 160 → 1000) → restart → noticeably more "fell in love with this wall"; delete a row → that narration never appears again; delete **every** scope=KissWall row → the five tiers and messages still fire (built-in fallback distribution), memory descriptions fall back to static text.
- [ ] Wall edges: a wall unreachable from the room center → greyed item says no standable reachable spot exists; already kissing (person or wall) → "已经在亲了"; inside cooldown → remaining time shown; wall-kissing shares the per-pawn cooldown with two-person kisses. **Note: with "ignore kiss cooldowns" on, every cooldown above is skipped (walls included)** — a "walls have no cooldown" report checks this switch first. A wall destroyed mid-performance → the job ends cleanly with no settlement.
- [ ] Wall switch: turning "允许亲吻墙壁" off in settings → the wall menu disappears immediately (menu open, switch off, click the stale item → silent no-op, see the constitution row); factory default **on**; it is independent of the gate slider (gate at the far left still kisses walls).

### Mood and values

- [ ] Mood values: the "被X亲吻过" entry = +5 × own SocialImpact × multiplier, duration matching the "mood duration" setting (factory 60000 t / 1000 s / 24 game hours); a level-10 social pawn's entry is visibly higher than a level-0 one (the 0.82→1.37 curve); mute/deaf pawns score lower (Talking weight 0.9, Hearing 0.3); a hat wearer slightly higher.
- [ ] Stack cap: one pawn kissed by two people in quick succession → exactly 2 entries, summing ≈ +7.5, **below** one lovemakin's +8; a third kiss waits for the first to expire.
- [ ] Psychopath nullify: a colonist with Psychopath (or, with Anomaly, the `Inhumanized` hediff) gets kissed → animation and hearts as normal, but the mood panel shows **no entry** and total mood is untouched (nullification happens at read time; the entry still exists in memory, filtered out — do not treat "panel shows nothing" as a bug).
- [ ] Leaving a mark (`changeOpinion` on): the social tab shows "我们的那一吻" ≈ +6 × the same multiplier, still bound by "mood duration", one entry per pair maximum (`stackLimitForSameOtherPawn`).

### Cooldowns and the settings page

- [ ] Cooldowns: repeatedly kissing one person → greyed item shows the remaining time readable in all three units; pair cooldown at 0 → back-to-back kisses allowed.
- [ ] Numeric fields: every slider has a right-side input box (tick items take ticks); Enter or click-away commits, quantized by step and clamped; garbage/empty re-echoes the old value; the "who may kiss whom" slider has **no** box (a tier is not a quantity).
- [ ] Settings scrolling (re-fixed 2026-09-30): wheel/drag reaches the **very bottom** — "诊断日志" and "恢复默认值" fully visible and clickable; **no** control ghosts beyond the window's right edge (the old bug: overflow entries column-switched outside, the orange restore button floated past the frame); verify in both languages and at two window sizes.
- [ ] Three-unit readout and persistence: every duration row shows `tick / seconds / game hours`; edits take effect immediately and survive reopening; "restore defaults" returns everything to `Constants`.

## Explicitly deferred / never

- [ ] Never: world-map / caravan-tile right-clicks; RimWorld Multiplayer sync; true body-contact kiss pose (needs a custom `PawnRenderNodeWorker` + art — not an API limit).
- [ ] Never: downed pawns as initiators (would need Harmony on `FloatMenuMakerMap.ShouldGenerateFloatMenuForPawn`, conflicting with the zero-Harmony boundary).
