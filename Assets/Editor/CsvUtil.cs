using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Mossmark.Inventory;
using UnityEditor;
using UnityEngine;

namespace Mossmark.Editor
{
    // Shared low-level CSV plumbing used by every data importer (ImportGameData,
    // ConditionCsvImporter, and future relational importers). Extracted from
    // ImportGameData in the relational-data migration so the parse/lookup logic
    // has a single source of truth. Domain-specific parsing (ItemYield packing,
    // color unpacking) stays with its owning importer — only the generic
    // primitives live here.
    public static class CsvUtil
    {
        // Item lookup keyed by DisplayName (the reference form used throughout the
        // CSV pipeline — items are never referenced by asset path). OrdinalIgnoreCase
        // so authored display names don't have to match asset casing exactly.
        public static Dictionary<string, ItemDefinition> BuildItemDb()
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

        public static float F(string s) => float.TryParse(s,
            NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;

        public static int I(string s) => int.TryParse(s, out int v) ? v : 0;

        public static bool B(string s) =>
            string.Equals(s?.Trim(), "true", StringComparison.OrdinalIgnoreCase);

        // Reads a CSV into a list of header->value dictionaries. Blank lines and
        // '#'-prefixed comment lines are skipped; the first surviving line is the header.
        public static List<Dictionary<string, string>> ReadCsv(string path)
        {
            if (!File.Exists(path))
            { Debug.LogWarning($"[Import] CSV not found: {path}\n  Run Mossmark/Data/Export All first."); return null; }

            var lines = File.ReadAllLines(path, Encoding.UTF8);

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
