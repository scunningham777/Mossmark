using System;
using System.Collections.Generic;
using System.IO;
using Mossmark.Inventory;
using Mossmark.World;
using UnityEditor;
using UnityEngine;
using static Mossmark.Editor.CsvUtil;

namespace Mossmark.Editor
{
    public static class ImportGameData
    {
        static string DataDir => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../Tools/Data"));

        [MenuItem("Mossmark/Data/Import All")]
        public static void ImportAll()
        {
            var items = BuildItemDb();

            // stage_conditions.csv is shared by NPC and building stages — one row per
            // condition, keyed by stageId (P1's upgrade_dependencies.csv pattern).
            var stageConditions = ConditionCsvImporter.ReadByStage(
                Path.Combine(DataDir, "stage_conditions.csv"), items);

            int changed = 0;
            changed += ImportYieldTables(items);
            // Spot stages import before spots — spots reference pools by name (Iteration 44).
            changed += ImportSpotStages(items, stageConditions);
            changed += ImportSpots(items);
            changed += ImportNpcStages(items, stageConditions);
            changed += ImportBuildingStages(items, stageConditions);
            changed += ImportArchetypes(items);
            changed += ImportWandering(items);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ImportGameData] Done — {changed} asset(s) updated.");
        }

        // ------------------------------------------------------------------ //
        // NPC Stages + Pools
        // ------------------------------------------------------------------ //

        // One row per NpcStageDef; the `pool` column groups stages into NpcStagePool
        // assets (P1's upgrades.csv `pool` pattern). Stage and pool assets are created
        // on demand — new content is new rows, never Editor clicks.
        static int ImportNpcStages(Dictionary<string, ItemDefinition> items,
            Dictionary<string, List<Development.IDependencyCondition>> stageConditions)
        {
            var rows = ReadCsv(Path.Combine(DataDir, "npc_stages.csv"));
            if (rows == null) return 0;
            int changed = 0;

            var poolMembers = new Dictionary<string, List<Development.NpcStageDef>>(StringComparer.Ordinal);
            var poolOrder = new List<string>();

            foreach (var row in rows)
            {
                string id = row.Get("stageId");
                if (string.IsNullOrEmpty(id)) continue;

                var asset = LoadOrCreate<Development.NpcStageDef>(
                    $"Assets/Game/Data/Development/NpcStages/{id}.asset");
                var so = new SerializedObject(asset);
                so.FindProperty("stageId").stringValue      = id;
                so.FindProperty("displayName").stringValue  = row.Get("displayName");
                so.FindProperty("progressCost").intValue    = I(row.Get("progressCost", "6"));
                so.FindProperty("flavorText").stringValue   = row.Get("flavorText");
                so.FindProperty("worldStateFlag").stringValue = row.Get("worldStateFlag");
                so.FindProperty("passiveDriftSourceSpotId").stringValue =
                    row.Get("passiveDriftSourceSpotId");
                ConditionCsvImporter.AssignConditions(so.FindProperty("conditions"),
                    stageConditions.TryGetValue(id, out var conds) ? conds : null);
                if (so.ApplyModifiedProperties()) changed++;

                string pool = row.Get("pool");
                if (!string.IsNullOrEmpty(pool))
                {
                    if (!poolMembers.TryGetValue(pool, out var list))
                    { poolMembers[pool] = list = new List<Development.NpcStageDef>(); poolOrder.Add(pool); }
                    list.Add(asset);
                }
            }

            foreach (var pool in poolOrder)
                if (UpdatePool<Development.NpcStagePool, Development.NpcStageDef>(pool, poolMembers[pool]))
                    changed++;

            Debug.Log($"  NPC stages: {rows.Count} rows, {poolOrder.Count} pools");
            return changed;
        }

        // ------------------------------------------------------------------ //
        // Building Stages + Pools
        // ------------------------------------------------------------------ //

