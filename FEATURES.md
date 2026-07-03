# Mossmark — Post-Prototype Feature Roadmap

> Begins after Prototype 2's core loop (Iterations 1–30) is complete and proven out. PROTOTYPE2.md is retained as a historical record of that arc and stops receiving new iteration entries — this document is the active roadmap going forward. See CLAUDE.md for current architecture and active constraints; see IDEAS.md for the unstructured long-horizon vision this roadmap draws from.

---

## Premise

P2 validated the core verb (Attention), the generic dependency/response system, and a first pass at ambient outcome influence (`WorldContext` / `IOutcomeModifier`). What it hasn't yet tested: whether the world can have its own momentum independent of the player.

Today, every entity's progress is driven entirely by attention — nothing advances between visits. The dependency/phase structure (dilapidated → revived, unspecialized → specialized → post-spec) is sound as a skeleton, but with attention as the only force moving anything through it, progress reads as a sequence of player-triggered steps rather than the player tapping into something already in motion. That's the gap between "tried not chosen" as a slogan and as a felt experience.

The throughline for this phase: **attending should mean accelerating a process, not triggering one.** Entities should drift forward (and backward, per the existing maintenance/drift model) on their own schedule, shaped by ambient state — what's nearby, what's tended, what's developed — and attention should be a multiplier on that drift, not its sole cause. This is a generalization of a pattern P2 already proved twice (`RealizedSpecializationChanceModifier`, `TwilightChanceModifier`) at the level of *odds*. The work here is extending the same shape to *progress itself*.

---

## Iteration 31 — Passive Drift on a Single Post-Spec Stage (Pilot)

A single-stage experiment to test whether ambient-driven progress actually feels different in play before generalizing anything. No new schema, no new ScriptableObject fields beyond what's strictly needed for this one stage. If it doesn't feel meaningfully different from the current attention-only model, the broader ecosystem direction needs rethinking before more work goes in — this iteration exists to find that out cheaply.

**Target:** one existing NPC post-spec stage with a plausible nearby ambient signal to draw from — `bog_keeper_drainage` (Bog Keeper) or `hedge_witch_wound_lore` (Hedge Witch), whichever has a wilderness spot or building near it whose `tendedness` or development state makes narrative sense as the drift source. Decide at implementation time by checking what's actually nearby in `Overworld.unity`.

**Mechanism:**

- Each rest, before any player action, the target stage's progress increments by a small passive amount — separate from and additive with the existing attention-driven `progressCost` contribution.
- The passive increment is read from `WorldContext` the same way `TwilightChanceModifier` reads `CurrentDayPhase` — likely the `tendedness` of an associated nearby spot, since that's already a continuous 0–1 value with no new plumbing required.
- Hook point: `DayCycleManager.DayAdvanced`, the same event `MaintenanceManager`'s drift-increment pass already subscribes to. This is "drift but counting up instead of down," reusing a wiring pattern that already exists for this exact stage of the entity (`DevelopableEntity.DriftProgress` / `IncrementDrift()`).
- Attention's existing contribution to this stage is unchanged — it still calls `OnAttentionComplete()` the same way it does today. The only change is that progress can now also move without the player.

**Explicitly out of scope for this iteration:**

- No new field on `NpcPostSpecStageDef` or any other shared schema — hardcode the passive-increment logic for this one stage if that's fastest. Generalizing into data-driven fields is only worth doing after the pilot validates the feeling.
- No UI to surface passive progress. The player should *discover* it by returning to an entity and noticing it's moved, not be told it's happening.
- No application to other stages, other entities, or other tracks (Hedge Witch *and* Bog Keeper both having it is not the goal here — one is).
- No work on the spot-relationship system from IDEAS.md (Bee Skep ↔ Bramble Patch, etc.) — that's a related but separate future iteration; don't let this pilot expand into it.

**Success criterion (playtest, not metric):** rest several times without attending to the target NPC, then approach it. Does it feel like something has been happening without you — like you've walked up to a process already underway — versus the current feeling of an inert entity waiting for input? This is a felt judgment, made in play mode, not a number to hit.

**If it lands:** next iteration generalizes the mechanism — likely a `passiveProgressPerRest` (or ambient-signal-scaled equivalent) field added to the shared stage definition schema, applied to more tracks.

