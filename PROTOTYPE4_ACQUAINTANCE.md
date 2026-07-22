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

## The Results Thread (planned 7-17-26)

Scoped from the 7-17-26 playtest findings above. The three iterations attack the two named problems — countable crossings and tradeoff-free daylight — and add the first *results of knowing*, keeping the flavor reveal as the only direct reward. Same discipline as every prior thread: no shared-script behavior changes, new behavior in `Mossmark.Prototype4` components and scene data.

### Iteration 4.7 — Organic Crossings: Felt, Not Counted

Replace the exact `attendsCost` crossing with the Organic value's own prescription: each stage authors a `minAttends` floor (no crossing possible before it — the authored pacing bound stays) and a `ripenChance` that ramps per qualifying attend past the floor, so the crossing *arrives* somewhere in a band rather than landing on a countable tick. The crossing itself still runs through the shared resolver (`TryApplyStage`, `OnDeveloped`, `EntityFeedback` pop, tint swap) — only *when* it fires becomes probabilistic, and gates stay exact per the Design Values (Gates can be exact; outcomes shouldn't be).

**Explicitly out of scope:** any change to what the stages reveal; any new entity; day-gating (that's 4.8).

**Success criterion:** across several run-throughs the same stage crosses on different ticks, always at or past its floor — and holding attention no longer feels like watching a counter, because there is no counter to watch.

---

### Iteration 4.8 — Temperament: Trust That Keeps Its Own Time

Some entities gate acquaintance the way P2's Standing gates a spot: by *returning across days*, not by quantity in a sitting. A wary entity accepts one qualifying attend per day (tracked against `DayCycleManager.DayIndex` — per-entity, so no `WorldSite` needed); further attends the same day resolve as flavor visits, with an authored sated line surfacing descriptively in the overlay ("They've said what they'll say today"), never as an instruction. The Collier (who counts everything in nights) and the Osier Bed (a place that only shows itself in returns) become wary; the river-landing entities stay sitting-gated — temperament expressed through the gate type itself.

**Explicitly out of scope:** site-scoped aggregation (`WorldSite` stays parked); any penalty for over-attending (extra visits are visits, not scolding); more than two wary entities.

**Success criterion:** getting to know the Collier *cannot* be brute-forced in a day and therefore threads through multiple rests — and that reads as who the Collier is, not as a timer. Rest/daylight acquire real weight in the acquaintance loop.

---

### Iteration 4.9 — The Earned Workshop: The Smoking Racks

The first result-of-knowing, and the competing daylight use the checklist finding calls for. A new hand-placed working surface (The Smoking Racks, beside the Smokehouse) is present from cold load but not usable — its overlay reads as another's workplace. The Smokehouse's `Known` crossing quietly sets a WorldState flag (`worldStateFlag`, new per-stage data); the racks' `CanAttend()` reads it. Nothing is announced — you find out the racks will have you by coming back to them, felt-not-read. The surface itself is P3's 3.7 shape rebuilt as a P4 component (non-modal, works over the `TakenLedger`, bias-filtered, one reveal per held tick, 1 daylight each): bias `burns_slow`/`keeps_well`, plus `binds_fast` unioned in only while the Osier Bed is Known (the Iteration 54 additive-seam pattern, keyed on the osier's own crossing flag) — knowing the bed teaches the racks' work a second language. To give discovery something to answer, the site's pickups stop auto-revealing (Withy, Alder Billet, Char Knot go "there's more to it") and a fourth unrevealed pickup (A Smoked Eel, `keeps_well`) hangs forgotten by the smokehouse. The Withy's `split_prone` stays off-bias everywhere in this scene — a deliberately unanswerable itch, per the vending-machine guard: not every question gets a dispenser.

**Explicitly out of scope:** any modal UI; recipes/conversion/consumption; a second surface; per-reveal rewards of any kind beyond the reveal itself; teaching (still excluded).

**Success criterion — the thread's go/no-go:** cold load → the racks read as not-yours → earn the Smokehouse to Known → the racks will have you, and it feels earned rather than dispensed ("I've developed enough trust to be allowed to work here") → discovery ticks at the racks now genuinely compete with acquaintance ticks elsewhere for the same daylight. Judge whether the checklist/grind feel from the 7-17-26 playtest recedes once the day has real alternatives.

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

## Results Thread Build Notes (7-17-26, same day as the thread was scoped)

All three iterations (4.7–4.9) are built and verified in Play Mode via MCP, 0 errors/warnings, with the Greybox + Prototype3 regression gates re-run clean afterward. New code stays in `Mossmark.Prototype4` (`AcquaintableAttendable` reworked; new `EarnedSurfaceAttendable`); no shared script touched.

**4.7 (organic crossings), verified:** each `AcquaintanceStage` now authors `minAttends` + `ripenChance` instead of `attendsCost`; the component rolls `ripenChance × (attends past floor + 1)` per qualifying tick and only routes through `TryApplyStage` on the tick that crosses (each `DevelopmentStage`'s `ProgressCost` is now 1). Observed in play: the Netmender crossed Acquainted on attend 2 (at the floor) and Known on attend 4 (one past), with a separate run's Known chain deepening through attend 3 without crossing — the band is real. A hold at zero remaining daylight correctly ends (the existing `AttentionManager` daylight check, unmodified). Baseline tuning: sitting-gated stages are 2/0.5 (Acquainted) and 3/0.34 (Known); wary stages are 1/0.65 and 2/0.5 — in *days*.

**One real bug caught by the build's own ladder:** with every stage at `ProgressCost` 1, `TryApplyStage()`'s random draw among available stages could apply *Known before Acquainted* (observed live: "A charcoal-burner: developed - Known!"). The old cumulative `attendsCost` values had been enforcing order by accident. Fixed structurally: each stage's `DevelopmentStage` now also carries a private `InOrderCondition` (satisfied only when `CurrentStageIndex` is exactly the prior index), making acquaintance explicitly sequential. Re-verified: crossings land in order on both the sitting-gated and wary paths.

**4.8 (temperament), verified:** `oneQualifyingTickPerDay` + `todaySpentLine` on the entity. The Collier's hold ends after its single daily ripening tick; a same-day re-attend resolves as a flavor visit and the overlay gains the sated line ("They don't turn around. They've said what they'll say today."); the next day's attend qualifies again. Wary entities: the Collier and the Osier Bed. Deliberate texture: the sated visit still costs its daylight — sitting with someone who's done talking is still an afternoon spent.

**4.9 (the earned workshop), verified end-to-end:** cold load → the racks refuse the hold (`CanAttend` false; state stays `InRange`) with the not-yours read → Smokehouse Known crossing logs `WorldState flag 'p4_smokehouse_known' set` (fired from `HandleDeveloped`, so the debug advance and the real attend path both set it) → racks open, and three work ticks revealed, in strict first-taken order: `burns_slow` on An Alder Billet, `keeps_well` on A Smoked Eel, then the fallback linger (the Withy's `binds_fast` off-bias) → Osier Bed Known set `p4_osier_bed_known` → the next racks tick revealed `binds_fast` on A Cut Withy. Final ledger confirmed: `split_prone` alone stays unknown — the deliberately unanswerable itch. All four pickups now take unrevealed ("There's more to it"), giving discovery something to answer without a pack or a modal UI.

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

## Playtest findings — the Results Thread (Sean, 7-17-26)

**Overall: better again.** The day-pacing adds variety, and **gaining access to the racks felt earned** — 4.9's go/no-go reads as passed.

- **4.7 (organic crossings): mechanically working, barely felt.** Nearly every stage still landed in 2–3 tending sessions, so the variance didn't register as variance — but crucially, nothing felt countable either ("I definitely did not feel like I was explicitly able to count out updates"). Verdict: the counter-feel is gone, which was the point; the band is just too narrow to read as a band. Worth a light odds tweak (lower floors, lower per-tick chance → wider spread) but explicitly **not worth getting bogged down in**.
- **4.8 (temperament): the day-gating adds variety.** No issues raised.
- **4.9 legibility gap, confirmed from play:** Sean asked whether any spot *other* than the racks offers revealing at Known — the answer is no (the Osier Bed's crossing extends the racks' bias rather than opening its own surface), and the fact that the question needed asking is the finding. The osier→racks seam is currently invisible except through a withy yielding where it previously didn't. Two fixes queued: the **Taken Ledger HUD** (P3's 3.9 `TakenLedgerUI`, pure reuse — makes "there are properties you haven't uncovered" visible at a glance, which also makes the bias extension legible as new reveals landing), and possibly a felt-not-read clause in the racks' open description while the bonus bias is active.
- **Daylight: felt, but a touch rushed.** Candidate retune: `maxDaylight` 5 → 6 (same judgment-call shape as P3's 4 → 5).
- **The big one: the need to *do* something with what's being uncovered is now pressing.** Two pulls named: reconnecting the flow/valve thread parked at the end of P2 (Standing/dominance/flow-reserve, Iterations 42–54), and the sense that hold-to-attend is "a placeholder for something more rich and involved" — with the open question of how to enrich interaction without breaking the design values.

---

## Build Notes (7-18-26)

The three quick items queued at the end of the 7-17-26 session are landed in `Prototype4.unity`, no new script: `TakenLedgerUI` added as a scene root (`Transform` + `UIDocument` + `TakenLedgerUI`, wiring identical to its P3 original — the component has no serialized asset references, which is what made it safe to add by hand-editing the scene YAML directly); `maxDaylight` 5 → 6; and the ripening bands widened one tier further, extending the existing 0.65/0.5/0.34 progression to 0.5/0.34/0.2 and dropping each `minAttends` floor by 1 where it was above the `[Min(1)]` clamp, across all six entities' stages.

**Verification gap, flagged rather than papered over:** this pass was made without a live Play Mode check — MCP-Unity was unreachable for the whole session (`load_scene`/`get_console_logs` timed out repeatedly), consistent with the documented lock-stall limitation, not a code problem. The YAML was hand-verified (script GUID checked against `TakenLedgerUI.cs.meta`, the inserted GameObject block re-read, all twelve `minAttends`/`ripenChance` pairs and `maxDaylight` grepped for the expected values) but the regression gate (cold-load, 0 console errors, Greybox + Prototype3 still clean) has not actually been run via MCP. The playtest below is the real gate — it came back clean — but log it as a formal regression pass next time the editor's reachable via MCP too.

---

## Playtest findings (Sean, 7-18-26)

**All three quick items landed positive.** The wider ripening bands read as more variation across thresholds, and the Taken Ledger HUD removes a lot of "wait, what did I just pick up?" / "do I have anything left to work?" noise — direct hits on both findings it was queued to answer.

Two items flagged to workshop — notes, not yet scoped as iterations:

**1. Tending-to-yield instead of one-shot pickup.** Sean prefers P2's shape — attend a spot repeatedly until it yields, rather than an item just lying there for a single take. This has to look different than P2 proper since P4 deliberately has no item quantity/stacking (Reuse Discipline), but the fix isn't reintroducing P2's yield-table machinery wholesale: condense a site's item pickups (e.g. the river landing's Withy + Alder Billet, or the hearth's Char Knot + Smoked Eel) into a single attendable styled after the old, pre-Iteration-44 generic wilderness spot — no development stages, attention always plays a flavor line, and each attend rolls a chance to yield one of the spot's still-untaken items (checked against `TakenLedger`, so a taken item drops out of the roll). Sean also names this as the natural seam for reconnecting the parked flow/valve thread (`WorldSite`/dominance/flow-reserve, Iterations 42–54) once that's picked back up: these become the upstream sources a flow system would throttle.

*My addition:* this could also do double duty against finding 2 below, almost for free. If a tending spot's *yield chance* (never its flavor line, which should always fire — tending should never feel like it "failed") scales up the longer it's been since the player's last visit — the wary-entity days-gating shape from Iteration 4.8, reused for ripeness instead of trust — then sitting at one spot in a single visit burns through its yield chance fast, while a return after time away finds it fuller. Same precedent as the 4.4 re-skin finding (one meter, re-skinned per entity type), just applied to a place's yield instead of a person's trust. Candidate name: `TendableSpotAttendable`, sibling to `EarnedSurfaceAttendable` in `Mossmark.Prototype4`.

**2. No strategic weight between diving deep on one site and going broad across several.** Right now the choice is close to pure preference: the days-gated entities (4.8) push weakly toward breadth, economy of movement pushes toward depth, and neither creates real tension. Sean is sitting with how to fix this rather than proposing an answer yet.

*My addition, offered as notes rather than a recommendation:* the trap to watch for is turning this into an optimization puzzle — if there's a calculable right order (visit A then B then C to minimize days), that's a menu choice wearing a spreadsheet, which violates "Tried, not chosen" as surely as a literal dropdown would. Directions that seem more likely to stay organic:
- Lean on item 1's ripeness idea directly — spreading attention across sites keeps more of the world "eager" at once, felt through flavor-line texture (a spot that "seems glad to see you" after time away) rather than shown as a number to route around.
- Extend the cross-site bonus-bias seam already proven in 4.9 (the Osier Bed's crossing quietly enriching the Smoking Racks) so that knowing *several* sites, not just one deeply, occasionally opens a richer read or bias somewhere else entirely — breadth paying off in occasional Flame-Sword-register moments, not a per-visit bonus, so the direct reward stays the reveal itself (per the 7-17-26 finding).
- Whatever shape this takes, it should surface as the same overlay/flavor texture the player already reads, never a new stat — the "felt, not read" discipline this whole doc has held to so far.

Neither item is scoped as an iteration yet; recorded here for whenever this thread gets picked up, alongside the teach-loop composition already recommended below.

---

## Where to pick up (for a new session)

**Quick items — done (7-18-26), see Build Notes above.** `TakenLedgerUI` added, `maxDaylight` 5 → 6, ripening bands widened a further tier. Regression gate (cold-load, 0 errors, Greybox + Prototype3 clean) still needs a formal re-run — flagged in the Build Notes verification gap.

**Three candidate threads now sit unscoped, none chosen yet:**
- **Compose acquaintance with P3's teach loop** (recommended below, from 7-17-26) — take → work out at an earned surface → teach it to someone you've come to know → their work visibly changes.
- **Tending-to-yield spots** (7-18-26, above) — condenses item pickups into a P2-shaped generic spot, and doubles as the flow/valve thread's eventual upstream source.
- **Strategic depth-vs-breadth weight** (7-18-26, above) — still an open question, no shape committed.

These aren't mutually exclusive — the ripeness idea in particular threads through both of the 7-18-26 items — but which to build first is a real choice, not yet made.

**The thread choice — what "doing something" should mean next.** Recommendation (from 7-17-26, still standing): **compose acquaintance with P3's teach loop before reconnecting flow/valve.** Reasons:
- Both halves are independently proven (P3: teaching under scarcity is worth choosing; P4: getting-to-know is worth doing), and their composition was already flagged in *After this* as the natural next question: does deep acquaintance gate or ease what someone will accept being taught?
- It answers the itch *directly*: the loop becomes take → work out at an earned surface → **teach it to a person you've come to know** → their work visibly changes. That's a spend for uncovered knowledge that stays inside the attention vocabulary — no new verb needed.
- The wary gate gives teaching a natural new texture for free: perhaps the Collier will only *hear* something taught once Known, or a taught property lands differently at different acquaintance depths. One authored pairing (e.g. teaching the Netmender `keeps_well` from the smoked eel — smoking the catch is the historically real answer to her three households' winters) would be a P3-3.4-sized pilot inside the existing scene.
- The flow/valve thread (Greybox, parked) answers a *different* question — pacing and site economy, not "what is knowledge for." Reconnect it after knowledge has a spend, not instead of one; the merge is bigger and touches `WorldSite`/`WorldGenerator` machinery this scene deliberately excludes.

**On richer interaction than hold-E (notes, not scope — this needs its own pilot when it's picked up):** the design values constrain the direction usefully. "Tried, not chosen" rules out outcome menus, recipe grids, and QTE-shaped skill checks — all of them make the player pick what happens. What it *doesn't* rule out is enriching the channels attention already has:
- **Where you stand** — position relative to the entity as expressive input (watching from the path vs. standing alongside the work vs. walking the osier rows while attending). The player still only chooses where to stand and how long to stay — that's the design value's own sentence, given a larger vocabulary.
- **When you come** — time-of-day/rhythm as texture (a dusk watch shared with the Collier reading differently than a midday one), via the existing `IOutcomeModifier` pattern as ambient chance/flavor influence, never as a puzzle requirement to be solved.
- **What you bring** — attention flavored by what the player knows or carries, which is P3's teach shape generalized; already half-built.
Any of these keeps "hold" as the core act while making *how* it's held mean something. The wrong fork is adding a second input verb; the right fork is making the world read more out of the one verb. A one-entity pilot (positional attending, probably — it's the most bodily and least puzzle-prone) would be the 4.x-sized way to test it.

---

## The Teaching Thread (scoped 7-21-26)

The recommended thread from "Where to pick up" above, given shape. P3 already proved teaching under scarcity is worth choosing (`KnowingEntityAttendable`: `TeachableWant`/`TeachPending`/`Teach()`, gated by `KnownPropertyCondition`). P4 already proved getting-to-know is worth doing. Composing them is not a reuse of P3's components directly — `KnowingEntityAttendable` and `KnownPropertyCondition` are both hard-typed to `KnowingEntityAttendable` (`entity is KnowingEntityAttendable`), and Iteration 4.4's build notes already established why a second `IAttendable` sharing a GameObject is the wrong shape here. The plan below rebuilds the same *shape* as new P4-side pieces, the way 4.9's `EarnedSurfaceAttendable` rebuilt P3's 3.7 working surface rather than reusing it — same discipline, same reasoning.

**The one thing worth deciding before either iteration starts, not during:** P4's zero-effect rule protects the *acquaintance track* specifically — nothing about attending-to-know may change the subject. It says nothing about a second, separate mechanism having effect; P3's teaching already established that having-effect is fine once it's a deliberate, gated spend rather than a side effect of looking. So teaching composes as an *addition* to an entity's track, not a loosening of 4.2's rule: a new stage becomes reachable only once taught, sitting past wherever the acquaintance ladder itself ends. That means `IsFullyAcquainted` needs to stop meaning "no stages left" (`NextStageDef == null`) and start meaning "the acquaintance ladder itself is complete" (an authored ladder-end index) — otherwise a taught-gated stage sitting in the same array would make every post-Known flavor visit silently attempt (and fail) to ripen toward a stage it can't yet cross, which is wasted mechanism wearing the "fully known" reads that are supposed to mean nothing more is coming. Small change, but it's the hinge both iterations below depend on, so it happens once, in 4.10, rather than being rediscovered mid-build.

**What already exists to build on, so neither iteration is starting from zero:** the full "take → work out → teach" pipeline is already scaffolded by content, not just precedent — and it's more reused than it first looks. `PropertyPickupAttendable` and `EarnedSurfaceAttendable` both already mark a reveal known two ways: under the item's own id (`PropertyKnowledge.MarkKnown(itemId, propertyId)`) *and* under a shared `playerKnowerId` field, carried over verbatim from P3 as the literal string `"p3_player"` rather than renamed for this scene. That second mark is exactly the "does the player generally know this, regardless of which item taught them" fact P3's own `KnowingEntityAttendable.TeachPending` reads via `WorldContext.IsPropertyKnown(playerKnowerId, want.propertyId)` — so a P4 taught-gate can check the identical thing, on the identical id, with zero new infrastructure. A Smoked Eel (`p4_smoked_eel`, `keeps_well`) sits unrevealed by the Smokehouse; the Smoking Racks (4.9) already reveal it via a real held work-tick.

### Iteration 4.10 — Teaching What They'll Hear

The atomic composition claim. One entity (the Netmender), one authored pairing — `keeps_well`, from A Smoked Eel, exactly the pairing already named in "Where to pick up" ("smoking the catch is the historically real answer to her three households' winters"). Build: `Mossmark.Prototype4.TaughtPropertyCondition` (the P4 analog of P3's `KnownPropertyCondition`, typed to `AcquaintableAttendable`, checking a new `KnowsOfSelf`-style method against the entity's *own* post-teach knowledge); `AcquaintableAttendable` gains a small `TeachableWant[]` (mirroring P3's shape: propertyId, teachLine, and the taught stage's own display data) and a `TeachPending`/`Teach()` pair that only ever activates once the entity is Known (`IsFullyAcquainted`, using the corrected ladder-end sense from above) *and* the player generally knows the property — `WorldContext.IsPropertyKnown("p3_player", "keeps_well")`, the exact call P3's own teach gate already makes, reusing its `playerKnowerId` convention rather than inventing a `p4_player` equivalent. No carrying, no consuming — knowledge is the gate, exactly P3's reasoning. Teaching itself is a short one-shot tick, same register as P3's `teachDuration`; the resulting stage crosses through the ordinary shared resolver (`TryApplyStage`/`OnDeveloped`/`EntityFeedback` pop), so it *feels* like every other crossing in this doc, not a special case.

**Explicitly out of scope:** a second entity or pairing (4.11); teaching before Known, or any depth-differentiated reception (a single binary gate for now); any change to what the acquaintance ladder itself reveals or how it paces; UI beyond the existing overlay/notification vocabulary.

**Success criterion — the actual go/no-go:** cold load → get to know the Netmender → work out the smoked eel at the racks → teach her → does it read as the natural next thing to do with someone you've come to know, rather than a bolted-on mechanic wearing an acquaintance skin? And does her post-teach read (new tint, new description, new flavor lines) land as an earned, visible consequence — the Flame Sword register the 7-17-26 findings called for — or does it feel like just one more stage crossing with different flavor text? If the latter, the fix is likely in what visibly changes (behavior/flavor richness), not in the gating mechanism itself.

---

### Iteration 4.11 — Whether Depth Changes What Lands

Only after 4.10 holds. Extends the same mechanism, unmodified, to a second pairing on a *wary* entity — the Collier is the obvious candidate, both because "Where to pick up" already named this exact texture question ("perhaps the Collier will only hear something taught once Known") and because it's the entity whose temperament (returns-gated trust, per 4.8) has the most to say about whether a flat "Known unlocks teaching" gate feels native everywhere or needs its own voice for entities that don't warm easily. Candidate pairing: `burns_slow` is already seeded knowledge on the Collier and shared with the Smokehouse/Alder Billet register, so a genuinely new pairing (not yet decided) is worth choosing deliberately rather than reusing a property already spoken for. Also worth a deliberate choice: what happens if the player tries to teach *before* Known — does no such option simply fail to surface (today's default, since `TeachPending` only evaluates once Known), or does attempting it early deserve its own felt rejection line ("They're not ready to hear that from you yet"), which would be new authored texture, not new mechanism.

**Explicitly out of scope:** acquaintance-depth-*scaled* reception (e.g., a property landing differently at Acquainted vs. Known) — that's a further idea flagged in "Where to pick up" but a bigger claim than this iteration needs to test; a third entity; any UI.

**Success criterion — the actual go/no-go:** does the same mechanism, reused with zero code changes and only new authored data, feel native to the Collier's specific temperament — or does teaching a wary entity need its own texture (a distinct gate, a distinct refusal) to avoid feeling like the Netmender's pairing with different words pasted over it? If it needs its own texture, that's a real finding about temperament and teaching, not a bug in 4.10's mechanism.

---

## Teaching Thread Build Notes (7-21-26)

Iteration 4.10 is built and verified live in Play Mode via MCP, 0 console errors/warnings throughout the whole session; the Greybox + Prototype3 regression gate was re-run clean afterward. Iteration 4.11 (the Collier pairing) is not built — only 4.10 landed this session. New code: `Mossmark.Prototype4.TaughtPropertyCondition` (new file); `AcquaintableAttendable` extended (no new component needed for the entity side). No shared script touched.

**The hinge, built as planned:** `IsFullyAcquainted` now reads `NextStageDef == null || !string.IsNullOrEmpty(NextStageDef.taughtPropertyId)` instead of the old `NextStageDef == null`. This one-line change is what lets a taught-gated stage sit past the acquaintance ladder's own end without ordinary post-Known visits silently attempting (and failing) to ripen toward it. Everything else follows from reusing `DevelopableEntity.CanMakeProgress()` (already public, already used elsewhere in the game) as the single source of truth for "is there something to work toward right now" — it naturally returns false for an authored-but-not-yet-taught stage (its `TaughtPropertyCondition` fails) and naturally returns true once taught, so `OnAttentionComplete()`'s ripening branch needed no special-casing between ladder stages and taught stages — both flow through the identical `attendsTowardNextStage`/`RollCrossing`/`Cross()` path.

**`TaughtPropertyCondition`** mirrors P3's `KnownPropertyCondition` almost exactly, typed to `AcquaintableAttendable` instead of `KnowingEntityAttendable` (the two component families stay deliberately separate, per Iteration 4.4's original reasoning against sharing a GameObject). `TeachPending` reuses P3's exact convention for "does the player generally know this" — `WorldContext.IsPropertyKnown(playerKnowerId, propertyId)` against the literal `"p3_player"` id — which turned out to already be populated by both `EarnedSurfaceAttendable` and `PropertyPickupAttendable` (both mark it on every reveal, carried over from P3 verbatim). Confirmed live: zero new "does the player know this" infrastructure was actually needed, exactly as scoped.

**Teaching and the zero-effect rule:** `Teach()` marks `PropertyKnowledge.MarkKnown(entityId, propertyId)` and immediately re-baselines `subjectFingerprint` to the post-teach state. This is the one deliberate exception to 4.2's zero-effect check — documented in code and here, not silent — and it held clean across the whole test: no `VerifySubjectUnchanged()` error ever fired, before or after teaching, on any entity.

**A nice thing that fell out of reuse, unplanned:** `GetOverlayDescription()`'s seeded-knowledge line (`GetSeededKnownProperties()`) reads current `PropertyKnowledge` generically, not specifically the entity's *seeded* properties — so the moment the Netmender is taught `keeps_well`, her line automatically grows from "They speak of what binds fast" to "They speak of what binds fast, and of what keeps well," with no code written for that specifically. It's a second, smaller visible-change signal alongside the stage crossing's own tint/description/interaction swap.

**Verified live (MCP, Play Mode, 7-21-26), two passes:**
- **Deterministic pass (`DebugForceTeach`)**: Netmender advanced to Known → pre-teach real attend correctly resolved as a plain flavor visit with no ripening attempt (`CanMakeProgress()` false, taught stage locked) → `DebugForceTeach` marked `keeps_well` known on the entity directly, logged `The Netmender: taught 'keeps_well'.` → a real held attend then ran two ticks in the same hold (deepening, then crossed — the existing multi-tick hold-continuation behavior working identically for a taught stage) → `The Netmender: developed - Smokes the Catch!` → final state: `acquaintanceStage=2`, tint/description/interaction all switched to the new stage's authored data, seeded-knowledge line correctly grew to include `keeps_well`.
- **Real-path pass (no debug shortcuts)**: fresh Play Mode session → Netmender to Known → Smokehouse to Known (`WorldState flag 'p4_smokehouse_known' set`) → A Smoked Eel taken unrevealed (`"taken - nature not yet known"`, confirming 4.9's no-auto-reveal is untouched) → worked at the Smoking Racks, revealing `keeps_well` and marking it under `"p3_player"` → back at the Known Netmender, her interaction line read **"Hold E to speak of what keeps well"** before any debug tool touched her, confirming `TeachPending` correctly picked up the real discovery path → a real held attend resolved as the teach tick itself (`0.6s`, matching `teachDuration` exactly, distinct from the ordinary `2s` `attendDuration`) → a further real attend ripened and crossed to *Smokes the Catch*. Full pipeline — take → work out → teach → visible change — run for real, start to finish, with zero errors.
- **Non-regression checks**: every other entity (Smokehouse, Osier Bed, Collier, Bothy, Hearth Ring) logged unaffected states throughout (no `taughtPropertyId` authored on any of them, so `IsFullyAcquainted`/`CanMakeProgress` behave exactly as before 4.10). `DebugAdvanceAcquaintance` (switched from an `IsFullyAcquainted` guard to a `CanMakeProgress()` guard) correctly still blocks a not-yet-taught stage and correctly still forces a taught-and-ready one across.

**A verification-methodology note, not a code issue:** mid-session, daylight ran out (`0/6`) from real-time day-cycle decay during an extended stretch of MCP connectivity trouble, which silently blocked every subsequent `Begin Attend` at the `RequiresDaylight` gate with no console output — worth remembering as a "nothing happened and nothing logged" failure mode distinct from an actual bug, diagnosable via `Log Daylight`. Resting at the bedroll cleared it.

**Content:** the Netmender's third stage, `smokes_the_catch` ("Smokes the Catch"), taught by `keeps_well` from A Smoked Eel — the exact pairing named in "Where to pick up" 7-17-26 ("smoking the catch is the historically real answer to her three households' winters"). `minAttends: 1`, `ripenChance: 0.5` (the most generous existing tier — once earned, the payoff shouldn't make the player wait long). Tint moves from Known's green (`0.45, 0.62, 0.4`) to a warm amber (`0.68, 0.56, 0.32`), reading as a genuinely new state rather than a deeper shade of the same one.

---

## Iteration 4.11 Build Notes (7-21-26, same session as 4.10)

Built and verified live in Play Mode via MCP, 0 console errors/warnings throughout; the Greybox + Prototype3 regression gate re-run clean afterward. New code: one field (`AcquaintanceStage.earlyTeachHintLine`) plus a `TaughtStageDef`/`EarlyTeachAttemptPending` helper pair on `AcquaintableAttendable`. `TaughtPropertyCondition` itself needed no changes — confirming the mechanism really is temperament-agnostic, exactly what the iteration set out to test. No shared script touched.

**The pairing chosen:** `heavy_true` ("heavy and true"), the one property in `PropertyRegistry` not yet spoken for in P4 (`binds_fast`/`split_prone` on the Netmender's register, `burns_slow` shared by the Smokehouse/Alder Billet/Char Knot, `keeps_well` on the Netmender's taught stage) — deliberately not `burns_slow`, which the Collier already seeds and which the doc flagged as already spoken for. Content: a new pickup, **A Fused Clinker** (`p4_fused_clinker`, unrevealed like every other P4 pickup), placed near the Hearth Ring — a fused lump of slag from the kiln floor, judged by weight the way a collier judges a burn without waiting on the smoke's color. Added to the Smoking Racks' base `biasPropertyIds` (not gated behind any flag — the racks already reveal properties from both sites' pickups, `burns_slow` via the Char Knot being the existing precedent, so this isn't a new cross-site seam, just the established one carrying one more property). The Collier's new taught stage, `shares_the_watch` ("Shares the Watch"): `minAttends: 1`, `ripenChance: 0.5` (the same generous once-earned tier as the Netmender's `smokes_the_catch`), tint shifts from Known's cool blue-grey (`0.38, 0.36, 0.45`) to a warm ember brown (`0.5, 0.42, 0.3`) — a genuinely new state, not a deeper shade of Known's grey. The content leans on the Collier's own seeded loneliness (`trueStateNote`: "burns the hearth ring alone since their partner died... the second bed in the bothy has been cold a year") — the taught stage's description has the watch "not kept alone anymore, not quite," the smallest possible narrative payoff for the teach, not a resolution of the loneliness itself.

**The open question, decided:** "The Teaching Thread" left open whether attempting to teach early should silently fail to surface (the 4.10 default) or get its own felt rejection line. Decided in favor of the felt line, authored only on the Collier's stage (the Netmender's `smokes_the_catch` has no `earlyTeachHintLine` — she never needed one, being sitting-gated rather than wary). Mechanism: `TaughtStageDef` finds the array's final entry whenever it's taught-gated, regardless of how far the ladder has progressed (unlike `NextStageDef`, which only points at the taught stage once the ladder's actually done); `EarlyTeachAttemptPending` is true the moment the player already knows the paired property but the entity hasn't reached it yet. Descriptive only — appended to `GetOverlayDescription()`, ordered after the `SpentToday` branch — it never touches `GetOverlayInteractionLine()`, so "Hold E to speak of..." still only ever surfaces once `TeachPending` itself is true. New authored texture, not a new gate, exactly as scoped.

**Verified live (MCP, Play Mode, 7-21-26), one continuous real-path run (no debug shortcuts for the take/reveal steps):**
1. Cold load — all six P4 entities read Unfamiliar, `6/6` daylight, day 0.
2. A Fused Clinker taken near the Hearth Ring → `"A Fused Clinker: taken - nature not yet known."` (autoRevealOnTake false, working as every other P4 pickup does).
3. Smokehouse forced to Known (debug advance, to open the racks without burning the whole session on an orthogonal ladder) → `WorldState flag 'p4_smokehouse_known' set`.
4. Worked at the Smoking Racks → `"The Smoking Racks: revealed 'heavy_true' on A Fused Clinker."` — confirms the bias-list addition; marked under both the item id and `p3_player`.
5. **The early-teach hint, confirmed before the Collier had been attended even once:** overlay read *"A figure grey to the elbows, minding something on the slope you can't see from here. They don't turn around. They're not ready to hear that from you yet."* — appended correctly while `CurrentStageIndex == -1`.
6. Advanced to Acquainted (debug advance) — hint line still present, correctly re-evaluated against the new stage's base description.
7. Advanced to Known (debug advance) — hint line correctly *disappears* (`IsFullyAcquainted` now true, so the seeded-knowledge branch takes over instead); interaction line flipped unprompted to **"Hold E to speak of what heavy and true"** — `TeachPending` picked up the real discovery path with zero debug intervention, mirroring 4.10's real-path pass exactly.
8. A real held attend on the Known Collier resolved as the teach tick itself — `0.6s` elapsed wall-clock between `HandleHoldStarted` and completion, matching `teachDuration` exactly (distinct from the ordinary `2s`/wary-gated `attendDuration`) — logging `"The Collier: taught 'heavy_true'."` The hold correctly ended after the teach tick (no `ContinueAttending`), same as the Netmender's.
9. A further attend (debug advance, since real ripening is a probabilistic roll) crossed to *Shares the Watch*: `acquaintanceStage=2`, tint `RGBA(0.5, 0.42, 0.3, 1)`, `knows=[burns_slow, heavy_true]` (fingerprint correctly re-baselined post-teach), overlay correctly showing the new stage's own description **plus** the seeded-knowledge line automatically grown to *"They speak of what burns slow, and of what heavy and true"* — the same unplanned-but-welcome `GetSeededKnownProperties()` behavior 4.10 noted, now confirmed to generalize to a second entity without any code awareness of it happening twice. Interaction line correctly switched to the new stage's own `"Hold E to weigh it with them"`.
10. `VerifySubjectUnchanged()` never fired an error at any point in the run, before or after teaching — the zero-effect fingerprint held.
11. 0 errors, 0 warnings in the console for the entire session. Regression gate re-run clean: Greybox (wandering-thing spawner firing normally) and Prototype3 both loaded and entered Play Mode with 0 errors/warnings.

**A grammar bug caught by this pairing specifically:** every other property in `PropertyRegistry` is a verb phrase ("binds fast", "burns slow") that reads fine spliced straight after "what"/"it"/"that" ("what binds fast", "it burns slow"). `heavy_true`'s phrase ("heavy and true") is adjectival — every call site that built a "what {Phrase}"/"it {Phrase}"/"that {Phrase}" clause read ungrammatically the moment this pairing exercised it live (e.g. "Hold E to speak of what heavy and true"). Fixed at the source rather than per call site: `PropertyDefinition` gained an `IsAdjectival` flag (default false, set only on `heavy_true`) and a `Clause` property (`IsAdjectival ? $"is {Phrase}" : Phrase`) that every splice site now reads instead of `Phrase` directly — `AcquaintableAttendable` (both the interaction line and the seeded-knowledge join), `EarnedSurfaceAttendable`, `PropertyAvailableCondition`, and P3's `KnowingEntityAttendable`/`WorkingSurfaceAttendable`, plus Greybox's `WorkshopUI` failure-reveal line, since all of them read the same shared registry. The two places that just print `Phrase` as a standalone label (`ItemPropertyDisplay`, `TakenLedgerUI`) were left alone — a bare tag needs no verb. Re-verified live: the Collier's interaction line now reads "Hold E to speak of what is heavy and true" and his post-teach description reads "They speak of what burns slow, and of what is heavy and true." 0 errors/warnings; Greybox and Prototype3 regression gates re-run clean.

**The go/no-go, answered:** the mechanism needed **zero new gating logic** to work on a wary entity — `TaughtPropertyCondition`, `CanMakeProgress()`, and the whole ripening/crossing pipeline ran identically for the Collier as for the Netmender. What the Collier's temperament actually asked for was purely authored: a distinct pairing, a distinct tint, and one optional descriptive line for the "not ready yet" moment — confirming the finding 4.11 was scoped to test. The days-gating from 4.8 (`oneQualifyingTickPerDay`) never interacted with teaching in this run, since `TeachPending` is checked before the `SpentToday` ripening branch in `OnAttentionComplete()` — the same "teaching takes priority whenever it's available" note 4.10 already flagged as an open question about wary entities is still open; this session didn't force a same-day double-attempt (teach, then immediately try to ripen again) to see whether the wary gate should also cap the crossing tick itself. Worth a look if temperament-and-teaching gets revisited.

---

## Playtest findings (Sean, 7-21-26)

**Composition works, but the payoff doesn't yet feel distinct.** Teaching lands mechanically — the gate, the tick, the crossing all read clean — but in play it comes across as "just one more stage crossing," the exact risk 4.10's own success criterion named up front. Not judged as needing an immediate fix; recorded as a real finding rather than let slide.

The likely gap isn't the mechanism, it's what a crossing *does*: acquaintance stages and taught knowledge currently all cash out the same way — a richer read, a new tint, a new line. That's the right shape for plain getting-to-know (4.1–4.9 already proved that's wantable on its own), but a *taught* crossing is a different kind of moment — the player spent discovery on it, not just attention — and right now nothing distinguishes what it unlocks from what patient attention alone would have eventually shown. Worth returning to once there's more than one taught pairing to compare (4.11 is the natural next data point): does downstream impact — a new bias, a new access, a new option elsewhere — need to be part of what teaching *means*, the way 4.9's earned surface gave plain acquaintance its own downstream payoff. Flagging now so it isn't rediscovered cold later.

---

## After this

4.1–4.9 held, across two rounds of playtesting (7-17-26, 7-18-26): "getting to know an already-alive place" is a real, attention-spending activity distinct from both development-by-tending (Greybox) and teaching (P3), and the first result-of-knowing (4.9's Smoking Racks) landed as a genuinely earned consequence rather than a dispensed one. What's still open from here:

- **Procedural rollout**: multiple sites, randomized count and archetype mix, using `WorldGenerator`'s existing clustering/member-pool machinery (Iterations 47/51). 4.6 confirmed two hand-authored sites read as genuinely different voices (the differentiation criterion held) — but procedural generalization itself is still untried, and stays parked until one of the live threads (see "Where to pick up") makes it worth the machinery.
- **Reconnecting to P3's teach loop**: no longer just a recommendation — Iteration 4.10 (7-21-26, see "The Teaching Thread" above) built and verified it end-to-end on one pairing (the Netmender, `keeps_well`). Iteration 4.11 (7-21-26, same session — the Collier, `heavy_true`) is also built and verified: the mechanism needed zero temperament-specific code, only authored data (a distinct pairing, tint, and an optional early-teach hint line). Not yet playtested by Sean, so the 4.10 finding below (that a taught crossing doesn't yet feel distinct from a plain one) still stands untested against a second pairing — worth checking whether having *two* taught entities to compare changes that read.
- **A review surface**: the item-side itch is already answered — `TakenLedgerUI`, reused into P4 on 7-18-26, directly after the 7-17-26 playtest named the osier→racks legibility gap. Still open is the acquaintance-depth side: a glanceable "who and what have I come to know, and how well" summary. The overlay's per-entity applied-upgrades list already surfaces this one entity at a time (Build Notes, 7-17-26) but reads as slightly ledger-like — worth a dedicated non-modal summary only if a future playtest names this itch specifically for acquaintance, not assumed here.
- **Stakes / the possibility of loss** (explored 7-21-26): a same-session pilot tested whether attending needs real risk, not just accrual, to stop feeling flat — a wary entity's trust regressing on a same-day attend pushed past its daily gate, tell-then-roll, re-earnable. The mechanism itself worked cleanly (verified live: the warning landed before the roll, releasing during it was safe, a sour regressed and re-earned correctly, the floor held), but it was backed out before playtesting: the current attend/day pacing (2-second ticks, ladders that cross in 2-3 days) doesn't give "going too far" enough room to register as a felt risk rather than a coin flip. Worth revisiting once the loop has more breathing room — the depth-vs-breadth and tending-to-yield threads above are the more likely path to that room than the stakes mechanism needing rework. Not concluded against; concluded *too early for*.
