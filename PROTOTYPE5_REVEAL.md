# Mossmark — Prototype 5: Reveal

> New scene, built alongside `Greybox.unity`, `Prototype3.unity`, and `Prototype4.unity`, not on top of any of them. Tests whether attending your *surroundings* — undirected attention, spent without a target, that gradually turns unmarked things in the world into things you can attend to — produces a wantable moment-to-moment loop, then bridges that loop onto the acquaintance baseline kept from P4.

> **Status (7-29-26): the first approach was scrapped after Iteration 5.1.** The claim under test is unchanged; the mechanism is entirely different. 5.1 put the reveal on *the ground itself*, subdividing a clearing into attend-zones — built, verified, and immediately wrong in play. The redirected thread starts at 5.2 below. The original 5.2–5.5 were never built and their numbers are reused. See **Appendix: the scrapped approach** for what 5.1 was, what survives it, and what it taught.

---

## Premise

P4 answered "does deepening acquaintance with an already-alive entity feel like a wantable activity" — yes, confirmed across 4.1–4.9. What P4 never tested is *where the player's attention goes to find that entity in the first place*. Every P4 (and Greybox, and P3) attendable is pre-flagged and immediately legible as an attendable; everything else in the scene is dead space the player walks through without engaging.

P5 tests the missing piece. The world contains things — visible, grey, mute. Standing near one tells you nothing: no name, no description, no "hold E to…", not even confirmation that there is anything there to attend to at all. The only way to find out is to stop and attend to *the place you are standing in*, spending daylight on no particular thing, and let whatever is nearby come into focus on its own.

**The atomic claim under test:** does undirected attention — held on your surroundings, costing real daylight, most of it turning up nothing — feel like a wantable activity on its own, and does something resolving out of it feel like *noticing* rather than like a collectible spawning?

---

## The mechanic

Precise enough to build against; the numbers are opening guesses, not commitments.

**1. Unnoticed things are visible but mute.** An unnoticed object renders as a grey triangle. It has no overlay, no name, no interaction line, and is not a valid attention target — walking up to it and pressing E does nothing, because as far as the attention system is concerned it isn't there.

**2. With nothing attendable in range, E attends your surroundings.** This is the fallback, not a mode: if a *noticed* attendable is in range, E attends it directly exactly as in every prior prototype. Only when there is no such target does the hold resolve to the ambient attend.

**3. The ambient attend is a real attend.** The player rocks. Daylight drains continuously while held, at a slower rate than focused attention (opening guess: one daylight per ~3s tick, against ~2s for a direct attend — undirected attention is cheaper per second but buys nothing specific). Day-phase messaging advances off it normally. It ends when E is released, or when the day does.

**4. While it runs, everything unnoticed nearby is being silently attended.** Any unnoticed object within a noticing radius of the player (opening guess: 2–3 units) accumulates dwell time. Cross the dwell threshold (opening guess: 1–2s) and it is **noticed**: it gains color, gains a description (the "unknown" register — it is now a thing you can attend to, not yet a thing you know), gains its "Hold E to…" line, and becomes a normal target from then on, permanently. Everything that crosses, crosses; there is no one-at-a-time queue. Dwell resets when an object leaves the radius.

**5. The ambient attend does not hand off.** When something is noticed mid-hold, the hold stays ambient. It does not retarget, does not end, does not offer. The player releases E and chooses to attend the thing, or doesn't. This is load-bearing — see below.

**6. Not everything grey is something.** Some of the grey shapes in the world are just shapes. Standing among them and attending turns up nothing, ever. This is what makes point 1's uncertainty real rather than decorative.

**Why point 5 is load-bearing.** Cutting over on notice would make the ambient attend a search action that terminates on a hit — the player would hold E, get interrupted by a find, and the whole thing would read as a scan with a payout. Refusing to hand off keeps the two verbs distinct: attending your surroundings is its own activity that happens to change what's available, and attending a specific thing is a separate decision made afterwards, standing still, with the day already partly spent. It also protects "Tried, not chosen" — the player never picks what surfaces, only where they stood and how long.

---

## What carries forward from P4 (the baseline)

Kept, unmodified, as the acquaintance layer P5 builds its backdrop around:

