# Mossmark — Prototype 5: Reveal

> New scene, built alongside `Greybox.unity`, `Prototype3.unity`, and `Prototype4.unity`, not on top of any of them. Tests whether attend-to-reveal generalized across ordinary space — not just pre-flagged entities — produces a wantable moment-to-moment loop, then bridges that loop onto the acquaintance baseline kept from P4.

---

## Premise

P4 answered "does deepening acquaintance with an already-alive entity feel like a wantable activity" — yes, confirmed across 4.1–4.9. What P4 never tested is *where the player's attention goes to find that entity in the first place*. Every P4 (and Greybox, and P3) attendable is pre-flagged and immediately legible as an attendable; everything else in the scene is dead space the player walks through without engaging.

P5 tests the missing piece: what if the field itself responds to attention, not just the entities seeded into it? Approach a patch of ordinary ground, hold attend, and it gains clarity/color/legibility — most of the time revealing nothing but itself, occasionally revealing that a real entity was there all along. The loop this produces, if it holds, isn't a new verb — it's the existing attend verb applied to a much larger surface, so that *finding* the NPC/building/site becomes part of the loop rather than a given.

**The atomic claim under test:** does reveal-by-attend, applied to ordinary space with no entity underneath most of it, feel like a wantable activity on its own — distinct from, and a genuine backdrop to, P4's acquaintance-with-a-known-entity loop?

---

## What carries forward from P4 (the baseline)

Kept, unmodified, as the acquaintance layer P5 builds its backdrop around:

