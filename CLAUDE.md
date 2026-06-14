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
| `Mossmark.Inventory` | Inventory manager, item database, item pickups, settlement chest |
| `Mossmark.Development` | Generic dependency/response resolver and developable entities (buildings, NPCs, POIs, town) — generalizes P1's `Entity`/`UpgradePool`/`TownEntity` |
| `Mossmark.World` | Region and town generation, place archetypes |

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
| Attention Framework Core | Complete | `IAttendable`, `AttendableZone`, `AttendableDetector`, `AttentionInput`, `AttentionManager`, `PlaceholderAttendable` |
| Attendable Overlay UI | Complete | `AttendableOverlayUI` |

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
- **`PlaceholderAttendable`** is a temporary `IAttendable` test fixture (Iteration 2 only) standing in for the wilderness-spot/building/NPC attendable types that arrive in later iterations; it and the "Placeholder Attendable" GameObject should be removed once a real attendable type exists.
- **`AttentionManager.AttendingTarget`** exposes the in-progress attend target separately from `CurrentTarget` (the detector's nearest target, which can change mid-hold). `AttendableOverlayUI` reads `AttendingTarget` while `State == Attending` and `CurrentTarget` otherwise, so the overlay always reflects the attendable actually being held.
- **`AttendableOverlayUI` builds its `VisualElement` tree entirely in code** (`OnEnable`, via `UIDocument.rootVisualElement`) and creates a runtime `PanelSettings` via `ScriptableObject.CreateInstance` rather than referencing UXML/USS/PanelSettings assets — keeps the overlay code-first/grey-box and avoids asset-reference wiring through MCP-Unity's `update_component` (which has no reliable way to assign `UnityEngine.Object` asset references). The hold-progress bar reuses P1's `[####....]` text-bar pattern (10 segments, built from `HoldProgress01`).
- **"Overlay UI" GameObject** (root, `UIDocument` + `AttendableOverlayUI`) is a new sibling to `AttentionManager` in the scene — UI reads `AttentionManager.Instance` state each frame in `Update()`, consistent with "UI holds no game logic."
