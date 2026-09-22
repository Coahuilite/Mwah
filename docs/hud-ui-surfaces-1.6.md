# Player-Visible HUD / UI Surfaces Available to a Zero-Harmony RimWorld 1.6 Mod

**Scope:** authoritative inventory of surfaces a mod can use for an entry point or status/feedback element, verified against the RimWorld 1.6 (Odyssey) reference source + Core/DLC def index.
**Verification method (two independent passes).** (1) The rimsage "RimWorld 1.6 Def and Source Index" — decompiled source tree + `Defs/Core`, `Defs/Ideology`, `Defs/Biotech`, `Defs/Anomaly`, `Defs/Royalty`, `Defs/Odyssey`; this is what proves `private`/`virtual` modifiers and registration mechanics. (2) A metadata dump of `Krafs.Rimworld.Ref 1.6.4871` `ref/net472/Assembly-CSharp.dll` (type + member + method-attribute enumeration, run by the main agent after the fact). Where the two disagree, the doc states both; members written as `METH Public …` / `FIELD …` below are from pass (2). Two of this document's original claims were corrected that way: `TipSetDef`/`GameplayTipWindow` **do** exist in 1.6 (the tips surface is alive, see §1.18), and `Verse.Pawn`/`Thing.GetGizmos` are public virtuals, not the non-existent `Pawn_GetGizmos` helper. Items neither pass could confirm stay marked **UNVERIFIED**.

**Correction to the assignment's premise up front:** several "known 1.6 systems" named in the task **do not exist in 1.6**:

| Claimed type | Status in 1.6 | Evidence |
|---|---|---|
| `GameUIRootDef`, `GameUITabDef`, `GameUIDataDef`, `UIDataFolderDef` | **DOES NOT EXIST** | source search `GameUIRoot|UIDataFolder|GameUITab|GameUIData` → 0 hits; only `WindowLayer.GameUI` (enum member, `Verse/WindowLayer.cs:5`) exists |
| `MainTabDef`, `ITabWindow`, `Dialog_InspectTab` | **DOES NOT EXIST** | word-boundary search `\bMainTabDef\b|\bITabWindow\b|\bDialog_InspectTab\b` → 0 hits |
| `ArchitectGridDef`, `ArchitectPatternDef`, `ArchitectTabDef` | **DOES NOT EXIST** (real system is `DesignationCategoryDef`) | `class ArchitectGridDef` → 0 hits; only `ArchitectCategoryTab` (`RimWorld/ArchitectCategoryTab.cs:7`) + `MainTabWindow_Architect` (`RimWorld/MainTabWindow_Architect.cs:10`) |
| `CompGetGizmoExtra` (as a type), `Pawn_GetGizmos`, `JobGiver_GetGizmos`, `ThingOwner<Gizmo>` | **DO NOT EXIST** | the hook is `virtual ThingComp.CompGetGizmosExtra()` (`Verse/ThingComp.cs:109`); gizmos flow as plain `IEnumerable<Gizmo>` (`Verse/Thing.cs:1404`, `Verse/Pawn.cs:4587`); `Pawn_GetGizmos`/`JobGiver_GetGizmos` → 0 hits |
| `TipTransmitter`, `TipDef` | **REMOVED** as types. The *tutorial-tip* pair is gone; what lives in 1.6 instead is the **data-driven `Verse.TipSetDef`** (`Defs/Core` has `GameplayTips`, see `RimWorld.TipSetDefOf.GameplayTips`) drawn by `Verse.GameplayTipWindow`, plus the separate `ConceptDef` learning system. | source search `TipDef`/`TipTransmitter` → 0 hits; metadata: `Verse.TipSetDef : Def` (FIELD `tips`), `Verse.GameplayTipWindow` (FIELDS `allTipsCached`, `tipUpdateInterval`; METH `DrawWindow`), `RimWorld.TipSetDefOf.GameplayTips`, `Verse.ActiveTip.DrawTooltip` |
| `MainDialogCrossMap` | **DOES NOT EXIST** in 1.6 | `class MainDialog_|MainDialogCrossMap` → 0 hits; dialogs are plain `Window` subclasses opened via `public void WindowStack.Add(Window)` (`Verse/WindowStack.cs:358`) |
| top-left "news/event ticker" | **DOES NOT EXIST** (never has) | no ticker type; nearest surfaces are the bottom-right letter stack (`Verse/LetterStack.cs`) and toast messages (`RimWorld/FloatMenuMakerMap.cs:53` calls `Messages.Message(..., MessageTypeDefOf.RejectInput, ...)`) |

