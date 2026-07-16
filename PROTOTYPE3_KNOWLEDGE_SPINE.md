# Mossmark — Prototype 3: The Knowledge Spine
> New scene, built alongside Greybox.unity, not on top of it. Tests one claim before any more of the 7-14-26 IDEAS.md entry gets built toward.

---

## Premise

IDEAS.md's "Wilderness Loop → Living Economy" entry (7-14-26) sketched a much larger reframe than anything tested so far: items as elements to *know about* rather than quantities to stockpile, settlements that already hold partial knowledge, productivity shifting because the player *taught* a place something rather than *delivered* something to it. That entry was explicitly flagged as argued, not tested — same shape as the Druid/Provenance/Kinship package the original Course Correction shelved.

Prototype 3 exists to test the single atomic claim underneath that whole reframe, in isolation, as fast as possible: **does teaching an entity a property it didn't know change what it does, in a way that feels meaningful with zero item quantity or delivery involved?**

Everything else in the 7-14-26 entry — the catalog, settlement-seeded knowledge at scale, attention as the sole lever — either already has a validated analog elsewhere (Iteration 50's dominance model) or is downstream of this one claim being true at all. This prototype is scoped to answer that one question, nothing more, per the same single-pilot discipline used throughout Greybox — a new scene buys setup speed, not permission to test more at once.

---

## Reuse Discipline

Prototype 3 is a **new scene** referencing the existing project's shared scripts directly — not a duplicated codebase, not a fork of `Greybox.unity`.

**Hard rule:** nothing built for Prototype 3 may modify a shared script's existing behavior, or mutate a ScriptableObject asset instance `Greybox.unity` depends on. New behavior comes from new components, new SO assets, or new conditions/interfaces — never edits to a shared logic path Greybox's `WorldGenerator` or its content assets rely on. If a shared class genuinely needs to change to serve this prototype, prefer a new subclass over modifying the base — the same call `DevelopingWildernessSpotAttendable` made when it couldn't share a base class with `WildernessYieldAttendable`.

**Standing regression gate, every iteration:** before an iteration in this doc is called done, open `Greybox.unity`, confirm it still loads and plays cleanly, 0 console errors. Same weight as the "clean recompile" check used throughout FEATURES.md — this is what makes "reuse aggressively" safe to do quickly instead of cautiously.

**Reused directly, unmodified:**
- `AttentionManager` / `AttendableDetector` / `IAttendable` — the input layer doesn't change
- `DevelopableEntity` / `DevelopmentStage` / `DevelopmentTrack` / `WorldState` — the generic dependency/response resolver
- `PropertyRegistry` / `PropertyDefinition` / `PropertyKnowledge` / `WorldContext.IsPropertyKnown()` — this *is* the knowledge system under test, already built, currently idle
- `EntityFeedback` — pulse / stage-cross pop / halo, the whole "felt, not read" visual vocabulary
- The `biasPropertyIds` station-bias pattern (Iteration 39, reused again in Iteration 54) — the scaffold for "a place whose behavior is shaped by which properties are known"

**Deliberately absent from this scene** (not deleted from the project — just not relevant to this test, and including them would reintroduce the exact "muddied by unrelated mechanics" cost this prototype exists to avoid):
- `WorldGenerator`'s procedural site/archetype/POI generation
- The exhaustion / Standing / `WorldSite` / dominance / flow-reserve thread (Iterations 43–53) — a pacing question, orthogonal to knowledge transfer
- Item quantity, stacking, and carry-weight
- `PropertyAvailableCondition`'s carry-and-consume want path — this is the mechanic under test *against*, it shouldn't coexist in the same scene contradicting the hypothesis

---

## Core loop under test

Attend an entity → it shows what it currently knows → teach it something it doesn't → it visibly does something different because of that, not because anything was delivered to it.

---

## Iterations

### Iteration 3.1 — Scene Scaffold + Reuse Audit

New scene (e.g. `Prototype3.unity`), created alongside `Greybox.unity`. Wire in the minimal reused subset — `AttentionManager`, `PropertyRegistry`/`PropertyKnowledge`, `EntityFeedback`, `DayCycleManager` if needed for a day boundary — and confirm each runs standalone without any `WorldGenerator` dependency. One hand-placed ground plane, one hand-placed player. No content yet — this iteration is purely "does the reused foundation stand up on its own."

**Explicitly out of scope:** any entity, any property content, any procedural placement.

**Success criterion:** empty scene loads, player moves and can hold-attend a placeholder object with no errors — and `Greybox.unity` still opens and plays cleanly afterward, confirming the reuse didn't reach back and touch anything.

---

### Iteration 3.2 — One Entity, Seeded Partial Knowledge

One hardcoded entity (reuse `DevelopableEntity` directly — either a trimmed reuse of an existing NPC/building script or a fresh minimal stand-in, whichever needs less new code) placed in the scene. It starts already "knowing" one property via `PropertyKnowledge`, seeded at spawn — direct reuse of Iteration 49's mid-process-start instinct, aimed at knowledge instead of a progress counter. One authored base-level appearance/behavior tied to that starting knowledge, via `EntityFeedback` (tint or shape, no numbers).

**Explicitly out of scope:** a second entity, any yield or item pickup, any procedural variation.

**Success criterion:** on scene load, the entity visibly reflects that it already knows something — a "this place has history" read, the same test 49 ran for progress, now run for knowledge.

---

### Iteration 3.3 — Teach Interaction

New attend-branch: the player can know a property too (minimal single-item pickup — no stacking, no quantity — with its one property auto-revealed on pickup for this pilot; discovery-effort is a separate, already-proven system and deliberately not what's under test here). When the player, knowing a property the 3.2 entity doesn't, attends it, offer a one-shot "teach" resolution — not a hold — that adds the property to the entity's known set. Reuse `EntityFeedback.TriggerPop()` for the moment itself. *(The "not a hold" zero-duration resolution was superseded 7-16-26 by a short `teachDuration` hold — see Build Notes — after playtesting found it read as instantaneous next to every other attendable's held tick; the interaction is still a one-shot, just no longer immediate.)*

**Explicitly out of scope:** any UI beyond what's needed to present the offer; no discovery mechanic; no second property.

**Success criterion:** does teaching read as a distinct, deliberate action — clearly different from delivering an item — the moment it happens?

---

### Iteration 3.4 — Behavior Branch on Taught Knowledge

One authored branch: once the entity knows the taught property in addition to what it started with, something about it changes — a new `DevelopmentStage` becomes reachable, or its bias (reusing the `biasPropertyIds` pattern from 39/54) shifts toward something new. This is the one thing the entire prototype exists to test; everything in 3.1–3.3 was setup for this moment.

**Explicitly out of scope:** a second branch, a second property, any generalization beyond this one pairing.

**Success criterion — the actual go/no-go:** cold scene load → find or already hold the property → teach the entity → see it do something different, timed end to end. Judge it on whether that final moment feels meaningful with zero numbers on screen. This is the result that determines whether the rest of the 7-14-26 knowledge spine is worth building toward.

---

### Iteration 3.5 — Teaching Under Real Scarcity

3.1-3.4 deliberately excluded the pacing thread (exhaustion/Standing/dominance/reserve, Iterations 43-53) as noise — correctly. But daylight itself isn't part of that thread; it's Prototype 2's actual premise, and 3.1-3.4 left it out too, which means 3.4's result can't yet distinguish "teaching feels meaningful" from "teaching was the only button available, so of course I pressed it." This iteration adds back the one piece of scarcity that was never optional, and nothing else.

Reuse `DayCycleManager`'s daylight pool as-is — already built, already proven in Greybox, no new mechanic. Give the player one more legitimate daylight cost that competes directly with teaching: either a second entity that also wants attention, or a second, different need at the same 3.2 entity. The specific competing use matters less than that it's real — something the player would actually want, not a decoy.

**Explicitly out of scope:** any pacing mechanism beyond the daylight pool itself (no exhaustion, no Standing, no dominance, no reserve); a third entity; any change to how teaching itself works from 3.3/3.4.

**Success criterion:** re-run 3.4's exact end-to-end test, but with daylight now tight enough that teaching and the competing use can't both happen the same day. Does choosing to teach — knowing it costs the alternative — feel different from teaching because there was nothing else to spend attention on? This is the test 3.4 couldn't run on its own: not whether the effect is legible, but whether it's worth choosing.

**Verdict (played 7-15-26): the choice reads.** Teaching at the cost of the weir felt like choosing, not sequencing. That's the green light for the discovery thread below.

---

## The Discovery Thread (planned 7-15-26)

With 3.5 landed, knowledge has a *spend* (teaching, under scarcity) but only one trivial *source* (auto-reveal on pickup). This thread adds the second faucet — property discovery, the already-proven Iteration 35/36 system — so the loop becomes: take things whose nature you don't yet know → work them out → carry that knowledge to someone it changes. Same currency, second faucet.

**Decision, 7-15-26 — what the working surface works over:** P2's Workshop drew its options from the pack (`InventoryManager.Stacks`). P3 has no pack, deliberately. The working surface instead works over **the full list of everything the player has taken** — identities, not quantities. This is not a loophole; it's the 7-14-26 frame stated plainly: what you carry forward from a taken thing is *acquaintance with it*, not a stack of it. A UI for viewing that list outside the workshop is deliberately a later step (3.9).

**Why not reuse `WorkshopUI` directly:** it's coupled to `InventoryManager`/`ConversionDef` (quantity-and-consumption semantics this scene excludes), and a *new* modal UI can't be added cleanly either — the modal input guards (`IsOpen` checks) live hardcoded in shared `PlayerController`/`AttentionManager`, and adding a P3 guard there would break the hard rule. So the surface is **non-modal, attend-driven** — no input capture, no shared-script edits, and truer to Tried, Not Chosen anyway: the player chooses what to take and where to work, never which outcome to have. If the loss of P2's combination-picking texture is felt in play, a selection surface becomes a later iteration with that known cost attached.

---

### Iteration 3.6 — The Taken Ledger

A minimal session store of what the player has taken: item id, display name, property ids — identity only, no counts, nothing consumable. `PropertyPickupAttendable` registers each take. Pickups gain an authorable **auto-reveal flag**: the Lump of Clay keeps its 3.3 auto-reveal (the proven teach path stays intact as the control), but two new pickups — Bark Strips (`binds_fast`, `keeps_well`) and Reeds (`binds_fast`, `split_prone`) — are taken *without* their properties revealing. Their overlay read is the itch discovery answers: identity known, nature not ("There's more to it," same vocabulary as `InventoryUI`).

**Explicitly out of scope:** the working surface itself; any way to learn the unrevealed properties; any teach target for them; any UI listing the ledger.

**Success criterion:** taking a thing whose nature stays unknown reads clearly as "I have this, but I don't know it yet" — the pull toward a place to work it out exists before that place does. Clay/teach/weir loop unchanged.

---

### Iteration 3.7 — Discovery at the Working Surface

One hand-placed working surface (a scouring bench by the water, or similar — new minimal P3 attendable, not `BuildingAttendable`). Attending it works over the full Taken Ledger: each completed hold-tick costs 1 daylight and reveals one not-yet-known property from among the taken things — filtered by the surface's authored **bias** (the Iteration 39 `biasPropertyIds` pattern, reused as data: e.g. `binds_fast`/`keeps_well`, so this bench can teach you what binds and what keeps, and nothing else). Reveals land in `PropertyKnowledge` under both the item id and `p3_player` — exactly where pickup auto-reveal already puts them, so the teach gate needs no changes. Deterministic one-reveal-per-tick for the pilot; the organic pass can probabilize later. When nothing on-bias remains unknown, attending falls back to a flavor linger.

**Explicitly out of scope:** recipes, conversion, item consumption, success/failure states; a second surface; any change to teaching; any modal UI.

**Success criterion:** working out `binds_fast` at the bench — spending scarce daylight that the weir, the Dyer, and the pit all also want — feels like *earning* a piece of knowledge rather than being handed it. The day math should force the same shape of choice 3.5 proved: a discovery tick is a real alternative, not filler.

---

### Iteration 3.8 — Teaching What You Worked Out

Loop closure, and the doc's deferred "second taught property" test in the same stroke. The Dyer gains a second want: `binds_fast` — the mordant. Her colors take now (3.4's pit) but they fade with washing; bark's tannin is the historically real fix. Generalize the teach branch from one authored property to a short authored list (each entry: property, teach line, gated stage — `KnownPropertyCondition` is already generic), and add the second stage ("Colors That Hold" or similar) with its own needs line, tint step, and developed read. Teaching stays one press per property — each transfer its own moment.

