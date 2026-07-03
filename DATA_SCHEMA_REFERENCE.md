# Mossmark — Data Schema

Sixteen CSV files define all authored game data. Each maps to a Unity ScriptableObject type or to entries within one. String IDs are the universal key — every cross-file reference uses IDs, never asset paths or display names.

The CSV importer lives at `Mossmark/Data/Import from CSV` in the Unity menu.

---

## File Overview

| File | Maps to | Creates / updates |
|------|---------|-------------------|
| `data/items.csv` | `ItemDefinition` | One SO per row in `Assets/Game/Data/Items/` |
| `data/loot_tables.csv` | `LootTable` | One SO per unique `id` in `Assets/Game/Data/Combat/` |
| `data/upgrades.csv` | `UpgradeDefinition` / `TownUpgradeDefinition` + `UpgradePool` | One SO per row; one pool SO per unique `pool` value |
| `data/upgrade_dependencies.csv` | `UpgradeDependency` list on each `UpgradeDefinition` | Updates existing upgrade SOs |
| `data/upgrade_rewards.csv` | `WeightedRewardItem` list on each `UpgradeDefinition` | Updates existing upgrade SOs |
| `data/npc_pool.csv` | `NpcSpawnConfig` list on `BiomeData` | Replaces `NpcPool` on existing `BiomeData` SOs |
| `data/building_pool.csv` | `BuildingSpawnConfig` list on `BiomeData` | Replaces `BuildingPool` on existing `BiomeData` SOs |
| `data/item_pool.csv` | `ItemSpawnConfig` list on `BiomeData` | Replaces `ItemPool` on existing `BiomeData` SOs |
| `data/enemy_pool.csv` | `EnemySpawnConfig` list on `BiomeData` | Replaces `EnemyPool` on existing `BiomeData` SOs |
| `data/quest_item_pools.csv` | `WeightedQuestItem` list on `NpcSpawnConfig` / `BuildingSpawnConfig` in `BiomeData` | Updates `WeightedItems` on existing spawn config entries |
| `data/upgrade_pool_mutations.csv` | `QuestPoolMutation` list on `NpcSpawnConfig` / `BuildingSpawnConfig` / `BiomeData` (town hall) | Updates `PoolMutations` on existing spawn config entries |
| `data/town_hall_config.csv` | `TownUpgradePool`, `TownMinQuestSize`, `TownMaxQuestSize` on `BiomeData` | Updates town hall fields on existing `BiomeData` SOs |
| `data/encounter_locations.csv` | `EncounterLocationConfig` list on `BiomeData` | Replaces `EncounterLocations` on existing `BiomeData` SOs |
| `data/encounter_location_enemies.csv` | `EnemySpawnConfig` list on `EncounterLocationConfig` in `BiomeData` | Updates `EnemyPool` on existing encounter location entries |
| `data/wilderness_zones.csv` | `WildernessZoneConfig` list on `BiomeData` | Replaces `WildernessZones` on existing `BiomeData` SOs |
| `data/wilderness_zone_nodes.csv` | `TendedSpotConfig` and `StruckNodeConfig` lists on `WildernessZoneConfig` in `BiomeData` | Updates `TendedSpots` and `StruckNodes` on existing zone entries |

The importer processes files in the order shown. **`quest_item_pools.csv` must run after `npc_pool.csv` and `building_pool.csv`** because it looks up spawn config entries by display name. **`upgrade_pool_mutations.csv` must run after `npc_pool.csv`, `building_pool.csv`, and `upgrades.csv`** because it references both spawn config entries and upgrade assets. **`town_hall_config.csv` must run after `upgrades.csv`** because it references upgrade pool assets by ID. **`enemy_pool.csv` and `encounter_location_enemies.csv` must run after `loot_tables.csv`** because they reference loot table assets by ID. **`encounter_location_enemies.csv` must run after `encounter_locations.csv`** because it looks up locations by name. **`wilderness_zone_nodes.csv` must run after `wilderness_zones.csv`** because it looks up zones by name.

All files use standard CSV (comma-separated, UTF-8, first row is the header). Multi-value cells use semicolons as an inner separator.

---

## `items.csv`

