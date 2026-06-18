# Mossmark — Prototype 2 Design Draft
> A fresh prototype. Built from scratch, informed by Prototype 1 but not constrained by its architecture.

---

## Premise

Prototype 1 proved out a quest/upgrade loop but felt transactional — menus mediate every meaningful action, and the world itself is inert until you click on a labeled thing. Prototype 2 replaces menu-driven interaction with a single embodied verb: **Attention**.

The player holds a button to attend to whatever they're standing on or near. What happens is a property of the thing being attended to, not a choice presented to the player. The world responds to presence over time; the player doesn't select from the world's menu of offerings.

This reframes the day cycle too: **attention is the clock**. There is no separate "day action pool" sitting above moment-to-moment play — spending attention *is* spending the day. Phase transitions (midday, afternoon, evening) are ambient narration of the same underlying resource, not a second system to track.

### Core loop under test

POIs, generic wilderness spots, tended-style spots, buildings, NPCs, and the Town Hall all have **interconnected dependencies** — what each can become depends on the state of others. The prototype's job is to test whether attending to each of these in different ways, discovering what's blocked and why, and watching the web shift as things develop, produces a satisfying loop on its own — independent of any narrative framing, item-collection mechanics, or completion goal.

---

## Core Verb: Attention

- **Hold-to-attend** is the universal interaction. One button, held on whatever is in range — and *held*, not tapped: for most attendables, the response repeats at a steady rate for as long as the button stays down, not once per press.
- **The hold ends on its own**, when any of the following happens: the player releases the button, the attendable's response can no longer continue (it's run out of whatever it needed — materials, a satisfied dependency), the daylight pool runs out, or the attendable **interrupts** the hold to surface something worth noticing (a rare item, a development crossing a threshold). After any of these, the hold is simply over — a fresh press resumes if the player wants to continue.
- **Outcome type is intrinsic to the target.** The player never picks the outcome type — only where they choose to spend attention, and how long to stay.
- **Some attendables are intrinsically one-shot** — a single discrete action with nothing to "stay in" (marking a tended spot, lighting a signal fire, resting at the bedroll). Whether an attendable is an ongoing hold or a one-shot action is a property of the target, decided the same way its response type is.
- **Attention = daylight = the day's clock.** A pool depletes as attention is spent — each productive tick of an ongoing hold spends one point, the same as a one-shot action does. Crossing thresholds triggers ambient phase messages ("midday now," "the light is going — you should head back"). Direct successor to P1's `DayCycleManager` 7-action pool, but unified with the resource itself rather than counted alongside it.
- **When the pool is empty:** the player can still move freely. Attempting to start an attention that requires daylight shows a message ("too late to start that now") and consumes nothing; an ongoing hold that drains the pool to zero mid-task ends the same way it would on release. The only way to end the day is interacting with a **bedroll**, always present in the settlement, which triggers the day-transition sequence (the fade/reseed pattern from P1 is a useful reference, rebuilt against daylight thresholds rather than action counts).

---

## Attendable Taxonomy

Everything attention can target falls into one of these categories. All share the **generic dependency/response system** described below — the categories differ in *what they yield or require* and in *whether attention to them is an ongoing hold or a one-shot action*, not in the overall resolution process.