---

## 1. Verified extensible surfaces (no Harmony)

### 1.1 Bottom menu bar — `MainButtonDef` / `MainButtonWorker` (what the mod uses today)
- **What/where:** horizontally-laid buttons in the bottom bar, drawn by `MainButtonsRoot` (`RimWorld/MainButtonsRoot.cs:13,33` — `DefDatabase<MainButtonDef>.AllDefs.OrderBy(x => x.order)`; mods are first-class members of this list).
- **Verified members** (`RimWorld/MainButtonDef.cs`): `workerClass`, `tabWindowClass`, `buttonVisible`, `order`, `defaultHotKey`, `canBeTutorDenied`, `validWithoutMap`, `minimized`, `iconPath`, `closesWorldView`, `validWithClassicIdeo`, `hotKey` (auto-generated `KeyBindingDef` via `Verse/KeyBindingDefGenerator.cs:33`), `Worker`, `TabWindow`.
- **Extension cost:** pure XML `<MainButtonDef>` + optional C# worker. Dynamic visibility via `public virtual bool MainButtonWorker.Visible` (`RimWorld/MainButtonWorker.cs`) — the mod already uses this correctly (`MainButtonWorker_KissDirector.Visible`), which is the supported pattern (vanilla research-tab button does the same via a worker subclass).
- **Vanilla order landscape** (`Defs/Core/Misc/MainButtonDefs/MainButtons.xml`): Architect=1, Work=10, Schedule=20, Assign=30, Animals=40, Wildlife=50, Research=60, Quests=65, World=70, History=80(minimized), Factions=90(minimized), Menu=500(minimized). The mod's `order=200` is accurate and lands it between tabs and the gear. `minimized` (true) parks the button in the bottom-right "+" overflow — reduces crowding.
- **Risk:** every bar mod competes for the same strip; labels are truncated (`ShortenedLabelCap`). Moderate crowding; conflicts only cosmetic (two mods both at order 200 → stable but adjacent).

### 1.2 Gizmo strip (bottom, above the bar, when something is selected) — `Gizmo`
- **What/where:** command buttons under the map when a pawn/thing/zone is selected; `abstract class Gizmo` (`Verse/Gizmo.cs`) — `GizmoOnGUI(Vector2, float, GizmoRenderParms)`, `GetWidth`, `virtual bool Visible`, `virtual float Order`, `virtual bool Disabled`, `alsoClickIfOtherInGroupClicked`, `const float Height = 75f`.
- **Extension hooks, all virtual (no patching):**
  - **`public ThingComp.CompGetGizmosExtra()`** — independently re-verified against `Krafs.Rimworld.Ref 1.6.4871` metadata as `METH Public CompGetGizmosExtra` on `Verse.ThingComp`. Aggregate point: `ThingWithComps.GetGizmos()` yields comps' gizmos, and a comp is attached to any pawn/building by XML `<comps>` on its ThingDef (`Verse.ThingDef.comps`). **There is no `CompGetGizmoExtra` *interface* type in 1.6** (metadata: zero `TYPE …GizmoExtra` besides compiler-generated iterators) — you override the method, you do not implement an interface.
  - `virtual IEnumerable<Gizmo> Hediff.GetGizmos()` (`Verse/Hediff.cs:759`) — a "kiss status" hediff could show a contextual gizmo. Verified present in metadata.
  - `Thing.GetGizmos()` virtual (`Verse/Thing.cs:1404`); `ISelectable.GetGizmos()` (`Verse/ISelectable.cs:7`). Verified: `METH Public GetGizmos` on `Verse.Thing` and `Verse.Pawn`.
