### MVP
* Overview: can collect things from multiple sources (foraging, monster drops, crafting, quest rewards), and randomized collections of things are used to complete quests. Quests upgrade npcs, buildings, or towns/tech. Tech is per town, so it needs to be "manually" transferred at first, but later upgrades allow more efficient transfer. 
* Randomized world with multiple biomes. 
* Randomized towns with randomized npcs, buildings, and starting town-level tech
* Interacting with these objects can provide "quests", which are essentially just "complete this random collection of items and bring it to me". Quests can upgrade the npc, building, or town that assigned them, and also give rewards in the form of currency(?) and other items.
* Entities (npcs, buildings, towns) will have semi-random, semi-unique upgrade trees, with the pool of options being determined by the biome they are found in, and influenced by neighboring towns.
* Randomized "dungeons" that often need tech (town upgrades, or specific items that can only be crafted by upgraded npcs or at upgraded buildings) to unlock
* Randomized zelda II-style Wilderness (walking off a path gives a chance of random encounters, which will bring up a small combat "level") 

### Brainstorm
* An rpg where each city you come across acts a little like a city in civ
* there are different resources available, and you can do quests and work for NPCs to develop the cities in various ways, essentially developing their technology and specific buildings or trades. As the cities grow it will unlock new items or quests or bonuses (better ingredients or weapon buffs, eg).
* Could possibly link this to previous "soldier returning from war" ideas where you are are prisoner released after your side lost a war.
  * You basically are working to ingratiate yourself with the locals and build a new life. 
  * You can also move closer to your home country, which has been devastated and as you travel even further in, there is a sort of infestation of evil in some supernatural form resulting from the atrocities of war. 
  * Maybe even include options to join a rebellion group or just assimilate and forge a new life, but grapple with the complexities of either choice. Not trying to make specific comments on war or whatever, need to focus on the complexities of large-scale human interactions.
* I think this might be a better fit for the "warring tribes in the shadow of a declining empire" idea I wrote somewhere. 
  * That might open the door for weird resource distribution, as well as varying levels of "tech".
  * Could probably still combine with the previous idea. 
  * Could include an idea of "warring tribes" or factions, but it should work in an organic way, so factions can arise, and grow or shrink in size and influence based on various factors, but you would be able to directly influence those factors. Could calculate those values (size, etc) mathematically and then derive actual numbers of towns and units, tech levels, etc. 
* Maybe tech can be localized, so each city or at least region has its own tech tree, and the hero can bring ideas from one to another, plus maybe have a "natural" flow of ideas as well, which can be boosted by the hero at some point. 
* The hero might also have to personally transmit materials at first, and then set up or somehow facilitate trade networks for new tech to succeed in a new place. 
* Procedurally generate maps AMAP
  * City/local maps could maybe be "1d" on top with a list of resources under it, like the kind of NPCs, shops, organizations, politics, etc. The 1D map could be just be saved as an ordered list of places to keep it simple. 
  * Then maybe have regional maps and a world map, each region is a hex on the world map. 
  * Not sure how to save info for regional maps - maybe save locations as nodes but re-generate the actual terrain whenever moving from one region to another, or save some sort of grid of areas, so the actual layout of each area is re-generated, but some data is persisted. 
  * shouldn't be so many hexes on world map to make the save data onerous. 
* Some idea of tech being more beneficial in different circumstances, and having to try things out (like permaculture). Not sure how to reflect this in-game in a way that would be fun, but even looking at hidden costs and long-term vs short-term benefits to one solution over another, maybe even having trade offs, etc. Or maybe this could play into the "research" mechanism, for how new tech is learned. Could even look at trial and error/info sharing/ancestral knowledge vs scientific method, and have tradeoffs for both. 
* Perhaps a good way to simplify "quests" or advancement for npcs, buildings, towns would be to use a "collection" mechanic similar to Cradle of Empires. So you would explore and do whatever to get random things, and when you go into the towns, there would be npcs and buildings, and some could be "upgraded" by giving them specific random collections of things. Some things would be crafted by certain types of npcs or buildings so you would be encouraged to explore new towns and also improve them. 
* Entities can offer multiple "quests", and whichever one the PC chooses to complete will guide the direction of their upgrade tree. 

### Concrete
So I want to try to walk through a more concrete example of how I envision things working, so we can start to extrapolate to randomizable systems from there. I'll try to just spill out everything I could possibly want at first, and then try to distill it down to a MVP.

So the player starts on the edge of a town. In the town there are some buildings: town hall, market, and 2 houses. The construction of the buildings is something similar to "rural" bronze age northern Europe. There are also some residents: a chieftain, a healer, a leather-worker, and a hunter. The player starts with next to nothing in their inventory. They offer to gather herbs for the healer, and head to the wilderness around the town to get them. At this point, encounters with wild animals will need to be avoided, or perhaps a battle-dulled sword that they start with can be used to fend off or defeat smaller animals. Once they have the herbs, they are exchanged for (local?) currency, and they are able to buy a knife or billhook from the market. Continuing with the healer, they could track down a collection of slightly more rare plants, and turning in this collection will increase the healer's knowledge, unlocking the healer's ability to heal more serious injuries. A further quest would allow help the healer learn to make potions or salves that the player could then take with them. A further quest would help the healer build some aparatus for brewing potions more efficiently. If the player then visits an adjacent town, they would be able to pass this knowledge on to the healer there without having to complete the quest again, because they can get the new healer to visit and learn from the one in the previous town. There needs to be a proximity limit for this until writing technology and materials are discovered, at which point the player can carry "instructions" for any tech upgrade with them to any city. Once printing technologies are discovered, tech advances discovered in any one town could be randomly learned by other towns in the region maybe, but that's getting way ahead of myself.
The player could also offer to work on a house, completing a quest to gather clay, rocks, and sticks. Completing this would add a 3rd house to the town, and another NPC would appear. No more houses could be built though until some food storage technology was discovered, which would require a quest through the town hall. Actually, building a new house would probably also just be a quest through the town hall. 
For the leather worker, I guess the progression would be similar to the healer: quests help unlock "tech" that increases the offerings of the npc. 

So without trying to scale this back too much, I'm seeing somewhat of a pattern of options based on the various technologies/knowledge unlocked at whichever level (town/building/npc), and also resources owned by the player or available in the town. I haven't quite gotten to these details in my walkthrough above, but I have been envisioning that some upgrades will require the player to have certain equipment - eg, you can't build certain buildings via the town hall until you have carpentry tools - but this really seems like it's reversing a lot of the other concepts of the game. Everywhere else you are just doing collection quests, and then it's the local entities (not the player) that advance and unlock further options. But in that case I think there would be some interplay between the upgrade options at the town hall and the npcs and buildings present in the town. So maybe you wouldn't have the option to build wooden barns until there is a carpenter in town, so you would have to follow an upgrade branch to get a farmer to specialize in carpentry, and then you would unlock the options in the town hall.

So I think our upgrade tree might need one more layer of sophistication, in that each node might need to have a list of requirements before it can be unlocked, rather than just a progression from one node to the next. And I think instead of an NPC's "progression" being viewed as "which step are they on currently", it's more of a list of all techs that have been unlocked for them so far.

