using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Mossmark.Inventory;
using Mossmark.World;
using UnityEditor;
using UnityEngine;

namespace Mossmark.Editor
{
    public static class ExportGameData
    {
        static string OutDir => Path.GetFullPath(
            Path.Combine(Application.dataPath, "../Tools/Data"));

        [MenuItem("Mossmark/Data/Export All")]
        public static void ExportAll()
        {
            Directory.CreateDirectory(OutDir);
            ExportYieldTables();
            ExportSpots();
            ExportNpcStages();
            ExportBuildingStages();
            ExportStageConditions();
            ExportArchetypes();
            ExportWandering();
            Debug.Log($"[ExportGameData] CSVs written to {OutDir}");
        }

        // ------------------------------------------------------------------ //
        // Yield Tables
        // ------------------------------------------------------------------ //

        static void ExportYieldTables()
        {
            // Folder is created lazily by import — absent until the first table is authored.
            var guids = AssetDatabase.IsValidFolder("Assets/Game/Data/World/YieldTables")
                ? AssetDatabase.FindAssets("t:YieldTable",
                    new[] { "Assets/Game/Data/World/YieldTables" })
                : System.Array.Empty<string>();

            var assets = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<YieldTable>(
                    AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

            var headers = new[] { "tableId", "item", "weight", "minQty", "maxQty" };
            var rows = new List<string[]>();
            foreach (var a in assets)
                foreach (var e in a.Entries ?? System.Array.Empty<ItemYield>())
                {
                    if (e?.Item == null) continue;
                    rows.Add(new[]
                    {
                        a.name, e.Item.DisplayName, Fmt(e.Weight),
                        e.MinQuantity.ToString(), e.MaxQuantity.ToString()
                    });
                }

            WriteCsv(Path.Combine(OutDir, "yield_tables.csv"), headers, rows);
            Debug.Log($"  Yield tables: {rows.Count} rows");
        }

        // ------------------------------------------------------------------ //
        // Wilderness Spots
        // ------------------------------------------------------------------ //

        static void ExportSpots()
        {
            var guids = AssetDatabase.FindAssets("t:WildernessSpotDefinition",
                new[] { "Assets/Game/Data/World/Spots" });

            var assets = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<WildernessSpotDefinition>(
                    AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

            int maxKnowledge = assets.Count > 0
                ? assets.Max(a => a.knowledgeYields?.Length ?? 0) : 0;

            var headers = new List<string>
            {
                "assetName", "spotId", "kind", "displayName",
                "color_r", "color_g", "color_b",
                "interactionVerb", "commonYields", "rareYields", "rareDropChance",
                "commonYieldTable", "rareYieldTable",
                "minTickInterval", "maxTickInterval",
                "tendVerb", "harvestYields", "restsToHarvest", "maxConcurrentMarked"
            };
            for (int i = 1; i <= maxKnowledge; i++)
                headers.AddRange(new[]
                    { $"knowledge{i}_flag", $"knowledge{i}_specializationId",
                      $"knowledge{i}_item",
                      $"knowledge{i}_minQty", $"knowledge{i}_maxQty", $"knowledge{i}_weight" });

            var rows = new List<string[]>();
            foreach (var a in assets)
            {
                var row = new List<string>
                {
                    a.name,
                    a.spotId,
                    a.kind == WildernessSpotDefinition.SpotKind.Tended ? "Tended" : "Generic",
                    a.displayName,
                    Fmt(a.color.r), Fmt(a.color.g), Fmt(a.color.b),
                    a.interactionVerb,
                    YieldsToCompact(a.commonYields),
                    YieldsToCompact(a.rareYields),
                    Fmt(a.rareDropChance),
                    a.commonYieldTable != null ? a.commonYieldTable.name : "",
                    a.rareYieldTable != null ? a.rareYieldTable.name : "",
                    Fmt(a.minTickInterval), Fmt(a.maxTickInterval),
                    a.tendVerb,
                    YieldsToCompact(a.harvestYields),
                    a.restsToHarvest.ToString(),
                    a.maxConcurrentMarked.ToString()
                };
                for (int i = 0; i < maxKnowledge; i++)
                {
                    if (i < (a.knowledgeYields?.Length ?? 0))
                    {
                        var e = a.knowledgeYields[i];
                        row.AddRange(new[]
                        {
                            e.requiredFlag, e.requiredSpecializationId,
                            e.item != null ? e.item.DisplayName : "",
                            e.minQty.ToString(), e.maxQty.ToString(),
                            Fmt(e.injectedWeight)
                        });
                    }
                    else row.AddRange(new[] { "", "", "", "", "", "" });
                }
                rows.Add(row.ToArray());
            }

            WriteCsv(Path.Combine(OutDir, "wilderness_spots.csv"), headers.ToArray(), rows);
            Debug.Log($"  Spots: {rows.Count} rows");
        }

        // ------------------------------------------------------------------ //
        // NPC / Building Stages + shared conditions
        // ------------------------------------------------------------------ //

        // Pool membership is recovered by walking pools rather than stages, so the
        // exported `pool` column round-trips exactly what import consumes. A stage in
        // no pool exports with an empty pool cell.
        //
        // Row order is pool order, NOT alphabetical: import rebuilds each pool's stages
        // array from row order, and pool order is semantic (development/track order;
        // building index 0 is the revival stage). Unpooled stages follow at the end.
        static void ExportNpcStages()
        {
            var pools = LoadAll<Development.NpcStagePool>("Assets/Game/Data/Development/Pools");
            var stages = OrderByPools(
                LoadAll<Development.NpcStageDef>("Assets/Game/Data/Development/NpcStages"),
                pools.Select(p => (IEnumerable<Development.NpcStageDef>)p.Stages));

            var poolOf = new Dictionary<Development.NpcStageDef, string>();
            foreach (var pool in pools)
                foreach (var s in pool.Stages ?? System.Array.Empty<Development.NpcStageDef>())
                    if (s != null && !poolOf.ContainsKey(s)) poolOf[s] = pool.name;

            var headers = new[]
            {
                "stageId", "displayName", "progressCost", "pool",
                "flavorText", "worldStateFlag", "passiveDriftSourceSpotId"
            };
            var rows = new List<string[]>();
            foreach (var s in stages)
                rows.Add(new[]
                {
                    s.StageId, s.DisplayName, s.ProgressCost.ToString(),
                    poolOf.TryGetValue(s, out var pool) ? pool : "",
                    s.FlavorText, s.WorldStateFlag, s.PassiveDriftSourceSpotId
                });

            WriteCsv(Path.Combine(OutDir, "npc_stages.csv"), headers, rows);
            Debug.Log($"  NPC stages: {rows.Count} rows");
        }

        static void ExportBuildingStages()
        {
            var pools = LoadAll<Development.BuildingStagePool>("Assets/Game/Data/Development/Pools");
            var stages = OrderByPools(
                LoadAll<Development.BuildingStageDef>("Assets/Game/Data/Development/BuildingStages"),
                pools.Select(p => (IEnumerable<Development.BuildingStageDef>)p.Stages));

            var poolOf = new Dictionary<Development.BuildingStageDef, string>();
            foreach (var pool in pools)
                foreach (var s in pool.Stages ?? System.Array.Empty<Development.BuildingStageDef>())
                    if (s != null && !poolOf.ContainsKey(s)) poolOf[s] = pool.name;

            var headers = new[]
            {
                "stageId", "displayName", "verb", "material", "costPerTick", "progressCost",
                "pool", "tint_r", "tint_g", "tint_b", "worldStateFlag"
            };
            var rows = new List<string[]>();
            foreach (var s in stages)
                rows.Add(new[]
                {
                    s.stageId, s.displayName, s.verb,
                    s.material != null ? s.material.DisplayName : "",
                    s.costPerTick.ToString(), s.progressCost.ToString(),
                    poolOf.TryGetValue(s, out var pool) ? pool : "",
                    Fmt(s.tint.r), Fmt(s.tint.g), Fmt(s.tint.b),
                    s.worldStateFlag
                });

            WriteCsv(Path.Combine(OutDir, "building_stages.csv"), headers, rows);
            Debug.Log($"  Building stages: {rows.Count} rows");
        }

        // One row per condition across all NPC and building stages — the P2 analogue of
        // P1's upgrade_dependencies.csv. Only the columns relevant to each row's
        // conditionType are populated; the mapping mirrors ConditionCsvImporter.BuildCondition.
        static void ExportStageConditions()
        {
            var headers = new[]
            {
                "stageId", "conditionType", "item", "quantity", "propertyId", "wantDescription",
                "flagId", "requiredValue", "needsDescription", "specializationId"
            };
            var rows = new List<string[]>();

            foreach (var s in LoadAll<Development.NpcStageDef>("Assets/Game/Data/Development/NpcStages"))
                AppendConditionRows(rows, s.StageId, s.Conditions);
            foreach (var s in LoadAll<Development.BuildingStageDef>("Assets/Game/Data/Development/BuildingStages"))
                AppendConditionRows(rows, s.stageId, s.conditions);

            WriteCsv(Path.Combine(OutDir, "stage_conditions.csv"), headers, rows);
            Debug.Log($"  Stage conditions: {rows.Count} rows");
        }

        static void AppendConditionRows(List<string[]> rows, string stageId,
            Development.IDependencyCondition[] conditions)
        {
            if (conditions == null) return;
            foreach (var c in conditions)
            {
                switch (c)
                {
                    case Development.ItemAvailableCondition item:
                        rows.Add(new[] { stageId, "item",
                            item.Item != null ? item.Item.DisplayName : "", item.Quantity.ToString(),
                            "", "", "", "", "", "" });
                        break;
                    case Development.PropertyAvailableCondition prop:
                        rows.Add(new[] { stageId, "property", "", "",
                            prop.PropertyId, prop.WantDescription, "", "", "", "" });
                        break;
                    case Development.WorldStateCondition flag:
                        rows.Add(new[] { stageId, "worldflag", "", "", "", "",
                            flag.FlagId, flag.RequiredValue ? "true" : "false",
                            flag.GetNeedsDescription(null), "" });
                        break;
                    case Development.SpecializationRealizedCondition spec:
                        rows.Add(new[] { stageId, "spec", "", "", "", "", "", "",
                            spec.GetNeedsDescription(null), spec.SpecializationId });
                        break;
                    case Development.TimeCondition time:
                        rows.Add(new[] { stageId, "time", "", time.RequiredProgress.ToString(),
                            "", "", "", "", "", "" });
                        break;
                    case null:
                        break;
                    default:
                        Debug.LogWarning($"[Export] stage '{stageId}': condition type " +
                            $"{c.GetType().Name} has no CSV mapping — row skipped.");
                        break;
                }
            }
        }

        static List<T> LoadAll<T>(string folder) where T : ScriptableObject
        {
            if (!AssetDatabase.IsValidFolder(folder)) return new List<T>();
            return AssetDatabase.FindAssets($"t:{typeof(T).Name}", new[] { folder })
                .Select(g => AssetDatabase.LoadAssetAtPath<T>(AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();
        }

        // Re-orders an alphabetical asset list so pooled members appear in pool order
        // (which import consumes as the pool's stages array), followed by any assets in
        // no pool.
        static List<T> OrderByPools<T>(List<T> all, IEnumerable<IEnumerable<T>> poolMemberships)
            where T : ScriptableObject
        {
            var ordered = new List<T>();
            var seen = new HashSet<T>();
            foreach (var members in poolMemberships)
                foreach (var s in members ?? Enumerable.Empty<T>())
                    if (s != null && seen.Add(s)) ordered.Add(s);
            foreach (var s in all)
                if (seen.Add(s)) ordered.Add(s);
            return ordered;
        }

        // ------------------------------------------------------------------ //
        // Place Archetypes
        // ------------------------------------------------------------------ //

        static void ExportArchetypes()
        {
            var guids = AssetDatabase.FindAssets("t:PlaceArchetype",
                new[] { "Assets/Game/Data/World/Archetypes" });

            var assets = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<PlaceArchetype>(
                    AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

            var headers = new List<string>
            {
                "assetName", "archetypeId", "displayName", "spots",
                "specializationId", "stageDisplayName", "npcTitle",
                "npcTint_r", "npcTint_g", "npcTint_b",
                "poiDisplayName", "poiLockedDescription", "poiVerb",
                "poiColor_r", "poiColor_g", "poiColor_b",
                "poiCommonYields", "poiRareYields", "poiRareDropChance",
                "poiMinTickInterval", "poiMaxTickInterval",
                "npcStagePool", "buildingStagePool", "npcMaintenanceMaterial",
                "buildingColdFlavor", "buildingMaintenanceCost",
                "npcColdFlavor", "npcMaintenanceCost",
                "buildingDilapidatedName",
                "buildingDilapidatedColor_r", "buildingDilapidatedColor_g", "buildingDilapidatedColor_b"
            };

            var rows = new List<string[]>();
            foreach (var a in assets)
            {
                var row = new List<string>
                {
                    a.name, a.ArchetypeId, a.DisplayName,
                    string.Join(";", (a.Spots ?? System.Array.Empty<WildernessSpotDefinition>())
                        .Where(s => s != null).Select(s => s.name)),
                    a.SpecializationId, a.StageDisplayName, a.NpcTitle,
                    Fmt(a.NpcTint.r), Fmt(a.NpcTint.g), Fmt(a.NpcTint.b),
                    a.PoiDisplayName, a.PoiLockedDescription, a.PoiVerb,
                    Fmt(a.PoiColor.r), Fmt(a.PoiColor.g), Fmt(a.PoiColor.b),
                    YieldsToCompact(a.PoiCommonYields), YieldsToCompact(a.PoiRareYields),
                    Fmt(a.PoiRareDropChance),
                    Fmt(a.PoiMinTickInterval), Fmt(a.PoiMaxTickInterval)
                };

                row.AddRange(new[]
                {
                    a.NpcStagePool != null ? a.NpcStagePool.name : "",
                    a.BuildingStagePool != null ? a.BuildingStagePool.name : "",
                    a.NpcMaintenanceMaterial != null ? a.NpcMaintenanceMaterial.DisplayName : "",
                    a.BuildingColdFlavor, a.BuildingMaintenanceCost.ToString(),
                    a.NpcColdFlavor, a.NpcMaintenanceCost.ToString()
                });

                var bc = a.BuildingDilapidatedColor;
                row.AddRange(new[] { a.BuildingDilapidatedName, Fmt(bc.r), Fmt(bc.g), Fmt(bc.b) });

                rows.Add(row.ToArray());
            }

            WriteCsv(Path.Combine(OutDir, "place_archetypes.csv"), headers.ToArray(), rows);
            Debug.Log($"  Archetypes: {rows.Count} rows");
        }

        // ------------------------------------------------------------------ //
        // Wandering Things
        // ------------------------------------------------------------------ //

        static void ExportWandering()
        {
            var guids = AssetDatabase.FindAssets("t:WanderingThingDefinition",
                new[] { "Assets/Game/Data/World/WanderingThings" });

            var assets = guids
                .Select(g => AssetDatabase.LoadAssetAtPath<WanderingThingDefinition>(
                    AssetDatabase.GUIDToAssetPath(g)))
                .Where(a => a != null).ToList();

            int maxMod = assets.Count > 0
                ? assets.Max(a => a.additionalModifiers?.Length ?? 0) : 0;

            var headers = new List<string>
            {
                "assetName", "displayName", "approachDescription", "attendVerb",
                "color_r", "color_g", "color_b", "colliderRadius",
                "goodYields", "goodFlavor", "badFlavor", "badDaylightCost", "baseGoodChance"
            };
            for (int i = 1; i <= maxMod; i++)
                headers.AddRange(new[] { $"mod{i}_flagId", $"mod{i}_multiplier" });

            var rows = new List<string[]>();
            foreach (var a in assets)
            {
                var row = new List<string>
                {
                    a.name, a.displayName, a.approachDescription, a.attendVerb,
                    Fmt(a.color.r), Fmt(a.color.g), Fmt(a.color.b), Fmt(a.colliderRadius),
                    YieldsToCompact(a.goodYields),
                    a.goodFlavor, a.badFlavor,
                    a.badDaylightCost.ToString(), Fmt(a.baseGoodChance)
                };
                for (int i = 0; i < maxMod; i++)
                {
                    if (i < (a.additionalModifiers?.Length ?? 0))
                    {
                        var m = a.additionalModifiers[i];
                        row.AddRange(new[] { m.flagId, Fmt(m.multiplier) });
                    }
                    else row.AddRange(new[] { "", "" });
                }
                rows.Add(row.ToArray());
            }

            WriteCsv(Path.Combine(OutDir, "wandering_things.csv"), headers.ToArray(), rows);
            Debug.Log($"  Wandering things: {rows.Count} rows");
        }

        // ------------------------------------------------------------------ //
        // Shared helpers
        // ------------------------------------------------------------------ //

        static string YieldsToCompact(ItemYield[] yields)
        {
            if (yields == null || yields.Length == 0) return "";
            return string.Join(",", yields
                .Where(y => y?.Item != null)
                .Select(y => $"{y.Item.DisplayName}:{Fmt(y.Weight)}:{y.MinQuantity}:{y.MaxQuantity}"));
        }

        static string Fmt(float f) =>
            f.ToString("G5", CultureInfo.InvariantCulture);

        static void WriteCsv(string path, IEnumerable<string> headers, List<string[]> rows)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Join(",", headers.Select(CsvCell)));
            foreach (var row in rows)
                sb.AppendLine(string.Join(",", row.Select(CsvCell)));
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
        }

        static string CsvCell(string s)
        {
            if (s == null) s = "";
            if (s.IndexOfAny(new[] { ',', '"', '\n' }) >= 0)
                return "\"" + s.Replace("\"", "\"\"") + "\"";
            return s;
        }
    }
}