        static int ImportBuildingStages(Dictionary<string, ItemDefinition> items,
            Dictionary<string, List<Development.IDependencyCondition>> stageConditions)
        {
            var rows = ReadCsv(Path.Combine(DataDir, "building_stages.csv"));
            if (rows == null) return 0;
            int changed = 0;

            var poolMembers = new Dictionary<string, List<Development.BuildingStageDef>>(StringComparer.Ordinal);
            var poolOrder = new List<string>();

            foreach (var row in rows)
            {
                string id = row.Get("stageId");
                if (string.IsNullOrEmpty(id)) continue;

                var asset = LoadOrCreate<Development.BuildingStageDef>(
                    $"Assets/Game/Data/Development/BuildingStages/{id}.asset");
                var so = new SerializedObject(asset);
                items.TryGetValue(row.Get("material"), out var mat);
                so.FindProperty("stageId").stringValue      = id;
                so.FindProperty("displayName").stringValue  = row.Get("displayName");
                so.FindProperty("verb").stringValue         = row.Get("verb");
                so.FindProperty("material").objectReferenceValue = mat;
                so.FindProperty("costPerTick").intValue     = I(row.Get("costPerTick", "2"));
                so.FindProperty("progressCost").intValue    = I(row.Get("progressCost", "6"));
                SetColor(so.FindProperty("tint"), row, "tint");
                so.FindProperty("worldStateFlag").stringValue = row.Get("worldStateFlag");
                so.FindProperty("stationName").stringValue = row.Get("stationName");
                var biasProp = so.FindProperty("biasPropertyIds");
                var biasIds = row.Get("biasPropertyIds")
                    .Split(';', StringSplitOptions.RemoveEmptyEntries);
                biasProp.arraySize = biasIds.Length;
                for (int i = 0; i < biasIds.Length; i++)
                    biasProp.GetArrayElementAtIndex(i).stringValue = biasIds[i].Trim();
                ConditionCsvImporter.AssignConditions(so.FindProperty("conditions"),
                    stageConditions.TryGetValue(id, out var conds) ? conds : null);
                if (so.ApplyModifiedProperties()) changed++;

                string pool = row.Get("pool");
                if (!string.IsNullOrEmpty(pool))
                {
                    if (!poolMembers.TryGetValue(pool, out var list))
                    { poolMembers[pool] = list = new List<Development.BuildingStageDef>(); poolOrder.Add(pool); }
                    list.Add(asset);
                }
            }

            foreach (var pool in poolOrder)
                if (UpdatePool<Development.BuildingStagePool, Development.BuildingStageDef>(pool, poolMembers[pool]))
                    changed++;

            Debug.Log($"  Building stages: {rows.Count} rows, {poolOrder.Count} pools");
            return changed;
        }

        // Creates/updates a pool asset (its `stages` array of SO references) under
        // Assets/Game/Data/Development/Pools/. Returns true if the asset changed.
        static bool UpdatePool<TPool, TStage>(string poolName, List<TStage> members)
            where TPool : ScriptableObject where TStage : ScriptableObject
        {
            var asset = LoadOrCreate<TPool>($"Assets/Game/Data/Development/Pools/{poolName}.asset");
            var so = new SerializedObject(asset);
            var prop = so.FindProperty("stages");
            prop.arraySize = members.Count;
            for (int i = 0; i < members.Count; i++)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = members[i];
            return so.ApplyModifiedProperties();
        }

        // ------------------------------------------------------------------ //
        // Yield Tables
        // ------------------------------------------------------------------ //

        // One row per entry, grouped by tableId (P1 loot_tables.csv pattern). Table assets
        // are created on demand — a new pool is a new CSV row, never an Editor click.
        static int ImportYieldTables(Dictionary<string, ItemDefinition> items)
        {
            var rows = ReadCsv(Path.Combine(DataDir, "yield_tables.csv"));
            if (rows == null) return 0;

            var byTable = new Dictionary<string, List<ItemYield>>(StringComparer.Ordinal);
            var order = new List<string>();
            foreach (var row in rows)
            {
                string id = row.Get("tableId");
                if (string.IsNullOrEmpty(id)) continue;
                if (!items.TryGetValue(row.Get("item"), out var item))
                { Debug.LogWarning($"[Import] yield_tables: unknown item '{row.Get("item")}'"); continue; }
                if (!byTable.TryGetValue(id, out var list))
                { byTable[id] = list = new List<ItemYield>(); order.Add(id); }
                list.Add(new ItemYield
                {
                    Item = item,
                    Weight = F(row.Get("weight", "1")),
                    MinQuantity = I(row.Get("minQty", "1")),
                    MaxQuantity = I(row.Get("maxQty", "1"))
                });
            }

            int changed = 0;
            foreach (var id in order)
            {
                var asset = LoadOrCreate<YieldTable>($"Assets/Game/Data/World/YieldTables/{id}.asset");
                var so = new SerializedObject(asset);
                SetYields(so.FindProperty("entries"), byTable[id].ToArray());
                if (so.ApplyModifiedProperties()) changed++;
            }

            Debug.Log($"  Yield tables: {changed}/{order.Count} updated");
            return changed;
        }

