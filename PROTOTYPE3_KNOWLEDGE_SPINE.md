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

New attend-branch: the player can know a property too (minimal single-item pickup — no stacking, no quantity — with its one property auto-revealed on pickup for this pilot; discovery-effort is a separate, already-proven system and deliberately not what's under test here). When the player, knowing a property the 3.2 entity doesn't, attends it, offer a one-shot "teach" resolution — not a hold — that adds the property to the entity's known set. Reuse `EntityFeedback.TriggerPop()` for the moment itself.

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

---

## Build Notes (7-14-26, updated 7-15-26)

All five iterations (3.1–3.5) are built and verified in `Assets/Game/Scenes/Prototype3.unity`, with the Greybox regression gate run clean (0 errors, 0 warnings) after each one. New code is confined to `Assets/Game/Scripts/Prototype3/` (`Mossmark.Prototype3`) plus an Editor-only test driver — no shared script was modified, no Greybox asset touched.

**The content pairing:** The Dyer works a steeping pit that won't hold water. She spawns knowing `draws_the_eye` (warm tint + "They speak of what draws the eye" — the 3.2 "this person has history" read). A Lump of Clay sits across the ground plane; taking it (hold E, no inventory, no quantity) auto-reveals `turns_water`. With that known, attending the Dyer offers "Press E to speak of what turns water" — a zero-duration one-shot, distinct from every hold in the game. Once taught, the Clay-Lined Steeping Pit stage (gated on `KnownPropertyCondition`) becomes reachable: two held ticks and it applies — stage pop, triangle→circle, deepened tint, description flips to "The steeping pit sits dark and full."

**The 3.5 scarcity layer:** `DayCycleManager` is wired in as-is at **maxDaylight 4**, with the Day Cycle HUD, transition fade, and a Bedroll (rest works at zero daylight). Every completed attention in the scene now costs 1 daylight — visits, teaching, development ticks, and taking the clay all draw from the same pool. The competing use is a **Fish Weir** (`LandmarkAttendable`, pure reuse, progress cost 3 — three separate holds): food for the fen, a completion the player would actually want. The arithmetic makes the choice real: weir = 3, teach arc = 3 (+1 for the clay), day = 4 — one or the other, never both. Verified in play: weir + clay spent day 1 to 0/4 at Dusk, the teach *refused to start* at zero daylight (the existing "too late to start that now" gate, unmodified), and day 2 ran teach → develop with 1 daylight to spare.

**Implementation choices worth knowing:**
- Entity knowledge is stored in `PropertyKnowledge` keyed by the entity's own id (`p3_dyer`), and the player's teachable knowledge under `p3_player` — the store is already a flat (subject, property) map, so aiming it at knowers needed zero new store code. Caveat: the backtick `RevealAll()` debug would flood entity knowledge too; that debug hook isn't wired in this scene.
- Teaching is gated on *knowledge*, not possession — the clay is not consumed, and nothing is delivered. `TeachPending` = player knows it, entity doesn't.
- The teach one-shot works by returning `AttentionDuration = 0` while `TeachPending` — `AttentionManager` completes a zero-duration attention on the press frame, unmodified.
- The pre-teach blockage surfaces descriptively in the overlay ("The pit will not hold water; they seem resigned to it.") — the blockage stated, never the remedy.
- Play-mode verification used `Mossmark/Prototype3/*` menu items (teleport + reflection-invoked hold start) since MCP can't press keys; the full cold-load → find → teach → change sequence ran end to end in-engine, and 3.5's two-day weir-vs-teach sequence likewise.
- 3.5 daylight rule: `KnowingEntityAttendable.RequiresDaylight` and `PropertyPickupAttendable.RequiresDaylight` are constant `true` (every completed attention there is productive, so the Greybox `LastAttentionMadeProgress` latch degenerates to `true` — and a constant is safe under both of `AttentionManager`'s reads, the pre-start gate and the post-complete spend). The Fish Weir uses `LandmarkAttendable`'s own latch unchanged. Teaching itself was not touched, per 3.5's out-of-scope line.
- `maxDaylight: 4` on the scene's Day Cycle Manager is the tuning knob if the squeeze feels wrong in play — 5 gives one spare tick of slack, 3 forces the teach arc itself across two days.

**Still open — the actual go/no-go:** whether the 3.4 moment *feels* meaningful, and whether 3.5's choice — teaching at the cost of the weir (or the reverse) — feels like *choosing* rather than sequencing, are judgments only playing can give. Timed path, day one of two: spawn → weir is 3 holds, or clay (~5s walk, 1 hold) → Dyer (~10s) → press E → hold ~4s; whichever you didn't pick waits for tomorrow.

---

## After this

If 3.5 lands: property discovery (Iteration 35/36) is a natural, low-risk next thread to pull in — it already populates the same `PropertyKnowledge` state teaching does, just via a different route (Workshop failure-reveal instead of being taught). Discover a property at a Workshop, then go teach it somewhere — same currency, second faucet, not a merge risk.

Delivery-driven upgrades (buildings/NPCs progressing via carried items) are a different matter — the actual rival mechanic to teaching, not a candidate for folding in yet. Putting both in the same scene before either is separately proven risks the player defaulting to whichever is more familiar (probably delivery) and quietly starving the other of a fair test. Keep them apart until teaching has survived 3.5 on its own merits; only then is it worth deliberately testing whether the two coexist as real alternatives or whether one should replace the other.

Beyond that: a second taught property (does compounding knowledge stay legible or turn muddy) and seeding partial knowledge across more than one entity at once — the direct test of "settlements already know some things" from the IDEAS.md entry. None of this is scoped yet. Decide after playing 3.5, not before — same rule that's held for every iteration so far.

---