**Explicitly out of scope:** a third property; a second entity; any generalization beyond the authored list.

**Success criterion — the thread's go/no-go:** cold load → take bark (nature unknown) → work it out at the bench → teach the Dyer → see the second change, all under the same tight days *(4-tick at the time; retuned to 5 on 7-16-26 — see Build Notes)*. Two questions, judged together in play: does discover-then-teach feel like one loop with knowledge as its currency, and does the Dyer's state stay legible with two taught properties and two applied stages — or does compounding start to turn muddy?

---

### Iteration 3.9 — Seeing What You Carry

The later step flagged in the 7-15-26 decision: a glanceable, **non-modal** HUD strip (the `InventoryUI` pattern — persistent, captures no input) listing taken things with their known phrases in folk language, "There's more to it" for the rest. This is the first small gesture at the 7-14-26 "literal in-world catalog," in HUD form only. Build it when playing 3.7/3.8 actually produces the "wait, what have I taken?" question — not before.

**Explicitly out of scope:** any diegetic/physical catalog object; recording/Witnessed-vs-Recorded; any interaction beyond reading it.

**Success criterion:** you can answer "what am I carrying knowledge of?" at a glance without the display collapsing into a stats screen — phrases, not numbers.

---

## Build Notes (7-14-26, updated 7-15-26, updated 7-15-26 again for the Discovery Thread)

