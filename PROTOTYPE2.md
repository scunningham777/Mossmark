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

This is a parallel system to the dependency resolver above, not a replacement or extension of it. Dependencies stay binary gates — can this proceed at all. Influence is about degree, and only ever applies once a dependency check has already passed. **What `CanAttend()` blocks today — locked POIs, under-resourced buildings, specialized NPCs going inert — is unchanged by this layer.** Whether any of those hard gates should soften into "always something happens, just weaker" was a deliberately deferred decision (see Open Questions) — **resolved in Iteration 28.5**: fully-developed NPCs and buildings are now always attendable via a "visit/linger" mechanic (see below and CLAUDE.md). Locked POIs and under-resourced buildings remain hard-gated.

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
| 19 | [x] | Playtest + Tuning Pass | Full day loop playable end-to-end; daylight costs, yield tables, and dependency thresholds tuned for a legible web | — |
| 20 | [x] | Code Generalization Audit | Audit all systems for hardcoded field duplication that should be data-driven; produce a prioritized list of ScriptableObject-authoring refactors for subsequent iterations; `BuildingAttendable`'s per-stage duplicate fields are the leading example | New |
| 21 | [x] | Building Data Generalization (G1 + G2) | `BuildingAttendable`'s parallel stage field groups collapsed into a `BuildingStageDef[]`; `PlaceArchetype`'s building block likewise; any number of stages without code changes; all 5 archetype assets migrated | New |
| 22 | [x] | NPC Post-Spec + Spot Tick Tuning (G3 + G6) | `NpcPostSpecStageDef[]` on `PlaceArchetype` replaces `BuildPostSpecStages`' hardcoded switch; `WildernessSpotDefinition` and `PlaceArchetype` gain per-type tick interval fields; all archetype assets updated | New |
| 23 | [x] | Wandering Thing Definition (G4 + G5) | Per-creature data extracted to `WanderingThingDefinition` ScriptableObject pool; spawner picks one at random each cycle; hardcoded Herald modifier calls become data-authored `WorldStateOddsModifier[]` on the definition | New |
| 24 | [x] | Code Quality Pass (G7 + G8 + G9) | `WildernessYieldAttendable` base class eliminates `GenericWildernessSpotAttendable`/`PoiAttendable` duplication; `OldCairnAttendable`/`WatchersPostAttendable` replaced by a generic `LandmarkAttendable`; `TendedSpotAttendable` and `WildernessSpotDefinition` gain yield-pool support | New |
| 25 | [x] | CSV / ScriptableObject Data Pipeline | All `WildernessSpotDefinition`, `PlaceArchetype` building/NPC stages, and `WanderingThingDefinition` assets exported to canonical CSVs; C# Editor importer regenerates all SOs from CSV on demand; Google Sheets serves as the editing surface going forward | P1 data tooling |
| 26 | [x] | Playtest + Numeric Tuning Pass | Full day loop played end-to-end; daylight costs, yield weights, tick intervals, dependency thresholds, and rest counts all tuned until the loop feels legible and unhurried; all tuning expressed in CSV data, not code | — |
| 27 | [x] | Spot Tendedness — Landscape Reacts to Attention | Each wilderness spot accumulates a `tendedness` float that drifts toward wild when left alone and shifts when attended; yield pool composition, rare-drop chance, and tick interval all respond to tendedness; heavy extraction degrades a spot; patient return visits improve it; no UI exposes this directly | New |
| 28 | [x] | Knowledge-Gated Yield Layers | NPC WorldState flags silently unlock additional yield possibilities on wilderness spots — a Bog Keeper with `bog_keeper_iron_sense` adds a weighted bog iron entry to fen-adjacent spots' common pools; a Hedge Witch with `hedge_witch_ravens_eye` changes what the Deep Wood Shrine's inner grove can produce; player discovers this by returning to known spots after NPC development, not by being told | New |
| 28.5 | [x] | Always-Something-Happens: Visits and Linger | Fully-developed NPCs and fully-restored buildings remain attendable via a lighter interaction mode. Archetype-spec NPCs (Bog Keeper, Herald, etc.) have a 20% chance to gift a thematic item (mushrooms, beeswax, honeycomb, etc. as common; POI-exclusive rarities as the rare option); universal-spec NPCs give flavor-only visits. Buildings deliver a free ambient flavor line at no daylight cost. Exchange flavor text is always paired to the actual item gifted — no mismatch possible | New |
| 29 | [ ] | Settlement Maintenance + Ongoing Demand | Restored buildings and specialized NPCs accumulate a `driftProgress` counter each rest; crossing a per-entity threshold drops one effective capability stage (not specialization identity — the building doesn't become dilapidated again, but its output or NPC productivity visibly dims); attending with appropriate common materials resets drift; overlay shows a distinct low-maintenance state string before and after threshold ("the hearth is cooling" / "the hearth has gone cold") | New |
| 30 | [ ] | Settlement Growth — New Arrivals | Two or three WorldState flag combinations, checkable via `WorldContext`, trigger a new-arrival event: a Wandering Thing variant that doesn't despawn but instead becomes a persistent NPC attendable; growth isn't announced, it's noticed; each trigger combination is authored in data and checked at rest-transition | New |

---

### Iteration 25 — CSV / ScriptableObject Data Pipeline

The generalization passes (Iterations 20-24) brought all development content into ScriptableObject assets. The next bottleneck is bulk authoring and comparison: tuning 7 spot definitions side-by-side, comparing NPC post-spec stage costs across archetypes, or adding a new wandering thing creature currently means clicking through individual Inspector panels. A CSV pipeline eliminates that friction and restores the Google Sheets editing surface from P1.

**Deliverables:**

**Export script (`Tools/ExportGameData.py`):**
- One CSV per asset type: `wilderness_spots.csv`, `place_archetypes.csv`, `wandering_things.csv`
- Each row is one asset; columns are the asset's editable fields (not Unity internals, not GUIDs — display names and plain values only)
- `ItemDefinition` references are stored as item display names (a stable lookup key); Unity GUIDs are resolved at import time, not stored in the CSV
- ItemYield arrays (commonYields, harvestYields, goodYields) use a compact inline format: `"BogIron:0.6:1:2,CrowFeather:0.4:1:1"` (name:weight:minQty:maxQty) — human-readable, parseable without regex
- `NpcPostSpecStageDef[]` arrays are flattened into numbered column groups: `stage1_id`, `stage1_displayName`, `stage1_progressCost`, `stage1_useRareItem`, `stage1_itemCount`, `stage1_flavorText`, `stage1_worldStateFlag`, `stage2_*`, etc. — enough columns for the maximum number of stages any archetype currently has

**Import script (`Tools/ImportGameData.py`):**
- Reads each CSV and regenerates all SOs of that type under their existing `Assets/Game/Data/` paths
- Preserves GUIDs by reading existing `.meta` files before overwriting — Unity scene/prefab references stay intact
- Validates: item names resolve to known `ItemDefinition` assets; required fields non-empty; numeric ranges sane (weights sum to >0, quantities min ≤ max); duplicate asset names flagged as errors
- Dry-run mode prints what would change without writing files
- Reports unchanged assets as skipped (not regenerated) so Unity's asset import doesn't re-process the whole project on every run

**Workflow:**
1. `python Tools/ExportGameData.py` → produces CSVs under `Tools/Data/`
2. Upload to Google Sheets (or edit locally); make changes
3. Download updated CSVs back to `Tools/Data/`
4. `python Tools/ImportGameData.py` → regenerates changed SOs
5. Unity auto-imports changed `.asset` files; play and verify

**What stays code-first:** `DevelopmentStage` construction in `NpcAttendable.Awake()` and `BuildingAttendable.Awake()` — these are structural (which conditions get wired to which stages) rather than numeric. The CSV pipeline handles the *values inside* the stage definitions, not the stage-wiring logic itself. This boundary keeps the importer simple and the code readable.

**Verification:** export → edit one field in a CSV → import → enter Play → confirm the changed field is reflected in-game behavior (e.g. change a spot's `rareDropChance` in the CSV, import, confirm the rate is visibly different in the playtest sequence for that spot type).

**Implemented (Iteration 25):**
- **`Assets/Editor/ExportGameData.cs`** — Unity Editor script (`[MenuItem("Mossmark/Data/Export All")]`) that uses `AssetDatabase.FindAssets`/`LoadAssetAtPath` to load typed C# objects directly, then writes three CSVs to `Tools/Data/`: `wilderness_spots.csv`, `place_archetypes.csv`, `wandering_things.csv`. No external dependencies or YAML parsing required — Unity handles all serialization.
- **`Assets/Editor/ImportGameData.cs`** — Unity Editor script (`[MenuItem("Mossmark/Data/Import All")]`) that reads CSVs and applies values back to assets via `SerializedObject`/`SerializedProperty`. `ApplyModifiedProperties()` detects changes and marks only modified assets dirty; `AssetDatabase.SaveAssets()` writes them. `PlaceArchetype`'s private fields are reached via `SerializedProperty` (the Unity way); `WildernessSpotDefinition` and `WanderingThingDefinition`'s public fields are equally reachable through the same API for uniformity.
- **No external dependencies**: no Python, no pyyaml, no install step. Both scripts live in `Assets/Editor/` and are excluded from player builds automatically.
- **Dynamic column counts**: `place_archetypes.csv` has `stage1_*`/`stage2_*` NPC post-spec columns and `bStage1_*`/`bStage2_*` building stage columns driven by the per-run maximum. Adding a third stage requires only CSV data; the column count auto-expands on next export.
- **Item references as display names**: `ItemYield.Item` references are stored in CSVs as `ItemDefinition.DisplayName` strings (e.g. `"Bog Iron"`); the importer pre-loads all `ItemDefinition` assets into a `Dictionary<string, ItemDefinition>` keyed by display name and resolves them at import time. Object references in `BuildingStageDef.material` resolve the same way.
- **Compact yield format**: `ItemYield[]` arrays serialize as `"Name:weight:min:max,Name:weight:min:max"` — human-readable and diffable in Google Sheets without extra tooling.
- **Tooling language note**: an earlier draft of this iteration used Python + pyyaml for the scripts. Replaced with C# Editor scripts because Unity's own APIs give type-safe, dependency-free access to all game data with less code. See the "Tooling language default" rule in the Code Conventions section of CLAUDE.md.

---

### Iteration 26 — Playtest + Numeric Tuning Pass ✓

Full end-to-end playtest with focus on feel and pacing, now that all numeric parameters are in CSV. No new systems — this is a tuning pass that produces a stable baseline for the new mechanics in Iterations 27-30 to build on.

**What gets tuned:**

- **Daylight pool size and per-tick costs**: does a day feel full without feeling rushed? A productive session should leave the player feeling like they made real choices about where to spend time — not that they barely scratched the surface, and not that they ran out before doing anything meaningful. Target feel: 3-4 meaningful "legs" of activity per day (e.g. a foraging run, attending to two buildings, one NPC interaction, back to bedroll with something left over — but only just).
- **Wilderness spot tick intervals**: generic spots at 1.5-2s feel like repeated small actions; archetype spots (Fen Bog, Deep Wood, etc.) might warrant a slightly longer 2-2.5s interval to feel more deliberate — like you are doing something careful, not just holding a button.
- **Rare-drop chance and twilight modifier**: 8% base with ×1.5 at dawn/dusk means roughly 1 rare per 12 common ticks normally, 1 per 8 at the edges of day. Does a wilderness run reliably produce one rare per session without guaranteeing it? That's the target — the rare should feel found, not farmed.
- **Building material costs**: Stage 1 revival at 3 ticks of a common material (e.g. 3x Clay) and Stage 2 at 4-5 ticks of a rarer material should feel proportional to what those buildings unlock. If reviving a building takes longer than making three foraging trips, the loop drags. If it's faster, development trivializes.
- **NPC development threshold**: the current 3-tick universal track before specialization should feel like *enough time to notice the NPC exists* before they commit to a role — not a grind, not an instant.
- **Post-spec stage costs**: the Bog Keeper's `bog_keeper_drainage` costs 3x common item; `bog_keeper_iron_sense` costs 4x rarer item. Both should feel meaningful without feeling like a second job. These are the gating costs for the knowledge-layer mechanics in Iteration 28, so they need to be loose enough that players reach them in a reasonable session count.
- **Tended spot rest counts**: Bramble Patch (1 rest), Bee Skep (2 rests) — are these distinct enough to feel like different rhythms? Is 1 rest too fast to feel like cultivation?

**Output:** updated CSVs with tuned values committed as the new baseline. All changes justified in a short comment block at the top of each CSV (`# tuned: iteration 26`), so future sessions can see what was changed and why.

**Implemented (Iteration 26):**

- **`ImportGameData.cs` `ReadCsv()` updated** to skip `#`-prefixed comment lines before finding the header row, and also within the data section — enabling the `# tuned: iteration 26` comment blocks now present at the top of all three CSVs.
- **`place_archetypes.csv`**: `spotMinTickInterval` 1.5→2.0, `spotMaxTickInterval` 2.0→2.5 (all 5 archetypes) — archetype spots now land in the 2-2.5s target window, feeling more deliberate than generic spots. `bStage1_progressCost` 6→4 (all 5 archetypes) — Stage 1 revival now costs ~3 ticks × 2 material/tick = 6 common items, which is approximately one foraging trip: reviving a building costs a morning's work, not an entire day. `bStage2_progressCost` kept at 4 (unchanged); all `costPerTick` values kept at 2.
- **`wilderness_spots.csv`**: all values unchanged; generic and tended spot intervals (1.5-2s) already sit at the baseline target. Comment block added documenting the reasoning.
- **`wandering_things.csv`**: all values unchanged; 50%/80% good-chance swing and `badDaylightCost: 1` are the correct risk profile. Comment block added.
- **`Overworld.unity`** (Wanderer + Roughhand `NpcAttendable`): `minTickInterval` 1→1.5, `maxTickInterval` 1.5→2 — NPC attention ticks now match the archetype-spot interval, so spending time with an NPC feels the same tempo as working a wilderness spot. `progressCost` kept at 8 (unchanged): specialization after 8 ticks × ~2s/tick = ~16s of held attention gives the player time to observe the NPC and consciously choose to invest before the draw fires.
- **Daylight pool (maxDaylight: 24)**: unchanged — math analysis shows 24 daylight supports 3-4 meaningful legs (building revival at 4 ticks + foraging runs of ~6-8 ticks + NPC attention of ~4 ticks before specialization + bedroll), without exhausting on the first leg or leaving large surplus.
- **Rare-drop chances**: unchanged — 6-8% base (12% at dawn/dusk) reliably produces one rare per longer wilderness session without guaranteeing it per trip.
- **Tended spot rest counts**: unchanged — Bramble Patch (1 rest) vs Bee Skep (2 rests) provides the intended short/medium cultivation cadence distinction.

---

### Iteration 27 — Spot Tendedness: The Landscape Reacts

The wilderness currently has no memory. A spot attended ten times looks and behaves exactly like one attended for the first time. This iteration gives each spot a simple internal state — `tendedness` — that accumulates from repeated attention and degrades from neglect, and wires that state into yield behavior. The player is never shown a tenderness value; the change is felt, not read.

**Design intent:** Some spots should become noticeably more yielding over the course of a session if visited repeatedly in a patient way. Heavy extraction should cause mild depletion. A spot left alone for several rests should recover and perhaps feel slightly more alive on return. This isn't a stamina system or a resource cooldown — it's the landscape responding to the quality of attention paid to it, expressed through the texture of outcomes.

**Deliverables:**

- **`float tendedness`** (range `0f` to `1f`, initial value `0.5f`) added to `WildernessYieldAttendable`. Not serialized (per-session state only, like all current spot state). Resets to `0.5f` on day transition (the same `OnDayAdvanced` hook that already reseeds spots).
- **Drift rules**, applied at the end of each `OnAttentionComplete()` tick and at each `OnDayAdvanced()` call:
  - Each successful tick *during active hold*: `tendedness += 0.04f` (attending raises tenderness, up to cap)
  - Each rest (day advance) while unattended: `tendedness -= 0.08f` (neglect drops faster than a single visit raises — neglect accumulates)
  - Each rest while attended (at least once that day): `tendedness += 0.03f` — *less* than a tick, reflecting that a single visit per day is gentle cultivation, not intensive extraction
  - All values clamped `[0, 1]`; these are tuning baselines, to be revised in the next playtest
- **Yield effects**, applied inside `ItemYieldRoller.Roll()` when `tendedness` is provided (new optional parameter — existing call sites pass null and get current behavior):
  - `tendedness > 0.7f` (well-tended): common yield quantity range shifts up by 1 (e.g. a spot that normally yields 1-3 berries yields 2-4); rare-drop chance multiplied by `1.2f`
  - `tendedness < 0.3f` (depleted): common yield quantity range shifts down by 1 (floor 0 — a tick can produce nothing); rare-drop chance multiplied by `0.7f`
  - Middle band (`0.3f` to `0.7f`): no adjustment — baseline behavior
- **Overlay text** reflects tendedness only at the extremes, and only descriptively — not as a stat:
  - `tendedness > 0.7f`: description line appends `"— this place feels well-known to you"` (or archetype-specific: `"the clay here works easily"`, `"the mushrooms are thick this season"`)
  - `tendedness < 0.3f`: description line appends `"— the ground here is disturbed"` / `"— you've taken a lot from this spot recently"`
  - Middle band: description unchanged
- **Tended spots** (`TendedSpotAttendable`) are excluded — they already have their own rest-cycle rhythm and the tenderness model would conflict with `restsToHarvest`. Tendedness applies to ongoing (`WildernessYieldAttendable`) spots only.

**What this produces in play:** a player who returns to the same field repeatedly over the course of a day will find it gradually more giving. One who strips a clay pit multiple sessions in a row will notice it feeling thinner. A spot left alone for a few rests after heavy use returns to baseline. None of this is communicated as a mechanic — the player notices it as texture.

**Verification:** set `tendedness` to `0.9f` in the Inspector at runtime, hold E on the spot for several ticks, and confirm quantity yields are shifted up. Set it to `0.1f` and confirm some ticks yield nothing. Watch the overlay description change at both extremes. Confirm `tendedness` resets to `0.5f` after a rest (confirm via a `Debug.Log` in `OnDayAdvanced`).

**Implemented (Iteration 27):**

- **`float tendedness` (range `0f`–`1f`, initial `0.5f`)** added to `WildernessYieldAttendable` as non-serialized per-session runtime state. Not exposed in the Inspector (Inspector manipulation is used only in the verification playtest); per-session initialization to `0.5f` is handled by the C# field initializer.
- **Drift rules implemented in `WildernessYieldAttendable`**:
  - Each `OnAttentionComplete()` tick: `tendedness = Mathf.Clamp01(tendedness + 0.04f)` — attending raises a spot within the current day.
  - Each `OnDayAdvanced()` (subscribes in `Start()`, unsubscribes in `OnDestroy()`): if `attendedThisDay` → `+0.03f`; else → `-0.08f`; then `attendedThisDay = false`. Neglect compounds across multiple days (three consecutive unattended rests from the 0.5 baseline brings a spot into the depleted `<0.3` band); consistent daily attendance slowly grows the cross-day base toward the well-tended `>0.7` band after ~6–7 days. A `Debug.Log` fires on each rest showing the new value.
- **Design note on "resets to 0.5f"**: the spec phrase "resets to `0.5f` on day transition" is the *initial/session-start value*, not a per-rest hard reset. A hard reset each rest would make the `<0.3` depleted band unreachable naturally and would break the "multiple sessions of heavy use → thin spot" feel described in "what this produces in play." The implemented cross-day accumulation (±0.03/0.08 applied to current value) correctly produces depletion after ~3 rests of neglect and recovery after ~5 rests of neglect from peak. Tuning baselines (0.04/0.03/0.08) are to be revised per the next playtest pass.
- **Yield effects in `ItemYieldRoller.Roll()`**: added `float? tendedness = null` optional parameter (null = no adjustment, preserving all existing call sites). When provided: `tendedness > 0.7f` shifts the common yield's min/max quantity range up by 1 and multiplies `rareDropChance` by 1.2f; `tendedness < 0.3f` shifts qty range down by 1 (floor 0 — `Mathf.Max(0, ...)`) and multiplies by 0.7f. Middle band (0.3–0.7) is unchanged. A qty of 0 logs `"the spot feels thin — nothing found this time."` rather than the misleading "no room to carry it" message.
- **`WildernessYieldAttendable.OnAttentionComplete()`** now passes `tendedness` (the runtime `float` implicitly converts to `float?`) to `ItemYieldRoller.Roll()` after applying the per-tick drift. `GetEffectiveRareChance()` is still evaluated first (so `TwilightChanceModifier` continues to stack), and the result is passed as the base `rareDropChance` before `ItemYieldRoller` applies the tendedness multiplier on top.
- **Overlay text** via new `protected string WithTendednessSuffix(string baseDescription)` on the base class: appends `" — this place feels well-known to you"` when `>0.7f`, `" — the ground here is disturbed"` when `<0.3f`, and returns unchanged in the middle band. `GenericWildernessSpotAttendable.GetOverlayDescription()` now calls `WithTendednessSuffix(displayName)`; `PoiAttendable.GetOverlayDescription()` calls `WithTendednessSuffix(displayName)` when unlocked and returns `lockedDescription` unchanged when locked (tendedness text on a locked POI is meaningless).
- **`TendedSpotAttendable` is excluded**: it already subscribes to `DayAdvanced` for its own mark/harvest countdown and the tendedness model would conflict with `restsToHarvest`. No changes made to `TendedSpotAttendable`.
- **No scene edits needed**: `WildernessYieldAttendable` subclasses in the scene (`GenericWildernessSpotAttendable`, `PoiAttendable`) pick up the new behaviour automatically; `tendedness` is non-serialized and initializes at runtime.

**Manual playtest sequence (#27):** enter Play and approach any wilderness spot (e.g. a Field). Hold E repeatedly — on the first tick the overlay description is plain ("Field"); after several ticks the tenderness climbs above 0.7 and the overlay should append "— this place feels well-known to you." Confirm yields occasionally increase by 1 unit. Rest once; the console should log `"Field: tendedness X.XX after rest."` and the value should be slightly above 0.5 (attended). Approach a different spot you have never attended; after two rests of ignoring it, confirm its tenderness drops (log shows value decreasing toward 0.42, 0.34, etc.). When tenderness drops below 0.3 the overlay should append "— the ground here is disturbed" and occasional ticks should log "the spot feels thin — nothing found this time."

---

### Iteration 28 — Knowledge-Gated Yield Layers

NPC development currently affects Wandering Thing encounter odds and daylight costs via the `IOutcomeModifier` system. This iteration extends that same pattern to wilderness spot yield pools, creating a direct, felt connection between town development and what the wilderness offers.

**Design intent:** A player who has developed a Bog Keeper through `bog_keeper_iron_sense` should eventually notice that the Fen Bog spot yields something it didn't before. They didn't unlock this via a menu or receive a notification. They return to a familiar spot and find it changed. This is the "flame sword" feeling applied to foraging — the world giving you something earned, not a drop you farmed.

The same pattern works across archetypes: the Hedge Witch's knowledge of the grove changes what attention to the Deep Wood Shrine can surface. The Herald's trail records make the Old Road's rubble more legible. These are felt as the landscape becoming more readable as the people who know it develop.

**Deliverables:**

- **`KnowledgeYieldModifier`** (new `IOutcomeModifier` implementation, `Mossmark.World`) — checks a WorldState flag via `WorldContext.GetFlag(flagId)` and, if true, injects an additional `ItemYield` entry into the spot's common pool for this tick only (not permanently modifying the asset). The injected item uses a configurable weight alongside the spot's existing pool weights — `0.15f` by default, which meaningfully increases its appearance without dominating. Multiple `KnowledgeYieldModifier`s can be applied to one spot (one per relevant flag), each adding its item to the pool independently.
- **`WildernessSpotDefinition` gains `KnowledgeYieldEntry[] knowledgeYields`** — each entry holds `string requiredFlag`, `ItemDefinition item`, `int minQty`, `int maxQty`, `float injectedWeight`. This is authored in the CSV pipeline from Iteration 25 (`knowledge_flag`, `knowledge_item`, etc. column groups, up to a maximum of 2-3 entries per spot). Spots with no knowledge yields leave this array empty — no behavior change.
- **`WildernessYieldAttendable.OnAttentionComplete()`** builds the `OutcomeRequest` as before, then iterates `def.knowledgeYields`, constructing one `KnowledgeYieldModifier` per entry and calling `Apply(request)`. The roll call to `ItemYieldRoller.Roll()` gains a new `ItemYield[] injectedYields` parameter — a short list of items to include in this tick's pool alongside the base `commonYields`, each injected only if its modifier set it.
- **Archetype asset updates** — concrete first examples:
  - `fen_bog.asset` gains one `knowledgeYield` entry: flag `bog_keeper_iron_sense`, item Bog Iron, qty 1-2, weight 0.2f. *A Fen Bog spot already yields Bog Iron as its common item — but with `bog_keeper_iron_sense` active, Bog Iron becomes dramatically more likely and can yield 1-2 instead of 1.* The player notices the spot feels different without being told why.
  - `sacred_grove.asset` gains one entry: flag `hedge_witch_ravens_eye`, item Raven's Eye, qty 1, weight 0.15f. *Raven's Eye was the POI's rare drop — with Hedge Witch knowledge, it starts appearing in the archetype spot itself, at lower rate.*
  - `old_road.asset` gains one entry: flag `herald_trail_markers`, item Old Coin, weight 0.2f. *Coins were always in the rubble; the Herald just knows where to look.*

**What this is not:** a quest, a notification, or an explicit unlock. No overlay text changes. No "you can now find X here" message. The player returns to a spot and something is different. Whether they connect it to the NPC development is up to them.

**Verification:** develop a Bog Keeper through `bog_keeper_iron_sense` (confirmed via `WorldState.GetFlag` log). Attend a Fen Bog spot 20+ times and note Bog Iron yield frequency. Reset the flag (temporarily, via a debug toggle), attend the same spot 20+ times, and confirm frequency drops. Repeat for Hedge Witch / Raven's Eye on the Deep Wood Shrine archetype spot.

**Implemented (Iteration 28):**

- **`KnowledgeYieldEntry` (`[Serializable]` class, `Mossmark.World`, in `GenericWildernessSpotAttendable.cs`)** holds `string requiredFlag`, `ItemDefinition item`, `int minQty`, `int maxQty`, `float injectedWeight` — the authored data for one knowledge-gated injection. Sits alongside `ItemYield` in the same file since both are `[Serializable]` data classes used across the World namespace.
- **No separate `KnowledgeYieldModifier` class**: the `IOutcomeModifier` interface shape would require `OutcomeRequest` (in `Mossmark.Development`) to hold a `List<ItemYield>` (in `Mossmark.World`), creating a cross-namespace dependency inversion. Instead, the injection logic lives directly in `WildernessYieldAttendable.BuildKnowledgeInjectedYields()` — a private method that iterates `knowledgeYields`, checks each entry's flag via `WorldContext.GetFlag()`, and returns the matching yields as a temporary `ItemYield[]`. This achieves the same result (per-tick conditional pool injection with no asset modification) without the cross-namespace coupling.
- **`PlaceArchetype` gains `KnowledgeYieldEntry[] spotKnowledgeYields`** (new `[Header("Knowledge Yield")]` block, public accessor `SpotKnowledgeYields`) — the knowledge entries for an archetype's wilderness spot. Three archetype assets updated: `archetype_bog.asset` (flag `bog_keeper_iron_sense`, Bog Iron, qty 1-3, weight 0.2), `archetype_sacred_grove.asset` (flag `hedge_witch_ravens_eye`, Raven's Eye, qty 1-1, weight 0.15), `archetype_old_road.asset` (flag `herald_trail_markers`, Old Coin, qty 1-2, weight 0.2). Archetypes without a knowledge entry (`archetype_quarry.asset`, `archetype_reed_marsh.asset`) pick up the empty `Array.Empty<KnowledgeYieldEntry>()` C# default with no asset changes needed.
- **`WildernessSpotDefinition` gains `KnowledgeYieldEntry[] knowledgeYields`** (default empty array) — for the generic spot pool. All existing spot assets pick up the empty default; no asset edits needed for generic spots that have no knowledge-gated yields.
- **`WildernessYieldAttendable`** stores `KnowledgeYieldEntry[] knowledgeYields` as a private non-serialized field, set via `InitializeBase()` (new optional final parameter, default null → treated as empty). `OnAttentionComplete()` calls `BuildKnowledgeInjectedYields()` and passes the result to `ItemYieldRoller.Roll()` as the new `injectedYields` parameter.
- **`GenericWildernessSpotAttendable.Initialize()`** gained `KnowledgeYieldEntry[] knowledgeYields = null` as an optional final parameter, passed through to `InitializeBase()`. `PoiAttendable.Initialize()` was not changed — POIs do not participate in knowledge yields (their yield pool is already the distinctive archetype-unlocked content; adding a second knowledge layer on top would be redundant).
- **`ItemYieldRoller.Roll()`** gained `ItemYield[] injectedYields = null` as an optional final parameter. When non-null, the injected entries are merged with `commonYields` into a single `effectivePool` array using `System.Array.Copy` (avoiding a `using System;` directive that would conflict with `UnityEngine.Random`). The merged pool is used for one weighted pick exactly as `commonYields` was before — per-tick, no asset modification. All existing callers that omit the parameter get current behavior unchanged.
- **`WorldGenerator.SpawnWildernessSpot(archetype)`** passes `archetype.SpotKnowledgeYields` to `GenericWildernessSpotAttendable.Initialize()`. `SpawnSpotFromDefinition(def)` passes `def.knowledgeYields`. `SpawnPoi()` is unchanged — `PoiAttendable.Initialize()` has no knowledge yields parameter.
- **Export (`ExportGameData.cs`) and Import (`ImportGameData.cs`) updated**: `ExportSpots` now writes `knowledge{i}_flag`/`_item`/`_minQty`/`_maxQty`/`_weight` columns for `WildernessSpotDefinition.knowledgeYields`; `ExportArchetypes` writes `spotKnowledge{i}_flag`/`_item`/`_minQty`/`_maxQty`/`_weight` columns for `PlaceArchetype.SpotKnowledgeYields`. Both import counterparts parse these columns and update the corresponding `SerializedProperty` arrays. Column count is driven by the per-run maximum so adding a third entry to any asset requires only a data change.

**Manual playtest sequence (#28):** enter Play with a session that selects Fen Bog — confirm the Fen Bog spot initially yields only Bog Iron (its only common yield). Develop a Bog Keeper NPC through both post-spec stages until `WorldState.SetFlag("bog_keeper_iron_sense", true)` logs. Return to the Fen Bog spot and hold E 15-20 times: occasionally a tick should still yield Bog Iron but now the injected entry (weight 0.2 alongside the base weight 1.0) gives a 17% chance per tick of picking the injected pool entry (yielding 1-3 Bog Iron from that entry). Confirm no notification, no overlay text change, no "unlock" message — the change is felt as the spot behaving slightly differently. Repeat for Deep Wood Shrine + Hedge Witch (`hedge_witch_ravens_eye` → Raven's Eye begins appearing in the archetype spot at ~13% rate) and Old Road + Herald (`herald_trail_markers` → Old Coin injected at ~17% rate).

---

### Iteration 28.5 — Always-Something-Happens: Visits and Linger Interactions

Fully-developed NPCs and fully-restored buildings currently go inert — `CanAttend()` returns false and the player has no reason to interact with them again. This creates a feel of abandonment once the development arc completes. This iteration resolves the long-deferred design question of "hard CanAttend() gates vs. always-something-happens" by making completed entities remain attendable through a lighter interaction mode.

**Design intent:** Attending a fully-developed NPC or restored building should feel like checking in, not progressing. The mechanic should occasionally yield something that feels earned and special — but not be a reliable farming opportunity. The framing is mutual gift-giving and companionship, not a transaction.

**NPCs:** Archetype-spec NPCs (Bog Keeper, Herald, Hedge Witch, Weaver, Stonemason) have a 20% chance per visit to gift an item from a small, thematic, archetype-specific pool. Universal-spec NPCs (forager/caretaker/tinkerer) always give a flavor-only response. Visits cost 1 daylight (same as productive progress ticks), preventing farming by the daily limit. Exchange flavor text is always matched to the specific item gifted — text and item can never mismatch.

**Buildings:** Fully-restored buildings deliver a random flavor line at no daylight cost — a free "linger" interaction that feels ambient. The flavor lines are authored per archetype in `PlaceArchetype.BuildingRestoredFlavors`.

**Gift pool design:** Each archetype's gifts are items the NPC would naturally have on hand, not the development materials the player previously gave them. The common gift (65% weight) is a thematic non-dev-material item; the rare gift (35% weight) is the archetype's POI-exclusive rare item — something the player would otherwise only find deep in the wilderness.

| Archetype | Common gift | Rare gift |
|---|---|---|
| Bog Keeper | Mushrooms | Sunken Bell |
| Herald | Berries | Tally Stick |
| Hedge Witch | Beeswax | Antler Talisman |
| Weaver | Honeycomb | Woven Reed Charm |
| Stonemason | Pebbles | Carved Sigil Stone |

**Implemented (Iteration 28.5):**

- **`NpcAttendable` visit mechanic**: when `drawnSpecializationId != null && GetNextStage() == null` (fully developed), `OnAttentionComplete()` bypasses `ResolveAttention()` and calls `RunVisitInteraction()`. `lastAttentionWasVisit` flag is set to true before the call; `RequiresDaylight => lastAttentionWasVisit || LastAttentionMadeProgress` (visit costs 1 daylight) and `ContinueAttending => !lastAttentionWasVisit && ...` (visit is one-shot). `CanAttend()` returns `true` when fully developed. `GetOverlayInteractionLine()` shows `"Hold E to visit with {specializedName}"`.
- **`NpcExchangePool` private inner class** holds `exchangeChance`, `gifts` (`ItemYield[]`), `visitFlavors`, `exchangeFlavors`. Built in `HandleDeveloped()` via `BuildExchangePool(specId)`: archetype specs pull all four fields from `PlaceArchetype`; universal specs use hardcoded `universalSpecFlavors` static dictionary (flavor-only, `exchangeChance = 0f`, no items).
- **`PickWeightedGiftIndex()`** (renamed and changed return type from `PickWeightedGift`) returns the selected gift's array index rather than the `ItemYield` directly. `RunVisitInteraction()` uses this index to look up `exchangeFlavors[giftIndex]` — the exchange flavor is always the one authored for the exact gift received, preventing text/item mismatch.
- **`BuildingAttendable` flavor visit**: when `GetNextStage() == null`, `CanAttend()` returns `true` and `GetOverlayInteractionLine()` returns `"Hold E to linger near the {DisplayName}"`. `OnAttentionComplete()` calls `PostRestoredFlavor()` — picks a random line from `restoredFlavors`, posts via `NotificationManager.Post()`. `lastAttentionWasVisit` is set so `RequiresDaylight => false` (lingering costs no daylight) and `ContinueAttending => false` (one-shot).
- **`PlaceArchetype` gained** `npcExchangeChance`, `npcExchangeGifts`, `npcVisitFlavors`, `npcExchangeFlavors`, and `buildingRestoredFlavors` fields (new `[Header("NPC Exchange")]` and building-header addition). All 5 archetype assets updated with themed gift pools and flavor lines.
- **`using Mossmark.Visuals;` added to `NpcAttendable.cs`** to access `NotificationManager.Post()` — previously this class never posted notifications directly.

**Manual playtest sequence (#28.5):** develop any NPC through specialization and all post-spec stages until `GetNextStage()` returns null. Approach the NPC and confirm the overlay shows `"Hold E to visit with {name}"`. Hold E — confirm a notification appears (either a flavor line or a gift item notification), confirm the hold costs 1 daylight, confirm the hold does not continue (one-shot). Visit 10+ times to confirm the 20% exchange rate feels occasional. Approach a fully-restored building and confirm the overlay shows `"Hold E to linger near the {name}"`, the linger notification posts, and no daylight is spent.

---

### Iteration 29 — Settlement Maintenance: The Cost of Keeping Things Alive

Restored buildings and specialized NPCs currently stay in their developed state indefinitely at no ongoing cost. This means items become functionally useless once the restoration+upgrade loop is satisfied — there's nothing to spend them on. Maintenance introduces ongoing demand: the wilderness sustains the settlement, not just bootstraps it.

**Design intent:** The maintenance model should feel like natural upkeep, not a timer or a threat. A building that hasn't been tended starts to show it. The player notices before it becomes a problem and has time to respond. Nothing collapses suddenly. The emotional register is "this place needs me to keep coming back" — not "I failed to do something in time."

This is also the natural cap on expansion: the more developed entities in the settlement, the more common materials they collectively consume. Choosing to restore a new building is partly a choice about whether the wilderness can support another mouth.

**Deliverables:**

- **`int driftProgress` and `int driftThreshold`** added to `DevelopableEntity`. `driftProgress` increments by 1 on each `OnDayAdvanced()` call for any entity that has been developed (past its first threshold crossing — pre-development entities don't drift). `driftThreshold` is authored per entity type: buildings use a threshold of 5 rests (about 5 in-game days); NPCs use 7 rests (they're people, slower to feel neglect than a physical structure). These are the first-pass values; Iteration 26's tuning pass will revisit.
- **Two drift states**, expressed as overlay strings rather than new enums:
  - `driftProgress >= driftThreshold * 0.6f` and `< driftThreshold`: `"— needs tending"` appended to the entity's current overlay description. The building or NPC is still fully functional — this is a heads-up, not a penalty.
  - `driftProgress >= driftThreshold`: the entity enters a `MaintenanceNeeded` effective state. One capability is reduced: for buildings, the `DaylightCostMultiplier` on their yield output rises by `0.5f` (attending the building costs more daylight, reflecting reduced efficiency); for NPCs, their `baseGoodChance` equivalent (if applicable) drops, or their post-spec modifier flags temporarily become inactive. The overlay description line changes to `"— the [hearth/practice/path] has grown cold"` (building/NPC/landmark-specific strings authored in the entity's definition).
- **Maintenance reset:** attending an entity in drift with appropriate materials resets `driftProgress` to 0 and restores full capability. The maintenance materials are the same materials used to restore or develop the entity initially — no new item types required. The hold feels like the same action as development, but now it's upkeep. One or two ticks of the building's Stage 1 material is enough to reset drift — maintenance should be a light recurring cost, not a second full restoration.
- **`IMaintenanceConsumer` interface** (new, `Mossmark.Development`) — `int DriftThreshold { get; }`, `ItemDefinition MaintenanceMaterial { get; }`, `int MaintenanceCostPerReset { get; }`. `BuildingAttendable` and `NpcAttendable` implement it. `DevelopableEntity.OnDayAdvanced()` checks `this is IMaintenanceConsumer` before incrementing drift — pre-development entities and entities without a maintenance definition never drift.
- **Overlay integration:** `GetOverlayDescription()` on `DevelopableEntity` appends the drift warning/critical string when relevant. `HorizonUI` already reads `GetOverlayInteractionLine()` for each entity — maintenance state becomes visible in the Settlement Horizon panel automatically, at no UI cost.
- **What maintenance does not do:** it does not reset specialization, erase NPC identity, re-dilapidate buildings, or create a lose condition. The worst outcome is reduced efficiency for a few days. Recovery is always available and always quick. The point is to create ongoing pull for common materials, not ongoing anxiety.

**Concrete example:** the Smithy has been restored and has produced its `smith` demand. Five rests later, the player notices the Smithy overlay reads "the Forge — the hearth is cooling." Two more rests without attention and it reads "the Forge — the hearth has gone cold." Attending it with 2x Clay (its Stage 1 material) in a single hold resets drift and restores normal behavior. The player now has a reason to keep gathering Clay even after the Smithy is fully developed — and the wilderness keeps mattering.

**Verification:** restore a building, then rest 4 times and confirm `driftProgress` log shows 4/5 and overlay appends `"— needs tending"`. Rest once more and confirm capability reduction (log the `DaylightCostMultiplier` change). Attend with maintenance materials and confirm `driftProgress` resets to 0 and capability restores. Confirm pre-development entities do not drift (their overlay is unchanged after rests).

---

### Iteration 30 — Settlement Growth: New Arrivals

The settlement currently generates its full population at world-gen time and never changes. Development changes the *character* of existing entities but doesn't grow the settlement itself. This iteration introduces the first form of organic growth: new NPCs who arrive because the settlement has become the kind of place people come to.

**Design intent:** Growth shouldn't be announced or unlocked via a menu. The player rests one morning and someone new is standing near the settlement's edge — a presence that wasn't there before. Whether the player connects it to the specific development state they've cultivated is up to them. "The place attracted someone" is the feeling, not "you hit a milestone and received a reward."

The arrival model reuses the Wandering Thing infrastructure (a thing that appears near the settlement) but inverts its nature: instead of disappearing after a lifespan, the arrival stays and eventually becomes a fully attendable NPC.

**Deliverables:**

- **`ArrivalCondition`** — a new `IDependencyCondition` implementation that checks a combination of WorldState flags via `WorldContext`. Each condition is authored as a `string[] requiredFlags` (all must be true) and optionally a `int minimumDevelopedEntities` count (how many `DevelopableEntity` instances have crossed at least one development threshold). Both are checked in `IsSatisfied()`. Failure message: `""` (arrivals don't surface needs — they just don't happen yet).
- **`ArrivalSpawner`** (`Mossmark.World`) — a scene-level MonoBehaviour that checks its authored `ArrivalCondition` on each `OnDayAdvanced()` call. If satisfied and no arrival has already happened for this slot, it spawns an `ArrivalAttendable` at a position outside the town boundary (rejection-sampled, same as wandering things). Supports multiple spawner instances with different conditions, so the second arrival can be gated on different flags than the first.
- **`ArrivalAttendable : MonoBehaviour, IAttendable`** (`Mossmark.World`) — an ongoing attendable (same tick shape as `NpcAttendable`). First few ticks: the entity is wary and overlay reads something like `"A stranger resting at the settlement edge — hold E to approach"`. After a `wariness` progress threshold (3 ticks of simple attention, no material cost), the entity transitions: overlay reads `"[Name] — they seem willing to stay"` and `ArrivalSpawner` promotes them to a full `NpcAttendable` (same initialize-and-activate pattern as world-gen-spawned NPCs). From that point they behave like any other NPC, including drawing from the existing specialization pool.
- **Two authored arrival triggers** for the first prototype:
  - **First arrival**: `requiredFlags: ["bog_keeper_iron_sense"]` (or whichever flag set represents "the settlement has depth") + `minimumDevelopedEntities: 3`. A place that has started developing and has at least one deeply developed NPC attracts someone new.
  - **Second arrival**: `requiredFlags: ["herald_trail_markers", "hedge_witch_wound_lore"]` + `minimumDevelopedEntities: 4`. A settlement with knowledge, healing capacity, and a connected road attracts a second settler. The combination implies a place worth stopping at.
- **WorldState integration:** when an `ArrivalAttendable` transitions to an `NpcAttendable`, it calls `WorldState.SetFlag("settlement_grew", true)` — a hook for future modifiers to read (e.g. a wandering thing modifier that shifts odds when the settlement has grown at least once).

**What this is not:** a building queue, a housing system, a population cap mechanism. It's the smallest possible expression of "the settlement attracts people based on what it's become." Full population dynamics, multi-region migration, and housing requirements are post-prototype.

**Verification:** develop a Bog Keeper through `bog_keeper_iron_sense` and restore 3 entities. Rest once and confirm a new entity appears near the settlement edge (log position and confirm it's outside town bounds). Approach and attend for 3 ticks; confirm overlay transitions from wary to willing. Confirm the entity becomes a full `NpcAttendable` and can be attended/developed the same way as existing NPCs. Confirm a second `ArrivalSpawner` with the second trigger condition does not fire until both flag requirements are met.

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
- **Hard `CanAttend()` gates vs. always-something-happens**: **resolved (Iteration 28.5)** — fully-developed NPCs and fully-restored buildings are now always attendable. NPCs run a visit interaction: archetype-spec NPCs (Bog Keeper, Herald, etc.) have a 20% chance to exchange an item from their archetype's gift pool (drawn from `PlaceArchetype.NpcExchangeGifts`), otherwise delivering a flavor-only line; universal-spec NPCs (forager/caretaker/tinkerer) always deliver flavor-only lines. NPC visits cost 1 daylight (same as productive progress ticks) to prevent farming. Fully-restored buildings deliver a random `buildingRestoredFlavors` line at no daylight cost — a free "linger" interaction that feels ambient rather than mechanical. Both use `NotificationManager.Post()` to surface the result in the HUD. The `lastAttentionWasVisit` bool pattern (checked before `LastAttentionMadeProgress` in `RequiresDaylight`/`ContinueAttending`) ensures the visit path bypasses `ResolveAttention()` cleanly without touching `DevelopableEntity` internals. POIs and under-resourced buildings retain their hard gates (the always-something mechanic applies to *completed* development, not blocked development).
- **Outcome Influence Layer scope**: `OutcomeRequest` (Section 10) intentionally starts with a single `ChanceMultiplier` field. Additional dimensions are added only when a specific modifier needs one — not speculatively. **Iteration 17** added `DaylightCostMultiplier` (scales the bad-outcome daylight cost, `WoundLoreModifier` sets it to 0). A yield-weight bonus or quantity bonus remain unneeded so far.
- **Shaped attention / spot manipulation (two-button vs. single-verb)**: a "give vs. take" framing was explored and declined — a second button reintroduces player-selected action type, which is what the attention mechanic replaced. The tenderness model (Iteration 27) is the approved alternative: the landscape reacts to *how* it's been attended over time, not to an explicit "cultivate" choice. The two-verb idea remains worth revisiting if the single-verb tenderness system doesn't produce enough felt differentiation between exploitation and cultivation.
- **Ritual manipulation view (close-up shrine interaction)**: a distinct input mode for special objects — shrines, inner sanctums, POI-class landmarks — where the player places items, arranges carvings, marks runes, and the combination influences downstream systems (NPC productivity, Wandering Thing odds, tended spot recovery rate) with delayed and ambiguous feedback. Not a puzzle with a solution; closer to folk practice than game mechanic. The core design constraint: feedback must be delayed by multiple sessions and indirect enough that causality is never certain. Architecturally new (separate input mode, a spatial canvas, a deferred-modifier system) — significant scope. Captured here as a named future system, not an open question to resolve before the prototype is complete.
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
