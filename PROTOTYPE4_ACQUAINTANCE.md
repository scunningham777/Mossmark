# Mossmark — Prototype 4: Acquaintance

> New scene, built alongside `Greybox.unity` and `Prototype3.unity`, not on top of either. Tests whether "getting to know a world that was already going on without you" is a legible, wantable moment-to-moment loop — before any of it is wired back into Greybox's living systems.

---

## Premise

P3 answered the knowledge spine's atomic claim: teaching an entity a property it didn't know changes what it does, and that change is worth choosing under real scarcity. What P3 deliberately never tested — because it would have muddied that one claim — is the moment *before* teaching: arriving somewhere that already has a history, and not knowing anything about it yet.

Right now in Greybox, a settlement's NPCs, buildings, and wilderness spots are all either fully legible on sight (a triangle is a triangle, its tooltip tells you what it needs) or gated behind development thresholds the player is actively driving (Standing, stage progress). There's no phase where the player is *ignorant of a thing that already has a state* — every entity's current condition is either plainly shown or the player's own doing. P4 tests the missing piece: a settlement seeded at scene load with its own history — NPCs mid-relationship with their crafts, buildings in whatever repair they're in, spots already Familiar or not — and the player's only initial access to any of it is attention itself, spent not to change anything but to *find out* what's there.