        // ------------------------------------------------------------------ //
        // Spot Stages + Pools (Iteration 44 — generalized from Iteration 43's Fen Bog pilot)
        // ------------------------------------------------------------------ //

        // One row per SpotStageDef; the `pool` column groups stages into SpotStagePool
        // assets, same shape as ImportNpcStages/ImportBuildingStages. Every Generic spot
        // has exactly one stage (Familiar) in its own single-member pool today, but the
        // pool grouping is generic in case a spot ever earns a second Standing stage.
        static int ImportSpotStages(Dictionary<string, ItemDefinition> items,
            Dictionary<string, List<Development.IDependencyCondition>> stageConditions)
        {
            var rows = ReadCsv(Path.Combine(DataDir, "spot_stages.csv"));
            if (rows == null) return 0;
            int changed = 0;

            var poolMembers = new Dictionary<string, List<SpotStageDef>>(StringComparer.Ordinal);
            var poolOrder = new List<string>();

            foreach (var row in rows)
            {
                string id = row.Get("stageId");
                if (string.IsNullOrEmpty(id)) continue;

                var asset = LoadOrCreate<SpotStageDef>(
                    $"Assets/Game/Data/Development/SpotStages/{id}.asset");
                var so = new SerializedObject(asset);
                so.FindProperty("stageId").stringValue      = id;
                so.FindProperty("displayName").stringValue  = row.Get("displayName");
                so.FindProperty("progressCost").intValue    = I(row.Get("progressCost", "1"));
                so.FindProperty("flavorText").stringValue   = row.Get("flavorText");
                so.FindProperty("rareChanceMultiplier").floatValue = F(row.Get("rareChanceMultiplier", "1"));
                SetColor(so.FindProperty("tint"), row, "tint");
                ConditionCsvImporter.AssignConditions(so.FindProperty("conditions"),
                    stageConditions.TryGetValue(id, out var conds) ? conds : null);
                if (so.ApplyModifiedProperties()) changed++;

                string pool = row.Get("pool");
                if (!string.IsNullOrEmpty(pool))
                {
                    if (!poolMembers.TryGetValue(pool, out var list))
                    { poolMembers[pool] = list = new List<SpotStageDef>(); poolOrder.Add(pool); }
                    list.Add(asset);
                }
            }

            foreach (var pool in poolOrder)
                if (UpdatePool<SpotStagePool, SpotStageDef>(pool, poolMembers[pool]))
                    changed++;

            Debug.Log($"  Spot stages: {rows.Count} rows, {poolOrder.Count} pools");
            return changed;
        }

        // ------------------------------------------------------------------ //
        // Wilderness Spots
        // ------------------------------------------------------------------ //

