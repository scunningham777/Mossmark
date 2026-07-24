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
- The overlay's applied-upgrades list shows crossed acquaintance stages ("Acquainted", "Known") in the detail panel — legible, if slightly ledger-like; a candidate for the review-surface question in *Next Steps*.

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

Neither item is scoped as an iteration yet; recorded here for whenever this thread gets picked up.

---

## The Teaching Thread (scoped 7-21-26)

The thread recommended after 4.9, now given shape. P3 already proved teaching under scarcity is worth choosing (`KnowingEntityAttendable`: `TeachableWant`/`TeachPending`/`Teach()`, gated by `KnownPropertyCondition`). P4 already proved getting-to-know is worth doing. Composing them is not a reuse of P3's components directly — `KnowingEntityAttendable` and `KnownPropertyCondition` are both hard-typed to `KnowingEntityAttendable` (`entity is KnowingEntityAttendable`), and Iteration 4.4's build notes already established why a second `IAttendable` sharing a GameObject is the wrong shape here. The plan below rebuilds the same *shape* as new P4-side pieces, the way 4.9's `EarnedSurfaceAttendable` rebuilt P3's 3.7 working surface rather than reusing it — same discipline, same reasoning.

**The one thing worth deciding before either iteration starts, not during:** P4's zero-effect rule protects the *acquaintance track* specifically — nothing about attending-to-know may change the subject. It says nothing about a second, separate mechanism having effect; P3's teaching already established that having-effect is fine once it's a deliberate, gated spend rather than a side effect of looking. So teaching composes as an *addition* to an entity's track, not a loosening of 4.2's rule: a new stage becomes reachable only once taught, sitting past wherever the acquaintance ladder itself ends. That means `IsFullyAcquainted` needs to stop meaning "no stages left" (`NextStageDef == null`) and start meaning "the acquaintance ladder itself is complete" (an authored ladder-end index) — otherwise a taught-gated stage sitting in the same array would make every post-Known flavor visit silently attempt (and fail) to ripen toward a stage it can't yet cross, which is wasted mechanism wearing the "fully known" reads that are supposed to mean nothing more is coming. Small change, but it's the hinge both iterations below depend on, so it happens once, in 4.10, rather than being rediscovered mid-build.

**What already exists to build on, so neither iteration is starting from zero:** the full "take → work out → teach" pipeline is already scaffolded by content, not just precedent — and it's more reused than it first looks. `PropertyPickupAttendable` and `EarnedSurfaceAttendable` both already mark a reveal known two ways: under the item's own id (`PropertyKnowledge.MarkKnown(itemId, propertyId)`) *and* under a shared `playerKnowerId` field, carried over verbatim from P3 as the literal string `"p3_player"` rather than renamed for this scene. That second mark is exactly the "does the player generally know this, regardless of which item taught them" fact P3's own `KnowingEntityAttendable.TeachPending` reads via `WorldContext.IsPropertyKnown(playerKnowerId, want.propertyId)` — so a P4 taught-gate can check the identical thing, on the identical id, with zero new infrastructure. A Smoked Eel (`p4_smoked_eel`, `keeps_well`) sits unrevealed by the Smokehouse; the Smoking Racks (4.9) already reveal it via a real held work-tick.

### Iteration 4.10 — Teaching What They'll Hear

The atomic composition claim. One entity (the Netmender), one authored pairing — `keeps_well`, from A Smoked Eel, exactly the pairing already named in the prior session's notes ("smoking the catch is the historically real answer to her three households' winters"). Build: `Mossmark.Prototype4.TaughtPropertyCondition` (the P4 analog of P3's `KnownPropertyCondition`, typed to `AcquaintableAttendable`, checking a new `KnowsOfSelf`-style method against the entity's *own* post-teach knowledge); `AcquaintableAttendable` gains a small `TeachableWant[]` (mirroring P3's shape: propertyId, teachLine, and the taught stage's own display data) and a `TeachPending`/`Teach()` pair that only ever activates once the entity is Known (`IsFullyAcquainted`, using the corrected ladder-end sense from above) *and* the player generally knows the property — `WorldContext.IsPropertyKnown("p3_player", "keeps_well")`, the exact call P3's own teach gate already makes, reusing its `playerKnowerId` convention rather than inventing a `p4_player` equivalent. No carrying, no consuming — knowledge is the gate, exactly P3's reasoning. Teaching itself is a short one-shot tick, same register as P3's `teachDuration`; the resulting stage crosses through the ordinary shared resolver (`TryApplyStage`/`OnDeveloped`/`EntityFeedback` pop), so it *feels* like every other crossing in this doc, not a special case.

**Explicitly out of scope:** a second entity or pairing (4.11); teaching before Known, or any depth-differentiated reception (a single binary gate for now); any change to what the acquaintance ladder itself reveals or how it paces; UI beyond the existing overlay/notification vocabulary.

**Success criterion — the actual go/no-go:** cold load → get to know the Netmender → work out the smoked eel at the racks → teach her → does it read as the natural next thing to do with someone you've come to know, rather than a bolted-on mechanic wearing an acquaintance skin? And does her post-teach read (new tint, new description, new flavor lines) land as an earned, visible consequence — the Flame Sword register the 7-17-26 findings called for — or does it feel like just one more stage crossing with different flavor text? If the latter, the fix is likely in what visibly changes (behavior/flavor richness), not in the gating mechanism itself.

---

### Iteration 4.11 — Whether Depth Changes What Lands

Only after 4.10 holds. Extends the same mechanism, unmodified, to a second pairing on a *wary* entity — the Collier is the obvious candidate, both because the prior session's notes already named this exact texture question ("perhaps the Collier will only hear something taught once Known") and because it's the entity whose temperament (returns-gated trust, per 4.8) has the most to say about whether a flat "Known unlocks teaching" gate feels native everywhere or needs its own voice for entities that don't warm easily. Candidate pairing: `burns_slow` is already seeded knowledge on the Collier and shared with the Smokehouse/Alder Billet register, so a genuinely new pairing (not yet decided) is worth choosing deliberately rather than reusing a property already spoken for. Also worth a deliberate choice: what happens if the player tries to teach *before* Known — does no such option simply fail to surface (today's default, since `TeachPending` only evaluates once Known), or does attempting it early deserve its own felt rejection line ("They're not ready to hear that from you yet"), which would be new authored texture, not new mechanism.

**Explicitly out of scope:** acquaintance-depth-*scaled* reception (e.g., a property landing differently at Acquainted vs. Known) — that's a further idea flagged in *Next Steps* below but a bigger claim than this iteration needs to test; a third entity; any UI.

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

