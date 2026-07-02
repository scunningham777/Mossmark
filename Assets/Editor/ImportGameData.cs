using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Mossmark.Inventory;
using Mossmark.World;
using UnityEditor;
using UnityEngine;

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
            int changed = 0;
            changed += ImportSpots(items);
            changed += ImportArchetypes(items);
            changed += ImportWandering(items);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[ImportGameData] Done — {changed} asset(s) updated.");
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
                var path = $"Assets/Game/Data/World/Spots/{row.Get("assetName")}.asset";
                var asset = AssetDatabase.LoadAssetAtPath<WildernessSpotDefinition>(path);
                if (asset == null) { Debug.LogWarning($"[Import] Not found: {path}"); continue; }

                var so = new SerializedObject(asset);
                so.FindProperty("kind").intValue =
                    row.Get("kind") == "Tended" ? 1 : 0;
                so.FindProperty("displayName").stringValue   = row.Get("displayName");
                SetColor(so.FindProperty("color"), row, "color");
                so.FindProperty("interactionVerb").stringValue = row.Get("interactionVerb", "forage");
                SetYields(so.FindProperty("commonYields"),
                    ParseYields(row.Get("commonYields"), items));
                SetRare(so.FindProperty("rareYield"),
                    ParseRare(row.Get("rareYield"), items));
                so.FindProperty("rareDropChance").floatValue  = F(row.Get("rareDropChance", "0.08"));
                so.FindProperty("minTickInterval").floatValue = F(row.Get("minTickInterval", "1.5"));
                so.FindProperty("maxTickInterval").floatValue = F(row.Get("maxTickInterval", "2"));
                so.FindProperty("tendVerb").stringValue        = row.Get("tendVerb", "tend");
                SetYields(so.FindProperty("harvestYields"),
                    ParseYields(row.Get("harvestYields"), items));
                so.FindProperty("restsToHarvest").intValue     = I(row.Get("restsToHarvest", "1"));
                so.FindProperty("maxConcurrentMarked").intValue = I(row.Get("maxConcurrentMarked", "2"));

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
                so.FindProperty("spotDisplayName").stringValue = row.Get("spotDisplayName");
                so.FindProperty("spotVerb").stringValue      = row.Get("spotVerb", "forage");
                SetColor(so.FindProperty("spotColor"), row, "spotColor");
                SetYields(so.FindProperty("commonYields"),
                    ParseYields(row.Get("commonYields"), items));
                SetRare(so.FindProperty("rareYield"),
                    ParseRare(row.Get("rareYield"), items));
                so.FindProperty("rareDropChance").floatValue = F(row.Get("spotRareDropChance", "0.08"));
                so.FindProperty("archetypeSpotMinTickInterval").floatValue =
                    F(row.Get("spotMinTickInterval", "1.5"));
                so.FindProperty("archetypeSpotMaxTickInterval").floatValue =
                    F(row.Get("spotMaxTickInterval", "2"));
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
                SetRare(so.FindProperty("poiRareYield"),
                    ParseRare(row.Get("poiRareYield"), items));
                so.FindProperty("poiRareDropChance").floatValue = F(row.Get("poiRareDropChance", "0.05"));

                // Spot knowledge yields (Iteration 28; requiredSpecializationId added Iteration 34)
                var kSpotEntries = new List<(string flag, string specId, ItemDefinition item, int minQ, int maxQ, float weight)>();
                for (int i = 1; ; i++)
                {
                    string kFlag   = row.Get($"spotKnowledge{i}_flag");
                    string kSpecId = row.Get($"spotKnowledge{i}_specializationId");
                    if (string.IsNullOrEmpty(kFlag) && string.IsNullOrEmpty(kSpecId)) break;
                    items.TryGetValue(row.Get($"spotKnowledge{i}_item"), out var kItem);
                    kSpotEntries.Add((
                        kFlag, kSpecId, kItem,
                        I(row.Get($"spotKnowledge{i}_minQty", "1")),
                        I(row.Get($"spotKnowledge{i}_maxQty", "2")),
                        F(row.Get($"spotKnowledge{i}_weight", "0.15"))
                    ));
                }
                var kSpotProp = so.FindProperty("spotKnowledgeYields");
                kSpotProp.arraySize = kSpotEntries.Count;
                for (int i = 0; i < kSpotEntries.Count; i++)
                {
                    var e = kSpotProp.GetArrayElementAtIndex(i);
                    var (kFlag, kSpecId, kItem, kMinQ, kMaxQ, kWeight) = kSpotEntries[i];
                    e.FindPropertyRelative("requiredFlag").stringValue             = kFlag;
                    e.FindPropertyRelative("requiredSpecializationId").stringValue = kSpecId;
                    e.FindPropertyRelative("item").objectReferenceValue            = kItem;
                    e.FindPropertyRelative("minQty").intValue                      = kMinQ;
                    e.FindPropertyRelative("maxQty").intValue                      = kMaxQ;
                    e.FindPropertyRelative("injectedWeight").floatValue            = kWeight;
                }

                // NPC post-spec stages (passiveDriftSourceArchetypeId added Iteration 34)
                var npcStages = new List<(string id, string name, int cost,
                    bool useRare, int count, string flavor, string flag, string passiveSrc)>();
                for (int i = 1; ; i++)
                {
                    string id = row.Get($"stage{i}_id");
                    if (string.IsNullOrEmpty(id)) break;
                    npcStages.Add((
                        id,
                        row.Get($"stage{i}_displayName"),
                        I(row.Get($"stage{i}_progressCost", "3")),
                        B(row.Get($"stage{i}_useRareItem")),
                        I(row.Get($"stage{i}_itemCount", "2")),
                        row.Get($"stage{i}_flavorText"),
                        row.Get($"stage{i}_worldStateFlag"),
                        row.Get($"stage{i}_passiveDriftSourceArchetypeId")
                    ));
                }
                var npcProp = so.FindProperty("npcPostSpecStages");
                npcProp.arraySize = npcStages.Count;
                for (int i = 0; i < npcStages.Count; i++)
                {
                    var e = npcProp.GetArrayElementAtIndex(i);
                    var (id, name, cost, useRare, count, flavor, flag, passiveSrc) = npcStages[i];
                    e.FindPropertyRelative("stageId").stringValue                        = id;
                    e.FindPropertyRelative("displayName").stringValue                    = name;
                    e.FindPropertyRelative("progressCost").intValue                      = cost;
                    e.FindPropertyRelative("useRareItem").boolValue                      = useRare;
                    e.FindPropertyRelative("itemCount").intValue                         = count;
                    e.FindPropertyRelative("flavorText").stringValue                     = flavor;
                    e.FindPropertyRelative("worldStateFlag").stringValue                 = flag;
                    e.FindPropertyRelative("passiveDriftSourceArchetypeId").stringValue  = passiveSrc;
                }

                // Maintenance fields (Iteration 29)
                so.FindProperty("buildingColdFlavor").stringValue   = row.Get("buildingColdFlavor");
                so.FindProperty("buildingMaintenanceCost").intValue = I(row.Get("buildingMaintenanceCost", "2"));
                so.FindProperty("npcColdFlavor").stringValue        = row.Get("npcColdFlavor");
                so.FindProperty("npcMaintenanceCost").intValue      = I(row.Get("npcMaintenanceCost", "1"));

                // Building stages (worldStateFlag added Iteration 34)
                so.FindProperty("buildingDilapidatedName").stringValue = row.Get("buildingDilapidatedName");
                SetColor(so.FindProperty("buildingDilapidatedColor"), row, "buildingDilapidatedColor");
                var bStages = new List<(string name, string verb, ItemDefinition mat,
                    int cost, int prog, string spec, Color tint, string wFlag)>();
                for (int i = 1; ; i++)
                {
                    string name = row.Get($"bStage{i}_displayName");
                    if (string.IsNullOrEmpty(name)) break;
                    items.TryGetValue(row.Get($"bStage{i}_material"), out var mat);
                    bStages.Add((
                        name,
                        row.Get($"bStage{i}_verb"),
                        mat,
                        I(row.Get($"bStage{i}_costPerTick", "2")),
                        I(row.Get($"bStage{i}_progressCost", "6")),
                        row.Get($"bStage{i}_requiredSpecialization"),
                        new Color(
                            F(row.Get($"bStage{i}_tint_r", "0.5")),
                            F(row.Get($"bStage{i}_tint_g", "0.5")),
                            F(row.Get($"bStage{i}_tint_b", "0.45")), 1f),
                        row.Get($"bStage{i}_worldStateFlag")
                    ));
                }
                var bProp = so.FindProperty("buildingStages");
                bProp.arraySize = bStages.Count;
                for (int i = 0; i < bStages.Count; i++)
                {
                    var e = bProp.GetArrayElementAtIndex(i);
                    var (name, verb, mat, cost, prog, spec, tint, wFlag) = bStages[i];
                    e.FindPropertyRelative("displayName").stringValue            = name;
                    e.FindPropertyRelative("verb").stringValue                   = verb;
                    e.FindPropertyRelative("material").objectReferenceValue      = mat;
                    e.FindPropertyRelative("costPerTick").intValue               = cost;
                    e.FindPropertyRelative("progressCost").intValue              = prog;
                    e.FindPropertyRelative("requiredSpecialization").stringValue  = spec;
                    e.FindPropertyRelative("tint").colorValue                    = tint;
                    e.FindPropertyRelative("worldStateFlag").stringValue         = wFlag;
                }

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

        static Dictionary<string, ItemDefinition> BuildItemDb()
        {
            var db = new Dictionary<string, ItemDefinition>(StringComparer.OrdinalIgnoreCase);
            foreach (var guid in AssetDatabase.FindAssets("t:ItemDefinition"))
            {
                var item = AssetDatabase.LoadAssetAtPath<ItemDefinition>(
                    AssetDatabase.GUIDToAssetPath(guid));
                if (item != null && !db.ContainsKey(item.DisplayName))
                    db[item.DisplayName] = item;
            }
            return db;
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

        static void SetRare(SerializedProperty prop, ItemYield rare)
        {
            prop.FindPropertyRelative("Item").objectReferenceValue = rare?.Item;
            prop.FindPropertyRelative("MinQuantity").intValue      = rare?.MinQuantity ?? 1;
            prop.FindPropertyRelative("MaxQuantity").intValue      = rare?.MaxQuantity ?? 1;
            prop.FindPropertyRelative("Weight").floatValue         = 1f;
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

        static ItemYield ParseRare(string compact, Dictionary<string, ItemDefinition> items)
        {
            if (string.IsNullOrWhiteSpace(compact)) return null;
            var p = compact.Trim().Split(':');
            if (p.Length < 4) return null;
            if (!items.TryGetValue(p[0].Trim(), out var item))
            { Debug.LogWarning($"[Import] Unknown item: '{p[0]}'"); return null; }
            return new ItemYield { Item = item, Weight = 1f, MinQuantity = I(p[2]), MaxQuantity = I(p[3]) };
        }

        static float F(string s) => float.TryParse(s,
            NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;
        static int I(string s) => int.TryParse(s, out int v) ? v : 0;
        static bool B(string s) =>
            string.Equals(s?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

        static List<Dictionary<string, string>> ReadCsv(string path)
        {
            if (!File.Exists(path))
            { Debug.LogWarning($"[Import] CSV not found: {path}\n  Run Mossmark/Data/Export All first."); return null; }

            var lines = File.ReadAllLines(path, Encoding.UTF8);

            // Find the header: first non-blank, non-comment line.
            int headerLine = -1;
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                if (lines[i].TrimStart().StartsWith("#")) continue;
                headerLine = i;
                break;
            }
            if (headerLine < 0) return new List<Dictionary<string, string>>();

            var headers = ParseCsvLine(lines[headerLine]);
            var result = new List<Dictionary<string, string>>();
            for (int i = headerLine + 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                if (lines[i].TrimStart().StartsWith("#")) continue;
                var vals = ParseCsvLine(lines[i]);
                var row = new Dictionary<string, string>(StringComparer.Ordinal);
                for (int j = 0; j < headers.Count && j < vals.Count; j++)
                    row[headers[j]] = vals[j];
                result.Add(row);
            }
            return result;
        }

        static List<string> ParseCsvLine(string line)
        {
            var fields = new List<string>();
            var sb = new StringBuilder();
            bool inQuotes = false;
            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"')
                        { sb.Append('"'); i++; }   // escaped ""
                        else inQuotes = false;
                    }
                    else sb.Append(c);
                }
                else if (c == '"') inQuotes = true;
                else if (c == ',') { fields.Add(sb.ToString()); sb.Clear(); }
                else sb.Append(c);
            }
            fields.Add(sb.ToString());
            return fields;
        }
    }

    static class CsvRowExt
    {
        public static string Get(this Dictionary<string, string> d, string key, string def = "") =>
            d.TryGetValue(key, out var v) ? v : def;
    }
}