One row per `ItemDefinition` asset.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `id` | string | ✅ | Unique, `snake_case`. Used as the primary key everywhere. Must match the `_id` field already set on existing assets. |
| `displayName` | string | ✅ | Shown in UI and quest panel. |
| `description` | string | | Flavour text. Leave blank if none. |
| `acquisitionTier` | `forage` \| `tended` \| `struck` \| `conversion` \| `encounter` | ✅ | How this item enters the world. `forage` — spawns as a free-standing pickup. `tended` — produced only by tended spot interactables. `struck` — produced only by struck node interactables. `conversion` — produced only by upgraded entity actions (e.g. Commission Work). `encounter` — drops only from combat encounters; never spawns in the wilderness. Used by the importer to validate quest pool authoring and by `QuestUIPanel` to show source hints for `encounter` items. |

See `data/items.csv` for current authored items.

---

## `loot_tables.csv`

One row per `LootEntry` within a loot table. Multiple rows sharing the same `id` populate a single `LootTable` SO. Each entry is rolled independently — all entries whose chance succeeds drop on a single kill.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `id` | string | ✅ | Unique, `snake_case`. All rows with the same `id` are collected into one `LootTable` SO at `Assets/Game/Data/Combat/LootTable_{id}.asset`. Referenced by `enemy_pool.csv`. |
| `itemId` | string | ✅ | References `items.csv` `id`. |
| `dropChance` | float | ✅ | 0.0 – 1.0. Independent per-entry probability of dropping on a roll. |
| `minQuantity` | int | ✅ | Minimum quantity dropped when this entry succeeds. |
| `maxQuantity` | int | ✅ | Maximum quantity dropped. Set equal to `minQuantity` for a fixed amount. |

**Example:**
```
id,itemId,dropChance,minQuantity,maxQuantity
goblin_loot,stick,0.8,1,2
goblin_loot,pebble,0.5,1,1
```

---

## `upgrades.csv`

One row per `UpgradeDefinition` (or `TownUpgradeDefinition`) asset. The `pool` column groups upgrades — the importer creates one `UpgradePool` SO per unique pool ID.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `id` | string | ✅ | Unique, `snake_case`. |
| `displayName` | string | ✅ | Shown in menus and upgrade info panel. |
| `description` | string | | One sentence describing what this upgrade represents. |
| `visualColor` | hex string | | e.g. `#FF8040`. Entity sprite tints to this color on unlock. Defaults to `#FFFFFF`. |
| `progressCost` | int | ✅ | 2–6. Progress points required to unlock this upgrade. Spent from the entity's `PendingProgress` when the player applies the upgrade in the interaction menu. |
| `pool` | string | ✅ | Pool ID (snake_case). All upgrades sharing a pool ID are grouped into one `UpgradePool` asset. |
| `type` | `standard` \| `town` | | Defaults to `standard`. Use `town` to create a `TownUpgradeDefinition`. |
| `townEffect` | `none` \| `encounterRateReduction` \| `itemSpawnBonus` \| `merchantBonus` \| `spawnNpc` | | Only meaningful when `type=town`. Defaults to `none`. |
| `spawnNpcDisplayName` | string | | Display name of the NPC to spawn. Only used when `townEffect=spawnNpc`. |
| `spawnNpcUpgradePool` | string | | Pool ID for the spawned NPC's upgrade tree. Only used when `townEffect=spawnNpc`. |
| `spawnNpcMinQuestSize` | int | | Minimum quest size for the spawned NPC. Defaults to `1`. |
| `spawnNpcMaxQuestSize` | int | | Maximum quest size for the spawned NPC. Defaults to `2`. |

Quest items for a spawned NPC are configured in `quest_item_pools.csv` with `entityType=spawnedNpc` and `entityKey` equal to the triggering upgrade's `id`.

---

## `upgrade_dependencies.csv`

One row per dependency entry. An upgrade with multiple dependencies gets multiple rows. All dependencies on an upgrade must be satisfied before it becomes available.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `upgradeId` | string | ✅ | References `upgrades.csv` `id`. |
| `type` | `single` \| `anyOf` | ✅ | `single` requires one specific upgrade. `anyOf` requires at least one from a list. |
| `scope` | `self` \| `town` | ✅ | `self` checks this entity's unlocked upgrades. `town` checks any entity registered with the town. |
| `required` | string | | Upgrade ID. Used when `type=single`. Leave blank otherwise. |
| `anyOfList` | string | | Semicolon-separated upgrade IDs. Used when `type=anyOf`. Leave blank otherwise. |