- **Acquaintance as a `DevelopmentTrack`** (4.2/4.3) — attention-count-gated, zero effect on the entity, multi-stage reveal of a fixed true state. This is P5's one seeded NPC's behavior, verbatim.
- **The teach stub** (4.10/4.11) — present as a stub only; not extended, not exercised beyond confirming it still composes.
- **Yield-pool sites, pre-ripeness** (4.12/4.14's shape, not 4.13) — `TendableSpotAttendable` as a condensed multi-item foraging spot, flat `yieldChance`, no ripeness ramp, no character drift.
- **`EntityFeedback`'s existing tint/pulse vocabulary** — reveal should read as a variation on this, not a new visual language.
- **`DayCycleManager`** — daylight as the attend cost, same as every prior prototype.

**Explicitly not carried forward, on purpose:** ripeness (4.13), site `character`/drift (4.15), any chaining/dependency-gated stage (4.16, 4.18–4.20), the wreck fork mechanism (4.17 — good idea, wrong prototype), properties/knowledge transfer beyond the existing stub, procedural generation, the debt-vs-witness dynamic.

---

## Reuse Discipline

Same standing rule. Nothing built for P5 may modify a shared script's existing behavior or mutate an SO instance `Greybox`/`Prototype3`/`Prototype4` depend on. New behavior comes from new components, new conditions, or new interfaces layered on `IAttendable`/`AttendableDetector`. Regression gate every iteration: `Greybox`, `Prototype3`, `Prototype4` all still load and play clean, 0 console errors.

---

## Core loop under test

Walk into a space where nothing is pre-flagged → attend ordinary ground → it gains legibility (color/clarity/detail) with no other payoff most of the time → occasionally, revealed ground turns out to have a real entity underneath, previously invisible → the found entity then runs P4's acquaintance loop as normal.

---

## Iterations

### Iteration 5.1 — One Revealed Patch, No Entity, No Reward

Single hand-placed space (your garden/wooded-area image is the right scale — small enough to fully attend in one sitting). No entities, no items, nothing seeded underneath. The space is authored in a muted/low-clarity visual state; attending any point within it locally brightens/clarifies that point, permanently, with no other effect. Reuse `AttendableDetector`'s targeting shape, but the "target" is a subdivided patch of the space itself (grid or scatter of small invisible attend-zones) rather than a discrete entity.

**Explicitly out of scope:** entities underneath, items, session tracking of coverage, a completion state, cost tuning.

**Success criterion:** does clarifying ordinary ground, with nothing else attached, feel like an activity worth doing — the direct P5 analog of P4.1/4.2's go/no-go? If revealing empty ground isn't wantable on its own, nothing built on top of it will be either.

---

### Iteration 5.2 — Multiple Patches, No Score

Widen 5.1 to a handful of sub-areas within the same space, each with its own local reveal state. No aggregate counter, no percentage-complete, no session summary — the thing to guard against explicitly, per the coverage-optimization risk flagged in discussion: nothing should imply "more revealed is better." If a HUD element is needed at all, it should show *presence* (there's more here) not *progress* (you're at 60%).

**Explicitly out of scope:** any entity, any yield, any cross-patch bias.

**Success criterion:** does wandering the space and revealing what you happen to pass near feel organic, or does the player start systematically covering the grid? If it's the latter, the zone shape/spacing needs rework before 5.3 — a systematic sweep is the coverage trap manifesting in play, not just in theory.

---

### Iteration 5.3 — One Seeded Entity, Found Rather Than Given

Seed one NPC (reuse P4's acquaintance `DevelopmentTrack` verbatim) somewhere inside the 5.2 space — but invisible/unflagged until the local patch around them has been revealed to at least some threshold. Once revealed, the NPC becomes a normal P4-style attendable and runs the existing acquaintance track unmodified.

**Explicitly out of scope:** any interaction between reveal-depth and acquaintance-depth beyond "revealed enough to become attendable"; buildings or sites; daylight-cost scaling (5.5).

**Success criterion:** does finding the NPC this way — stumbled into via ordinary-ground attending rather than walking toward a pre-flagged icon — change how the first acquaintance attend feels? This is the actual bridge claim: reveal-space as backdrop, acquaintance as the thing it's a backdrop *for*.

---

### Iteration 5.4 — One Yield-Pool Site, Same Treatment

Add one `TendableSpotAttendable` (P4.12/4.14 shape, flat yield chance, no ripeness) elsewhere in the same space, gated the same way as 5.3's NPC — invisible until its local patch is sufficiently revealed.

**Explicitly out of scope:** ripeness, character, any content differentiation between the NPC's and the site's reveal thresholds unless a real reason emerges in play.

**Success criterion:** does having two different kinds of found entity (a person to get to know, a place to tend) inside one reveal-space read as a coherent single space, or as two unrelated mechanics stapled into the same clearing?

---

### Iteration 5.5 — Reveal Cost Scales with Acquaintance (Rate, Not Gate)

Once 5.3/5.4 hold: tie the daylight cost of revealing a patch near the NPC/site to that entity's current acquaintance stage — cheaper to reveal nearby ground the better you know what's there, never blocked beforehand. Continuous cost curve, not a threshold unlock — the explicit distinction to hold onto from the P4 postmortem: this is a **rate**, not a **gate**.

**Explicitly out of scope:** any version of this that blocks reveal entirely below a stage; any version that's shown to the player as a number.

**Success criterion:** does familiarity-cheapens-noticing read as its own small payoff for depth, without ever implying a required order or a blocked action?

---

## Explicitly out of scope for all of 5.1–5.5

- Properties, teaching beyond the existing stub, knowledge-as-currency
- Ripeness, site character/drift, any chaining or dependency-gated stage
- The wreck fork mechanism
- Multiple sites, procedural generation
- The debt-vs-witness dynamic, or any player-facing framing of motivation
- Any UI beyond overlay text, `EntityFeedback`, and (if 5.2 needs it) a presence-only indicator

## Why this is the right next pilot set

It isolates the one genuinely new claim — that attention applied to ordinary, entity-less space is itself wantable — before spending anything on how it composes with what's already proven (acquaintance) or what's parked (everything else from the P4 postmortem). If 5.1 doesn't hold, nothing past it is worth building; if it does, 5.3/5.4 are the cheapest possible test of the actual thesis: reveal as backdrop, acquaintance as figure.
