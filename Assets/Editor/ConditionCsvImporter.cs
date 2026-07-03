using System;
using System.Collections.Generic;
using System.IO;
using Mossmark.Development;
using Mossmark.Inventory;
using UnityEditor;
using UnityEngine;
using static Mossmark.Editor.CsvUtil;

namespace Mossmark.Editor
{
    // Foundational relational-data helper (relational-data migration, phase 1). Reads a
    // "stage conditions" CSV — one row per condition, grouped by an owning stage id —
    // and constructs concrete IDependencyCondition instances ready to assign into a
    // [SerializeReference] array via SerializedProperty.managedReferenceValue.
    //
    // This is the P2 analogue of P1's CsvDataImporter.ImportUpgradeDependencies, with one
    // extra step: because IDependencyCondition is a genuine interface (not a closed
    // enum-discriminated struct like P1's UpgradeDependency), the importer switches on a
    // `conditionType` column to `new` up the concrete object, then assigns the whole
    // object as a managed reference. managedReferenceValue is the documented Unity API
    // for exactly this (2019.3+); it is unrelated to the MCP-Unity `update_component`
    // limitation noted in CLAUDE.md, which only constrains MCP's generic inspector tool —
    // not custom Editor scripts.
    //
    // Supported conditionType values — the day-one set, i.e. the gates archetype-owned
    // stages actually use today. Only the columns relevant to a row's type are read:
    //   item      -> ItemAvailableCondition(item, quantity)             cols: item, quantity
    //   property  -> PropertyAvailableCondition(propertyId, want)       cols: propertyId, wantDescription
    //   worldflag -> WorldStateCondition(flagId, requiredValue, needs)  cols: flagId, requiredValue, needsDescription
    //   spec      -> SpecializationRealizedCondition(specId, needs)     cols: specializationId, needsDescription
    //   time      -> TimeCondition(requiredProgress)                    cols: quantity (progress; unused at runtime)
    //
    // An unknown conditionType or an unresolvable required reference logs a warning and is
    // skipped, so a bad row never silently produces a null gate.
    public static class ConditionCsvImporter
    {
        // Reads the CSV and returns conditions grouped by stage id, preserving row order
        // within each stage so authored condition order is stable.
        public static Dictionary<string, List<IDependencyCondition>> ReadByStage(
            string csvPath, Dictionary<string, ItemDefinition> items,
            string stageIdColumn = "stageId")
        {
            var byStage = new Dictionary<string, List<IDependencyCondition>>(StringComparer.Ordinal);
            var rows = ReadCsv(csvPath);
            if (rows == null) return byStage;

            foreach (var row in rows)
            {
                string stageId = row.Get(stageIdColumn);
                if (string.IsNullOrEmpty(stageId)) continue;

                var condition = BuildCondition(row, items);
                if (condition == null) continue;

                if (!byStage.TryGetValue(stageId, out var list))
                    byStage[stageId] = list = new List<IDependencyCondition>();
                list.Add(condition);
            }
            return byStage;
        }

        // Constructs one condition from a row. Returns null (with a warning) on an unknown
        // conditionType or an unresolvable required reference.
        public static IDependencyCondition BuildCondition(
            Dictionary<string, string> row, Dictionary<string, ItemDefinition> items)
        {
            string type = row.Get("conditionType").Trim().ToLowerInvariant();
            switch (type)
            {
                case "item":
                {
                    string itemName = row.Get("item");
                    if (!items.TryGetValue(itemName, out var item))
                    { Warn($"item condition references unknown item '{itemName}'"); return null; }
                    return new ItemAvailableCondition(item, Math.Max(1, I(row.Get("quantity", "1"))));
                }
                case "property":
                {
                    string prop = row.Get("propertyId");
                    if (string.IsNullOrEmpty(prop)) { Warn("property condition missing propertyId"); return null; }
                    return new PropertyAvailableCondition(prop, row.Get("wantDescription"));
                }
                case "worldflag":
                {
                    string flag = row.Get("flagId");
                    if (string.IsNullOrEmpty(flag)) { Warn("worldflag condition missing flagId"); return null; }
                    return new WorldStateCondition(flag, B(row.Get("requiredValue", "true")),
                        row.Get("needsDescription"));
                }
                case "spec":
                {
                    string spec = row.Get("specializationId");
                    if (string.IsNullOrEmpty(spec)) { Warn("spec condition missing specializationId"); return null; }
                    return new SpecializationRealizedCondition(spec, row.Get("needsDescription"));
                }
                case "time":
                    return new TimeCondition(I(row.Get("quantity", "0")));
                default:
                    Warn($"unknown conditionType '{type}'");
                    return null;
            }
        }

