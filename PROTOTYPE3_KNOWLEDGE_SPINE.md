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

## After this

If 3.4 lands: the next honest tests are a second taught property (does compounding knowledge stay legible or turn muddy) and seeding partial knowledge across more than one entity at once — the direct test of "settlements already know some things" from the IDEAS.md entry. Neither is scoped here. Decide after playing 3.4, not before — same rule that's held for every iteration so far.

---