- **Dynamic visibility:** yes — `Gizmo.Visible`/`Disabled` are re-evaluated every frame the strip draws; gated per game-state freely.
- **Risk:** only visible when the right thing is selected → poor *entry point* (you must already be looking at that pawn), excellent *per-pawn feedback*. Strip crowding is real with other mods; keep width small. Adding a comp to the `Human` ThingDef also adds a `CompTick`-bearing object to every human pawn in every map — do not tick it.

### 1.3 Inspect-pane tabs — `ITab` / `InspectTabBase` (data-list registration)
- **What/where:** the tab strip of the selection info pane. `abstract class InspectTabBase` (`Verse/InspectTabBase.cs:6`) with `labelKey`, `size`, `tutorTag`, `virtual bool IsVisible`, `abstract FillTab()`, `TabTick/TabUpdate`; `abstract class ITab : InspectTabBase` (`RimWorld/ITab.cs:8`).
- **Registration, no patching:** `List<Type> ThingDef.inspectorTabs` (`Verse/ThingDef.cs:241`) is resolved into `inspectorTabsResolved` at load (`Verse/ThingDef.cs:1653-1665` via `InspectTabManager.GetSharedInstance`), shown when `Thing.GetInspectTabs()` returns them (`Verse/Thing.cs:1467-1469`; consumed by `MainTabWindow_Inspect.CurTabs`, `RimWorld/MainTabWindow_Inspect.cs:96-120`). `WorldObjectDef.inspectorTabs` also exists (`RimWorld/WorldObjectDef.cs:45`).
- **Dynamic visibility:** yes — `InspectTabBase.IsVisible` is virtual and checked in `DoTabGUI`/tab bar.
- **How a mod adds it without touching vanilla defs:** its *own* ThingDef declares `<inspectorTabs>`; to put a tab on *colonists* you'd XML-patch human/pawn ThingDefs (`PatchOperation` — allowed, still no Harmony) or attach a gizmo/comp instead. Order is list order of the def — mods can't re-order vanilla tabs.
- **Risk:** mid (a full tab is a heavy commitment; a "Kiss log" tab is plausible but competes with 12+ vanilla pawn tabs).