        static int ImportSpots(Dictionary<string, ItemDefinition> items)
        {
            var rows = ReadCsv(Path.Combine(DataDir, "wilderness_spots.csv"));
            if (rows == null) return 0;
            int changed = 0;

            foreach (var row in rows)
            {
                var asset = LoadOrCreate<WildernessSpotDefinition>(
                    $"Assets/Game/Data/World/Spots/{row.Get("assetName")}.asset");

                var so = new SerializedObject(asset);
                so.FindProperty("kind").intValue =
                    row.Get("kind") == "Tended" ? 1 : 0;
                so.FindProperty("spotId").stringValue        = row.Get("spotId");
                so.FindProperty("displayName").stringValue   = row.Get("displayName");
                SetColor(so.FindProperty("color"), row, "color");
                so.FindProperty("interactionVerb").stringValue = row.Get("interactionVerb", "forage");
                SetYields(so.FindProperty("commonYields"),
                    ParseYields(row.Get("commonYields"), items));
                SetYields(so.FindProperty("rareYields"),
                    ParseYields(row.Get("rareYields"), items));
                so.FindProperty("rareDropChance").floatValue  = F(row.Get("rareDropChance", "0.08"));
                so.FindProperty("rareYieldTable").objectReferenceValue =
                    LoadDataAsset<YieldTable>(row.Get("rareYieldTable"), "World/YieldTables");
                so.FindProperty("minTickInterval").floatValue = F(row.Get("minTickInterval", "1.5"));
                so.FindProperty("maxTickInterval").floatValue = F(row.Get("maxTickInterval", "2"));
                so.FindProperty("tendVerb").stringValue        = row.Get("tendVerb", "tend");
                SetYields(so.FindProperty("harvestYields"),
                    ParseYields(row.Get("harvestYields"), items));
                so.FindProperty("restsToHarvest").intValue     = I(row.Get("restsToHarvest", "1"));
                so.FindProperty("maxConcurrentMarked").intValue = I(row.Get("maxConcurrentMarked", "2"));
                so.FindProperty("spotStagePool").objectReferenceValue =
                    LoadDataAsset<SpotStagePool>(row.Get("spotStagePool"), "Development/Pools");

                // Knowledge yield entries (Iteration 28; requiredSpecializationId added Iteration 34)
                var kEntries = new List<(string flag, string specId, ItemDefinition item, int minQ, int maxQ, float weight)>();
                for (int i = 1; ; i++)
                {
                    string flag   = row.Get($"knowledge{i}_flag");
                    string specId = row.Get($"knowledge{i}_specializationId");
                    if (string.IsNullOrEmpty(flag) && string.IsNullOrEmpty(specId)) break;
                    items.TryGetValue(row.Get($"knowledge{i}_item"), out var kItem);
                    kEntries.Add((
                        flag, specId, kItem,
                        I(row.Get($"knowledge{i}_minQty", "1")),
                        I(row.Get($"knowledge{i}_maxQty", "2")),
                        F(row.Get($"knowledge{i}_weight", "0.15"))
                    ));
                }
                var kProp = so.FindProperty("knowledgeYields");
                kProp.arraySize = kEntries.Count;
                for (int i = 0; i < kEntries.Count; i++)
                {
                    var e = kProp.GetArrayElementAtIndex(i);
                    var (flag, specId, kItem, minQ, maxQ, weight) = kEntries[i];
                    e.FindPropertyRelative("requiredFlag").stringValue             = flag;
                    e.FindPropertyRelative("requiredSpecializationId").stringValue = specId;
                    e.FindPropertyRelative("item").objectReferenceValue            = kItem;
                    e.FindPropertyRelative("minQty").intValue                      = minQ;
                    e.FindPropertyRelative("maxQty").intValue                      = maxQ;
                    e.FindPropertyRelative("injectedWeight").floatValue            = weight;
                }

                if (so.ApplyModifiedProperties()) changed++;
            }

            Debug.Log($"  Spots: {changed}/{rows.Count} updated");
            return changed;
        }

        // ------------------------------------------------------------------ //
        // Place Archetypes
        // ------------------------------------------------------------------ //