---

## `upgrade_rewards.csv`

One row per potential reward item per upgrade. At quest turn-in, the game aggregates reward entries from all of the giver's unlocked upgrades and weight-samples one item from that pool.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `upgradeId` | string | ✅ | References `upgrades.csv` `id`. |
| `itemId` | string | ✅ | References `items.csv` `id`. |
| `weight` | int | ✅ | Relative weight within the aggregated pool. Higher = more likely. |
| `minQuantity` | int | ✅ | Minimum quantity granted when this item is selected. |
| `maxQuantity` | int | ✅ | Maximum quantity granted. Set equal to `minQuantity` for a fixed amount. |

---

## `npc_pool.csv`

One row per `NpcSpawnConfig` entry per biome. Rows for the same biome are applied in order and replace the entire `NpcPool` array. The `WeightedItems` list on each entry is populated separately via `quest_item_pools.csv`.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name (e.g. `IronwoodReach`). |
| `displayName` | string | ✅ | Shown in-game and used as the lookup key in `quest_item_pools.csv`. |
| `upgradePool` | string | | Pool ID — references the `pool` column in `upgrades.csv`. Leave blank for no upgrade tree. |
| `minQuestSize` | int | ✅ | Minimum number of distinct items in a generated quest. |
| `maxQuestSize` | int | ✅ | Maximum number of distinct items in a generated quest. |
| `weight` | int | ✅ | Relative weight for NPC type selection during world generation. |
| `allowDuplicates` | bool | ✅ | `true` / `false`. When `false`, `TownGenerator` will spawn at most one NPC of this type per town regardless of weight. Use `false` for specialist roles (Hedge Witch, Itinerant Smith, etc.) where multiple instances would be thematically incoherent. Defaults to `true`. |

**Example:**
```
biomeId,displayName,upgradePool,minQuestSize,maxQuestSize,weight,allowDuplicates
IronwoodReach,Bog Keeper,bog_keeper_pool,1,2,3,true
IronwoodReach,Hedge Witch,hedge_witch_pool,1,2,1,false
IronwoodReach,Itinerant Smith,smith_pool,1,3,1,false
```

---

## `building_pool.csv`

One row per `BuildingSpawnConfig` entry per biome. Replaces the entire `BuildingPool` array for each biome. `WeightedItems` is populated via `quest_item_pools.csv`.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `displayName` | string | ✅ | Shown in-game and used as the lookup key in `quest_item_pools.csv`. |
| `upgradePool` | string | | Pool ID from `upgrades.csv`. Leave blank for no upgrade tree. |
| `minQuestSize` | int | ✅ | Minimum number of distinct items in a generated quest. |
| `maxQuestSize` | int | ✅ | Maximum number of distinct items in a generated quest. |
| `visualColor` | hex string | | Building sprite tint. Defaults to the brown placeholder `#8C6B52`. |
| `weight` | int | ✅ | Relative weight for building type selection during world generation. |
| `allowDuplicates` | bool | ✅ | `true` / `false`. When `false`, at most one building of this type spawns per town. Defaults to `true`. |

---

## `item_pool.csv`

One row per `ItemSpawnConfig` entry per biome. Replaces the entire `ItemPool` array for each biome. Only items with `acquisitionTier=forage` (set in `items.csv`) are spawned as free-standing world pickups by `TownGenerator`. Entries for `tended`, `struck`, `conversion`, and `encounter` tier items are valid here for data completeness (e.g. to set weight and quantity ranges used by other systems), but `TownGenerator` will not place them as pickups.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `itemId` | string | ✅ | References `items.csv` `id`. |
| `minCount` | int | ✅ | Minimum number of pickup instances spawned in the wilderness. Ignored for non-forage items. |
| `maxCount` | int | ✅ | Maximum number of pickup instances spawned. Ignored for non-forage items. |
| `quantityPerPickup` | int | ✅ | Items added to inventory per pickup interaction. |
| `weight` | int | ✅ | Relative weight for item type selection during world generation and merchant trade generation. |

**Forage pickups are not individually renewable.** All forage-tier pickups in the wilderness are cleared and re-seeded as a batch when the player rests (see [PROTOTYPE.md](PROTOTYPE.md) Section 12, Day Cycle System).

