# Mossmark — Project Guide

## Project Goal

A top-down 2D RPG focused on **world and town development** rather than player character progression. The player moves through the world and holds a single **Attention** action on whatever is nearby — what happens is a property of the thing being attended to, not a menu choice. Sustained attention develops NPCs, buildings, and the town over time, revealing new specializations and access rather than presenting an upgrade menu. Tech is per-town and must be manually transferred between towns in later phases.

See [IDEAS.md](IDEAS.md) for the high-level vision and [PROTOTYPE2.md](PROTOTYPE2.md) for the current prototype design and iteration roadmap.

> **Note:** This is "Prototype 2" — a fresh Unity project started from scratch (see PROTOTYPE2.md's "Why a second prototype"). It is informed by Prototype 1 but does not carry over P1's architecture; P1 systems referenced in PROTOTYPE2.md (e.g. `PlayerMovement`, `DayCycleManager`, `Entity`/`UpgradePool`) are reference material from the old project, not code that exists here yet.

---

## Environment

- **Unity version**: 6000.4.11f1 (Unity 6)
- **Render pipeline**: URP, 2D Renderer (`Assets/Settings/Renderer2D.asset`). The active scene has a Global Light 2D — 2D lights are available and may be used later for day/night ambience (the stamina/day cycle in PROTOTYPE2.md).
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
| `Mossmark.Day` | Stamina/day-clock — phase tracking, ambient text, and the stamina HUD. Direct successor to P1's `DayCycleManager` |
| `Mossmark.Inventory` | Inventory manager, item database, item pickups, settlement chest |
| `Mossmark.Development` | Generic dependency/response resolver and developable entities (buildings, NPCs, POIs, town) — generalizes P1's `Entity`/`UpgradePool`/`TownEntity` |
| `Mossmark.World` | Region and town generation, place archetypes, and generic wilderness-spot attendables (e.g. `GenericWildernessSpotAttendable`) |

P1's `Mossmark.Quests` and `Mossmark.Combat` are **not** part of P2 — collection quests and discrete combat encounters were dropped per IDEAS.md's "Prototype 2" section. Add namespaces here as new systems land; keep this table in sync with what's actually implemented.

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
| Stamina / Day Clock | Complete | `DayCycleManager`, `DayCycleAmbientTextData`, `DayCycleUI` |
| Bedroll + Day Transition | Complete | `BedrollAttendable`, `DayTransitionFadeUI`, `DayCycleManager.Rest()` |
| Tended-Style Spots | Complete | `TendedSpotAttendable` |

Full iteration plan is in [PROTOTYPE2.md](PROTOTYPE2.md). Update this table as each iteration lands.

---

## Key Design Decisions

This section records decisions made during implementation that aren't already captured in PROTOTYPE2.md. Populate it iteration-by-iteration, the way the old CLAUDE.md did for Prototype 1.

- **Render pipeline & input, locked in at bootstrap**: kept the Unity 6 2D template's URP 2D Renderer (2D lights available for the later day/night cycle) and the new Input System with project-wide actions (see "Input Actions" above), rather than reverting to built-in 2D / legacy `Input.GetAxis` as the old CLAUDE.md (written for P1) specified.
- **`PlayerController`/`CameraFollow`/`TriangleSpriteGenerator`** ported from P1's `PlayerMovement.cs`, `CameraFollow.cs`, `TriangleSpriteGenerator.cs`, dropping the `CombatManager`/`DayCycleManager`/`RestInteractable` hooks that don't exist in P2 yet (per PROTOTYPE2.md, Section 1: "Drop the `CombatManager`/exhaustion-collapse hooks").
- **`AttentionInput` uses the `Attend` action's `started`/`canceled` callbacks, not `performed`.** The Hold interaction's `started` fires on press and `canceled` fires on release regardless of the interaction's own (default 0.4s) duration, so these give clean raw press/release signals. `AttentionManager` owns the actual hold-duration timing per-target via `IAttendable.AttentionDuration`, so the Hold interaction's configured duration is present but unused — left as-is rather than switching to a plain Button interaction, since `started`/`canceled` already give the needed signals.
- **`AttendableZone` resolves `IAttendable` via `GetComponent<IAttendable>()`** on its own GameObject rather than a serialized interface reference — every attendable type is expected to carry its `IAttendable` component and an `AttendableZone` on the same GameObject, which fits the inactive-GO procedural spawn pattern for later iterations.
- **Player's attention-range trigger doubles as its required `Collider2D`**: a `CircleCollider2D` (trigger, radius 1.5) was added directly to the Player in the editor for `AttendableDetector`'s range queries; `PlayerController.Awake()`'s fallback `AddComponent<CircleCollider2D>()` now finds this one and skips adding a second.
- **`AttentionManager` finds `AttendableDetector`/`AttentionInput` via `FindAnyObjectByType` in `Start()`** rather than serialized cross-scene references — keeps the Player's components decoupled from the manager's placement in the hierarchy. (Originally `FindFirstObjectByType`; switched to `FindAnyObjectByType` in Iteration 3 since the former is obsolete and ordering guarantees aren't needed for a one-of-each singleton lookup.)
- **`AttentionManager.AttendingTarget`** exposes the in-progress attend target separately from `CurrentTarget` (the detector's nearest target, which can change mid-hold). `AttendableOverlayUI` reads `AttendingTarget` while `State == Attending` and `CurrentTarget` otherwise, so the overlay always reflects the attendable actually being held.
- **`AttendableOverlayUI` builds its `VisualElement` tree entirely in code** (`OnEnable`, via `UIDocument.rootVisualElement`) and creates a runtime `PanelSettings` via `ScriptableObject.CreateInstance` rather than referencing UXML/USS/PanelSettings assets — keeps the overlay code-first/grey-box and avoids asset-reference wiring through MCP-Unity's `update_component` (which has no reliable way to assign `UnityEngine.Object` asset references). The hold-progress bar reuses P1's `[####....]` text-bar pattern (10 segments, built from `HoldProgress01`).
- **"Overlay UI" GameObject** (root, `UIDocument` + `AttendableOverlayUI`) is a new sibling to `AttentionManager` in the scene — UI reads `AttentionManager.Instance` state each frame in `Update()`, consistent with "UI holds no game logic."
- **`PlaceholderAttendable` removed in Iteration 4**: now that `GenericWildernessSpotAttendable` is a real attendable type, the script, its "Placeholder Attendable" GameObject, and the corresponding scene wiring were all deleted per the removal note this section previously carried.
- **`GenericWildernessSpotAttendable` (Iteration 4) lives in `Mossmark.World`**, not `Mossmark.Attention` or `Mossmark.Development` — it's the first of the "generic spots/POIs" attendable types `Mossmark.World` was reserved for, keeping `Mossmark.Attention` limited to the framework itself (interfaces, zones, detector, manager, overlay).
- **`GenericWildernessSpotAttendable.AttentionDuration` is `0f`**: foraging is an instant yield-roll rather than a timed hold. `AttentionManager`'s existing hold-duration logic handles a zero duration as "complete immediately on attend", so no changes to `AttentionManager`/`AttendableOverlayUI` were needed — the overlay's progress bar simply never has time to render.
- **Yield rolls use a weighted pool (`ItemYield[] commonYields`) plus an independent rare-drop roll (`ItemYield rareYield` + `rareDropChance`)**: every attend rolls exactly one common yield by weight, and separately has a flat chance (default 8%) to also award the rare yield. Both rolls are logged via `Debug.Log` for now — there's no toast/notification UI yet.
- **`ItemDatabase` (from PROTOTYPE2.md's P1 reference list) was deferred**: `GenericWildernessSpotAttendable` references `ItemDefinition` assets directly via serialized `ItemYield` fields, and `InventoryManager` only needs `ItemDefinition` identity (not id-based lookup). Revisit if a system needs to resolve items by string id (e.g. save/load, crafting recipes).
- **`InventoryManager.AddItem` returns the number of units actually added**; any remainder when both the matching stacks are full and `CarryLimit` (8 stacks) is reached is refused outright rather than dropped on the ground or queued — callers (e.g. `GenericWildernessSpotAttendable`) log a "no room to carry it" message when `added == 0`.
- **`InventoryUI` reads `InventoryManager.Instance` in `Start()`, subscribing to `InventoryChanged` there** rather than `OnEnable()` — same cross-GameObject `Awake`-ordering reasoning as `AttentionManager`'s `Start()`-time lookups, since `Start()` is guaranteed to run after every `Awake()`.
- **"Inventory UI" GameObject** (`UIDocument` + `InventoryUI`) is another code-first overlay sibling to "Overlay UI", anchored top-left vs. the attendable overlay's placement — both follow the same runtime-`PanelSettings`/`VisualElement`-tree-in-code pattern established in Iteration 3.
- **New `Mossmark.Day` namespace (Iteration 5)** holds the stamina/day-clock system — `DayCycleManager`, `DayCycleAmbientTextData`, `DayCycleUI`. Kept separate from `Mossmark.Attention` even though attention spends stamina: "attention is the clock" per PROTOTYPE2.md, but the day-cycle state itself is a concern other future attendables (buildings, NPCs, the Iteration 6 bedroll) will also read/write without being part of the attention framework.
- **`IAttendable.RequiresStamina`** is the new interface member deciding whether `AttentionManager` calls `DayCycleManager.SpendStamina()` after `OnAttentionComplete()`, and whether `AttendableOverlayUI` replaces the attendable's normal interaction line with "Too late to start that now." when `DayCycleManager.HasStamina` is false. `AttentionManager.HandleHoldStarted` also checks this *before* starting a hold, so a zero-stamina attempt on a stamina-costing target consumes nothing and never enters `Attending`. `GenericWildernessSpotAttendable.RequiresStamina => true`; non-stamina attendables (e.g. the Iteration 6 bedroll) will return `false`.
- **`DayCycleManager.SpendStamina()` costs a flat 1 per completed stamina-costing attention** — the direct successor to P1's `ActionsPerDay`/`ActionsRemaining` action-pool. `MaxStamina` defaults to 24, split into 6 even `DayPhase` bands (Dawn → Morning → Midday → Afternoon → Evening → Dusk, 4 stamina each) via a ratio-threshold `GetPhase()`, so phase-crossing ambient text fires 5 times over a full day.
- **`DayCycleAmbientTextData` is a `ScriptableObject`** (`Assets/Game/Data/Day/DefaultAmbientText.asset`) holding one or more candidate lines per `DayPhase`; `DayCycleManager` picks one at random on each phase crossing and logs it via `Debug.Log`, following Iteration 4's "no toast/notification UI yet" precedent rather than adding a transient-message UI.
- **Wiring `DayCycleManager.ambientTextData`** (a `ScriptableObject` reference, unlike the code-first UI components) couldn't be set via MCP-Unity's `update_component` per the asset-reference limitation noted above — the GameObject/component were added via MCP, then the scene `.unity` file was hand-edited to add `{fileID: 11400000, guid: ..., type: 2}` for the `DefaultAmbientText` asset, followed by `load_scene` to pick up the change before re-saving.
- **"Day Cycle UI" GameObject** (`UIDocument` + `DayCycleUI`) is another code-first overlay sibling, anchored top-right with a phase label and a `[####....]`-style stamina bar (20 segments, same text-bar pattern as the hold-progress bar) that dims to translucent white at zero stamina.
- **`DayCycleManager.Rest()` (Iteration 6)** is a coroutine-driven fade-to-black / reset / fade-in: `RestRoutine()` sets `IsTransitioning = true`, fades `FadeAmount01` 0→1 over `transitionFadeDuration` (0.6s), resets `StaminaRemaining` to `maxStamina` and calls `SetPhase(DayPhase.Dawn)`, fades 1→0, then clears `IsTransitioning`. `SpendStamina`'s inline phase-change/ambient-text logic was extracted into the shared `SetPhase()` helper so `Rest()` reuses the same ambient-text-on-phase-change behavior.
- **`DayCycleManager.IsTransitioning` is the cross-system lock for the rest transition**: `PlayerController.HandleInput()` zeroes `movement` and returns early while it's true, and `AttentionManager.HandleHoldStarted()` refuses to start a new hold — generalizing P1's `RestInteractable` lock to the whole game loop (movement + attention) rather than just the bedroll itself.
- **`BedrollAttendable` lives in `Mossmark.Day`**, not `Mossmark.World` or `Mossmark.Attention` — it's tightly coupled to `DayCycleManager.Rest()` rather than being a generic world/wilderness attendable. It sets `RequiresStamina => false` (resting restores stamina, so the "too late to start that now" zero-stamina gate doesn't apply) and `CanAttend() => true`, since `AttentionManager.HandleHoldStarted`'s `IsTransitioning` check already covers re-attending the bedroll mid-transition.
- **`DayTransitionFadeUI`** is a code-first overlay sibling like the others, but with `UIDocument.sortingOrder = 100` so its full-screen black `VisualElement` renders above Overlay/Inventory/Day Cycle UI. It polls `DayCycleManager.Instance.FadeAmount01` every frame and sets the overlay's `backgroundColor` alpha directly — same "manager owns progress, UI just displays it" pattern as `AttendableOverlayUI`'s hold-progress bar, applied to a continuous fade instead of a discrete bar.
- **"Reseeding wilderness spots" (third clause of the Iteration 6 deliverable) is currently a no-op**: `GenericWildernessSpotAttendable` is stateless and instant-yield ("no state, no wait" per PROTOTYPE2.md), so attending Field/Clay Pit again after `Rest()` already behaves identically to before — there's nothing to reseed yet. This will need a real `OnDayAdvanced`-style hook once Iteration 7 (Tended-Style Spots) introduces per-spot state.
- **"Bedroll" GameObject** (`TriangleSpriteGenerator` colored dark red-brown, `CircleCollider2D` trigger radius 0.5, `AttendableZone` + `BedrollAttendable`) was hand-placed in the scene at world (0, 3, 0), following the same pattern as the Field/Clay Pit wilderness spots. **"Day Transition Fade UI"** (`UIDocument` + `DayTransitionFadeUI`) is a new sibling alongside Overlay UI / Inventory UI / Day Cycle UI.
- **`DayCycleManager.DayAdvanced` event (Iteration 7)** fulfills the `OnDayAdvanced`-style hook flagged as deferred above. It fires once per `Rest()`, in `RestRoutine()` right after `StaminaRemaining`/`CurrentPhase` are reset to Dawn but before the fade back in. `TendedSpotAttendable` subscribes in `Start()` and unsubscribes in `OnDestroy()` to advance its mark → wait → harvest countdown.
- **`TendedSpotAttendable` (Iteration 7) lives in `Mossmark.World`**, alongside `GenericWildernessSpotAttendable` — both are wilderness-spot attendable types; tended spots add an `Unmarked`/`Marked`/`Ready` state machine (ported from P1's `TendedSpotInteractable`) on top of the same `IAttendable` shape.
- **State-driven `CanAttend()`/overlay text**: `Unmarked` → `CanMark()` (gated by the cap; overlay shows "(too many spots already tended)" without consuming a hold when capped), `Marked` → `false` (overlay shows "ready in N rest(s)" countdown, blocked from re-marking), `Ready` → `true` (harvest allowed). `Mark()` and `Harvest()` both run inside a single `OnAttentionComplete()`, switched on `state`.
- **`MaxConcurrentMarked` cap via a `static Dictionary<string, int>` keyed by `displayName`** — generalizes P1's `TendedSpotConfig`-keyed `_markedCounts` (one shared SO per spot type) to P2's per-component-config approach, where each `TendedSpotAttendable` instance carries its own `displayName`/`maxConcurrentMarked` rather than referencing a shared asset. `OnDestroy()` decrements the count if a `Marked` spot is destroyed.
- **`TendedSpotAttendable.RequiresStamina => false`, for both marking and harvesting**: `AttentionManager.CompleteAttention` reads `RequiresStamina` *after* `OnAttentionComplete()` runs, by which point a successful `Harvest()` has already reset `state` back to `Unmarked` — a state-dependent answer at that point can't distinguish "just harvested" from "nothing to harvest." Rather than add latching state to work around this read-order, tended spots are framed as free "tending" check-ins (cultivation), distinct from `GenericWildernessSpotAttendable`'s stamina-costing active extraction.
- **Visual state feedback via `SpriteRenderer.color` tinting**, not sprite/material swaps: `TriangleSpriteGenerator` bakes its base color into the sprite texture at `Awake()`, and `TendedSpotAttendable.UpdateVisual()` sets the (default-white, multiplicative) `SpriteRenderer.color` directly — unchanged (white) for Unmarked, dark grey for Marked, warm yellow for Ready.
- **`Assets/Refresh` needed after creating a brand-new component's `.cs` file before MCP-Unity's `update_component` can find it**: `recompile_scripts` compiled `TendedSpotAttendable` with 0 errors, but `update_component` still failed with "Component type not found" because no `.meta` existed yet for the new file — `recompile_scripts` compiles but doesn't trigger an `AssetDatabase` import. `execute_menu_item` with `menuPath: "Assets/Refresh"` triggers the import (the `.meta` then appears), after which `update_component` succeeds.
- **"Bramble Patch" GameObject** (`TriangleSpriteGenerator` colored dark magenta/plum, `CircleCollider2D` trigger radius 0.5, `AttendableZone` + `TendedSpotAttendable`) was hand-placed at world (0, -3, 0) as the first `TendedSpotAttendable` test entity: `displayName: "Bramble Patch"`, `tendVerb: "tend"`, `restsToHarvest: 1`, `maxConcurrentMarked: 2`, `harvestYield` → new `berries.asset` `ItemDefinition` (1-2 Berries, weight 1). The `harvestYield.Item` reference was wired via the established hand-edit-`.unity`-then-`load_scene` pattern, since MCP `update_component` can't set `UnityEngine.Object` references on nested serializable fields.