        static int ImportArchetypes(Dictionary<string, ItemDefinition> items)
        {
            var rows = ReadCsv(Path.Combine(DataDir, "place_archetypes.csv"));
            if (rows == null) return 0;
            int changed = 0;

            foreach (var row in rows)
            {
                var path = $"Assets/Game/Data/World/Archetypes/{row.Get("assetName")}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<PlaceArchetype>(path);
                if (asset == null) { Debug.LogWarning($"[Import] Not found: {path}"); continue; }

                var so = new SerializedObject(asset);

                so.FindProperty("archetypeId").stringValue   = row.Get("archetypeId");
                so.FindProperty("displayName").stringValue   = row.Get("displayName");

                // Spot data is relational — spots live in wilderness_spots.csv; the
                // archetype row references them by asset name (semicolon-separated list).
                var spotsProp = so.FindProperty("spots");
                var spotNames = row.Get("spots").Split(';', StringSplitOptions.RemoveEmptyEntries);
                spotsProp.arraySize = spotNames.Length;
                for (int i = 0; i < spotNames.Length; i++)
                    spotsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                        LoadDataAsset<WildernessSpotDefinition>(spotNames[i].Trim(), "World/Spots");

                so.FindProperty("specializationId").stringValue  = row.Get("specializationId");
                so.FindProperty("stageDisplayName").stringValue  = row.Get("stageDisplayName");
                so.FindProperty("npcTitle").stringValue          = row.Get("npcTitle");
                SetColor(so.FindProperty("npcTint"), row, "npcTint");
                so.FindProperty("poiDisplayName").stringValue    = row.Get("poiDisplayName");
                so.FindProperty("poiLockedDescription").stringValue = row.Get("poiLockedDescription");
                so.FindProperty("poiVerb").stringValue           = row.Get("poiVerb", "search");
                SetColor(so.FindProperty("poiColor"), row, "poiColor");
                SetYields(so.FindProperty("poiCommonYields"),
                    ParseYields(row.Get("poiCommonYields"), items));
                SetYields(so.FindProperty("poiRareYields"),
                    ParseYields(row.Get("poiRareYields"), items));
                so.FindProperty("poiRareDropChance").floatValue = F(row.Get("poiRareDropChance", "0.05"));
                so.FindProperty("poiMinTickInterval").floatValue = F(row.Get("poiMinTickInterval", "2"));
                so.FindProperty("poiMaxTickInterval").floatValue = F(row.Get("poiMaxTickInterval", "2.5"));

                // Stage pools are relational now — stages live in npc_stages.csv /
                // building_stages.csv; the archetype row just references pools by name.
                so.FindProperty("npcStagePool").objectReferenceValue =
                    LoadDataAsset<Development.NpcStagePool>(row.Get("npcStagePool"), "Development/Pools");

                // Maintenance fields (Iteration 29; explicit material ref added in the
                // relational-data migration, replacing the CommonYields[0] positional lookup)
                so.FindProperty("buildingColdFlavor").stringValue   = row.Get("buildingColdFlavor");
                so.FindProperty("buildingMaintenanceCost").intValue = I(row.Get("buildingMaintenanceCost", "2"));
                so.FindProperty("npcColdFlavor").stringValue        = row.Get("npcColdFlavor");
                so.FindProperty("npcMaintenanceCost").intValue      = I(row.Get("npcMaintenanceCost", "1"));
                items.TryGetValue(row.Get("npcMaintenanceMaterial"), out var maintMat);
                so.FindProperty("npcMaintenanceMaterial").objectReferenceValue = maintMat;

                so.FindProperty("buildingDilapidatedName").stringValue = row.Get("buildingDilapidatedName");
                SetColor(so.FindProperty("buildingDilapidatedColor"), row, "buildingDilapidatedColor");
                so.FindProperty("buildingStagePool").objectReferenceValue =
                    LoadDataAsset<Development.BuildingStagePool>(row.Get("buildingStagePool"), "Development/Pools");

                if (so.ApplyModifiedProperties()) changed++;
            }

            Debug.Log($"  Archetypes: {changed}/{rows.Count} updated");
            return changed;
        }

        // ------------------------------------------------------------------ //
        // Wandering Things
        // ------------------------------------------------------------------ //