### 1.4 Architect menu (bottom bar → "architect" grid) — `DesignationCategoryDef`
- **What/where:** the grid pane opened from the Architect button. `DesignationCategoryDef` (`Verse/DesignationCategoryDef.cs`) is a full Def: `order`, `showPowerGrid`, `researchPrerequisites`, `specialDesignatorClasses` (instantiated as `Designator`s — pure C#, `ResolveDesignators()`), `preferredColumn`, plus `Visible` computed from research/monolith level (`DesignationCategoryDef.Visible`). Vanilla XML lives under Core defs. Designators auto-generate from `ThingDef.designationCategory`.
- **Extension, no patching:** declare `<DesignationCategoryDef>` + `<specialDesignatorClasses><li>Mwah.Designator_Kiss</li></specialDesignatorClasses>`, or just a new column in an existing category.
- **Dynamic visibility:** partial — `researchPrerequisites` is static data; per-frame logic needs the Designator's own label/`CanDesignate` logic. Also filtered by `Current.Game.Rules.DesignatorAllowed` (GameRules — usable without patching).
- **Fit for this mod:** a "Kiss" designation category is the *idiomatic* RimWorld pattern for "click two things on the map": it lives next to zone/stockpile designators where players already expect map-painting actions. Cost: architect menu is crowded; entering it replaces the current selection workflow.

### 1.5 Right-click context menu — `FloatMenuOptionProvider`
- **What/where:** the float menu from right-clicking a thing/pawn. `abstract class FloatMenuOptionProvider` (`RimWorld/FloatMenuOptionProvider.cs`) with `SelectedPawnValid`, `TargetThingValid`, `TargetPawnValid`, `Applies`, `GetOptions/GetOptionsFor`.
- **Registration, no patching and no XML:** `FloatMenuMakerMap` reflects `typeof(FloatMenuOptionProvider).AllSubclassesNonAbstract()` once (`RimWorld/FloatMenuMakerMap.cs:20-25`) — every loaded mod assembly's subclasses are picked up automatically. The mod already ships `FloatMenuOptionProvider_Kiss`.
- **Known constraint (documented in the mod's own def comment, and correct):** the float menu is gated by `ShouldGenerateFloatMenuForPawn` + context pawn validity (`FloatMenuMakerMap.cs:46-63`) — you cannot offer options for pawns the player can't select/order. Dynamic per-state: yes, inside `Applies`/`GetOptions`.
- **Risk:** zero crowding cost (menu is per-cell); good *companion* action, cannot serve as the sole entry.

### 1.6 Alerts readout (right column, above letters) — `Alert`
- **What/where:** stacked pills on the right edge. `abstract class Alert` (`RimWorld/Alert.cs`) — `abstract AlertReport GetReport()` (dynamic visibility!), `virtual AlertPriority Priority`, `GetExplanation()`, `GetJumpToTargetsText`, `virtual Rect DrawAt(float, bool)`, click-to-jump via `CameraJumper.TryJumpAndSelect(GlobalTargetInfo)` in `Alert.OnClick`.
- **Registration, no patching:** `AlertsReadout` ctor builds `allAlertTypesCached` from `typeof(Alert).AllLeafSubclasses()`, excluding `Alert_Custom*` (`RimWorld/AlertsReadout.cs:52-70`) — mod `Alert` subclasses are auto-instantiated, **no Harmony and no About.xml needed**.
- **Entry-point potential:** high. An "active kiss order" alert (Priority Medium, label "kiss director: ready" when armed, hidden otherwise via `GetReport().active`) doubles as status *and* a click target (jump/select via `AlertReport.AllCulprits` — targets are selectable things, pawns qualify). Risk: the right column is crowded with vanilla + quality-of-life mod alerts; player may dismiss semantics ("alerts = problems").
- **`Find.Alerts`** accessor: `Verse/Find.cs:60`.

### 1.7 Free OnGUI layers — `GameComponent.GameComponentOnGUI` / `MapComponent.MapComponentOnGUI`
- **What/where:** arbitrary overlay drawing per frame. `public virtual void GameComponent.GameComponentOnGUI()` (`Verse/GameComponent.cs:15`) called from `GameComponentUtility.GameComponentOnGUI()` (`Verse/GameComponentUtility.cs:40-47`) which is invoked by `UIRoot` (`Verse/UIRoot.cs:66`) — every frame, all screens. `public virtual void MapComponent.MapComponentOnGUI()` (`Verse/MapComponent.cs:20`) via `MapComponentUtility.MapComponentOnGUI(map)` (`:40-47`) called from `RimWorld/MapInterface.cs:39` — in-game map only.
- **Registration:** `Game.FillComponents()` instantiates `typeof(GameComponent).AllSubclassesNonAbstract()` (`Verse/Game.cs:472-481`); `Map` ctor same for `typeof(MapComponent)` (`Verse/Map.cs:713`). Assembly-scan: no patching, and in 1.6 not even the About.xml `<gameComponents>` (that path appears retired).
- **Dynamic visibility:** fully (it's your own code).
- **Risk:** per-frame IMGUI cost if sloppy; overlays drawn here can collide with any other overlay mod. The mod already uses this for target picking. Use for transient UI (cursor mode banner), not persistent chrome.

### 1.8 Windows / dialogs — `Window` + `Find.WindowStack`
- `public void WindowStack.Add(Window)` (`Verse/WindowStack.cs:358`); `Find.WindowStack.ImmediateWindow(...)` is the public per-frame window API used pervasively (e.g., `Verse/InspectTabBase.cs:55`, `Verse/EnvironmentStatsDrawer.cs:112`, `Verse/Designator.cs:373`). Vanilla layering enum `Verse/WindowLayer.cs` (`GameUI`, `Super`, ...).
- A mod can open **any** custom dialog at any time (hotkey, from a button, from an alert click). This is the fallback for anything the bar can't host. Cost: modal focus-steal; players dislike surprise windows.
- Mod settings: `Mod.SettingsCategory()` / `Mod.DoSettingsWindowContents(Rect)` (`Verse/Mod.cs`) — the "Mod settings" surface in Options; not an in-game entry point but where the enable-toggles live.

### 1.9 Letters — `LetterStack` + `Letter`/`LetterDef`
- `public void LetterStack.ReceiveLetter(...)` overloads + `MakeLetter` (`Verse/LetterStack.cs:37-70`); archived via `Find.Archive` (`:66`). `LetterDef` is a Def (Core defs; used e.g. `MessageTypeDefOf` sibling pattern). Dynamic: yes, at will. Good for one-off milestones ("first pair kiss"), bad as a repeated UI element (auto-pause rules: `let.def.pauseMode`, `:57-63`).

### 1.10 Toast messages — `MessageDef` + `Messages.Message`
- Call-site evidence of public API: `RimWorld/FloatMenuMakerMap.cs:53` (`Messages.Message(reason, pawn, MessageTypeDefOf.RejectInput, historical: false)`); `Verse/Message.cs` + `MessageDef` defs (`RimWorld/MessageDefOf.cs` family). Left-side transient toasts, cheap, non-blocking, fade out — best for "kiss dispatched/rejected" feedback.

### 1.11 Learning helper concepts — `ConceptDef` (+ `LessonAutoActivator`)
- `ConceptDef` is a Def with `priority`, `needsOpportunity`, `highlightTags`, `HelpTextAdjusted` (`RimWorld/ConceptDef.cs:7-94`); XML in `Defs/Core/Tutor/Concepts_*.xml` (notedSelfshow / notedOpportunistic / triggeredModal). Mods register new concepts by *adding def XML* and fire them with `public static void LessonAutoActivator.TeachOpportunity(ConceptDef, Thing, OpportunityType)` (`RimWorld/LessonAutoActivator.cs:43`). Read-out UI: `RimWorld/LearningReadout.cs` (the "!" window). Dynamic: yes. Best use here: one-time "how the kiss director works" opportunistic tip; **not** a recurring HUD element.

### 1.12 Room info panel rows — `RoomStatDef`
- The mouse-hover room stats list iterates `DefDatabase<RoomStatDef>.AllDefsListForReading` (`Verse/EnvironmentStatsDrawer.DoRoomInfo`, `Source/Verse/EnvironmentStatsDrawer.cs:194-240`) and each row's value comes from `room.GetStat(roomStatDef)` with `RoomStatDef.workerClass` (`Verse/RoomStatDef.cs:9` + `Worker` getter). So a mod *can* add a row (e.g. "room kissiness") via XML + a `RoomStatWorker` subclass, no patching. Shown only when the room-stats window is open (`showBeauty`/`showTemperatureOverlay` toggles gate it, `EnvironmentStatsDrawer.cs:181-191, ShouldShowRoomStats()`). Poor entry point (hidden behind a vanilla toggle), novelty status surface. **`RoomOverlayDef` does not exist in 1.6** — room color overlays (beauty etc.) are hardcoded (`OverlayDrawHandler` is only frame flags, `RimWorld/OverlayDrawHandler.cs:6-38`).

### 1.13 Pawn overlays (map render) — `PawnRenderTreeDef` / `PawnRenderNodeProperties` via def `renderNodeProperties`
- Tree per race: `RaceProperties.renderTree` (`Verse/RaceProperties.cs:68`), defs in `Defs/Core/PawnRenderTreeDefs/`; nodes built from `PawnRenderNodeProperties.nodeClass` (`Verse/PawnRenderTree.cs:333-343`).
- **The moddable, no-patch path:** any def implementing `Verse.IRenderNodePropertiesParent` (`Source/Verse/IRenderNodePropertiesParent.cs`) can add overlay nodes to *existing* pawns: `HediffDef.renderNodeProperties` (`Verse/HediffDef.cs:159`, `HasDefinedGraphicProperties:199`, `forceRenderTreeRecache:169`), `GeneDef.renderNodeProperties` (`Verse/GeneDef.cs:29`), apparel/mutant/trait equivalents. Vanilla precedent: `<li Class="PawnRenderNodeProperties_Overlay">` in `Defs/Core/PawnRenderTreeDefs/PawnRenderTreeDefs.xml:29` and `Defs/Core/HediffDefs/Hediffs_Local_Misc.xml:166`.
- Practical use: a tiny "kiss mark" hediff with an overlay node (heart above head / lip mark) — pure XML + one hediff, dynamic (add/remove at runtime via public hediff API), zero per-frame IMGUI cost (render tree is cached). Risk: only shows at the game's render layer — could be missed; visually competes with tattoos/attachments.
- `FleckMaker` is even cheaper for transient feedback: `public static FleckMaker.Static/AttachedOverlay/ThrowMetaIcon` (`RimWorld/FleckMaker.cs`), e.g. hearts on kiss completion. `FleckDef` is a Def (`Verse/FleckDef.cs:7`). Speech-bubble-style motes: `InteractionDef.interactionMote` is a `ThingDef` (`RimWorld/InteractionDef.cs:11`) — a "kiss" interaction could carry its own mote with zero custom render code.

### 1.14 Key bindings — `KeyBindingDef` + `KeyBindingCategoryDef`
- `KeyBindingDef` is a Def (`Verse/KeyBindingDef.cs:7-161`): `category`, `defaultKeyCodeA/B`, rebindable and conflict-checked through vanilla Options → Key bindings (`RimWorld/Dialog_KeyBindings.cs`). Bindings are polled with `JustPressed`/`IsDownEvent` inside any OnGUI/Update hook (e.g., used by `PlaySettings.DoMapControls` itself, `PlaySettings.cs:148`). A hotkey to toggle the kiss-director is free, discoverable (shows in the bindings list), and non-crowding. Also: `MainButtonDef.defaultHotKey` auto-generates its binding (`Verse/KeyBindingDefGenerator.cs:33`) — the mod's bar button can get a rebinding entry for nothing.

### 1.15 Social/mood surfaces (feedback, not entry points)
- **`InteractionDef`** (`RimWorld/InteractionDef.cs`): fully data-driven social interaction — `workerClass`, `initiatorThought`/`recipientThought`, `logRulesInitiator/Recipient` (RulePacks), `interactionMote`, `symbol`. Its log entries flow through `public PlayLogEntry_Interaction(InteractionDef, Pawn, Pawn, List<RulePackDef>)` + `Find.PlayLog.Add` (`Verse/PlayLogEntry_Interaction.cs:51`, `Verse/Find.cs:144`) and are **rendered by vanilla** in the pawn **Log tab** (`RimWorld/ITab_Pawn_Log.cs:79`, labelKey "TabLog") and on the **social card** (`RimWorld/SocialCardUtility.cs:177`, `InteractionCardUtility.DrawInteractionsLog`). If the mod expresses "kiss" as an interaction, the play-by-play text appears in these tabs automatically. (The mod already has `JobDriver_Kiss` + thought rewards — an `InteractionDef` layer would buy the log surfaces for free, but interactions have AI-side semantics (think tree) that may not suit player-commanded acts; mark as *design option, UNVERIFIED fit*.)
- **`ThoughtDef`** (`RimWorld/ThoughtDef.cs`): mood-list row + needs-tab bubble (`showBubble`), `icon` texture, `stages`, situational via `workerClass`. The mod already grants `MWAH` thought(s) — that *is* the mood-tab feedback surface, zero extra work.
- **`HistoryEventDef`** (`RimWorld/HistoryEventDef.cs:5-8`) exists but in 1.6 is a bare Def (`maxRemembered` only) — history *events* feed the History tab aggregates and thought requirements; it is **not** an independently-visible HUD row. Low value here.
- **Stat rows:** `StatDrawEntry` (`RimWorld/StatDrawEntry.cs:232`) is the object the character/health/social tabs render; a mod can't inject into vanilla tabs' entry lists without its own ITab (1.3) — custom *stats* (`StatDef`) on pawns do surface automatically in stat reads where `statCategories` apply. Mod-side `Bill`s show in `ITab_Bills` (`RimWorld/ITab_Bills.cs:34`) and work-tab columns via `WorkGiverDef` XML (`Defs/Core/WorkGiverDefs/`) / `PawnColumnDef : Def` (`RimWorld/PawnColumnDef.cs:7`) — present but irrelevant to a kiss mod. `PlayLogUtility` (named in the task) **does not exist**; the log API is `Find.PlayLog.Add` (verified above).

### 1.16 Resource readout (top-right) — `Verse/Listing_ResourceReadout.cs:39,90`
- Iterates vanilla "important resource" nodes; the stockpile-category rule is ThingDef-data-driven (`thingCategories` + market value). A mod's valuable item appears with no code. [INFERENCE on exact inclusion rule; class existence verified.] Not useful here.

### 1.17 Colonist bar (top-left pawn portraits) — **verified not extensible without Harmony**
- `Verse.ColonistBar` (`RimWorld/ColonistBar.cs:10`) toggled by `PlaySettings.showColonistBar` (metadata: `FIELD showColonistBar` on `RimWorld.PlaySettings`). Entries are built internally (`FIELD cachedEntries`) and the per-cell icon set is a hardcoded field list on the drawer (`RimWorld.ColonistBarColonistDrawer`: `FIELD Icon_MentalStateNonAggro`, `Icon_MedicalRest`, `Icon_Sleeping`, `Icon_Fleeing`, `Icon_Attacking`, `Icon_Idle`, `Icon_Burning`, `Icon_Inspired`, … + `METH DrawIcons`/`DrawIcon`). No def database, no registration list, no public hook found (searches `ColonistBarEntry`, `AddEntry` → 0 hits; confirmed against the 1.6.4871 reference metadata). **Custom colonist-bar badges therefore need Harmony → out of scope.**

### 1.18 Gameplay tips (small auto-rotating tip window) — `TipSetDef` (pure XML)
- Metadata-verified: `Verse.TipSetDef : Def` with `FIELD tips`, vanilla instance `RimWorld.TipSetDefOf.GameplayTips`, drawn by `Verse.GameplayTipWindow` (`FIELD allTipsCached`, `FIELD tipUpdateInterval`, `FIELD currentTipIndex`, `METH DrawWindow/DrawContents/ResetTipTimer`) — `UIRoot_Play` hosts it. A mod can add its own `<TipSetDef>` (or `<Operation Patch="true">` onto `GameplayTips`) to teach "点底栏「亲吻导演台」，再连点两个人" with zero C# and zero Harmony. Cost: near-zero (it is drawn only when vanilla decides to show a tip); it is a *discovery* surface, not a status surface.
- Distinct from the 1.11 `ConceptDef` learning readout (the "!"/concepts window) — both exist in 1.6.

### 1.19 Pawn table columns — `PawnColumnDef` (pure XML)
- Metadata-verified `RimWorld.PawnColumnDef : Def` (`workerClass`, `sortable`, `headerTip`, `headerIcon`, `width`, `workType`, `paintable`, `groupable`, `METH get_Worker`) + `RimWorld.PawnColumnWorker` (`DoCell`, `DrawCell`, `GetHeaderTip`, `Compare`, `VisibleCurrently`). These drive vanilla's pawn tables (Work tab, Animals tab, Recruiting…). A mod column ("kissed recently", "kisses today") is data-driven and needs no patching — but a kiss mod has no business occupying the work tab; listed for completeness.

---

## 2. Harmony-only surfaces (rejected per project red line) — with evidence

| Surface | Why patching is required |
|---|---|
| **Bottom-right map-controls row** (the row of eye/zone/beauty/... toggles next to the "+" overflow) | Rendered by `private void PlaySettings.DoMapControls(WidgetRow)` (`RimWorld/PlaySettings.cs:153`) with a hardcoded `row.ToggleableIcon(ref <privateField>, ...)` call list; the only public door is `PlaySettings.DoPlaySettingsGlobalControls(WidgetRow, bool)` (`PlaySettings.cs:108`) which *calls* the private method — no list, no def, no virtual. Adding an icon = IL-patching `DoMapControls` or the caller. (Matches the note already in `MWAH_MainButtonDefs.xml`.) |
| **Room color overlays / temperature overlay etc.** | No def DB behind them: `OverlayDrawHandler` is just frame counters (`RimWorld/OverlayDrawHandler.cs:6-38`); each overlay's draw call is hardcoded (`EnvironmentStatsDrawer.DrawRoomOverlays`, `Verse/EnvironmentStatsDrawer.cs:181`, gated by fixed `PlaySettings.showX` booleans at `PlaySettings.cs:153+`). |
| **Colonist bar cells** | See 1.17 — no found public registration; internals (`AlertsReadout.allAlertsBase`-style caching) not exposed for the bar. |
| **Vanilla tab ordering inside inspect pane / architect categories** (insert *between* specific vanilla siblings of things you don't own) | Tab order = the order of `inspectorTabs` list on the owning ThingDef / `DesignationCategoryDef.order`; to reorder **vanilla** defs' own lists you must patch their XML — allowed via `PatchOperation` (still no Harmony!). This one is therefore *not* Harmony-only; listed to correct the common claim. |
| **`GameUI*` data-driven root system** | Doesn't exist in 1.6 (table at top). Claims that a mod can register a "GameUI root" are false; only `MainButtonDef` + `MainTabWindow` + `WindowStack` compose the game UI. |
| **`Find.MainTabsRoot` / bar layout internals beyond the def list** | `MainButtonsRoot.allButtonsInOrder` is private and rebuilt from DefDatabase only (`RimWorld/MainButtonsRoot.cs:13,33`); you cannot reorder/reparent the bar itself. But since `order` + `minimized` + `Visible` cover placement and visibility, no need arises. |

---

## 3. Recommendation for Mwah! (kiss director)

### Entry-point candidates (vs. today's menu bar)

| Surface | Better/worse than bar? | Why |
|---|---|---|
| **Menu bar `MainButtonDef` (current)** | baseline; keep | Always visible, `order=200` sits in free space, `Visible` hook = setting-driven (already implemented), hotkey auto-generated (`defaultHotKey`), `minimized=true` available if bar gets crowded. Best entry. |
| **`DesignationCategoryDef` "Kiss" architect category + custom `Designator`** | *complementary, arguably more discoverable* | This is RimWorld's native "pick pawns on the map" idiom (matches how zones/stockpiles work); the two-step target UX feels less alien. Costs architect-menu crowding and deeper clicks (bar → architect → category). |
| **`Alert` subclass (auto-registered)** | better for *entry-when-relevant*, not sole entry | Clickable, dynamic (hide via `GetReport()` when setting off or no map), zero def needed; but alert semantics = "problem", and right column is crowded with vanilla + QoL mods. |
| **Hotkey-only (`KeyBindingDef` + `JustPressed` in existing `GameComponentOnGUI`)** | worse as sole surface, better as accelerator | Invisible until learned; pair with the bar button (via `MainButtonDef.defaultHotKey`, free). |

### Status / feedback elements (not entry points)

| Surface | Verdict for "who is kissing / kissed recently" |
|---|---|
| Flecks (`FleckMaker.Static/AttachedOverlay`, heart `FleckDef`) | **Yes — momentary feedback at the kiss site.** Cheapest, most readable, no chrome cost. |
| Hediff + `renderNodeProperties` overlay (`HediffDef.cs:159`) or `InteractionDef.interactionMote` bubble | **Yes — per-pawn "recently kissed" mark** with ~pure XML; needs a `forceRenderTreeRecache`-style refresh on add/remove (verified pattern, `Hediff.cs:741`). |
| `ThoughtDef` mood row (already present via `MWAH_ThoughtDefs.xml`) + `showBubble` | **Yes — already the mod's feedback channel; add `showBubble` icon for free colonist-bar bubble.** |
| Pawn Log tab via `PlayLogEntry_Interaction` | **Yes if kisses become `InteractionDef`s** — vanilla renders the sentences; otherwise a custom ITab (1.3 route). |
| Per-pawn gizmo (`ThingComp.CompGetGizmosExtra`) | Only if you want a per-pawn "kiss this one now" action; strip crowding. |
| Letter / toast (`Messages.Message`) | Toast yes (dispatch confirm); letter only for rare milestones. |
| `RoomStatDef` row ("room kissiness") | Fun flavor, low reachability (gated by room-stats window). Skip. |

## 4. Key file references (this repo)
- `1.6/Defs/Kiss/MWAH_MainButtonDefs.xml` — current entry (order=200, workerClass).
- `Source/Mwah/Actions/Kiss/MainButtonWorker_KissDirector.cs` — `Visible` override pattern, correct per `MainButtonWorker.Visible` virtual.
- `Source/Mwah/Actions/Kiss/FloatMenuOptionProvider_Kiss.cs` — auto-registered per `FloatMenuMakerMap.cs:20` AllSubclassesNonAbstract.