**Content:** the Netmender's third stage, `smokes_the_catch` ("Smokes the Catch"), taught by `keeps_well` from A Smoked Eel — the exact pairing named in the prior session's notes, 7-17-26 ("smoking the catch is the historically real answer to her three households' winters"). `minAttends: 1`, `ripenChance: 0.5` (the most generous existing tier — once earned, the payoff shouldn't make the player wait long). Tint moves from Known's green (`0.45, 0.62, 0.4`) to a warm amber (`0.68, 0.56, 0.32`), reading as a genuinely new state rather than a deeper shade of the same one.

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

## The Tending Thread (scoped 7-21-26)

Scoped from the 7-18-26 playtest note: Sean prefers P2's shape — attend a spot repeatedly until it yields, rather than an item just lying there for a single take (see Build Notes, 7-18-26, above). This thread condenses the river landing's three item pickups (A Cut Withy, An Alder Billet, A Smoked Eel) into one tending spot, deliberately including A Smoked Eel — by this point load-bearing for the Teaching Thread's Netmender pairing — so this also tests whether real acquisition scarcity sits well alongside an already-earned teach gate, not just whether tending feels better than one-shot pickup in isolation. Same discipline as every prior thread: no shared-script behavior changes, new behavior in a new `Mossmark.Prototype4` component.

### Iteration 4.12 — Tending to Yield: One Site, Full Pool

Replace the river landing's three separate `PropertyPickupAttendable` GameObjects with one new `TendableSpotAttendable` (plain `MonoBehaviour : IAttendable` — no `DevelopableEntity`/track, since there's no stage to cross here). Every attend rolls a flat `yieldChance` (0.3, per Sean's steer toward flavor being the more common outcome) against the pool of still-unclaimed entries, checked live against `TakenLedger` so an already-taken item drops out of the roll — no separate claimed-state tracked, the ledger is the only bookkeeping. A miss always plays a flavor line (tending should never feel like it "failed"); a hit registers the picked entry in `TakenLedger` exactly as `PropertyPickupAttendable` does today and posts that entry's own unrevealed take-line, so the downstream pipeline (unrevealed → worked out at the Smoking Racks → teachable) is completely untouched — this iteration only changes *acquisition*, not what a taken item does afterward. Once all three entries are claimed, the spot settles into flavor-only forever — no failure state, just an exhausted, quiet spot.

**Explicitly out of scope:** ripeness/time-since-last-visit scaling (4.13); the hearth site (Char Knot, Fused Clinker keep their existing one-shot pickups); any change to the Racks/teach pipeline itself.

**Success criterion:** does tending this one spot repeatedly — not knowing which of the three items (if any) a given attend will turn up — feel better than the guaranteed one-shot take it replaces? And does A Smoked Eel's new uncertainty read as real stakes on the teach loop, or as arbitrary friction in front of something the player already knows they want?

---

### Iteration 4.13 — Ripeness: Yield Chance Scales with Time Away

Only after 4.12 holds. The 7-18-26 idea, still unbuilt: a tending spot's yield chance (never its flavor line, which should always fire) rises the longer it's been since the player's last visit, reusing Iteration 4.8's `DayCycleManager.DayIndex`-based shape but aimed at ripeness instead of trust. Sitting at the spot in one visit burns through its yield chance fast; a return after time away finds it fuller.

**Explicitly out of scope:** any change to the pool itself or what it yields; a second tending spot; the hearth site.

**Success criterion:** does spreading attention across the site (and back to this spot later) read as more rewarding than sitting here until the pool empties — the same felt-not-counted register 4.7's ripening bands aimed for, applied to yield instead of acquaintance?

---

## Iteration 4.12 Build Notes (7-21-26)

Built and verified live in Play Mode via MCP, 0 console errors/warnings throughout; the Greybox + Prototype3 regression gate re-run clean afterward. New code: one file, `Mossmark.Prototype4.TendableSpotAttendable` (plain `MonoBehaviour : IAttendable`, no `DevelopableEntity`). No shared script touched. Iteration 4.13 (ripeness) is scoped above but not built this session.

**The river landing's three pickups became one GameObject, The Landing:** A Cut Withy, An Alder Billet, and A Smoked Eel's `PropertyPickupAttendable` instances were removed from `Prototype4.unity` and their item data (itemId, displayName, propertyIds, take-line) moved into a 3-entry `pool` on the new component. Placed centrally among their old positions, near the Osier Bed. `yieldChance: 0.3` (Sean's steer — flavor-only is the more common outcome). `TakenLedger` is the only bookkeeping the pool needs: `GetUnclaimedEntries()` filters the authored pool against `TakenLedger.All` live, every attend, so there's no separate claimed-state to keep in sync or get stale.

**Verified live (MCP, Play Mode, 7-21-26):**
- Repeated attends at The Landing produced both outcomes: several flavor-only misses (no `TakenLedger` change, daylight still spent — a miss is real time, not a free look) and, over enough attends, all three pool entries yielded in mixed order, each logged (`"The Landing: yielded '<name>'."`) and confirmed unrevealed via `Mossmark/Prototype3/Log Taken Ledger` (`P3Debug: ... = unknown` for each, exactly matching a fresh `PropertyPickupAttendable` take).
- Pool exhaustion confirmed clean: once all three entries were claimed, further attends produced 0 errors and always resolved as flavor — `GetUnclaimedEntries()` correctly returns empty and the `candidates.Count == 0` guard short-circuits before any roll.
- Downstream pipeline confirmed untouched: with the Smokehouse advanced to Known (opening the Racks), working the Racks revealed `keeps_well` on the Landing-sourced Smoked Eel exactly as it would have on the old standalone pickup — then the Netmender's interaction line flipped unprompted to "Hold E to speak of what keeps well," and a real held attend completed the teach tick (`"The Netmender: taught 'keeps_well'."`). The full take → work out → teach pipeline runs identically on an item acquired via tending as it did on one-shot pickup.

**A verification-methodology snag, not a bug:** mid-session, a Racks attend appeared to silently do nothing (no reveal, no daylight change) — this was the same "daylight ran out, no console output" failure mode the 4.10 session flagged (`RequiresDaylight` blocks `HandleHoldStarted` before it starts, with nothing logged). Diagnosed via `Log Daylight` (0/6), resolved by resting. Recorded again here since it bit a second, unrelated iteration — worth remembering as a standing verification habit (check daylight first when an attend "does nothing"), not something to fix in code.

**Fix, same session: holding E now continues tending regardless of outcome.** Shipped with `ContinueAttending => false`, copied by habit from `PropertyPickupAttendable`'s one-shot shape — but a one-shot take and a repeatable tending spot aren't the same kind of thing, and the initial build missed that: every attend interrupted the hold, hit or miss, forcing a re-press for each tick. Flagged by Sean in play. Fixed by changing `ContinueAttending` to an unconditional `true` — unlike `AcquaintableAttendable`'s stage-cross, there's no moment here that needs the hold to break for the player to read it (a yield and a miss are both just a posted line), so nothing else needed special-casing. `AttentionManager.ShouldContinue()` already handles the rest generically (held key, in range, daylight remaining) — confirmed by reading it, not assumed. Verified live: a single held press ticked through multiple attends unbroken, including back-to-back yields (An Alder Billet immediately followed by A Cut Withy in the same hold) and, separately, a yield followed by further ticks afterward — the hold only ever ended when daylight ran out, never on an outcome. Pool-exhausted holds (flavor-only) continue the same way, cleanly, no errors. 0 console errors/warnings; Greybox and Prototype3 regression gates re-run clean.

**Content:** The Landing's description and flavor lines are written to cover general foraging at the site rather than any one item specifically ("Whatever the work here leaves loose — driftwood, trimmings, the odd catch set aside — turns up along the bank if you take the time to look."), since the same spot now stands in for what were three separately-themed pickups. The hearth site (Char Knot, Fused Clinker) is untouched, still one-shot `PropertyPickupAttendable` pickups, per this iteration's explicit scope.

---

## Iteration 4.13 Build Notes (7-21-26)

Built and verified live in Play Mode via MCP, 0 console errors/warnings throughout; the Greybox + Prototype3 regression gate re-run clean afterward. New code confined to `TendableSpotAttendable` (three new authored fields, a `DebugRipenessState()` accessor) plus two menu items in `Prototype4Debug.cs`. No shared script touched.

**The shape:** `yieldChance` (0.3) is now the floor a per-attend effective chance ramps up from and depletes back toward, computed fresh each attend from two pieces of state — `lastVisitDayIndex` and `attendsToday` — rather than any persistent "ripeness" value:

```
daysAway = clamp(today - lastVisitDayIndex, 0, maxDaysAwayBonus)   // 0 on a spot never yet visited
effectiveChance = clamp01(yieldChance + ripenBonusPerDayAway * daysAway - depletionPerAttendToday * attendsToday)
```

`daysAway` is read *before* `attendsToday` resets for a new day and *before* `lastVisitDayIndex` updates to today — so the bonus is always computed against the *previous* visit, never the one being spent. `maxDaysAwayBonus` (3) caps the ramp so a long absence makes a hit more likely, never guaranteed — the Organic value's "outcomes shouldn't be exact" applies to the cap as much as to the roll itself. Tuning: `ripenBonusPerDayAway` 0.15, `depletionPerAttendToday` 0.08, so the first attend after 3+ days away rolls at 0.75 while a fourth attend in the same sitting is already back down to the 0.3 floor's edge (0.06). Nothing here is shown to the player as a number anywhere — felt only through hit frequency, per the "felt, not read" discipline.

**Verified live (MCP, Play Mode, 7-21-26), one continuous run that exercised both halves of the ramp at once:** cold load, The Landing fresh (`daysAway=0` on a never-visited spot — no artificial bonus for a spot that's never been away from) → 6 attends in one sitting on day 0 depleted the roll exactly as predicted (`chance` logged at each tick: 0.30, 0.22, 0.14, 0.06, 0.00, 0.00 — all misses, unlucky but the depletion curve is what mattered) → `Force Rest (Advance Day)` (new debug menu item, calls `DayCycleManager.Rest()` directly — the same public method `BedrollAttendable` already calls, added so multi-day ripeness testing doesn't require walking to the bedroll and holding through a real attend cycle for every day advanced) three times to reach day 4 (4 real days past the last visit, clamped to the 3-day cap) → `Log Tending Spot State` (new debug menu item) confirmed `daysAway=3, nextEffectiveChance=0.75` → a fresh held attend then landed, in one hold, across four ticks: **first tick hit at the elevated 0.75 chance, yielding A Smoked Eel** (`daysAway=3, attendsToday=1`), and the very next three ticks in the same sitting depleted right back down (0.22 → yielded A Cut Withy, 0.14 → miss, 0.06 → miss) as `attendsToday` climbed — both the ripeness bonus and the same-visit depletion demonstrated in one uninterrupted sequence, not two separate tests. Final state: `unclaimed=1` (Alder Billet only), daylight `2/6`, 0 console errors/warnings. Cap behavior confirmed separately: a 4-real-day gap still read as `daysAway=3`, matching the authored `maxDaysAwayBonus`.

**A verification-methodology note:** because Play Mode runs in real time and each MCP round-trip (menu execute + console-log poll) costs real seconds, a single logical "Begin Attend → wait → Release Attend" step can tick more times than the literal sleep duration between the two calls suggests — the hold stays open (and daylight keeps draining) for as long as the wall-clock gap actually is, not just the explicit wait. Not a bug; a reminder for future sessions pacing MCP-driven holds, consistent with the "nothing happened and nothing logged" and "check daylight first" notes from 4.10/4.12.

**Content/scope:** no change to the pool, its items, or the hearth site, per this iteration's explicit scope — only `TendableSpotAttendable`'s roll logic and two new debug conveniences.

---

### Iteration 4.14 — Generalizing the Tending Thread: The Hearth Site

Scoped and built 7-21-26, same session as 4.13. Sean's steer: with ripeness landed and felt at The Landing, do the quick generalization pass 4.12 explicitly deferred — condense the hearth site's two remaining one-shot `PropertyPickupAttendable` pickups (A Char Knot, A Fused Clinker) into a second `TendableSpotAttendable`, so both sites carry the same tending-to-yield shape rather than only the river landing.

**Explicitly out of scope:** any change to `TendableSpotAttendable` itself — this is a pure data/content generalization, proving the component is genuinely site-agnostic the same way 4.4 proved acquaintance was entity-agnostic. No new properties, no new pool entries beyond the two that already existed, no change to the Collier/Bothy/Hearth Ring acquaintance tracks.

**Success criterion:** does the second site take the identical component with zero code changes — confirming `TendableSpotAttendable` (and 4.13's ripeness on top of it) is a reusable shape, not something fit to the river landing's specific content?

**Build notes:** condensed A Char Knot (`p4_char_knot`, `burns_slow`) and A Fused Clinker (`p4_fused_clinker`, `heavy_true`) into one new GameObject, **The Ash Bed**, placed centrally between their old positions near the Hearth Ring — same shape as 4.12's Landing, same component, same defaults (`yieldChance` 0.3, `ripenBonusPerDayAway` 0.15, `maxDaysAwayBonus` 3, `depletionPerAttendToday` 0.08), authored via hand-edited scene YAML (`TendableSpotAttendable` has no `UnityEngine.Object` references, so this is safe per the standing MCP-Unity limitation note — same precedent as 4.12's original build and 7-18-26's `TakenLedgerUI` addition). Their two `PropertyPickupAttendable` GameObjects were removed outright, and `Prototype4Debug.cs`'s now-stale `Teleport Player To Char Knot`/`Teleport Player To Fused Clinker` menu items were replaced with `Teleport Player To Ash Bed`; `Log Tending Spot State` needed no change since it already enumerates every `TendableSpotAttendable` in the scene generically.

**Verified live (MCP, Play Mode, 7-21-26):** cold load, 0 errors → The Ash Bed teleport + `Log Tending Spot State` confirmed it enumerated alongside The Landing, both fresh (`unclaimed=2`) → a held attend at day 0 reproduced the identical depletion curve already proven at The Landing (0.30 → 0.22 → 0.14 → 0.06 → 0.00 → 0.00, one silent hit in the unlogged first tick dropping `unclaimed` to 1) → `Force Rest` three times to day 3 → `Log Tending Spot State` showed The Ash Bed at `daysAway=3, nextEffectiveChance=0.75` while **The Landing stayed completely untouched** (`lastVisitDayIndex=-1`), confirming the two tending spots' ripeness state is fully independent → one more attend yielded 'A Fused Clinker' at the elevated 0.75 chance, exhausting the pool (`unclaimed=0`) cleanly with no errors. 0 console errors/warnings throughout; Greybox and Prototype3 regression gates re-run clean afterward.

**The go/no-go, answered:** zero changes to `TendableSpotAttendable.cs` were needed — only new scene data and a debug-menu swap. Confirms both 4.12's tending shape and 4.13's ripeness ramp are genuinely site-agnostic, the same finding 4.4 established for acquaintance a layer up.

---

# Iteration 4.15 — Site Character (Single-Site Pilot)

## The claim under test

Everything that currently repeats indefinitely (exhaustion) only ever penalizes. Everything that rewards (yield, Standing) fires once or drains to empty and stays there. There's no axis where a place carries a permanent, continuously-legible record of how it's been treated — something felt on sight and in tone, every visit, not inferred from a probability nudge buried under RNG and ripeness's own sliding modifier.

**This iteration tests only whether that axis is felt at all.** It's deliberately not attached to a goal, economy, or downstream system yet. Naming what it's *for* before confirming it's *felt* is the same mistake the Druid Framing spine made — reasoning a system into coherence before greybox testing it.

**Revision note:** an earlier pass of this doc proposed feeding `character` into ripeness's `ChanceMultiplier`. Rejected before build: a probability nudge on top of ripeness's own probability nudge on top of base RNG isn't a legible second axis, it's noise on an existing one — the same "designing around architectural reuse" trap the project has flagged before. This version gives `character` its own outlet instead of sharing ripeness's.

## Mechanism

One new float per site, `character` (0–1, starts at 0.5), on `TendableSpotAttendable` (The Landing — pilot site only). Unlike `exhaustion` (resets daily) and unlike Standing (latches once, done), `character` is:

- **Persistent** — never resets, carries across days and sessions.
- **Never resolved** — no threshold, no stage crossing, no "done." Always still moving.
- **Slow** — nudged a small amount per attend, not swung by any single visit.

### Input — pattern, not volume

Nudging `character` off raw attendance would be circular (attending more just proves you attended more). Instead it reads the same `daysAway`/`attendsToday` state 4.13's ripeness already tracks — no new tracking required:

- Attending while the spot is **ripe** (returned after time away) nudges `character` up.
- Attending while the spot is **depleted for the day** (already hammered today) nudges `character` down, by a larger step than the up-nudge.

This makes `character` a record of *how* the site has been treated — patient vs. greedy — not *how much* it's been visited.

### Outlet — visible independent of yield RNG

Two channels, both driven purely by `character`'s current band (low / mid / high), neither touching yield chance:

1. **Ambient tint.** Same lerp pattern already used for `SpotStageDef.Tint`/Standing — the site's sprite shifts slowly toward a "thriving" or "worn" tint as `character` moves. Legible on sight, no UI.
2. **Flavor line selection.** `character`'s band biases which flavor line fires on attend — and flavor fires on **every** attend, hit or miss, forever, even once the item pool (capped at 3 possible hits total) is fully exhausted. This is the piece that keeps the signal alive long after hits stop being possible, and incidentally fixes the "exhausted spot goes dead" problem as a side effect.

No new UI beyond tint, no change to yield chance, no NPC-facing changes, no second site.

## Explicitly out of scope

- Any feed into yield chance, ripeness, or any other probability — `character` is display/flavor only in this pilot.
- The Ash Bed — single-site pilot only, per standing discipline.
- Any tie-in to Standing, dominance, or the parked flow/valve thread — this is a *prerequisite* experiment for that reconnection, not the reconnection itself.
- Any purpose for the axis (goal, resource, unlock) — deferred until legibility is confirmed.
- New flavor-line content beyond enough lines to cover the three bands convincingly — full authoring pass waits until the pilot lands.

## Success criterion (playtest, not metric)

Across normal, undirected play at The Landing — including well after its three items are fully claimed — does the site read as "thriving" or "worn" through tint and tone alone, in a way that tracks how it's actually been treated? Does this remain legible once the yield pool is exhausted and RNG is no longer in play at all? If the tint/flavor shift isn't noticed or doesn't track treatment, that's a real negative result on the mechanism itself — cause to drop it, not just re-tune the bands.

## Why this is the right next pilot

It isolates the one open question — *is a persistent-but-still-live axis felt at all* — from every question about what it should mean, and from ripeness's own RNG so the two signals can't be confused for each other. If it lands, the next conversation is what to attach it to, and the flow/dominance thread becomes a much more informed reconnection. If it doesn't land, that's cheap to learn now, before any of that architecture gets built on top of it.

---

## Iteration 4.15 Build Notes (7-22-26)

Built and verified live in Play Mode via MCP; the Greybox + Prototype3 regression gate re-run clean afterward (0 errors, 0 warnings, both scenes loaded and entered Play Mode without issue — Greybox's wandering-thing spawner firing normally). New code confined to `TendableSpotAttendable` (a private runtime `character` float, two nudge-size fields, two tint fields, three banded flavor-line arrays, `RefreshTint()`/`CurrentFlavorPool()`, and a `DebugCharacterState()` accessor) plus one line in `Prototype4Debug.cs`'s `LogTendingSpotState()`. No shared script touched, no `DevelopableEntity`/track involved (matching 4.12's original reasoning: there's no stage to cross here).

**The mechanism, as built:** `character` starts at 0.5 and is nudged inside `OnAttentionComplete()`, reading the exact `daysAway`/`attendsToday` values 4.13's ripeness math already computes that same tick, before either is updated for this attend:
- `daysAway > 0` (this attend arrives after a real absence — the same condition that grants ripeness's bonus) nudges `character` up by `characterNudgeUp` (0.03).
- Otherwise, `attendsToday > 0` (the spot's already been worked at least once today) nudges it down by `characterNudgeDown` (0.06) — twice the step, per the doc's "greed should read faster than patience earns it back."
- The two conditions are mutually exclusive by construction (`daysAway > 0` only happens on a new day, exactly when `attendsToday` was just reset to 0), and a spot's very first-ever attend (both zero) nudges neither — there's nothing yet to have been patient or greedy about.

Two outlets, both reading `character` directly, neither touching `yieldChance`/ripeness:
- **Tint** (`RefreshTint()`): `Color.Lerp(wornTint, thrivingTint, character)` applied to the `SpriteRenderer` every attend (and once at `Start()` for the cold-load baseline) — continuous, not banded, per the Organic value.
- **Flavor pool** (`CurrentFlavorPool()`): a three-way band split (`< 0.3` worn, `> 0.7` thriving, else mid) selecting which line array the miss-branch (hit-or-exhausted-pool-or-real-miss, all the same branch already) posts from. Thresholds match the existing `0.3`/`0.7` convention `WildernessYieldAttendable.WithTendednessSuffix` already uses for tendedness banding.

**Verified live (MCP, Play Mode, 7-22-26):**
- **Cold-load baseline**: both The Landing and The Ash Bed read `character=0.50 (mid)`, tint `RGBA(0.775, 0.760, 0.725, 1)` — the exact `Lerp` midpoint between the authored `wornTint`/`thrivingTint` — confirming the mechanism is live on both sites even though only The Landing has bespoke banded flavor content authored this iteration (Ash Bed's `flavorLines` field was renamed to `midFlavorLines` in the scene YAML to preserve its two existing hearth lines; its worn/thriving arrays fall back to this iteration's generic C# defaults, same precedent 4.13 set before 4.14 explicitly generalized ripeness's content).
- **Up-nudge, isolated single-tick case**: a single held attend arriving after a day away moved `character` from `0.26` to `0.29` — exactly `+characterNudgeUp` — with tint shifting warmer to match.
- **Down-nudge and floor-clamp, repeated confirmations**: multiple same-day multi-tick holds produced clean `-0.06`-per-tick sequences (e.g. `0.50→0.44→0.38→0.32→0.26`, and separately `0.11→0.05→0.00→0.00`), the second sequence confirming `Mathf.Clamp01` correctly floors at `0.00` rather than going negative, with tint correctly locking to the exact authored `wornTint` at the floor.
- **Exhausted-pool persistence, the success criterion's central question**: after all three of The Landing's pool entries were claimed (`unclaimed=0`) during one of the depleting holds, further attends kept firing the miss branch cleanly — `character` kept updating, tint kept updating, no errors — confirming the axis (and the ordinary flavor-on-every-miss behavior it now biases) survives the pool going empty, exactly the "never goes dead" property the mechanism is supposed to guarantee.
- **A verification-methodology limitation, flagged rather than pushed through**: reaching the numeric `> 0.7` "thriving" band live wasn't achieved this session. MCP's per-call round-trip latency (already noted as a pacing hazard in 4.13's build notes) made single-tick holds unreliable to isolate — attempts to pace a lone ripe tick via an explicit `sleep` between Begin/Release Attend repeatedly let a second, same-day (and therefore depleting) tick slip into the same hold, which nets `-0.03` overall and pins `character` back toward the floor rather than climbing it. Since the up-nudge, down-nudge, clamping, and the continuous `Lerp` were each confirmed independently and exactly against hand-computed expected values at multiple points spanning most of the 0–1 range, and `CurrentFlavorPool()`'s high-band branch is structurally identical to the already-exercised low-band branch (a symmetric threshold comparison, not a separate code path), the mechanism is code-verified across its full range even though the literal "thriving" tint/flavor pairing wasn't independently eyeballed live. Worth an explicit live check next session if MCP pacing is easier to control, or by walking the loop manually in the Editor rather than through debug menu round-trips.

**The go/no-go is therefore only partially exercised**: the "worn" half of the success criterion (does the site legibly read as picked-over, does it stay legible once RNG is out of the picture) has a clean live pass. The "thriving" half rests on code-level confidence rather than an eyeballed live read this session — a real gap, named here rather than folded into "verified," per this doc's own standing discipline (see the 7-18-26 Build Notes for the precedent of flagging a verification gap rather than papering over it). A real playtest — walking the loop by hand across enough in-game days, not via debug-menu round-trips — is the actual test of the success criterion regardless.

---

# Iteration 4.16 — One Dependency Edge (Cross-Site Pilot)

## The claim under test

The five current vectors (state, acquisition, properties, one-off upgrades, daylight) sit in parallel — progress on any one of them never requires output from another. That's why the game still reads as thin even as content grows: more parallel tracks isn't interlock, it's more of the same shape. This pilot tests whether **one dependency edge between two existing, already-built tracks** is enough to make progress feel bottlenecked on something outside itself — the thing BotW's equipment-chain actually supplies, independent of BotW's chosen-goal/advancement structure, which this project deliberately doesn't use.

**Deliberately not a new mechanism.** `TaughtPropertyCondition` (4.10/4.11) already gates a stage on the player generically knowing a property — `WorldContext.IsPropertyKnown("p3_player", propertyId)` is checked without regard to *where* that property was learned. Nothing about the pipeline needs to change to make one site's gate reference a property whose only current teach-source is a different site. This pilot is a content/authoring change, not a systems change — same shape 4.11 confirmed for temperament, now aimed at cross-site interlock instead.

## Mechanism

One new stage on the Collier (Ash Bed), past his existing `heavy_true` taught stage, gated on `keeps_well` — the property currently taught only by the Netmender (The Landing), sourced from A Smoked Eel via the Smoking Racks (4.9/4.10).

Concretely:
- New `AcquaintanceStage` entry on the Collier's ladder, `TaughtPropertyCondition(keeps_well)`, same shape as the existing `heavy_true` stage — no new condition type, no new field.
- Reuses 4.11's `earlyTeachHintLine` texture for the "not ready to hear that yet" case — attending the Collier before the player knows `keeps_well` should read as a real, felt rejection, not a locked/greyed-out state. This is what makes the dependency discoverable without a quest marker: you find out by trying.
- No change to the Netmender/Landing side at all — `keeps_well` already becomes globally known the moment it's taught there, per the existing `"p3_player"` convention.

## Explicitly out of scope

- Any second cross-site edge — one dependency, one pairing, single-pilot discipline.
- Any UI, marker, or hint pointing the player toward the Netmender as the source — the whole test is whether the *rejection itself* ("not ready to hear that yet") is enough to send the player looking, the same way every other piece of this game withholds explicit direction.
- Any change to `TaughtPropertyCondition`, the teach pipeline, or acquaintance ladder mechanics.
- Reordering or regating any other existing stage.
- The `character` axis (4.15) — unrelated thread, not touched here.

## Success criterion (playtest, not metric)

Cold(ish) run, starting from a state where the Collier is Known but the Netmender hasn't been taught yet: does hitting the gated stage at the Collier create a real, felt pull back toward the Landing — "I need to go deal with something over there before I can continue here" — rather than reading as an arbitrary wall? And once the player *has* gone and taught the Netmender, does returning to the Collier and crossing the gated stage feel like the two threads paying off *together*, not like two unrelated errands that happened to share a checkbox?

If the pull isn't felt — if the player doesn't notice the connection, or notices it but it reads as busywork rather than payoff — that's the actual finding: a single dependency edge isn't enough density to feel like a web, and the answer is more edges (a small dependency graph, not one link), not a different mechanism.

## Why this is the right next pilot

It's the cheapest possible test of the actual hypothesis — that interlock, not volume, is the missing depth — because it costs zero new code and reuses two tracks already fully built and already playtested individually. If one edge lands, the next step is deliberately mapping a handful more (which existing stages could reference which existing properties) rather than continuing to add parallel content.

---

## Iteration 4.16 Build Notes (7-22-26)

Built and verified live in Play Mode via MCP; the Greybox + Prototype3 regression gate re-run clean afterward (0 errors, 0 warnings, both scenes loaded and entered Play Mode without issue). Zero code changed — the entire iteration is one new `AcquaintanceStage` array entry on the Collier's `acquaintanceStages` in `Prototype4.unity`, hand-edited into the scene YAML (safe per the standing MCP-Unity object-reference limitation: `AcquaintableAttendable` carries no `UnityEngine.Object` references, same precedent as every prior hand-YAML content pass in this doc), confirming the doc's own framing that this is a content/authoring change, not a systems one.

**The content:** a fourth stage, `banked_to_keep` ("Banked to Keep"), added past `shares_the_watch` — gated on `TaughtPropertyCondition("keeps_well")`, same shape as the existing `heavy_true` stage (`minAttends: 1`, `ripenChance: 0.5`, the same generous once-earned tier used for every taught stage so far). Tint moves from `shares_the_watch`'s amber (`0.5, 0.42, 0.3`) to a richer gold (`0.62, 0.5, 0.26`) — a further step along the same warming path, not a new hue family, since this is a deepening of the same taught relationship rather than a new kind of state. Content leans on the Collier's existing loneliness fingerprint (`trueStateNote`) the same way `shares_the_watch` did: the clamp now holds unwatched through the night, and "some nights... they climb down off the slope, same as anyone might" — a small further easing, not a resolution, per the Design Values' "no preaching" and "differentiation, not advancement."

**Verified live (MCP, Play Mode, 7-22-26), one continuous run exercising the actual dependency edge:**
1. Cold load, 0 errors — Collier teleported and driven to `shares_the_watch` (`Advance Acquaintance` x2 to Known, `Force Teach` for `heavy_true`, `Advance Acquaintance` once more to cross) — the pre-existing ladder unaffected by the new fourth entry.
2. **The gate confirmed closed**: with the player not yet knowing `keeps_well`, `Advance Acquaintance On Nearest Entity` correctly logged `"The Collier: nothing further to advance right now."` (`CanMakeProgress()` false, same as any not-yet-satisfied dependency) — and a *real* held attend at this state resolved as an ordinary flavor visit, 0 errors, no crash, no special-cased handling needed for a stage sitting past `shares_the_watch`.
3. **The real cross-site path, no debug shortcuts**: Smokehouse driven to Known (opening the Racks) → held attend at The Landing yielded *A Smoked Eel* on the first tick → held attend at the Smoking Racks revealed `keeps_well` on it, marking it under `"p3_player"` — the exact "does the player generally know this" flag `TaughtPropertyCondition` already reads, same as 4.10/4.11 — **with zero new code or wiring**.
4. **Back at the Collier, unprompted**: `Log Entity State` showed the interaction line already flipped to `"Hold E to speak of what keeps well"` before any debug tool touched the Collier — `TeachPending` picked up the real discovery path exactly as it did for the Netmender in 4.10 and the Collier's own `heavy_true` pairing in 4.11.
5. A real held attend resolved as the teach tick itself (`"The Collier: taught 'keeps_well'."`), ending the hold (no `ContinueAttending`) — then, after the wary gate's one-ripening-attend-per-day correctly ended a same-day hold at attend 1 (a miss), a `Force Rest` + one further real attend crossed: `"The Collier: developed - Banked to Keep!"`.
6. Final state: `acquaintanceStage=3`, tint `RGBA(0.62, 0.5, 0.26, 1)`, `knows=[burns_slow, keeps_well, heavy_true]`, overlay description and interaction line both switched to the new stage's authored data, the seeded-knowledge line correctly grown a third time (unplanned since 4.10, still holding): *"They speak of what burns slow, and of what keeps well, and of what is heavy and true."* 0 console errors/warnings for the entire session.

**A structural nuance worth flagging, not a bug:** `TaughtStageDef` (4.11) always resolves to the array's *final* entry when it's taught-gated — this is what lets `EarlyTeachAttemptPending`'s "not ready to hear that yet" hint surface before a taught stage is reached. Adding a second taught stage past the first means that hint's underlying check (`WorldContext.IsPropertyKnown(playerKnowerId, ...)`) now reads against `keeps_well` (the new final entry) rather than `heavy_true` even during the pre-`shares_the_watch` window where `heavy_true` is actually the nearer unlock. Since `earlyTeachHintLine`'s text is deliberately property-agnostic ("They're not ready to hear that from you yet"), this doesn't read wrong in play — but it means the hint's *trigger condition* is quietly keyed to whichever taught property is authored last in the array, not to whichever one is actually next. Also structural, independent of authoring order: `IsFullyAcquainted` latches true the moment the ladder first reaches a taught-gated `NextStageDef` (at Known, before `heavy_true` is even taught) and stays true for every stage after, which is why `EarlyTeachAttemptPending`'s hint can only ever fire pre-Known — this iteration's actual "is the gate felt" moment (post-`shares_the_watch`, pre-`keeps_well`) surfaces through the ordinary unchanged-flavor-visit texture instead, confirmed clean in step 2 above. Neither behavior needed fixing to hit this iteration's explicit scope ("no change to `TaughtPropertyCondition`, the teach pipeline, or acquaintance ladder mechanics") — flagged here per this doc's standing practice of naming a limitation rather than folding it into "verified," should a future multi-taught-stage entity make it worth revisiting.

**The go/no-go — mechanically confirmed, playtest still open:** the dependency edge is real and required zero new gating logic — the Collier's ladder was genuinely bottlenecked on a fact learned exclusively at a different site, discovered by trying rather than by any marker, exactly as scoped. Whether this reads as a *felt pull* back toward the Landing rather than an arbitrary wall — the actual success criterion — is a human-playtest question this MCP-driven verification pass can't answer on its own (same limitation this doc has flagged before for felt/tone judgments, e.g. 4.15's build notes); recorded here as confirmed-working, not yet confirmed-felt.

---

# Iteration 4.17 — War Scars: Recovery Lean (Single-Site Pilot)

## Where this comes from

Scoped 7-23-26, following a design-direction session (see IDEAS.md, "Protagonist Goal: Roaming Debt, and the Interconnected-Sites / War Scars Threads") that worked out what the protagonist's big-picture goal is and what it's measured against, without mechanizing anything beyond a light reframe of what P4 already does. That session named "war scars" — physical sites bearing visible damage from the war/collapse — as the first concrete build target, ahead of the separate (and explicitly bigger, procedural-generation-adjacent) interconnected-sites thread. This iteration is that pilot, scoped down to the single mechanism actually under test.

**This iteration deliberately carries no debt/roaming-goal framing.** The IDEAS.md session was theme-level and explicitly stopped short of mechanizing the goal itself, per the project's standing rule against building on argued-not-tested reasoning. What's being tested here is narrower and prior to that: whether a site whose fate is shaped by *how* it's attended, without ever presenting the player a choice, is felt as meaningfully different from ordinary tendedness drift. Debt-flavored flavor text and any framing language are explicitly withheld until this bare mechanism has been played.

## The claim under test

4.15 proved a site can carry a continuous, felt record of treatment (`character`) without that record ever resolving into a stage-cross or an outcome. This iteration asks the opposite-shaped question: can manner-of-attention drive a site toward one of two genuinely different *resolved* outcomes — without the player ever being asked to choose, per Tried Not Chosen — and have that resolution read as consequence rather than a coin flip?

**This iteration tests only whether the fork itself is felt and legible.** It is explicitly not testing what either resolved state is *for* (no downstream unlock, no debt payoff, no economy) — naming that before the fork itself lands would repeat the same reasoning-before-testing mistake IDEAS.md's session was careful to avoid mechanizing.

## Mechanism

**Entity:** one hand-placed POI-style wreck (a wrecked cart-and-goods scene, or similar small tableau consistent with the project's material-culture grounding) — not a Building. Chosen over a building specifically because it's structurally closer to existing `WildernessYieldAttendable`/`LandmarkAttendable` precedent than to the heavier `DevelopableEntity` building-repair shape, and finite salvage suits a POI better than a multi-stage repair track suits a first pilot.

**Starting stage:** `Ruined` — seeded already-collapsed, distinct from the ordinary dilapidated-with-latent-specialization starting stage buildings use elsewhere in Greybox. New stage, not a reuse of an existing one, since nothing existing means "already destroyed by the war" specifically.

**The fork — site's-choice, not player's-choice.** No menu prompt. A single new float, `recoveryLean` (0–1, same shape and precedent as 4.15's `character`), nudged inside the same kind of attend-completion hook 4.15 uses, reading manner of attention rather than presenting a decision:
- An attend that claims yield and ends there (grab-and-go, no lingering once there's something to take) nudges `recoveryLean` down, toward salvage.
- An attend that continues past the point where the yield pool has nothing left to give for the day (staying with the site once there's no reward left) nudges `recoveryLean` up, toward repair — the same "care read from presence without reward" signal 4.15's up-nudge uses for `character`.

At a threshold (same stage-cross convention every other track in this doc already uses), the site resolves once, into one of two end states:
- **Salvaged-and-laid-to-rest**: yield pool fully exhausted, entity becomes an inert landmark-class marker, flavor shifts to a closed/grave-marked register.
- **Repaired**: a minimal restored state — for this pilot, a single visible change (tint/silhouette shift) is enough to register the branch; a full multi-stage rebuild track is out of scope here.

## Explicitly out of scope

- **Any player-facing choice or prompt.** The entire point is that the fork is legible only in hindsight, from how the site responded — not offered as a decision.
- **Reclamation / a second transition after resolution.** A salvaged-and-laid-to-rest site drifting again later (nature or new settlers reclaiming it) is a real and named idea — but it's a second mechanism layered on one that hasn't been tested once yet. Deferred; see Next Steps.
- **Debt/roaming-goal flavor language.** Withheld deliberately this iteration — see "Where this comes from" above.
- **A full building-style repair track.** The repair end-state is a single visible change for this pilot, not a multi-stage rebuild.
- **Procedural rollout or a second wreck.** Single hand-placed pilot only, per standing single-pilot discipline.
- **Any feed back into `character`, Standing, ripeness, or the parked flow/valve thread.** `recoveryLean` is its own axis on its own entity, same isolation discipline 4.15 used.

## Success criterion (playtest, not metric)

Across normal, undirected play — attending the wreck a handful of times with no prompt or explanation of the mechanism — does the eventual resolution (salvage vs. repair) feel like a consequence of how the player actually treated the site, or does it read as arbitrary/unexplained? If a player can't retroactively make sense of which way it went, that's a real negative result on the mechanism, not just a tuning problem — the whole premise is that Tried Not Chosen still needs to be legible after the fact, even without being legible in advance.

## Why this is the right next pilot

It isolates the one genuinely new claim — a site's *resolved, branching* fate can be shaped by manner-of-attention alone, no choice presented — from every question about what that fate is *for*. That question (debt, cross-site consequence, reclamation) stays open and undecided in IDEAS.md on purpose. If the fork isn't felt here, in the smallest possible form, attaching thematic weight to it later would just be dressing up a coin flip — the same trap 4.15's own "revision note" flagged and turned away from with `character`.

---

## Iteration 4.17 Build Notes (7-23-26)

Built and verified live in Play Mode via MCP, 0 console errors/warnings across the full session. **The Greybox + Prototype3 regression gate was deliberately skipped this session at Sean's request** (he ran it manually) — the first time this doc's standing "every iteration" regression-gate discipline has been handed off rather than run by the agent; flagged here per this doc's own practice of naming what wasn't done rather than folding it into "verified." New code confined to `Mossmark.Prototype4` (`WreckAttendable`, new file) plus debug-driver additions to `Prototype4Debug.cs`; no shared script touched. MCP-Unity was unreachable for part of the session (the documented Windows-session-lock stall) — the script and scene wiring were prepared by hand (a manually-created `.meta` GUID, hand-edited scene YAML, same precedent as every prior hand-YAML content pass in this doc) and picked up cleanly once `Assets/Refresh` + `recompile_scripts` ran, 0 compile warnings.

**The mechanism, as built:** `WreckAttendable` is a plain `MonoBehaviour : IAttendable` (no `DevelopableEntity`), holding a single runtime float `recoveryLean` (start 0.5, never serialized, same "not shown to the player" precedent as `TendableSpotAttendable.character`) plus a 2-entry salvage pool (A Cracked Axle Pin / `heavy_true`, A Splintered Yoke / `split_prone`) checked against `TakenLedger` exactly like `TendableSpotAttendable`'s pool. Each day allows exactly one roll (`yieldChance` 0.5) against the still-unclaimed pool; a hit registers the item, posts its take line, and nudges `recoveryLean` down by `leanNudgeOnClaim` (0.2). Every further attend the same day — or any attend once the pool is fully claimed, any day — reads as "nothing left to give" and nudges `recoveryLean` up by `leanNudgeOnLinger` (0.09); a miss on an available roll is neutral (no nudge either way, since an RNG miss isn't evidence of manner). `ContinueAttending` is unconditionally `true` (same reasoning as `TendableSpotAttendable`), which is what makes "ends there" vs. "continues past" in the iteration spec fall directly out of whether the player releases E or keeps holding it — no new input, no new gate, just the existing hold behavior read for a signal it wasn't read for before. `GetOverlayDescription()`/`GetOverlayInteractionLine()` are fixed per state (`Ruined`/`Salvaged`/`Repaired`) and never reference `recoveryLean` directly — the fork is deliberately not legible in advance, per the iteration's explicit-scope requirement. Crossing either threshold (`salvageThreshold` 0.15, `repairThreshold` 0.85) locks the state permanently: tint swaps (discrete assignment, not `character`'s continuous lerp — a single visible change, not a drift), description/interaction lines swap, `EntityFeedback.TriggerPop()` fires once, and every attend after that is a fixed flavor line with zero further mechanism engagement (no more nudging, no more pool rolls, even if pool entries remain unclaimed).

**A deliberate design detail worth flagging:** Salvaged does not require the pool to be fully claimed. The doc's mechanism section describes "yield pool fully exhausted" as the Salvaged case, but the implementation resolves purely off `recoveryLean`'s accumulated history — a mixed history (one real claim plus some real lingering) can cross the salvage threshold with an item still sitting unclaimed forever after, which is exactly what happened in this session's verification run. This keeps the fork "Organic over deterministic" (the resolution reads the *pattern* of attention, not a hard pool-exhausted flag) rather than mechanically guaranteeing Salvaged the instant N items are taken.

**Verified live (MCP, Play Mode, 7-23-26), two full passes:**
- **Cold load:** `state=Ruined, recoveryLean=0.50, unclaimed=2`.
- **Pass 1 (fully real, no debug shortcuts) — the Repaired path:** one continuous hold on day 0 ran 5 ticks back-to-back (MCP's round-trip pacing kept the hold open regardless of intended wait time — the same class of limitation 4.13/4.15 already flagged, now confirmed to also govern *how many* ticks a single Begin/Release pair produces, not just their timing). Tick 1 missed the day's roll (neutral, `recoveryLean` held at 0.50); ticks 2–5 each correctly read "nothing left to give today" and nudged up by exactly +0.09 (0.50→0.59→0.68→0.77→0.86), crossing `repairThreshold` on tick 5 and resolving to *Repaired* live, mid-hold. `SpriteRenderer.color` confirmed matching `repairedTint` `(0.5, 0.58, 0.42, 1)` exactly. A further attend afterward posted a `repairedFlavorLines` line and left `recoveryLean`/`unclaimed` completely unchanged, confirming the permanent lock.
- **A re-entry gotcha, noted for future sessions:** exiting and re-entering Play Mode for a fresh instance (needed to reset the wreck for a second pass) also resets the player to the scene's authored spawn position, not wherever they were last teleported — a Begin/Release cycle with nobody in range silently produces zero ticks and zero console output (`AttentionManager` state stayed `Idle`, target `none`). Worth remembering alongside this doc's existing "check daylight first when an attend does nothing" notes (4.10/4.12/4.13) as a second silent-failure mode with the same fix (re-teleport, don't debug the mechanism).
- **Pass 2 (mixed real + debug-assisted) — the Salvaged path:** three real day-0 ticks (miss, then two lingers to 0.68) → `Force Rest` → a real day-1 hold whose first tick was a genuine claim: `"The Wrecked Cart: yielded 'A Cracked Axle Pin' (recoveryLean=0.48)."` — confirming the down-nudge branch fires correctly and matches the exact expected math (0.68 − 0.20 = 0.48) — followed by two more real lingers in the same hold (0.48→0.57→0.66). From there, isolating a genuine single-tick "grab and go" hold proved impractical under this session's MCP pacing (every observed hold ran 3+ ticks regardless of intended wait time, including a 0-wait Begin/Release pair that — separately — produced zero ticks at all, apparently absorbed by dispatch overhead before the first tick boundary), so the new `Debug Nudge Wreck Toward Salvage` menu item (wrapping `WreckAttendable.DebugNudgeLean`, same shortcut role `Advance Acquaintance`/`Force Teach` play elsewhere in this doc) closed the remaining distance. Resolved to *Salvaged* at `recoveryLean=0.06` with one pool entry (A Splintered Yoke) never claimed. `SpriteRenderer.color` confirmed matching `salvagedTint` `(0.5, 0.5, 0.52, 1)` exactly. A further attend confirmed the lock: `state`/`recoveryLean`/`unclaimed` all unchanged, correct `salvagedFlavorLines` line posted.
- 0 console errors, 0 warnings across both passes.

**What this session did and didn't answer:** the mechanism is confirmed working end-to-end in both directions — the claim/down-nudge and linger/up-nudge branches both fire correctly with exact expected magnitudes, both thresholds resolve and lock correctly, and the pre-resolution state is confirmed to expose nothing (no tint/flavor tell) regardless of `recoveryLean`'s current value. What it did *not* answer is the iteration's actual success criterion — whether the resolution *feels* like a consequence of how the site was treated, retroactively, with nothing legible in advance. That's a human-playtest question no MCP-driven pass can answer, the same distinction this doc has drawn before for felt/tone judgments (4.15, 4.16). Recorded here as mechanism-verified, not yet playtested.

---

## Playtest Findings (Sean, 7-24-26)

Regression gate passed (Greybox + Prototype3 both re-run clean, this time by Sean rather than the agent). The 4.17 mechanism itself surfaced a real bug rather than just a felt-legibility question:

**The core problem — checking and lingering were the same tick.** The only way the player ever learned "there's nothing more to take today" was by triggering the linger branch itself — but that branch was also the one nudging `recoveryLean` toward Repair. There was no way to *ask* whether more was available without the act of asking counting as an answer. Concretely: claim something, then attend once more just to check → the check reads as "nothing left today" and nudges up, very nearly cancelling the claim's down-nudge even though the player never intended to linger. Worse, on a later day, retrying after a plain miss (reasonable — "did I do that right?") triggers the identical linger-coded tick. The resolution ends up depending less on the player's actual manner and more on whether they happened to poke the site exactly once more after learning it had nothing left — which is a UI-legibility accident, not a read of intent.

**A second, separate finding:** with only one such entity in the scene, there's no way to know there are even two possible outcomes, let alone that behavior shapes which one lands. Real, but out of scope to fix here — it's the single-pilot discipline itself producing this, not a bug, and it only resolves once there's a second wreck (or several) to compare against. Carried into Next Steps rather than acted on.

**A third finding, flagged rather than settled:** the wreck can cross either threshold — Salvaged *or* Repaired — without the pool ever being fully claimed, and once resolved, whatever's left sits permanently inaccessible (the resolved-state branch in `OnAttentionComplete()` never rolls the pool again). The original 4.17 build notes above named this for the Salvaged case specifically and framed it as a deliberate "Organic over deterministic" choice; Sean's read is that it's a real open question rather than a settled one — noted here as unresolved, not reaffirmed. No design direction chosen yet. Carried into Next Steps.

---

## Iteration 4.17 Revision Build Notes (7-24-26): Day-Resolved Lean

Same session as the finding above. Per-attend nudging is gone entirely — `recoveryLean` now resolves once per day, at rest, off that day's total attend count alone (`HandleDayAdvanced`, subscribed to `DayCycleManager.DayAdvanced`, same hook `WildernessYieldAttendable`/`NpcAttendable` already use for their own per-rest drift). Hit/miss no longer has any bearing on the lean — only how many times the player showed up. `OnAttentionComplete()` keeps the existing pool-roll logic (one roll per day, `attendsToday == 0` gates it) but no longer touches `recoveryLean` at all; it only increments `attendsToday`. The formula: `dayNudge = (attendsToday − neutralAttendsPerDay) × leanPerAttendStep + jitter` — fewer attends than `neutralAttendsPerDay` (3) nudges down toward Salvage, more nudges up toward Repair, continuous rather than a hardcoded per-count table (the Organic value's own objection to discrete steps), with `leanJitter` (±0.02) keeping the same attend count from producing a bit-identical nudge every time.

This fixes both halves of the bug at once: a claim-then-recheck day still nets toward Salvage (attendsToday=2 still nudges down, just softer than attendsToday=1), and nothing can ever resolve mid-attend or be misread from a single ambiguous tick, since the whole day is judged once, retrospectively, at its close. It also fully decouples the lean from RNG — a string of unlucky misses no longer has any chance of quietly biasing the read, which is a stronger fit for the iteration's own "does it read as consequence or as a coin flip" success criterion than the original per-tick design was.

**A bug caught before it shipped:** the first pass of the formula was written `(neutralAttendsPerDay − attendsToday)`, which is backwards — it nudged *up* for few attends and *down* for many, exactly inverted from the intended salvage/repair mapping. Caught by re-deriving the expected sign by hand before testing, fixed to `(attendsToday − neutralAttendsPerDay)` prior to any Play Mode verification.

**Verified live in `Prototype4.unity` only (MCP, Play Mode, 7-24-26), per Sean's request — Greybox/Prototype3 regression left to him:**
- Cold load: `state=Ruined, recoveryLean=0.50, attendsToday=0, unclaimed=2`.
- A 2-attend day (one claim, one "nothing left" tick — `recoveryLean` held at 0.50 through both, confirming no per-tick nudge fires anymore) → `Force Rest` → `"day closed with 2 attend(s), recoveryLean nudged -0.13 to 0.37."` — negative nudge for a below-neutral day, magnitude matching `(2−3)×0.12 ± 0.02`.
- A 6-attend day (daylight-capped hold; the second pool entry was also claimed mid-hold, `recoveryLean` still held flat until rest) → `Force Rest` → `"day closed with 6 attend(s), recoveryLean nudged 0.37 to 0.74."` — positive nudge for an above-neutral day, correct sign, magnitude matching `(6−3)×0.12 ± 0.02`.
- A second 6-attend day (pool now empty, every tick read "nothing left to give today" from tick 1) → `Force Rest` → `"day closed with 6 attend(s), recoveryLean nudged 0.37 to 1.00."`, immediately followed by `"resolved - Repaired (recoveryLean=1.00)."` — crossed live, correct direction. `SpriteRenderer.color` confirmed matching `repairedTint` exactly.
- 0 console errors, 0 warnings throughout.

Not re-verified this session (unchanged by this revision, already confirmed working in the original 4.17 pass): the Salvaged resolution branch, the permanent post-resolution lock, and the pre-resolution no-tell property. The day-resolution mechanism is the only thing that changed, and it's the only thing this pass targeted.

---

## Next Steps

Merges what used to be two separate lists ("Where to pick up," "After this") into one. 4.1–4.11 all held across four rounds of playtesting (7-17-26, 7-18-26, 7-21-26): "getting to know an already-alive place" is a real, attention-spending activity distinct from both development-by-tending (Greybox) and teaching (P3); the first result-of-knowing (4.9's Smoking Racks) landed as a genuinely earned consequence; and the teach-loop composition (4.10, 4.11) works mechanically on both a sitting-gated and a wary entity. None of that is repeated below — see each iteration's own Build Notes and Playtest Findings sections above for what's already landed and verified. What follows is only what's still open and unscoped.

- **Does a taught crossing need its own downstream payoff?** The 7-21-26 playtest found composition works, but a taught crossing doesn't yet feel distinct from a plain acquaintance crossing — nothing currently distinguishes what teaching unlocks from what patient attention alone would eventually show. Now that there are two taught pairings to compare (Netmender/`keeps_well`, Collier/`heavy_true`), worth checking whether that finding still holds, and whether downstream impact — a new bias, a new access, a new option elsewhere — needs to be part of what teaching *means*, the way 4.9's earned surface gave plain acquaintance its own payoff.
- **Depth-scaled teaching reception**: a taught property landing differently depending on how deep the acquaintance was when it was taught (e.g. Acquainted vs. Known) — deliberately deferred by both 4.10 and 4.11 as a bigger claim than either needed to test.
- **Strategic depth-vs-breadth weight** (7-18-26, see Build Notes above): no real tension yet between diving deep on one site vs. spreading across several — still an open question, no shape committed. Iteration 4.13 (built 7-21-26) landed the ripeness mechanism this item named as one candidate direction, but only at The Landing — whether it actually changes how depth-vs-breadth *feels* across a whole site (or several) hasn't been playtested yet, since 4.13 verified the mechanism live via MCP, not through an actual multi-site play session. Whatever the fix, avoid anything with a calculable optimal order (a menu choice wearing a spreadsheet, against "Tried, not chosen"). The other organic-leaning direction still on the table: extend 4.9's cross-site bonus-bias seam (Osier Bed → Smoking Racks) so knowing several sites occasionally opens a richer read elsewhere, not as a per-visit bonus. Also worth reconnecting once picked up: Greybox's parked flow/valve thread (`WorldSite`/dominance/flow-reserve, Iterations 42–54) — the Tending Thread's yield pools are the natural upstream sources a flow system would eventually throttle.
- **Richer interaction than hold-E** (notes, not scope — needs its own pilot when picked up): hold-to-attend reads as "a placeholder for something more rich and involved" (7-17-26 finding). The design values rule out outcome menus, recipe grids, and QTE-shaped skill checks — all of them make the player pick what happens — but not enriching the channels attention already has: **where you stand** (position relative to the entity as expressive input — the player still only chooses where to stand and how long to stay), **when you come** (time-of-day/rhythm as texture via the existing `IOutcomeModifier` pattern, never a puzzle requirement), **what you bring** (attention flavored by what the player knows or carries — P3's teach shape generalized, already half-built). The wrong fork is a second input verb; the right fork is the world reading more out of the one verb. A one-entity pilot on positional attending is the likeliest next step — most bodily, least puzzle-prone.
- **A glanceable acquaintance-depth summary**: the item-discovery side of this itch is already answered (`TakenLedgerUI`, reused into P4 on 7-18-26). Still open is the acquaintance-depth side — a dedicated non-modal "who and what have I come to know, and how well" summary, distinct from the overlay's per-entity applied-upgrades list (which already does this one entity at a time but reads as slightly ledger-like). Worth building only if a future playtest names this itch specifically for acquaintance, not assumed here.
- **Procedural rollout**: multiple sites, randomized count and archetype mix, using `WorldGenerator`'s existing clustering/member-pool machinery (Iterations 47/51). 4.6 confirmed two hand-authored sites read as genuinely different voices — but procedural generalization itself is still untried, and stays parked until one of the threads above makes it worth the machinery.
- **A single instance of a resolved-fork entity can't teach the player it's a fork at all** (7-24-26 playtest): with only one wreck in the scene, there's no way to know two outcomes even exist, let alone that manner of attention shapes which one lands — a limitation of the single-pilot discipline itself, not a bug, and not fixable without a second wreck (or several) to compare against. Worth returning to once 4.17 generalizes past its single-site pilot.
- **Unclaimed pool items become permanently inaccessible once the wreck resolves** (7-24-26 playtest): either threshold can cross before the 2-item pool is fully claimed, and nothing ever rolls it again after resolution — genuinely open, not concluded either way. Candidate directions if it does need fixing (none chosen): auto-resolve any remaining items into the outcome that fired (e.g. Salvaged sweeps the rest as a closing take, Repaired folds them into the restoration and they're simply gone); let the resolved state still roll the pool at a reduced rate; or decide a stranded item is itself the intended texture (a Flame-Sword-adjacent "some things are just lost" note) and leave it alone. Needs a decision, not just an implementation, before touching code.
- **A salvaged site isn't necessarily a permanent dead end**: 4.17 deliberately defers this, but the idea itself is worth keeping live — a salvaged-and-laid-to-rest wreck drifting again later, reclaimed by nature or by other people (new settlers, a different NPC's use), the same way `character` drifts toward worn rather than sitting still. Structurally this would be a second transition layered on top of 4.17's resolved end state, not a new axis — closest existing precedent is `character`'s continuous drift itself. Not worth building until 4.17's single fork is confirmed felt; named here so it isn't lost.
- **Stakes / the possibility of loss** (explored 7-21-26): a same-session pilot tested whether attending needs real risk, not just accrual, to stop feeling flat — a wary entity's trust regressing on a same-day attend pushed past its daily gate, tell-then-roll, re-earnable. The mechanism itself worked cleanly (verified live: the warning landed before the roll, releasing during it was safe, a sour regressed and re-earned correctly, the floor held), but it was backed out before playtesting: the current attend/day pacing (2-second ticks, ladders that cross in 2-3 days) doesn't give "going too far" enough room to register as a felt risk rather than a coin flip. Worth revisiting once the loop has more breathing room — the depth-vs-breadth and tending-to-yield threads above are the more likely path to that room than the stakes mechanism needing rework. Not concluded against; concluded *too early for*.

None of the above is mutually exclusive, and none is chosen yet — which to build next is a real decision, not made here.
