# Mossmark — Project Guide

## Project Goal

A top-down 2D RPG focused on **world and town development** rather than player character progression. Players collect items, complete collection-based quests, and use quest rewards to upgrade NPCs, buildings, and towns. Tech is per-town and must be manually transferred between towns in later phases.

See [IDEAS.md](IDEAS.md) for the high-level vision and [PROTOTYPE.md](PROTOTYPE.md) for the current prototype plan and iteration roadmap.

---

## Environment

- **Unity version**: 6000.2.12f1 (Unity 6)
- **Render pipeline**: 2D (built-in, no URP/HDRP)
- **Physics**: 2D (`Rigidbody2D`, gravity = 0 for top-down movement)
- **Input**: Unity legacy Input system (`Input.GetAxisRaw`, `Input.GetButtonDown`)
- **UI system**: Unity UI Toolkit (`UIDocument`, `VisualTreeAsset`, `VisualElement`) — **not** uGUI/Canvas

---

## Code Conventions

### Language and approach
- **C# only** — no visual scripting (no Bolt, no Unity Visual Scripting graphs)
- **Code-first**: all game logic lives in MonoBehaviours, plain C# classes, and ScriptableObjects
- **1st-party APIs first**: prefer Unity's built-in systems over 3rd-party plugins for gameplay code
- **No 3rd-party gameplay frameworks** — the Top Down Engine (TDE) integration is a deprecated prior approach; ignore all assets under `Assets/Game/Scenes/TDE/` and any folder with "TDE" in the name

### Namespaces
All scripts live under the `Mossmark` root namespace, organized by system:

| Namespace | Purpose |
|---|---|
| `Mossmark.Player` | Player controller, camera, input relay |
| `Mossmark.Interaction` | Interaction framework (zones, manager, UI) |
| `Mossmark.Quests` | Quest definitions, instances, log, givers |
| `Mossmark.Entities` | NPC/building/town base class and upgrade trees |
| `Mossmark.Inventory` | Inventory manager and item pickup |
| `Mossmark.World` | World and town generation |
| `Mossmark.Combat` | Combat encounters and loot |

### Architecture rules
- **Event-driven**: systems communicate via `UnityEvent<T>` or C# `event Action<T>`, not direct method calls across system boundaries
- **Singleton managers**: one per system (e.g., `InteractionManager.Instance`), implemented as `MonoBehaviour` singletons with `Awake` guard
- **UI holds no game logic**: UI components only display state pushed from managers; they never write to game state directly
- **Interfaces for contracts**: use interfaces (`IInteractable`, `IInteractionUI`) for cross-system contracts so systems stay decoupled
- **ScriptableObjects for data**: item definitions, upgrade definitions, biome data, loot tables — all as `ScriptableObject` assets stored under `Assets/Game/Data/`. Quest data (`QuestDefinition`) is generated at runtime as a plain C# object — not a ScriptableObject asset. Do **not** use `Assets/Game/Resources/` for new assets; the `Resources/` folder is legacy and should not grow.
- **Procedural spawning — inactive-GO pattern**: when creating GameObjects at runtime, call `SetActive(false)` immediately after `new GameObject()`, add all components and call their `Initialize()` methods, then `SetActive(true)`. This ensures `Awake` fires with correct data already set on serialized fields. Components that participate in procedural spawning must expose a public `Initialize()` method.
- **Each iteration leaves prior systems functional**: never break existing systems when adding new ones

### Comments
Default to no comments. Only add a comment when the *why* is non-obvious (a hidden constraint, a Unity quirk, a workaround). Never describe what the code does — good naming handles that.

---

## Active Scenes

- **`Assets/Game/Scenes/Overworld.unity`** — the active prototype scene; all new work targets this scene
- **`Assets/Game/Scenes/TDE/`** — deprecated prior approach using Top Down Engine; ignore entirely

---

## System Overview (Prototype)