This is explicitly the "exploration playthrough" Sean described wanting to actually play: arrive in a region, and the first thing to do is look — not act. Concretely, per the 7-16-26 conversation this doc is drawn from: attending something unknown for the first time yields a vague read only (silhouette-level: "a dwelling, tended by someone" rather than "the Dyer's house, she wants binds_fast"). Continued attention — separate from P3's teaching hold — deepens that read toward its true current state, on a track of its own, with **zero effect on the entity itself**. Trust/acquaintance is one meter reused for NPCs, buildings, and wilderness sites alike, re-skinned per entity type rather than three separate systems. Items fold in via P3's existing knowledge-as-currency system once the acquaintance layer proves out, but transferring knowledge to *other* entities (P3's teach loop) is explicitly out of scope until then — P4 stays one-directional: world → player, never player → world.

**The atomic claim under test:** does deepening acquaintance with an already-alive entity — through attention alone, producing zero change in the entity — feel like a wantable activity in its own right, distinct from both P2's development-by-tending and P3's teaching?

If this doesn't hold up in isolation, wiring it into a full settlement (multiple sites, procedural archetypes, item knowledge, eventual teaching) would be building on an untested foundation — the exact shape the Course Correction and P3's own premise both exist to avoid repeating.

---

## Reuse Discipline

Same hard rule as P3: nothing built for Prototype 4 may modify a shared script's existing behavior, or mutate a ScriptableObject asset instance `Greybox.unity` or `Prototype3.unity` depend on. New behavior comes from new components, new SO assets, or new conditions/interfaces. Prefer a new subclass over modifying a base class if one genuinely needs to change.

**Standing regression gate, every iteration:** before an iteration in this doc is called done, open both `Greybox.unity` and `Prototype3.unity`, confirm each still loads and plays cleanly, 0 console errors.

**Reused directly, unmodified:**
- `AttentionManager` / `AttendableDetector` / `IAttendable` — the input layer doesn't change
- `PropertyRegistry` / `PropertyDefinition` / `PropertyKnowledge` / `WorldContext.IsPropertyKnown()` — P4's item-knowledge phase (4.5+) is pure P3 reuse, not a new system
- `EntityFeedback` — pulse / stage-cross pop / halo vocabulary; acquaintance deepening should feel like this, not like a new visual language
- `DevelopableEntity` / `DevelopmentStage` / `DevelopmentTrack` / `IDependencyCondition` — the generic resolver. **Acquaintance is a `DevelopmentTrack`**, same machinery as a building's repair stages or a spot's Standing — just gated on attention-count-with-zero-world-effect instead of progress-with-effect. This is the single largest reuse decision in this doc; see Iteration 4.2.
- `EntityFeedback`'s tint/shape vocabulary for acquaintance depth, mirroring how Standing and stage-crosses already read
- `PlaceArchetype` / `WorldGenerator`'s site-clustering and member-spot-pool machinery (Iterations 47, 51) — reused once P4 moves from one hand-placed site to several (Iteration 4.6+), not before
- `WorldSite` — the site-scoped container P2/Greybox already uses for standing and spot clustering

**Deliberately absent, at least through Iteration 4.5:**
- `WorldGenerator`'s full procedural region generation — P4 hand-places its first site rather than proc-gens one, same discipline P3 used for its single Dyer
- P3's teach interaction (`KnowingEntityAttendable.TeachableWant`) — acquaintance is read-only; nothing the player does in P4 changes an entity's state
- Item quantity, stacking, carry-weight — same as P3
- Multiple sites / multiple archetypes — single site until the atomic claim is proven (4.1–4.4), then generalized (4.5+)
- Any UI beyond overlay text and `EntityFeedback` — no journal, no codex, no map. If the "getting to know a place" loop needs a way to review what's been learned, that's a candidate for its own later iteration (mirrors P3's 3.9), not assumed here.

---

## Core loop under test

Arrive at an unfamiliar site → attend something (NPC, building, or spot) for the first time → get a vague, silhouette-level read → attend it again (and again) → the read deepens toward its true current state → nothing about the entity has changed, only what the player knows of it.

---

## Iterations

### Iteration 4.1 — Scene Scaffold + One Seeded Entity, Two-State Read

New scene (`Prototype4.unity`), alongside `Greybox.unity`/`Prototype3.unity`. One hand-placed entity — reuse `NpcAttendable` or a trimmed stand-in, whichever needs less new code — seeded at spawn already mid-relationship with its craft (same seeding pattern as P3's Dyer / Iteration 49's mid-process-start), so it has a real "current state" to be unknown about. No acquaintance track yet — just two hardcoded overlay reads, swapped by a bool: `Unfamiliar` (vague silhouette-level description) and `Known` (the entity's actual current state, description and tint both). Player starts `Unfamiliar`; flipping the bool via a debug menu item (mirroring `Prototype3Debug.cs`'s pattern) is the only way to see `Known` for now.

**Explicitly out of scope:** any real acquaintance mechanic, any attention-driven progression, buildings or spots, a second entity.

**Success criterion:** the `Unfamiliar` read is genuinely vague — it should not leak the entity's specific craft, name, or want — and the contrast with `Known` is felt, not just textually different. If `Unfamiliar` already tells you too much, the whole premise of this prototype collapses before it starts; get this contrast right before anything else.

---

### Iteration 4.2 — Acquaintance as a DevelopmentTrack

Wire the 4.1 entity's familiarity to a real `DevelopmentTrack` — reusing the exact machinery a building's repair stages use, gated on a new `IDependencyCondition` (`AttentionCountCondition` or similar: N completed attends, no other requirement, closest existing analog is `TimeCondition`'s "needs more time" shape from P2's design but scoped to attends-at-this-entity specifically, not global time). Two stages only for this iteration: `Unfamiliar → Acquainted`, mirroring 4.1's two-state read but now reached through real attention rather than a debug flip. Attending the entity when `Unfamiliar` costs daylight (reuse `DayCycleManager`, same as P3) and produces **zero effect on the entity itself** — `OnAttentionComplete()` advances the track and nothing else; no yield, no stage-cross on the entity's own function, no bias shift. This zero-effect property is load-bearing and must be checked explicitly, not assumed: log or otherwise verify that nothing about the entity's actual `DevelopmentStage`/behavior track changes as a side effect of acquaintance progress.

**Explicitly out of scope:** a third stage; a second entity; buildings or spots; any UI beyond the overlay text swap already built in 4.1.

**Success criterion — the actual go/no-go:** cold scene load → entity reads as `Unfamiliar` → attend it across however many ticks the track requires → it flips to `Acquainted`, description and tint both updating → confirm via log that the entity's own functional state never moved. Judge whether *finding out* what something is felt like an activity worth doing, with literally nothing else happening.

---

### Iteration 4.3 — Deepening: Three-Stage Track, One Entity Type

If 4.2 holds, extend the same entity's track to three stages instead of two: `Unfamiliar → Acquainted → Known` (naming is placeholder — pick whatever folk-phrase vocabulary fits the "getting to know someone" register, same discipline as P3's property phrases). Each stage's overlay read should be a strictly richer description of the *same underlying true state* seeded in 4.1 — not new information being invented per stage, but the existing truth becoming progressively less obscured. This is the test of whether a single reveal-by-attention track can carry more than one meaningful step, since P4's later generalization to buildings/spots depends on the track holding up past a single threshold.

**Explicitly out of scope:** a second entity; any interaction beyond attending; any change to what "true state" means for this entity (it was fixed in 4.1 and stays fixed).

**Success criterion:** does the middle stage (`Acquainted`) read as a real, distinct waypoint — not just "less vague" but its own legible state — or does the track feel like padding between two meaningful poles? If it's padding, two stages (4.2's shape) may be the right grain for acquaintance generally, and that's a real finding, not a failure.

