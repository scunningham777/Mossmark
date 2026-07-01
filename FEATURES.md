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

## Iteration Status

Numbering continues from PROTOTYPE2.md's final iteration (30) to avoid renumbering confusion across documents — this table is the active one going forward. PROTOTYPE2.md's own table stops at 30 and is not updated further.

| # | Status | Iteration | Summary | Key Types |
|---|---|---|---|---|
| 31 | [x] | Passive Drift on a Single Post-Spec Stage (Pilot) | Bog Keeper's `bog_keeper_drainage` stage gains rest-driven passive progress (0/1/2, banded off the Fen Bog spot's `tendedness`), additive with existing attention-driven progress and gated the same as any other stage (item still required to apply) — pending playtest verdict | `NpcAttendable.OnDayAdvanced()`, `WorldGenerator.GetArchetypeSpot()`, `WildernessYieldAttendable.Tendedness` |
| 32 | [x] | State-Change Feedback Pass (Greybox) | Wire visible/audible feedback to three signal tiers (progress tick → scale pulse, stage-cross → shape swap triangle↔circle + bigger pop, passive drift → white halo child until next productive attend); player sprite rocks ±5° Z while attending | `EntityFeedback`, `CircleSpriteGenerator`, `DevelopableEntity.OnProgressMade`, `WildernessYieldAttendable.OnProgressMade`, `TendedSpotAttendable.OnProgressMade`, `NpcAttendable.OnPassiveDriftAccrued`, `PlayerController.HandleAttentionRock` |
| 33 | [x] | Detail Overlay (Bottom-Right) | Second UI panel anchored to the bottom-right corner showing the hovered entity's name and a bullet list of all development stages permanently applied to it — gives the player a persistent at-a-glance record of what has been achieved without attending | `IAttendable.GetAppliedUpgrades()`, `DevelopableEntity.GetAppliedUpgradeNames()`, `AttendableOverlayUI` detail panel |

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