So at a basic level (MVP), I think we could say that:
* each entity (NPC/building/town) always has a quest to offer. 
* Once the quest is completed, that entity earns 1 skill point, and then the player will be able to start a new quest for them. 
* Skill points are spent on upgrades
* the list of available upgrades is determined based on... I still need to flesh this out a little, because I want some upgrades to be specific to the "owned" upgrades of the current entity (eg, a smith with weapon-smithing expertise could unlock a folded steel forging tech for stronger weapons), but also maybe some of the upgrades on some of the other entities in the town (using my earlier example, the town hall won't make a "wooden barn" upgrade available until some NPC in town has "carpentry" unlocked). So just throwing this out there then: 
  * each upgrade has a list of dependencies, all of which must be present in order for this upgrade to be available
  * each dependency is EITHER another upgrade, OR a list of upgrades. If it's a list, there will also be a number ("n") associated with this dependency, and that will mean that at least "n" items from the list must be present for that dependency to be satisfied. (eg, something like having a list of "weapon smithing", "armor smithing", and "tool smithing" upgrades, and you only need 1 of them)
  * each dependency will also have an enum that marks whether the dependency must exist on the current entity, in the town, or maybe beyond? (this will probably be separate from the tech transfer concept I fumbled with earlier)

### On Organic Development and Player Direction

The player should feel like they are tending the growth of a settlement rather than executing a build order. This has a few specific implications:

* **Legibility of direction**: At any point, the player should be able to look at the current state of upgrades in a settlement and understand what directions are available to them — what dependencies are satisfied, what is close to being satisfied, and what the next few steps toward any given direction would be. The dependency system already creates this space; the UI needs to make it readable.

* **Differentiation over advancement**: Upgrades represent a settlement taking on a particular character, not moving up a ladder toward a single optimal end state. A settlement deep in shrine and hedge witch development is not behind one deep in smithing — it's different. The upgrade system should feel like this in play: branching paths that give settlements genuine personality, not tiers that sort them by progress.

* **Organic detail within directed development**: The player can choose a direction — "I want this settlement to become a center of ritual knowledge" — but the specifics of how that develops (which items are requested, what the merchant brings, which wandering NPC shows up) are shaped by randomness. The player tends the growth; the system fills in the texture. This becomes richer with more biomes and inter-town relationships in later phases, where the interaction between neighboring settlements with different development paths produces emergent configurations no one explicitly designed.

* **Tech as settlement identity, not score**: In later phases (tech transfer, multiple biomes), the fact that a settlement knows iron smelting should feel like something it *owns* — part of its identity and history — not a stat that gets applied wherever the player travels. Tech transfer should require effort and feel meaningful because you are genuinely carrying something from one place to another. The writing/runescript tech in the prototype is an early gesture toward this: it represents the settlement's ability to preserve and communicate knowledge, which is a precondition for more efficient transfer in later phases.

(Above generated from my prompt:
> one dynamic that I’ve felt important to chase is the idea that the player should have choices of which “direction” to help the world grow in, and as they work toward that direction I want it to be fairly clear where they need to go and what actions they need to take in order to effect that. Though I also want there to be a level of “organicness” to the development: you are planting the seeds and can tend to the growth to help sway the general direction, but the details are ultimately determined by the larger complexities of life. I also want the “development” to be a somewhat hierarchical organizing of information, but not a rigid idea of “we’re moving from dumb barbarians to gun-toting enlightenment sophisticates” or whatever, which I think a lot of early civ type games simplified down to.
)

### Place Archetypes and Coherent World Generation

**The problem.** World generation currently runs as two independent passes: wilderness features (zones, nodes, encounter locations) are drawn from one pool, and town entities (NPCs, buildings) are drawn from another. They share a Region SO as a container but don't constrain each other. This produces mismatches — a Bog Tender gating a fen encounter location in a town that never spawned a Bog Keeper; a quest asking for bog iron in a session where no bog zone was placed. These are tolerable in the single-region prototype but become a design problem at scale.

**The reframe.** A town and its surrounding wilderness grew up together. The people become who their place needs them to be. Generation should reflect this: the wilderness and the town should be drawn from the same act of world-building, not two parallel random draws.

**Place archetypes.** Rather than a flat NPC pool and a flat zone pool, the region defines a set of **place archetypes** — self-contained bundles that include a wilderness feature and the human specialization it implies:

- **Bog archetype**: Fen zone, bog iron and crow feather nodes, Fen Bog Hollow encounter location → implies Bog Keeper NPC track, `bog_tender` gates the encounter location
- **Old Road archetype**: Old Road zone, flint nodes, Old Road Checkpoint encounter → implies Herald/Chieftain's Herald track, `watchtower_built` gates the checkpoint
- **Sacred Grove archetype**: Deep Wood zone, mistletoe and ravens_eye nodes, a shrine encounter → implies Hedge Witch and Fen Shrine tracks

World generation selects 2–3 archetypes per region. Zones, encounter locations, and implied NPC/building specializations all derive from that selection. Encounter gates are always reachable because the NPC track that unlocks them is guaranteed to be present.

**NPC specialization.** Towns generate with a small number of generic settlers (Wanderer, Settler, Roughhand — the existing `displaced_pool`). Each archetype present in the region makes certain specialization paths available. The player triggers specialization through early interaction — likely the first quest from a generic NPC involves exploring a nearby zone, and completing it unlocks a specialization choice. Once specialized, the NPC slots into the existing upgrade tree for that archetype. This makes the "people become who their place needs them to be" dynamic legible in play: you are not assigned a Bog Keeper, you help someone become one.

**Quest pool coherence.** A Bog Keeper only appears if a bog archetype was selected, so bog iron quests only appear if bog iron nodes exist in the world. This resolves the conversion item mismatch problem systematically rather than through per-session workarounds.

**On "Biome" → "Region".** The `BiomeData` SO should be renamed `RegionData`. "Biome" implies ecological specificity that doesn't match the actual usage — the SO defines a named cultural-geographic area (IronwoodReach, etc.) that can contain multiple zone types. "Region" is neutral and accurate.

**Prototype note.** This is post-prototype scope. The current single-region, fixed-zone prototype is the degenerate case of this system — one implicit archetype selection, no specialization mechanic. Nothing needs to change now. The `displaced_pool` NPC archetypes (Wanderer → Settler → Kinsman) should eventually read less like a generic progression ladder and more like genuine pre-specialization starting states; this is a data/naming concern for when the specialization system is designed properly.

---

### Aesthetics/Theme
one aesthetic that is a core part of me wanting to make games is the idea that I can create a game experience that evokes feelings similar to listening to the Midlake albums “the Trials of Van occupanther” and “the Courage of Others”. I don’t have a clear idea of how to do that mechanically, but I feel like this theme is a good direction for that. I think probably the closest I’ve felt to that is the sense of melancholy that arises in a few places in hollow knight. Or in Claude's words: 
> The feeling those albums produce isn't nostalgia or pastoral prettiness; it's something more like the weight of being embedded in a place and a season. The world in those songs has been there longer than the people in it, and the people know that. That's the feeling you're reaching for.

---

## Prototype 2

The following reflects a separate, later round of thinking that led to starting a second prototype from scratch (see PROTOTYPE2.md for the resulting systems draft). Kept here as its own dated section rather than merged into the discussion above — synthesis can happen later if it makes sense.

### Why a second prototype

Prototype 1's quest/upgrade loop — walk up, open a menu, get a quest, fetch named items, turn in — worked mechanically but felt transactional. It didn't produce the sense of being embedded in a place that the Midlake reference is reaching for. The menu itself was identified as the core problem: the player was always selecting from the world's offerings rather than the world simply responding to presence.

### Attention as the universal verb

The central idea: replace all menu-driven interaction with a single embodied verb, **Attention** — hold a button on whatever you're near, and something happens based on what that thing *is*, not on a choice you make. A field yields forage. A clay pit yields raw material with a rare-item chance. A dilapidated building consumes materials and produces development progress. An NPC produces progress or reveals what it needs. A wandering creature produces a good or bad outcome. The response is a property of the target, never a player-selected option.

This also collapses the day-cycle bookkeeping from Prototype 1 (a 7-action pool tracked alongside play) into the play itself: attention **is** daylight **is** the day's clock. Spending attention spends the day; there's no separate layer to manage. This was identified as a meaningful simplification, not just a re-skin — the old day cycle was a second system sitting on top of the moment-to-moment loop, and P2 makes them the same thing.

### Collection quests were never the core idea

Revisiting the original brainstorm above, the "Cradle of Empires"-style collection mechanic was floated as a *simplification* for prototyping, not as a core pillar. The actual throughlines from the original brainstorm — Civ-like city development, localized tech that has to be carried or transmitted, "tried not chosen" research outcomes (the permaculture comparison), and entities offering paths whose direction the player influences but doesn't dictate — are still the things that matter. P2 is an attempt to get back to those throughlines without the collection-quest scaffolding that P1 built around them.

### Tension: tech-tree love vs. "progress" ideology

There's a real, acknowledged tension between enjoying tech-tree/development systems for their own sake (the Age of Empires "build a huge empire, ignore the military objective" impulse) and discomfort with framing development as "progress" in a way that reads as "rescuing" a post-imperial society or recapitulating a modernist civilizational ladder. The resolution explored here: if development outcomes are **discovered through sustained attention rather than selected from a menu** — "tried, not chosen" — then rich, varied development can exist without being a checklist to optimize. A settlement that's been attended toward "ritual knowledge" for a long time might develop something nobody picked, which is interesting precisely because it wasn't chosen. This reframes "rich development" as a source of texture and surprise rather than a ladder to climb.

It's also worth remembering, per this session: the point is for the game to be fun, not to make an argument about how people actually lived 1500 years ago, about whom almost no historical record survives anyway. The post-imperial framing is a mood and a setting, not a thesis to defend — and an impartial presentation of "differentiation vs. advancement" as two valid postures (rather than one being correct) was floated as a way to keep the theme present without moralizing.

### The "Flame Sword" feeling

A guiding value, named from an earlier session: right now a Raven's Eye is "a quest item you farm." In the Dragon Warrior idiom, an item like that should instead be something you finally reach after a long approach — finding it feels like *the world giving you something*, not a drop you farmed. The risk with an attention-based micro-loop (gather mud, work on a building, repeat) is that everything flattens into the same texture of small grindy moments, with nothing that provides this larger payoff feeling. P2 needs to preserve room for these larger moments even as the moment-to-moment loop becomes more granular and attention-based.

### NPC relationship stages, and what NPC development *is for*

Early framing of "attend to an NPC until they give you a quest" felt thin — same transactional shape as P1, just with a different input. Reframing NPCs around **relationship stages** helped: early on, an NPC might be wary, anonymous, even slightly hostile (an outsider arriving in a threatened/anemic settlement); over time, through attention, that relationship develops; eventually something opens up that wasn't available before — a place, an item, a piece of the NPC's own history — that feels *earned* rather than *purchased*.

This connects to a bigger-picture question raised this session: **what is town/region development actually *for***, if not "save the savages" or "maximize resources toward a Golden Clock" (the Stardew Valley end-state critique)? One answer explored: development — especially NPC development — is what **unlocks access to more of the region** (and, in a later layer, to other regions entirely). An NPC's specialization isn't just "they have an upgrade now," it's "what they've become determines what becomes reachable nearby" — directly generalizing P1's `bog_tender` → Fen Bog Hollow gating pattern into the primary mechanism by which the world opens up. The Flame Sword feeling and the "what is development for" question may be the same question seen from two angles: the payoff for sustained attention to an NPC might *be* a Flame-Sword-shaped moment — access, an item, information, or company — rather than a generic stat upgrade.

This reframing led to "diagnosis" language for NPC attention: early attention reveals what's missing or needed (not necessarily an item — could be a building that needs reviving first, a threat that needs dealing with, or simply more time/attention with no material cost), and addressing that is what lets the NPC become what the region needs. But this was also explicitly flagged as feeling potentially thin if it's *just* "hold E, get a popup telling you what to do" — the resolution in PROTOTYPE2.md is that attention sometimes produces progress directly (when whatever's needed is already satisfied) and *only* surfaces a "needs" message when something's blocking it, so diagnosis is information you're owed when stuck, not the entire point of every interaction.

### Buildings: revival, not specialization

Initially considered: buildings have a "basic" vs. "specialized" distinction, with the player choosing which building to develop in which direction. After working through it, this was reframed around the post-collapse setting more directly — **all buildings are dilapidated with a latent specialization** fixed at generation (biased by local materials/POIs, hinted at via description), and the player's choice is *which to revive*, not *what it becomes*. Reviving a building creates "demand" for a corresponding NPC specialization, which generic NPCs draw from when they specialize. This was further simplified in PROTOTYPE2.md to: no NPC is distinguished as "the specialized one" at generation either — uniformity at the start keeps things legible, and differentiation emerges entirely through play.

A "trust" mechanic — townspeople initially wary of an outsider, requiring some early action (like fixing something) before normal interaction opens up — was raised as thematically appropriate but explicitly deferred as post-prototype material. If it's added later, it should ideally layer on as a soft modifier (faster/slower progress, willingness to specialize) rather than a hard gate, so the early loop doesn't need retrofitting.

### Regions, unlocking, and travel — Kingdom vs. the roguelite framing

A bigger structural idea, explicitly placed *above* the prototype-scope work as a later layer: the world could be divided into **regions**, where development in one region unlocks access to others (a true Metroidvania/Dracula's-Castle structure). This was floated as a way to answer "what is development for" without either a savior narrative or an optimization treadmill — the reward for developing a place isn't more resources to optimize, it's *access to somewhere new*, and because development is about differentiation, *which* regions become reachable depends on *how* a given region was developed. Two playthroughs could open up genuinely different parts of the world.

Two framings for what happens to a region once you move on were discussed and compared against reference points:

- **Decay-while-away** (Kingdom: Two Crowns) — the place continues to be affected by your absence; there's an ongoing relationship even when you're not present. This was identified as closer to the Midlake feeling: the world continues, and continues to matter, regardless of whether you're looking at it.
- **Roguelite carry-forward** — leave a region in a "complete" state, carry some unlockables (items, NPC companions, knowledge) to the next region which starts in a state similar to where the first one began. This felt potentially *contrary* to the Midlake mood — if every region resets to a template, each one risks feeling disposable rather than a place that existed and will continue to exist.

A possible synthesis: **one-way travel doesn't have to mean the place stops existing** — it could simply mean the player can never go back to *experience* it again, while the place itself conceptually continues off-screen, shaped by what the player left it as. The melancholy isn't "this place is now locked," it's "you will never know what it became." This framing was offered as potentially the *strongest* Midlake beat of everything discussed, though it remains unresolved and explicitly **out of scope for the prototype** — PROTOTYPE2.md's dependency web operates entirely within a single region, and any region-level/inter-region layer should sit on top without requiring the core mechanics to change.

Kingdom (Two Crowns) was also flagged more generally as a strong mood reference point alongside Hollow Knight and the Midlake albums — particularly its "move and commit resources to things" core loop, which independently arrived at something close to P2's "move and attend" verb pair.

A related idea — town/region growth eventually hitting diminishing returns or tradeoffs, with some "secrets" only emerging at population/scale extremes — was raised as a possible future instance of "tried not chosen" operating at a larger scale, but filed as clearly post-prototype.

### No win condition

Explicitly decided: no completion or "fully unlocked" win-state for the prototype, and likely not for the final game either. Reasoning: it's easier to add a completion condition later than to remove one that's become load-bearing, and an open-ended, sandbox-like structure (where 100% "completion" doesn't mean the game is over, à la Stardew Valley) fits the desired feel better anyway.

---

## Post-Prototype Ideas

Ideas developed during and after P2's main prototype arc that are deliberately out of scope for the prototype but worth preserving with enough specificity that they can be designed against later. Organized by theme rather than chronology.

---

### Wilderness as a Shaped Landscape

**Spot tendedness as the seed, not the whole system.** P2's `tendedness` mechanic (Iteration 27) establishes that the wilderness reacts to how it's been attended. The long-game extension: spots don't just become more or less yielding — they can become genuinely *different* based on sustained attention patterns. A Field attended carefully and frequently for many sessions might eventually shift into something closer to a cultivated plot, with a different yield pool entirely. This isn't a menu unlock ("upgrade field to garden") — it's a threshold crossed by accumulated tenderness, discovered by the player returning and noticing the spot is different. The shift would be irreversible within a session but could reset between regions, so it's a relationship with a specific place rather than a permanent game-state change.

**Spatial relationships between spots.** Some spot types should affect nearby spots in ways the player can discover but is never told about. A well-tended Hollow Log (decay, fungi) raising the rare-drop rate of a nearby Mushroom Patch is the simplest version. A well-tended Bee Skep (pollination) improving the yield range of an adjacent Bramble Patch is another. These relationships would be authored per spot-type pair, checkable in the same `WorldContext` / `IOutcomeModifier` pattern already in place. The player discovers them through patient attention to the spatial layout of the wilderness rather than through any UI hint.

**The Ritual Manipulation View.** A distinct interaction mode for special-class objects: inner shrine sanctums, landmark cairns, threshold stones. Entering the mode requires sustained attention on the outer object (the same hold gesture, just longer). Inside it, the player sees a close-up spatial canvas — a small, hand-authored scene — and can place items from their pack, rearrange stone arrangements from a limited set, mark runes from a small vocabulary. The combination isn't a puzzle with a known solution. Different arrangements influence downstream systems (Wandering Thing odds, tended spot recovery rates, NPC post-spec progress speed) with feedback that's delayed by several sessions and never direct. The player might connect two things that happened to coincide, and might be wrong. This is "tried, not chosen" pushed into a domain where even the nature of the outcome is uncertain. Architecturally: a separate input mode (`ManipulationView` scene or subscene), a spatial canvas component, and a hash-to-modifier function that converts the current arrangement state into one or more `IOutcomeModifier` additions for the next N rests. The hash function should be designed so no two obviously different arrangements produce the same outcome, but so there's no optimal arrangement the player can reverse-engineer — lots of "different" but no "correct."

---

### Settlement Economy and Ongoing Demand

**The resource sink problem.** Items that matter only for the initial restoration/upgrade loop feel useless once that loop is complete. P2's maintenance mechanic (Iteration 29) solves this for common materials — buildings and NPCs now consume them as upkeep. The longer-term version extends this principle: items should have multiple pulls at different stages of settlement development, so something gathered early retains meaning later, just in different form. Clay isn't "restoration material for the Smithy." It's what the Smithy needs to stay warm, what the Bog Keeper uses to mark drainage channels, what a second settler might bring as an offering. The same item in three different demand contexts — but never all at once, and never advertised.

**Trade and the idea of surplus.** Once maintenance is established as a mechanic, a surplus of any item — more than the settlement's current needs — could power something qualitatively different from upkeep: attracting a traveling merchant, enabling a one-time offering at a shrine, supporting a wandering thing encounter with better odds. Not a market with prices, not an optimization target. More like: this place has more than it needs right now, and that abundance draws something. The "Wandering Thing variant that responds to surplus" is the prototype-adjacent version of this; a fuller trade-and-surplus system belongs here as a longer-term design direction. Key design constraint: surplus should feel like fortune, not like a resource-management problem to solve. If the player starts tracking "how much clay do I need to maintain X before I can afford to offer some," it has become optimization rather than tending.

**Item transformation without crafting.** The game currently has no crafting. The risk of adding it is the "how do I optimize my income" incentive that displaces the tending-oriented mindset. A lighter alternative: certain NPC developments enable item *transformation* rather than creation — the Smithy's NPC can convert Bog Iron into something more durable; the Hedge Witch can process Raven's Eye into something useful for a specific interaction. The player doesn't manage a crafting queue. They attend to the NPC (the same hold gesture), holding a relevant material, and the transformation happens as part of a development tick. Whether they get the transformed item or progress toward the transformation depends on the NPC's state. This keeps the item-gaining loop in the attend-to-world register rather than in a menu.

---

### Settlement Growth and Population

**Growth as attraction, not construction.** P2's first-arrival mechanic (Iteration 30) establishes the principle: people come to places that have become worth coming to, discovered rather than unlocked. The longer-term extension of this is a small, dynamic population — maybe 6-8 NPCs maximum for the prototype's scale — where the character of the settlement (which archetypes have been developed, which WorldState flags are set) shapes who arrives and what they can become. A settlement deep in ritual/shrine development attracts different arrivals than one deep in smithing and road-building. Not because there's a lookup table matching "shrine flags → shrine NPCs" but because the WorldState flags that gate arrival conditions are the same flags that post-spec NPC development sets — so the settlement's development history is implicitly its attractor profile.

**Housing as a soft cap.** Eventually, new arrivals should require something physical — not a "build a house" construction queue, but something like: a ruined structure in the settlement that hasn't been revived creates a possible dwelling, and the first unhoused arrival can settle into it once it's been attended to. The building restoration loop already handles dilapidated structures; a ruined dwelling would work the same way. The cap is therefore: as many settled NPCs as there are viable dwellings, where "viable" means restored to at least Stage 1. This gives building restoration a second reason beyond the specialization demand it currently creates.

**The settlement the player leaves behind.** In the eventual multi-region structure, leaving a region doesn't mean it stops existing — it means the player can no longer experience it directly. The shape of the settlement at departure (which buildings are developed, which NPCs have specialized, how many arrivals have settled, what the tenderness of the wilderness spots looks like) is the "save state" of that region as a place. Whether anything mechanically uses this (a later game revealing what a prior region became, a wandering NPC from a region the player left) is an open design question — but the record of it should be preserved so the option is available. Even if nothing reads from it, the player knowing it continues is part of the Midlake feeling.

---

### Regions, Travel, and Knowledge Transfer

*(See earlier entries on regions and unlocking in this document — the synthesis point and the Metroidvania structure are captured there. Adding only what emerged in the P2 design sessions that isn't covered above.)*

**Knowledge as a carried thing.** Tech transfer in the early design was framed as requiring physical presence (the player must bring an NPC from one town to another to transfer knowledge, or carry instructions once writing is available). P2 doesn't implement transfer at all — it's a single-region prototype. But the underlying principle is preserved in the WorldState flag system: flags are properties of a specific region's state, not of the player. A player moving to a new region starts with no flags, no realized specializations, no maintenance history. What they carry is the knowledge of how to cultivate — but not the specific cultivated state of the place they left. This distinction matters for the feel: the player is a person who knows what to do, not a save file that copies progress.

**Localized tech and the value of difference.** Two adjacent regions with different place archetypes should develop into genuinely different places, and the difference should have game-world consequences: a region with deep shrine/ritual development can do something that a region with deep smithing cannot, and vice versa. The player choosing which region to develop, and how, is indirectly choosing which futures become reachable. This is the long-term expression of "differentiation over advancement" — at regional scale, it means the world has genuine variety, not tier-progression in different skins.

#### 7-1-26
I'm not sure how much story-telling will be present in the game yet, but if there is any, there should be characters who saught the benefits of "the old ways", and characters who saught the benefits of imperial order. There should even be those who feel the loss of direction that imperial order brought, but are equally unable to find their way back to the "old path". Some will learn to forge a new way forward, while others will sink beneath the weight of loss.

---

### Druid Framing, Memory, and the Seasonal Cycle

Emerged from design discussion after Iteration 46. A premise-level shift, not yet scoped as any iteration — captured here as three threads to prioritize before drilling into one.

**The druid reframe.** The player is an ancient druid gathering and preserving knowledge of the land as Roman order recedes — straddling pre-Roman and Roman threads directly, rather than referencing them only through world dressing. This gives CLAUDE.md's writing/runescript note (the settlement's ability to preserve and communicate knowledge as a precondition for transfer) a concrete first-person frame instead of an abstract settlement property. It also names an explicit exception to the no-PC-development stance held elsewhere: a meta-skill like writing/runecraft — a capacity that changes *what persists*, not a direct-power stat like attack or enchanting — is compatible with the existing values. This is a deliberate carve-out, not a reversal of the broader stance.

**1. Memory — witnessed vs. recorded, and the forgetting problem.**
Splits property knowledge (Iteration 35) into two tiers: *Witnessed* (personally learned — today's `PropertyKnowledge`, ephemeral) and *Recorded* (transcribed by a rune-carver/scribe NPC, persists, populates a physical artifact in the world — a wall of rune-stones, a growing shelf — not a menu screen).

The open design problem is how witnessed knowledge fades without it reading as punishment. The target analog is spot exhaustion (Iteration 43): abundant and instant at first, then requiring progressively more effort to "recall," dwindling but never a hard cliff — framed as a conscious tradeoff, not a thing being taken away. This needs its own decay-cost curve, not a binary forgotten flag, and isn't resolved yet. A color-coded/mnemonic UI hint once text fades was considered and shelved — it shifts the burden onto the player's own memory, which doesn't fit the intended feel.

Design principle carried over from exhaustion: the unrecorded/oral cycle should read as a legitimate stripped-down default, not a lesser tier waiting to be obsoleted. Recording should be a net gain with a real cost (material, an NPC dependency, a trip) so oral memory keeps a niche — the "magic missile is weak, but it always hits" shape, not a strict upgrade path.

Also open: what triggers recording, given the game's single verb. Three candidate shapes, none chosen yet:
  - *Ambient* — right items plus the right town-side upgrades present → a discovery at a workstation auto-carves as a bonus outcome (`IOutcomeModifier`/`OutcomeRequest`), no separate trip.
  - *NPC-transformation-style* — carry the witnessed-but-unrecorded item to the rune-carver and attend them holding it; recording happens as a development tick. Reuses the "item transformation without crafting" pattern above directly.
  - *Hybrid* — ambient conditions gate whether recording is possible at all, but the actual verb stays attend-the-NPC.
  The latter two preserve the NPC-interleaving and settlement-development tie-in that the ambient-only version risks flattening away.

**2. Lore/Memory as a content type.**
Generalizes the "Reading the past" pursuit raised in an earlier brainstorm: attention over *meaning* rather than material. Fragments — an inscription, a story an elder tells, a barrow's shape — assembled into a picture of what the land was before the player arrived. No item output; the payoff is understanding itself, a literal expression of "The World Was Here Before You" rather than just a design principle behind the scenes.

Once Witnessed/Recorded exists, the same two-tier structure generalizes past item properties — a Wandering Thing's behavior, an NPC's story, a barrow's meaning could all use it, giving the "bestiary/library" impulse a diegetic home (physical artifacts you built) instead of a bare menu.

Two sibling pursuit-axes from the same brainstorm are relevant background, not elaborated yet: **Kinship/Trust** (a second axis of NPC relationship distinct from specialization — how they regard you, not what they've become) and **Continuity/Inheritance** (knowledge passing to a trusted NPC, or lost when an NPC's arc ends — needs Kinship first, and would be the one place permanent loss might belong, kept deliberately separate from the memory-fade mechanic above, which is trying hard not to punish).

Risk to watch: lore content is an easy place to violate "No Preaching." Keep entries as fragments and physical artifacts, not exposition paragraphs.

**3. Seasonal cycle — ambient influence and festivals.**
Ambient half: season/weather as another `WorldContext` read — the slot was deliberately left open (PROTOTYPE2.md's `WorldContext` note) rather than stubbed in early. Would shift item availability, yield tables, and Standing/exhaustion rates. Cheap architecturally; the open question is thematic meaning, not implementation.

Festival half: a fixed-cadence calendar event (not a full lunar/seasonal simulation at prototype scope), signaled diegetically a few days out — same low-weight injection style as a Site's hint line, no countdown UI. What's available at a given festival is assembled from whatever state already exists — a Site at Standing, a surplus of a property-tagged item, high Kinship/trust if that axis exists — never pre-flagged as "the thing the festival wants." Outcomes ripple forward as a season-long ambient condition (softer exhaustion decay, a passive-drift boost) rather than a direct reward, keeping festivals in the same tried-not-chosen register as everything else. No log, no "next year, do X" — if the player wasn't ready, that's simply what happened this cycle.

**Prioritization note:** Memory (#1) is the most self-contained starting point — it only touches the existing property system and doesn't require the druid reframe, Kinship, or the seasonal system to exist first. Lore (#2) and Seasonal Cycle (#3) both lean on pieces that don't exist yet (a broader knowledge-content type; `WorldContext` weather/season) and are better sequenced after Memory proves out.

---

### Historical Everyday Life as a Core Pillar (Not the RPG Triad)

Raised as a core-pillar question, not a tweak: most RPG/farm-sim games run on combat power, wealth accumulation, and gear progression. This game's premise (knowledge-gatherer helping settlements gain access to land, history, and people) calls for historically real replacements for that triad, not just new activities layered onto the existing loop.

**The triad, replaced:**
- Combat power → **resilience against want.** Success in this period was the kin-group surviving a bad winter, a raid, a disease year — not personal strength. The maintenance economy already does this; it just isn't named this way yet.
- Wealth accumulation → **reputation through redistribution.** Status came from *giving away* well — a chieftain's worth was measured by generosity at the mead-hall, not a stockpile. Inverts the usual "acquire more" instinct.
- Gear/itemization progression → **craft mastery and preserved knowledge.** The properties/recording system already built *is* the historically-correct replacement for loot progression — worth recognizing as the deliberate answer to this, not a side system that happens to exist alongside it.

**Four real pursuits with no analog yet:**
- **Boundary-walking / land-right.** Anglo-Saxon charters describe land grants by walking the perimeter and naming what's there — a tree, a stone, a stream bend. A different relationship to wilderness than foraging: not "what does this place give me" but "what does this place mean, and to whom." Maps directly onto the druid-as-knowledge-keeper premise.
- **Hospitality and gift-giving as politics.** Not surplus triggering a merchant (the existing Trade/surplus idea above) — actual hosting and giving as the primary way standing grows. Reframes wealth from a resource sink into a redistribution act with social payoff.
- **Propitiation.** Giving something back to maintain a place's favor — ritual-register, distinct from attending-to-extract. The historically real version of the Ritual Manipulation View already captured above: maintaining a relationship with the land, not a mechanic for extracting more from it.
- **Lawspeaking / genealogy.** A lawspeaker held precedent and lineage in memory since writing was rare — a real oral profession, and a different knowledge-type than craft knowledge: who has right to what, and why. Likely the deepest, hardest-to-record tier the Memory mechanic (above) could have, and a natural home for the Continuity/Inheritance axis already flagged there.

Propitiation and Lawspeaking connect directly to threads already queued for the next session (Ritual Manipulation, Memory/Continuity). Boundary-walking and Hospitality are new territory, not yet connected to anything built.

---

### Everyday-Life Activities (Supporting Material, Not Yet Prioritized)

Raised alongside the above as raw material for content richness — grouped by how directly each connects to what's already built, not by importance.

**Content — sits next to existing archetypes with no new geography needed:**
- **Peat/turf cutting** — the primary fenland fuel source. At home in Fen Bog specifically.
- **Eel/fish weirs** — well-documented Anglo-Saxon wetland economy. Fits next to Reed Marsh/Fen Bog.
- **Charcoal-burning** — iron-smelting needs charcoal, not raw wood. A collier's clamp burns slow over several unattended days — a near-perfect fit for existing Standing/multi-day pacing. Pairs with Bog Iron and the Bog Keeper's existing thread.

**Mechanics — ties directly into the Memory thread above:**
- **Beekeeping** (Bee Skep already floated once) — honey for mead, and **wax for tablets**, the real physical medium for pre-parchment writing. Candidate as the actual material the rune-carver/scribe mechanic consumes, not just flavor.
- **Herbal medicine / a leechbook** — Bald's Leechbook is a real Anglo-Saxon text. A player-assembled leechbook gives Witnessed/Recorded knowledge an already-historical container instead of an invented one.

**Larger theme-inspired systems — genuinely new activity categories, not yet touched:**
- **Textile work** — wool processing (shearing, carding, spinning, weaving) and plant-dyeing (woad, madder, weld). The single biggest untouched everyday-life category. Dye-plants map onto the property system almost for free — a plant's dye result *is* a property.
- **Tanning/leatherworking** — an NPC role from the earliest brainstorm in this document, never built. A smelly, multi-step transformation process — a good fit for a station distinct from the Workshop.
- **Brewing/mead-making** — ties into mead-hall political culture (a stated research interest) and into the Festival thread above — feast hospitality as a real activity, not flavor text at the festival.

Strongest three to pull on first given what's already built: peat-cutting (nearly free, fits Fen Bog today), beekeeping/wax (gives Memory its historical container), and textiles (biggest untouched category, cleanest link to properties).

---

### Synthesis: Provenance, Reciprocity, and Kinship (emerged from Druid Framing follow-up, 7-8-26)

Reached by stress-testing concrete scenarios against theme and design values, rather than starting from mechanics — a deliberate reversal of the usual order, done because the moment-to-moment gameplay vision (see gameplay/aesthetics discussion, same session) was still too abstract to build from directly.

**The test case.** Bottom-up question: how would a post-imperial village actually fix a dilapidated building, per theme alone? The honest answer — reused/spolia material with known provenance, not raw extraction; collective or reciprocal labor, not solitary work; a real skill bottleneck, not a quantity threshold — exposed that the current wilderness-gather → settlement-build loop fails on three specific counts, not just mood: material is anonymous and ownerless, extraction and construction are solitary, and there's no knowledge gate. This is the same diagnosis as the standing field-grind complaint (Site attendance for rare drops), generalized: **the wilderness currently behaves like a vending machine and the settlement like a crafting bench**, when the theme wants materials to carry provenance and construction to carry social and skill cost.

**The resulting spine, four pieces that turned out to be one integrated thread:**

- **Memory generalizes into currency.** Witnessed/Recorded knowledge (design discussion, Iteration 46 follow-up) isn't just item-property knowledge with a decay curve — it's the thing a knowledge-keeper has to give when they lack surplus goods or spare hands. A story, a name, a piece of preserved knowledge becomes tradeable the same way a material good is. This is a strong thematic fit for the druid frame specifically: the player's contribution to the social economy is memory and connection, not stockpiled goods.

- **Provenance attaches to Sites, not a new system.** The existing Site architecture (named locations, VisibleInert/Interactable states, Standing) already gives places identity. Provenance is an attribute layer on what's already there: a claim-state per source (ownerless ruin — free but limited; a living farmstead's wall — asking required; a boundary cairn — taking from it means something). Fixes the vending-machine problem without waiting on boundary-walking or Kinship to exist first; both can extend it later.

- **Facilitated Reciprocity — player as outside-facilitator, not participant.** A druid/knowledge-keeper is historically liminal — valued by communities without being fully of them. Resolving a real tension flagged in this session (gift-giving reciprocity vs. the Midlake narrator's remove/loneliness): the player doesn't build a personal web of obligations, they enable reciprocity *between* NPCs — carrying word, discharging a debt on someone's behalf, making a hosting or a repair possible. Keeps the social fabric moving because of the player without making the player its center.

- **Kinship/Trust as NPC↔NPC relational state, same shape as exhaustion/Standing.** Explicitly *not* a player↔NPC friendship meter (rejected during this session as in direct conflict with the Midlake mood — the original Stardew-style framing doesn't fit). Instead: a range-valued state between NPCs (or aggregated at settlement level), perceivable and nudgeable via the Ritual Manipulation View, that shifts through facilitation and time rather than a single trigger — "tried not chosen," ambient over triggered, same as everything else. Reads into multiple systems the way WorldState flags already do: some activities get cost/duration modifiers, others get outcome-probability modifiers. Gates **collective settlement-scale outcomes** (a multi-hand repair, a festival, a coordinated response to a bad season) rather than individual NPC specialization — the direct mechanical expression of the existing "resilience against want" reframe (kin-group surviving together, not personal strength). A settlement riddled with unresolved feuds is fragile regardless of stockpiled material or any one NPC's skill level.

**A fifth piece, resolving a separate tension from the same session:** skill-use growth (the Skyrim pull) reframed as **accumulated relationship between the player's attention and a specific place or practice** rather than a player-character power stat — consistent with the no-PC-development stance, and it turns out to double as the knowledge-modifier for craft/repair work: a steep-but-improving cost curve at low relationship, same shape as spot exhaustion and memory-fade, not a hard skill gate (which would violate Tried Not Chosen).

**How this relates to the original three threads.** Memory absorbs and is strengthened by this, rather than sitting separately. Provenance and Reciprocity are new — they didn't exist as named threads before this session; they emerged from testing a concrete scenario rather than being brainstormed directly, which is itself a useful data point about where the design process needs to run bottom-up rather than top-down. Lore still follows Memory once Witnessed/Recorded is generalized, per the original prioritization note. Seasonal Cycle remains queued behind all of this — nothing in this session moved it up.

**Open, not yet resolved:**
- What "give a memory" actually looks like as a verb/interaction, distinct from attending a Site or trading a material.
- How provenance claim-states are authored/generated per Site (manual per-archetype, or systemic).
- Whether Kinship is tracked per-NPC-pair, per-NPC-vs-settlement, or both, and what data shape that implies.
- Whether the "relationship-with-place" skill mechanic and the Kinship mechanic should share an underlying value-and-decay implementation, given they're structurally identical (range, shifts through use/facilitation, reads into cost/outcome elsewhere) — worth checking before building either, to avoid duplicating a pattern.

---

### Course Correction (7-9-26)

The Provenance/Memory/Reciprocity/Kinship spine (see synthesis above) was reasoned into existence entirely through thematic argument — "what would someone in this world actually do" — chained across several sessions without touching the greybox prototype at any point. That's a process failure specific to a greybox prototype: theme is good for generating candidates and killing bad ideas fast, but it can't tell you whether a mechanic is fun or whether several separately-plausible systems actually cohere in play — only building and testing can answer that. Worth naming explicitly: this spine also turned out to be quietly merging three things that don't automatically belong in one game — the Midlake mood (solitude, remove), the druid/post-imperial framing (socially embedded, reciprocity-driven), and settlement-development mechanics (systems, progression) — and Kinship specifically exists only because the reciprocity thread needed a mechanism, not because it was a wanted feature on its own terms. The spine isn't discarded, but it's shelved as a package until there's a played, working mechanic to check it against, rather than more argument. Next step reverts to the original complaint that kicked this off — the wilderness field-grind — approached theme-agnostically: smallest possible greybox change, one hardcoded spot, testing only whether tending-shifts-odds-over-time feels better in play than flat RNG. Theme gets reintroduced only after something is actually fun.

---

### Wilderness Loop → Living Economy (7-14-26)
Traced a single thread from the original field-grind complaint through several tested increments (Iterations 47-54) to a much larger reframe of what the game might fundamentally be about.
What got tested, in order: Site-scoped Standing and clustered member spots killed single-spot camping, but flattened all spots in a site into interchangeable contributors to one meter — solved the grind, cost differentiation. Attention-weighted flow bonus (dominance between two archetypes) proved mechanically sound but was imperceptible in play — not a magnitude problem, a comparison-point problem: post-spec tracks are too short to have a "last time" to feel faster than. Fix wasn't a louder number, it was a continuous ambient state (building halo) instead of a discrete rate. That same lesson was carried forward into the not-yet-built flow-filled reserve pilot before it could repeat the mistake. Once site-level mechanics were legible, the differentiation loss became visible as its own problem — direct tension with Differentiation, Not Advancement — leading to Iteration 54, restoring per-spot seams on top of (not instead of) the shared site meter.
Where this is now pointing: the resource/stockpile framing itself may be the wrong container. Emerging alternative — items aren't quantities to accumulate, they're elements: discrete, catalog-worthy things (Flint, Bog Iron, Iron Bloom as a distinct advanced variant) whose significance is knowledge of their existence and properties, not how many you're holding. Properties (already built, Iteration 35) are the real content — learned from workshops or from tending NPCs who hold expert knowledge. Settlements start with partial knowledge of their local elements already (consistent with the mid-process-start thread, Iteration 49) and productivity shifts not by the player supply-chaining materials in, but by the player introducing knowledge the town didn't have — a property, an element's existence — nudging what a place produces or advances toward. Explicit target: remove direct item/supply management as the primary loop, leaving attention-allocation (Iteration 50's model) as the only lever. A literal in-world catalog (physical object/place, not a menu) was raised as the container for "knowledge of all elements and properties" as a visible goal state.
Flag, not a decision: this is a wide reframe assembled across one long conversation, argued rather than tested — the same shape as the Druid/Provenance/Kinship package that prompted the original Course Correction. Worth treating as shelved-but-alive the same way, not built toward directly. If it holds up, Iteration 54's differentiated seams and the flow/reserve thread (52-53) are probably the right existing on-ramp: they already point at "knowledge/influence, not delivery" without having been framed that way yet.

---

### Protagonist Goal: Roaming Debt, and the Interconnected-Sites / War Scars Threads (7-23-26)

A session working backward from a felt gap: P4's playtests confirmed acquaintance-as-activity works, but nothing yet says *why* the protagonist is doing this or what the loop is ultimately measured against — the "big purpose" the smaller mechanics push toward. Distinct from the shelved Druid Framing (Course Correction, 7-9-26) in both process and content: this thread stayed theme-level and deliberately stopped short of mechanizing anything beyond a light reframe of what already exists, precisely to avoid repeating the argued-not-tested failure mode.

**Backstory, revisited and left open.** Returned to the pre-druid "soldier returning from a lost war" premise (present in this document since the earliest MVP brainstorm) as a better fit for the Midlake mood than the druid frame — Midlake's characters read as achingly average, not central or heroic. Six variant backstories were brainstormed (foederatus discharged from imperial service; loser of a local tribal war; garrison left behind after withdrawal; unwilling conscript; sole survivor of an annihilated unit; a man who tried to change sides too late). None was chosen — left as flavor to pick from later, since the goal question turned out to be separable from and prior to the specific backstory.

**Four candidate framings for the protagonist's big-picture goal**, evaluated against one constraint: it must not resolve into "find a place to put down roots," since that's character-arc thinking and this game is explicitly about the world's development, not the protagonist's.
1. **Witness** — understand a place fully before it changes past recognition. Strong Courage of Others mood fit, but risks tipping into pure nostalgia/preachiness, and doesn't itself generate a measurable loop-anchor.
2. **Debt** — the protagonist owes something to these lands (structurally, via his side's role in the war/collapse), and the goal is discharging that debt through understanding and small repair. Chosen as the working answer — see below.
3. **A carried message/purpose** — compelling but risks collapsing back into a personal-arc quest, which is the thing being avoided.
4. **No goal, attention as sufficient unto itself** — the purest Midlake option, but self-erasing as a design anchor: gives the smaller mechanics nothing to push toward or be measured against.

**Debt selected, roaming not per-site.** Considered whether debt should be tied to a specific home region (stronger "why leave" beat, but requires backstory specificity that fights procedural generation) or diffuse/roaming (travels more easily, never fully resolves at any one site). Roaming was chosen for now, explicitly deferred rather than closed off — per-site could still be layered in later once regions have more identity.

**Debt maps onto P4's existing acquaintance track with no new system.** The key move: debt isn't a separate ledger to build, it's a *reading* of the acquaintance mechanic that already exists. Attention is the only currency the protagonist has to give — thematically apt for someone with nothing else to offer. A site's acquaintance curve flattening (most entities Familiar/Known, flavor pools trending toward "worn," fewer new reads surfacing) is the existing, already-built signal for "I've done what I can here, time to move on" — no new completion meter required, and consistent with the standing "no win condition" decision. Debt is deliberately never fully zeroed at any one site, which avoids the ledger-closing/absolution problem that made framing #1 (Witness) risky, and keeps moving on feeling like a soft, felt beat rather than a checklist completion. Named risk, not yet solved: don't build a debt meter as a *second* number alongside acquaintance — that repeats the "second verb to fix flatness" mistake this project has already identified and rejected once.

**Thread A: Interconnected sites (the Metroidvania itch).** A long-standing idea (present since the earliest MVP brainstorm, re-raised here) — visiting one site should be able to unlock or bias something at another, whether nearby or distant. Named tension: true Metroidvania-style legible hard gates (specific key unlocks specific door) conflict directly with existing design values (Tried Not Chosen, Felt Not Read, "no required chains" already stated in FEATURES.md's cross-influence work). Three positions on the legibility spectrum were named: pure ambient bias (what already exists, e.g. the Osier Bed → Smoking Racks seam), soft gate with multiple satisfiers (a blocked thing has several possible unlock paths, cross-site knowledge being one), and hard single-key gates (rejected as too far from existing values). Working resolution, articulated in play-feel terms rather than mechanism terms: most development stages are already gated on *something*; the natural shape is that early visits to a first site surface little because the player's knowledge pool is thin, and each new thing learned anywhere quietly raises the odds of progress elsewhere — producing a recurring, ungated feeling of "I wonder if anyone back at the last town could use this" rather than a specific key-to-lock puzzle. This is explicitly a turn-up-the-visibility-of-existing-bias move, not a new hard-gate system — closest existing precedent is the soft cross-influence seam work already scoped in FEATURES.md.

**Named open risk on Thread A, not yet solved:** a pacing/ceiling problem. If knowledge and properties are a shared, finite vocabulary that accumulates as the player visits more sites, there's a real risk that late-game sites become trivial to "roll through" once the player's knowledge pool is large enough to satisfy most soft gates on sight — the inverse of the intended slow-reveal feel. No fix proposed yet; flagged as a problem that only bites once procedural multi-site rollout (P4.6+) exists, not something the next pilot needs to resolve. Worth deliberately revisiting before or during procedural rollout.

**A related, separately-shelved idea surfaced and explicitly deferred:** losing acquaintance/knowledge of a previous site upon leaving, as a felt-loss mechanic in its own right (distinct from debt — this would be about the cost of moving on, not the reason for it). Noted as worth trying later but not pursued now, in favor of following the interconnected-sites thread first.

**Thread B: War scars, chosen as the first concrete build target.** Physical sites bearing visible damage from the war/collapse — ruined buildings, burned homesteads, wrecked carts and goods — framed as recovery-from-tragedy rather than any implied arc of technological or civilizational progress, which keeps it clear of the "progress preaching" risk flagged elsewhere in this document. Mechanically near-free within the existing entity taxonomy: a war-scar site is seeded at a different starting state (`Ruined`, standing before the existing dilapidated-with-latent-specialization stage), with a branching resolution instead of a single restore path — **salvage-and-move-on** (finite yield, becomes an inert marker, "proper burial") versus **repair-and-restore** (rejoins a normal development track toward restoration).

**The fork is site's-choice, not player's-choice — true to Tried Not Chosen.** Explicitly rejected: a menu prompt asking the player to pick salvage or repair. Instead, the outcome is biased by *how* the site is attended, the same lever tendedness already uses (`WildernessYieldAttendable.WithTendednessSuffix`'s existing 0.3/0.7 banding, and 4.14's `character` axis are the direct precedents). Working shape: a single float (`recoveryLean`, 0–1) nudged by the manner of attention rather than a choice — extractive, take-and-go attends nudge it toward salvage; attends that continue past the point where there's anything left to take (staying once there's no reward left) nudge it toward repair, as the legible tell of care versus extraction. Crossing a threshold resolves the site into one of the two tracks, using the same stage-cross conventions as everything else.

**Explicitly not a dead end, but deferred out of this pilot.** A salvaged-and-laid-to-rest site shouldn't be permanently inert forever — reclamation by nature or by other people (new settlers, a different NPC's use) as a second-order drift state is a real and good idea, structurally similar to tendedness's drift-toward-wild. Named here as a future extension, explicitly not built in the first pilot — the priority is testing whether the primary fork lands at all before adding a second transition on top of an untested mechanism.

**Pilot scope, decided:** the war-scar entity is a POI-style wreck/midden (a wrecked cart-and-goods scene or similar), not a Building — finite salvage, less structurally complex than a repairable building, and closer to existing `LandmarkAttendable`/wilderness-yield precedent. One hand-placed instance, no procedural rollout. The pilot is a pure mechanism test: no debt-flavored language or framing folded in yet — the question under test is narrowly whether a site whose fate is legibly shaped by manner-of-attention (without ever presenting a choice) reads as meaningfully different in play from ordinary tendedness drift, since on paper the mechanism is structurally very close to 4.14. If it doesn't feel different in play, that's signal the idea needs a sharper mechanical hook, not just new flavor on tendedness — debt/roaming language is deliberately withheld until the bare mechanism has been felt on its own.

**Immediate next step (not yet scoped as a numbered iteration):** define the smallest possible single-entity pilot — one hand-placed wreck/midden POI, starting `Ruined`, with the `recoveryLean` mechanism and its two-track resolution as the only new mechanism under test.