**If it doesn't land:** revisit before assuming the ecosystem-influence direction is right. Possible failure modes to watch for: the increment is too small to notice (tune up before concluding the idea fails), the signal source doesn't read as causally connected to the NPC (player can't tell *why* it moved, which may undercut "tried not chosen" rather than support it), or passive movement makes attention feel less consequential rather than more (the opposite of the intent).

---

## Iteration 32 — State-Change Feedback Pass (Greybox)

Before any visual style work, this iteration establishes a baseline of legible feedback — the minimum signal layer needed to know whether the game's interactions are registering at all. The current greybox gives the player one feedback channel (items appear in inventory) for several meaningfully different events (progress made, stage crossed, ambient thing shifted). That collapse makes the world feel inert regardless of what's actually happening beneath the surface.

This is not a juice pass. It is not art. It is the lowest-cost test of whether the game's core loop is readable without external knowledge of its systems.

**Three signal types, in priority order:**

1. **Progress tick** — every `OnAttentionComplete()` that advances progress on *anything* emits a small, consistent visual beat on the attended object: a brief scale-pulse, color flash, or similar. Decoupled entirely from item pickup. This is the most important signal: right now "I attended and got loot" and "I attended and nothing visibly happened" are the only two legible states. A third state — "I attended and *something moved*, even if I can't see what yet" — is the core of this iteration.

2. **Stage crossed** — when an entity crosses a development threshold (dilapidated→revived, unspecialized→specialized, post-spec stage advanced), the transition emits a distinct, bigger beat than the progress tick: a flash, a silhouette-swap with a noticeable transition, a sound sting. Should read as a *moment* — the "Flame Sword Feeling" in miniature — not a silent instant swap. This is the beat that makes persistent effort feel like it landed.

3. **Passive/ambient movement (lower priority)** — when the player returns to something that drifted without them (Iteration 31's passive drift), some minimal affordance that it moved: a different idle tint, a subtle ambient particle, anything that doesn't require comparing present-state to remembered-state. Don't let this one delay the first two.

**Scope constraints:**

- Greybox-only: no new art assets, no sound design polish — placeholder or free SFX is fine, colored particle bursts are fine.
- Wire these to events that already exist (`OnAttentionComplete`, stage-cross events in `DevelopableEntity`) — this should be mostly an event-subscription pass in `Mossmark.Attention` and `Mossmark.Development`, not new architecture.
- No feedback on micro-interactions that don't represent actual state change. The goal is signal clarity, not density.

**Success criterion (playtest, not metric):** play the same greybox loop without explaining the systems to yourself first. Does the world *communicate* what's happening — does attending feel like it's doing something, does stage-cross feel like an event worth noticing — independent of visual polish? If yes, proceed to art direction work with confidence that legibility is solved. If no, the problem is deeper than feedback and needs a design pass before art.

**Design principle (forward-looking):** from this iteration onward, every new system or stage added to the game should include a feedback specification alongside its mechanical spec — what does the player see/hear when this moves? This doesn't need to be elaborate, but it should be explicit. Feedback is not a polish pass at the end; it is part of what makes a mechanic exist from the player's perspective.

---

## Iteration 33.1 — Progress Cost Tuning Pass

A pacing adjustment to ensure passive drift (Iteration 31) and cross-influence (Iteration 34) have room to be felt. Currently, NPC post-spec stages complete in one or two rests — fast enough that passive acceleration is invisible before it completes, which undermines the core premise of Iteration 31. Resting is not a burden, so extending timelines is cost-free from a player-friction standpoint.

**Change:** double all `progressCost` values on NPC post-spec stages as a starting baseline. This is a deliberate estimate, not a settled number — revisit after Iteration 34's cross-influence content is in place and a few play sessions have given a feel for the new rhythm. More concurrent things happening passively makes any single track feel slower already; the right final values may be higher or lower than double depending on how the ecosystem density feels once more cross-influence seams are authored.

**Scope:** data-only change to existing `NpcPostSpecStageDef` assets. No code changes, no new fields. Touch only post-spec stages — specialization costs and building restoration costs are separate pacing questions, not in scope here.

**Scope guard:** do not simultaneously tune specialization costs, building costs, or wilderness spot rest counts. Isolate the variable. If the prototype feels sluggish after this, diagnose before adjusting — the cause may be that the loop is thin (too few things to do between rests) rather than that the costs are wrong, and adding cross-influence content (Iteration 34) may resolve it without touching costs again.

---

## Iteration 34 — Cross-Pursuit Influence (Authored Seams)

Establishes loose, discoverable connections between the three pursuit dimensions (wilderness, NPCs, buildings) so that investing in one occasionally opens something visible in another — without creating prescribed chains or required sequences. The player who ignores buildings entirely still has a game; they're just not seeing some of what the world can do. Connections are ambient and authored, not telegraphed by UI.

This is the "loose connections, not tight loops" framing: each seam exists in the world whether or not the player notices it. Discovery is the mechanic.

**Three seams to author, in priority order:**

1. **Wilderness → NPC** (already half-implemented): extend Iteration 31's `tendedness`-driven passive drift from the single `bog_keeper_drainage` hardcode to a small authored set of NPC/spot pairs via `IOutcomeModifier`. Two or three pairs to start — the Bog Keeper/fen spot connection is already proven; add one more NPC whose post-spec drift plausibly responds to a nearby wilderness spot's tendedness (Hedge Witch + herb spot is the natural candidate). Move from hardcode to data-driven `IOutcomeModifier` instances at the same time, since Iteration 33.1's cost increases now give drift enough time to be felt.

2. **NPC → Wilderness**: a specialized NPC changes what nearby wilderness spots can yield — not better yields, *different* yields. A specialized Bog Keeper shifts the fen spot's yield table toward drainage-specific items; a specialized Hedge Witch shifts herb spot yields toward wound-treatment materials. This keeps specialization meaningful beyond inventory accumulation and makes the wilderness feel like it responds to the settlement's character. Author one NPC/spot pair to test the pattern — implement via a new `IOutcomeModifier` that reads `WorldContext` for NPC specialization state and modifies the spot's `OutcomeRequest`.

3. **Building → NPC or Wilderness** (lower priority): a restored building quietly changes what's possible nearby. A restored Granary extends the window before maintenance demands recur; a restored Mill shifts what grain-adjacent spots can produce. Author one building/entity connection — whichever has the clearest world-logic justification given current entity roster. If implementing the first two seams reveals a clean architectural pattern for this, follow it; if it requires significant new plumbing, defer to a future iteration.

**Explicitly out of scope:**
- No UI labeling of connections. No tooltip saying "tend the bog to help the Bog Keeper." Discovery is the mechanic.
- No required chains — these are influence modifiers, not unlock gates. Nothing should be *blocked* by whether a cross-influence seam has been triggered.
- No new pursuit dimensions beyond wilderness/NPC/building. The goal is depth in the existing three, not breadth.

**Success criterion (playtest, not metric):** play through a session focusing on wilderness tending, then visit NPCs without having attended them deliberately. Does anything feel like it has been quietly affected? Does it prompt a theory about why without being explained? That moment of "wait, did tending the bog do that?" is the target.

**Feedback spec:** no new feedback signals for the cross-influence itself — the existing passive drift halo (Iteration 32) handles the "this moved without me" signal. The connection's cause should remain slightly opaque. If it becomes necessary to signal that *a building* specifically affected something, use the same ambient particle tier, not a new dedicated signal type.

---

## The Investigation Loop (Iterations 35–37)

A proof-of-concept arc for a new dimension of play: **items as clues, stations as the place you interrogate them, NPC wants as the place answers become useful.** The core insight this arc tests: the pull of "maybe I should check that bog again" comes from having a *theory* about what things are for — and right now nothing in the game gives items meaning beyond inventory accumulation.

Design shape: items carry 1–2 hidden **properties**, drawn from a small shared vocabulary of folk phrases (e.g. *"holds the cold"*, *"turns water"*, *"binds fast"*). Properties are discovered by experimenting at a conversion station, not read off tooltips. The same phrases appear in NPC wants, so recognition — "wait, the cord I made *binds fast*, and the Bog Keeper needs something that binds" — is the dopamine beat. Learning compounds because phrases are shared across items: learning *"binds fast"* once pays off every time it reappears.

Register decision (settled in design discussion): folk phrases with **rigid exact-match wording** wherever a property appears — item descriptions, discovery moments, NPC wants. The consistency does the mechanical work; the diegetic register keeps it inside "Felt Not Read." Unknown properties are not displayed as "?" — the item's description simply feels incomplete (*"heavy in the hand. There's more to it."*). A mechanical-tag debug display toggle is kept for development.

The three iterations are deliberately ordered so each is independently testable: 35 proves the data and display layer, 36 proves the experimentation verb, 37 closes the loop. Do not start 36 until 35's display reads well in the overlay; do not start 37 until 36's discovery beat feels rewarding in playtest.

---

## Iteration 35 — Item Properties (Folk-Phrase Vocabulary)

The data and display foundation. Items gain hidden properties; the player-facing surface shows only what has been learned.

**Deliverables:**

- **Property vocabulary as data**: a single authored list of ~8–10 properties, each with an `id` and an exact display phrase. Starting set (adjust wording at authoring time, but keep each phrase short and consistent): `holds_the_cold` ("holds the cold"), `turns_water` ("turns water"), `binds_fast` ("binds fast"), `split_prone` ("splits under strain"), `burns_slow` ("burns slow"), `keeps_well` ("keeps well"), `draws_the_eye` ("draws the eye"), `heavy_true` ("heavy and true"). Author as a ScriptableObject asset or static registry — small enough that either works; prefer whatever the CSV pipeline can absorb later.
- **`ItemDefinition` gains `propertyIds` (1–2 per item)**. Author properties on ~6 existing items to start: Bog Iron (`heavy_true`, `holds_the_cold`), Reeds (`split_prone`, `binds_fast`), Crow Feather (`draws_the_eye`), Mistletoe (`keeps_well`, `turns_water`?), Flint (`burns_slow`?), plus one more chosen at implementation time. Exact assignments are an authoring decision — the constraint that matters is **every property must appear on at least two items** somewhere in the full set, so recognition can compound.
- **Discovery state**: per-item-per-property known/unknown flags on a new `PropertyKnowledge` store (player-scoped, session-only for now — no save system exists). `WorldState`-flag-style access via `WorldContext` so future `IOutcomeModifier`s can read it.
- **Display**: the detail overlay (Iteration 33) shows known properties as their folk phrases under the item name. Items with undiscovered properties append a single fixed line: *"There's more to it."* Items with all properties known show no such line. No question marks, no empty slots, no counts.
- **Debug affordances**: a debug key/command to reveal all properties (for testing 36/37 without grinding discovery), and a debug toggle to render properties as mechanical tags instead of phrases.

**Out of scope:** any discovery *mechanism* beyond debug reveal — that's Iteration 36's job. This iteration ships with all properties unknown in normal play; that's fine, it's testable via the debug toggle.

**Feedback spec:** none needed yet — no new player-facing events occur in this iteration. Display-only.

**Success criterion:** with debug-revealed properties, does reading the overlay across several items make the shared vocabulary *visible* — do you notice that two items share a phrase, and does that noticing feel like it means something? If the phrases read as flavor text rather than signal, tighten the wording before proceeding.

---

## Iteration 36 — Conversion Station (Crude Working Surface)

The experimentation verb. One station where items are combined, with discovery as the payoff for every attempt — successful or not.

**Deliverables:**

- **One station**: the restored Workshop becomes the working surface. Attending the restored Workshop (which currently short-circuits to flavor) now opens the **working view** — a minimal UI panel, not a new scene: pack list on one side, 2–3 placement slots, a "work" hold-interaction. Reuse the chest UI's interaction pattern (`InteractionMenuControllerUITK` rendering, movement freeze, Esc to close). This is deliberately the *crude* version of the Ritual Manipulation View — no spatial canvas, no runes; just place, hold, resolve. If this loop proves out, the full ritual view (Open Threads) inherits from it.
- **Recipe data**: `ConversionDef` assets, each holding input requirements, output `ItemDefinition` + qty, and a result flavor line. Two recipe key styles, both supported from the start:
  - *Item-keyed* (exact items): 3× Reeds → 1× Plaited Cord. Author 2 of these.
  - *Property-keyed* (any items with matching properties): 1× item with `binds_fast` + 1× item with `heavy_true` → 1× Weighted Snare (or similar — output item chosen at authoring time). Author 1 of these. Property-keyed recipes are the payoff for learning the vocabulary; item-keyed recipes are the on-ramp.
- **New output items**: Plaited Cord (`binds_fast`, second property TBD) and one property-keyed output, each a new `ItemDefinition` with its own properties — conversions produce new questions, not dead ends.
- **Discovery on every attempt**: a successful conversion reveals the properties that *made it work* on the consumed items (and marks them known). A failed attempt (no recipe matches) consumes nothing, produces a diegetic failure line, and reveals **one** unknown property on one placed item — chosen randomly among placed items with unknowns. Failure always teaches. This is the core honesty of the loop: experiments pay in knowledge even when they don't pay in goods.
- **Daylight cost**: each work attempt costs 1 daylight (same as a productive attention tick) — experimentation is real activity, not free menu time.

**Out of scope:** more than one station; station-specific recipe pools; multi-step chains (output-of-output recipes) beyond what falls out naturally from Cord being usable as an input; any UI polish beyond functional.

**Feedback spec (required per Iteration 32 principle):** successful conversion = the stage-cross tier signal (bigger pop + sting) on the Workshop, plus the output item's arrival notification. Property discovery = its own small distinct beat — a brief highlight on the item row + the folk phrase appearing with a short fade-in, whether from success or failure. Failed attempt with no discovery remaining = flat diegetic line only, no juice ("nothing comes of it").

**Success criterion (playtest, not metric):** place random junk in the Workshop with zero foreknowledge. After ~5 attempts, do you have theories? Do you *want* to go gather something specific to test one? The pull to leave the station and go get something is the whole test.

---

## Iteration 37 — Property-Phrased Wants (Loop Closure)

The loop's destination. One NPC want expressed in the shared vocabulary, satisfiable by property rather than by exact item.

**Deliverables:**

- **One want, on the Bog Keeper**: an existing or new post-spec stage whose item gate changes from "requires exact item X" to "requires any item with `turns_water`" (or `binds_fast` — pick whichever has better item coverage after 35's authoring pass). The overlay/detail text phrases it diegetically with the exact folk phrase: *"needs something that turns water for the drainage channels."*
- **Property-gate resolution**: extend the existing stage item-gate check to accept a `requiredPropertyId` as an alternative to `requiredItem`. When satisfied by property, consume the matched item as usual. If multiple carried items match, consume the cheapest/most-common one (or prompt — implementation's choice, don't overbuild).
- **Recognition guard**: the want's phrase must be discoverable through Iteration 36 — i.e., at least one reachable item with the required property must be learnable at the Workshop before this want appears. Verify the content path at authoring time; this is a content-ordering check, not a system.
- **Keep one exact-item want** elsewhere unchanged, for contrast in playtest.

**Out of scope:** converting all existing wants to property-phrasing; multiple property-wants; want generation. One authored instance, tested against one authored discovery path.

**Feedback spec:** satisfying a property-want uses the existing stage-cross signal — no new tier. The recognition moment itself needs no signal; it happens in the player's head, which is the point.

**Success criterion (playtest, not metric):** the full loop, cold: gather → experiment at the Workshop → learn a phrase → encounter the Bog Keeper's want → recognize that something you carry (or know how to make) answers it. Does the recognition land as *your* insight rather than a fetch instruction? If yes, this dimension is worth building out — more properties, more stations, the full ritual view. If the recognition feels like menu-matching with extra steps, the vocabulary is too small or the phrases too legible — diagnose before expanding.

---

## Iteration 39 — Multi-Station Property Discovery

Generalizes Iteration 36's single Workshop into several stations, each biased toward different properties, each unlocked through the existing dependency/attend pattern rather than a new gate type. Fixes the "grind one station to learn everything" problem — station choice becomes a real decision, not a UI skin.

**Deliverables:**

- **Station = existing building pattern.** A station is a `BuildingAttendable` whose final stage opens the working view (Iteration 36's `WorkshopUI`, generalized to point at any station instance). No new unlock system — reuse `BuildingStageDef`/`BuildingStagePool`/`stage_conditions.csv` as-is. Unlocking a station is exactly as legible/illegible as restoring any other building.
- **Station bias**: each station def gets a short list of property ids it can resolve (`biasPropertyIds`). Recipe resolution filters `ConversionDef` candidates to ones whose required properties intersect the station's bias — same recipe list as today, just partitioned by station.
- **Two new stations**, reusing existing archetype locations for narrative grounding:
  - Fen shrine (bog archetype) — biased `turns_water`, `holds_the_cold`.
  - Hearth or kiln (sacred_grove or old_road archetype — pick whichever has a plausible building) — biased `burns_slow`, `keeps_well`.
- **Coverage check**: with 3 stations total (Workshop + 2 new), every one of the 8 properties from Iteration 35 must be resolvable at ≥2 stations. Audit the existing property/item/recipe assignments and add 1-2 recipes if a property falls short — don't invent new properties for this iteration.

**Explicitly out of scope:** the full Ritual Manipulation View (spatial canvas, runes) — this stays the crude place/hold/resolve UI; more than 2 new stations; moving `ConversionDef` into the CSV pipeline (worth doing eventually, not required here).

**Feedback spec:** reuse Iteration 36's beats as-is (success pop, discovery highlight, flat failure line). No new tier.

**Success criterion (playtest, not metric):** do you pick which station to visit based on a property theory ("I think this binds — the Workshop's more likely to confirm it than the shrine")? If station choice never factors into a decision, the bias isn't legible enough — tighten it before adding more stations.

---

## Iteration Status

Numbering continues from PROTOTYPE2.md's final iteration (30) to avoid renumbering confusion across documents — this table is the active one going forward. PROTOTYPE2.md's own table stops at 30 and is not updated further.

| # | Status | Iteration | Summary | Key Types |
|---|---|---|---|---|
| 31 | [x] | Passive Drift on a Single Post-Spec Stage (Pilot) | Bog Keeper's `bog_keeper_drainage` stage gains rest-driven passive progress (0/1/2, banded off the Fen Bog spot's `tendedness`), additive with existing attention-driven progress and gated the same as any other stage (item still required to apply) — pending playtest verdict | `NpcAttendable.OnDayAdvanced()`, `WorldGenerator.GetArchetypeSpot()`, `WildernessYieldAttendable.Tendedness` |
| 32 | [x] | State-Change Feedback Pass (Greybox) | Wire visible/audible feedback to three signal tiers (progress tick → scale pulse, stage-cross → shape swap triangle↔circle + bigger pop, passive drift → white halo child until next productive attend); player sprite rocks ±5° Z while attending | `EntityFeedback`, `CircleSpriteGenerator`, `DevelopableEntity.OnProgressMade`, `WildernessYieldAttendable.OnProgressMade`, `TendedSpotAttendable.OnProgressMade`, `NpcAttendable.OnPassiveDriftAccrued`, `PlayerController.HandleAttentionRock` |
| 33 | [x] | Detail Overlay (Bottom-Right) | Second UI panel anchored to the bottom-right corner showing the hovered entity's name and a bullet list of all development stages permanently applied to it — gives the player a persistent at-a-glance record of what has been achieved without attending | `IAttendable.GetAppliedUpgrades()`, `DevelopableEntity.GetAppliedUpgradeNames()`, `AttendableOverlayUI` detail panel |
| 33.1 | [x] | Progress Cost Tuning Pass | Doubled all NPC post-spec stage `progressCost` values: stage 1 (common-item) 3→6, stage 2 (rare-item) 4→8, across bog/old_road/sacred_grove archetypes; data-only change to `.asset` files and `place_archetypes.csv`; reed_marsh and quarry have no post-spec stages and were untouched | `NpcPostSpecStageDef` fields in `archetype_bog.asset`, `archetype_old_road.asset`, `archetype_sacred_grove.asset`, `place_archetypes.csv` |
| 34 | [x] | Cross-Pursuit Influence (Authored Seams) | Three authored seams between pursuit dimensions — Seam 1: generalized Iteration 31 hardcode to data-driven `passiveDriftSourceArchetypeId` on `NpcPostSpecStageDef`; two live pairs (bog_keeper_drainage ← bog, hedge_witch_wound_lore ← sacred_grove). Seam 2: `KnowledgeYieldEntry.requiredSpecializationId` — hedge_witch specialization injects Bark Strips into Deep Wood Shrine yields. Seam 3: `BuildingStageDef.worldStateFlag` — Woodland Shrine restoration sets `shrine_tended` flag, which unlocks Carved Sigil Stone injection at Deep Wood Shrine. No new UI; discovery is the mechanic. | `NpcPostSpecStageDef.passiveDriftSourceArchetypeId`, `KnowledgeYieldEntry.requiredSpecializationId`, `BuildingStageDef.worldStateFlag`, `BuildingAttendable.HandleDeveloped()`, `WorldContext.IsSpecializationRealized()` |
| 35 | [x] | Item Properties (Folk-Phrase Vocabulary) | 8 folk-phrase properties as a static `PropertyRegistry` (holds_the_cold, turns_water, binds_fast, split_prone, burns_slow, keeps_well, draws_the_eye, heavy_true); 1–2 properties authored on 10 items (Bog Iron, Reeds, Crow Feather, Mistletoe, Flint, Bark Strips, Flat Stones, Clay, Sticks, Raven's Eye) — each property on ≥2 items; session-only `PropertyKnowledge` discovery store in `Mossmark.Development`; inventory slots show known folk phrases in muted green under item name, "There's more to it." for unknowns; debug backtick reveals all, F2 toggles tag display; `WorldContext.IsPropertyKnown()` accessor for future `IOutcomeModifier` use | `PropertyDefinition`, `PropertyRegistry`, `PropertyKnowledge`, `ItemDefinition.PropertyIds`, `WorldContext.IsPropertyKnown()`, `InventoryUI` (AppendPropertyLines, debug Update) |
| 36 | [x] | Conversion Station (Crude Working Surface) | `WorkshopAttendable` (single restoration stage, flat_stones) at (0,3) in town; attended when restored opens `WorkshopUI` — 3 slots + Work row; 2 item-keyed recipes (3×Reeds→Plaited Cord, 2×Bark Strips+1×Sticks→Tinder Bundle) + 1 property-keyed (binds_fast+heavy_true→Weighted Snare); success reveals all properties of consumed items, failure reveals one unknown property from placed items; 1 daylight per attempt; 3 new `ItemDefinition` assets with properties (Plaited Cord: binds_fast+keeps_well, Tinder Bundle: burns_slow+keeps_well, Weighted Snare: heavy_true+draws_the_eye); gold row highlight for newly discovered properties; WorkshopUI blocks movement + new attention starts; EntityFeedback.TriggerPop() fires on success | `ConversionDef` SO, `WorkshopAttendable`, `WorkshopUI`, `EntityFeedback.TriggerPop()`, `PlayerController`+`AttentionManager` WorkshopUI.IsOpen guards |
| 37 | [x] | Property-Phrased Wants (Loop Closure) | Bog Keeper's `bog_keeper_drainage` stage gate changed from exact Bog Iron (3×) to any carried item with `turns_water`; `PropertyAvailableCondition` implements the gate (carry-only, not chest); matched item consumed from pack when stage fires (highest-quantity item chosen); overlay shows "needs something that turns water for the drainage channels" when gate unsatisfied; `bog_keeper_iron_sense` kept as exact-item stage for contrast; content path verified: Mistletoe+Clay both carry `turns_water` and are learnable at the Workshop before Bog Keeper specializes | `PropertyAvailableCondition`, `NpcPostSpecStageDef.requiredPropertyId`/`wantDescription`, `NpcAttendable.BuildPostSpecStages()` property-gate branch, `NpcAttendable.ConsumePropertyMatchedItem()`, `archetype_bog.asset` |
| 38 | [x] | Relational Data Architecture Migration | P1-style relational authoring layer over P2's resolver, in 8 phases: (1) `ConditionCsvImporter` — CSV rows → concrete `IDependencyCondition` objects assigned via `SerializedProperty.managedReferenceValue` into `[SerializeReference]` arrays, with idempotent change-detection + self-test menu item; (2) `YieldTable` (ID-addressable shared yield pool, P1 LootTable analogue); (3) `PlaceArchetype.spots` — real list of `WildernessSpotDefinition` refs replaces the inline single-spot block; `spotId` registry (`WorldGenerator.GetSpot`) replaces archetype-keyed drift-source lookup; explicit `npcMaintenanceMaterial` replaces `CommonYields[0]` positional coupling; (4) `NpcStageDef` SO + `NpcStagePool` replace embedded `NpcPostSpecStageDef[]` — gates are authored condition lists, killing `useRareItem`/`requiredPropertyId` ad hoc unions; (5) `BuildingStageDef` promoted to SO + `BuildingStagePool` — `requiredSpecialization` becomes an authored `SpecializationRealizedCondition`; (6) `place_archetypes.csv` trimmed from 96 slot-numbered columns to a 32-column composition root; relational files `npc_stages.csv`/`building_stages.csv`/`stage_conditions.csv`/`yield_tables.csv`; export emits pool-order (semantic) not alphabetical; full round-trip verified (import→export→import = 0 changes); (7) rare yields become weighted pools (`ItemYield[]` through `ItemYieldRoller`); first shared table `old_coin_finds` (Clay Pit + Field); (8) DATA_SCHEMA.md schema reference. Play-mode verified: world gen, spawns, wandering things all run through the new data path | `ConditionCsvImporter`, `CsvUtil`, `YieldTable`, `NpcStageDef`, `NpcStagePool`, `BuildingStageDef` (SO), `BuildingStagePool`, `PlaceArchetype.Spots`, `WildernessSpotDefinition.spotId`, `WorldGenerator.GetSpot()`, `ItemYieldRoller` rare pools, `DATA_SCHEMA.md` |
| 39 | [x] | Multi-Station Property Discovery | Station-ness became stage data: `BuildingStageDef` gained `stationName` + `biasPropertyIds` — a pool's final stage with non-empty bias makes the fully-developed building a station (attend opens the working view instead of linger, free/one-shot). Workshop converted from bespoke `WorkshopAttendable` (deleted) to a scene-placed `BuildingAttendable` with single-stage `workshop_pool`; Fen Shrine = new third bog-pool stage `bog_fen_shrine` (Clay); Hearth = existing `sacred_grove_hearth` stage, station-ized (building renamed Consecrated Hearth on completion). `WorkshopUI` generalized to `IWorkStation` (defined in `Mossmark.Inventory`); recipe resolution and property discovery (success *and* failure) filter to the open station's bias. Coverage: all 8 properties at exactly 2 stations; 2 new property-keyed recipes (Sealing Daub: turns_water+holds_the_cold; Warding Charm: draws_the_eye+binds_fast) + 2 new items give turns_water/holds_the_cold/draws_the_eye their first recipes | `BuildingStageDef.stationName`/`.biasPropertyIds`, `IWorkStation`, `BuildingAttendable` station branch, `WorkshopUI.IsRecipeInStationBias()`, `workshop_pool`, `bog_fen_shrine`, `recipe_sealing_daub`, `recipe_warding_charm` |

Update this table as each iteration lands, the same way CLAUDE.md's System Overview table tracked PROTOTYPE2.md's iterations.

---

## Open Threads (Captured, Not Yet Scoped)

Carried forward from PROTOTYPE2.md's deferred section and IDEAS.md. Not in iteration-ready form — listed here so they aren't lost, to be scoped individually as their turn comes.

- **Wilderness tendedness long-game**: spot-to-spot relationships (e.g. a well-tended Bee Skep improving an adjacent Bramble Patch's yield range), authored per spot-type pair via the `WorldContext`/`IOutcomeModifier` pattern, discoverable only through spatial attention to the wilderness layout — no UI hint.
- **Ritual Manipulation View**: a distinct close-up interaction mode for shrine/cairn/threshold-stone-class objects. Item placement and rune/stone arrangement within a small hand-authored spatial canvas, hashed into a deferred set of `IOutcomeModifier` additions affecting outcomes several rests later. Deliberately uncertain causality — "tried, not chosen" pushed into a domain where even the nature of the outcome is unclear. Architecturally significant (new input mode, spatial canvas component, hash-to-modifier function) — treat as its own multi-iteration arc, not a quick add.
- **Full maintenance economy**: extends Iteration 29's maintenance model so items have multiple demand contexts across a settlement's lifetime rather than one (e.g. clay as Smithy fuel, Bog Keeper drainage marker, and settler offering — never all three demands surfaced at once). Trade/surplus as a layer on top: surplus enabling traveling-merchant visits, shrine offerings, or better wandering-thing odds, without becoming a priced market.
- **Population dynamics**: extends Iteration 30's single-trigger arrival model toward something with its own ongoing rhythm — departures, generational change, or population pressure as a force the player responds to rather than only welcomes.
- **Organic outcomes audit**: the "Organic Over Deterministic" principle was never retroactively applied to existing systems. NPC drift thresholds, building stage costs, and tended-spot rest counts are current candidates for deterministic-shape-where-probabilistic-would-feel-better. Audit pass, not a single iteration — likely produces a list of small `IOutcomeModifier` additions rather than one big change.
- **Inter-region / inter-town travel and the macro loop**: how the player moves between settlements, what "progress" means at that scale, and how to keep the macro loop emphasizing differentiation (settlements diverging) rather than advancement (settlements climbing one ladder). Deliberately not scoped yet — needs its own dedicated design pass, separate from the ecosystem-influence work above. Town-level "technology"/knowledge transfer is architecturally distinct from NPC/building specialization (see CLAUDE.md) and that distinction should hold here too.

---

## Integration Rules (Carried Forward)

Unchanged from PROTOTYPE2.md — still apply:

- Systems communicate via **events**, not direct references where possible.
- Managers are **singletons** — one per system.
- Game logic lives in **managers and components**, never in UI classes.
- Each iteration must leave all previous systems functional.
- Prefer data-driven (ScriptableObject-authored) extension over hardcoded per-case branching — but per the Iteration 31 scope above, a single-target pilot is allowed to hardcode first and generalize only after the feeling is validated.
