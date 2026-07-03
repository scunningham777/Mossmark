# Mossmark — Data Schema (Prototype 2)

Seven CSV files under `Tools/Data/` define the authored game data. Each maps to a Unity ScriptableObject type or to entries within one. Asset names are the universal key — every cross-file reference uses an asset name (or an item's `DisplayName`), never asset paths or GUIDs.

- **Import**: `Mossmark/Data/Import All` — applies CSVs to assets, creating missing assets (and folders) on demand. Re-running on unchanged CSVs is a no-op (0 assets updated), including `[SerializeReference]` condition arrays.
- **Export**: `Mossmark/Data/Export All` — regenerates all CSVs from assets. **Export drops `#` comment lines**; re-add durable authoring notes after exporting, or keep them in this document.
- **Self-test**: `Mossmark/Data/Validate Condition Import (self-test)` — round-trips `Tools/Data/_sample_conditions.csv` through the polymorphic condition importer against an in-memory probe. Run it after adding a new condition type to the pipeline.

This structure deliberately mirrors Prototype 1's relational pipeline (see `DATA_SCHEMA_REFERENCE.md` for that historical reference): small single-purpose row types, one row per entry, ID-joined files, and ID-addressable pool assets that any number of owners can reference.

---

## File Overview

| File | Maps to | Creates / updates |
|------|---------|-------------------|
| `yield_tables.csv` | `YieldTable` | One SO per unique `tableId` in `Assets/Game/Data/World/YieldTables/` |
| `wilderness_spots.csv` | `WildernessSpotDefinition` | One SO per row in `Assets/Game/Data/World/Spots/` |
| `npc_stages.csv` | `NpcStageDef` + `NpcStagePool` | One stage SO per row in `Assets/Game/Data/Development/NpcStages/`; one pool SO per unique `pool` in `Assets/Game/Data/Development/Pools/` |
| `building_stages.csv` | `BuildingStageDef` + `BuildingStagePool` | One stage SO per row in `Assets/Game/Data/Development/BuildingStages/`; one pool SO per unique `pool` in `Assets/Game/Data/Development/Pools/` |
| `stage_conditions.csv` | `[SerializeReference] IDependencyCondition[]` on NPC **and** building stage SOs | Updates the `conditions` array on existing stage assets, keyed by `stageId` |
| `place_archetypes.csv` | `PlaceArchetype` | Updates existing SOs in `Assets/Game/Data/World/Archetypes/` |
| `wandering_things.csv` | `WanderingThingDefinition` | Updates existing SOs in `Assets/Game/Data/World/WanderingThings/` |

Import order (fixed in `ImportGameData.ImportAll`): yield tables → spots → NPC stages → building stages → archetypes → wandering things. `stage_conditions.csv` is read once up front and applied during both stage imports. Archetypes run last because they reference spot and pool assets by name.

Multi-value cells use semicolons (`spots`) or the packed yield format (below). Booleans are `true`/`false`. Colors are per-channel `_r`/`_g`/`_b` float columns.

**Packed yield format**: `Item Name:weight:minQty:maxQty`, comma-separated for multiple entries within one (quoted) cell, e.g. `"Clay:0.7:1:2,Flat Stones:0.3:1:2"`. Items are referenced by `ItemDefinition.DisplayName`.

---

## `yield_tables.csv`

One row per entry; rows sharing a `tableId` populate one `YieldTable` — the P2 analogue of P1's `loot_tables.csv`. Referenced from `wilderness_spots.csv` via `rareYieldTable`; a set table reference **replaces** that spot's inline `rareYields` list. Author a table only when a pool is genuinely shared across owners — inline lists remain right for one-off content. There is no `commonYieldTable` — no shared common pool has existed yet; add one only if that changes.

| Column | Type | Notes |
|--------|------|-------|
| `tableId` | string | Asset name. All rows with the same id form one table, in row order. |
| `item` | string | Item `DisplayName`. |
| `weight` | float | Relative weight within the table. |
| `minQty` / `maxQty` | int | Quantity range when this entry is picked. |

Live example: `old_coin_finds` — shared rare pool for Clay Pit, Field, and Old Road, which previously duplicated identical inline Old Coin rare yields.

---

## `wilderness_spots.csv`

One row per `WildernessSpotDefinition` — both the generic/tended pool spots randomly placed by `WorldGenerator` and the archetype spots referenced from `place_archetypes.csv`.

| Column | Type | Notes |
|--------|------|-------|
| `assetName` | string | Asset key; created on import if missing. |
| `spotId` | string | Stable id other systems reference — `npc_stages.csv`'s `passiveDriftSourceSpotId` reads this spot's runtime tendedness. Empty for spots nothing references. Extracted archetype spots kept their former archetype id (`bog`, `sacred_grove`, …) so drift-source data carried over unchanged. If several spawned instances share an id, the last spawned wins the registry slot. |
| `kind` | `Generic` \| `Tended` | Generic = ongoing hold, yields per tick; Tended = mark → rest(s) → harvest. |
| `displayName`, `color_r/g/b`, `interactionVerb` | | Presentation. |
| `commonYields` | packed yields | Weighted per-tick pool (Generic only). |
| `rareYields` | packed yields | Weighted **pool** of rare candidates — `rareDropChance` gates whether a rare drops; the pool decides which. A single entry reproduces the old scalar behavior. |
| `rareDropChance` | float | 0–1 per tick, modified at runtime (twilight ×1.5, tendedness bands). |
| `rareYieldTable` | string | Optional `yield_tables.csv` `tableId`; when set, replaces the inline `rareYields` list. |
| `minTickInterval` / `maxTickInterval` | float | Seconds per attention tick (generic pool 1.5–2.0; archetype spots 2.0–2.5 per iteration 26). |
| `tendVerb`, `harvestYields`, `restsToHarvest`, `maxConcurrentMarked` | | Tended-kind fields. |
| `knowledgeN_flag` / `knowledgeN_specializationId` / `knowledgeN_item` / `_minQty` / `_maxQty` / `_weight` | | Knowledge yield injections (iterations 28/34): entry activates via WorldState flag **or** realized specialization (flag wins if both set) and injects the item into the common pool per tick. N = 1, 2, 3, … |

---

## `npc_stages.csv`

One row per `NpcStageDef` asset — the P2 analogue of P1's `upgrades.csv`. The `pool` column groups stages **in row order** into `NpcStagePool` assets; row order is track order.

| Column | Type | Notes |
|--------|------|-------|
| `stageId` | string | Asset key and the join key into `stage_conditions.csv`. Unique across NPC *and* building stages. |
| `displayName` | string | Stage/development name shown on apply. |
| `progressCost` | int | Attention ticks of accumulated progress required (carries the iteration-33.1 doubling: stage 1 = 6, stage 2 = 8). |
| `pool` | string | `NpcStagePool` asset name. Empty = stage exists but no pool grants it. |
| `flavorText` | string | Logged as `{specializedName}: {flavorText}` — exclude the NPC name. |
| `worldStateFlag` | string | Flag set true on apply (suspended when the NPC goes cold, restored on maintenance reset). |
| `passiveDriftSourceSpotId` | string | `wilderness_spots.csv` `spotId` whose tendedness drives rest-driven passive progress (0 / +1 / +2 by band). Empty = attention-only. |

The archetype's own "specialization realized" gate is **not** authored per stage — `NpcAttendable.BuildPostSpecStages()` prepends it structurally, since post-spec stages presuppose their archetype's specialization.

---

## `building_stages.csv`

One row per `BuildingStageDef` asset. The `pool` column groups stages in row order into `BuildingStagePool` assets — **each pool's first row is the revival stage**, whose `material` doubles as the building's ongoing maintenance material.

| Column | Type | Notes |
|--------|------|-------|
| `stageId` | string | Asset key and `stage_conditions.csv` join key. |
| `displayName` | string | Stage 0's displayName is also the building's revived name. |
| `verb` | string | Interaction verb ("shore up", "consecrate", …). |
| `material` | string | Item `DisplayName` consumed per productive tick. |
| `costPerTick` | int | Material consumed per tick. |
| `progressCost` | int | Ticks to complete the stage (iteration-26 baseline: 4). |
| `pool` | string | `BuildingStagePool` asset name. |
| `tint_r/g/b` | float | Building tint once this stage completes. |
| `worldStateFlag` | string | Flag set true when this stage completes (iteration 34, seam 3). |
| `stationName` | string | Iteration 39: name the building takes on when its station opens (empty = name unchanged). Only meaningful on a pool's final stage. |
| `biasPropertyIds` | string | Iteration 39: semicolon-separated property ids. Non-empty on a pool's **final** stage makes the fully-developed building a conversion station; recipe resolution and property discovery at that station are filtered to these ids. Coverage rule: every `PropertyRegistry` id must appear in ≥2 stations' bias lists. |

The material-availability gate is implicit — `BuildingAttendable` derives it from `material`/`costPerTick`. Extra gates (the old `requiredSpecialization`) are authored rows in `stage_conditions.csv`.

Live stations: `workshop_restoration` (pool `workshop_pool`, scene-placed Workshop), `bog_fen_shrine` (Fen Shrine, bog building's third stage), `sacred_grove_hearth` (Consecrated Hearth). Conversion recipes themselves (`ConversionDef`) are not in the CSV pipeline — authored as `.asset` files under `Assets/Game/Data/Conversions/` and referenced from the scene's `WorkshopUI.recipes`.

---

## `stage_conditions.csv`

One row per `IDependencyCondition`, keyed by the owning `stageId` (shared by NPC and building stages) — the P2 analogue of P1's `upgrade_dependencies.csv`, generalized to P2's polymorphic condition system. Only the columns relevant to a row's `conditionType` are populated.

| `conditionType` | Constructs | Populated columns |
|-----------------|-----------|-------------------|
| `item` | `ItemAvailableCondition(item, quantity)` — carry + chest availability (a pure gate, not a spend) | `item`, `quantity` |
| `property` | `PropertyAvailableCondition(propertyId, want)` — carry-only; one matched item is consumed when the stage fires | `propertyId`, `wantDescription` |
| `worldflag` | `WorldStateCondition(flagId, requiredValue, needs)` | `flagId`, `requiredValue`, `needsDescription` |
| `spec` | `SpecializationRealizedCondition(specId, needs)` | `specializationId`, `needsDescription` |
| `time` | `TimeCondition(progress)` — always satisfied ("needs more time") | `quantity` (as progress) |

The importer (`ConditionCsvImporter`) constructs the concrete C# object per row and assigns it via `SerializedProperty.managedReferenceValue` into the stage's `[SerializeReference]` array — the standing pattern for any future condition-bearing data. To support a new condition type: add a case to `ConditionCsvImporter.BuildCondition`, a case to `ExportGameData.AppendConditionRows`, make the condition class `[Serializable]` with `[SerializeField]` (non-readonly) fields, and extend `_sample_conditions.csv` + run the self-test. `EntityStateCondition` (scene-object reference) stays hand-authored in scene YAML.

---

## `place_archetypes.csv`

One row per `PlaceArchetype` — a thin composition root bundling independently-real pieces by reference. Rows update existing assets (archetypes are not auto-created; they carry hand-authored exchange flavor arrays that live only in the `.asset`).

| Column | Type | Notes |
|--------|------|-------|
| `assetName`, `archetypeId`, `displayName` | string | Identity. |
| `spots` | string | Semicolon-separated `wilderness_spots.csv` asset names — any number; one instance of each spawns per session. List a name twice for two instances. |
| `specializationId`, `stageDisplayName`, `npcTitle`, `npcTint_r/g/b` | | Specialization identity. |
| `poiDisplayName`, `poiLockedDescription`, `poiVerb`, `poiColor_r/g/b`, `poiCommonYields`, `poiRareYields`, `poiRareDropChance`, `poiMinTickInterval`, `poiMaxTickInterval` | | POI block — stays embedded (no cross-archetype reuse case yet). `poiRareYields` is a weighted pool like the spot equivalent. |
| `npcStagePool` / `buildingStagePool` | string | Pool asset names from `npc_stages.csv` / `building_stages.csv`. Empty = none. |
| `npcMaintenanceMaterial` | string | Item `DisplayName` the NPC maintenance path consumes — explicit reference, was `CommonYields[0]` positionally. |
| `buildingColdFlavor`, `buildingMaintenanceCost`, `npcColdFlavor`, `npcMaintenanceCost` | | Iteration-29 maintenance config. |
| `buildingDilapidatedName`, `buildingDilapidatedColor_r/g/b` | | Pre-revival building identity. |

NPC exchange gifts/visit flavors are **not** in the CSV pipeline — they're multi-line flavor text with an index-paired gifts↔flavors constraint, authored directly in the `.asset`.

---

## `wandering_things.csv`

Unchanged by the migration — one row per `WanderingThingDefinition` with dynamic `modN_flagId`/`modN_multiplier` odds-modifier columns. See the file itself.

---

## Authoring checklists

- A new **shared yield pool**: row(s) in `yield_tables.csv`; reference from a spot's `rareYieldTable`.
- A new **wilderness spot type**: one row in `wilderness_spots.csv`. Give it a `spotId` only if something will reference it. Add it to an archetype's `spots` list and/or the scene `WorldGenerator.spotPool` for random placement.
- A new **NPC post-spec stage**: a row in `npc_stages.csv` (with `pool`), gate rows in `stage_conditions.csv`. New pool names create pool assets automatically; point an archetype's `npcStagePool` at it.
- A new **building stage**: a row in `building_stages.csv` in the right pool position (order matters; first = revival), gate rows in `stage_conditions.csv` if it needs more than material.
- A new **conversion station**: set `stationName` + `biasPropertyIds` on the building pool's **final** stage row in `building_stages.csv`. Audit that every property id still appears in ≥2 stations' bias lists. New recipes are `ConversionDef` assets added to the scene's `WorkshopUI.recipes`.
- A new **archetype**: create the `.asset` (menu: Mossmark/World/Place Archetype), then a `place_archetypes.csv` row referencing its spots/pools; add it to `RegionData.ArchetypePool` by hand.
- A **cross-pursuit seam**: wilderness→NPC = `passiveDriftSourceSpotId` on the stage row; NPC→wilderness = `knowledgeN_specializationId` on the spot row; building→anything = `worldStateFlag` on the building stage row + `knowledgeN_flag`/`worldflag` gates on the consumer.