---

### Iteration 4.4 — Second Entity Type: A Building

Add one hand-placed building (reuse `BuildingAttendable` directly, seeded with an existing repair stage already applied — it should look, on `Known`, like something with history, same as the Dyer's steeping pit read in P3). Give it its own acquaintance track via the same `AttentionCountCondition` mechanism from 4.2 — this is the reuse-across-entity-type test, confirming acquaintance isn't accidentally coupled to `NpcAttendable` specifically. Re-skin the meter's presentation for a building vs. a person if the language genuinely calls for it (e.g. "how well you know this place" vs. "how well you know them") — but confirm first whether re-skinning is actually needed or whether one vocabulary covers both, since the premise document explicitly floats reusing one meter type across NPCs and structures.

**Explicitly out of scope:** wilderness spots (4.5 tests the third type); a second building; any interaction between the NPC's and the building's acquaintance tracks (they should be fully independent).

**Success criterion:** confirm the acquaintance track is genuinely generic — same condition type, same track shape, working unmodified on a `BuildingAttendable` as it did on the `NpcAttendable`. If building the second entity type required new acquaintance-specific code, that's a signal the 4.2 abstraction was fit to one entity type rather than truly generic, and needs revisiting before a third type or a full site.

---

### Iteration 4.5 — Third Entity Type: A Wilderness Spot, and the Site as a Whole

Add one hand-placed `DevelopingWildernessSpotAttendable` (or a trimmed stand-in), same acquaintance track. With all three entity types now carrying the mechanism, assess the *site* as a felt whole for the first time — three unfamiliar things in one place, discoverable in any order, no track requiring another's completion. This is also the point where item-knowledge (pure P3 reuse — `PropertyRegistry`/`PropertyKnowledge`, an item pickup or two, auto-reveal on take) folds back in, since items were the third pillar named alongside NPCs/buildings/spots in the premise and P3 already proved this system works; P4 doesn't re-test it, just places it in the same scene as further texture, not as this iteration's success criterion.

**Explicitly out of scope:** teaching anything to anything (P3's `TeachableWant` stays excluded); a second site; procedural placement of any kind; item quantity or stacking beyond what P3 already established.

**Success criterion — the site-level go/no-go:** cold load into the site → without being told where to look, does the player naturally start attending things to find out what they are, the way they'd explore in the kind of game Sean described wanting to play? Does the site read as a place with its own life already in progress, or as a checklist of three identical "attend me 3 times" objects wearing different sprites? If the latter, the per-entity read (4.1's vagueness-to-clarity contrast) needs more differentiation before generalizing further — the fix is content/writing, not mechanism, per this doc's premise-vs-mechanics discipline.

---

### Iteration 4.6 — Generalizing to a Second, Differently-Themed Site

Only after 4.5 lands: a second hand-placed site with a different archetype flavor (reuse `PlaceArchetype`'s existing shape if convenient, but this can still be hand-authored rather than proc-gen'd — the test here is content variety, not procedural generation). Confirms the vague-read-to-known contrast (4.1's core finding) holds up when the *kind* of thing being obscured varies — a site built around a different trade or terrain reads with a genuinely different "unfamiliar" voice, not a reskinned copy of the first site's phrasing.

**Explicitly out of scope:** procedural site placement or count (`WorldGenerator`'s clustering/member-pool machinery stays parked until this iteration proves two hand-placed sites are worth generalizing); any cross-site mechanic (visiting one site should have zero effect on the other's acquaintance state).

**Success criterion:** does a player who's already learned the first site's rhythm find the second site's unfamiliarity genuinely unfamiliar — or does the acquaintance mechanism itself become the thing being learned, after which every subsequent site is trivially the same task? If the latter, that's an argument for the *content* differentiation (archetype flavor, entity mix, vague-read writing) mattering more than any further mechanical iteration here — worth a note for whenever this reframe considers `WorldGenerator`'s procedural rollout.

---

## Build Notes (7-17-26)

All six iterations (4.1–4.6) are built in `Assets/Game/Scenes/Prototype4.unity`. New code is confined to `Assets/Game/Scripts/Prototype4/` (`Mossmark.Prototype4`: `AcquaintableAttendable`, `AttentionCountCondition`) plus an Editor-only test driver (`Assets/Editor/Prototype4Debug.cs`) — no shared script was modified, no Greybox or Prototype3 asset touched.

**Verification status:** all six iterations verified in Play Mode via MCP, 0 console errors/warnings throughout:
- **4.1/4.2** — full go/no-go sequences run per-iteration (details below), Greybox + Prototype3 regression gates clean after 4.1.
- **4.3** — cold load → Netmender Unfamiliar → one held attend (2 ticks) crossed to *Acquainted* with the authored middle read ("A mender of nets — traps too…", mid tint, no seeded-knowledge phrase yet) → a second held attend (3 ticks) crossed to *Known* (full history read + "They speak of what binds fast", green tint). The 2+3 ladder consumed exactly one 5-tick day. Subject fingerprint unchanged end-to-end; all other entities' tracks untouched (independence confirmed).
- **4.4** — the Smokehouse crossed *Acquainted* through real held attend ticks (the identical attention path, on a Structure instance, zero new code) and read correctly at *Known* including the structure-voice lead ("The place is given to what burns slow").
- **4.5** — Osier Bed crossed via a real attend + debug advance, Known read correct with the place-voice lead; A Cut Withy taken → `binds_fast`, `split_prone` revealed (P3 pickup path working unmodified in this scene).
- **4.6** — Collier crossed via real attends + debug advance; Bothy and Hearth Ring advanced via the debug driver (which runs the same `TryApplyStage` path plus the zero-effect check) — all three Known reads correct in the second site's voice; A Char Knot taken → `burns_slow` revealed.
- **Regression gates after the full build:** Greybox and Prototype3 both re-run in Play Mode after all six iterations landed — 0 errors, 0 warnings each (Greybox's wandering-thing spawner firing normally; the Dyer's seeded state intact). The gates are clean; the iterations are landed.

**The architecture decision, and the one deviation from this doc's letter:** acquaintance is one generic component, `AcquaintableAttendable : DevelopableEntity, IAttendable`, whose `DevelopmentTrack` *is* the acquaintance track — `AttentionCountCondition` is always satisfied (TimeCondition's shape), each stage's `attendsCost` is its `ProgressCost`, and one completed attend adds one progress. NPC, building, and spot are the *same component with different authored data*; there is no per-entity-type subclass. This deviates from Iteration 4.4's "reuse `BuildingAttendable` directly," deliberately:
- `BuildingAttendable` cannot be seeded mid-repair without shared-script changes (stage application only flows through the resolver plus inventory material consumption, and this scene deliberately has no `InventoryManager`).
- Attending a live `BuildingAttendable` inherently mutates it (maintain → develop → open chain), which contradicts P4's zero-effect rule — every path would have had to be bypassed, leaving only its serialized fields "reused."
- Two `IAttendable`s on one GameObject is component-order-fragile (`AttendableZone.Awake` takes the first match).

This is the same call P3 made with `KnowingEntityAttendable` over `NpcAttendable`, and this doc's own Reuse Discipline ("prefer a new subclass over modifying a base") points the same way. 4.4's success criterion translates intact: the building (and later the spot, and the whole second site) had to require **zero acquaintance-specific code** — and did. The mechanism runs entirely on the shared resolver (`DevelopableEntity`/`DevelopmentStage`/`DevelopmentTrack`), which is the reuse the doc actually cares about.

**The zero-effect check (4.2), made explicit:** the subject's own state — `trueStateNote` (its authored functional state) plus its seeded `PropertyKnowledge` entries (keyed by `entityId`, exactly P3's Dyer pattern) — is fingerprinted at `Start()` and re-compared after every attend (`VerifySubjectUnchanged()`, `Debug.LogError` on any drift). Acquaintance progress (`CurrentStageIndex`/`PendingProgress`) is deliberately excluded from the fingerprint: that state belongs to the player's knowing, not to the subject. Verified in the 4.2 play test: fingerprint identical before and after the full Unfamiliar → Acquainted arc.