        // Assigns a list of conditions into a [SerializeReference] array property. The
        // caller owns the SerializedObject and its ApplyModifiedProperties() call.
        //
        // Idempotency: assigning a managedReferenceValue always creates a fresh managed
        // reference id, which would re-serialize the asset (and churn version control) on
        // every import even when nothing changed. So the existing array is compared first
        // — same length, same concrete types, same field values (via JsonUtility, which
        // serializes exactly the [SerializeField] surface) — and equivalent arrays are
        // left untouched.
        public static void AssignConditions(
            SerializedProperty arrayProp, IReadOnlyList<IDependencyCondition> conditions)
        {
            int n = conditions?.Count ?? 0;

            if (arrayProp.arraySize == n)
            {
                bool same = true;
                for (int i = 0; i < n && same; i++)
                {
                    var existing = arrayProp.GetArrayElementAtIndex(i).managedReferenceValue;
                    same = existing != null
                        && existing.GetType() == conditions[i].GetType()
                        && JsonUtility.ToJson(existing) == JsonUtility.ToJson(conditions[i]);
                }
                if (same) return;
            }

            arrayProp.arraySize = n;
            for (int i = 0; i < n; i++)
                arrayProp.GetArrayElementAtIndex(i).managedReferenceValue = conditions[i];
        }

        static void Warn(string message) => Debug.LogWarning($"[ConditionImport] {message}");

        // --------------------------------------------------------------------------- //
        // Self-test (phase-1 validation)
        // --------------------------------------------------------------------------- //

        // Proves the whole CSV -> concrete condition -> [SerializeReference] path works
        // end-to-end without touching any production asset: parses the sample fixture,
        // assigns each stage's conditions onto an in-memory probe, then re-reads through a
        // fresh SerializedObject to confirm the managed references survived the
        // serialize/apply round-trip (the acid test of the managedReferenceValue technique).
        [MenuItem("Mossmark/Data/Validate Condition Import (self-test)")]
        public static void SelfTest()
        {
            var items = BuildItemDb();
            string samplePath = Path.GetFullPath(
                Path.Combine(Application.dataPath, "../Tools/Data/_sample_conditions.csv"));
            var byStage = ReadByStage(samplePath, items);
            if (byStage.Count == 0)
            { Debug.LogError("[ConditionImport] self-test: sample CSV produced no conditions."); return; }

            var probe = ScriptableObject.CreateInstance<ConditionImportProbe>();
            int total = 0, ok = 0;
            try
            {
                foreach (var kvp in byStage)
                {
                    var so = new SerializedObject(probe);
                    AssignConditions(so.FindProperty("conditions"), kvp.Value);
                    so.ApplyModifiedProperties();

                    var readback = new SerializedObject(probe);
                    var arr = readback.FindProperty("conditions");
                    for (int i = 0; i < arr.arraySize; i++)
                    {
                        total++;
                        var val = arr.GetArrayElementAtIndex(i).managedReferenceValue as IDependencyCondition;
                        if (val == null)
                        { Debug.LogError($"[ConditionImport] stage '{kvp.Key}' cond {i}: null after round-trip"); continue; }
                        ok++;
                        Debug.Log($"[ConditionImport] stage '{kvp.Key}' cond {i}: " +
                            $"{val.GetType().Name} — \"{val.GetNeedsDescription(null)}\"");
                    }
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(probe);
            }

            string verdict = ok == total && total > 0 ? "PASS" : "FAIL";
            Debug.Log($"[ConditionImport] self-test: {ok}/{total} conditions round-tripped " +
                $"via managedReferenceValue. {verdict}");
        }
    }

    // Editor-only, in-memory scratch host for the self-test. Never saved as an asset;
    // exists solely so there is a [SerializeReference] array to exercise.
    class ConditionImportProbe : ScriptableObject
    {
        [SerializeReference] public IDependencyCondition[] conditions = Array.Empty<IDependencyCondition>();
    }
}