---

## `enemy_pool.csv`

One row per `EnemySpawnConfig` entry per biome. Replaces the entire `EnemyPool` array for each biome. Used for random wilderness encounters; encounter locations have their own enemy pools configured in `encounter_location_enemies.csv`.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `displayName` | string | ✅ | Shown during combat encounters. |
| `visualColor` | hex string | | Enemy sprite tint. Defaults to `#CC3333`. |
| `lootTableId` | string | | References `loot_tables.csv` `id`. Leave blank for no drops. |
| `weight` | int | ✅ | Relative weight for enemy type selection during encounters. |

---

## `quest_item_pools.csv`

One row per item that a given entity type can request in a generated quest. The `biomeId` + `entityType` + `entityKey` together identify which `NpcSpawnConfig` or `BuildingSpawnConfig` entry in a `BiomeData` asset to update.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `entityType` | `npc` \| `building` \| `townHall` \| `spawnedNpc` | ✅ | `townHall` ignores `entityKey`. `spawnedNpc` ignores `biomeId` and uses `entityKey` as the upgrade ID of a `TownUpgradeDefinition` with `townEffect=spawnNpc`. |
| `entityKey` | string | | Display name of the NPC/building entry (for `npc`/`building`), the upgrade ID (for `spawnedNpc`), or blank (for `townHall`). |
| `itemId` | string | ✅ | References `items.csv` `id`. |
| `weight` | int | ✅ | Relative weight for item selection within a generated quest. |
| `minQuantity` | int | ✅ | Minimum quantity required of this item. |
| `maxQuantity` | int | ✅ | Maximum quantity required. |

**Authoring note:** only include items in an entity's quest pool that can realistically be acquired in the same session. `encounter` tier items are acceptable since combat is always available. `conversion` tier items should only appear in the quest pool of the entity that produces them, or entities that have a guaranteed upgrade dependency path to that producer. Items that are impossible to acquire given the session's random NPC selection can always be abandoned — no schema enforcement is needed, but thoughtful authoring avoids unnecessary frustration in the early game.

---

## `upgrade_pool_mutations.csv`

One row per `QuestPoolMutation` entry on an entity type. Mutations shift what an entity requests (`questPool`) or rewards (`rewardPool`) when a specific upgrade is unlocked.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `entityType` | `npc` \| `building` \| `townHall` | ✅ | `townHall` ignores `entityKey`. |
| `entityKey` | string | | Display name of the NPC or building entry. |
| `upgradeId` | string | ✅ | References `upgrades.csv` `id`. This upgrade's unlock triggers the mutation. |
| `mutationType` | `addItem` \| `removeItem` \| `replaceItem` | ✅ | Operation to perform on the target pool. |
| `poolTarget` | `questPool` \| `rewardPool` | ✅ | Which pool to modify. |
| `itemToRemove` | string | | Item ID. Required for `removeItem` and `replaceItem`. |
| `itemToAdd` | string | | Item ID. Required for `addItem` and `replaceItem`. |
| `addWeight` | int | | Relative weight of the added item. Defaults to `1`. |
| `addMinQty` | int | | Minimum quantity for the added item. Defaults to `1`. |
| `addMaxQty` | int | | Maximum quantity for the added item. Defaults to `1`. |

**Mutation semantics:**
- `addItem`: adds a new item entry to the pool.
- `removeItem`: removes all entries matching `itemToRemove`.
- `replaceItem`: removes all entries matching `itemToRemove`, then adds the item from `itemToAdd` columns.

`questPool` mutations take effect on the next quest generated after the upgrade fires. `rewardPool` mutations are re-applied each time a quest is generated.

---

## `town_hall_config.csv`

One row per biome. Sets the town hall's upgrade pool and quest size range on the `BiomeData` asset.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `upgradePool` | string | ✅ | Pool ID from `upgrades.csv`. |
| `minQuestSize` | int | ✅ | Minimum number of distinct items the town hall requests per quest. |
| `maxQuestSize` | int | ✅ | Maximum number of distinct items per quest. |

---

## `encounter_locations.csv`