- **Generic wilderness spots** *(ongoing)* — field, clay pit, and similar. Each tick of attention yields from a basic item pool with rare-drop chances; holding repeats this for as long as daylight lasts, until released, or until a rare drop interrupts the hold. No state, no wait.
- **Tended-style spots** *(one-shot)* — carried from P1's mark → wait → harvest model. First attention marks the spot ("ready in N days"); a later attention (after the wait) harvests the yield. Marking and harvesting are each a single action, not a hold.
- **Points of Interest (POIs)** *(inherits)* — the 2-3 selected per-session archetypes (Fen Bog, Deep Wood, Old Road, etc.). Mechanically a wilderness spot (ongoing) or tended-style spot (one-shot), but its yield pool is the region's *distinctive* items, and it is the attendable most likely to be **gated** by another entity's development (e.g. Fen Bog Hollow ↔ Bog Tender).
- **Buildings** *(ongoing)* — all dilapidated-with-latent-specialization at generation. Holding attention repeatedly consumes materials and produces development progress, one tick at a time, for as long as materials and daylight hold out — until materials run out (surfacing a "needs" response and ending the hold) or a development threshold is crossed (interrupting the hold). Specialization is fixed at generation (biased by local POIs/materials) but realized only once development progress crosses a threshold.
- **NPCs** *(ongoing)* — all unspecialized at generation. Holding attention either produces development progress directly, tick by tick (if whatever the NPC needs is already satisfied), or surfaces a "needs" response and ends the hold. What an NPC needs may include items, but is not limited to items (see Generic Dependency/Response System).
- **Wandering things** *(one-shot)* — creatures or figures, static-with-lifespan for this prototype (appear, available for a time, then disappear). A single attention triggers either a positive outcome (special drop pool) or a negative outcome (lose all carried items + a creature-specific daylight/time cost, with a flavor message). Odds can be shifted by town development (e.g. a sufficiently advanced Hedge Witch changes a Bog Wraith encounter's odds).
- **Bedroll** *(one-shot)* — always present in the settlement. The only way to end the day. Not otherwise attendable (no yield, no progress).

---

## Generic Dependency / Response System

Every attendable (except the bedroll) resolves attention through the same generic process. For an **ongoing** attendable this process runs once per *tick* of the hold; for a **one-shot** attendable it runs once per press.

1. Check whether this attendable's current dependencies are satisfied.
2. **If satisfied:** attention produces this attendable's normal yield/progress, and spends one daylight point. For an ongoing attendable, the hold continues to another tick unless this tick **interrupts** it (see below).
3. **If not satisfied:** attention surfaces a human-readable "needs" response describing the unmet dependency, consumes no daylight, and — for an ongoing attendable — ends the hold here. This is the "ran out of X mid-task" stop condition (e.g. a building running out of materials while held).

**Interrupts**: independent of the satisfied/not-satisfied check, a tick can end the hold even though step 2 succeeded — because something happened that's worth the player noticing on its own (a rare-drop yield, a development threshold crossed). An interrupted hold isn't blocked; the player can press again immediately. The interrupt's only effect is to *not* auto-continue, so the moment registers rather than disappearing into the next tick.

**Dependencies can reference any entity type and any dependency kind** — not just "needs item X." Examples spanning the web:

- A building needs a material (item-in-hold-or-chest dependency).
- An NPC needs a building revived first (building-state dependency).
- A building needs a curse lifted by an NPC with a specific development (NPC-state dependency).
- An NPC needs a nearby threat (wandering thing) dealt with (world-state dependency).
- An NPC simply needs **more time** — repeated attention with no material cost, until a threshold is crossed.
- A POI is inaccessible until an NPC reaches a specific development (P1's `bog_tender` → Fen Bog Hollow pattern, generalized).

No entity type is restricted from depending on another entity of the same type; the resolver doesn't need to special-case this. If same-type dependency chains produce cycles or other incoherent results in practice, revisit then.

**The overlay** shown when approaching any attendable has two lines: a description/name (current state — dilapidated description pre-revival, proper name post-revival, etc.), and an interaction line — either the action prompt ("Hold E to repair," "Hold E to attend") or the "needs" message if dependencies are unmet.

---

## Outcome Influence

The dependency system above answers whether attention can produce anything at all. A separate question — what attending should actually produce, given that it can — currently gets answered ad hoc per attendable: a Wandering Thing's good/bad odds shift if a specific specialization has been realized, and nothing else in the prototype varies an outcome by world state at all.

Going forward, any ambient or accumulated fact about the world — items carried, another entity's development, time of day, and eventually season, moon phase, or weather if those end up mattering — should be able to bias an outcome's odds or texture, without requiring a new dependency type or a rewrite of the attendable it affects. This is the Civ-2-city-radius idea pulled back a layer: the terrain/state facts already exist (place archetypes, declared/realized specializations, daylight phase), but there's no general path yet from "a fact is true" to "an outcome leans one way" — each existing instance hardcodes that path for itself.

This is a parallel system to the dependency resolver above, not a replacement or extension of it. Dependencies stay binary gates — can this proceed at all. Influence is about degree, and only ever applies once a dependency check has already passed. **What `CanAttend()` blocks today — locked POIs, under-resourced buildings, specialized NPCs going inert — is unchanged by this layer.** Whether any of those hard gates should soften into "always something happens, just weaker" is a separate, deliberately deferred decision (see Open Questions).

---

## Development Application

When attention produces progress (for a building or NPC) and that progress crosses the relevant threshold, an upgrade is **applied automatically** — no player-facing selection menu. For this draft, the specific upgrade is chosen at random from the pool of upgrades whose dependencies are currently satisfied and which are relevant to the local region (POIs present, materials available, etc.).

- **Crossing a threshold interrupts the hold**: the tick that applies a stage is the last tick of that hold, even if the player is still holding the button and the next stage's dependencies happen to already be satisfied. This gives the development a moment of its own (a log line for now, eventually a toast) rather than letting it disappear into the next stage's progress.
- **Building → NPC demand:** a building reaching a development threshold and realizing its latent specialization **declares a "need"** for a corresponding NPC specialization (e.g. a revived smithy creates demand for a smith-track NPC).
- **NPC specialization selection:** when an unspecialized NPC's development crosses its first threshold, it picks its specialization at random from whichever specializations are currently "needed" by buildings in town. If nothing is currently needed, NPCs draw from a small **universal track** (2-3 generic upgrades available to any NPC regardless of town state) so there's no dead period early in a session.
- **"Tending the direction" via material choices** (the player's chosen materials biasing which upgrade gets selected) is explicitly deferred — but the random-selection-from-satisfied-dependencies system should be built so this can be layered in later without restructuring.

---

## World Generation: Points of Interest

At session start, **2-3 POIs are selected** from a pool of 5-6 archetypes (Fen Bog, Deep Wood, Old Road, and similar — drawn from P1's wilderness zones and IDEAS.md's "place archetype" concept). This selection:

- Determines which wilderness zones exist and their item/material biases, including each POI's own distinctive yield pool.
- Biases the latent specializations available to buildings (a Fen Bog POI makes a bog-tending building specialization plausible).
- Biases which NPC specializations can eventually be "needed."

Wilderness and town are drawn from the same act of world-building, made load-bearing from session one rather than deferred to post-prototype, per IDEAS.md's "place archetypes" framing.

---

## Town Generation

A town starts with:

- **Town Hall** — always present.
- **A small number of buildings**, all dilapidated with a latent specialization fixed at generation (biased by local POIs/materials, hinted at via description but not revealed mechanically until revival).
- **A small number of NPCs**, all unspecialized at generation.

No building or NPC is distinguished as "the specialized one" at generation — uniformity here keeps the early game legible (every building is "a thing to work on," every NPC is "someone to get to know") while specialization differentiates through play.

---

## Wilderness

- **POI zones** — see Attendable Taxonomy and World Generation above.
- **Generic wilderness spots and tended-style spots** — randomly placed, independent of the selected POIs, giving the wilderness texture without per-tile authoring. A small number of *ground/feature types* (field, clay pit, etc.), each with its own yield table.
- **Wandering things** — see Attendable Taxonomy above.

---

## Inventory & Storage

- **Carry limit**: 8 distinct stacks (unchanged from P1).
- **Per-stack quantity cap**: 8-10 units per stack (new — prevents degenerate single-item hoarding while carrying).
- **Settlement chest**: unlimited stacks, unlimited quantity per stack. Deposit/withdraw remains **menu-based** — explicitly the one place menus stay, since it's an inventory/storage interface rather than an action-taking interface, and a good target for mood-setting artwork later.
  - Withdrawing more than the player's stack cap from the chest gives the player one full stack at a time; repeated withdrawals are needed for more.
- Rebuild the chest fresh for P2 rather than porting `SettlementChest` — new stack-cap rules and lack of a menu wrapper around the *action* layer make a clean rewrite cheaper than retrofitting.

---

## Day / Daylight Cycle

- Daylight pool depletes as attention is spent; thresholds trigger ambient phase messages, same narrative beats as P1's `DayPhase` progression but driven by the daylight value directly rather than a discrete action counter.
- **At zero daylight**: movement remains free; any daylight-requiring attention shows a "too late to start that" message and consumes nothing.
- **Ending the day**: always via a **bedroll**, present in every settlement. Buildings (including the Mead Hall, if/when it exists) behave as ordinary attendables — no dual-purpose "attend vs. rest" disambiguation needed. A more elegant unification is deferred.
- The fade/transition *pattern* from P1's `DayCycleManager` (black fade → world reset → fade in) is a useful reference for the rebuild, but should be re-triggered by daylight hitting the rest threshold rather than the old action-pool model — rebuild, don't port.

---

## Settlement Horizon (Possibility Space)

The Horizon UI's role shifts from **directive** ("here's what you're one step from unlocking") to **descriptive of possibility space and current demand**:

- Lists buildings/NPCs that have realized a specialization and any "needs" they currently declare.
- Surfaces unmet dependencies in the same human-readable form as the per-attendable overlay — the Horizon is a town-wide view of the same "needs" language, not a separate system.
- Does **not** show a player-selectable upgrade path — development is automatic and randomized within constraints. Legibility comes from understanding *what's currently blocked, on what, and where* — the shape of the web — not *what to click next*.

---

## Core Systems

This section translates the design above into concrete systems and component types, organized for incremental build-out in the new project. Each subsection notes what carries forward from Prototype 1 (by type/file name, for direct reference when setting up the new project) versus what's new or needs a rebuild. Namespaces below are proposals — reconcile with the new project's `CLAUDE.md` once it exists; `Mossmark.Interaction` likely becomes `Mossmark.Attention`, and the upgrade-system namespace generalizes into something like `Mossmark.Development`.

### 1. Input + Movement
Carries over near-verbatim.
- **`PlayerController`** (from `PlayerMovement.cs`) — Rigidbody2D 8-directional movement with smoothing. Drop the `CombatManager`/exhaustion-collapse hooks; Section 6 below replaces that flow with the bedroll/rest sequence only.
- **`CameraFollow`** — unchanged.
- **`TriangleSpriteGenerator`** — unchanged; remains the grey-box visual primitive for procedurally spawned entities.

### 2. Attention Framework
Replaces the Interaction Framework. One verb, hold-to-attend, on whatever is nearest — and the hold can repeat.

- **`IAttendable`** (generalizes `IInteractable`) — every attendable implements `CanAttend()`, `GetOverlayDescription()` (line 1: name / current-state), `GetOverlayInteractionLine()` (line 2: action prompt or "needs" message), `AttentionDuration` (the *tick interval* — time per repetition; a short non-zero value for ongoing attendables, 0 for instant one-shots), `RequiresDaylight` and the new `ContinueAttending` (both read *after* `OnAttentionComplete()`, same timing), and `OnAttentionComplete()` / `OnAttentionCancelled()`. One-shot attendables (Bedroll, Signal Fire, tended spots) simply return `ContinueAttending => false`.
- **`AttendableZone`** (from `InteractionZone`) — trigger collider, unchanged.
- **`AttendableDetector`** (from `InteractionDetector`) — nearest-target tracking ports directly; `CanInteract` is still not used as a pre-filter (a blocked target should still take focus so its "needs" line shows).
- **`AttentionManager`** (simplifies `InteractionManager`) — collapses `Idle → Prompting → InProgress` to `Idle → InRange → Attending`. `Attending` is now a **loop**: each tick runs the existing hold-timer/progress-bar over `AttentionDuration`, then calls `OnAttentionComplete()`. If the attend key is still held, the target is still in range, `ContinueAttending` is true, and (when `RequiresDaylight`) daylight remains after this tick's spend, the loop resets `HoldProgress01` to 0 and starts another tick; otherwise it returns to `InRange`/`Idle` as before. No menu state and no separate focused-interactable indirection.
- **`AttentionInput`** (replaces `InteractionInput`) — unchanged: still just raw `HoldStarted`/`HoldReleased` press/release events. `AttentionManager` tracks "is the key currently held" itself from these events to drive the tick loop. The hold-progress pattern from `StruckNodeInteractable` (`[####....]` fill text) generalizes to every attendable with `AttentionDuration > 0`, refilling once per tick for ongoing attendables.
- **Daylight gating**: whether a tick "counts" (consumes daylight) is decided by the result of `OnAttentionComplete()` — see Section 4. `AttentionManager`/`DayCycleManager` deduct daylight per productive tick, and an ongoing hold that drains the pool to zero ends on that same tick.

### 3. Daylight / Day Cycle
Direct successor to `DayCycleManager`; the action pool becomes a daylight pool.

- **`DayCycleManager`** — `ActionsPerDay`/`ActionsRemaining` (int) become `MaxDaylight`/`DaylightRemaining`. `GetPhase()` thresholds re-tune to the new pool size, but the Dawn→Dusk progression and `DayCycleAmbientTextData` line-lookup port unchanged.
- **Zero-daylight behavior**: when `DaylightRemaining` is 0, a daylight-costing attention's `OnAttentionComplete` is skipped and the overlay shows the "too late to start that now" line — ported from `OnAmbientText`/`NothingLeftToGiveText`. Movement and instant browsing (Horizon, chest) stay free regardless.
- **Rest**: the `Rest()` coroutine and `OnDayTransitionBegin`/`OnDayTransitionEnd` event pair port unchanged; `DayTransitionFadeUI` ports unchanged. Triggered only by the Bedroll (Section 5).

### 4. Generic Dependency / Response Resolver
The single most reusable P1 system. `Entity` + `UpgradeDependency` + `UpgradePool` + `TownEntity` already implement "a thing has a flat pool of candidate developments, each gated by dependency rules, resolved fresh at interaction time." P2 generalizes the *kinds* of dependency and the *kinds* of entity that can hold them.

- **`IDependencyCondition`** (generalizes the body of `UpgradeDependency.IsSatisfied`) — one implementation per dependency kind:
  - `ItemAvailableCondition` — item present in carry or chest, above a quantity threshold
  - `EntityStateCondition` — another `DevelopableEntity` (any type) has reached a given stage — generalizes `Single`/`AnyOf` × `Self`/`Town` from `UpgradeDependency` / `TownEntity.IsUnlockedInTown`
  - `WorldStateCondition` — a flag set elsewhere (a wandering thing resolved, a curse lifted)
  - `TimeCondition` — N attentions of accumulated progress with no other requirement ("needs more time")
- **`DevelopmentStage`** (generalizes `UpgradeDefinition`) — id, display name, description, visual color, `ProgressCost`, `List<IDependencyCondition>`, and what it produces: a yield pool (buildings/POIs) or a specialization unlock (NPCs).
- **`DevelopmentTrack`** (generalizes `UpgradePool`) — flat candidate list per entity type; availability recomputed fresh at attention time, same as `Entity.GetAvailableUpgrades()`.
- **`DevelopableEntity`** (generalizes `Entity`/`TownEntity`) — base class for Building/NPC/POI/Town. Retains `_pendingProgress`, `AddProgress()`, `TryApplyStage()`, `GetAvailableStages()`, `OnDeveloped`. `TownEntity`'s `RegisteredEntities` + town-scope resolution ports unchanged for `Town`-scope conditions.
- **`ResolveAttention()`** — the entry point `IAttendable.OnAttentionComplete` calls for buildings/NPCs/POIs. Implements the 3-step process from the design draft: check dependencies → produce yield/progress (daylight consumed), or surface "needs" text (daylight not consumed).

**Implemented (Iteration 8)**: all four `IDependencyCondition` types, `DevelopmentStage`, `DevelopmentTrack`, `DevelopableEntity` (abstract `MonoBehaviour`, tracks built programmatically in `Awake()` rather than via `[SerializeReference]` — no inspector-authoring path for polymorphic condition lists via MCP-Unity), and the static `WorldState` flag registry backing `WorldStateCondition`. `ResolveAttention()`'s order is: `AddProgress(1)` unconditionally, then `TryApplyStage()` (requires both `PendingProgress >= ProgressCost` and all dependencies satisfied), then `LogDependencyReport()`/`GetNeedsOrDefault()` if the stage didn't apply — this lets `TimeCondition` accumulate progress even while another condition on the same stage remains unsatisfied. `RequiresDaylight => LastAttentionConsumedDaylight` (read after `OnAttentionComplete()`, the pattern `TendedSpotAttendable` established in Iteration 7) gates daylight consumption on whether the stage actually applied. `TownEntity`'s town-scope registry and `DevelopmentStage`'s visual-color/yield-pool fields are deferred to the iterations that need them (#9+).

Three hand-placed test entities exercise the resolver:
- **"Old Cairn"** (`Mossmark.Development.OldCairnAttendable`, grey, world (3, 3)) — one stage "Repair the Cairn" (`progressCost: 1`), gated by `ItemAvailableCondition(sticks, 2)` + `WorldStateCondition("signal_lit", true, "needs the signal fire lit nearby")`.
- **"Signal Fire"** (`Mossmark.World.SignalFireAttendable`, orange, world (3, -3)) — a one-shot world-state toggle (no `DevelopableEntity`); attending it while unlit sets the `signal_lit` flag via `WorldState.SetFlag`.
- **"Watcher's Post"** (`Mossmark.Development.WatchersPostAttendable`, slate blue, world (-3, -3)) — one stage "Take Up the Watch" (`progressCost: 2`), gated by `EntityStateCondition(oldCairn, 0)` + `TimeCondition(2)`.

**Manual playtest sequence** (no automated test framework exists yet — see Section "Implementation Iterations" note): forage the Field repeatedly until carrying 2x Sticks → attend Old Cairn and observe the overlay/log report both "needs 2x Sticks" (now satisfied) and "needs the signal fire lit nearby" (unsatisfied) → attend Signal Fire to light it (`Debug.Log("Signal Fire: lit! ...")`) → re-attend Old Cairn to see both conditions satisfied and `"Old Cairn: developed - Repair the Cairn!"` logged, with daylight consumed → attend Watcher's Post twice (each attend logs `EntityStateCondition`/`TimeCondition` status; `TimeCondition` accumulates progress on each attend regardless of `EntityStateCondition`) and confirm `"Watcher's Post: developed - Take Up the Watch!"` once both `PendingProgress >= 2` and the Old Cairn has developed.

**Deferred to Buildings/NPCs (#9/#10)**: once those `DevelopableEntity` subclasses exist, `ResolveAttention()`'s per-tick signals will need to split into two independent reads — `RequiresDaylight` true whenever this tick's dependencies were satisfied and progress was added (regardless of whether a stage applied), while `ContinueAttending` is true under that same condition *except* when a stage was just applied this tick (a stage-crossing tick is also the hold's last tick, per Development Application's interrupt rule). This likely means `LastAttentionConsumedDaylight`/`LastAttentionMadeProgress`-style tracking in `DevelopableEntity` distinguishes "made progress this tick" from "applied a stage this tick" — currently these are the same flag. `OldCairnAttendable`/`WatchersPostAttendable` keep their Iteration 8 one-shot framing (`ContinueAttending => false`, added in #8.5) since they remain debug-output test entities, not part of the hold-to-build gameplay loop.

**Implemented (Iteration 8.5)**: see Section 2/6 below for the `IAttendable.ContinueAttending` member and `AttentionManager` tick loop, and the retrofitted `GenericWildernessSpotAttendable`.

**Manual playtest sequence (#8.5)**: stand on the Field and hold E — the overlay's `[####....]` bar fills over ~0.5s, a forage log line prints, the bar resets to empty and refills again automatically as long as E stays held, the Field stays in range, and daylight remains; release E at any point and the hold simply stops (a later press resumes it). Hold long enough to drain daylight to 0 and confirm the hold ends on that tick with "too late to start that now" then showing in the overlay. Separately, hold E (don't tap) on the Bramble Patch, Bedroll, Signal Fire, Old Cairn, and Watcher's Post and confirm each still performs exactly one action (mark/harvest, rest, light, develop) and returns to `InRange` rather than looping — `ContinueAttending => false` on all of them.

**Implemented (Iteration 9)**: `ResolveAttention()` now exposes the two independent post-tick flags anticipated above. `LastAttentionMadeProgress` is true whenever this tick's dependencies were satisfied (progress was added toward `ProgressCost`), regardless of whether a stage applied — it drives `RequiresDaylight`. `LastAttentionAppliedStage` is true only on the tick that crosses a stage threshold; combined with `LastAttentionMadeProgress`, it drives `ContinueAttending` on ongoing `DevelopableEntity` subclasses (a stage-crossing tick is also the hold's last, per Development Application's interrupt rule). `OldCairnAttendable`/`WatchersPostAttendable` were updated to read `RequiresDaylight => LastAttentionMadeProgress`; their `ContinueAttending => false` from #8.5 is unchanged since they remain one-shot debug-output entities. See Section 6 for `BuildingAttendable`, the first consumer of both flags together.

### 5. Inventory & Storage
- **`ItemDatabase` / `ItemDefinition`** — port unchanged, plus a new `StackCap` field (8-10) on `ItemDefinition` (or as a global constant).
- **`InventoryManager`** — `CarryLimit` (8 stacks) ports unchanged; `AddItem` additionally caps each stack at `StackCap`. Decide during implementation whether overflow is dropped or refused outright.
- **`SettlementChest`** — rebuilt per the design above (uncapped stacks/quantities; withdraw gives one player-stack-cap's worth at a time). Remains the one menu-based interaction — reuse `InteractionMenuControllerUITK`'s rendering for this single surface, renamed to something like `ChestMenuUI`.

**Implemented (Iteration 15)**: `ChestAttendable` (`Mossmark.Inventory`) is a thin `IAttendable` — `AttentionDuration => 2f`, `RequiresDaylight => false` (opening the chest is a free menu interaction, not an active extraction), `ContinueAttending => false` (opening is a single check-in; the menu itself owns deposit/withdraw). It holds its own `List<InventoryStack>` with no cap on stack count or per-stack quantity — `Deposit()` never refuses, and `Withdraw()` returns the units actually removed (capped at what's stored, deleting an emptied stack). `OnAttentionComplete()` calls `ChestUI.Instance?.Open(this)`.

`ChestUI` (`Mossmark.Inventory`) is the one remaining menu surface, built the same code-first `UIDocument`/runtime-`PanelSettings` way as every other overlay (`sortingOrder = 50`, above the HUD overlays but below `DayTransitionFadeUI`'s 100). It renders two columns, "Pack" and "Chest," each row showing `{ItemDefinition.DisplayName} x{Quantity}` (Pack additionally shows `/{StackCap}`). Rather than UI Toolkit's focus/event system (no `EventSystem`/`InputSystemUIInputModule` exists in the scene), it reads the project-wide "UI" action map's `Navigate`/`Submit`/`Cancel` `InputAction`s directly via `InputSystem.actions.FindActionMap("UI")` — the same raw-action-read convention `AttentionInput` established: `Navigate`'s Vector2 drives row selection (Y, edge-detected at a 0.5 threshold) and column switching (X), `Submit` transfers the selected row, `Cancel` closes the menu.

**Deposit (Pack → Chest) moves a selected stack in full**: `InventoryManager.RemoveItem(stack.Item, stack.Quantity)` (always succeeds — it's the player's own stack) then `ChestAttendable.Deposit(item, removed)` (uncapped, never refuses). **Withdraw (Chest → Pack) moves at most one player-stack-cap's worth**: `Mathf.Min(stack.Quantity, stack.Item.StackCap)` is offered to `InventoryManager.AddItem()`, and only the amount it actually accepted (`added`, which may be less if the pack is full) is removed from the chest via `ChestAttendable.Withdraw(item, added)` — nothing is lost from the chest if the pack can't take it, and a chest stack larger than `StackCap` requires repeated withdrawals to drain, per the design draft.

**Three lock-pattern additions** mirror the existing `DayCycleManager.IsTransitioning` lock, generalized to "`ChestUI.Instance.IsOpen`": `AttentionManager.HandleHoldStarted()` refuses to start a new hold while the chest menu is open; `PlayerController.HandleInput()` zeroes movement and returns early (so the "UI" map's WASD-driven `Navigate` doesn't also move the player via the "Gameplay" map's `Move`); `AttendableOverlayUI.Update()` hides the attendable overlay entirely. All three checks live alongside (immediately after) the corresponding `IsTransitioning` check in each file.

**"Settlement Chest" GameObject** (`TriangleSpriteGenerator` colored warm wood-brown `{0.45,0.32,0.15,1}`, `CircleCollider2D` trigger radius 0.5, `AttendableZone` + `ChestAttendable`) was hand-placed at world (1.5, 3, 0) — immediately east of the Bedroll (0, 3, 0), forming a small "camp" cluster. **"Chest UI"** (`UIDocument` + `ChestUI`) is a new code-first overlay sibling alongside Overlay UI / Inventory UI / Day Cycle UI / Day Transition Fade UI, at world (0, 0, 0).

**Manual playtest sequence (#15)**: forage the Field and Clay Pit until carrying a few distinct item stacks → walk to the Settlement Chest at (1.5, 3) and hold E for 2 seconds; the chest menu opens (movement freezes, the attendable overlay disappears) showing "Pack" (your stacks, with `/StackCap`) and an empty "Chest" column → with "Pack" highlighted, press W/S to move the row selection, press D to switch to "Chest" and A to switch back → with a Pack row selected, press Enter (Submit) to deposit that stack in full into the Chest column → forage more of that same item until the Pack stack exceeds `StackCap` (e.g. >8 Sticks isn't possible directly, so instead deposit a partial stack, then deposit more of the same item to build up a Chest stack larger than 8) → switch to "Chest", select that oversized stack, and press Enter to withdraw — confirm only up to `StackCap` units return to Pack and the Chest stack retains the remainder, requiring a second Enter-press to fully drain it → press Esc (Cancel) to close the menu and confirm movement, the attention hold loop, and the attendable overlay all resume normally.

### 6. Attendable Types
Each implements `IAttendable`; response logic differs per the taxonomy.

- **Generic wilderness spots** *(ongoing)* — repeated instant-feeling yields while held: `AttentionDuration` becomes a short tick interval (tuned during implementation to a randomized 1.5-2s range, rerolled each tick) rather than 0, so each tick's `[####....]` bar fills and resets visibly. Weighted item pool with rare-drop chance, rolled once per tick. `ContinueAttending => true` unless this tick's roll hit the rare drop, which interrupts the hold so the player notices it. Generalizes `ItemPickupInteractable` and the yield-roll logic from `StruckNodeInteractable.CompleteExtraction`.
  - **Implemented (Iteration 8.5)**: `GenericWildernessSpotAttendable` gained a serialized `tickInterval` field (default `0.5f`) backing `AttentionDuration`, replacing the old `0f` instant-complete. `OnAttentionComplete()` now sets `continueAttending = !RollYield()`, where `RollYield()` returns `true` only on a successful (room-to-carry) rare-drop hit — that's the interrupt. The overlay text changed from "Press E to forage" to "Hold E to forage" to match the new hold-based feel. (Superseded — see CLAUDE.md's "pre-Iteration-13 polish pass" entries: `tickInterval` was replaced by a randomized `minTickInterval`/`maxTickInterval` range, default 1.5-2s.)
- **Tended-style spots** *(one-shot)* — mark → wait → harvest. Implemented as `TendedSpotAttendable` (`Mossmark.World`), porting `TendedSpotInteractable`'s `Unmarked`/`Marked`/`Ready` states, `OnDayAdvanced` transition, and `MaxConcurrentMarked` cap (the latter as a static `displayName`-keyed dictionary rather than an SO-keyed one). Marking and harvesting both go through `OnAttentionComplete`, and both return `ContinueAttending => false` — each is a single check-in, not a hold. **Both marking and harvesting cost no daylight** — `AttentionManager.CompleteAttention` reads `RequiresDaylight` *after* `OnAttentionComplete` runs, by which point a successful harvest has already reset state, so a state-dependent answer can't distinguish "just harvested" from "nothing to harvest"; tended spots are framed as low-effort cultivation check-ins rather than active extraction. Test entity: "Bramble Patch" (2 max concurrent, 1 rest to harvest, yields 1-2 Berries).
- **POIs** — mechanically a wilderness spot (ongoing) or tended spot (one-shot) with a distinctive yield pool and an optional `IDependencyCondition` gate. Generalizes `EncounterLocationConfig.UnlockUpgrade` + `EncounterLocation`'s locked-prompt pattern.

  **Implemented (Iteration 13)**: one `PoiAttendable` (`Mossmark.World`, new `IAttendable` - not a `DevelopableEntity`, since a POI has nothing to develop, only to unlock) is spawned per selected archetype at `(-9,3)`/`(-9,0)`/`(-9,-3)` (one column west of the Iteration 12 wilderness spots). Mechanically it's an ongoing wilderness spot: the same randomized `minTickInterval`/`maxTickInterval` hold-and-repeat shape as `GenericWildernessSpotAttendable` - both now share the extracted `ItemYieldRoller.Roll()` helper (`GenericWildernessSpotAttendable` was refactored to call it too, with no behavior change). Each archetype's POI yields its *former rare item as a new common yield*, plus a brand-new POI-exclusive rare item (`poiRareDropChance: 0.05`, slightly rarer than the wilderness 0.08): **Fen Bog Hollow** (dredge, common Crow Feather / rare new "Sunken Bell"), **Old Road Checkpoint** (search the ruins, common Old Coin / rare new "Tally Stick"), **Old Quarry Cut** (dig through the rubble, common Quartz / rare new "Carved Sigil Stone"), **Reed Marsh Blind** (search the blind, common Duck Egg / rare new "Woven Reed Charm"), **Deep Wood Shrine - Inner Grove** (approach the inner grove, common Raven's Eye / rare new "Antler Talisman") - all seven new fields (`poiDisplayName`/`poiLockedDescription`/`poiVerb`/`poiColor`/`poiCommonYields`/`poiRareYield`/`poiRareDropChance`) live on `PlaceArchetype`.

  The gate is the new `SpecializationRealizedCondition(archetype.SpecializationId, "needs a {NpcTitle} in town")`, checking a new static `RealizedSpecializations` registry (`Mossmark.Development`) - the mirror, in the opposite direction, of Iteration 11's `DeclaredSpecializationNeeds`/`SpecializationNeededCondition`. `NpcAttendable.HandleDeveloped` calls `RealizedSpecializations.Declare(stage.Id)` for every specialization stage (universal or archetype-derived alike), so a POI unlocks the moment *any* NPC realizes that archetype's specialization - e.g. once a Wanderer becomes "Wanderer the Bog Keeper" (`bog_tender`), Fen Bog Hollow's gate opens. While locked, `GetOverlayDescription()` shows `poiLockedDescription` (flavor text) and `GetOverlayInteractionLine()` shows the gate's "needs a {NpcTitle} in town" line; `CanAttend()` is `false`, so holding E does nothing. Once unlocked, both lines and the hold behave exactly like a wilderness spot.

  **Manual playtest sequence (#13)**: enter Play and note the 3 selected archetypes from the world-gen log → approach each of the three POIs at `(-9,3)`/`(-9,0)`/`(-9,-3)` and confirm all show their `poiLockedDescription` + "needs a {NpcTitle} in town", with holding E doing nothing → revive the Workshop (as in #9/#12) so it declares one selected archetype's specialization as needed → hold E on the Wanderer for 4 ticks; on tick 4 it should draw that archetype's specialization (as in #12) and `RealizedSpecializations.Declare(...)` fires → re-approach that archetype's POI and confirm it's now unlocked: overlay reads `poiDisplayName` / "Hold E to {poiVerb}", holding E yields the former-rare item each tick (occasionally the new POI-exclusive rare item, interrupting the hold) → confirm the other two POIs remain locked, since their specializations were never realized.

  **Implemented (Iteration 18)**: wilderness spot pool expanded to 7 types — **5 generic** (Field, Clay Pit, Mushroom Patch, Stone Outcrop, Hollow Log) and **2 tended** (Bramble Patch, Bee Skep) — plus random placement for all wilderness objects. New items: Mushrooms, Spore Cluster, Pebbles, Bark Strips, Beeswax, Honeycomb (6 new `ItemDefinition` assets under `Assets/Game/Data/Items/`). All 7 spot types are authored as `WildernessSpotDefinition` assets (`Assets/Game/Data/World/Spots/`), a new `[CreateAssetMenu]` ScriptableObject that bundles all the per-type data for either a generic spot (`kind: Generic`, interaction verb, `commonYields`, `rareYield`, `rareDropChance`) or a tended spot (`kind: Tended`, `tendVerb`, `harvestYield`, `restsToHarvest`, `maxConcurrentMarked`).

  `TendedSpotAttendable` gained a public `Initialize(displayName, tendVerb, harvestYield, restsToHarvest, maxConcurrentMarked)` method, following the inactive-GO pattern already established for `GenericWildernessSpotAttendable`. `WorldGenerator` now places **all** wilderness objects (archetype spots, POIs, and the 10-12 random generic/tended spots) via rejection-sampling: `FindValidPosition()` draws candidate points uniformly from `WorldLayoutGenerator.WildernessBounds` minus `TownBounds`, checks that the candidate is at least `minSeparation` (default 2 units) from every previously recorded position, and falls back to the best-so-far candidate after `maxAttempts = 200` — avoiding infinite loops on dense maps. All placed positions are accumulated in a single `placedPositions` list shared across `SpawnWildernessSpots`, `SpawnPois`, and `SpawnGenericWildernessSpots`, so archetype spots and POIs also participate in the separation constraint with each other and with the random generic spots. Building positions remain fixed (as a `buildingPositions: Vector2[]` array) since buildings are town objects, not wilderness ones. `SpawnGenericWildernessSpots` draws `Random.Range(minSpotCount, maxSpotCount + 1)` spots (default 10-12) from the `spotPool: WildernessSpotDefinition[]` array, calling `SpawnSpotFromDefinition` which branches on `def.kind` to add either `GenericWildernessSpotAttendable` or `TendedSpotAttendable`. The three hand-placed Field, Clay Pit, and Bramble Patch GameObjects were removed from `Overworld.unity` (fileIDs 1600088223-1600088229, 2054293845-2054293851, 854711395-854711401 and their SceneRoots entries); the `wildernessSpotPositions` and `poiPositions` arrays were removed from the WorldGenerator scene YAML and replaced with `spotPool` (7 asset refs), `minSpotCount: 10`, `maxSpotCount: 12`, and `minSeparation: 2`.

  **Spot types** (yield/tint highlights): **Field** (forage; sticks wt 0.6, wild greens wt 0.4; rare old coin 8%; light green); **Clay Pit** (dig; clay wt 0.7, flat stones wt 0.3; rare old coin 8%; ochre); **Mushroom Patch** (gather; mushrooms 1-3; rare spore cluster 6%; warm brown); **Stone Outcrop** (chip; pebbles 2-4; rare flint 8%; grey); **Hollow Log** (search; bark strips 1-3; rare beeswax 8%; dark brown); **Bramble Patch** (tend; harvest 1-2 berries after 1 rest, max 2; plum); **Bee Skep** (tend; harvest 1-2 honeycomb after 2 rests, max 2; golden amber).

  **Manual playtest sequence (#18)**: enter Play and check the console — confirm the world-gen log still lists 3 selected archetypes, then scan the wilderness for 10-12 additional spots beyond the archetype-driven ones and the 3 POIs; all should be at least ~2 units apart and none should be inside town bounds → confirm no two spots overlap visually or have overlapping colliders (the `[...]` hold bar should not appear on two spots simultaneously when standing in the gap between them) → identify each spot type by color and name in the overlay, and confirm the 7 types can appear: hold E on a grey triangle ("Hold E to chip") for pebbles/flint; hold E on a dark brown triangle ("Hold E to search") for bark strips/beeswax; hold E on a warm brown triangle ("Hold E to gather") for mushrooms/spore cluster → find a Bramble Patch and hold E to mark it (as in Iteration 7); find a Bee Skep and mark it; after 2 rests confirm the Bee Skep shows "Hold E to harvest" with 1-2 Honeycomb yielding → re-enter Play several times and confirm the spot positions vary each session while the 2-unit gap constraint holds; confirm the spot type composition also varies (e.g. one session may have 3 Stone Outcrops, another may have 0).

- **Buildings** *(ongoing)* — `DevelopableEntity` subclass. `ResolveAttention()` runs once per tick while held: if deps satisfied, consume configured materials, `AddProgress()`, spend daylight (`RequiresDaylight => true`), and continue (`ContinueAttending => true`) unless this tick crossed a stage threshold (interrupt — `ContinueAttending => false`); if deps are unsatisfied, surface "needs" overlay text, spend no daylight, and end the hold (`ContinueAttending => false`). Crossing a stage threshold declares an NPC-specialization "need" (Section 7).

  **Implemented (Iteration 9)**: `BuildingAttendable` (`Mossmark.Development`) is a generic, reusable `DevelopableEntity` subclass — `dilapidatedName`, `revivedName`, `repairVerb`, `material` (`ItemDefinition`), `materialCostPerTick`, `progressCost`, `tickInterval`, and `revivedTint` are all serialized fields, with a single-stage `DevelopmentTrack` (`new DevelopmentStage("revive", $"Revive the {revivedName}", progressCost, new ItemAvailableCondition(material, materialCostPerTick))`) built in `Awake()` per the no-inspector-polymorphism constraint from Section 4. `DisplayName` switches from `dilapidatedName` to `revivedName` once `CurrentStageIndex >= 0`. Each productive tick (`LastAttentionMadeProgress`) removes `materialCostPerTick` units of `material` via the new `InventoryManager.RemoveItem()` and spends daylight (`RequiresDaylight => LastAttentionMadeProgress`); `ContinueAttending => LastAttentionMadeProgress && !LastAttentionAppliedStage`, so the revival tick interrupts the hold. A blocked tick (`!LastAttentionMadeProgress`) logs the "needs" report via `LogDependencyReport()`, consumes nothing, and ends the hold. On revival, `OnDeveloped` retints the sprite via `revivedTint` (`UpdateVisual()`), following Iteration 7's `SpriteRenderer.color`-tinting precedent. Test entity: **"Tumbledown Smithy" → "Smithy"** (dark brown-grey `{0.35,0.3,0.28,1}`, retints to warm gold `{1,0.85,0.5,1}` on revival, world (-3, 3)) — `material: flat_stones` (obtainable from the Clay Pit), `materialCostPerTick: 1`, `progressCost: 3`, `tickInterval: 0.5`. (Superseded — see CLAUDE.md's "pre-Iteration-13 polish pass" entries: `tickInterval` was replaced by a randomized `minTickInterval`/`maxTickInterval` range, default 2-3s.)

  **Manual playtest sequence (#9)**: forage the Clay Pit (hold E) until carrying at least 3x Flat Stones → hold E on the Tumbledown Smithy and watch the `[####....]` bar fill and reset each ~0.5s tick, with one Flat Stone consumed and one daylight spent per tick; the hold continues automatically across ticks 1 and 2 → on tick 3 (`PendingProgress` reaches `progressCost`), the revival stage applies: `"Tumbledown Smithy: developed - Revive the Smithy!"` logs, the sprite retints to warm gold, the display name switches to "Smithy", and the hold ends (interrupt) even if E is still held and a 4th Flat Stone is available → re-approach and confirm the overlay reads "Smithy" / "The Smithy stands restored." with `CanAttend() => false` → separately, with 0 Flat Stones carried, hold E on the Tumbledown Smithy and confirm `"needs 1x Flat Stones"` is reported, no daylight/material is consumed, and the hold ends after one tick.
- **NPCs** *(ongoing)* — `DevelopableEntity` subclass, unspecialized at generation, same per-tick shape as Buildings. `TimeCondition` ("needs more time") is the default early dependency — ticks toward it are productive (dependencies satisfied by construction, so each tick spends daylight and continues). First threshold crossing triggers the specialization draw (Section 7) and interrupts the hold.

  **Implemented (Iteration 10)**: `NpcAttendable` (`Mossmark.Development`) is the first consumer of the universal specialization track from Section 7. Its `DevelopmentTrack` holds three same-cost stages — `forager`, `caretaker`, `tinkerer` (each `progressCost: 4`, default `tickInterval: 0.5`) — each gated by `TimeCondition(progressCost)`. This required correcting `TimeCondition.IsSatisfied()` back to its Section 4 description ("N attentions of accumulated progress with no other requirement"): it now always returns `true`, rather than the drifted `PendingProgress >= requiredProgress` form, which made every pre-threshold tick non-productive and would have ended the hold on tick 1 (`ContinueAttending` requires `LastAttentionMadeProgress`). With the fix, ticks 1-3 are productive — `RequiresDaylight => LastAttentionMadeProgress` and `ContinueAttending => LastAttentionMadeProgress && !LastAttentionAppliedStage` both true, so the hold continues automatically, spending daylight and logging "progress N." each tick. On tick 4, `PendingProgress` reaches `progressCost` for all three stages at once, so `TryApplyStage()`'s existing random-among-available selection *is* the specialization draw — `OnDeveloped` fires, the NPC's `DisplayName` becomes `"{genericName} the Forager/Caretaker/Tinkerer"`, its sprite retints (green/orchid/gold), and the hold ends (interrupt), even if E is still held. `CanAttend() => CurrentStageIndex < 0` makes a specialized NPC inert — re-approaching shows "{specializedName} has found their place here." Test entity: **"Wanderer"** (cream/tan `{0.85,0.78,0.65,1}`, world (6, 0)) — `genericName: "Wanderer"`, `progressCost: 4`, `tickInterval: 0.5`.

  **Manual playtest sequence (#10)**: approach the Wanderer and confirm the overlay reads "Wanderer" / "Hold E to spend time with Wanderer - they need more time to find their place" → hold E and watch the `[####....]` bar fill and reset each ~0.5s tick, with "progress N." logged and daylight spent on ticks 1-3, the hold continuing automatically across all three → on tick 4, a specialization is drawn at random: `"Wanderer: developed - Take up {Foraging|Caretaking|Tinkering}!"` logs, the sprite retints, the display name switches to "Wanderer the {Forager|Caretaker|Tinkerer}", and the hold ends even though E is still held → re-approach and confirm the overlay reads the specialized name / "has found their place here." with `CanAttend() => false` (holding E does nothing further).
- **Wandering things** *(one-shot)* — new type, not a `DevelopableEntity`. Static-with-lifespan spawn/despawn timer; a single `OnAttentionComplete` (`ContinueAttending => false`) rolls a positive (drop pool) vs. negative (clear carry + daylight/time penalty + flavor text) outcome. Odds adjustable via an `EntityStateCondition`-style lookup against town development.

  **Implemented (Iteration 14)**: `WanderingThingAttendable` (`Mossmark.World`, new `IAttendable` - not a `DevelopableEntity`, since it has nothing to develop, only a single resolution) has `AttentionDuration => 2f`, `RequiresDaylight => true`, `ContinueAttending => false`. A private `Update()` counts down a randomized `lifespanSeconds` and self-despawns via the same path as a resolved attend if it hits zero first - "available for a time, then disappears," no roaming. `OnAttentionComplete()` computes `goodChance = baseGoodChance` (+ `goodChanceBonus` if `favorableSpecializationId` is in `RealizedSpecializations` - reusing Iteration 13's registry as the "town development shifts odds" hook, rather than a new condition type) and rolls against it: a good roll calls `ItemYieldRoller.Roll(displayName, "received", goodYields, null, 0f)` (`goodYields` *is* the special drop pool, no separate rare roll); a bad roll calls the new `InventoryManager.ClearInventory()` (returns total units cleared, mirroring `RemoveItem`'s "units affected" return shape) and, if `badDaylightCost > 0`, an *additional* `DayCycleManager.SpendDaylight(badDaylightCost)` on top of the attend's own daylight spend. Both branches log a flavor line (`goodFlavor`/`badFlavor`) before despawning.

  `WanderingThingSpawner` (`Mossmark.World`) keeps exactly one instance alive at a time via a coroutine: wait `minSpawnDelay`-`maxSpawnDelay` (5-15s) → spawn at a random `spawnPositions` entry with a random `minLifespan`-`maxLifespan` (20-35s) lifespan, using the inactive-GO `Initialize()` pattern (same shape as `WorldGenerator`'s spawn methods) → wait for the spawned instance's `onGone` callback (fired on resolution or lifespan expiry) → repeat. `PickFavorableSpecialization()` runs once in `Start()` (after `WorldGenerator.Awake()`, per its `[DefaultExecutionOrder(-1000)]`), picking one of this session's `SelectedArchetypes` at random as `favorableSpecializationId` and logging which `NpcTitle` will make travelers more trusting once realized - ties the odds shift to *this session's* randomized town development. Test entity: **"Wary Traveler"** (dusty purple `{0.5,0.45,0.55,1}`, spawning along the southern row `(-6,-6)` through `(6,-6)`) - good outcome yields 1-2 Old Coin or 1-3 Berries, bad outcome clears the inventory and costs 1 extra daylight; `baseGoodChance: 0.5`, `goodChanceBonus: 0.3`.

  **Manual playtest sequence (#14)**: enter Play and note the world-gen log's "travelers will be more trusting once this town has a {NpcTitle}" line → within 5-15s, a "Wary Traveler" appears somewhere along y = -6 → approach and hold E for 2 seconds; observe either a good-outcome log (flavor line + an Old Coin/Berries "received" pickup) or a bad-outcome log (flavor line + "everything you were carrying is gone." + an extra daylight spend on top of the attend's own) → confirm it despawns either way, and a new one appears elsewhere after another 5-15s cooldown → separately, let one expire unattended (~20-35s) and confirm "...gives up waiting and slips away." logs, it despawns, and the spawner still proceeds to its next cooldown/spawn → to test the odds shift, develop an NPC into the specialization named in the world-gen log (per #10-#13) and confirm the good-outcome rate across several encounters increases (baseGoodChance 0.5 → 0.8).
- **Bedroll** *(one-shot)* — thin `IAttendable`: `AttentionDuration` ≈ 1s, `ContinueAttending => false`, `OnAttentionComplete` calls `DayCycleManager.Rest()`. Ports `RestInteractable`'s hold-and-lock-during-transition behavior.

### 7. Development Application
- On `DevelopableEntity.TryApplyStage()`, the stage is chosen **automatically** — random among the dependency-satisfied candidates from `GetAvailableStages()`, not player-selected.
- **Building → NPC demand**: a building reaching its specialization threshold writes to a shared "needs" registry (e.g. `TownEntity.DeclaredSpecializationNeeds: HashSet<SpecializationId>`).
- **NPC specialization draw**: an unspecialized NPC's first threshold crossing reads `DeclaredSpecializationNeeds`; if non-empty, draws from it (consuming the need), else draws from a small universal track (2-3 generic specializations, always available).
- Builds entirely on Section 4's resolver — no new dependency machinery, just the new needs-registry condition type plus a "pick one at random" step (`GetAvailableUpgrades()` already exists in P1; only random selection is new — P1 leaves selection to the player).

**Implemented (Iteration 11)**: `DeclaredSpecializationNeeds` (`Mossmark.Development`) is a static registry wrapping a `HashSet<string>` of specialization ids — ids correspond directly to `DevelopmentStage.Id` values on NPC tracks, with `Declare`/`Contains`/`Consume`. The new `SpecializationNeededCondition(specializationId)` is an `IDependencyCondition` that checks `DeclaredSpecializationNeeds.Contains(specializationId)`. `BuildingAttendable` gained a `declaredSpecialization` field (default `"smith"`, empty = no demand); its `OnDeveloped` handler now retints (as before) *and* calls `DeclaredSpecializationNeeds.Declare(declaredSpecialization)` once revived. `NpcAttendable`'s track gained a fourth stage, `"smith"` ("Take up Smithing"), gated by `SpecializationNeededCondition("smith") + TimeCondition(progressCost)` and placed **last** in track order — `GetNextStage()` (used for `LastAttentionMadeProgress` and the overlay's "needs more time" line) still always resolves to `"forager"` pre-specialization, so this gate only affects which stages are *candidates* at the threshold-crossing tick, not ticks 1-3's productivity. `DevelopableEntity.TryApplyStage()` gained a generic priority-filter step: after computing `available` (dependencies satisfied + `PendingProgress >= ProgressCost`), if any available stage's `Id` matches a currently-declared need, `available` narrows to just those before the random pick; whichever stage is ultimately chosen is then passed to `DeclaredSpecializationNeeds.Consume()` (a no-op for ids that were never declared). This is fully generic — any `DevelopableEntity` subclass's stages can participate, not just NPCs. No scene edits were needed: both the Smithy's `declaredSpecialization` and the Wanderer's new `"smith"` stage pick up their C# defaults, per Iteration 8.5's precedent.

**Manual playtest sequence (#11)**: forage the Clay Pit until carrying >=3x Flat Stones → revive the Smithy (hold E for 3 ticks, as in #9) and confirm the extra log line `"Smithy: the town now needs a smith."` alongside the usual `"...developed - Revive the Smithy!"` → approach the Wanderer and hold E for 4 ticks as in #10; on tick 4, confirm the log reads `"Wanderer: developed - Take up Smithing!"` (not Foraging/Caretaking/Tinkering), the sprite retints to a rust/iron color, and the display name becomes "Wanderer the Smith" → separately, repeat with the Wanderer developed *before* the Smithy is revived and confirm the draw is unaffected (still random among forager/caretaker/tinkerer, per #10), demonstrating the universal-track fallback when no need has been declared yet.

### 8. World Generation: Region & Place Archetypes
Generalizes `WorldGenerator` + `TownGenerator` + `BiomeData`, adding the place-archetype layer from IDEAS.md that P1 deferred to post-prototype.

- **`RegionData`** (renamed from `BiomeData`) — defines a pool of 5-6 `PlaceArchetype` entries instead of flat NPC/zone/item pools.
- **`PlaceArchetype`** — bundles a `WildernessZoneConfig` (zone, item biases, tended spots, POI) with the building specialization(s) it biases and the NPC specialization track(s) it makes available.
- **World gen flow**: select 2-3 `PlaceArchetype`s → derive wilderness zones, building latent-specialization pool, and NPC specialization-track pool from that single selection (replacing P1's two independent draws).
- **`WorldGenerator`/`TownGenerator`** — ground-plane creation, container setup (`EnsureContainer`), seeded `Random.InitState`, and the inactive-GO spawn pattern (`SetActive(false)` → add components → `Initialize()` → `SetActive(true)`) all port unchanged.

**Implemented (Iteration 12)**: new `Mossmark.World` types `PlaceArchetype` (ScriptableObject — wilderness-spot data `spotDisplayName`/`spotVerb`/`spotColor`/`commonYields`/`rareYield`/`rareDropChance`, plus specialization data `specializationId`/`stageDisplayName`/`npcTitle`/`npcTint`) and `RegionData` (`regionName`, `archetypePool: PlaceArchetype[]`, `archetypeSelectionCount` default 3). `WorldGenerator` (`[DefaultExecutionOrder(-1000)]`, so its `Awake()` runs before every other scene object's — `NpcAttendable` in particular) selects `archetypeSelectionCount` distinct archetypes from `regionData.ArchetypePool` via seeded `UnityEngine.Random` (serialized `seed`, or `Environment.TickCount` if `0`) in `Awake()`, exposing the result as the static `WorldGenerator.SelectedArchetypes`. `Start()` then spawns one `GenericWildernessSpotAttendable` per selected archetype (via new `Initialize()` overloads on `GenericWildernessSpotAttendable` and `BuildingAttendable`, following the inactive-GO pattern) at positions clear of the Iteration 1-11 layout, plus a single new "Crumbling Shed" → "Workshop" `BuildingAttendable` whose `declaredSpecialization` is drawn from one of the selected archetypes — directly implementing "building specialization bias... derive[s] from that selection." `NpcAttendable.Awake()` appends one additional `DevelopmentTrack` stage per selected archetype with a non-empty `SpecializationId` (same `SpecializationNeededCondition(...) + TimeCondition(progressCost)` shape as Iteration 11's `"smith"` stage, placed after the universal three so ticks 1-3 stay unaffected), implementing "NPC specialization pools... derive from that selection." Five archetypes ship in `DefaultRegion` (`Assets/Game/Data/World/DefaultRegion.asset`, pool of 5, selection count 3): **Fen Bog** (dig → Bog Iron/Crow Feather, → Bog Keeper), **Old Road** (search → Flint/Old Coin, → Herald), **Deep Wood Shrine** (gather → Mistletoe/Raven's Eye, → Hedge Witch), **Reed Marsh** (cut → Reeds/Duck Egg, → Weaver), **Old Quarry** (chip → Flat Stones/Quartz, → Stonemason) — each backed by a new `PlaceArchetype` asset under `Assets/Game/Data/World/Archetypes/` and (where new) `ItemDefinition` assets under `Assets/Game/Data/Items/`. `TownGenerator`/ground-plane/container concepts from the P1 reference are deferred — this prototype has no separate "town" container yet, so the new spots/building spawn directly as scene siblings, same as all prior hand-placed entities.

**Manual playtest sequence (#12)**: enter Play and check the console for `"World generation (seed <N>): Mossmark Reaches selected <A>, <B>, <C>."` (3 of the 5 archetype display names, randomized per session) → confirm three new wilderness spots appear at world (-6,3), (-6,0), (-6,-3), one per selected archetype, each tinted/named/verbed per its `PlaceArchetype` (e.g. holding E on a Fen Bog spot logs "Hold E to dig" and yields Bog Iron, occasionally Crow Feather) → confirm a "Crumbling Shed" appears at world (0,6) and revives into "Workshop" after 2 ticks of `sticks` (gold tint), logging `"Workshop: the town now needs a {specialization}."` where `{specialization}` is one of the 3 selected archetypes' `specializationId` → approach the Wanderer and hold E for 4 ticks; if the Workshop already declared one of the *new* archetype specializations, confirm tick 4 draws that stage (e.g. `"Wanderer: developed - Take Up Bog-Tending!"`, sprite retints, name becomes "Wanderer the Bog Keeper") instead of forager/caretaker/tinkerer/smith → re-enter Play a few times to confirm the selected archetype set, spawned spots, and Wanderer's specialization pool all vary session-to-session while remaining internally consistent with each other.

### 9. UI
- **`AttendableOverlayUI`** (replaces `InteractionPromptUI` + the menu stack) — two-line overlay: description/name + interaction line. Live hold-progress reuses the `[####....]` text-bar pattern from `StruckNodeInteractable.GetInteractionPrompt`.
- **`DayCycleUI`** — phase label + daylight bar (replaces the 7-segment action display); dimmed state at zero daylight ports unchanged.
- **`DayTransitionFadeUI`** — ports unchanged.
- **`InventoryUI`** — ports unchanged (carry-limit display); add per-stack quantity display if `StackCap` is implemented.
- **`SettlementHorizonUI`** — reframed: lists realized specializations and current declared "needs" per entity, using the same "needs" string the overlay shows. Drops the "next upgrade" framing entirely — `GetAvailableStages()` is used only to confirm "ready" status, never rendered as a choice.
- **`ChestMenuUI`** — the one remaining menu surface (Section 5); reuses `InteractionMenuControllerUITK`'s rendering approach for a simple deposit/withdraw row list.

  **Implemented (Iteration 15)**: built as `ChestUI` (`Mossmark.Inventory`) — see Section 5 for the full writeup.

  **Implemented (Iteration 16)**: built as `HorizonUI` (`Mossmark.Development`), toggled by a new `Horizon` action on the `Gameplay` map (plain `Button`, keyboard `Tab`). A code-first `UIDocument`/runtime-`PanelSettings` overlay (`sortingOrder = 50`, same as `ChestUI`) with two sections built via a shared `BuildSection()` helper: **"The Settlement"** lists every `DevelopableEntity` in the scene (`FindObjectsByType<DevelopableEntity>(FindObjectsInactive.Exclude)`) by `DisplayName`, paired with that entity's *current* `IAttendable.GetOverlayInteractionLine()` — the exact same action-prompt / "needs ..." / post-development "realized" string the per-attendable overlay would show if the player walked up to it right now. **"Town Needs"** lists `DeclaredSpecializationNeeds.All` (a new read-only enumeration accessor added to the existing registry), each rendered as `"The town needs a {Humanize(id)}."` (e.g. `"smith"` → `"The town needs a Smith."`). `GetAvailableStages()`/the development track itself is never shown — exactly the "drops the next-upgrade framing entirely" requirement. `Refresh()` re-queries both sections once, on open (a menu snapshot, not a live HUD). Locking reuses `ChestUI`'s 3-checkpoint pattern (`AttentionManager.HandleHoldStarted`, `PlayerController.HandleInput`, `AttendableOverlayUI.Update` each gained a `HorizonUI.Instance.IsOpen` check alongside their existing `ChestUI.Instance.IsOpen` check), plus mutual exclusion in both directions: `HorizonUI.Update()` refuses to open while the chest is open, and `AttentionManager.HandleHoldStarted()`'s existing chest-open check now also covers refusing to open the chest while the Horizon is open.

  **Manual playtest sequence (#16)**: enter Play and press Tab — confirm the Settlement Horizon panel opens, covering the screen, and movement/attention/the attendable overlay all stop responding (holding E or pressing WASD does nothing while it's open) → confirm "The Settlement" lists every hand-placed and world-gen-spawned `DevelopableEntity` (Old Cairn, Watcher's Post, Smithy/Workshop, Wanderer, plus any world-gen buildings) each with the same status line its own overlay would show (e.g. "Old Cairn" / "Old Cairn needs 2x Sticks", or "Wanderer" / "Wanderer has found their place here." once specialized) → confirm "Town Needs" reads "(nothing currently needed)" on a fresh session, then after reviving a building that declares a specialization (e.g. the Smithy → `"smith"`), re-open the panel and confirm "The town needs a Smith." appears, then disappears again once an NPC draws that specialization (consuming the need) → press Tab again to close, confirming control returns to the player → approach the Settlement Chest and open it, then try pressing Tab — confirm the Horizon does not open over the chest menu; close the chest, open the Horizon, and confirm attempting to open the chest (hold E) does nothing while the Horizon is open.

### 10. World Context & Outcome Influence Layer
Translates the Outcome Influence design section above into concrete types, the same way Section 4 translates the Generic Dependency / Response System design section. New — extends the resolver from Section 4 with a parallel system for graduated influence rather than binary gating.

- **`WorldContext`** (`Mossmark.Development`) — a static, read-only facade over state that's currently scattered across separate statics: forwards `DayCycleManager.Instance.CurrentPhase`, `WorldGenerator.SelectedArchetypes`, `DeclaredSpecializationNeeds.Contains()`, `RealizedSpecializations.Contains()`, and `WorldState.GetFlag()` behind one set of read methods (e.g. `CurrentDayPhase`, `IsSpecializationRealized(id)`, `IsSpecializationNeeded(id)`, `GetFlag(id)`). It owns no new state — it's a single place for modifiers to read from instead of each one reaching into five different statics directly, the same role `WorldState` already plays for `WorldStateCondition`. **Season, moon phase, and weather are not implemented and should not be stubbed in now** — `WorldContext`'s entire purpose is to make adding one later a single new read method plus whatever modifier consumes it, not a redesign. Adding unused fields now would be exactly the kind of scope creep the project has been pulling back from elsewhere.
- **`IOutcomeModifier`** — one implementation per influence, mirroring `IDependencyCondition`'s one-implementation-per-condition-kind shape: a single `void Apply(OutcomeRequest request)` method. Each implementation reads whatever `WorldContext` state it cares about internally (the same pattern `WorldStateCondition` already uses for `WorldState`) and adjusts the request in place.
- **`OutcomeRequest`** — a small mutable object passed through a single resolution call. Starts with just `float ChanceMultiplier = 1f`, since that's the only dimension anything currently needs. Add fields (a yield-weight bonus, a quantity bonus, a daylight-cost discount) only when a specific modifier actually needs one — not preemptively.
- **Call sites build a fresh `OutcomeRequest`, run every relevant modifier's `Apply()` over it, then scale whatever probability that roll already uses by the resulting `ChanceMultiplier`.** Two call sites adopt this in this iteration:
  - `WanderingThingAttendable.OnAttentionComplete()` — `goodChance` is currently `baseGoodChance + (goodChanceBonus if favorableSpecializationId is realized)`. This becomes `baseGoodChance * request.ChanceMultiplier`, with the existing bonus reimplemented as `RealizedSpecializationChanceModifier(favorableSpecializationId, multiplier: 1.6f)` — `0.5 * 1.6 = 0.8`, reproducing the current `baseGoodChance: 0.5` / `goodChanceBonus: 0.3` outcome exactly, so the refactor is provably behavior-neutral before anything new is layered on.
  - `GenericWildernessSpotAttendable`'s rare-drop roll (currently a flat `rareDropChance`) gains a second, new modifier: `TwilightChanceModifier`, which multiplies the rare-drop chance up during `DayPhase.Dawn`/`DayPhase.Dusk` (start around `×1.5`, tune by feel during implementation). This is the first outcome that responds to time-of-day, and proves the same modifier-list approach works against a different attendable type and a different meaning of "chance" — a good/bad resolution vs. a rare-item find — without either attendable needing to know the other's modifier exists.
- **No change to `IDependencyCondition`, `CanAttend()`, or any existing gating logic.** This layer only ever runs once an attendable has already decided attention can proceed — it's strictly additive to the resolver, never a path to make something attendable that wasn't before.

**Implemented (Iteration 16.5)**: `OutcomeRequest` (a mutable `float ChanceMultiplier = 1f` object), `IOutcomeModifier` (`void Apply(OutcomeRequest)` interface), and `WorldContext` (static facade forwarding `CurrentDayPhase`, `SelectedArchetypes`, `IsSpecializationNeeded`, `IsSpecializationRealized`, and `GetFlag` from their respective statics — one place for modifiers to read world state rather than each reaching into five separate statics directly). Two `IOutcomeModifier` implementations ship: `RealizedSpecializationChanceModifier(string specializationId, float multiplier)` multiplies `ChanceMultiplier` when the given specialization has been realized (wrapping the logic previously hardcoded in `WanderingThingAttendable`), and `TwilightChanceModifier(float multiplier)` multiplies `ChanceMultiplier` during `DayPhase.Dawn` or `DayPhase.Dusk`. `WanderingThingAttendable.OnAttentionComplete()` now builds an `OutcomeRequest`, applies `RealizedSpecializationChanceModifier(favorableSpecializationId, 1.6f)`, and computes `goodChance = baseGoodChance * request.ChanceMultiplier` — `0.5 * 1.6 = 0.8`, reproducing the prior `baseGoodChance + goodChanceBonus` outcome exactly and confirming the refactor is behavior-neutral. `goodChanceBonus` was removed from `WanderingThingAttendable` and `WanderingThingSpawner` (the scene YAML's `goodChanceBonus: 0.3` entry becomes an orphaned key, silently ignored per the established precedent). `GenericWildernessSpotAttendable.OnAttentionComplete()` builds an `OutcomeRequest`, applies `TwilightChanceModifier(1.5f)`, and passes `rareDropChance * request.ChanceMultiplier` to `ItemYieldRoller.Roll` — the first outcome that responds to time-of-day rather than town development state, proving the same modifier-list approach works against a different attendable type and a different meaning of "chance." All five new types live in `Mossmark.Development` under `Assets/Game/Scripts/Development/`.

**Manual playtest sequence (#16.5)**: develop an NPC into the Wandering Thing spawner's logged "favorable" specialization (as in Iteration 14) and confirm the good-outcome rate still shifts the same way it did before this refactor (≈50% → ≈80%) — a regression check, not new behavior. Separately, hold E on a wilderness spot repeatedly during Dawn or Dusk (watch the `DayCycleUI` phase label) versus during Midday, across enough attempts to notice a difference, and confirm Dawn/Dusk trends toward more frequent rare drops.

---

## Implementation Iterations

Ordered bottom-up: the attention/overlay loop first (proven against a placeholder), then daylight and the day cycle, then the first two attendable types, then the dependency resolver that everything else (buildings, NPCs, POIs, wandering things) builds on, then the continuous-attention framework that generalizes holding into a repeating tick loop (proven against the already-built wilderness/tended spots before buildings need it), then world generation ties it all into randomized sessions, then remaining UI and polish. Each row is sized to be independently testable, similar in granularity to Prototype 1's iteration list.

Iteration 8.5 is inserted between 8 and 9, and Iteration 16.5 between 16 and 17 (rather than renumbering forward each time) so existing "Iteration N" references in CLAUDE.md's history — and in the Open Questions section below — stay accurate. Iteration 17 was added as new content before the Playtest pass, renaming the old Iteration 17 to 18.

| # | Status | Focus | Deliverable | P1 Reference |
|---|--------|-------|-------------|---------------|
| 1 | [x] | Project Bootstrap + Movement | New Unity 6.4 project scaffolded (folders, namespaces, grey-box scene); player moves with camera follow | `PlayerMovement`, `CameraFollow`, `TriangleSpriteGenerator` |
| 2 | [x] | Attention Framework Core | Holding E on a placeholder attendable fires complete/cancel after a hold duration; releasing early cancels | `InteractionZone`, `InteractionDetector`, `StruckNodeInteractable` (hold-tracking) |
| 3 | [x] | Attendable Overlay UI | Approaching the placeholder shows a two-line overlay (name + "Hold E to attend" / live progress bar) | `InteractionPromptUI`, `StruckNodeInteractable` (progress-bar text) |
| 4 | [x] | Generic Wilderness Spots + Inventory | Attending a field/clay pit yields items (with rare-drop chance) into a capped, stack-limited inventory shown in HUD | `ItemDatabase`, `ItemDefinition`, `InventoryManager`, `InventoryUI`, yield-roll from `StruckNodeInteractable` |
| 5 | [x] | Daylight / Day Clock | Attention drains a visible daylight bar; phase-crossing ambient text fires; at zero daylight, attending shows "too late" and consumes nothing | `DayCycleManager` (phase thresholds, ambient text), `DayCycleAmbientTextData` |
| 6 | [x] | Bedroll + Day Transition | Holding E on the Bedroll fades to black, restores daylight, reseeds wilderness spots, fades back in | `RestInteractable`, `DayTransitionFadeUI`, `DayCycleManager.Rest()` |
| 7 | [x] | Tended-Style Spots | Attending an unmarked spot marks it; after rest(s), attending again harvests | `TendedSpotInteractable` (mark/wait/harvest state machine) |
| 8 | [x] | Generic Dependency / Response Resolver | A hand-placed test entity with authored dependency conditions reports satisfied/unsatisfied correctly via debug output | `Entity`, `UpgradeDependency`, `UpgradePool`, `TownEntity` (generalized) |
| 8.5 | [x] | Continuous Attention Framework | Holding E on the Field yields repeatedly at a steady tick rate, spending daylight each tick, until released, out of daylight, or a rare drop interrupts the hold; tended spots/bedroll/signal fire are retrofitted with `ContinueAttending => false` and remain one-shot | New: `IAttendable.ContinueAttending`, `AttentionManager` tick loop |
| 9 | [x] | Buildings: Revival via Attention | A dilapidated building is revived by repeatedly attending with required materials in hand; shows "needs" overlay when blocked, consumes no daylight | Resolver from #8; `UpgradeDefinition.RewardItems` / dependency model |
| 10 | [x] | NPCs: Development + Specialization Draw | An unspecialized NPC develops via repeated attention ("needs more time"); specializes at first threshold from the universal track | Resolver from #8 |
| 11 | [x] | Building → NPC Demand Loop | Reviving a building changes which specialization an unspecialized NPC draws | New `DeclaredSpecializationNeeds` registry on resolver from #8 |
| 12 | [x] | World Gen: Place Archetypes | A session selects 2-3 archetypes; wilderness zones, building specialization bias, and NPC specialization pools all derive from that selection | `WorldGenerator`/`TownGenerator` (ground planes, containers, inactive-GO pattern, seeded random); `BiomeData` → `RegionData` |
| 13 | [x] | POIs | A POI tied to a selected archetype is inaccessible until its gating dependency is satisfied, then attendable with its distinctive yield | `EncounterLocationConfig`/`EncounterLocation` (gated-prompt pattern) |
| 14 | [x] | Wandering Things | A wandering thing spawns, is attendable for a randomized good/bad outcome, and despawns after its lifespan; odds shift with town development | New |
| 15 | [x] | Settlement Chest (Rebuilt) | Player can deposit/withdraw at the chest; withdrawals respect the player's stack cap, one stack per withdrawal | `InteractionMenuControllerUITK` (rendering only) |
| 16 | [x] | Settlement Horizon UI | Horizon panel lists each entity's realized specialization and current "needs," matching the overlay's language | `SettlementHorizonUI` (reframed) |
| 16.5 | [x] | World Context + Outcome Influence Layer | `WorldContext` static facade and a generic `IOutcomeModifier`/`OutcomeRequest` abstraction extracted from the existing ad hoc bonus pattern; Wandering Things' specialization-bonus odds-shift reimplemented as a modifier with no behavior change, plus a new day-phase-based rare-drop bonus on wilderness spots, proving the same modifier list works across attendable types | New |
| 17 | [x] | NPC Post-Specialization Tracks | Two archetype specializations (Hedge Witch, Bog Keeper) gain post-spec development stages gated by item availability; pool-sealing mechanism lets `GetNextStage()` reach those stages after a spec draws; `WoundLoreModifier` (new `DaylightCostMultiplier` dimension on `OutcomeRequest`) eliminates the bad-outcome daylight penalty while Wound Lore is active, proving the full flag-set-by-stage → modifier-reads-flag loop | New |
| 18 | [x] | Wilderness Depth + Random Placement | Wilderness spot pool expanded to 7 types (5 generic + 2 tended); all wilderness objects placed via rejection-sampling; 10-12 random generic/tended spots spawned each session | New |
| 19 | [ ] | Playtest + Tuning Pass | Full day loop playable end-to-end; daylight costs, yield tables, and dependency thresholds tuned for a legible web | — |
| 20 | [ ] | Code Generalization Audit | Audit all systems for hardcoded field duplication that should be data-driven; produce a prioritized list of ScriptableObject-authoring refactors for subsequent iterations; `BuildingAttendable`'s per-stage duplicate fields are the leading example | New |
| 21 | [x] | Building Data Generalization (G1 + G2) | `BuildingAttendable`'s parallel stage field groups collapsed into a `BuildingStageDef[]`; `PlaceArchetype`'s building block likewise; any number of stages without code changes; all 5 archetype assets migrated | New |
| 22 | [x] | NPC Post-Spec + Spot Tick Tuning (G3 + G6) | `NpcPostSpecStageDef[]` on `PlaceArchetype` replaces `BuildPostSpecStages`' hardcoded switch; `WildernessSpotDefinition` and `PlaceArchetype` gain per-type tick interval fields; all archetype assets updated | New |
| 23 | [x] | Wandering Thing Definition (G4 + G5) | Per-creature data extracted to `WanderingThingDefinition` ScriptableObject pool; spawner picks one at random each cycle; hardcoded Herald modifier calls become data-authored `WorldStateOddsModifier[]` on the definition | New |
| 24 | [x] | Code Quality Pass (G7 + G8 + G9) | `WildernessYieldAttendable` base class eliminates `GenericWildernessSpotAttendable`/`PoiAttendable` duplication; `OldCairnAttendable`/`WatchersPostAttendable` replaced by a generic `LandmarkAttendable`; `TendedSpotAttendable` and `WildernessSpotDefinition` gain yield-pool support | New |

---

## Open Questions / Deferred

- **Building development tiers**: is realizing the latent specialization a single threshold, or does a building continue developing through further tiers afterward (each potentially with its own material needs)?
- **Tending direction via materials**: deferred per Development Application above; system should be built to accommodate it later.
- **Wandering-thing persistence**: static-with-lifespan (appear, available for a time, disappear) is acceptable for this draft; roaming/pathing not required.
- **Maintenance-without-materials**: some lesser benefit from attending to a building without the right materials — noted as future, not in scope.
- **NPC "needs more time" vs. "needs diagnosis"**: **resolved (Iteration 10)** — "needs more time" is the normal, non-blocking, productive-tick prompt (`TimeCondition` is always satisfied, so every pre-threshold tick continues the hold and spends daylight), not a "needs" message surfaced when attention fails to make progress. The universal track's three stages all use `TimeCondition` alone, so `NpcAttendable` never surfaces a "needs" line pre-specialization. Iteration 11 deliberately kept this true: its new `"smith"` stage (gated by `SpecializationNeededCondition`) is appended *after* the universal stages, so `GetNextStage()` still resolves to `"forager"` and ticks 1-3 remain non-blocking regardless of declared needs. A genuinely blocking NPC dependency (one that can surface a "needs" line pre-specialization) remains unintroduced — revisit if a future iteration wants NPCs to diagnose a missing item/state before they'll develop at all.
- **Specialized NPC's own further advancement**: **partially resolved (Iteration 17)** — two archetype specializations (Hedge Witch, Bog Keeper) now have post-spec development stages; `CanAttend()` switches from `true` to `CanMakeProgress()` once the specialization draws, and a pool-sealing step in `HandleDeveloped` ensures `GetNextStage()` advances past the unchosen pool stages to the drawn spec's own authored stages. Universal specializations (forager/caretaker/tinkerer/smith) still become inert — post-spec content for those tracks and for the remaining archetypes (Herald, Weaver, Stonemason) remains deferred.
- **Region-level "unlocking" and inter-region travel**: explicitly out of scope for this prototype. The dependency web described here operates *within* a region; whether/how a region's overall development state gates travel to other regions is a later layer that should sit on top of these mechanics without requiring them to change.
- **No completion/win condition**: intentionally absent from this draft and likely absent from the final game — easier to add later than remove.
- **Carryover candidates**: chest and day-transition are rewrites-with-reference, not ports, per discussion. No other P1 systems are assumed to carry over; pull in deliberately if a gap appears.
- **Interrupt legibility**: for now, an interrupt just ends the hold (plus a `Debug.Log` line), consistent with the project's "no toast/notification UI yet" convention — the player notices by the hold stopping and the overlay/console. Continuous holding (#8.5) makes interrupts (rare drops) reachable mid-hold now; revisit whether a brief on-screen message (a "Flame Sword"-style moment, per IDEAS.md) becomes worth the UI investment once playtesting shows how often this comes up.
- **Ongoing-hold tick interval tuning**: **partially resolved (pre-Iteration-13 polish pass)** — `GenericWildernessSpotAttendable` (#8.5) and `BuildingAttendable` (#9), which both defaulted to a fixed `tickInterval = 0.5f`, were retuned to randomized `minTickInterval`/`maxTickInterval` ranges (1.5-2s and 2-3s respectively, rerolled each tick via `RollTickInterval()`). `NpcAttendable` (#10) still defaults to a fixed `tickInterval = 0.5f` and remains an open tuning item for Iteration 17, along with revisiting whether the foraging/building ranges feel right once playtested end-to-end. Separately (also pre-Iteration-13 polish pass): the five one-shot checks — Old Cairn, Signal Fire, Bramble Patch mark/harvest, Watcher's Post — gained a flat 2-second `AttentionDuration` (previously `0f`), and `OldCairnAttendable`/`WatchersPostAttendable`/`BuildingAttendable` gained `CanAttend() => CanMakeProgress()` so a blocked attend no longer enters the `Attending` state at all. See CLAUDE.md for both.
- **Hard `CanAttend()` gates vs. always-something-happens**: flagged in discussion as a tension between the current implementation (POIs, under-resourced buildings, and specialized NPCs all hit a flat `CanAttend() => false` — no hold even starts) and the broader goal of attention almost always producing *some* outcome. Explicitly deferred — this is a content/feel decision, not a code change: softening these gates risks the player burning daylight repeatedly on flavor-only outcomes without a clear signal that nothing is actually changing, which needs to be thought through before touching any code. No action taken in Iteration 16.5.
- **Outcome Influence Layer scope**: `OutcomeRequest` (Section 10) intentionally starts with a single `ChanceMultiplier` field. Additional dimensions are added only when a specific modifier needs one — not speculatively. **Iteration 17** added `DaylightCostMultiplier` (scales the bad-outcome daylight cost, `WoundLoreModifier` sets it to 0). A yield-weight bonus or quantity bonus remain unneeded so far.
- **Season / moon phase / weather as influencers**: named as eventual possibilities but not implemented and not stubbed into `WorldContext`. Adding one later is meant to be a small, additive change — one new read on `WorldContext`, one new `IOutcomeModifier` consuming it — revisit if/when any of these become real mechanics.
- **Code generalization — prefer data-driven, ScriptableObject-authored development paths over hardcoded fields**: as the system matures, hardcoded per-stage field duplication in MonoBehaviours should be replaced with generic data assets. The clearest current example is `BuildingAttendable`: Stage 1 and Stage 2 each have their own separate serialized fields (`revivedName`/`repairVerb`/`material`/`materialCostPerTick`/`progressCost`/`tint` and the `stage2*` mirror set). The right shape is a `List<BuildingStageDefinition>` (a ScriptableObject or plain serializable class) where each entry carries the same fields — `displayName`, `attentionVerb`, `material`, `costPerTick`, `progressCost`, `requiredSpecialization`, `tint` — so that any building can have any number of stages without code changes, and development paths can be altered or extended entirely in data. The same principle applies broadly: NPC post-spec track stages, archetype-specific outcome modifiers, wilderness spot yield tables, and wandering thing outcome pools are all candidates for data-driven extraction rather than hardcoded switch/case blocks or duplicated fields. **A full audit pass was conducted (Iteration 20)** — findings and decisions are documented in the section below.

---

## Code Generalization Audit (Iteration 20)

Full codebase audit of hardcoded duplication and data-authoring gaps. Each candidate is described with the problem it presents and a concrete suggestion. Decisions (approved / deferred / declined) to be recorded here after review.

*Status key: ***approved*** · **approved** · **deferred** · **declined***

---

### G1 — `BuildingAttendable`: duplicate per-stage field groups

**Problem:** Stage 1 and Stage 2 each have their own parallel field groups (`revivedName` / `repairVerb` / `material` / `materialCostPerTick` / `progressCost` / `revivedTint` and the `stage2*` mirror set). A Stage 3 requires another complete duplicate field group and a new `InitializeStage3()` method. `OnAttentionComplete()` switches on `CurrentStageIndex` to pick the right material.

**Suggestion:** Replace both field groups with a `[Serializable] class BuildingStageDef { string displayName; string verb; ItemDefinition material; int costPerTick; int progressCost; string requiredSpecialization; Color tint; }` and a `BuildingStageDef[]` field on `BuildingAttendable`. `Awake()` iterates the array to build the track. `OnAttentionComplete()` indexes into the array by stage. `Initialize()` and `InitializeStage2()` collapse to a single `Initialize(string dilapidatedName, Color dilapidatedColor, BuildingStageDef[])`. Any number of stages, no code changes needed.

**Decision:** **approved**

---

### G2 — `PlaceArchetype` Building section: mirrors G1's duplication in asset fields

**Problem:** `PlaceArchetype` has a `[Header("Building")]` block with two parallel field groups that mirror `BuildingAttendable`'s code duplication (`buildingRepairVerb` / `buildingMaterial` / `buildingProgressCost` / `buildingRevivedTint` for Stage 1 and `buildingStage2DisplayName` / `buildingStage2Verb` / ... for Stage 2, with Stage 2 material sourced indirectly from `PoiCommonYields[0]`). Adding an archetype with three building stages requires touching both the code and every archetype asset.

**Suggestion:** Replace the Building field groups with `BuildingStageDef[] buildingStages` (same type as G1) plus `string buildingDilapidatedName` and `Color buildingDilapidatedColor` (pre-revival identity stays separate). `WorldGenerator.SpawnBuilding()` becomes a pass-through of the stage array. Stage 2 material is authored directly in the asset rather than sourced indirectly via `PoiCommonYields[0]`.

**Note:** Depends on G1. Should be done together.

**Decision:** **approved**

---

### G3 — `NpcAttendable.BuildPostSpecStages`: hardcoded switch per specialization ID

**Problem:** `BuildPostSpecStages(archetype)` is a `switch (specId)` that hardcodes stage IDs, progress costs, item quantities, and which item to require (`CommonYields[0]` vs. `RareYield`) for `hedge_witch`, `bog_keeper`, and `herald`. Adding Weaver or Stonemason post-spec content means editing this switch. `LogPostSpecEffect` has a parallel switch for per-stage flavor text. The two switches must be kept in sync manually.

**Suggestion:** Add a `[Serializable] class NpcPostSpecStageDef { string stageId; string displayName; int progressCost; bool useRareItem; int itemCount; string flavorText; string worldStateFlag; }` and a `NpcPostSpecStageDef[] npcPostSpecStages` field on `PlaceArchetype`. `BuildPostSpecStages` becomes a generic loop over `archetype.NpcPostSpecStages`; `LogPostSpecEffect` reads `flavorText` from the definition. New or modified specialization paths require only asset editing — no code changes.

**Decision:** **approved**

---

### G4 — `WanderingThingSpawner`: all creature data baked into one spawner instance

**Problem:** `WanderingThingSpawner` carries `displayName`, `approachDescription`, `attendVerb`, `color`, `goodYields`, `goodFlavor`, `badFlavor`, `badDaylightCost`, `baseGoodChance` — all per-creature data for exactly one creature type. A second creature variant (a beast, a merchant, a wandering spirit) requires either a second spawner GameObject in the scene or duplicating all these fields on the existing one.

**Suggestion:** Extract per-creature data to a `WanderingThingDefinition` ScriptableObject. The spawner holds a `WanderingThingDefinition[] pool` and picks one at random each spawn cycle. New creatures are new assets, not new scene GameObjects or code changes. Pairs naturally with G5.

**Decision:** **approved**

---

### G5 — `WanderingThingAttendable`: hardcoded outcome modifier list

**Problem:** `OnAttentionComplete()` hardcodes four specific modifier calls by name, including `WorldStateChanceModifier("herald_trail_markers", 1.3f)` and `WorldStateChanceModifier("herald_toll_records", 1.2f)`. As new NPC post-spec stages add WorldState flags that should affect encounter odds, this list grows by hand-editing `WanderingThingAttendable.cs`. The flag strings are also duplicated between `NpcAttendable.HandleDeveloped` (where they're set) and here (where they're read).

**Suggestion:** Add a `[Serializable] class WorldStateOddsModifier { string flagId; float multiplier; }` and a `WorldStateOddsModifier[] additionalModifiers` field on `WanderingThingAttendable` (or on `WanderingThingDefinition` if G4 is adopted). The Herald-specific calls become entries in this array. `WoundLoreModifier` and `RealizedSpecializationChanceModifier` stay hardcoded — they have special logic (a `DaylightCostMultiplier` dimension and a session-specific `favorableSpecializationId`) that can't be collapsed to a simple flag lookup.

**Decision:** **approved** — natural companion to G4

---

### G6 — `WildernessSpotDefinition`: tick interval not configurable per spot type

**Problem:** `WildernessSpotDefinition` has no `minTickInterval` / `maxTickInterval` fields. `WorldGenerator` hardcodes `1.5f, 2f` in every `Initialize()` call for both archetype-driven spots and random generic spots. All spot types share the same foraging rhythm regardless of type — a Hollow Log search can't feel slower and more deliberate than a quick Field forage.

**Suggestion:** Add `minTickInterval` (default `1.5f`) and `maxTickInterval` (default `2f`) to `WildernessSpotDefinition`. Pass them through in `WorldGenerator.SpawnSpotFromDefinition()`. Add the same fields to `PlaceArchetype` for archetype-driven spots, defaulting to the same values so existing assets need no changes unless a designer wants to tune them.

**Decision:** **approved**

---

### G7 — `GenericWildernessSpotAttendable` / `PoiAttendable`: near-duplicate classes

**Problem:** The two classes are almost identical — same fields (`displayName`, `interactionVerb`, `commonYields`, `rareYield`, `rareDropChance`, `minTickInterval`, `maxTickInterval`), same `IAttendable` shape, same `ItemYieldRoller.Roll()` call, same `RollTickInterval()`. The only meaningful difference is `PoiAttendable` adds a `gate` (`IDependencyCondition`) and a `lockedDescription`. Any fix or enhancement to the shared yield/tick logic must currently be made in two places.

**Suggestion:** Extract a shared base class `WildernessYieldAttendable : MonoBehaviour, IAttendable` with all common logic. `PoiAttendable` subclasses it, overriding `CanAttend()`, `GetOverlayDescription()`, and `GetOverlayInteractionLine()` to add the gate. `GenericWildernessSpotAttendable` either subclasses it or is replaced by the base class directly with no gate set.

**Note:** Code-quality / DRY fix rather than a data-authoring change — lower urgency than G1–G5, but the duplication compounds as both classes evolve.

**Decision:** **approved**

---

### G8 — `OldCairnAttendable` / `WatchersPostAttendable`: named one-off debug entities

**Problem:** These are explicit Iteration 8 test entities with hardcoded `DisplayName`, stage IDs, and dependency conditions in `Awake()`. They exist to prove the resolver works, not as authored content. They add two single-purpose `MonoBehaviour` scripts that serve no reusable purpose and will never be spawned by world-gen.

**Option A:** Delete both scripts and their scene GameObjects. The resolver is thoroughly exercised by world-gen-spawned entities; these no longer add coverage.

**Option B:** Replace with a generic `LandmarkAttendable : DevelopableEntity, IAttendable` whose single stage, progress cost, and dependency conditions are all serialized fields — the same pattern `BuildingAttendable` uses, but without material costs. Reusable for future "puzzle landmark" content.

**Decision:** **approved** Option B

---

### G9 — `TendedSpotAttendable`: single harvest yield, no pool support

**Problem:** `harvestYield` is a single `ItemYield` (one item type, random quantity in range). `GenericWildernessSpotAttendable` uses `ItemYield[] commonYields` with a weighted pool. A Bee Skep can only ever yield one item; a tended bed can't yield one of several crops. `WildernessSpotDefinition` has the same limitation.

**Suggestion:** Change `harvestYield` → `harvestYields: ItemYield[]` on both `TendedSpotAttendable` and `WildernessSpotDefinition`, using the same weighted-pick logic already in `ItemYieldRoller`. Single-item cases still work as a length-one array. Existing scene/asset `harvestYield` references need migrating to the new array field.

**Note:** Also a mild design question — tended spots may intentionally always yield the same predictable item as part of their "planning" feel. Decide the design direction before implementing.

**Decision:** **approved**

---

### Noted: `GenericWildernessSpotAttendable` / `PoiAttendable` rare-drop interrupt intentionally removed

**Intentional design decision, not a regression.** Both `GenericWildernessSpotAttendable.OnAttentionComplete()` and `PoiAttendable.OnAttentionComplete()` set `continueAttending = true` unconditionally and ignore the `bool` return value of `ItemYieldRoller.Roll()`. The interrupt-on-rare-drop behavior (`continueAttending = !roll`) was deliberately removed because with `NotificationUI` in place the hold-ending interrupt is no longer needed to surface the moment — the notification banner handles that. The `continueAttending` field is kept as the mechanism for any future interrupt conditions that may be introduced. `ItemYieldRoller.Roll()`'s `bool` return value is likewise kept in case a caller wants to act on it.

---

## Generalization Iterations (21–24)

These four iterations implement all approved generalizations from the Iteration 20 audit, in dependency order. Each is sized to complete in a single session.

---

### Iteration 21 — Building Data Generalization (G1 + G2)

Collapses `BuildingAttendable`'s duplicate per-stage field groups and `PlaceArchetype`'s mirroring asset fields into a shared `BuildingStageDef` serializable type, so any number of building stages can be authored without code changes.

**Deliverables:**

- **`[Serializable] class BuildingStageDef`** (new type, `Mossmark.Development`) with fields: `string displayName`, `string verb`, `ItemDefinition material`, `int costPerTick`, `int progressCost`, `string requiredSpecialization`, `Color tint`. Placed in a new file or alongside `BuildingAttendable`.
- **`BuildingAttendable` refactor**: replace the `revivedName`/`repairVerb`/`material`/`materialCostPerTick`/`progressCost`/`revivedTint` + `stage2*` parallel field groups with `string dilapidatedName`, `Color dilapidatedColor`, and `BuildingStageDef[] stages`. `Awake()` iterates `stages` to build the `DevelopmentTrack` — one `DevelopmentStage` per entry, with `ItemAvailableCondition(stage.material, stage.costPerTick)` + `SpecializationNeededCondition(stage.requiredSpecialization)` (if non-empty) as before. `OnAttentionComplete()` indexes into `stages[CurrentStageIndex + 1]` to pick the correct material for the tick being attended rather than switching on stage. `UpdateVisual()` reads `stages[CurrentStageIndex].tint` (or `dilapidatedColor` when `CurrentStageIndex < 0`). `InitializeStage2()` is removed; `Initialize()` collapses to `Initialize(string dilapidatedName, Color dilapidatedColor, BuildingStageDef[] stages)`.
- **`PlaceArchetype` building block refactor**: replace `buildingRepairVerb`/`buildingMaterial`/`buildingProgressCost`/`buildingMaterialCostPerTick`/`buildingRevivedTint` and all `buildingStage2*` fields with `string buildingDilapidatedName`, `Color buildingDilapidatedColor`, and `BuildingStageDef[] buildingStages`. Stage 2 material is now authored directly in the asset rather than sourced indirectly from `PoiCommonYields[0]`. `WorldGenerator.SpawnBuilding()` passes `archetype.BuildingStages` through to `BuildingAttendable.Initialize()` — becomes a direct pass-through of the stage array.
- **All 5 archetype `.asset` files** migrated: existing Stage 1 + Stage 2 data moved into `buildingStages` array entries. `buildingDilapidatedName`/`buildingDilapidatedColor` carry what was previously the implicit pre-revival state. The Stage 2 material, previously sourced indirectly from `PoiCommonYields[0]` in code, is now explicitly authored on each asset.
- **`Overworld.unity` not touched**: the hand-placed Wanderer / Roughhand / Bedroll / Chest GameObjects have no `BuildingAttendable` components; all buildings are procedurally spawned by `WorldGenerator`.

**Verification:** `recompile_scripts` returns 0 errors/warnings. `get_gameobject` on "World Generator" shows `regionData` still resolves correctly. In Play mode, reviving any archetype building proceeds through Stage 1 and Stage 2 as before, with correct material consumption per tick and correct tint on each stage's completion.

---

### Iteration 22 — NPC Post-Spec + Spot Tick Tuning (G3 + G6)

Makes NPC post-specialization content fully data-authored (eliminating the hardcoded switch) and adds per-spot-type tick interval tuning to wilderness spot definitions and archetypes.

**Deliverables:**

**G3 — NPC post-spec stages data-driven:**

- **`[Serializable] class NpcPostSpecStageDef`** (new type, `Mossmark.Development` or alongside `PlaceArchetype`) with fields: `string stageId`, `string displayName`, `int progressCost`, `bool useRareItem` (false = `CommonYields[0]`, true = `RareYield`), `int itemCount`, `string flavorText`, `string worldStateFlag` (empty = no flag set on apply).
- **`PlaceArchetype` gains `NpcPostSpecStageDef[] npcPostSpecStages`**. Each entry describes one post-spec development stage for the archetype's NPC specialization, in progression order.
- **`NpcAttendable.BuildPostSpecStages(archetype)` becomes a generic loop** over `archetype.NpcPostSpecStages`, constructing each `DevelopmentStage` as: `SpecializationRealizedCondition(archetype.SpecializationId) + ItemAvailableCondition(useRareItem ? archetype.RareYield.Item : archetype.CommonYields[0].Item, entry.itemCount)`. The existing switch on `specId` is deleted.
- **`NpcAttendable.HandleDeveloped` / `LogPostSpecEffect`**: the per-stage flavor log reads `entry.flavorText` from the definition; `WorldState.SetFlag(entry.worldStateFlag, true)` is called when non-empty. The parallel flavor switch is deleted.
- **All 5 archetype `.asset` files** gain `npcPostSpecStages` arrays. Existing Hedge Witch (2 stages: `hedge_witch_wound_lore`, `hedge_witch_ravens_eye`) and Bog Keeper (2 stages: `bog_keeper_drainage`, `bog_keeper_iron_sense`) and Herald (2 stages: `herald_trail_markers`, `herald_toll_records`) data migrates from hardcoded C# into assets. Weaver and Stonemason gain empty arrays (no post-spec content yet) — adding their stages later requires only asset editing.

**G6 — Per-spot-type tick intervals:**

- **`WildernessSpotDefinition` gains `float minTickInterval` (default `1.5f`) and `float maxTickInterval` (default `2f`)**. `WorldGenerator.SpawnSpotFromDefinition()` passes these through to `GenericWildernessSpotAttendable.Initialize()` or `TendedSpotAttendable.Initialize()` (tended spots don't use tick intervals, so they're ignored for `kind == Tended`).
- **`PlaceArchetype` gains `float archetypeSpotMinTickInterval` (default `1.5f`) and `float archetypeSpotMaxTickInterval` (default `2f`)** — used for archetype-driven spots spawned by `WorldGenerator.SpawnWildernessSpot()`. All 5 archetype assets pick up the defaults with no explicit edit needed unless a designer wants to tune them.
- Existing hardcoded `1.5f, 2f` calls in `WorldGenerator` are replaced with the new fields.

**Implemented (Iteration 22):**

**G3 — NPC post-spec stages data-driven:** `NpcPostSpecStageDef` (`Mossmark.Development`, new `[Serializable]` class) holds `stageId`, `displayName`, `progressCost`, `useRareItem` (false = `archetype.CommonYields[0].Item`, true = `archetype.RareYield.Item`), `itemCount`, `flavorText` (logged as `"{specializedName}: {flavorText}"` — NPC name not included in the asset string), and `worldStateFlag` (empty = no flag set). `PlaceArchetype` gained `NpcPostSpecStageDef[] npcPostSpecStages` (new `[Header("NPC Post-Spec Stages")]` block, default empty array). `NpcAttendable.BuildPostSpecStages(archetype)` is now a generic loop over `archetype.NpcPostSpecStages`, constructing each `DevelopmentStage` with `SpecializationRealizedCondition + ItemAvailableCondition(useRareItem ? rareYield : commonYields[0], itemCount)` — the hardcoded per-archetype `switch` is gone. `NpcAttendable` builds a `Dictionary<string, NpcPostSpecStageDef> postSpecStageDefs` in `Awake()` (one entry per post-spec stage across all selected archetypes). `HandleDeveloped`'s post-spec branch now looks up `stage.Id` in `postSpecStageDefs`: if found, sets the flag (when non-empty) and logs `$"{specializedName}: {def.flavorText}"`; falls back to the old `WorldState.SetFlag(stage.Id) + stage.DisplayName` log if not found. `LogPostSpecEffect` was deleted. All 5 archetype assets updated: Bog (2 stages: `bog_keeper_drainage` progressCost 3 / `bog_keeper_iron_sense` progressCost 4), Sacred Grove (2 stages: `hedge_witch_wound_lore` progressCost 3 / `hedge_witch_ravens_eye` progressCost 4), Old Road (2 stages: `herald_trail_markers` progressCost 3 / `herald_toll_records` progressCost 4), Quarry and Reed Marsh (`npcPostSpecStages: []` — no stages yet, adding content requires only editing the asset). `WoundLoreModifier` and the Herald `WorldStateChanceModifier` calls in `WanderingThingAttendable` remain hardcoded — they have special logic that can't be collapsed to a flag lookup, and moving them to data is G5 (Iteration 23).

**G6 — Per-spot-type tick intervals:** `WildernessSpotDefinition` gained `float minTickInterval` (default `1.5f`) and `float maxTickInterval` (default `2f`) — existing spot assets pick up the defaults with no explicit edit. `PlaceArchetype` gained `float archetypeSpotMinTickInterval` (default `1.5f`) and `float archetypeSpotMaxTickInterval` (default `2f`) under the `[Header("Wilderness Spot")]` block. All three hardcoded `1.5f, 2f` calls in `WorldGenerator` are replaced: `SpawnWildernessSpot` and `SpawnPoi` now pass `archetype.ArchetypeSpotMinTickInterval / MaxTickInterval`; `SpawnSpotFromDefinition` (generic kind) passes `def.minTickInterval / maxTickInterval`. Building tick intervals (`2f, 3f` in `SpawnBuilding`) are unchanged — those are `BuildingAttendable`'s own tuning, not archetype spot tuning.

**Manual playtest sequence (#22):** verify that post-spec stages for Bog Keeper / Hedge Witch / Herald function identically to Iteration 17 (same item requirements, same WorldState flags set, same flavor log lines) — confirming the data migration is behavior-neutral. To test G6: author a `minTickInterval: 3f` / `maxTickInterval: 4f` on any spot definition or archetype asset, enter Play, and confirm spots of that type feel noticeably slower than the default 1.5-2s rhythm. Restore defaults after testing.

---

### Iteration 23 — Wandering Thing Definition (G4 + G5)

Extracts all per-creature data from the single `WanderingThingSpawner` instance into a `WanderingThingDefinition` ScriptableObject pool, and moves the hardcoded outcome modifier list onto the definition.

**Deliverables:**

- **`[Serializable] class WorldStateOddsModifier`** (new type, `Mossmark.World` or alongside `WanderingThingDefinition`) with fields: `string flagId`, `float multiplier`. Used for simple flag-keyed `goodChance` multipliers that don't need special outcome logic.
- **`WanderingThingDefinition` ScriptableObject** (`[CreateAssetMenu]`, `Mossmark.World`, `Assets/Game/Data/World/WanderingThings/`) with all per-creature fields moved from `WanderingThingSpawner`: `string displayName`, `string approachDescription`, `string attendVerb`, `Color color`, `ItemYield[] goodYields`, `string goodFlavor`, `string badFlavor`, `int badDaylightCost`, `float baseGoodChance`. Gains new field: `WorldStateOddsModifier[] additionalModifiers` (G5) — the Herald-specific `WorldStateChanceModifier("herald_trail_markers", 1.3f)` and `WorldStateChanceModifier("herald_toll_records", 1.2f)` calls become entries in this array.
- **`WanderingThingSpawner` refactor**: replaces all per-creature serialized fields with `WanderingThingDefinition[] pool`. `Spawn()` picks `pool[Random.Range(0, pool.Length)]` and passes the definition to `WanderingThingAttendable.Initialize()`. `PickFavorableSpecialization()` logic is unchanged (still reads `WorldGenerator.SelectedArchetypes` in `Start()`).
- **`WanderingThingAttendable.Initialize()` gains a `WanderingThingDefinition def` parameter** (or individual fields from it). `OnAttentionComplete()` loops `def.additionalModifiers`, calling `new WorldStateChanceModifier(m.flagId, m.multiplier).Apply(request)` for each entry — replacing the two hardcoded Herald calls. `WoundLoreModifier` and `RealizedSpecializationChanceModifier` remain hardcoded as noted in the audit decision (their logic can't be collapsed to a flag lookup).
- **`traveler.asset`** (`WanderingThingDefinition` under `Assets/Game/Data/World/WanderingThings/`) authored with current `WanderingThingSpawner` field values. The two Herald entries go into `additionalModifiers`. Referenced by `WanderingThingSpawner`'s new `pool` field (wired via the hand-edit-YAML pattern for `UnityEngine.Object` references).
- **`Overworld.unity`**: "Wandering Thing Spawner" YAML updated to replace per-creature fields with `pool` reference.

**Implemented (Iteration 23):**

**G4 — Per-creature data extracted to asset pool:** `WorldStateOddsModifier` (new `[Serializable]` class, `Mossmark.World`) holds `string flagId` and `float multiplier` — a simple flag-keyed goodChance multiplier that can be authored in data rather than hardcoded. `WanderingThingDefinition` (new `[CreateAssetMenu]` ScriptableObject, `Mossmark.World`, `Assets/Game/Data/World/WanderingThings/`) holds all per-creature fields previously on `WanderingThingSpawner`: `displayName`, `approachDescription`, `attendVerb`, `color`, `colliderRadius`, `goodYields`, `goodFlavor`, `badFlavor`, `badDaylightCost`, `baseGoodChance`. `WanderingThingSpawner` replaces all those per-creature serialized fields with a single `WanderingThingDefinition[] pool`; `Spawn()` picks `pool[Random.Range(0, pool.Length)]` and uses it to set up the spawned `TriangleSpriteGenerator`, `CircleCollider2D`, and `WanderingThingAttendable`. `PickFavorableSpecialization()` is unchanged — still reads `WorldGenerator.SelectedArchetypes` in `Start()` and passes the result as a separate `favorableSpecializationId` parameter to `Initialize()`, since it's session-specific rather than per-creature. `WanderingThingAttendable.Initialize()` now accepts `(WanderingThingDefinition def, string favorableSpecializationId, float lifespanSeconds, Action onGone)` — all individual per-creature parameter fields removed; the component stores `def` as a private non-serialized field set before `SetActive(true)` fires `Awake()`.

**(G5) — Hardcoded Herald modifiers become data:** `WanderingThingDefinition` gained `WorldStateOddsModifier[] additionalModifiers` — the two hardcoded `new WorldStateChanceModifier("herald_trail_markers", 1.3f)` and `new WorldStateChanceModifier("herald_toll_records", 1.2f)` calls in `WanderingThingAttendable.OnAttentionComplete()` were replaced by a `foreach` loop over `def.additionalModifiers`, calling `new WorldStateChanceModifier(m.flagId, m.multiplier).Apply(request)` per entry. `WoundLoreModifier` and `RealizedSpecializationChanceModifier` remain hardcoded — `WoundLoreModifier` sets `DaylightCostMultiplier` (a dimension `WorldStateOddsModifier` can't express) and `RealizedSpecializationChanceModifier` uses the session-specific `favorableSpecializationId` not stored on the definition.

**`traveler.asset`** (`Assets/Game/Data/World/WanderingThings/traveler.asset`, GUID `140d91ba06944db41b4b054cd852d69a`) authored with all prior `WanderingThingSpawner` field values: dusty purple `{0.5, 0.45, 0.55, 1}`, `colliderRadius: 0.5`, `goodYields` (Old Coin + Berries), `baseGoodChance: 0.5`, `badDaylightCost: 1`, and `additionalModifiers` with the two Herald entries (`herald_trail_markers × 1.3`, `herald_toll_records × 1.2`). Referenced in the scene's "Wandering Thing Spawner" YAML via `pool` array. Verified post-load via `get_gameobject`: `pool` resolves to `"traveler"`, all spawner timing fields (`minLifespan: 20`, `maxLifespan: 35`, `minSpawnDelay: 5`, `maxSpawnDelay: 15`, `minDistFromTown: 5`) confirmed correct, no console errors.

**Manual playtest sequence (#23):** enter Play and confirm a Wary Traveler appears after 5-15s with the same dusty purple color, approach description, and 2-second hold as before — behavior-neutral confirmation. Verify a good outcome still yields Old Coin or Berries with a flavor line; a bad outcome still clears the inventory and costs 1 extra daylight. Develop a Herald NPC through both post-spec stages (3x Flint, then 2x Old Coin) and confirm good-outcome frequency shifts upward as before — now driven by `def.additionalModifiers` rather than hardcoded calls. To test the pool mechanism: duplicate `traveler.asset` as a second definition with a different color and name, add it to the spawner's `pool` in the Inspector (or scene YAML), and confirm both creature types appear across multiple spawn cycles.

---

### Iteration 24 — Code Quality Pass (G7 + G8 + G9)

Three self-contained code-quality changes with no asset-authoring dependencies on each other. G7 and G8 are code-only; G9 requires migrating two spot `.asset` files.

**Deliverables:**

**G7 — Shared wilderness yield base class:**

- **`WildernessYieldAttendable : MonoBehaviour, IAttendable`** (new abstract class, `Mossmark.World`) extracts all fields and methods shared by `GenericWildernessSpotAttendable` and `PoiAttendable`: `displayName`, `interactionVerb`, `commonYields`, `rareYield`, `rareDropChance`, `minTickInterval`/`maxTickInterval`, `currentTickInterval`, `continueAttending`; `RollTickInterval()`, `AttentionDuration`, `ContinueAttending`, `RequiresDaylight`, `OnAttentionCancelled()`, and the core `OnAttentionComplete()` body (roll yield via `ItemYieldRoller`, reroll tick interval, set `continueAttending = true`). `GetOverlayDescription()` and `GetOverlayInteractionLine()` remain abstract (both subclasses implement them differently).
- **`GenericWildernessSpotAttendable : WildernessYieldAttendable`** becomes a thin subclass that only implements `CanAttend() => true`, `GetOverlayDescription()`, and `GetOverlayInteractionLine()`. `Initialize()` delegates to the base class fields.
- **`PoiAttendable : WildernessYieldAttendable`** likewise: adds only `gate` / `lockedDescription`, `CanAttend()`, and the gated overlay methods. No duplication of yield/tick logic remains.

**G8 — Generic `LandmarkAttendable` replaces debug entities:**

- **`LandmarkAttendable : DevelopableEntity, IAttendable`** (new class, `Mossmark.Development`) with serialized fields: `string displayName`, `Color entityColor`, `string attendVerb`, `int progressCost`, `float attendDuration`. Dependencies are authored via `[SerializeReference] IDependencyCondition[]` — polymorphic, set via the hand-edit-YAML pattern since MCP-Unity can't assign them through the inspector. A single `DevelopmentStage` is built in `Awake()` from `progressCost` + the serialized condition array. `RequiresDaylight => LastAttentionMadeProgress`, `ContinueAttending => false` (one-shot), `CanAttend() => CanMakeProgress()`. Overlay interaction line: `GetNeedsOrDefault($"Hold E to {attendVerb}")`.
- **`OldCairnAttendable.cs` and `WatchersPostAttendable.cs` deleted**. Their two GameObjects in `Overworld.unity` are updated to use `LandmarkAttendable` instead, with the same dependency conditions re-authored in YAML (`ItemAvailableCondition`, `WorldStateCondition`, `EntityStateCondition` already support `[SerializeReference]` serialization as plain C# classes).
- **`SignalFireAttendable`** is not a `DevelopableEntity` and has no equivalent in `LandmarkAttendable`'s shape — it's a one-shot flag-setter with unique behavior. Retained as-is; the audit's Option B scope was limited to the two cairn/post entities.

**G9 — Tended spot yield pool:**

- **`TendedSpotAttendable.harvestYield: ItemYield`** → **`harvestYields: ItemYield[]`**. `Harvest()` uses the same weighted-pick logic as `ItemYieldRoller` (inline, since tended spots have no rare-drop roll — single pool, no interrupt). `Initialize()` signature updated accordingly.
- **`WildernessSpotDefinition.harvestYield: ItemYield`** → **`harvestYields: ItemYield[]`**. `WorldGenerator.SpawnSpotFromDefinition()` passes the array through.
- **`spot_bramble_patch.asset` and `spot_bee_skep.asset`** migrated: `harvestYield` replaced by a length-one `harvestYields` array entry (same item, same quantity range). Functionally identical — single-item tended spots still work as a length-one array. No behavior change for existing spots.

**Verification:** `recompile_scripts` returns 0 errors/warnings. `get_gameobject` on "Old Cairn" and "Watcher's Post" confirms `LandmarkAttendable` is the active component (not the deleted scripts). In Play mode, both landmark entities behave identically to before. Tended-spot harvests (Bramble Patch, Bee Skep) yield the same item as before.