All nine iterations (3.1–3.9) are built and verified in `Assets/Game/Scenes/Prototype3.unity`, with the Greybox regression gate run clean (0 errors, 0 warnings) after each one. New code is confined to `Assets/Game/Scripts/Prototype3/` (`Mossmark.Prototype3`) plus an Editor-only test driver — no shared script was modified, no Greybox asset touched.

**The content pairing:** The Dyer works a steeping pit that won't hold water. She spawns knowing `draws_the_eye` (warm tint + "They speak of what draws the eye" — the 3.2 "this person has history" read). A Lump of Clay sits across the ground plane; taking it (hold E, no inventory, no quantity) auto-reveals `turns_water`. With that known, attending the Dyer offers "Hold E to speak of what turns water" — a short one-shot hold (`teachDuration`, 0.6s as of the 7-16-26 tuning pass below), still distinct from a development tick's longer hold but no longer instantaneous. Once taught, the Clay-Lined Steeping Pit stage (gated on `KnownPropertyCondition`) becomes reachable: two held ticks and it applies — stage pop, triangle→circle, deepened tint, description flips to "The steeping pit sits dark and full."

**The 3.5 scarcity layer:** `DayCycleManager` is wired in as-is, originally at **maxDaylight 4** *(retuned to 5 on 7-16-26 — see the tuning note below)*, with the Day Cycle HUD, transition fade, and a Bedroll (rest works at zero daylight). Every completed attention in the scene now costs 1 daylight — visits, teaching, development ticks, and taking the clay all draw from the same pool. The competing use is a **Fish Weir** (`LandmarkAttendable`, pure reuse, progress cost 3 — three separate holds): food for the fen, a completion the player would actually want. At maxDaylight 4 the arithmetic made the choice absolute: weir = 3, teach arc = 3 (+1 for the clay), day = 4 — one or the other, never both. Verified in play at the time: weir + clay spent day 1 to 0/4 at Dusk, the teach *refused to start* at zero daylight (the existing "too late to start that now" gate, unmodified), and day 2 ran teach → develop with 1 daylight to spare.