One row per `EncounterLocationConfig` entry per biome. Replaces the entire `EncounterLocations` array for each biome.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `locationName` | string | ✅ | Display name shown in the prompt and during interaction. Used as the lookup key in `encounter_location_enemies.csv`. |
| `zoneId` | string | | Zone name this location is associated with. Used by `TownGenerator` to position the location within the correct zone sub-region. Leave blank to place anywhere in the wilderness. Must match a `zoneName` in `wilderness_zones.csv` for the same biome. |
| `minEnemies` | int | ✅ | Minimum number of enemies spawned when the player triggers this location. |
| `maxEnemies` | int | ✅ | Maximum number of enemies spawned. |
| `cooldownDays` | int | ✅ | In-game days before the location can be triggered again after an encounter. |
| `unlockUpgradeId` | string | | Upgrade ID from upgrades.csv. If set, EncounterLocation is inaccessible until this upgrade is unlocked in the town. Leave blank for locations that are always approachable. |

---

## `encounter_location_enemies.csv`

One row per enemy type per encounter location.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `locationName` | string | ✅ | Must match a `locationName` in `encounter_locations.csv` for the same biome. |
| `displayName` | string | ✅ | Enemy display name shown during combat. |
| `visualColor` | hex string | | Enemy sprite tint. Defaults to `#CC3333`. |
| `lootTableId` | string | | References `loot_tables.csv` `id`. Leave blank for no drops. |
| `weight` | int | ✅ | Relative weight for enemy selection within this location's encounter. |

---

## `wilderness_zones.csv`

One row per `WildernessZoneConfig` entry per biome. Replaces the entire `WildernessZones` array for each biome. Zone nodes (tended spots and struck nodes) are configured separately via `wilderness_zone_nodes.csv`.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `zoneName` | string | ✅ | Display name shown on zone entry. Used as the lookup key in `wilderness_zone_nodes.csv` and `encounter_locations.csv`. |
| `zoneDescription` | string | ✅ | One-line character note shown in `ZoneEntryUI` on zone entry. e.g. "Iron seeps, crow roosts. The Wraith hunts here." |
| `groundTintColor` | hex string | | Ground plane tint color for this zone's sub-region. Should be visually distinct but subtle. Defaults to `#4A5C3A`. |
| `itemWeightOverrides` | string | | Semicolon-separated `itemId:multiplier` pairs. Multiplies the base spawn weight of the named item within this zone. e.g. `bog_iron:3;dried_herbs:0.5` triples bog iron's likelihood in this zone and halves dried herbs. Only applies to `forage` tier items placed as free-standing pickups. |

**Example:**
```
biomeId,zoneName,zoneDescription,groundTintColor,itemWeightOverrides
IronwoodReach,The Fen,Iron seeps and crow roosts. The Wraith hunts here.,#3A4A2E,bog_iron:3;crow_feather:2
IronwoodReach,Old Road,A crumbling imperial track. Bandits camp nearby.,#5A5040,dried_herbs:2;birch_bark:2
IronwoodReach,Deep Wood,Old growth. Quiet and strange.,#2A3A22,ravens_eye:2;beeswax:2
```

---

## `wilderness_zone_nodes.csv`

One row per tended spot or struck node per zone. The `biomeId` + `zoneName` tuple identifies the target zone. Must run after `wilderness_zones.csv`.

| Column | Type | Required | Notes |
|--------|------|----------|-------|
| `biomeId` | string | ✅ | Matches the `BiomeData` asset name. |
| `zoneName` | string | ✅ | Must match a `zoneName` in `wilderness_zones.csv` for the same biome. |
| `nodeType` | `tended` \| `struck` | ✅ | Which interactable type to spawn. |
| `nodeName` | string | ✅ | Display name shown on the interactable prompt. e.g. "Fen Iron Seep", "Old Flint Bank". |
| `itemId` | string | ✅ | References `items.csv` `id`. Must have `acquisitionTier=tended` or `struck` matching `nodeType`. |
| `spawnCount` | int | ✅ | Number of instances of this node spawned in the zone. |
| `markToHarvestSeconds` | float | | For `tended` nodes: real-time seconds after marking before harvest is available. Defaults to `120`. |
| `maxConcurrentMarked` | int | | For `tended` nodes: settlement-wide cap on simultaneously marked spots of this type. Prevents degenerate mark-everything-then-loop behavior. Defaults to `2`. |
| `holdDurationSeconds` | float | | For `struck` nodes: seconds the player must hold the interact key to extract. Defaults to `1.5`. |
| `minYield` | int | ✅ | Minimum quantity produced per harvest or extraction. |
| `maxYield` | int | ✅ | Maximum quantity produced. |
| `respawnSeconds` | float | ✅ | Seconds before the node resets and can be used again. For `tended` nodes this resets to the unmarked state; for `struck` nodes this resets to the extractable state. |