        static int ImportWandering(Dictionary<string, ItemDefinition> items)
        {
            var rows = ReadCsv(Path.Combine(DataDir, "wandering_things.csv"));
            if (rows == null) return 0;
            int changed = 0;

            foreach (var row in rows)
            {
                var path = $"Assets/Game/Data/World/WanderingThings/{row.Get("assetName")}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<WanderingThingDefinition>(path);
                if (asset == null) { Debug.LogWarning($"[Import] Not found: {path}"); continue; }

                var so = new SerializedObject(asset);
                so.FindProperty("displayName").stringValue        = row.Get("displayName");
                so.FindProperty("approachDescription").stringValue = row.Get("approachDescription");
                so.FindProperty("attendVerb").stringValue         = row.Get("attendVerb", "approach");
                SetColor(so.FindProperty("color"), row, "color");
                so.FindProperty("colliderRadius").floatValue      = F(row.Get("colliderRadius", "0.5"));
                SetYields(so.FindProperty("goodYields"),
                    ParseYields(row.Get("goodYields"), items));
                so.FindProperty("goodFlavor").stringValue         = row.Get("goodFlavor");
                so.FindProperty("badFlavor").stringValue          = row.Get("badFlavor");
                so.FindProperty("badDaylightCost").intValue       = I(row.Get("badDaylightCost", "1"));
                so.FindProperty("baseGoodChance").floatValue      = F(row.Get("baseGoodChance", "0.5"));

                var modsProp = so.FindProperty("additionalModifiers");
                var mods = new List<(string flag, float mult)>();
                for (int i = 1; ; i++)
                {
                    string flag = row.Get($"mod{i}_flagId");
                    if (string.IsNullOrEmpty(flag)) break;
                    mods.Add((flag, F(row.Get($"mod{i}_multiplier", "1"))));
                }
                modsProp.arraySize = mods.Count;
                for (int i = 0; i < mods.Count; i++)
                {
                    var e = modsProp.GetArrayElementAtIndex(i);
                    e.FindPropertyRelative("flagId").stringValue     = mods[i].flag;
                    e.FindPropertyRelative("multiplier").floatValue  = mods[i].mult;
                }

                if (so.ApplyModifiedProperties()) changed++;
            }

            Debug.Log($"  Wandering things: {changed}/{rows.Count} updated");
            return changed;
        }

        // ------------------------------------------------------------------ //
        // Shared helpers
        // ------------------------------------------------------------------ //

        // Resolves an asset-name cell to an asset reference under Assets/Game/Data/{folder}/.
        // Empty cell = null (a legitimate "no reference"); a non-empty cell that doesn't
        // resolve warns loudly, since silently nulling a reference is how data quietly breaks.
        static T LoadDataAsset<T>(string assetName, string folder) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(assetName)) return null;
            var asset = AssetDatabase.LoadAssetAtPath<T>($"Assets/Game/Data/{folder}/{assetName}.asset");
            if (asset == null)
                Debug.LogWarning($"[Import] Missing {typeof(T).Name}: {folder}/{assetName}");
            return asset;
        }

        // Creates the asset (and any missing folders) when it doesn't exist yet — the
        // P1 importer's "one SO per row / per unique id" behavior. Existing assets are
        // loaded and updated in place so GUIDs (and every reference to them) are stable.
        static T LoadOrCreate<T>(string path) where T : ScriptableObject
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null) return asset;

            EnsureFolder(Path.GetDirectoryName(path)!.Replace('\\', '/'));
            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            Debug.Log($"  Created {path}");
            return asset;
        }

        static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = Path.GetDirectoryName(folder)!.Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        static void SetYields(SerializedProperty prop, ItemYield[] yields)
        {
            prop.arraySize = yields.Length;
            for (int i = 0; i < yields.Length; i++)
            {
                var e = prop.GetArrayElementAtIndex(i);
                e.FindPropertyRelative("Item").objectReferenceValue = yields[i].Item;
                e.FindPropertyRelative("MinQuantity").intValue      = yields[i].MinQuantity;
                e.FindPropertyRelative("MaxQuantity").intValue      = yields[i].MaxQuantity;
                e.FindPropertyRelative("Weight").floatValue         = yields[i].Weight;
            }
        }

        static void SetColor(SerializedProperty prop, Dictionary<string, string> row, string prefix)
        {
            prop.colorValue = new Color(
                F(row.Get($"{prefix}_r", "1")),
                F(row.Get($"{prefix}_g", "1")),
                F(row.Get($"{prefix}_b", "1")),
                1f);
        }

        static ItemYield[] ParseYields(string compact, Dictionary<string, ItemDefinition> items)
        {
            if (string.IsNullOrWhiteSpace(compact)) return Array.Empty<ItemYield>();
            var result = new List<ItemYield>();
            foreach (var part in compact.Split(','))
            {
                var p = part.Trim().Split(':');
                if (p.Length < 4) continue;
                if (!items.TryGetValue(p[0].Trim(), out var item))
                { Debug.LogWarning($"[Import] Unknown item: '{p[0]}'"); continue; }
                result.Add(new ItemYield
                {
                    Item = item,
                    Weight = F(p[1]),
                    MinQuantity = I(p[2]),
                    MaxQuantity = I(p[3])
                });
            }
            return result.ToArray();
        }

    }
}
