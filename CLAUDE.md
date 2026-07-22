# Mossmark — Project Guide

## Project Goal

A top-down 2D RPG focused on **world and town development** rather than player character progression. The player moves through the world and holds a single **Attention** action on whatever is nearby — what happens is a property of the thing being attended to, not a menu choice. Sustained attention develops NPCs, buildings, and the town over time, revealing new specializations and access rather than presenting an upgrade menu. Tech is per-town and must be manually transferred between towns in later phases.

See [IDEAS.md](IDEAS.md) for the high-level vision and [FEATURES.md](FEATURES.md) for the current feature design and iteration roadmap.

> **Historical Note:** This is "Prototype 2" — a fresh Unity project started from scratch (see PROTOTYPE2.md's "Why a second prototype"). It is informed by Prototype 1 but does not carry over P1's architecture; P1 systems referenced in PROTOTYPE2.md (e.g. `PlayerMovement`, `DayCycleManager`, `Entity`/`UpgradePool`) are reference material from the old project, not code that exists here yet. (Iterations beyond P2's "30" are currently sourced in FEATURES.md)
>
> **[PROTOTYPE3_KNOWLEDGE_SPINE.md](PROTOTYPE3_KNOWLEDGE_SPINE.md)** is a new scene now being built alongside `Greybox.unity`, testing one atomic claim (does teaching an entity a property change its behavior in a way that feels meaningful with no item delivery involved?) before committing further iterations to the larger reframe it's drawn from. It reuses select `Greybox.unity` systems directly without modifying them — see that doc's "Reuse Discipline" section for exactly what's shared and what's deliberately left out.
>
> **[PROTOTYPE4_ACQUAINTANCE.md](PROTOTYPE4_ACQUAINTANCE.md)** is the next pilot after P3 answered its claim: a scene (`Prototype4.unity`) testing whether *getting to know an already-alive place* — attention spent only to find out what's there, with zero effect on the thing attended — is a wantable activity in its own right. Same Reuse Discipline as P3; acquaintance is a `DevelopmentTrack` on the shared resolver. See that doc's Build Notes for implementation decisions and verification status.

---

## Design Values

These are the load-bearing principles of Mossmark. They take precedence over implementation convenience, feature completeness, and genre convention. When a new system or mechanic is in tension with one of these, the value wins unless there's an explicit documented decision to defer it.

**Tried, not chosen.** Outcomes emerge from sustained attention rather than menu selection. The player never picks what happens — only where they stand and how long they stay. Any interaction that presents a player-facing choice of outcome type is pulling against this. The dependency resolver produces a "needs" message when something is blocked; it does not ask the player to select from a list of options.

**Organic over deterministic.** Outcomes should feel like they emerged from the world, not like a counter hit a threshold. Where a system has a meaningful threshold, approach it probabilistically — a rising chance per tick, not a precise trigger. Use min/max ranges wherever the variance is felt. State that changes gradually should change continuously, not in discrete steps. Feedback describes conditions, not values: "the ground here is disturbed," not "tendedness: 0.24." Gates (binary `CanAttend()` checks) can be exact — they're legible and intentional. Outcomes should not be. *A known audit pass is pending to apply this to existing implementations.*

**The world was here before you and will outlast you.** The settlement is not rescued by the player's arrival. It existed before and will continue after. Development reveals what was latent — latent specializations in dilapidated buildings, latent knowledge in people — rather than installing new things from outside. NPCs are not quest-givers waiting for the player to activate them; they're people with their own trajectories that the player's attention can influence. Nothing about the world should feel like it was placed there as content for the player to consume.

**Differentiation, not advancement.** A settlement deep in ritual knowledge and a settlement deep in ironworking are not at different points on the same ladder — they're genuinely different places. Development gives a settlement character, not score. Resist any framing where one development path is objectively better than another, or where there's a canonical "complete" state. The design tension between enjoying development systems and discomfort with "progress ideology" is resolved by making outcomes discovered rather than selected — see "Tried, not chosen" above.

**No preaching.** The post-imperial Dark Ages setting is a mood and a context, not a thesis. The game does not argue that pre-modern societies were noble, that development is inherently good, or that the player is rescuing anyone. The people in this world are complicated; the history is complicated; the player's role is to tend, not to save. Avoid mechanics that frame the settlement as deficient without the player's help, or that reward the player for "civilizing" anything. 

**Felt, not read.** The world communicates through texture rather than UI. Knowledge-gated yield changes are noticed by returning to a familiar spot, not announced. Spot tendedness is described in overlay language, not displayed as a stat. Maintenance drift is a building that seems quieter, not a timer bar. When in doubt, delay the feedback and make it indirect — the player should feel like they discovered something, not that they received a notification.

**The Flame Sword feeling.** Some moments must feel like the world giving you something earned. A rare item found after patient attention to a dusk wilderness run, an NPC gift after a long-developed relationship, a POI that opens because someone has become who the place needed — these moments lose their weight if everything flattens into the same small grindy texture. Preserve room for larger payoff moments even as the moment-to-moment loop becomes granular.

---

## Environment

- **Unity version**: 6000.4.11f1 (Unity 6)
- **Render pipeline**: URP, 2D Renderer (`Assets/Settings/Renderer2D.asset`). The active scene has a Global Light 2D — 2D lights are available and may be used later for day/night ambience (the daylight/day cycle in PROTOTYPE2.md).
- **Physics**: 2D (`Rigidbody2D`, gravity = 0 for top-down movement)
- **Input**: new Input System (`com.unity.inputsystem`), via the project-wide actions asset `Assets/InputSystem_Actions.inputactions`. See "Input Actions" below.
- **UI system**: Unity UI Toolkit (`UIDocument`, `VisualTreeAsset`, `VisualElement`) — **not** uGUI/Canvas
- **Editor tooling**: `com.gamelovers.mcp-unity` is installed for MCP-based editor automation. Files created outside the Editor (scripts, scenes) are not seen until `Assets/Refresh` runs — `recompile_scripts` alone can silently compile *without* the new files, so refresh first, then recompile, then read the generated `.meta` for GUIDs. Expect the MCP WebSocket (port 8090) to drop for ~15–30s around every domain reload (play enter/exit, recompile) — retry, don't diagnose. If the Windows session locks, the editor loop stalls entirely and MCP stays down until the machine is unlocked. **Entering/exiting Play Mode**: MCP-Unity's `execute_menu_item` can't reach Unity's built-in `Edit/Play` toggle (it silently fails) — use the project's own `Mossmark/Debug/Enter Play Mode` / `Mossmark/Debug/Exit Play Mode` menu items instead (`Assets/Editor/PlayModeMenu.cs`, thin wrappers around `EditorApplication.EnterPlaymode()`/`ExitPlaymode()`), confirmed working. Iterations 45/46/49 hit the `Edit/Play` dead end and skipped live verification as a result — use the custom menu items for any future Play Mode check instead of re-trying `Edit/Play`.

---

## Input Actions

`Assets/InputSystem_Actions.inputactions` is configured as the project-wide actions asset (Project Settings → Input System Package). Access it via `InputSystem.actions`, enabling the relevant map in `OnEnable`:

```csharp
var gameplay = InputSystem.actions.FindActionMap("Gameplay");
gameplay.Enable();
var move = gameplay.FindAction("Move");
```

- **`Gameplay` map**
  - `Move` — `Vector2`, WASD / arrow keys / gamepad left stick. Drives `PlayerController`.
  - `Attend` — `Button` with a `Hold` interaction (keyboard `E`, gamepad North button). Drives `AttentionInput` (see Key Design Decisions for how its `started`/`canceled` callbacks are used).
  - `Horizon` — plain `Button` (keyboard `Tab`). Toggles the Settlement Horizon panel (`HorizonUI`).
- **`UI` map** — retained from the default template for future menu surfaces (e.g. the settlement chest, the one menu-based interaction PROTOTYPE2.md keeps).

---

## Code Conventions

### Language and approach
- **C# only** — no visual scripting (no Bolt, no Unity Visual Scripting graphs)
- **Code-first**: all game logic lives in MonoBehaviours, plain C# classes, and ScriptableObjects
- **1st-party APIs first**: prefer Unity's built-in systems over 3rd-party plugins for gameplay code
- **No 3rd-party gameplay frameworks**

### Namespaces
All scripts live under the `Mossmark` root namespace, organized by system. This table reflects PROTOTYPE2.md's "Core Systems" section, generalized from Prototype 1:

| Namespace | Purpose |
|---|---|
| `Mossmark.Player` | Player controller, camera follow, input relay |
| `Mossmark.Visuals` | Shared grey-box visual primitives (e.g. `TriangleSpriteGenerator`) used across systems |
| `Mossmark.Attention` | Attention framework (zones, detector, manager, overlay UI) — generalizes P1's Interaction framework (`IInteractable` → `IAttendable`) |
| `Mossmark.Day` | Daylight/day-clock — phase tracking, ambient text, and the daylight HUD. Direct successor to P1's `DayCycleManager` |
| `Mossmark.Inventory` | Inventory manager, item database, item pickups, settlement chest |
| `Mossmark.Development` | Generic dependency/response resolver and developable entities (buildings, NPCs, POIs, town) — generalizes P1's `Entity`/`UpgradePool`/`TownEntity` |
| `Mossmark.World` | Region and town generation, place archetypes, and wilderness-spot attendables (e.g. `DevelopingWildernessSpotAttendable`, `PoiAttendable`) |
| `Mossmark.Prototype3` | Knowledge-spine pilot scripts scoped to `Prototype3.unity` (`KnowingEntityAttendable`, `PropertyPickupAttendable`, `KnownPropertyCondition`, `TakenLedger`, `WorkingSurfaceAttendable`) — new components only, per that doc's Reuse Discipline; nothing in Greybox references this namespace. `PropertyPickupAttendable` is additionally reused (unmodified) by `Prototype4.unity` |
| `Mossmark.Prototype4` | Acquaintance pilot scripts scoped to `Prototype4.unity` (`AcquaintableAttendable`, `AttentionCountCondition`, `TaughtPropertyCondition`) — new components only, same Reuse Discipline as P3; nothing in Greybox or Prototype3 references this namespace |

P1's `Mossmark.Quests` and `Mossmark.Combat` are **not** part of P2 — collection quests and discrete combat encounters were dropped per IDEAS.md's "Prototype 2" section. Add namespaces here as new systems land; keep this table in sync with what's actually implemented.

### Tooling language default
- **Default to C# for all project tooling** (Editor scripts, build helpers, data importers). Unity's `AssetDatabase`, `SerializedObject`, and `EditorUtility` APIs give direct, type-safe access to all game data without external dependencies. Only reach for another language when there is a concrete reason C# cannot do the job (e.g., a CI script that must run outside the Unity process and has no natural C# equivalent).
- Editor scripts live in `Assets/Editor/` and use `[MenuItem]` for discoverability inside the Editor. They are excluded from player builds automatically.

### Architecture rules
- **Event-driven**: systems communicate via `UnityEvent<T>` or C# `event Action<T>`, not direct method calls across system boundaries
- **Singleton managers**: one per system (e.g., `AttentionManager.Instance`), implemented as `MonoBehaviour` singletons with `Awake` guard
- **UI holds no game logic**: UI components only display state pushed from managers; they never write to game state directly
- **Interfaces for contracts**: use interfaces (e.g. `IAttendable`) for cross-system contracts so systems stay decoupled
- **ScriptableObjects for data**: item definitions, development stages, region/place-archetype data, loot tables — all as `ScriptableObject` assets stored under `Assets/Game/Data/`. Do **not** use `Assets/Game/Resources/` for new assets.
- **Procedural spawning — inactive-GO pattern**: when creating GameObjects at runtime, call `SetActive(false)` immediately after `new GameObject()`, add all components and call their `Initialize()` methods, then `SetActive(true)`. This ensures `Awake` fires with correct data already set on serialized fields. Components that participate in procedural spawning must expose a public `Initialize()` method.
- **Each iteration leaves prior systems functional**: never break existing systems when adding new ones

### Comments
Default to no comments. Only add a comment when the *why* is non-obvious (a hidden constraint, a Unity quirk, a workaround). Never describe what the code does — good naming handles that.

---

## Active Scenes

- **`Assets/Game/Scenes/Greybox.unity`** — the Prototype 2 scene; the site/exhaustion/Standing thread lives here and still plays
- **`Assets/Game/Scenes/Prototype3.unity`** — the Knowledge Spine pilot scene (see PROTOTYPE3_KNOWLEDGE_SPINE.md); hand-placed, no `WorldGenerator`. Day cycle wired in as of Iteration 3.5 (`maxDaylight: 5` as of a 7-16-26 retune — see PROTOTYPE3_KNOWLEDGE_SPINE.md's Build Notes; bedroll + fade + HUD reused from Greybox). The Discovery Thread (Iterations 3.6–3.9) added a Taken Ledger, a non-modal working surface, a second teachable want on the Dyer, and a non-modal HUD listing what's been taken. Reuses shared scripts unmodified; its own scripts live in `Assets/Game/Scripts/Prototype3/`. Editor test drivers: `Assets/Editor/Prototype3Debug.cs` (`Mossmark/Prototype3/*` menu items — teleports, begin/release attend via reflection, log entity/taken-ledger knowledge, log daylight)
- **`Assets/Game/Scenes/Prototype4.unity`** — the Acquaintance pilot scene (see PROTOTYPE4_ACQUAINTANCE.md); hand-placed, no `WorldGenerator`. Scaffold copied from `Prototype3.unity` (camera/player/attention/overlay/day cycle @ `maxDaylight: 5`/bedroll/notification UI), P3 content stripped. Two hand-authored sites: a river landing (Netmender / Smokehouse / Osier Bed + the flag-gated Smoking Racks surface, with The Landing — a tending spot, Iteration 4.12 — yielding Withy / Alder Billet / Smoked Eel on a per-attend chance rather than one-shot pickup) and a colliers' hearth (Collier / Bothy / Hearth Ring + Char Knot pickup); all pickups take unrevealed. The Collier and Osier Bed are days-gated (one qualifying attend per day); acquaintance crossings ripen probabilistically past a per-stage floor. As of Iteration 4.10, the Netmender can also be *taught*: once Known, attending her with `keeps_well` worked out (at the Smoking Racks, from A Smoked Eel) is a short teach tick instead of a flavor visit, unlocking a further stage (`smokes_the_catch`) that ripens and crosses through the ordinary resolver. Its own scripts live in `Assets/Game/Scripts/Prototype4/`; editor test drivers in `Assets/Editor/Prototype4Debug.cs` (`Mossmark/Prototype4/*` — teleports, begin/release attend via reflection, advance acquaintance, force teach, log entity state/daylight)
- **`Assets/Settings/Scenes/URP2DSceneTemplate.unity`** — Unity's URP 2D scene template (used when creating new scenes via the Editor); not part of gameplay

---

## System Overview (Prototype 2)

| System | Status | Key Types |
|---|---|---|
| Input + Movement | Complete | `PlayerController`, `CameraFollow` |
| Grey-box Visuals | Complete | `TriangleSpriteGenerator` |
| Attention Framework Core | Complete | `IAttendable`, `AttendableZone`, `AttendableDetector`, `AttentionInput`, `AttentionManager` |
| Attendable Overlay UI | Complete | `AttendableOverlayUI` |
| Generic Wilderness Spots | Complete | `GenericWildernessSpotAttendable`, `ItemYield` |
| Inventory + HUD | Complete | `ItemDefinition`, `InventoryManager`, `InventoryUI` |
| Daylight / Day Clock | Complete | `DayCycleManager`, `DayCycleAmbientTextData`, `DayCycleUI` |
| Bedroll + Day Transition | Complete | `BedrollAttendable`, `DayTransitionFadeUI`, `DayCycleManager.Rest()` |
| Tended-Style Spots | Complete | `TendedSpotAttendable` |
| Generic Dependency / Response Resolver | Complete | `IDependencyCondition`, `DevelopmentStage`, `DevelopmentTrack`, `DevelopableEntity`, `WorldState` |
| Continuous Attention Framework | Complete | `IAttendable.ContinueAttending`, `AttentionManager` tick loop |
| Buildings: Revival via Attention | Complete | `BuildingAttendable` |
| NPCs: Development + Specialization Draw | Complete | `NpcAttendable` |
| Building → NPC Demand Loop | Complete | `DeclaredSpecializationNeeds`, `SpecializationNeededCondition` |
| World Gen: Place Archetypes | Complete | `PlaceArchetype`, `RegionData`, `WorldGenerator` |
| POIs | Complete | `PoiAttendable`, `RealizedSpecializations`, `SpecializationRealizedCondition`, `ItemYieldRoller` |
| Wandering Things | Complete | `WanderingThingAttendable`, `WanderingThingSpawner` |
| Settlement Chest (Rebuilt) | Complete | `ChestAttendable`, `ChestUI` |
| Settlement Horizon UI | Complete | `HorizonUI` |
| World/Town Layout (Visual Polish) | Complete | `WorldLayoutGenerator`, `SquareSpriteGenerator` |
| NPC Post-Specialization Tracks | Complete | `WoundLoreModifier`, `WorldStateChanceModifier` (+ `DevelopableEntity.MarkStageAsApplied`, `OutcomeRequest.DaylightCostMultiplier`) |
| Wilderness Depth + Random Placement | Complete | `WildernessSpotDefinition`, `TendedSpotAttendable.Initialize()`, `WorldGenerator.FindValidPosition()` |
| Building Data Generalization (G1+G2) | Complete | `BuildingStageDef`, `BuildingAttendable` stages array, `PlaceArchetype` building block |
| NPC Post-Spec + Spot Tick Tuning (G3+G6) | Complete | `NpcPostSpecStageDef`, `PlaceArchetype.NpcPostSpecStages`, `WildernessSpotDefinition` tick intervals |
| Wandering Thing Definition (G4+G5) | Complete | `WanderingThingDefinition`, `WorldStateOddsModifier`, `WanderingThingSpawner` pool, `traveler.asset` |
| Code Quality Pass (G7+G8+G9) | Complete | `WildernessYieldAttendable` base class, `LandmarkAttendable`, `TendedSpotAttendable.harvestYields[]` |
| CSV / Data Pipeline | Complete | `Assets/Editor/ExportGameData.cs`, `Assets/Editor/ImportGameData.cs` |
| Numeric Tuning Pass (Iteration 26) | Complete | `place_archetypes.csv`, `wilderness_spots.csv`, `wandering_things.csv`, `Greybox.unity` NPC tick fields |
| Always-Something-Happens (Iteration 28.5) | Complete | `NpcAttendable` visit mechanic, `BuildingAttendable` flavor linger, `PlaceArchetype` exchange/restored-flavor data |
| Settlement Maintenance (Iteration 29) | Complete | `IMaintenanceConsumer`, `MaintenanceManager`, `DevelopableEntity.DriftProgress`, `BuildingAttendable`/`NpcAttendable` cold state, `PlaceArchetype` maintenance fields |
| Settlement Growth: New Arrivals (Iteration 30) | Complete | `ArrivalCondition`, `ArrivalAttendable`, `ArrivalSpawner`, `NpcAttendable.Initialize()`, `settlement_grew` WorldState flag |
| Passive Drift Pilot (Iteration 31) | Complete | `NpcAttendable.OnDayAdvanced()`, `WorldGenerator.GetArchetypeSpot()`, `WildernessYieldAttendable.Tendedness` |
| State-Change Feedback Pass (Iteration 32) | Complete | `EntityFeedback`, `CircleSpriteGenerator`, `DevelopableEntity.OnProgressMade`, `WildernessYieldAttendable.OnProgressMade`, `TendedSpotAttendable.OnProgressMade`, `NpcAttendable.OnPassiveDriftAccrued`, `PlayerController.HandleAttentionRock` |
| Progress Cost Tuning Pass (Iteration 33.1) | Complete | `archetype_bog.asset`, `archetype_old_road.asset`, `archetype_sacred_grove.asset`, `place_archetypes.csv` post-spec stage costs |
| Cross-Pursuit Influence (Iteration 34) | Complete | `NpcPostSpecStageDef.passiveDriftSourceArchetypeId`, `KnowledgeYieldEntry.requiredSpecializationId`, `BuildingStageDef.worldStateFlag`, `BuildingAttendable.HandleDeveloped()` |
| Item Properties: Folk-Phrase Vocabulary (Iteration 35) | Complete | `PropertyDefinition`, `PropertyRegistry` (static), `PropertyKnowledge`, `ItemDefinition.PropertyIds`, `WorldContext.IsPropertyKnown()`, `InventoryUI` property display |
| Conversion Station: Crude Working Surface (Iteration 36) | Complete | `ConversionDef` SO (item-keyed + property-keyed inputs, `TryMatch()`), `WorkshopAttendable` (single restoration stage via flat_stones), `WorkshopUI` (3 slots + Work, property discovery on failure), 3 recipes, 3 new item assets, `EntityFeedback.TriggerPop()` |
| Property-Phrased Wants: Loop Closure (Iteration 37) | Complete | `PropertyAvailableCondition` (carry-only property gate, authored `wantDescription` shown in overlay), Bog Keeper `bog_keeper_drainage` stage gates on `turns_water` (Mistletoe or Clay satisfy it), matched item consumed from pack at stage completion. *(The `requiredPropertyId` field this shipped with was superseded by Iteration 38's condition list.)* |
| Relational Data Architecture Migration (Iteration 38) | Complete | `ConditionCsvImporter` (CSV → `managedReferenceValue`), `CsvUtil`, `YieldTable`, `NpcStageDef` + `NpcStagePool` (replaces `NpcPostSpecStageDef`), `BuildingStageDef` (now a SO) + `BuildingStagePool`, `PlaceArchetype.Spots` (spot list replaces inline spot block), `WildernessSpotDefinition.spotId`, `WorldGenerator.GetSpot()`, `ItemYieldRoller` weighted rare pools, `npcMaintenanceMaterial`. Schema: [DATA_SCHEMA.md](DATA_SCHEMA.md) |
| Multi-Station Property Discovery (Iteration 39) | Complete | `BuildingStageDef.stationName`/`.biasPropertyIds`, `IWorkStation`, `BuildingAttendable` station branch (replaces `WorkshopAttendable`, deleted), `WorkshopUI` bias filtering, `workshop_pool`, `bog_fen_shrine` stage, `recipe_sealing_daub`, `recipe_warding_charm` |
| Station Availability Decoupled from Final Stage (Iteration 40) | Complete | `BuildingAttendable.AppliedStationStage` (most-recently-applied-stage lookup, replaces `FinalStage`/`IsStationOpen`), `.CanAttend()` (`\|\| IsStationCapable`), `.OnAttentionComplete()` (maintenance → develop-if-possible → open station → linger) |
| Outcome Cost Resolution: Duration + Daylight (Iteration 41) | Complete | `AttentionOutcomeKind`, `BuildingAttendable.PredictOutcomeKind()`/`.BuildOutcomeRequest()`, `DriftColdDaylightModifier`, `OutcomeRequest.DurationMultiplier` |
| Site Clustering + Progressive Reveal: Bog Pilot (Iteration 42) | Complete | `WorldGenerator.SpawnArchetypeSites()`/`.FindValidPositionNear()`, `PlaceArchetype.PoiDormantByDefault`/`.PoiRevealWorldStateFlag` *(superseded by Iteration 45's `PoiStartingTier`/`PoiRevealCondition`)*, `HintFlavorEntry`, `NpcAttendable.OnDayAdvanced()` `_started` flag |
| Wilderness Spot Development Stages + Exhaustion/Standing Split: Fen Bog Pilot (Iteration 43) | Complete | `SpotStageDef`, `SpotStagePool`, `SustainedGoodAttentionCondition`, `IGoodAttentionTracker`, `DevelopingWildernessSpotAttendable`, `ITendednessSource`, `DevelopableEntity.RaiseProgressMade()` |
| Exhaustion Cost Tiering: Felt, Not Read (Iteration 43.1) | Complete | `ExhaustionCostModifier`, `DevelopingWildernessSpotAttendable.BuildOutcomeRequest()` |
| Exhaustion/Standing Rollout: All Generic Wilderness Spots (Iteration 44) | Complete | `spot_stages.csv`, `ImportGameData.ImportSpotStages()`, `ExportGameData.ExportSpotStages()`, `wilderness_spots.csv` `spotStagePool` column, `GenericWildernessSpotAttendable` retired |
| POI Three-Tier Reveal: Hidden/VisibleInert/Interactable (Iteration 45) | Complete | `PoiTier`, `PlaceArchetype.PoiStartingTier`/`.PoiRevealCondition`/`.PoiUnlockCondition`, `PoiAttendable` collider-gated tiering, `SpotStageDef.WorldStateFlag` |
| POI Three-Tier Rollout: Old Road, Sacred Grove, Old Quarry (Iteration 46) | Complete | `archetype_old_road.asset`/`archetype_sacred_grove.asset`/`archetype_quarry.asset` `poiStartingTier`/`poiRevealCondition`/`poiUnlockCondition`, `old_road_familiar.asset`/`old_quarry_familiar.asset`/`stone_outcrop_familiar.asset` `worldStateFlag` |
| Site-Scoped Standing + Ground Plane: Fen Bog Pilot (Iteration 47) | Complete | `WorldSite`, `DevelopingWildernessSpotAttendable.site`, `WorldGenerator.SpawnArchetypeSites()`/`.SpawnWorldSite()`, `DayCycleManager.DayIndex` |
| Unconditional Ambient Flavor: Fen Bog Pilot (Iteration 48) | Complete | `WildernessSpotDefinition.ambientFlavors`, `ItemYieldRoller.TryFireAmbientFlavor()`, `DevelopingWildernessSpotAttendable.ambientFlavorChance` |
| Pre-Seeded Mid-Process Start: Bog Keeper / Fen Bog Pilot (Iteration 49) | Complete | `WorldGenerator.DebugSeedMidProcessStart`, `WorldSite.SeedGoodAttentionDays()`, `NpcAttendable.spawnedAsArrival` |
| Attention-Weighted Flow Bonus: Bog vs. Sacred Grove Pilot (Iteration 50) | Complete | `WorldSite.MarkAttendedToday()`/`.AttendedDaysInWindow()`, `WorldGenerator.GetSite()`/`.IsArchetypeDominant()`, `NpcAttendable.OnDayAdvanced()` dominance bonus |
| Generalized Site Member-Spot Pools: All Archetypes (Iteration 51) | Complete | `PlaceArchetype.SiteMemberSpotPool`/`.SiteMemberMinCount`/`.SiteMemberMaxCount`, `WorldGenerator.DrawSiteMemberSpots()` |
| Dominance Halo: Building-Level Ambient Feedback (Iteration 52) | Complete | `BuildingAttendable.OnDominanceChanged`/`.dominanceArchetypeId`, `EntityFeedback.SetDominanceHalo()` |
| Flow-Filled Reserve: Bog Keeper Drainage Pilot (Iteration 53) | Complete | `IPassiveReserveTracker`, `NpcAttendable.passiveReserveByProperty`, `PropertyAvailableCondition.EffectiveThreshold` |
| Differentiated Member-Spot Seam: Clay Pit → Fen Shrine Pilot (Iteration 54) | Complete | `ITendednessSource.AttendedToday`, `BuildingStageDef.temporaryBiasSourceSpotId`/`.temporaryBiasProperties`, `BuildingAttendable.BiasPropertyIds` |
| P3 Scene Scaffold + Reuse Audit (Iteration 3.1) | Complete | `Prototype3.unity`, `Assets/Editor/Prototype3Debug.cs` |
| P3 Seeded Partial Knowledge (Iteration 3.2) | Complete | `KnowingEntityAttendable` (knowledge in `PropertyKnowledge` keyed by `entityId`), knowing-tint on load |
| P3 Teach Interaction (Iteration 3.3) | Complete | `PropertyPickupAttendable`, `KnowingEntityAttendable.TeachPending` (zero-duration one-shot teach) |
| P3 Behavior Branch on Taught Knowledge (Iteration 3.4) | Complete | `KnownPropertyCondition`, `KnowingEntityAttendable.BuildTrack()` (Clay-Lined Steeping Pit stage) |
| P3 Teaching Under Real Scarcity (Iteration 3.5) | Complete | `DayCycleManager` @ maxDaylight 4 in `Prototype3.unity` *(retuned to 5 on 7-16-26 — see Build Notes)*, Fish Weir (`LandmarkAttendable`, cost 3), `RequiresDaylight => true` on P3 attendables, Bedroll/fade/HUD |
| P3 Taken Ledger (Iteration 3.6) | Complete | `TakenLedger` (static, insertion-ordered), `PropertyPickupAttendable.propertyIds[]`/`.autoRevealOnTake`, Bark Strips + Reeds pickups |
| P3 Discovery at the Working Surface (Iteration 3.7) | Complete | `WorkingSurfaceAttendable` (non-modal, bias-filtered, one reveal per hold), Scouring Bench |
| P3 Teaching What You Worked Out (Iteration 3.8) | Complete | `KnowingEntityAttendable.TeachableWant[]` (generalized from single-property pairing), second Dyer want (`binds_fast` / "Colors That Hold") |
| P3 Seeing What You Carry (Iteration 3.9) | Complete | `TakenLedgerUI` (non-modal HUD strip, top-left, mirrors `InventoryUI`'s layout pattern reading `TakenLedger` instead of `InventoryManager.Stacks`) |
| P4 Scene Scaffold + Two-State Read (Iteration 4.1) | Complete | `Prototype4.unity`, `AcquaintableAttendable` (initial bool-swapped read), `Assets/Editor/Prototype4Debug.cs` |
| P4 Acquaintance as a DevelopmentTrack (Iteration 4.2) | Complete | `AttentionCountCondition` (always-satisfied; stage `attendsCost` is the threshold), `AcquaintableAttendable` (track-driven reads, subject-fingerprint zero-effect check) |
| P4 Three-Stage Deepening (Iteration 4.3) | Complete | `AcquaintanceStage[]` scene data (Unfamiliar → Acquainted → Known on the Netmender) |
| P4 Second Entity Type: Building (Iteration 4.4) | Complete | Smokehouse authored as pure `AcquaintableAttendable` data; `seededKnowledgeLead` (the one language field the re-skin question actually required) |
| P4 Third Entity Type + Items (Iteration 4.5) | Complete | Osier Bed data; `PropertyPickupAttendable` (P3, unmodified) Withy/Alder Billet auto-reveal pickups |
| P4 Second Site (Iteration 4.6) | Complete | Collier / Colliers' Bothy / Hearth Ring / Char Knot — hand-authored second-archetype voice, zero new code |
| P4 Organic Crossings (Iteration 4.7) | Complete | `AcquaintanceStage.minAttends`/`.ripenChance` (rising-chance crossing, `ProgressCost` 1), `InOrderCondition` (structural stage ordering — the cumulative-cost ordering accident, fixed) |
| P4 Temperament: Days-Gated Trust (Iteration 4.8) | Complete | `AcquaintableAttendable.oneQualifyingTickPerDay`/`.todaySpentLine` (Standing's shape, per-entity via `DayCycleManager.DayIndex`; Collier + Osier Bed wary) |
| P4 Earned Workshop (Iteration 4.9) | Complete | `EarnedSurfaceAttendable` (flag-gated non-modal surface over `TakenLedger`), `AcquaintanceStage.worldStateFlag`, osier→racks bonus-bias seam, pickups unrevealed + `A Smoked Eel` |
| P4 Teaching What They'll Hear (Iteration 4.10) | Complete | `TaughtPropertyCondition` (P4 analog of P3's `KnownPropertyCondition`), `AcquaintableAttendable.TeachPending`/`.Teach()` (reuses P3's `"p3_player"` knowledge convention, zero new infra), `IsFullyAcquainted` redefined to separate "ladder complete" from "no stages left" so a taught-gated stage can sit past Known; Netmender taught `keeps_well` from A Smoked Eel |
| P4 Whether Depth Changes What Lands (Iteration 4.11) | Complete | `AcquaintanceStage.earlyTeachHintLine`, `AcquaintableAttendable.TaughtStageDef`/`.EarlyTeachAttemptPending` (descriptive-only "not ready to hear that yet" texture for a wary entity attempting early); zero changes to `TaughtPropertyCondition` or the gating pipeline — confirms the teach mechanism is temperament-agnostic; Collier taught `heavy_true` from A Fused Clinker (`shares_the_watch` stage) |
| P4 Tending to Yield: One Site, Full Pool (Iteration 4.12) | Complete | `TendableSpotAttendable` (`Mossmark.Prototype4`, plain `MonoBehaviour : IAttendable`, no `DevelopableEntity`) — condenses the river landing's three `PropertyPickupAttendable` pickups (Withy, Alder Billet, Smoked Eel) into one GameObject, The Landing; flat `yieldChance` (0.3) rolled per attend against `TakenLedger`-filtered still-unclaimed pool entries, miss always plays flavor; downstream take/reveal/teach pipeline unchanged |
| P4 Ripeness: Yield Chance Scales with Time Away (Iteration 4.13) | Complete | `TendableSpotAttendable.lastVisitDayIndex`/`.attendsToday` — per-attend effective yield chance rises with days since last visit (`DayCycleManager.DayIndex`, capped at `maxDaysAwayBonus`) and falls with attends already spent there today, computed fresh each attend rather than stored as a persistent value |
| P4 Generalizing the Tending Thread: The Hearth Site (Iteration 4.14) | Complete | Zero code changes — condensed the hearth site's A Char Knot + A Fused Clinker `PropertyPickupAttendable` pickups into a second `TendableSpotAttendable`, The Ash Bed, proving the component (and 4.13's ripeness) is site-agnostic, not fit to the river landing specifically; confirmed live that the two tending spots' ripeness state is fully independent |

Full iteration plan is in [FEATURES.md](FEATURES.md). Update this table as each iteration lands.

---

## Key Design Decisions

Active implementation constraints carried forward from prior iterations, condensed to what's still load-bearing for `Greybox.unity` and for the [Prototype 3](PROTOTYPE3_KNOWLEDGE_SPINE.md) direction. Full iteration-by-iteration history — including everything condensed out of this section — is in [CLAUDE_ARCHIVE.md](CLAUDE_ARCHIVE.md).

- **`RequiresDaylight` / `ContinueAttending` are read *after* `OnAttentionComplete()`**, not before. This means `OnAttentionComplete()` sets a latch (`lastAttentionMadeProgress`, `lastAttentionWasVisit`) and those flags drive what the `AttentionManager` does next. Never read them before the call or derive them from pre-call state.
- **Inactive-GO spawning pattern**: `SetActive(false)` → add components + call `Initialize()` → `SetActive(true)`. `Awake()` fires with correct data already in place. All procedurally spawned entities follow this.
- **Asset references (`UnityEngine.Object` fields) cannot be set via MCP-Unity `update_component`**. Set them by hand-editing the scene `.unity` YAML, then calling `load_scene` to pick up the change before `save_scene`. This applies to `ItemDefinition`, `PlaceArchetype`, `WildernessSpotDefinition`, and similar SO references on spawned components. **This limitation is specific to MCP-Unity's generic tools, not the engine** — custom Editor scripts set object references (and even `[SerializeReference]` polymorphic values, via `SerializedProperty.managedReferenceValue`) freely; the CSV importer (Iteration 38) is now the primary path for wiring references on *asset* data, leaving hand-YAML only for *scene* components.
- **`DayCycleManager.DayAdvanced` event is the hook for all per-rest state changes.** Subscribe in `Start()`, unsubscribe in `OnDestroy()`. `Start()` is used (not `Awake()`) because `DayCycleManager.Instance` is guaranteed available after all `Awake()`s have run.
- **`WorldContext` / `IOutcomeModifier` / `OutcomeRequest`** is the approved pattern for ambient world-state influencing outcomes. Do not hardcode world-state checks inside `OnAttentionComplete()` — add a modifier. `OutcomeRequest` currently carries `ChanceMultiplier` and `DaylightCostMultiplier`; add new dimensions only when a modifier concretely needs one.
- **`KnowledgeYieldModifier` was deliberately not created as an `IOutcomeModifier`** — it would have required a cross-namespace dependency (`Mossmark.Development` → `Mossmark.World`). Knowledge yield injection lives in `WildernessYieldAttendable.BuildKnowledgeInjectedYields()` instead. Follow this precedent: keep modifier logic in the World namespace when the data it touches is World-scoped.
- **`using System;` is avoided in files that use `UnityEngine.Random`** — it creates an ambiguous `Random` reference. Use `System.Array.Copy` (explicit), not `Array.Copy`.
- **CSV pipeline (Iteration 25, restructured in Iteration 38) uses C# Editor scripts**, not Python. `ExportGameData.cs` / `ImportGameData.cs` / `CsvUtil.cs` / `ConditionCsvImporter.cs` under `Assets/Editor/`. `ReadCsv()` skips `#`-prefixed comment lines. Item references stored as `ItemDefinition.DisplayName` strings, resolved to assets at import time via a pre-loaded dictionary. Unbounded lists (stages, conditions, yield-table entries) are one-row-per-entry relational files joined by id — the old slot-numbered column prefixes (`stage1_*`, `bStage1_*`, `spotKnowledge1_*`) are gone; only genuinely per-owner short lists (knowledge yields on spots, wandering-thing modifiers) keep dynamic per-run-maximum columns.
- **`EntityFeedback`** (`Mossmark.Visuals`, Iteration 32) is added to every entity that can produce signal — the "felt, not read" visual vocabulary. Three signals: a progress pulse on `OnProgressMade`; a one-way triangle→circle stage-cross shape swap plus a larger pop on `OnDeveloped`; a passive-drift halo shown on `NpcAttendable.OnPassiveDriftAccrued`, hidden on the next productive progress tick. Explicitly reused unmodified by [Prototype 3](PROTOTYPE3_KNOWLEDGE_SPINE.md).
- **`CircleSpriteGenerator`** mirrors `TriangleSpriteGenerator` (`Initialize(color)` + a static `CreateSprite(color, textureSize)`) and backs `EntityFeedback`'s shape swaps and halo children.
- **`PropertyRegistry` is a static class in `Mossmark.Inventory` (Iteration 35)**: 8 folk-phrase properties hard-coded as `PropertyDefinition[]` — not a ScriptableObject, since the vocabulary is small and authoring it in C# is simpler than asset-reference wiring. CSV pipeline can absorb ids/phrases as plain string columns in a future pass. `PropertyKnowledge` (static, `Mossmark.Development`) is the session-only discovery store — same flat dictionary shape as `WorldState`, accessible via `WorldContext.IsPropertyKnown(itemId, propertyId)`. Item properties are authored as `string[]` on `ItemDefinition.propertyIds` (YAML arrays in `.asset` files). `InventoryUI` shows known phrases in muted green below the item name; unknown properties render as the single fixed line "There's more to it." Debug: backtick (` `` `) calls `PropertyKnowledge.RevealAll()` and refreshes; F2 toggles `ShowDebugTags` (shows property ids instead of phrases). Property coverage: every property appears on ≥2 items — `heavy_true`: Bog Iron + Flat Stones; `holds_the_cold`: Bog Iron + Clay; `split_prone`: Reeds + Flint; `binds_fast`: Reeds + Bark Strips; `draws_the_eye`: Crow Feather + Raven's Eye; `keeps_well`: Mistletoe + Bark Strips; `turns_water`: Mistletoe + Clay; `burns_slow`: Flint + Sticks. `PropertyDefinition.IsAdjectival` (default false) flags phrases that are adjectival rather than verb phrases — only `heavy_true` ("heavy and true") sets it, since every other phrase reads fine spliced straight after "what"/"it"/"that". Any code building such a clause should read `PropertyDefinition.Clause` (`IsAdjectival ? $"is {Phrase}" : Phrase`) instead of `Phrase` directly — every existing splice site does (`AcquaintableAttendable`, `EarnedSurfaceAttendable`, `PropertyAvailableCondition`, P3's `KnowingEntityAttendable`/`WorkingSurfaceAttendable`, `WorkshopUI`'s failure line), found and fixed in P4's Iteration 4.11 when the Collier's `heavy_true` pairing exercised the bug live. The two sites that print `Phrase` as a bare label (`ItemPropertyDisplay`, `TakenLedgerUI`) intentionally still use `Phrase`, not `Clause` — a standalone tag needs no verb.
- **`ConversionDef` ScriptableObject** (`Mossmark.Inventory`, Iteration 36) is the recipe SO: `Input` entries are `Item`-keyed (specific `ItemDefinition` + quantity) or `Property`-keyed (any item carrying the named `propertyId`). `TryMatch(placed, out matchedSlots)` does greedy left-to-right matching. Underlies the `biasPropertyIds` station-bias pattern that [Prototype 3](PROTOTYPE3_KNOWLEDGE_SPINE.md) also reuses.
- **`WorkshopUI` slots are references, not removals**: placing an item in a slot doesn't remove it from inventory — items are consumed only on a successful `Work()` (`HandleSuccess()`), so `Close()` needs no return-all-items path. `HandleFailure()` reveals one not-yet-known property from the placed items at random. Its `IsOpen` guard (checked in both `PlayerController.HandleInput()` and `AttentionManager.HandleHoldStarted()`, same as `ChestUI`) is the pattern any future modal UI capturing input should follow.
- **`PropertyAvailableCondition`** (`Mossmark.Development`, Iteration 37) is the carry-and-consume "wants a property, not an item" gate — checks `InventoryManager.Instance.Stacks` (carry-only, not chest) for any item carrying the required `propertyId`, consumes the highest-quantity matching stack on success. This is the mechanic [Prototype 3](PROTOTYPE3_KNOWLEDGE_SPINE.md)'s knowledge-transfer test is designed to test *against* — deliberately excluded from that scene so it doesn't contradict the hypothesis.
- **Relational data architecture (Iteration 38)**: `PlaceArchetype` is a thin composition root referencing `WildernessSpotDefinition[]`, an `NpcStagePool`, and a `BuildingStagePool` rather than embedding their data. Stage gates are `[SerializeReference] IDependencyCondition[]` authored via `stage_conditions.csv` → `ConditionCsvImporter` (assigned via `SerializedProperty.managedReferenceValue`, idempotent). Pool order is semantic — a pool's `stages` array mirrors CSV row order, not alphabetical. Import creates assets on demand and is a no-op on unchanged CSVs. Full schema: [DATA_SCHEMA.md](DATA_SCHEMA.md). This pipeline is out of scope for Prototype 3, which hand-authors its content directly.
- **Station-ness is stage data, not a component (Iteration 39)**: `BuildingStageDef.biasPropertyIds` (plus optional `stationName`) on an applied stage makes the fully-developed `BuildingAttendable` a conversion station — attending opens `WorkshopUI` instead of the linger flavor. `IWorkStation` (`Mossmark.Inventory`) defines the station contract UI-side so `WorkshopUI` never references `Mossmark.Development` — same directionality precedent as `KnowledgeYieldModifier`. Bias filters both recipe resolution and property discovery: an off-bias property can't be learned at the wrong station, which is what makes station choice a decision.
- **Station capability is keyed off the most recently applied stage, not the last array entry (Iteration 40)**: `BuildingAttendable.AppliedStationStage` searches backward from `CurrentStageIndex` for the first stage with a non-empty `biasPropertyIds`, so a building can stay station-capable even with further stages pending.
- **`AttentionOutcomeKind`** (`Develop`/`Open`/`Maintain`/`Visit`, Iteration 41) formalizes `BuildingAttendable`'s maintenance → develop → open-station → visit priority chain into one `PredictOutcomeKind()` method, read via a single `lastOutcomeKind` field after `OnAttentionComplete()` (same read-after-complete timing rule as ever) — replacing three separate `lastAttentionWasX` bools. `OutcomeRequest.DurationMultiplier` (parallel to `ChanceMultiplier`/`DaylightCostMultiplier`) runs through the same `IOutcomeModifier` pipeline for tick-duration scaling, alongside `DriftColdDaylightModifier` for daylight cost.
- **Site/Exhaustion/Standing/Dominance/Flow-Reserve system (Iterations 42–54)** — `WorldGenerator` site clustering (`SpawnArchetypeSites()`), POI three-tier reveal (`PoiTier`), per-spot exhaustion vs. Site-scoped Standing (`DevelopingWildernessSpotAttendable`, `WorldSite`), attention-weighted archetype dominance, and the Bog Keeper flow-reserve/differentiated-seam pilots are a live, working system in `Greybox.unity`, but this whole thread is explicitly parked as orthogonal to the current direction — see [PROTOTYPE3_KNOWLEDGE_SPINE.md](PROTOTYPE3_KNOWLEDGE_SPINE.md)'s "Deliberately absent" list. Full iteration-by-iteration detail (including a same-session course-correction on `WorldSite`, a `[SerializeReference]`-defaults gotcha found in Iteration 53, and per-archetype POI unlock authoring) is in [CLAUDE_ARCHIVE.md](CLAUDE_ARCHIVE.md); the System Overview table above still indexes each iteration's key types for when this thread resumes.
