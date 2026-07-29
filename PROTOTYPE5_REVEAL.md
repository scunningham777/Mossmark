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

Ambient attending shows no UI whatsoever: no name panel above the player, no progress bar, no detail panel. The player rocks, the day clock advances, and that is the entire feedback surface. Reversible from the Inspector without a recompile.

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

---

### Iteration 5.4 — Some Of Them Are Nothing

Add grey shapes that never resolve, mixed in among the ones that do, indistinguishable beforehand.

**Explicitly out of scope:** any tell, any hint, any way to know in advance — that is the whole point.

**Success criterion:** does not knowing whether a place holds anything make attending it feel like a real expenditure, or does it just feel like being wasted? If the answer is "wasted," the fix is the ratio and the size of what's found, not the removal of decoys — a world where everything grey pays out isn't a world you can attend to, it's a checklist.

---

### Iteration 5.5 — A Noticed Thing Is A P4 Entity

One of the objects noticed in 5.3 is an NPC running P4's acquaintance `DevelopmentTrack` verbatim. Once noticed it behaves exactly as a P4 entity does, unmodified.

**Explicitly out of scope:** any interaction between how it was found and how it develops; buildings or sites; ambient-rate variation (5.7).

**Success criterion:** does having found someone this way — stumbled into by standing still and paying attention, rather than walked toward because they were flagged — change how the first acquaintance attend feels? This is the actual bridge claim: ambient attention as backdrop, acquaintance as the thing it is a backdrop *for*.

---

### Iteration 5.6 — A Second Kind Of Found Thing

Add one `TendableSpotAttendable` (P4.12/4.14 shape, flat yield chance, no ripeness) elsewhere in the same space, noticed the same way.

**Explicitly out of scope:** ripeness, character, any differentiation between how the NPC and the site are noticed unless a real reason emerges in play.

**Success criterion:** do two different kinds of found thing (a person to get to know, a place to tend) inside one attended space read as one coherent place, or as two unrelated mechanics dropped into the same clearing?

---

### Iteration 5.7 — Familiarity Widens What You Notice (Rate, Not Gate)

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

**Verification (MCP, Play Mode), both directions:**
- *During* an ambient hold — `state=Attending, attending=Here, holdProgress=0.72, namePanel=None, detailPanel=None`. Sampling during the hold rather than after it matters: after release the target is null and the panels would read hidden for the ordinary reason, which would have proved nothing.
- In `Prototype4`, standing at the Netmender — `currentTarget=Someone, fallback=none, namePanel=Flex, detailPanel=Flex`. `AcquaintableAttendable` does not implement `ShowOverlay` at all, so this also confirms the default interface member genuinely resolves to `true` at runtime under Unity's compiler rather than merely compiling.

Ambient ticks kept flooding the console window between the readout and the fetch; the readout was moved from `Log Ambient State` into `Log Attention State` (which is scene-agnostic) so it could be run in P4 too, which is what made the second direction testable at all.

**Regression gates, all clean:** `Prototype4` (overlay verified showing, as above), `Prototype3`, `Greybox` — 0 errors, 0 warnings each.

**Editor-loop note, for next time:** `Mossmark/Debug/Enter Play Mode` silently no-opped twice in a row immediately after a recompile — returning success without the usual socket drop, with `AttentionManager.Instance` still null. An explicit `Exit Play Mode` followed by `Enter Play Mode` cleared it. Worth trying that first rather than diagnosing, alongside the documented ~15–30s MCP drop around domain reloads.

**Forward constraint this sets for 5.3:** the progress bar was removed partly because it measures the daylight tick, and per-object dwell is a different clock. So if dwell needs a signal at all, it belongs *on the object* — a visual property of the thing coming into focus — not on the player, and not as a bar.

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
