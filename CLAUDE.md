# Mossmark — Project Guide

## Project Goal

A top-down 2D RPG focused on **world and town development** rather than player character progression. The player moves through the world and holds a single **Attention** action on whatever is nearby — what happens is a property of the thing being attended to, not a menu choice. Sustained attention develops NPCs, buildings, and the town over time, revealing new specializations and access rather than presenting an upgrade menu. Tech is per-town and must be manually transferred between towns in later phases.

See [IDEAS.md](IDEAS.md) for the high-level vision and [PROTOTYPE2.md](PROTOTYPE2.md) for the current prototype design and iteration roadmap.

> **Note:** This is "Prototype 2" — a fresh Unity project started from scratch (see PROTOTYPE2.md's "Why a second prototype"). It is informed by Prototype 1 but does not carry over P1's architecture; P1 systems referenced in PROTOTYPE2.md (e.g. `PlayerMovement`, `DayCycleManager`, `Entity`/`UpgradePool`) are reference material from the old project, not code that exists here yet.

---

## Environment

- **Unity version**: 6000.4.11f1 (Unity 6)
- **Render pipeline**: URP, 2D Renderer (`Assets/Settings/Renderer2D.asset`). The active scene has a Global Light 2D — 2D lights are available and may be used later for day/night ambience (the daylight/day cycle in PROTOTYPE2.md).
- **Physics**: 2D (`Rigidbody2D`, gravity = 0 for top-down movement)
- **Input**: new Input System (`com.unity.inputsystem`), via the project-wide actions asset `Assets/InputSystem_Actions.inputactions`. See "Input Actions" below.
- **UI system**: Unity UI Toolkit (`UIDocument`, `VisualTreeAsset`, `VisualElement`) — **not** uGUI/Canvas
- **Editor tooling**: `com.gamelovers.mcp-unity` is installed for MCP-based editor automation (not yet wired into this session's tools).

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
| `Mossmark.World` | Region and town generation, place archetypes, and generic wilderness-spot attendables (e.g. `GenericWildernessSpotAttendable`) |

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

- **`Assets/Game/Scenes/Overworld.unity`** — the active prototype scene; all new work targets this scene
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
| Numeric Tuning Pass (Iteration 26) | Complete | `place_archetypes.csv`, `wilderness_spots.csv`, `wandering_things.csv`, `Overworld.unity` NPC tick fields |
| Always-Something-Happens (Iteration 28.5) | Complete | `NpcAttendable` visit mechanic, `BuildingAttendable` flavor linger, `PlaceArchetype` exchange/restored-flavor data |
| Settlement Maintenance (Iteration 29) | Complete | `IMaintenanceConsumer`, `MaintenanceManager`, `DevelopableEntity.DriftProgress`, `BuildingAttendable`/`NpcAttendable` cold state, `PlaceArchetype` maintenance fields |
| Settlement Growth: New Arrivals (Iteration 30) | Complete | `ArrivalCondition`, `ArrivalAttendable`, `ArrivalSpawner`, `NpcAttendable.Initialize()`, `settlement_grew` WorldState flag |

Full iteration plan is in [PROTOTYPE2.md](PROTOTYPE2.md). Update this table as each iteration lands.

---

## Key Design Decisions

Active implementation constraints carried forward from prior iterations. Full iteration-by-iteration history is in [CLAUDE_ARCHIVE.md](CLAUDE_ARCHIVE.md).
**Patterns that remain active constraints (full history in CLAUDE_ARCHIVE.md):**

- **`RequiresDaylight` / `ContinueAttending` are read *after* `OnAttentionComplete()`**, not before. This means `OnAttentionComplete()` sets a latch (`lastAttentionMadeProgress`, `lastAttentionWasVisit`) and those flags drive what the `AttentionManager` does next. Never read them before the call or derive them from pre-call state.
- **Inactive-GO spawning pattern**: `SetActive(false)` → add components + call `Initialize()` → `SetActive(true)`. `Awake()` fires with correct data already in place. All procedurally spawned entities follow this.
- **Asset references (`UnityEngine.Object` fields) cannot be set via MCP-Unity `update_component`**. Set them by hand-editing the scene `.unity` YAML, then calling `load_scene` to pick up the change before `save_scene`. This applies to `ItemDefinition`, `PlaceArchetype`, `WildernessSpotDefinition`, and similar SO references on spawned components.
- **`DayCycleManager.DayAdvanced` event is the hook for all per-rest state changes.** Subscribe in `Start()`, unsubscribe in `OnDestroy()`. `Start()` is used (not `Awake()`) because `DayCycleManager.Instance` is guaranteed available after all `Awake()`s have run.
- **`WorldContext` / `IOutcomeModifier` / `OutcomeRequest`** is the approved pattern for ambient world-state influencing outcomes. Do not hardcode world-state checks inside `OnAttentionComplete()` — add a modifier. `OutcomeRequest` currently carries `ChanceMultiplier` and `DaylightCostMultiplier`; add new dimensions only when a modifier concretely needs one.
- **`KnowledgeYieldModifier` was deliberately not created as an `IOutcomeModifier`** — it would have required a cross-namespace dependency (`Mossmark.Development` → `Mossmark.World`). Knowledge yield injection lives in `WildernessYieldAttendable.BuildKnowledgeInjectedYields()` instead. Follow this precedent: keep modifier logic in the World namespace when the data it touches is World-scoped.
- **`using System;` is avoided in files that use `UnityEngine.Random`** — it creates an ambiguous `Random` reference. Use `System.Array.Copy` (explicit), not `Array.Copy`.
- **`tendedness` does not hard-reset to `0.5f` on rest** — it drifts continuously. `+0.03f` if attended that day, `-0.08f` if not. Depletion (`<0.3`) is reachable after ~3 unattended rests from baseline; peak (`>0.7`) after ~6–7 attended rests. Tuning baselines only.
- **`TendedSpotAttendable` is excluded from tendedness** — it has its own `restsToHarvest` rest-cycle rhythm that conflicts with the continuous drift model.
- **NPC visit mechanic (Iteration 28.5)**: fully-developed NPCs (`drawnSpecializationId != null && GetNextStage() == null`) bypass `ResolveAttention()` and call `RunVisitInteraction()` instead. `lastAttentionWasVisit = true` makes `RequiresDaylight => true` (costs 1 daylight) and `ContinueAttending => false` (one-shot). Universal specs get flavor only; archetype specs roll `npcExchangeChance (0.2)` for a gift. Exchange flavor text is indexed to the gifted item — text and item can never mismatch.
- **Building linger mechanic (Iteration 28.5)**: fully-developed buildings (`GetNextStage() == null`) deliver a random flavor line at no daylight cost. `lastAttentionWasVisit` sets `RequiresDaylight => false`, `ContinueAttending => false`.
- **CSV pipeline (Iteration 25) uses C# Editor scripts**, not Python. `ExportGameData.cs` / `ImportGameData.cs` under `Assets/Editor/`. `ReadCsv()` skips `#`-prefixed comment lines. Item references stored as `ItemDefinition.DisplayName` strings, resolved to assets at import time via a pre-loaded dictionary. Column counts for array fields (yield pools, knowledge yields, post-spec stages) are dynamic — driven by per-run maximum, never hardcoded.
- **Tuned baseline values (Iteration 26)**: archetype spot tick intervals 2.0–2.5s; generic spot tick intervals 1.5–2.0s; NPC tick intervals 1.5–2.0s; Building Stage 1 `progressCost` 4 (8 items total at 2/tick); `maxDaylight` 24; `NpcAttendable.progressCost` 8; rare drop chance 6–8% base ×1.5 at dawn/dusk. See `place_archetypes.csv` comment block for full reasoning.
- **`IMaintenanceConsumer` interface (Iteration 29)** (`Mossmark.Development`) exposes `DriftThreshold`, `MaintenanceMaterial`, `MaintenanceCostPerReset`. `MaintenanceManager` (`[DefaultExecutionOrder(500)]`) subscribes to `DayCycleManager.DayAdvanced`, then in `OnDayAdvanced()` runs two ordered passes: (1) increment all developed entities' drift, (2) silently consume from chest for any `IMaintenanceConsumer` with `DriftProgress > 0` and sufficient stock. Order matters — all entities increment before any chest resolution, so one chest stock can cover multiple entities in a single rest.
- **`DevelopableEntity` drift fields (Iteration 29)**: `DriftProgress { get; private set; }`, `IncrementDrift()` (no-op pre-development), `ResetDrift()` (no-op if already 0, calls `OnDriftReset()`), `protected virtual OnDriftChanged()` / `OnDriftReset()` (empty base, overridden by `NpcAttendable`). `GetDriftOverlayDescription(displayName, threshold, coldFlavor)` returns `displayName` below 60% threshold, `"{displayName} — needs tending"` in the warning band, and `coldFlavor` (fallback `"{displayName} — seems neglected"`) at or above threshold.
- **`BuildingAttendable` maintenance priority order (Iteration 29)**: (1) direct maintenance — if `DriftProgress > 0` and carrying `maintenanceCostPerReset` of `maintenanceMaterial`, consume from pack, `ResetDrift()`, post notification, return; (2) linger (fully developed); (3) development. Cold buildings add an extra `SpendDaylight(1)` on productive development ticks (total 2 daylight per tick). `lastAttentionWasMaintenance` guards `RequiresDaylight => false` (maintenance costs no daylight) and `ContinueAttending => false` (one-shot). `maintenanceMaterial` is set from `stages[0].material` in `Awake()` — revival material = maintenance material.
- **`NpcAttendable` maintenance (Iteration 29)**: archetype NPCs implement `IMaintenanceConsumer` with `maintenanceMaterial` set from `archetype.CommonYields[0].Item` in `HandleDeveloped()`. Universal-spec NPCs get `null maintenanceMaterial` — their visit IS the maintenance path (`RunVisitInteraction()` always calls `ResetDrift()` when `DriftProgress > 0`). `setWorldStateFlags: HashSet<string>` tracks all flags set by post-spec stages; `OnDriftChanged()` suspends them all at the cold threshold; `OnDriftReset()` restores them. Priority order mirrors `BuildingAttendable`: (1) direct maintenance; (2) visit/linger (always resets drift if drifted); (3) development.
- **`PlaceArchetype` maintenance fields (Iteration 29)**: `buildingColdFlavor`, `buildingMaintenanceCost` (default 2), `npcColdFlavor`, `npcMaintenanceCost` (default 1), under a `[Header("Maintenance")]` block. CSV pipeline updated with these 4 columns. Cold flavor values start empty — authored per-archetype in a future content pass.
- **`HorizonUI` drift (Iteration 29)**: `Refresh()` now reads `attendable.GetOverlayDescription()` for the entity name column instead of `entity.DisplayName`, so drift suffix appears in the Settlement Horizon panel automatically.
- **`ArrivalCondition` (`Mossmark.World`, Iteration 30)** is a `[Serializable]` `IDependencyCondition` whose scope is the world rather than any entity. Checks `string[] requiredFlags` (all must be true via `WorldContext.GetFlag()`) and optionally `int minimumDevelopedEntities` (count of `DevelopableEntity` instances with `CurrentStageIndex >= 0`). `GetNeedsDescription()` returns `""` — arrivals are silent. The `entity` parameter to `IsSatisfied` is ignored, following the pattern from `PoiAttendable`'s gate.
- **`ArrivalAttendable` (`Mossmark.World`, Iteration 30)**: thin `MonoBehaviour, IAttendable`, not a `DevelopableEntity`. `AttentionDuration => 2f`, `RequiresDaylight => false`, `ContinueAttending => !promoted && warnessProgress < warnessThreshold`. `OnAttentionComplete()` increments `warnessProgress` and fires `onPromote(gameObject)` on threshold crossing. `CanAttend() => !promoted` (inert once promoted, before Unity destroys the GO).
- **`ArrivalSpawner` (`Mossmark.World`, Iteration 30)**: holds `ArrivalTrigger[]` (each entry a `[Serializable]` inner class with condition, color, name, NPC params). Subscribes to `DayCycleManager.DayAdvanced`. Per-trigger `[NonSerialized] bool fired` and `[NonSerialized] ArrivalAttendable spawnedArrival` prevent duplicate spawning — `fired` is set on promotion and never clears (one arrival per trigger per session). `OnArrivalPromoted()` creates the new `NpcAttendable` via inactive-GO pattern, calls `NpcAttendable.Initialize()`, sets `WorldState.SetFlag("settlement_grew", true)`, posts `"The settlement grows."`, then `Destroy(arrivalGo)` — safe because Unity queues destruction until end-of-frame, so `AttentionManager` reads `ContinueAttending => false` cleanly first.
- **`NpcAttendable.Initialize()` (Iteration 30)** is a new public method (`genericName`, `progressCost = 8`, `minTickInterval = 1.5f`, `maxTickInterval = 2f`) — the first time `NpcAttendable` is constructed at runtime rather than hand-placed in the scene. Sets private `[SerializeField]` fields before the inactive-GO `Awake()` fires.
- **`ArrivalSpawner.FindSpawnPosition()` (Iteration 30)** picks a random side of `WorldLayoutGenerator.TownBounds` and places the arrival at a random distance (`minDistFromTown`–`maxDistFromTown`, defaults 2–6) outside that edge — arrivals spawn near the town edge, not the deep wilderness. Validates within `WildernessBounds` and not inside `TownBounds`; falls back to due-south of town center after 50 attempts.
- **Two `ArrivalTrigger` entries in `Overworld.unity` (Iteration 30)**: (1) "A Stranger" — flags `["bog_keeper_iron_sense"]`, `minimumDevelopedEntities: 3`; (2) "Another Traveler" — flags `["herald_trail_markers", "hedge_witch_wound_lore"]`, `minimumDevelopedEntities: 4`. Both `warnessThreshold: 3`, `npcProgressCost: 8`, tick intervals 1.5–2s.
