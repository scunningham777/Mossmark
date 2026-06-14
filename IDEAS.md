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

This also collapses the day-cycle bookkeeping from Prototype 1 (a 7-action pool tracked alongside play) into the play itself: attention **is** stamina **is** the day's clock. Spending attention spends the day; there's no separate layer to manage. This was identified as a meaningful simplification, not just a re-skin — the old day cycle was a second system sitting on top of the moment-to-moment loop, and P2 makes them the same thing.

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