**Implementation choices worth knowing:**
- Entity knowledge is stored in `PropertyKnowledge` keyed by the entity's own id (`p3_dyer`), and the player's teachable knowledge under `p3_player` — the store is already a flat (subject, property) map, so aiming it at knowers needed zero new store code. Caveat: the backtick `RevealAll()` debug would flood entity knowledge too; that debug hook isn't wired in this scene.
- Teaching is gated on *knowledge*, not possession — the clay is not consumed, and nothing is delivered. `TeachPending` = player knows it, entity doesn't.
- The teach interaction is still a one-shot (`ContinueAttending` stays `false`) but, as of 7-16-26, runs through `AttentionManager`'s normal hold-progress timer at a short `teachDuration` (0.6s) rather than firing on the press frame — see the tuning note below.
- The pre-teach blockage surfaces descriptively in the overlay ("The pit will not hold water; they seem resigned to it.") — the blockage stated, never the remedy.
- Play-mode verification used `Mossmark/Prototype3/*` menu items (teleport + reflection-invoked hold start) since MCP can't press keys; the full cold-load → find → teach → change sequence ran end to end in-engine, and 3.5's two-day weir-vs-teach sequence likewise.
- 3.5 daylight rule: `KnowingEntityAttendable.RequiresDaylight` and `PropertyPickupAttendable.RequiresDaylight` are constant `true` (every completed attention there is productive, so the Greybox `LastAttentionMadeProgress` latch degenerates to `true` — and a constant is safe under both of `AttentionManager`'s reads, the pre-start gate and the post-complete spend). The Fish Weir uses `LandmarkAttendable`'s own latch unchanged. Teaching itself was not touched, per 3.5's out-of-scope line.
- `maxDaylight` on the scene's Day Cycle Manager is the tuning knob if the squeeze feels wrong in play — now **5** (see below); 4 was the original 3.5 value, 3 would force the teach arc itself across two days.

**3.5 verdict, confirmed in play:** the choice reads — teaching at the cost of the weir felt like choosing, not sequencing.

**The Discovery Thread (3.6–3.8), built and verified 7-15-26:**

- **3.6 (Taken Ledger):** `TakenLedger` is a static, insertion-ordered store (`Mossmark.Prototype3`) — item id, display name, property ids, nothing consumable. `PropertyPickupAttendable` was generalized from a single `propertyId` to a `propertyIds[]` array plus an `autoRevealOnTake` bool. The Lump of Clay keeps `autoRevealOnTake: true` unchanged (the proven 3.3 control path). Two new pickups, Bark Strips (`binds_fast`, `keeps_well`) and Reeds (`binds_fast`, `split_prone`), ship with `autoRevealOnTake: false` — taking them logs "There's more to it than you can say yet." and registers with the ledger, properties left unknown. Verified: `Log Taken Ledger` showed both entries with all properties `unknown` immediately after taking.
- **3.7 (Discovery at the Working Surface):** `WorkingSurfaceAttendable` is a fresh, non-modal `IAttendable` (not `DevelopableEntity` — no dependency resolver needed). Attending it scans `TakenLedger.All` in take-order, and within each entry scans `PropertyIds` in authored order, returning the first property that's (a) in the surface's `biasPropertyIds` and (b) not yet known — one deterministic reveal per completed hold, costing 1 daylight like everything else in the scene. The Scouring Bench is biased to `[binds_fast, keeps_well]`, deliberately excluding `split_prone` — proving the bias filter works, since Reeds' `split_prone` is never revealed by this bench. Verified in play: two attends at the bench revealed `binds_fast` then `keeps_well` on Bark Strips, in that exact order; Reeds stayed fully unknown throughout (its `binds_fast` is tracked *per-item*, separately from Bark's — the existing `PropertyKnowledge` design already handles this correctly with no new code, though it means the same folk-property can need re-discovering per source item, a texture worth noting rather than a bug).
- **3.8 (Teaching What You Worked Out):** `KnowingEntityAttendable` was generalized from one hardcoded `teachablePropertyId`/stage pairing to a `TeachableWant[]` array (a plain `[System.Serializable]` nested class, not `[SerializeReference]` — safe to hand-edit in scene YAML like any other array). The Dyer now has two wants: `turns_water` (unchanged from 3.4) and a new `binds_fast` want ("Colors That Hold" — the historically real fix for dye that fades with washing). Each want independently gates its own `DevelopmentStage` via `KnownPropertyCondition`; `GetPendingWant()` offers whichever pending want comes first in list order, `CurrentDevelopedWant` (looked up via `LastAppliedStage.Id`, DevelopableEntity's own field) drives tint/description/interaction text for whichever stage *most recently* applied. **Deliberately not sequenced**: both stages share the same `PendingProgress` counter, so if both become satisfied before either is affordable, `DevelopableEntity.TryApplyStage()`'s existing random draw among available stages decides which crosses first — the same mechanic Greybox's NPC/building pools already use. Verified in play twice over: cold load → take bark (unrevealed) → work the bench twice (both properties revealed) → take clay (still auto-reveals) → teach `turns_water` then `binds_fast` → develop. In this playthrough the pit crossed first, then Colors That Hold second — confirmed via direct component inspection (`CurrentStageIndex: 1` after both) and by re-reading the full session log in order, not the truncated windows that initially made it look backwards. The display correctly tracked whichever stage was truly most recent throughout.

**A legibility texture to judge in play, not a bug:** between the two stages crossing, the interaction line always shows whichever stage just applied ("Hold E to watch the colors take" / "...hold") — it doesn't say "and there's more to develop" while a second want is still pending. The overlay's upgrades bullet list (`GetAppliedUpgrades()`, already wired to `AttendableOverlayUI`'s detail panel) does show both once both land, so the information isn't hidden, just not in the single-line prompt. Left as-is deliberately: whether this reads as a gap or as "felt, not read" (you find out by continuing to attend, not by being told) is exactly the kind of call 3.8's "does compounding stay legible" success criterion asks the player to make, not the code.

**3.9 (Seeing What You Carry), built 7-16-26:** `TakenLedgerUI` (`Mossmark.Prototype3`) is a persistent, non-modal `UIDocument` HUD strip, positioned top-left exactly like `InventoryUI` (whose corner is free in this scene, since P3 has no pack) — same "push state, capture no input" pattern, reading `TakenLedger.All` instead of `InventoryManager.Stacks`. Each entry renders its display name plus known property phrases (folk language, or `[tag]` under the shared `PropertyKnowledge.ShowDebugTags` toggle); any still-unknown property collapses to a single "There's more to it." line, same vocabulary as `InventoryUI`/`ItemPropertyDisplay`. Refreshes on `TakenLedger.Changed` and `PropertyKnowledge.PropertyRevealed`. Property-line rendering was written fresh rather than reusing the shared `ItemPropertyDisplay` helper (`Mossmark.Inventory`), which is keyed off `ItemDefinition` — `TakenLedger.Entry` only carries `itemId`/`displayName`/`propertyIds` — following the same precedent `WorkingSurfaceAttendable` set in 3.7 by inlining its own phrase lookup rather than touching a Greybox-shared display path. Verified in play: took Bark Strips (both properties unknown) and the Lump of Clay (auto-reveals `turns_water`), worked the bench to reveal `binds_fast` on Bark Strips, and confirmed via `Log Taken Ledger` that ledger state matches what the HUD renders — Clay `turns_water=known`, Bark Strips `binds_fast=known, keeps_well=unknown`. 0 console errors/warnings throughout; Greybox regression gate re-run clean afterward.

**Still open — the actual go/no-go:** whether the 3.4 moment *feels* meaningful, whether 3.5's scarcity choice feels like choosing, and now whether discover-then-teach reads as one loop with knowledge as its currency and stays legible with two taught properties — all judgments only playing can give.

**Playtest findings, 7-16-26 (post-3.8, sitting with these before acting):**
- **Knowledge-as-currency isn't clearly confirmed — and the reason is structural, not a discovery-mechanic failure.** Clay's `turns_water` want still auto-reveals on pickup (kept as 3.4's proven control path), so that half of the Dyer's loop never required the bench at all — only `binds_fast` did. 3.8 only fully tested discover-then-teach on one of the Dyer's two wants; the other bypasses discovery entirely. Adjacent idea surfaced in discussion: properties could also be learned by watching NPCs use items, a third faucet beyond pickup-auto-reveal and bench-discovery — bigger scope, parked for *After this*, not scoped now. Also flagged: discovery currently has no payoff beyond unlocking one teach, which may just be this prototype's small size (one recipient per property) rather than a flaw in discovery itself.
- **Legibility without felt weight.** The overlay's upgrades list does make two-stage state differentiable — that bar is met. But it doesn't *feel* like much, because a taught property changes tint/text on the one entity that received it and propagates nowhere else — no second entity reacts, no new interaction opens. `EntityFeedback`'s pop marks the moment but nothing downstream sustains it. Points at the same root cause as the currency finding: one recipient, no ripple, so taught knowledge has thin stakes almost by construction of the prototype's size.

**Teach duration tuning, 7-16-26:** playtesting flagged the teach interaction as instantaneous — a `Press E`, zero-duration one-shot — which read as inconsistent with every other attendable in the scene, all of which resolve over a held tick. Fixed by giving `KnowingEntityAttendable` a dedicated `teachDuration` field (0.6s, shorter than the 2s `attendDuration` used for visits/development, but no longer zero) and routing `AttentionDuration` through it whenever a want is pending; `AttentionManager`'s existing hold-progress timer handles the rest unmodified. The interaction line changed from "Press E" to "Hold E" to match. Teaching is still a one-shot (`ContinueAttending` stays `false` — it doesn't repeat), just no longer instant. Verified in play: `Log Attention State` mid-hold showed `state=Attending, target=The Dyer, holdProgress=0.25` before the teach completed, for both wants (`turns_water` and `binds_fast`); 0 console errors/warnings; Greybox regression gate re-run clean.

**Daylight retune, 7-16-26:** `maxDaylight` moved from 4 to 5. 3.5's original arithmetic (weir = 3, teach arc = 3+1 for the clay, day = 4) was tuned before the Discovery Thread existed — with the bench, the ledger, and a second Dyer want now also competing for the same pool, 4 stopped feeling like a real scarcity choice and started feeling like an arbitrary wall: there usually wasn't enough daylight left in a day to do more than one substantial thing regardless of which one, which reads as restrictive rather than as a meaningful trade-off. 5 restores one spare tick of slack — enough that a day can hold, say, a discovery tick *and* a teach, or the weir *and* a take, while still not enough for everything, so the choice stays real without feeling arbitrarily starved. This is a judgment call, not a re-run of 3.5's own success criterion — worth re-verifying in play if the squeeze still feels off with the full Discovery Thread loop in the mix.

---

## After this

Property discovery is now scoped as the Discovery Thread above (3.6–3.9), and the "second taught property" question is folded into 3.8's success criterion rather than waiting as its own test.

Delivery-driven upgrades (buildings/NPCs progressing via carried items) remain the actual rival mechanic to teaching, and still not a candidate for folding in. Putting both in the same scene before either is separately proven risks the player defaulting to whichever is more familiar (probably delivery) and quietly starving the other of a fair test. Only after the Discovery Thread has been played is it worth deliberately testing whether the two coexist as real alternatives or whether one should replace the other.

Beyond that: seeding partial knowledge across more than one entity at once — the direct test of "settlements already know some things" from the IDEAS.md entry — and, if 3.7's choiceless surface leaves the P2 combination-picking texture genuinely missed, a selection surface with the input-guard cost named in the thread preamble. None of this is scoped. Decide after playing 3.8, not before — same rule that's held for every iteration so far.

---