- **Acquaintance as a `DevelopmentTrack`** (4.2/4.3) — attention-count-gated, zero effect on the entity, multi-stage reveal of a fixed true state. This is P5's seeded NPC's behavior, verbatim, once it has been noticed.
- **The teach stub** (4.10/4.11) — present as a stub only; not extended, not exercised beyond confirming it still composes.
- **Yield-pool sites, pre-ripeness** (4.12/4.14's shape, not 4.13) — `TendableSpotAttendable` as a condensed multi-item foraging spot, flat `yieldChance`, no ripeness ramp, no character drift.
- **`EntityFeedback`'s existing tint/pulse vocabulary** — noticing should read as a variation on this, not a new visual language.
- **`DayCycleManager`** — daylight as the attend cost, same as every prior prototype.

**Explicitly not carried forward, on purpose:** ripeness (4.13), site `character`/drift (4.15), any chaining/dependency-gated stage (4.16, 4.18–4.20), the wreck fork mechanism (4.17 — good idea, wrong prototype), properties/knowledge transfer beyond the existing stub, procedural generation, the debt-vs-witness dynamic.

---

## Reuse Discipline

Same standing rule. Nothing built for P5 may modify a shared script's existing behavior or mutate an SO instance `Greybox`/`Prototype3`/`Prototype4` depend on. New behavior comes from new components, new conditions, or new interfaces layered on `IAttendable`/`AttendableDetector`. Regression gate every iteration: `Greybox`, `Prototype3`, `Prototype4` all still load and play clean, 0 console errors.

**One amendment, signed off 7-29-26.** The ambient attend needs the attention framework to resolve to a fallback target when no zone is in range — before this, `CurrentTarget` was null in that case and `AttentionManager` refused the hold. Three options were considered:

1. **Additive change to `AttendableDetector` + `AttentionManager`** — an optional fallback attendable, used only when nothing else is available. *Chosen.* It is additive, not a modification: with no fallback registered both classes behave identically, so the three prior scenes are unaffected by construction rather than by testing. And it is honest about scope — "attend the place you're in when there's nothing specific" is a change to the attention verb itself, not a P5 trick, so it belongs in `Mossmark.Attention` permanently.
2. **Park the ambient zone far away with a huge collider**, so it is always in range but always the farthest and therefore only ever wins when nothing else is. Zero shared-script change, works exactly right, and is a hack nobody reading it in three months will forgive. Rejected.
3. **A P5-local detector subclass.** Avoids touching the shared file but duplicates the targeting loop, which is the one piece of logic that most needs to stay single-sourced. Rejected.

The shape that landed keeps `CurrentTarget` meaning exactly what it always meant — nearest zone in range, null when there is none — and adds `FallbackTarget` beside it. That distinction is what keeps the overlay quiet: `AttendableOverlayUI` reads `CurrentTarget` when idle, so a fallback alone never puts a panel on screen, and reads `AttendingTarget` while attending, so the ambient hold shows its own panel and progress bar. No overlay change was needed.

Everything else stays strictly additive: unnoticed-ness is a new component, the ambient attendable is a new component, and no existing attendable type learns anything about either.

---

## Core loop under test

Walk through a space holding grey, mute shapes → stop somewhere and attend your surroundings, spending daylight on nothing in particular → most of the time that is all that happens → sometimes something nearby resolves into a thing with a name and a "hold E to…" → you release, and decide whether it is worth the rest of your day → the noticed thing then runs P4's acquaintance loop as normal.

---

## Iterations

### Iteration 5.2 — The Ambient Attend, With Nothing To Find

Hold E anywhere in an empty space. The player rocks, daylight drains at the ambient rate, the day advances through its phases, the overlay reads as attending to the surroundings rather than to a thing, and the hold ends on release. No objects in the scene at all — nothing hidden, nothing to notice, no payoff of any kind.

**Explicitly out of scope:** any object, noticing, dwell tracking, rate tuning beyond a first guess.

**Success criterion:** does spending real daylight on nothing in particular read as an *activity* — attending a place — or as an idle animation with a cost attached? This is the true go/no-go and it is deliberately harsher than 5.1's was: if standing in a place and attending it is not itself worth doing, then noticing is just a loot roll with extra steps, and the thread is dead again.

*Built and verified 7-29-26 — see Build Notes below.*

---

### Iteration 5.2.1 — No Overlay At All For Ambient Attending

Ambient attending shows no UI whatsoever: no name panel above the player, no progress bar, no detail panel. The player rocks, the day clock advances, and that is the entire feedback surface. Reversible from the Inspector without a recompile. *(Narrowed at 5.4.1: nothing is drawn for the ambient attend itself, but a thing revealed mid-hold now names itself immediately.)*

Two reasons, and the second is the one that matters past this iteration:

- **This should be a visual experience.** The player is walking through wilderness taking notice of what is even there; it should feel fluid and meandering, and a black panel pinned over their head while they stand still is the opposite of that.
- **The progress bar is measuring the wrong clock.** It shows the 3s daylight tick. From 5.3 onward the thing the player actually cares about is per-object dwell, which is a different clock entirely — different duration, different start, one per nearby object rather than one per player. Showing tick progress would train the player to read a meter that is about to stop meaning what they think it means. **This constrains 5.3:** if dwell needs a signal, it belongs on the object, not on the player, and it should be a visual property of the thing coming into focus rather than a bar.

**Explicitly out of scope:** the day-phase HUD (kept — the day passing is exactly the feedback the ambient attend is supposed to have), the player rock (kept — it is now the *only* "you are doing something" tell), and any overlay behavior for non-ambient attendables, which is untouched.

**Success criterion:** with no text at all, does the ambient attend still read as a deliberate action rather than as the game having stopped responding? The rock and the day clock are carrying the entire signal; if that is too thin, the answer is a *visual* addition, not the panel coming back.

*Built and verified 7-29-26 — see Build Notes below.*

---

### Iteration 5.3 — Unnoticed Things, Noticed By Dwell

Seed a handful of grey mute objects into the 5.2 space. Ambient attending near one for the dwell threshold notices it: color, description (unknown register), interaction line, permanently targetable. Multiple objects in radius all cross independently. The ambient attend keeps running through the whole thing and never hands off.

**Explicitly out of scope:** what the noticed objects actually *do* when attended (5.5); inert decoys (5.4); any variation in noticing rate (5.7).

**Success criterion:** does something resolving out of the ambient attend read as *noticing it* — a thing that was always there — or as a spawn? And does the no-hand-off rule feel right, or does it read as the game withholding? The second half is the one most likely to come back wrong, and the fallback if it does is a *softer* signal at the moment of notice rather than a hand-off.

*Built and verified 7-29-26 — see Build Notes below.*

---

### Iteration 5.4 — Some Of Them Are Nothing

Add grey shapes that never resolve, mixed in among the ones that do, indistinguishable beforehand.

**Explicitly out of scope:** any tell, any hint, any way to know in advance — that is the whole point.

**Success criterion:** does not knowing whether a place holds anything make attending it feel like a real expenditure, or does it just feel like being wasted? If the answer is "wasted," the fix is the ratio and the size of what's found, not the removal of decoys — a world where everything grey pays out isn't a world you can attend to, it's a checklist.

**The one real design question: what does a decoy do when its dwell crosses?** Three answers, and the choice decides whether the iteration lands.

1. *It accrues no dwell at all* — stays flat grey while you stand there. Rejected: 5.3's tint **is** the dwell tell, so a thing that refuses to warm announces itself as nothing within a fraction of a second. That is a tell, and it makes finding out cost nothing, which is the exact opposite of the success criterion.
2. *It warms, then stalls just short of crossing.* Rejected for the same reason one step later, and worse — a bar that fills to 90% and stops is the game visibly withholding, the failure mode the doc already flags at 5.3.
3. **It resolves, and what it resolves into is nothing.** Chosen. A decoy is identical in every respect — same grey, same radius, same threshold, same continuous warm-up — and the difference lands *only* at the moment of crossing: it settles to a dull, cooler colour instead of the warm noticed one, does not pop, and never becomes targetable.

Option 3 is the one that isn't a lie. The player did notice it; what they noticed is that it's a stump. Finding that out costs exactly what a find costs, which is what makes the expenditure real, and the negative result is genuine information rather than a withheld reward — consistent with **the world was here before you**, where most of what's in a wood is just wood.

The settle is a short fade *down* from the warm colour it had reached, not a snap, so the felt beat is "it came into focus, and what came into focus was ordinary." And per 5.2.1 it posts no notification: grey → warm + pop means something, grey → dull and quiet means nothing, and both are read in peripheral vision rather than in text. The settled colour has to be distinguishable from unnoticed grey at a glance or the player re-dwells on the same stump forever and the space becomes noise — that legibility is *after* the fact only, and costs nothing beforehand.

Opening ratio is 4 nothing among 5 something. One decoy is deliberately placed ~1.4 units from a real thing so a single hold resolves one of each, which is the mixed outcome the iteration is actually about.

*Built and verified 7-29-26 — see Build Notes below.*

---

### Iteration 5.4.1 — A Revealed Thing Says Its Name Immediately

Introduced at 5.3 but only visible as a problem once decoys landed: a thing noticed *during* an ambient hold showed nothing at all until the hold was released. A thing whose name you cannot see has not really been revealed — the pop said *something happened* without saying what, and with half the crossings now resolving into nothing, "something happened" is precisely the ambiguity the reveal is supposed to settle.

So the moment a thing is noticed, its name and its description panel appear, mid-hold. What does *not* appear is the interaction line: attending it is genuinely blocked, because the player is already attending in another capacity. The overlays are visible as soon as a thing is revealed; the ability to interact is gated by the hold in progress, and the "Hold E to look closer" line arrives on release. No progress bar either, for 5.2.1's reason — the running hold is not that thing's hold, and showing its progress over that thing's name is the mismatched-clock confusion the bar was removed for in the first place.

**Explicitly out of scope:** any change to what attending a noticed thing does, and any change to the direct-attend overlay.

**Success criterion:** does naming the thing at the moment of notice make the crossing legible without making the ambient attend feel like a UI mode again? The risk is the opposite of 5.2.1's: panels appearing mid-hold could re-clutter the thing that was deliberately stripped bare.

*Built and verified 7-29-26 — see Build Notes below.*

---

### Iteration 5.5 — A Noticed Thing Is A P4 Entity

One of the objects noticed in 5.3 is an NPC running P4's acquaintance `DevelopmentTrack` verbatim. Once noticed it behaves exactly as a P4 entity does, unmodified.

**Explicitly out of scope:** any interaction between how it was found and how it develops; buildings or sites; ambient-rate variation (5.7).

**Success criterion:** does having found someone this way — stumbled into by standing still and paying attention, rather than walked toward because they were flagged — change how the first acquaintance attend feels? This is the actual bridge claim: ambient attention as backdrop, acquaintance as the thing it is a backdrop *for*.

*Built and verified 7-29-26 — see Build Notes below.*

---

### Iteration 5.6 — A Second Kind Of Found Thing

Add one `TendableSpotAttendable` (P4.12/4.14 shape, flat yield chance, no ripeness) elsewhere in the same space, noticed the same way.

**Explicitly out of scope:** ripeness, character, any differentiation between how the NPC and the site are noticed unless a real reason emerges in play.

**Success criterion:** do two different kinds of found thing (a person to get to know, a place to tend) inside one attended space read as one coherent place, or as two unrelated mechanics dropped into the same clearing?

*Built 7-29-26, playtested live by Sean 7-30-26 — see Build Notes below. The playtest reopened questions the mechanism-level verification couldn't reach; read those before treating 5.6 as settled.*

---

### Iteration 5.7 — Familiarity Widens What You Notice (Rate, Not Gate)

**Paused 7-30-26, pending a rewrite.** On rereading this section against the 5.6 playtest findings, Sean doesn't think it's written quite right and wants to sit with it before it's built. Left as originally drafted below for now — treat this as provisional, not a spec to implement against.

Once 5.5/5.6 hold: tie the noticing radius, or the dwell threshold, or both, to how well the player knows what is already around them — you notice more, and sooner, in a place you have spent time in. Continuous, never a threshold unlock, never blocked beforehand. The explicit distinction to hold onto from the P4 postmortem: this is a **rate**, not a **gate**.

**Explicitly out of scope:** any version that makes something un-noticeable below a stage; any version shown to the player as a number.

**Success criterion:** does familiarity-sharpens-noticing read as its own quiet payoff for depth, without ever implying a required order or a blocked action?

---

## Explicitly out of scope for all of 5.2–5.7

- Properties, teaching beyond the existing stub, knowledge-as-currency
- Ripeness, site character/drift, any chaining or dependency-gated stage
- The wreck fork mechanism
- Multiple sites, procedural generation
- The debt-vs-witness dynamic, or any player-facing framing of motivation
- Any UI beyond overlay text and `EntityFeedback` — in particular no "things noticed" counter, no found/total, no minimap, no directional tell. Presence, never progress.

---

## Why this is the right pilot set

It isolates the one genuinely new claim — that undirected attention, spent on a place rather than a thing, is itself wantable — before spending anything on how it composes with what is already proven (acquaintance) or what is parked (everything else from the P4 postmortem). 5.2 is the cheapest possible statement of that claim and the harshest possible test of it. If 5.2 doesn't hold, nothing past it is worth building; if it does, 5.5/5.6 are the cheapest test of the actual thesis: noticing as backdrop, acquaintance as figure.

---

## Build Notes — Iteration 5.2 (7-29-26)

Built in `Prototype5.unity`, which was stripped back to its scaffold: The Clearing removed, `RevealField`/`RevealPatchAttendable` deleted, `maxDaylight` 36 → 12. New code is one component, `AmbientSurroundingsAttendable` (`Mossmark.Prototype5`), on the Player.

**The shared-framework amendment, as built.** Two additive changes, no existing behavior modified:
- `AttendableDetector` gains `FallbackTarget` + `SetFallbackTarget()`. `CurrentTarget` is untouched. `IsInRange()` returns true unconditionally for the fallback — it has no zone and no position, so it is never out of range.
- `AttentionManager` gains `FallbackTarget` (pass-through) and a private `ResolveTarget()` = `CurrentTarget ?? FallbackTarget`, used in the three places that previously read `CurrentTarget` directly to decide state or pick a hold target.

With no fallback registered both classes are behaviorally identical to before, which is why the prior scenes are safe by construction rather than by testing.

**Two properties fell out for free, and both are load-bearing.** `IsInRange(fallback)` being unconditionally true means an ambient hold cannot be cancelled by the world changing around it — which *is* the no-hand-off rule, without a line of code spent on it. And because `AttendableOverlayUI` reads `CurrentTarget` when idle and `AttendingTarget` while attending, keeping the fallback out of `CurrentTarget` means no panel is glued over the player while walking, but the ambient hold gets its own panel and progress bar. Folding the fallback into `CurrentTarget` would have broken both and forced an overlay change.

**Rate without touching the daylight model.** The ambient tick is 3s and costs the same one daylight as any other; a focused P4 attend is 2s. That is the whole of "slower rate" — no fractional daylight, no second currency. Undirected attention buys less per second because it isn't pointed at anything. `maxDaylight` 12 means a day is 36s of unbroken ambient attending, which should run out while the player is still deciding where to stand. Expect to retune at 5.5/5.6 when there is finally a competing use.

**Rock, drain, and phase messaging are all inherited, not reimplemented.** `PlayerController.HandleAttentionRock` keys purely off `AttentionManager.State`, and daylight/phase advance from the manager's own `SpendDaylight()`. Routing the ambient attend through the ordinary tick loop rather than around it is what makes all three come free.

**Verification (MCP, Play Mode):** cold load registered the fallback (`currentTarget=none, fallback=Here`, state `InRange`, so E is live with nothing in range and the overlay stays quiet). One hold resolved to the fallback (`state=Attending, attending=Here, holdProgress=0.71`), ran repeating 3s ticks that rotated the attending line with no immediate repeats, drained daylight 12 → 11 → 10, and advanced the day clock Dawn → Morning with its ambient text firing. Release ended the hold. 0 errors; the only two warnings are pre-existing `Rigidbody2D.isKinematic` deprecations in the third-party `ForestPixelLand` asset, untouched by this work.

**One content bug caught by the run and fixed:** an authored line read "The afternoon is doing what afternoons do" and fired at Dawn. The lines are drawn at random against a day clock running underneath them, so any line naming a time of day will eventually contradict the HUD. All lines are now phase-agnostic, and the constraint is commented at the field so 5.3+ doesn't reintroduce it.

**One known gap, deliberately left for the playtest to judge.** At zero daylight, holding E does nothing and says nothing: `AttentionManager` correctly refuses the hold, but the "Too late to start that now" line lives in `AttendableOverlayUI` and only shows when there is a `CurrentTarget` — which, for an ambient attend, there never is. The day-phase HUD reading Dusk is the only signal. Fixing it means teaching the overlay about the fallback, which is a wider surface than 5.2 needs; if the silence reads as a bug in play, that is the fix.

**Regression gates, all clean:** `Prototype4` (direct targeting re-verified live — `CurrentTarget` resolves to "A netmender", all six entities at their correct seeded state and tint, no fallback interference), `Prototype3`, and `Greybox`, each re-loaded and re-run in Play Mode after the framework change. 0 errors each.

**Debug drivers** (`Mossmark/Prototype5/*`): Begin/Release Attend, Log Ambient State (tick duration, current line, whether the fallback registered), Log Attention State (now reports `CurrentTarget`, `FallbackTarget`, and `AttendingTarget` separately — the three-way split is what makes fallback bugs visible), Teleport To Bedroll, Force Rest, Log Daylight.

---

## Build Notes — Iteration 5.2.1 (7-29-26)

**`IAttendable.ShowOverlay`, a default interface member.** `bool ShowOverlay => true;` — `AttendableOverlayUI` hides both panels when it's false, and `AmbientSurroundingsAttendable` returns a serialized `showOverlay` bool defaulting to off.

The default implementation is the whole reason this is cheap: there are **eighteen** `IAttendable` implementers across Greybox, P3, P4, and the shared World/Development namespaces. A required member would have meant eighteen edits to shared scripts to express one P5 decision. Defaulted, they all inherit "yes" untouched and the shared surface is two lines: the member itself, and one added clause in the overlay's existing null-target guard. It is also more honest than special-casing the fallback in the overlay would have been — "this attendable is a purely visual state" is a property of the attendable, not a fact about the ambient attend specifically.

**Reversal is an Inspector checkbox**, live, no recompile: tick `showOverlay` on the Player's `AmbientSurroundingsAttendable` and the name panel, progress bar, and detail panel all come back. The rotating `attendingLines` are deliberately kept rather than deleted for exactly this reason — flipping the switch restores the full 5.2 behavior rather than an empty panel.

**What still shows during an ambient attend:** the player rock, and the day-phase HUD with its ambient text. That is the entire feedback surface now. No notification lines are posted (none ever were).

*Narrowed at 5.4.1:* the ambient attend still draws nothing **of its own**, but a thing revealed during the hold now shows its name and description immediately. The claim as originally written — no UI whatsoever while ambient attending — held only while there was nothing in the world to reveal.

**Verification (MCP, Play Mode), both directions:**
- *During* an ambient hold — `state=Attending, attending=Here, holdProgress=0.72, namePanel=None, detailPanel=None`. Sampling during the hold rather than after it matters: after release the target is null and the panels would read hidden for the ordinary reason, which would have proved nothing.
- In `Prototype4`, standing at the Netmender — `currentTarget=Someone, fallback=none, namePanel=Flex, detailPanel=Flex`. `AcquaintableAttendable` does not implement `ShowOverlay` at all, so this also confirms the default interface member genuinely resolves to `true` at runtime under Unity's compiler rather than merely compiling.

Ambient ticks kept flooding the console window between the readout and the fetch; the readout was moved from `Log Ambient State` into `Log Attention State` (which is scene-agnostic) so it could be run in P4 too, which is what made the second direction testable at all.

**Regression gates, all clean:** `Prototype4` (overlay verified showing, as above), `Prototype3`, `Greybox` — 0 errors, 0 warnings each.

**Editor-loop note, for next time:** `Mossmark/Debug/Enter Play Mode` silently no-opped twice in a row immediately after a recompile — returning success without the usual socket drop, with `AttentionManager.Instance` still null. An explicit `Exit Play Mode` followed by `Enter Play Mode` cleared it. Worth trying that first rather than diagnosing, alongside the documented ~15–30s MCP drop around domain reloads.

**Forward constraint this sets for 5.3:** the progress bar was removed partly because it measures the daylight tick, and per-object dwell is a different clock. So if dwell needs a signal at all, it belongs *on the object* — a visual property of the thing coming into focus — not on the player, and not as a bar.

---

## Build Notes — Iteration 5.3 (7-29-26)

Two new components in `Mossmark.Prototype5`, five hand-placed things in `Prototype5.unity`, no shared script touched (5.2/5.2.1's amendments were already in).

**The gate is a separate component from the thing.** `NoticeableThing` holds the dwell logic and the collider; `UnknownThingAttendable` is what the thing *is*. `NoticeableThing` deliberately does **not** implement `IAttendable`, for two reasons: two `IAttendable`s on one GameObject is component-order-fragile (`AttendableZone.Awake` takes the first match — a trap P4's build notes already flag), and keeping them separate means **5.5 swaps the neighbour for P4's `AcquaintableAttendable` without touching the gate at all**. Nothing in `NoticeableThing` knows what it is gating.

**Why unnoticed means "collider off" and not "attendable refuses".** This is the one decision that would have quietly broken the whole prototype. An unnoticed thing that stayed in `zonesInRange` would become `CurrentTarget` — and since `ResolveTarget()` is `CurrentTarget ?? FallbackTarget`, standing near one would have *suppressed the ambient attend entirely*. The player would be unable to attend the place they were in precisely when there was something in it to find: exactly backwards. Disabling the zone collider keeps unnoticed things out of the detector's list completely. Verified directly — standing 0.6 units from an unnoticed thing reports `currentTarget=none, fallback=Here`.

**How a thing knows an ambient attend is running**, with no coupling: `manager.State == Attending && ReferenceEquals(manager.AttendingTarget, manager.FallbackTarget)`. That's it — "the hold currently running is the ambient one", expressed purely in the framework's own vocabulary. No reference to `AmbientSurroundingsAttendable`, no static, no registry. Each thing runs its own `Update`, so multiple things crossing independently needs no coordination and no code.

**The dwell tell is on the object, per 5.2.1's constraint.** A thing warms continuously out of grey toward its noticed color as dwell accrues (`Color.Lerp` on `dwell / dwellToNotice`), and cools again if the player drifts off — decay runs at 2× accrual, so a brief interruption is forgiving but wandering off genuinely resets. This is the whole of what ambient attending gives back *before* anything has crossed: something moving in the corner of your eye while you stand still. Opening numbers: radius 2.5, dwell 1.5s.

**No notification line at the moment of noticing** — the moment is meant to be seen, not read. The text arrives in the overlay when the player looks at the thing, which is what the mechanic spec asked for and what 5.2.1's visual-first steer wants. Crossing fires colour + `EntityFeedback.TriggerPop()` and nothing else.

**Verification (MCP, Play Mode) — every 5.3 claim checked directly:**
- Cold load: 5/5 grey (`0.45`), colliders off, dwell 0.
- Standing beside an unnoticed thing: `currentTarget=none` — no block, no overlay.
- Mid-hold, at the moment of crossing: `state=Attending, currentTarget=Something in the grass, attending=Here, namePanel=None, detailPanel=None`. The thing became targetable **while the ambient hold carried on unbroken** — the no-hand-off rule, observed rather than assumed. A bonus consequence worth noting: because the overlay follows `AttendingTarget` while attending, noticing something mid-hold doesn't pop a panel in the player's face either. The place stays quiet until they choose to stop.
- On release: `currentTarget=Something in the grass, namePanel=Flex, detailPanel=Flex` — the overlay arrives only when the player lets go.
- Two things 1.6 units apart both crossed in a single hold, while a third at 5.4 units stayed at `dwell=0.00`, grey, collider off. Independent accrual and the radius gate, both confirmed.

**Not directly observed:** the *intermediate* tint during dwell. MCP roundtrips are slower than the 1.5s threshold, so every sample landed at 0.00 or 1.50. Both endpoints are exact and the interior is a single `Color.Lerp` on values already proven correct (the observed grey confirms the white-baked sprite tints properly), so this is arithmetic rather than an untested path — but it is the main visual payoff of the iteration, so judge it in play first.

**Content:** five things, deliberately vague in the unknown register — *Something upright*, *Something low*, *Something half-buried*, *Something in the grass*, *Something at the treeline*. Two are placed 1.6 units apart specifically so one dwell can catch both; the rest are 5–7 units out from spawn so none is noticeable without walking somewhere and stopping. Attending a noticed thing costs a daylight and says one line about not being able to make more of it yet — placeholder, since what they *do* is 5.5's question.

**Regression gates, all clean:** `Prototype4` (overlay and direct targeting re-verified — `currentTarget=Someone`, `fallback=none`, both panels `Flex`), `Prototype3`, `Greybox` — 0 errors, 0 warnings each. Scene saved through Unity to confirm it accepts the hand-written YAML round-trip.

**Debug drivers added:** Teleport Player To Nearest Unnoticed Thing, Force Notice Nearest Thing, Log Noticeable Things (per-thing dwell, radius, collider state, tint, plus a noticed/total tally). The tally is editor-only for the same reason 5.1's coverage readout was — the ban is on showing the player a count, not on being able to verify the mechanism.

---

## Build Notes — Iteration 5.4 (7-29-26)

One new serialized bool and one new outcome branch on `NoticeableThing`, four hand-placed decoys, no new component and no shared script touched.

**A decoy resolves; what it resolves into is nothing.** `holdsSomething` is the authored truth and the *only* thing that differs between a thing and a decoy — same grey, same radius, same threshold, same continuous warm-up. `Resolve()` (renamed from `Notice()`, since crossing no longer implies finding) arrives at the warm colour for both outcomes and then branches: something enables the zone collider and pops, nothing starts a short fade *down* to a duller, cooler `settledColor` and never becomes targetable. See the iteration section above for why the two alternatives — a decoy that refuses to warm, and one that stalls just short — were rejected: both make finding out cost nothing, and the second is visibly the game withholding.

**The settled colour has to be legible after the fact and invisible before it.** If a resolved stump were indistinguishable from an unresolved one the player would re-dwell on it forever and the space would become noise; the settled tint is darker and cooler than unnoticed grey so it reads as checked in peripheral vision. That legibility is purely retrospective, so it costs nothing beforehand — which is the line 5.4's "no tell, no hint" scope constraint actually draws.

**`IsNoticed` became `IsResolved`, plus `HoldsSomething`.** Crossing and being findable are now different facts and one property could no longer carry both. Only `Prototype5Debug` read the old name.

**A serialized bool has no useful C# default here.** A scene entry written without the key deserializes to `false` regardless of the initializer, so `holdsSomething: 1` was written explicitly onto all five of 5.3's things rather than relying on absence to mean "something". Same for `settledColor`/`settleFadeDuration`, which would otherwise have loaded as transparent black and 0. This is the ordinary hand-authored-YAML tax, and worth flagging for the same reason Iteration 53's `[SerializeReference]` defaults gotcha was: the C# initializer is not the scene's default.

**Verification (MCP, Play Mode):**
- Cold load, all nine: identical grey (`0.450`), dwell `0.00/1.50`, radius 2.5, colliders off. The only differing field is the one with no observable effect yet — "indistinguishable beforehand", at the state level.
- A hold at a decoy: `resolved=True, colliderEnabled=False, tint=RGBA(0.300, 0.320, 0.280)` — exactly `settledColor`. Log timing brackets the fade: resolved at `:10.407`, `settling=False` by `:11.041`, i.e. 0.63s for a 0.6s fade.
- Standing 0.6 units from a *resolved* decoy, mid-hold: `currentTarget=none, fallback=Here, namePanel=None`. It is not attendable before resolution and not after.
- **The mixed hold** — one hold at (6.2, 4.4), a decoy 0.6 units away and a real thing 1.14 away — split cleanly: *Nothing (a dark shape)* to collider-off + settled `0.300/0.320/0.280`, *Something half-buried* to collider-**on** + noticed `0.720/0.660/0.450`. Same hold, same threshold, two outcomes.
- Mid-hold during that crossing: `currentTarget=Something half-buried, attending=Here, namePanel=None` — 5.3's no-hand-off rule re-confirmed in the mixed case, and the decoy correctly absent from targeting.
- On release: `currentTarget=Something half-buried, namePanel=Flex`. The settled decoy is **closer** to the player (0.6 vs 1.14) and still not the target — the cleanest proof it is out of the detector entirely rather than merely losing a proximity tiebreak.
- 0 errors. Compile clean apart from the two pre-existing third-party `Rigidbody2D.isKinematic` warnings.

**Not directly observed:** the settle fade mid-flight, for the same MCP-roundtrip reason 5.3's intermediate dwell tint went unobserved. Both endpoints are exact and the timing above brackets the interpolation to within a frame or two of its authored duration, but nobody has yet *seen* it fade. It carries the entire felt content of the iteration, so look at it first in play.

**Content:** four decoys, named in the hierarchy as *Nothing (…)* since a `NoticeableThing` name is never player-facing and honest editor names make verification legible — *a fold in the ground* (−4.2, 1), *a dark shape* (5.6, 4.4), *a lump in the moss* (0.5, 3.2), *something pale* (−6.5, −2.5). Ratio 4 nothing : 5 something. *A dark shape* is placed 1.42 units from *Something half-buried* specifically to make the mixed hold reachable. Decoys carry four components only (Transform, `SpriteRenderer`, `TriangleSpriteGenerator`, `NoticeableThing`) — no collider, no `AttendableZone`, no attendable, no `EntityFeedback`, since a decoy needs none of them and dead unreachable data in a pilot scene is how a prototype starts lying about what it tests. Tuning the ratio down is a GameObject delete; tuning it up is one bool flip.

**Debug drivers:** Teleport Player To Nearest Something / Nearest Nothing (added — the two outcomes have to be reachable *separately* to be verified separately, and from the editor there is no other way to choose which one a hold lands on, since the whole point is that they are indistinguishable in the scene), Force Notice → Force Resolve Nearest Thing (renamed), Log Noticeable Things (now reports `holdsSomething`, `settling`, and a something/nothing split in the tally).

*Regression gating with prior scenes was skipped at the user's request — they are handling it.*

---

## Build Notes — Iteration 5.4.1 (7-29-26)

One shared script changed (`AttendableOverlayUI`), no new component, no interface change, no scene change.

**No interface member was needed, and that is the design point.** The obvious move is another opt-out beside `ShowOverlay` — but "you cannot interact with this right now" is not a property of the thing. The revealed thing is perfectly attendable; the player is simply busy. It is a property of the *situation*, so it belongs in the overlay's own state logic and nowhere else. `IAttendable` is untouched.

**The mechanism** is a third target case in `Update()`. The overlay already picked `AttendingTarget` while attending and `CurrentTarget` otherwise; now, when the attending target draws nothing (`!ShowOverlay`) *and* something in range does, it draws that instead, with `passiveReveal` set. `passiveReveal` suppresses exactly two things — the interaction line (hidden outright, not blanked, so the panel shrinks to just the name) and the progress bar branch. The name panel still positions over the revealed thing rather than the player, since positioning follows the target.

**Regression argument, by construction rather than by testing.** The new branch cannot be entered without an attending target whose `ShowOverlay` is false. `ShowOverlay` is a default interface member returning true (5.2.1) and the sole override in the project is `AmbientSurroundingsAttendable`, which is P5-only and needs registering as a detector fallback to ever be attended at all. In Greybox/P3/P4 the condition is unreachable, so `passiveReveal` is permanently false and the remaining new statement — `interactionLabel.style.display = Flex` — writes the value it already had. This is the same shape of argument as 5.2's additive fallback: those scenes are safe because the code cannot run, not because it was tried and seemed fine.

That said, the *shared* half of the path was exercised directly: with `passiveReveal` false, a direct attend still reports `attending=Something in the grass, interactLine=Flex/"[..........]"` — the progress bar behaving exactly as before, on the identical code path every Greybox/P3/P4 attend takes. Scene-level gating is the user's, per their call on 5.4.

**Verification (MCP, Play Mode):**
- Ambient hold with only a *decoy* in dwell range: `namePanel=None, detailPanel=None`. 5.2.1's silence is intact where nothing is revealed — which is the case 5.4 made common.
- Ambient hold at the moment of crossing: `currentTarget=Something in the grass, attending=Here, namePanel=Flex, name="Something in the grass", interactLine=None, detailPanel=Flex`. Name and description mid-hold, no interaction line, ambient hold unbroken.
- On release: `interactLine=Flex/"Hold E to look closer"`.
- Direct attend, unchanged: progress bar as above.
- 0 errors, 0 warnings.

**Readout caveat for future verification:** `Log Attention State` now prints label *text* unconditionally, including when the label is hidden. Stale text behind `display=None` is normal and means nothing — read the display value first. The 5.4.1 samples above show `interactLine=None/"Hold E to rest"` in exactly that situation.

**Debug driver:** `DescribeOverlayPanels()` extended with the name label's text and the interaction line's display + text. The old readout could only say whether the panel was visible, and 5.4.1's whole claim is that the panel shows *part* of itself — pass and fail were indistinguishable before.

---

## Build Notes — Iteration 5.5 (7-29-26)

**Zero new code, and that was the test.** The whole iteration is one component swap in `Prototype5.unity`: *Something upright* at (−6, 3) lost its `UnknownThingAttendable` and gained P4's `AcquaintableAttendable`, unmodified, plus authored content. No new component, no new condition, no shared script touched, no P4 script touched. `NoticeableThing` beside it is byte-identical to what 5.3 shipped — which is precisely the claim 5.3's build notes made when they refused to fold the gate and the attendable into one component ("5.5 swaps the neighbour without touching the gate"). That prediction is now paid off rather than merely asserted.

**The one real design problem was colour, not code.** P5 and P4 both use grey for "you don't know this," but they mean different things by it, and stacking them naively inverts the grammar. 5.3 gives an unnoticed thing `unnoticedColor` 0.45 grey — deliberately P4's own unfamiliar tint, one layer earlier — and warms it to `noticedColor` 0.72/0.66/0.45 on crossing. P4's Netmender then goes *duller* from there (Acquainted is 0.52/0.55/0.45), so noticing someone would have made them warmer than getting acquainted with them does.

The fix is authored data, not a mechanism: the Fowler's `unfamiliarTint` **is** her `noticedColor`, and her ladder steps up from there (Acquainted 0.62/0.68/0.44, Known 0.46/0.72/0.42). That makes one continuous gradient across two systems — 0.45 grey unregistered → warm sand registered-but-unknown → deepening green with acquaintance — instead of two vocabularies colliding at the seam. It also means noticing her looks *identical* to noticing any other thing, which is correct: at the moment of the crossing you have registered that something is there, not that it is a person. Differentiation arrives with acquaintance, which is the whole thesis.

There is a second-order benefit worth recording: because the two colours are equal, the seam has no snap in it. `NoticeableThing` stops touching the renderer the instant it resolves and `AcquaintableAttendable.RefreshTint()` only ever runs on `Start` and on a crossing, so nothing ever repaints between notice and the first stage — verified directly below rather than argued.

**Ordering, checked rather than assumed.** `AcquaintableAttendable.Start()` calls `RefreshTint()` (unfamiliar sand) while `NoticeableThing.Awake()` has already set grey; Unity runs every `Awake`, then every `Start`, then every `Update`, and `NoticeableThing.Update` repaints grey at dwell 0 before the first frame renders. Cold-load tint reads 0.450 grey, so there is no one-frame flash. Component order on the GameObject is `AttendableZone` → `AcquaintableAttendable` → `NoticeableThing` → `EntityFeedback`, which keeps two prior rules intact for free: `AttendableZone.Awake` finds exactly one `IAttendable` (the swap replaced rather than added — two on one object is the component-order trap 5.3 avoided), and the attendable's `OnDeveloped` handler subscribes before `EntityFeedback`'s, so the stage-cross shape swap picks up the post-stage tint.

**No taught stage was authored, deliberately.** The doc's baseline calls for the teach stub "present as a stub only; not extended, not exercised beyond confirming it still composes." P5 has no pickups, no working surface, and no property-discovery path, so `WorldContext.IsPropertyKnown("p3_player", …)` can never go true and a taught-gated stage would be permanently unreachable content sitting in the scene. That is exactly the failure 5.4's notes named — "dead unreachable data in a pilot scene is how a prototype starts lying about what it tests." The machinery is present and inert on the component; `taughtPropertyId` is empty on both stages. Composition is demonstrated by the stub not interfering, which is the only thing 5.5 is entitled to claim about it.

For the same reason the ladder is the plain 4.2/4.3 shape: two stages (Acquainted, Known), `minAttends` 1, `ripenChance` 0.34 then 0.5 — the Netmender's own numbers. No wariness (4.8), no `worldStateFlag`, no state gate (4.20). `seededPropertyIds` carries `draws_the_eye`, so full acquaintance appends *"She speaks of what draws the eye"* — 4.2's seeded-reveal payoff, kept because it is baseline and because it is the one thing that makes the last crossing land as more than a paragraph change.

**Content.** The Fowler: someone who works by standing still and watching a ride for birds, dawn to mid-morning. The resonance is deliberate but unstated — the player finds her by doing what she does, and the game never says so. Her true state (nets rotting at the edges, told no one) is fixed at load and only revealed, per 4.2. Pronouns shift from *they* while she is a silhouette to *she* from Acquainted on; that is authored on purpose, since which person she is turns out to be part of what acquaintance reveals, and it is the one language choice here that isn't lifted straight from P4.

The GameObject is renamed *The Fowler* in the hierarchy, following 5.4's honest-editor-names rule — the name is never player-facing, and `NoticeableThing` logging "noticed 'The Fowler'" is what makes the crossing legible in verification. The other four somethings keep `UnknownThingAttendable`, so that component is still live rather than orphaned.

**Daylight left at 12.** The doc predicted a retune here, on the reasoning that a squeeze only reads as a choice once there's a competing use. There now is one, and the live numbers say 12 is already the right squeeze rather than a leftover: finding her took ~3 ambient daylight, the Acquainted crossing 1 more, and Known landed at 5/12 with the phase clock in Evening. One day buys a find and most of a ladder — tight enough to feel like a spend, not so tight it forecloses. Revisit at 5.6, when a second found thing actually competes for the same pool.

**Verification (MCP, Play Mode) — the seam, in both directions:**
- Cold load: `acquaintanceStage=-1`, `resolved=False`, `colliderEnabled=False`, `tint=0.450 grey`, `knows=[draws_the_eye]` (Awake's seeding ran). Overlay reads the unfamiliar description with **no** seeded line — `IsFullyAcquainted` is correctly false while an ungated stage is next, so the reveal doesn't leak.
- One ambient hold beside her resolved *her and a decoy in the same hold* (*Nothing (a fold in the ground)*, 2.33 units away — unplanned, and the exact mixed case 5.4 was built for). Mid-hold: `state=Attending, currentTarget=Someone, attending=Here, namePanel=Flex, name="Someone", interactLine=None`. The ambient hold never handed off (5.3), and 5.4.1 named her mid-hold as **"Someone"** — the veil holds through the reveal, which is the interesting confirmation: 5.4.1 shows the *thing's* name, and for a P4 entity that name is still the silhouette's.
- On release: `interactLine=Flex/"Hold E to watch a while"` — P4's own unfamiliar interaction line, arriving on release exactly as 5.4.1 specifies.
- **The no-snap seam, observed directly** (a second Play Mode run, `Force Resolve` with no attends spent): `acquaintanceStage=-1, resolved=True, colliderEnabled=True, tint=RGBA(0.720, 0.660, 0.450)`. Noticed colour and unfamiliar tint are the same pixel, and she is targetable at the unfamiliar read. This is the one state the first run skipped past, and it is the whole join between the two systems.
- Direct attends: Acquainted crossed on the first tick (`tint→0.620/0.680/0.440`, shortName "A fowler", stage overlay + stage interaction line). A second hold ran a deepening tick and then crossed to Known without releasing — `ContinueAttending` inherited from P4 unchanged. At Known: `tint=0.460/0.720/0.420`, and the overlay appends *"She speaks of what draws the eye."*
- Zero-effect held throughout: `subject=(trueState="…" knows=[draws_the_eye])` identical at every read, and `VerifySubjectUnchanged()` logged no error on any tick.
- 0 errors across both runs. Scene saved through Unity to confirm it accepts the hand-written YAML round-trip.

**Debug drivers added:** Teleport Player To Nearest Acquaintable, Advance Acquaintance On Nearest Entity, Log Acquaintance State — the first two mirror `Prototype4Debug`'s equivalents rather than inventing a P5 vocabulary, since the point of the iteration is that nothing about the entity is P5-specific. `Log Acquaintance State` prints the `NoticeableThing` gate on the same line as the entity state on purpose: 5.5's entire surface is the seam between the two, and a stage that won't advance because the thing was never resolved is otherwise indistinguishable from a stage that won't advance for its own reasons.

*Regression gating with prior scenes was skipped at the user's request — they are handling it. The exposure is smaller than 5.4.1's in any case: no shared script was touched, and `AcquaintableAttendable` is used here exactly as `Prototype4.unity` uses it.*

**Open for the playtest, and it is the actual go/no-go:** does finding her by standing still change how the first watch feels, versus P4 where every entity was flagged from the path? The mechanism is confirmed; the bridge claim is not, and cannot be from an editor.

---

## Build Notes — Iteration 5.6 (7-29-26, playtest 7-30-26)

**Zero new code, same move as 5.5.** *Something in the grass* at (−3, −4), renamed **The Hollow** in the hierarchy, lost `UnknownThingAttendable` and gained P4's `TendableSpotAttendable` (4.12/4.14's shape) unmodified. `NoticeableThing` beside it is untouched, same as the Fowler's swap. Content: two unrevealed pool items (A Handful Of Sloes, A Twist Of Bark), `yieldChance` 0.3 flat, no `propertyIds` — same "no property-discovery path in this scene" reasoning as 5.5's missing taught stage.

**"No ripeness" and "no character" both had to be authored, not just left out.** `TendableSpotAttendable` carries the ripeness (4.13) and character (4.15) machinery unconditionally — there's no flag to switch it off — so the doc's "flat yield chance, no ripeness" instruction meant zeroing it in data: `ripenBonusPerDayAway`/`maxDaysAwayBonus`/`depletionPerAttendToday` all 0, so the effective chance is permanently the authored 0.3 regardless of days away or attends already spent. `characterNudgeUp`/`characterNudgeDown` are also 0, so `character` never moves off its neutral 0.5 starting value.

**The colour-continuity fix from 5.5, but total.** Because character can't move, `RefreshTint()`'s `Color.Lerp(wornTint, thrivingTint, 0.5)` is the spot's tint forever — so both fields are authored to the same value, `noticedColor` (0.72/0.66/0.45), rather than two arbitrary endpoints whose lerp happens to land somewhere. Unlike the Fowler, there's no later stage to grow into here; a flat mechanic gets a flat, unchanging tint, which is the honest visual expression of "no ripeness, no character" rather than a side effect of not bothering to differentiate it.

**`wornFlavorLines`/`thrivingFlavorLines` were deliberately left empty**, the 5.6 analog of 5.5's missing taught stage. `CurrentFlavorPool()` bands on `character`, and with `character` pinned at exactly 0.5 it can never fall outside `[0.3, 0.7]` — the worn and thriving bands are structurally unreachable, so authoring lines into them would be exactly the "dead unreachable data" 5.4's notes warned against. Only `midFlavorLines` (two lines) is live.

**Not independently re-verified by me via MCP.** The Unity MCP bridge dropped after the recompile that picked up this iteration's script/scene changes and stayed unresponsive for several minutes — port 8090 kept accepting TCP connections but nothing behind it answered, which matches the documented locked-session failure mode rather than the ordinary ~15–30s domain-reload drop. Sean played the build directly instead and confirmed the mechanism (notice → resolve → tend), the regression gate against Greybox/Prototype3/Prototype4, and 0 console errors from his own session. Unlike 5.4/5.5, this iteration's build notes are reporting a live playtest first-hand rather than an MCP-driven one.

**Live tuning change, made during the same playtest:** Player `moveSpeed` 4 → 2 (`PlayerController` on the Player GameObject). Sean's own call, not scripted here — noted because it changes the felt pace of the whole scene and belongs in the historical record alongside the mechanism notes.

**Playtest findings — the actual payoff of this iteration, and it's a genuine open question, not a pass.** Sean's read: the prototype is moving in the right direction overall, and slowing traversal down helped make attending feel more deliberate and less frantic. But the reveal mechanic is still hard to *feel* — there's no delight of surprise landing yet. He can't yet tell which of three things is responsible:

1. **The content is static.** Five hand-placed things (now including The Hollow and the Fowler) in a small hand-authored scene means that once you've played it once, you know what's where — there's nothing left to discover on a second pass, which may simply be inherent to a pilot at this scale rather than a mechanism failure.
2. **"Some of them are nothing" (5.4) isn't landing as designed.** The whole point of the decoy ratio was to make *not knowing* a real expenditure, worth something in itself. It's not obvious from play that this reads as intended.
3. **The flavor-only misses aren't landing either** — neither the tending spot's miss lines (this iteration) nor whatever ambient/notice-adjacent flavor already existed. A miss may be reading as "nothing happened" rather than as texture.

No conclusion reached, and none forced. Sean wants to sit with this before building further — 5.7 is paused for the same reason (see that section above): its current phrasing doesn't sit right against what this playtest surfaced, and it needs a rewrite, not an implementation, next.

**Debug drivers added:** Teleport Player To Nearest Tendable Spot, Log Tendable Spot State (mirrors `DebugRipenessState()`/`DebugCharacterState()` plus the `NoticeableThing` gate on one line, same reasoning as 5.5's Log Acquaintance State — a spot that can't yield because it was never resolved should never look like one that's just rolling badly).

---

## Appendix: the scrapped approach (Iteration 5.1)

**What it was.** A hand-placed clearing subdivided into a jittered 6 × 5 grid of small attend-zones over ordinary ground. Attending a patch clarified it permanently — a color lerp from muted to legible, an `EntityFeedback` pop, and one descriptive line that never paid out or hinted. Revealed patches stayed targetable but returned `CanAttend()` false, so looking somewhere new required walking. Built and verified live in `Prototype5.unity` on 7-27-26: 30 patches, one held attend revealed one patch for one daylight, a second attend from the same spot correctly did nothing, collider coverage confirmed gapless, all three regression gates clean.

**Why it went.** Seeing it working made the problem obvious in a way the plan hadn't: it put the reveal on the *ground*, and the ground is not where the uncertainty is. Clarifying a patch of soil answers a question nobody was asking. The interesting uncertainty is "is that thing over there anything, or is it just a shape" — which requires the things to exist, visible and mute, before you attend. The 5.1 loop also quietly made the player the active party sweeping a surface; the redirected mechanic makes them the still party in a place that resolves around them, which is much closer to what this project's design values actually describe.

**What it taught, and what carries forward:**
- The locus of a reveal mechanic matters more than its texture. 5.1's texture was fine — the fade, the pop, the descriptive lines all worked. It was pointed at the wrong object.
- **Scene scaffold, kept:** `Prototype5.unity` stays. Its P4-derived scaffold (camera + `CameraFollow`, Global Light 2D, Player, `AttentionManager`, Overlay UI, Ground, Notification UI, Day Cycle Manager, Day Cycle UI, Day Transition Fade UI, Bedroll at (0, −9)) is exactly what 5.2 needs. The Clearing object comes out at 5.2.
- **Debug drivers, kept and extended:** `Assets/Editor/Prototype5Debug.cs` (`Mossmark/Prototype5/*`) — Begin/Release Attend via reflection into `AttentionManager`, teleports, Force Rest, Log Daylight, Log Attention State. The patch-specific entries go with the patches.
- **Daylight tuning, revisited:** 5.1 ran `maxDaylight` 36 against 30 patches so the clearing was clearable in a sitting. The redirected thread wants a much tighter pool (opening guess: ~12) because ambient attending is continuous and undirected — the day should run out while you are still deciding where to stand. The 5.1 reasoning still holds in general, though: a squeeze only reads as a choice when there is a competing use, so expect to retune again at 5.5/5.6 when there finally is one.
- **Scripts, removed:** `RevealField` and `RevealPatchAttendable` are deleted rather than left dormant at 5.2 — they have no role in the redirected mechanic and dead code in a pilot scene is how a prototype starts lying about what it tests. Git history keeps them; the 7-27-26 commit is the reference if the patch-grid idea ever wants revisiting for something else.
