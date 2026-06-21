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
            ExportSpots();
            ExportArchetypes();
            ExportWandering();
            Debug.Log($"[ExportGameData] CSVs written to {OutDir}");
        }

        // ------------------------------------------------------------------ //
        // Wilderness Spots
        // ------------------------------------------------------------------ //

        static void ExportSpots()
        {
            var guids = AssetDatabase.FindAssets("t:WildernessSpotDefinition",
                new[] { "Assets/Game/Data/World/Spots" });

            var headers = new[]
            {
                "assetName", "kind", "displayName",
                "color_r", "color_g", "color_b",
                "interactionVerb", "commonYields", "rareYield", "rareDropChance",
                "minTickInterval", "maxTickInterval",
                "tendVerb", "harvestYields", "restsToHarvest", "maxConcurrentMarked"
            };

            var rows = new List<string[]>();
            foreach (var guid in guids)
            {
                var a = AssetDatabase.LoadAssetAtPath<WildernessSpotDefinition>(
                    AssetDatabase.GUIDToAssetPath(guid));
                if (a == null) continue;
                rows.Add(new[]
                {
                    a.name,
                    a.kind == WildernessSpotDefinition.SpotKind.Tended ? "Tended" : "Generic",
                    a.displayName,
                    Fmt(a.color.r), Fmt(a.color.g), Fmt(a.color.b),
                    a.interactionVerb,
                    YieldsToCompact(a.commonYields),
                    RareToCompact(a.rareYield),
                    Fmt(a.rareDropChance),
                    Fmt(a.minTickInterval), Fmt(a.maxTickInterval),
                    a.tendVerb,
                    YieldsToCompact(a.harvestYields),
                    a.restsToHarvest.ToString(),
                    a.maxConcurrentMarked.ToString()
                });
            }

            WriteCsv(Path.Combine(OutDir, "wilderness_spots.csv"), headers, rows);
            Debug.Log($"  Spots: {rows.Count} rows");
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

            int maxNpc = assets.Count > 0
                ? assets.Max(a => a.NpcPostSpecStages?.Length ?? 0) : 0;
            int maxB = assets.Count > 0
                ? assets.Max(a => a.BuildingStages?.Length ?? 0) : 0;

            var headers = new List<string>
            {
                "assetName", "archetypeId", "displayName",
                "spotDisplayName", "spotVerb",
                "spotColor_r", "spotColor_g", "spotColor_b",
                "commonYields", "rareYield", "spotRareDropChance",
                "spotMinTickInterval", "spotMaxTickInterval",
                "specializationId", "stageDisplayName", "npcTitle",
                "npcTint_r", "npcTint_g", "npcTint_b",
                "poiDisplayName", "poiLockedDescription", "poiVerb",
                "poiColor_r", "poiColor_g", "poiColor_b",
                "poiCommonYields", "poiRareYield", "poiRareDropChance"
            };
            for (int i = 1; i <= maxNpc; i++)
                headers.AddRange(new[]
                {
                    $"stage{i}_id", $"stage{i}_displayName", $"stage{i}_progressCost",
                    $"stage{i}_useRareItem", $"stage{i}_itemCount",
                    $"stage{i}_flavorText", $"stage{i}_worldStateFlag"
                });
            headers.AddRange(new[]
            {
                "buildingDilapidatedName",
                "buildingDilapidatedColor_r", "buildingDilapidatedColor_g", "buildingDilapidatedColor_b"
            });
            for (int i = 1; i <= maxB; i++)
                headers.AddRange(new[]
                {
                    $"bStage{i}_displayName", $"bStage{i}_verb", $"bStage{i}_material",
                    $"bStage{i}_costPerTick", $"bStage{i}_progressCost",
                    $"bStage{i}_requiredSpecialization",
                    $"bStage{i}_tint_r", $"bStage{i}_tint_g", $"bStage{i}_tint_b"
                });

            var rows = new List<string[]>();
            foreach (var a in assets)
            {
                var row = new List<string>
                {
                    a.name, a.ArchetypeId, a.DisplayName,
                    a.SpotDisplayName, a.SpotVerb,
                    Fmt(a.SpotColor.r), Fmt(a.SpotColor.g), Fmt(a.SpotColor.b),
                    YieldsToCompact(a.CommonYields), RareToCompact(a.RareYield),
                    Fmt(a.RareDropChance),
                    Fmt(a.ArchetypeSpotMinTickInterval), Fmt(a.ArchetypeSpotMaxTickInterval),
                    a.SpecializationId, a.StageDisplayName, a.NpcTitle,
                    Fmt(a.NpcTint.r), Fmt(a.NpcTint.g), Fmt(a.NpcTint.b),
                    a.PoiDisplayName, a.PoiLockedDescription, a.PoiVerb,
                    Fmt(a.PoiColor.r), Fmt(a.PoiColor.g), Fmt(a.PoiColor.b),
                    YieldsToCompact(a.PoiCommonYields), RareToCompact(a.PoiRareYield),
                    Fmt(a.PoiRareDropChance)
                };

                for (int i = 0; i < maxNpc; i++)
                {
                    if (i < (a.NpcPostSpecStages?.Length ?? 0))
                    {
                        var s = a.NpcPostSpecStages[i];
                        row.AddRange(new[]
                        {
                            s.stageId, s.displayName, s.progressCost.ToString(),
                            s.useRareItem ? "true" : "false", s.itemCount.ToString(),
                            s.flavorText, s.worldStateFlag
                        });
                    }
                    else row.AddRange(new[] { "", "", "", "", "", "", "" });
                }

                var bc = a.BuildingDilapidatedColor;
                row.AddRange(new[] { a.BuildingDilapidatedName, Fmt(bc.r), Fmt(bc.g), Fmt(bc.b) });

                for (int i = 0; i < maxB; i++)
                {
                    if (i < (a.BuildingStages?.Length ?? 0))
                    {
                        var s = a.BuildingStages[i];
                        row.AddRange(new[]
                        {
                            s.displayName, s.verb,
                            s.material != null ? s.material.DisplayName : "",
                            s.costPerTick.ToString(), s.progressCost.ToString(),
                            s.requiredSpecialization,
                            Fmt(s.tint.r), Fmt(s.tint.g), Fmt(s.tint.b)
                        });
                    }
                    else row.AddRange(new[] { "", "", "", "", "", "", "", "", "" });
                }

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

        static string RareToCompact(ItemYield rare)
        {
            if (rare?.Item == null) return "";
            return $"{rare.Item.DisplayName}:1:{rare.MinQuantity}:{rare.MaxQuantity}";
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