**4.2 play verification (MCP, 7-17-26):** cold load → entity reads Unfamiliar ("Someone", grey, silhouette line) → one held attend ran three 2s ticks (progress 1, progress 2, stage cross), the stage-crossing tick interrupting the hold per the standard rule → name/tint/description/interaction all flipped ("The Netmender: Acquainted" arriving via the resolver's own notification) → exactly 3 daylight spent (2/5 remaining) → subject fingerprint unchanged → 0 errors, 0 warnings.

**Mechanics that fell out of reuse, worth knowing:**
- Deepening ticks repeat while held (`ContinueAttending` = deepening latch && `CanMakeProgress()`), each costing 1 daylight; the stage-crossing tick ends the hold. Fully-acquainted attends are one-shot flavor visits (also 1 daylight — attention is the day's clock, `RequiresDaylight` constant `true`, P3's reasoning).
- The stage-cross moment reuses `DevelopableEntity.ResolveAttention()`'s existing notification, which reads DisplayName *after* the cross — so the toast is "The Netmender: Acquainted", the name arriving with the crossing. Unplanned, kept.
- `EntityFeedback` (unmodified) gives deepening ticks the progress pulse and stage crossings the pop + one-way triangle→circle swap. The circle therefore means "known to you" here rather than "developed" — the same visual grammar carrying the analogous meaning one layer up; judge in play whether that reads or confuses.
- The overlay's applied-upgrades list shows crossed acquaintance stages ("Acquainted", "Known") in the detail panel — legible, if slightly ledger-like; a candidate for the review-surface question in *After this*.

**The 4.4 re-skin answer:** one vocabulary covers all three entity types because *all* surface language is authored per instance (unfamiliar lines, per-stage reads, interaction lines, flavors). Exactly one line of code was person-shaped — the seeded-knowledge lead ("They speak of…") — now an authored field (`seededKnowledgeLead`: "The place is given to" on the Smokehouse, "The willows here are known for" on the Osier Bed, etc.). No kind enum, no meter re-skin code.

**Stage naming:** uniform placeholder pair "Acquainted" / "Known" across all entities (stage *display names* only — every read is per-instance). Folk-phrase variants per entity type were considered and deferred: the reads carry the differentiation, and 4.5's checklist-feel criterion is the right place to judge whether the stage names themselves need voice.

**Content:** Site 1 is a river landing — the Netmender (was the ferryman; three households eat from her traps; hands slowing, knots unlearned), the Smokehouse (re-raised on its own timbers after a flood; fired every autumn run), the Osier Bed (coppiced on a three-year turn for the netmender's withies), plus A Cut Withy (`binds_fast`, `split_prone`) and An Alder Billet (`burns_slow`) as auto-reveal pickups (`PropertyPickupAttendable`, P3's script unmodified — the sanctioned "pure P3 reuse" of 4.5). Site 2 (4.6) is a colliers' hearth on the slope east — the Collier (burns alone since their partner died; counts burns in nights), the Colliers' Bothy (two beds, one slept in), the Hearth Ring (generations of char; the coppice around it is the age it is because of the ring), plus A Char Knot (`burns_slow`). The two sites deliberately share no seeded property between their NPCs' crafts' registers (water/withies/binding vs. slope/char/slow fire) so the "different unfamiliar voice" criterion has something real to bite on. Entity acquaintance ladders are 2 + 3 attends everywhere (Unfamiliar → Acquainted → Known); `maxDaylight` stays 5, so fully knowing one entity costs a day's attention and a whole site runs roughly three days.

**Debug drivers** (`Mossmark/Prototype4/*`): teleports to every entity/pickup, Begin/Release Attend (reflection into `AttentionManager`, same as P3), Advance Acquaintance On Nearest Entity (forces the next stage across, runs the zero-effect check), Log Entity State (acquaintance stage, pending attends, subject fingerprint, tint, overlay lines for every `AcquaintableAttendable`), Log Daylight, Log Attention State.

---

## Playtest findings (Sean, 7-17-26)

**Overall verdict: favorite direction yet, even with nothing to *do*.** The per-iteration criteria, answered in play:

- **4.2 (the atomic claim): finding out feels worth doing.** Deepening acquaintance with zero effect on the entity reads as a wantable activity in its own right — the claim this prototype exists to test holds.
- **4.3: the middle stage feels good.** A real waypoint, not padding — three stages are a valid grain for acquaintance, so the later generalization can keep multi-step tracks.
- **4.5/4.6: a little checklist feel, and finding-out gets slightly grindy under the daylight squeeze** — but diagnosed as a *tradeoff* problem, not a mechanism problem: aside from other acquaintance tracks and a few pickups, there's nothing else to spend daylight on, so the scarcity has no bite. This is exactly P3's 3.5 lesson resurfacing (scarcity only reads as choice when a real competing use exists). With competing activities the loop is expected to get *more* compelling, not less.

**Direction set for results-of-knowing:** unlocking the flavor is a real reward on its own and must stay the *direct* one — bolting a payout onto every reveal would tip the loop back toward the vending-machine feel P2 broke from, putting focus on the payout to the detriment of the flavor. Results should instead be **subtle and indirect, in P2's "felt, not read" register**: e.g. acquaintance quietly unlocking access to other spots, or adding properties to a working surface's bias — noticed by living in the place, never announced as a reward for the crossing. Not every crossing should pay out; occasional larger consequences (the Flame Sword register) over uniform per-reveal drips.

**Two additions to that direction (same conversation):**
- **Standing-shaped acquaintance for some entities.** Some NPCs (and maybe locations) should cross thresholds not by pure attend quantity but by *repeated attending across multiple days* — the shape of P2's Standing (`SustainedGoodAttentionCondition` / Iteration 43), aimed at trust instead of a spot's development. Adds temperament differentiation (a wary entity warms by days, an open one by hours), gives rest and the daylight cycle real weight in the acquaintance loop, and structurally prevents brute-forcing a whole site in one sitting — which also eats into the grind texture. Pairs with the organic-crossing note below: the two together make "when it crosses" something felt rather than counted.
- **The third phase of one or more sites as a workshop.** At full acquaintance, a site entity (the Smokehouse is the obvious candidate) opens as a working surface — "I've developed enough trust to be allowed to work here." A more direct reward than a quiet flag, but still earned rather than dispensed, and it doubles as exactly the *competing daylight use* the checklist finding calls for: discovery ticks at the earned surface compete with acquaintance ticks everywhere else. One large earned consequence (Flame Sword register), not a per-reveal drip.

**Also flagged for the next iteration:** current crossings are exact attend counts (`attendsCost`), which invites counting — "two more and it flips" reads as grind. The Organic value's pending audit applies here directly: a rising chance of crossing per (qualifying) tick, instead of a fixed count, makes recognition feel like it arrives rather than like a meter filled.

---

## After this

If 4.1–4.5 hold, P4 will have shown that "getting to know an already-alive place" is a real, attention-spending activity distinct from both development-by-tending (Greybox) and teaching (P3) — the missing first act of the playthrough described in the 7-16-26 conversation this doc is drawn from. What's still explicitly unbuilt at that point, in rough order of what would come next:

- **Indirect results of knowing (next-iteration candidate per the 7-17-26 findings)**: acquaintance crossings quietly changing what the world affords, without a per-reveal payout. The machinery already exists — a `worldStateFlag` on an acquaintance stage would let a crossing set a flag that a `PoiRevealCondition` (Iteration 45's three-tier reveal) or a working surface's bias (the Iteration 39/54 pattern, already reused by P3's Scouring Bench) reads. That's both of Sean's named examples — spot access and bias growth — on proven mechanisms, with the flavor reward left untouched as the direct one. Also the natural place to add the *competing daylight uses* the checklist finding calls for (a weir-shaped landmark, a working surface — both pure reuse).
- **Procedural rollout**: multiple sites, randomized count and archetype mix, using `WorldGenerator`'s existing clustering/member-pool machinery (Iterations 47/51) — parked until 4.6 confirms hand-authored variety is worth generalizing.
- **Reconnecting to P3's teach loop**: once acquaintance and knowledge-as-currency have both been proven independently (P3 and P4 respectively), a later prototype is the natural place to test whether they compose — does deepening acquaintance with the Dyer, say, gate or ease what she'll accept being taught? Not assumed here; a real open question for whenever that reframe is attempted.
- **A review surface**: if 4.5/4.6 playtesting produces the same "wait, what have I learned so far" itch P3's 3.9 answered for taken items, a non-modal acquaintance summary is the obvious next small iteration, same size and shape as `TakenLedgerUI`.