| System | Status | Key Types |
|---|---|---|
| Input + Movement | Complete | `PlayerController`, `InteractionInput` |
| Interaction Framework | Complete | `InteractionManager`, `InteractionUIManager`, `IInteractable`, `InteractionMenuControllerUITK`, `BuildingInteractable` |
| Quest Foundation | Complete | `WeightedQuestItem`, `QuestDefinition` (runtime class), `QuestInstance`, `QuestManager`, `QuestGiver`, `QuestLog`, `QuestUIPanel` |
| Items + Inventory | Complete | `ItemDatabase`, `InventoryManager`, `ItemPickupInteractable`, `InventoryUI`, `SettlementChest` |
| Entity Upgrades | Complete | `Entity`, `TownEntity`, `UpgradeDefinition`, `UpgradeDependency`, `UpgradePool`, `TownUpgradeDefinition`, `EntityManager` |
| World Generation | Complete | `WorldGenerator`, `TownGenerator`, `BiomeData`, `NpcSpawnConfig`, `BuildingSpawnConfig`, `ItemSpawnConfig`, `EnemySpawnConfig` |
| Combat Encounters | Complete | `CombatManager`, `Enemy`, `LootTable`, `WildernessEncounterHandler`, `CombatUIController` |
| Day Cycle | Complete | `DayCycleManager`, `DayPhase`, `DayCycleAmbientTextData`, `DayCycleUI`, `RestInteractable` |

Full iteration plan is in [PROTOTYPE.md](PROTOTYPE.md).

---

## Key Design Decisions

- **Interaction state machine**: `Idle → Prompting → InProgress`; managed exclusively by `InteractionManager`; UI reacts to state change events
- **Quest mechanic**: collection quests — bring X of item A, Y of item B; completing a quest upgrades the assigning entity. Quests are generated procedurally at runtime by `QuestGiver.GenerateQuest()` from a `WeightedQuestItem` pool (configured on `NpcSpawnConfig`, `BuildingSpawnConfig`, and `BiomeData`). A per-giver abandon cooldown prevents immediately re-rolling for a more convenient quest
- **Upgrade pools**: flat `UpgradePool` ScriptableObject per entity type holds a set of `UpgradeDefinition` candidates. Each definition declares `UpgradeDependency` entries (`Single`/`AnyOf`, `Self`/`Town` scope) that must all be satisfied before it becomes available. Quest turn-in grants a pending point; the player applies it via the interaction menu — available upgrades are computed fresh from satisfied dependencies at open time. Town Hall uses `TownEntity` + `TownUpgradeDefinition` for passive effects; `TownEntity` also aggregates all unlocked upgrades across registered entities for `Town`-scope dependency resolution.
- **World generation**: fully randomized each session (no save/load in prototype); single biome in prototype
- **Grey-box visuals**: prototype uses placeholder art throughout; no visual polish until systems are solid
- **Day cycle**: `DayCycleManager` tracks a 7-action pool per day and a `DayPhase` (`Dawn`→`Morning`→`Midday`→`Afternoon`→`Dusk`) derived from actions spent. `ConsumeAction(description)` decrements the pool and emits ambient text via `DayCycleAmbientTextData`; once spent, actions still proceed but show a "nothing left to give" note. `RestInteractable` (on the Mead Hall, or a standalone Bedroll) advances the day, restores the pool, re-seeds forage items via `WorldGenerator.ReseedForageItems()`, and fires `OnDayAdvanced` — consumed by `TendedSpotInteractable`, `StruckNodeInteractable`, `EncounterLocation`, and `MerchantSpawner` for their day-based timers, and by `NPCInteractable`/`BuildingInteractable` to reset once-per-day upgrade actions. `DayCycleUI` shows the current day/phase and dims when the pool is spent; once 1 or fewer actions remain, it also shows a narrative hint label that the day has little left to give.
- **Carry limit and settlement storage**: `InventoryManager.CarryLimit` (8) caps the number of distinct item stacks the player carries — `CanAddItem`/`AddItem` enforce it, `UsedSlots` exposes the current count, and `InventoryUI` shows "Inventory (N/8 slots)". `ItemPickupInteractable` shows a "Carry full" prompt and leaves the item in the world when picking it up would exceed the limit. `SettlementChest` is an `IInteractable` placed at the town center by `TownGenerator` with uncapped storage; its menu offers full-stack "Deposit"/"Withdraw" options (withdraw disabled when carry is full). `QuestManager` sums carried inventory and chest counts for completion checks, draws required items from carry first then the chest on turn-in, and routes rewards to the chest if carry is full.
