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

- **Hold-to-attend** is the universal interaction. One button, held for some duration, on whatever is in range.
- **Outcome type is intrinsic to the target.** The player never picks the outcome type — only where they choose to spend attention.
- **Attention = stamina = the day's clock.** A pool depletes as attention is spent. Crossing thresholds triggers ambient phase messages ("midday now," "the light is going — you should head back"). Direct successor to P1's `DayCycleManager` 7-action pool, but unified with the resource itself rather than counted alongside it.
- **When the pool is empty:** the player can still move freely. Attempting to attend to anything that requires stamina shows a message ("too late to start that now") and consumes nothing. The only way to end the day is interacting with a **bedroll**, always present in the settlement, which triggers the day-transition sequence (the fade/reseed pattern from P1 is a useful reference, rebuilt against stamina thresholds rather than action counts).

---

## Attendable Taxonomy

Everything attention can target falls into one of these categories. All share the **generic dependency/response system** described below — the categories differ in *what they yield or require*, not in *how attention works on them*.

- **Generic wilderness spots** — field, clay pit, and similar. Instant yield on attention: a basic item pool with rare-drop chances. No state, no wait.
- **Tended-style spots** — carried from P1's mark → wait → harvest model. First attention marks the spot ("ready in N days"); a later attention (after the wait) harvests the yield.
- **Points of Interest (POIs)** — the 2-3 selected per-session archetypes (Fen Bog, Deep Wood, Old Road, etc.). Mechanically a wilderness spot (instant-yield or tended-style), but its yield pool is the region's *distinctive* items, and it is the attendable most likely to be **gated** by another entity's development (e.g. Fen Bog Hollow ↔ Bog Tender).
- **Buildings** — all dilapidated-with-latent-specialization at generation. Attention either consumes materials and produces development progress, or — if required materials aren't present in hold or chest — surfaces a "needs" response (see below). Specialization is fixed at generation (biased by local POIs/materials) but realized only once development progress crosses a threshold.
- **NPCs** — all unspecialized at generation. Attention either produces development progress directly (if whatever the NPC needs is already satisfied) or surfaces a "needs" response. What an NPC needs may include items, but is not limited to items (see Generic Dependency/Response System).
- **Wandering things** — creatures or figures, static-with-lifespan for this prototype (appear, available for a time, then disappear). Attention triggers either a positive outcome (special drop pool) or a negative outcome (lose all carried items + a creature-specific stamina/time cost, with a flavor message). Odds can be shifted by town development (e.g. a sufficiently advanced Hedge Witch changes a Bog Wraith encounter's odds).
- **Bedroll** — always present in the settlement. The only way to end the day. Not otherwise attendable (no yield, no progress).

---

## Generic Dependency / Response System

Every attendable (except the bedroll) resolves attention through the same generic process:

1. Check whether this attendable's current dependencies are satisfied.
2. **If satisfied:** attention produces this attendable's normal yield/progress.
3. **If not satisfied:** attention surfaces a human-readable "needs" response describing the unmet dependency, and (for buildings/NPCs) consumes no stamina.

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

## Development Application

When attention produces progress (for a building or NPC) and that progress crosses the relevant threshold, an upgrade is **applied automatically** — no player-facing selection menu. For this draft, the specific upgrade is chosen at random from the pool of upgrades whose dependencies are currently satisfied and which are relevant to the local region (POIs present, materials available, etc.).

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

## Day / Stamina Cycle

- Stamina pool depletes as attention is spent; thresholds trigger ambient phase messages, same narrative beats as P1's `DayPhase` progression but driven by the stamina value directly rather than a discrete action counter.
- **At zero stamina**: movement remains free; any stamina-requiring attention shows a "too late to start that" message and consumes nothing.
- **Ending the day**: always via a **bedroll**, present in every settlement. Buildings (including the Mead Hall, if/when it exists) behave as ordinary attendables — no dual-purpose "attend vs. rest" disambiguation needed. A more elegant unification is deferred.
- The fade/transition *pattern* from P1's `DayCycleManager` (black fade → world reset → fade in) is a useful reference for the rebuild, but should be re-triggered by stamina hitting the rest threshold rather than the old action-pool model — rebuild, don't port.

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
Replaces the Interaction Framework. One verb, hold-to-attend, on whatever is nearest.

- **`IAttendable`** (generalizes `IInteractable`) — every attendable implements `CanAttend()`, `GetOverlayDescription()` (line 1: name / current-state), `GetOverlayInteractionLine()` (line 2: action prompt or "needs" message), `AttentionDuration` (hold time; 0 for instant-yield spots), and `OnAttentionComplete()` / `OnAttentionCancelled()`.
- **`AttendableZone`** (from `InteractionZone`) — trigger collider, unchanged.
- **`AttendableDetector`** (from `InteractionDetector`) — nearest-target tracking ports directly; `CanInteract` is still not used as a pre-filter (a blocked target should still take focus so its "needs" line shows).
- **`AttentionManager`** (simplifies `InteractionManager`) — collapses `Idle → Prompting → InProgress` to `Idle → InRange → Attending`. No menu state and no separate focused-interactable indirection.
- **`AttentionInput`** (replaces `InteractionInput`) — tracks the held key; on hold-complete calls `OnAttentionComplete`, on early release calls `OnAttentionCancelled`. The hold-progress pattern from `StruckNodeInteractable` (`[####....]` fill text) generalizes to every attendable with `AttentionDuration > 0`.
- **Stamina gating**: whether an attention "counts" (consumes stamina) is decided by the result of `OnAttentionComplete()` — see Section 4. `AttentionManager`/`DayCycleManager` only deduct stamina when the result indicates a yield/progress actually occurred.

### 3. Stamina / Day Cycle
Direct successor to `DayCycleManager`; the action pool becomes a stamina pool.

- **`DayCycleManager`** — `ActionsPerDay`/`ActionsRemaining` (int) become `MaxStamina`/`StaminaRemaining`. `GetPhase()` thresholds re-tune to the new pool size, but the Dawn→Dusk progression and `DayCycleAmbientTextData` line-lookup port unchanged.
- **Zero-stamina behavior**: when `StaminaRemaining` is 0, a stamina-costing attention's `OnAttentionComplete` is skipped and the overlay shows the "too late to start that now" line — ported from `OnAmbientText`/`NothingLeftToGiveText`. Movement and instant browsing (Horizon, chest) stay free regardless.
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
- **`ResolveAttention()`** — the entry point `IAttendable.OnAttentionComplete` calls for buildings/NPCs/POIs. Implements the 3-step process from the design draft: check dependencies → produce yield/progress (stamina consumed), or surface "needs" text (stamina not consumed).

### 5. Inventory & Storage
- **`ItemDatabase` / `ItemDefinition`** — port unchanged, plus a new `StackCap` field (8-10) on `ItemDefinition` (or as a global constant).
- **`InventoryManager`** — `CarryLimit` (8 stacks) ports unchanged; `AddItem` additionally caps each stack at `StackCap`. Decide during implementation whether overflow is dropped or refused outright.
- **`SettlementChest`** — rebuilt per the design above (uncapped stacks/quantities; withdraw gives one player-stack-cap's worth at a time). Remains the one menu-based interaction — reuse `InteractionMenuControllerUITK`'s rendering for this single surface, renamed to something like `ChestMenuUI`.

### 6. Attendable Types
Each implements `IAttendable`; response logic differs per the taxonomy.

- **Generic wilderness spots** — instant yield (`AttentionDuration` ≈ 0), weighted item pool with rare-drop chance. Generalizes `ItemPickupInteractable` and the yield-roll logic from `StruckNodeInteractable.CompleteExtraction`.
- **Tended-style spots** — mark → wait → harvest. Ports `TendedSpotInteractable` near-verbatim: `Unmarked`/`Marked`/`Ready` states, `OnDayAdvanced` transition, `MaxConcurrentMarked` cap. Marking and harvesting both go through `OnAttentionComplete`; marking costs no stamina (marking is intention, not act).
- **POIs** — mechanically a wilderness spot or tended spot with a distinctive yield pool and an optional `IDependencyCondition` gate. Generalizes `EncounterLocationConfig.UnlockUpgrade` + `EncounterLocation`'s locked-prompt pattern.
- **Buildings** — `DevelopableEntity` subclass. `ResolveAttention()`: if deps satisfied, consume configured materials and `AddProgress()`; else surface "needs" overlay text, no stamina consumed. Crossing a stage threshold declares an NPC-specialization "need" (Section 7).
- **NPCs** — `DevelopableEntity` subclass, unspecialized at generation. `TimeCondition` ("needs more time") is the default early dependency. First threshold crossing triggers the specialization draw (Section 7).
- **Wandering things** — new type, not a `DevelopableEntity`. Static-with-lifespan spawn/despawn timer; `OnAttentionComplete` rolls a positive (drop pool) vs. negative (clear carry + stamina/time penalty + flavor text) outcome. Odds adjustable via an `EntityStateCondition`-style lookup against town development.
- **Bedroll** — thin `IAttendable`: `AttentionDuration` ≈ 1s, `OnAttentionComplete` calls `DayCycleManager.Rest()`. Ports `RestInteractable`'s hold-and-lock-during-transition behavior.

### 7. Development Application
- On `DevelopableEntity.TryApplyStage()`, the stage is chosen **automatically** — random among the dependency-satisfied candidates from `GetAvailableStages()`, not player-selected.
- **Building → NPC demand**: a building reaching its specialization threshold writes to a shared "needs" registry (e.g. `TownEntity.DeclaredSpecializationNeeds: HashSet<SpecializationId>`).
- **NPC specialization draw**: an unspecialized NPC's first threshold crossing reads `DeclaredSpecializationNeeds`; if non-empty, draws from it (consuming the need), else draws from a small universal track (2-3 generic specializations, always available).
- Builds entirely on Section 4's resolver — no new dependency machinery, just the new needs-registry condition type plus a "pick one at random" step (`GetAvailableUpgrades()` already exists in P1; only random selection is new — P1 leaves selection to the player).

### 8. World Generation: Region & Place Archetypes
Generalizes `WorldGenerator` + `TownGenerator` + `BiomeData`, adding the place-archetype layer from IDEAS.md that P1 deferred to post-prototype.

- **`RegionData`** (renamed from `BiomeData`) — defines a pool of 5-6 `PlaceArchetype` entries instead of flat NPC/zone/item pools.
- **`PlaceArchetype`** — bundles a `WildernessZoneConfig` (zone, item biases, tended spots, POI) with the building specialization(s) it biases and the NPC specialization track(s) it makes available.
- **World gen flow**: select 2-3 `PlaceArchetype`s → derive wilderness zones, building latent-specialization pool, and NPC specialization-track pool from that single selection (replacing P1's two independent draws).
- **`WorldGenerator`/`TownGenerator`** — ground-plane creation, container setup (`EnsureContainer`), seeded `Random.InitState`, and the inactive-GO spawn pattern (`SetActive(false)` → add components → `Initialize()` → `SetActive(true)`) all port unchanged.

### 9. UI
- **`AttendableOverlayUI`** (replaces `InteractionPromptUI` + the menu stack) — two-line overlay: description/name + interaction line. Live hold-progress reuses the `[####....]` text-bar pattern from `StruckNodeInteractable.GetInteractionPrompt`.
- **`DayCycleUI`** — phase label + stamina bar (replaces the 7-segment action display); dimmed state at zero stamina ports unchanged.
- **`DayTransitionFadeUI`** — ports unchanged.
- **`InventoryUI`** — ports unchanged (carry-limit display); add per-stack quantity display if `StackCap` is implemented.
- **`SettlementHorizonUI`** — reframed: lists realized specializations and current declared "needs" per entity, using the same "needs" string the overlay shows. Drops the "next upgrade" framing entirely — `GetAvailableStages()` is used only to confirm "ready" status, never rendered as a choice.
- **`ChestMenuUI`** — the one remaining menu surface (Section 5); reuses `InteractionMenuControllerUITK`'s rendering approach for a simple deposit/withdraw row list.

---

## Implementation Iterations

Ordered bottom-up: the attention/overlay loop first (proven against a placeholder), then stamina and the day cycle, then the first two attendable types, then the dependency resolver that everything else (buildings, NPCs, POIs, wandering things) builds on, then world generation ties it all into randomized sessions, then remaining UI and polish. Each row is sized to be independently testable, similar in granularity to Prototype 1's iteration list.

| # | Status | Focus | Deliverable | P1 Reference |
|---|--------|-------|-------------|---------------|
| 1 | [x] | Project Bootstrap + Movement | New Unity 6.4 project scaffolded (folders, namespaces, grey-box scene); player moves with camera follow | `PlayerMovement`, `CameraFollow`, `TriangleSpriteGenerator` |
| 2 | [x] | Attention Framework Core | Holding E on a placeholder attendable fires complete/cancel after a hold duration; releasing early cancels | `InteractionZone`, `InteractionDetector`, `StruckNodeInteractable` (hold-tracking) |
| 3 | [x] | Attendable Overlay UI | Approaching the placeholder shows a two-line overlay (name + "Hold E to attend" / live progress bar) | `InteractionPromptUI`, `StruckNodeInteractable` (progress-bar text) |
| 4 | [x] | Generic Wilderness Spots + Inventory | Attending a field/clay pit yields items (with rare-drop chance) into a capped, stack-limited inventory shown in HUD | `ItemDatabase`, `ItemDefinition`, `InventoryManager`, `InventoryUI`, yield-roll from `StruckNodeInteractable` |
| 5 | [x] | Stamina / Day Clock | Attention drains a visible stamina bar; phase-crossing ambient text fires; at zero stamina, attending shows "too late" and consumes nothing | `DayCycleManager` (phase thresholds, ambient text), `DayCycleAmbientTextData` |
| 6 | [ ] | Bedroll + Day Transition | Holding E on the Bedroll fades to black, restores stamina, reseeds wilderness spots, fades back in | `RestInteractable`, `DayTransitionFadeUI`, `DayCycleManager.Rest()` |
| 7 | [ ] | Tended-Style Spots | Attending an unmarked spot marks it; after rest(s), attending again harvests | `TendedSpotInteractable` (mark/wait/harvest state machine) |
| 8 | [ ] | Generic Dependency / Response Resolver | A hand-placed test entity with authored dependency conditions reports satisfied/unsatisfied correctly via debug output | `Entity`, `UpgradeDependency`, `UpgradePool`, `TownEntity` (generalized) |
| 9 | [ ] | Buildings: Revival via Attention | A dilapidated building is revived by repeatedly attending with required materials in hand; shows "needs" overlay when blocked, consumes no stamina | Resolver from #8; `UpgradeDefinition.RewardItems` / dependency model |
| 10 | [ ] | NPCs: Development + Specialization Draw | An unspecialized NPC develops via repeated attention ("needs more time"); specializes at first threshold from the universal track | Resolver from #8 |
| 11 | [ ] | Building → NPC Demand Loop | Reviving a building changes which specialization an unspecialized NPC draws | New `DeclaredSpecializationNeeds` registry on resolver from #8 |
| 12 | [ ] | World Gen: Place Archetypes | A session selects 2-3 archetypes; wilderness zones, building specialization bias, and NPC specialization pools all derive from that selection | `WorldGenerator`/`TownGenerator` (ground planes, containers, inactive-GO pattern, seeded random); `BiomeData` → `RegionData` |
| 13 | [ ] | POIs | A POI tied to a selected archetype is inaccessible until its gating dependency is satisfied, then attendable with its distinctive yield | `EncounterLocationConfig`/`EncounterLocation` (gated-prompt pattern) |
| 14 | [ ] | Wandering Things | A wandering thing spawns, is attendable for a randomized good/bad outcome, and despawns after its lifespan; odds shift with town development | New |
| 15 | [ ] | Settlement Chest (Rebuilt) | Player can deposit/withdraw at the chest; withdrawals respect the player's stack cap, one stack per withdrawal | `InteractionMenuControllerUITK` (rendering only) |
| 16 | [ ] | Settlement Horizon UI | Horizon panel lists each entity's realized specialization and current "needs," matching the overlay's language | `SettlementHorizonUI` (reframed) |
| 17 | [ ] | Playtest + Tuning Pass | Full day loop playable end-to-end; stamina costs, yield tables, and dependency thresholds tuned for a legible web | — |

---

## Open Questions / Deferred

- **Building development tiers**: is realizing the latent specialization a single threshold, or does a building continue developing through further tiers afterward (each potentially with its own material needs)?
- **Tending direction via materials**: deferred per Development Application above; system should be built to accommodate it later.
- **Wandering-thing persistence**: static-with-lifespan (appear, available for a time, disappear) is acceptable for this draft; roaming/pathing not required.
- **Maintenance-without-materials**: some lesser benefit from attending to a building without the right materials — noted as future, not in scope.
- **NPC "needs more time" vs. "needs diagnosis"**: whether attention-without-progress always surfaces a "needs" message, or whether NPCs can simply require repeated attention with no message until a threshold — try both in practice.
- **Specialized NPC's own further advancement**: once an NPC specializes, does it have its own development track beyond the initial specialization choice? Unresolved.
- **Region-level "unlocking" and inter-region travel**: explicitly out of scope for this prototype. The dependency web described here operates *within* a region; whether/how a region's overall development state gates travel to other regions is a later layer that should sit on top of these mechanics without requiring them to change.
- **No completion/win condition**: intentionally absent from this draft and likely absent from the final game — easier to add later than remove.
- **Carryover candidates**: chest and day-transition are rewrites-with-reference, not ports, per discussion. No other P1 systems are assumed to carry over; pull in deliberately if a gap appears.