**Example:**
```
biomeId,zoneName,nodeType,nodeName,itemId,spawnCount,markToHarvestSeconds,maxConcurrentMarked,holdDurationSeconds,minYield,maxYield,respawnSeconds
IronwoodReach,The Fen,tended,Fen Iron Seep,bog_iron,2,180,2,,,2,4,300
IronwoodReach,The Fen,tended,Crow Roost,crow_feather,1,120,2,,,1,3,240
IronwoodReach,Old Road,struck,Old Flint Bank,flint,3,,,,1.5,1,2,600
IronwoodReach,Deep Wood,struck,Charcoal Deposit,charcoal,2,,,,2.0,1,3,480
```

---

## Notes for Claude

When asked to add or modify data, produce complete new rows for all affected files. Check consistency across files:

- A new **item** always needs a row in `items.csv`, including its `acquisitionTier`.
- A new **upgrade** always needs a row in `upgrades.csv`. If it has prerequisites, add rows to `upgrade_dependencies.csv`. If it unlocks rewards, add rows to `upgrade_rewards.csv`.
- A new **NPC type** needs a row in `npc_pool.csv` (with `allowDuplicates` set) and rows in `quest_item_pools.csv` for its quest items. Specialist roles should have `allowDuplicates=false`.
- A new **building type** needs a row in `building_pool.csv` (with `allowDuplicates` set) and rows in `quest_item_pools.csv` for its quest items.
- A new **enemy type** needs a row in `enemy_pool.csv`. If it drops items, add rows to `loot_tables.csv` first.
- A new **wilderness item spawn** (forage tier) needs a row in `item_pool.csv`; the item must exist in `items.csv` with `acquisitionTier=forage`.
- A new **tended or struck item** needs `acquisitionTier=tended` or `struck` in `items.csv` and a row in `wilderness_zone_nodes.csv` for each zone it appears in. Do not add tended/struck items to `item_pool.csv` as pickups.
- A new **conversion item** needs `acquisitionTier=conversion` in `items.csv`. It should not appear in `item_pool.csv`, `quest_item_pools.csv` (except for the entity that produces it), or `wilderness_zone_nodes.csv`.
- A new **encounter-exclusive item** needs `acquisitionTier=encounter` in `items.csv` and a row in `loot_tables.csv`. Do not add it to any forage or zone node config. When it appears in `quest_item_pools.csv`, `QuestUIPanel` will automatically show a source hint.
- A new **wilderness zone** needs a row in `wilderness_zones.csv`. If it has tended spots or struck nodes, add rows to `wilderness_zone_nodes.csv`. If an encounter location belongs to this zone, set `zoneId` on that row in `encounter_locations.csv`.
- A new **quest item** for an existing entity type only needs a row in `quest_item_pools.csv`.
- A **pool mutation** needs a row in `upgrade_pool_mutations.csv`. Both `itemToRemove` and `itemToAdd` must exist in `items.csv`.
- A new **encounter location** needs a row in `encounter_locations.csv` (with optional `zoneId`) and at least one row in `encounter_location_enemies.csv`.
- A **SpawnNpc town upgrade** needs `spawnNpcDisplayName`, `spawnNpcUpgradePool`, `spawnNpcMinQuestSize`, `spawnNpcMaxQuestSize` set in `upgrades.csv`, and rows in `quest_item_pools.csv` with `entityType=spawnedNpc`.
- The **town hall config** for a biome is set in `town_hall_config.csv`. Town hall quest items use `entityType=townHall` in `quest_item_pools.csv`.

Use semicolons as the inner separator in `anyOfList` and `itemWeightOverrides`. Use `#RRGGBB` hex format for colors. IDs are always `snake_case`. Descriptions are plain sentences, no markdown. Booleans are `true` / `false`.

When given the current CSV content as context, check existing IDs before generating new ones to avoid collisions.
