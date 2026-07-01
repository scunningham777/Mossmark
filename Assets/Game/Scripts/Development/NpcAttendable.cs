using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Day;
using Mossmark.Inventory;
using Mossmark.Visuals;
using Mossmark.World;
using UnityEngine;

namespace Mossmark.Development
{
    // Iteration 10: an unspecialized NPC develops via repeated attention. Each productive
    // tick is "spending time" with them - TimeCondition makes every tick productive until
    // PendingProgress reaches progressCost, at which point TryApplyStage's random-among-
    // available selection draws a specialization (interrupting the hold).
    //
    // Iteration 17: post-spec development stages gated by item availability; pool-sealing
    // lets GetNextStage() reach those stages after the spec draws.
    //
    // Iteration 22 (G3): BuildPostSpecStages is now a generic loop over
    // archetype.NpcPostSpecStages — the hardcoded per-archetype switch is gone.
    // New post-spec content requires only editing the PlaceArchetype asset.
    //
    // Iteration 28.5: fully-developed NPCs (GetNextStage() == null) remain attendable.
    // Universal specs (forager/caretaker/tinkerer) give flavor-only visits. Archetype
    // specs (Bog Keeper, Herald, etc.) have a chance to gift items from their exchange
    // pool (authored in PlaceArchetype.NpcExchangeGifts). Visiting always costs 1 daylight,
    // is one-shot per attend (ContinueAttending => false), and uses lastAttentionWasVisit
    // to override RequiresDaylight/ContinueAttending without touching DevelopableEntity.
    //
    // Iteration 29: settlement maintenance drift. NPCs accumulate drift each rest after
    // specialization (CurrentStageIndex >= 0). Cold NPCs (driftProgress >= 7) suspend
    // their WorldState flags (post-spec stage effects go dormant). Direct attend with
    // maintenanceMaterial (archetype's CommonYields[0].Item) resets drift. Visits (fully
    // developed) also reset drift — for universal-spec NPCs with no material, the visit
    // IS the maintenance path.
    public class NpcAttendable : DevelopableEntity, IAttendable, IMaintenanceConsumer
    {
        [SerializeField] private string genericName = "Wanderer";
        [SerializeField, Min(1)] private int progressCost = 8;
        [SerializeField, Min(0.1f)] private float minTickInterval = 1f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 1.5f;

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private string specializedName;
        private string drawnSpecializationId;
        private string coldFlavor;
        private List<string> poolStageIds;
        private Dictionary<string, List<string>> postSpecStageIdsBySpecId;
        private Dictionary<string, (string Title, Color Tint)> specializationInfo;
        private Dictionary<string, NpcPostSpecStageDef> postSpecStageDefs;
        private float currentTickInterval;

        // Maintenance fields set when specialization fires.
        private ItemDefinition maintenanceMaterial;
        private int maintenanceCostPerReset = 1;

        // Tracks WorldState flags set by post-spec stages so they can be suspended
        // (cold) and restored (reset). Built up in HandleDeveloped.
        private readonly HashSet<string> setWorldStateFlags = new();

        // Iteration 28.5: exchange pool set when specialization fires.
        private NpcExchangePool drawnExchangePool;
        private bool lastAttentionWasVisit;
        private bool lastAttentionWasMaintenance;

        // Iteration 32: fired in OnDayAdvanced when passive progress is added but the
        // stage doesn't cross that rest — signals EntityFeedback to show the drift halo.
        public event System.Action OnPassiveDriftAccrued;

        // Flavor text for universal specs — stable enough to live in code.
        private static readonly Dictionary<string, (string[] visitFlavors, string[] exchangeFlavors)>
            universalSpecFlavors = new()
            {
                ["forager"] = (
                    new[] {
                        "points out a plant you hadn't noticed.",
                        "shares what they know of the land without being asked.",
                        "is comfortable in a silence that doesn't need filling."
                    },
                    System.Array.Empty<string>()
                ),
                ["caretaker"] = (
                    new[] {
                        "tends to the space between you with quiet, habitual care.",
                        "has a way of making even a brief visit feel unhurried.",
                        "notices something about the day you hadn't registered."
                    },
                    System.Array.Empty<string>()
                ),
                ["tinkerer"] = (
                    new[] {
                        "is working on something small and intricate that they don't explain.",
                        "asks about your pack — curious, but not wanting anything.",
                        "doesn't look up when you arrive, but seems glad you did."
                    },
                    System.Array.Empty<string>()
                ),
            };

        public override string DisplayName => drawnSpecializationId != null ? specializedName : genericName;
        protected override DevelopmentTrack Track => track;

        // Procedural-spawn entry point (ArrivalSpawner), following the inactive-GO
        // pattern — call before SetActive(true) so Awake fires with correct field values.
        public void Initialize(string genericName, int progressCost = 8,
            float minTickInterval = 1.5f, float maxTickInterval = 2f)
        {
            this.genericName = genericName;
            this.progressCost = progressCost;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
        }

        public float AttentionDuration => currentTickInterval;

        public bool RequiresDaylight =>
            lastAttentionWasVisit || (!lastAttentionWasMaintenance && LastAttentionMadeProgress);

        public bool ContinueAttending =>
            !lastAttentionWasVisit && !lastAttentionWasMaintenance
            && LastAttentionMadeProgress && !LastAttentionAppliedStage;

        // IMaintenanceConsumer — universal-spec NPCs have null maintenanceMaterial
        // (visits are their maintenance path); archetype NPCs use CommonYields[0].Item.
        public int DriftThreshold => 7;
        public ItemDefinition MaintenanceMaterial => maintenanceMaterial;
        public int MaintenanceCostPerReset => maintenanceCostPerReset;

        private float RollTickInterval() => Random.Range(minTickInterval, maxTickInterval);

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            currentTickInterval = RollTickInterval();

            specializationInfo = new Dictionary<string, (string Title, Color Tint)>
            {
                ["forager"]   = ("Forager",   new Color(0.5f, 0.7f, 0.3f, 1f)),
                ["caretaker"] = ("Caretaker", new Color(0.8f, 0.6f, 0.8f, 1f)),
                ["tinkerer"]  = ("Tinkerer",  new Color(0.7f, 0.6f, 0.3f, 1f)),
            };

            // Universal pool — available in any session, no building dep required.
            var stages = new List<DevelopmentStage>
            {
                new("forager",   "Take up Foraging",   progressCost, new TimeCondition(progressCost)),
                new("caretaker", "Take up Caretaking", progressCost, new TimeCondition(progressCost)),
                new("tinkerer",  "Take up Tinkering",  progressCost, new TimeCondition(progressCost)),
            };

            poolStageIds = new List<string> { "forager", "caretaker", "tinkerer" };

            // Archetype-specific pool stages added for each selected archetype. Gated by
            // SpecializationNeededCondition so they only become candidates once the matching
            // building declares demand. Same progressCost as universal stages so TryApplyStage's
            // priority-filter (needed-stage wins over universal) fires at the same threshold.
            foreach (var archetype in WorldGenerator.SelectedArchetypes)
            {
                if (string.IsNullOrEmpty(archetype.SpecializationId)) continue;

                stages.Add(new DevelopmentStage(archetype.SpecializationId, archetype.StageDisplayName, progressCost,
                    new SpecializationNeededCondition(archetype.SpecializationId),
                    new TimeCondition(progressCost)));
                poolStageIds.Add(archetype.SpecializationId);
                specializationInfo[archetype.SpecializationId] = (archetype.NpcTitle, archetype.NpcTint);
            }

            // Post-spec stages appended after all pool stages so GetNextStage() still
            // resolves to "forager" pre-specialization (ticks 1–(progressCost-1) unaffected).
            postSpecStageIdsBySpecId = new Dictionary<string, List<string>>();
            postSpecStageDefs = new Dictionary<string, NpcPostSpecStageDef>();
            foreach (var archetype in WorldGenerator.SelectedArchetypes)
            {
                var postStages = BuildPostSpecStages(archetype);
                if (postStages.Count == 0) continue;

                stages.AddRange(postStages);
                postSpecStageIdsBySpecId[archetype.SpecializationId] = postStages.ConvertAll(s => s.Id);

                // Build the stageId → def lookup for HandleDeveloped.
                if (archetype.NpcPostSpecStages != null)
                    foreach (var def in archetype.NpcPostSpecStages)
                        postSpecStageDefs[def.stageId] = def;
            }

            track = new DevelopmentTrack(stages.ToArray());

            // Pre-seal all post-spec stages so they can't fire before this NPC has
            // drawn the matching specialization. SpecializationRealizedCondition is
            // global (static), so without this seal a second NPC would have its
            // post-spec stages become candidates the moment the first NPC specializes.
            foreach (var kvp in postSpecStageIdsBySpecId)
                foreach (var stageId in kvp.Value)
                    MarkStageAsApplied(stageId);

            OnDeveloped += HandleDeveloped;
        }

        private void Start()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced += OnDayAdvanced;
        }

        private void OnDestroy()
        {
            if (DayCycleManager.Instance != null)
                DayCycleManager.Instance.DayAdvanced -= OnDayAdvanced;
        }

        // Iteration 31 pilot: the Bog Keeper's Drainage Channels stage gains passive
        // progress each rest, scaled by the Fen Bog spot's own tendedness — a well-tended
        // bog drains itself a little further on its own. Hardcoded to this one stage per
        // FEATURES.md's pilot scope; only worth generalizing into shared schema once the
        // feeling is validated in play.
        private void OnDayAdvanced()
        {
            if (drawnSpecializationId != "bog_tender") return;
            if (GetNextStage()?.Id != "bog_keeper_drainage") return;

            var fenBog = WorldGenerator.GetArchetypeSpot("bog");
            if (fenBog == null) return;

            int passive = fenBog.Tendedness > 0.7f ? 2 : fenBog.Tendedness >= 0.3f ? 1 : 0;
            if (passive <= 0) return;

            AddProgress(passive);
            Debug.Log($"{specializedName}: Drainage Channels edge forward on their own " +
                $"(+{passive} passive progress, Fen Bog tendedness {fenBog.Tendedness:F2}).", this);

            // If the stage crossed, OnDeveloped fires (shape swap is the signal).
            // Otherwise, fire OnPassiveDriftAccrued so EntityFeedback can show the halo.
            bool stageApplied = TryApplyStage();
            if (!stageApplied)
                OnPassiveDriftAccrued?.Invoke();
        }

        public bool CanAttend()
        {
            if (drawnSpecializationId == null) return true;

            // Fully developed: always attendable (visit or maintenance).
            if (GetNextStage() == null) return true;

            // Direct maintenance: available when drifted and carrying archetype material.
            if (DriftProgress > 0 && maintenanceMaterial != null
                && InventoryManager.Instance != null
                && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
                return true;

            return CanMakeProgress();
        }

        // Drift suffix shows in the overlay name when warning/cold.
        public string GetShortName() => DisplayName;

        public string GetOverlayDescription() =>
            GetDriftOverlayDescription(DisplayName, DriftThreshold, coldFlavor);

        public IReadOnlyList<string> GetAppliedUpgrades() => GetAppliedUpgradeNames();

        public string GetOverlayInteractionLine()
        {
            if (drawnSpecializationId == null)
                return GetNeedsOrDefault($"Hold E to spend time with {genericName} - they need more time to find their place");

            // Maintenance prompt (archetype NPCs only) — shown when drifted and carrying material.
            if (DriftProgress > 0 && maintenanceMaterial != null
                && InventoryManager.Instance != null
                && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
                return $"Hold E to check in with {specializedName}";

            if (GetNextStage() == null)
                return $"Hold E to visit with {specializedName}";

            return GetNeedsOrDefault($"Hold E to help {specializedName} develop their craft further");
        }

        public void OnAttentionComplete()
        {
            lastAttentionWasVisit = false;
            lastAttentionWasMaintenance = false;

            // Only apply post-specialization logic once drawn.
            if (drawnSpecializationId != null)
            {
                // Priority 1: direct maintenance (archetype NPCs with material).
                if (DriftProgress > 0 && maintenanceMaterial != null
                    && InventoryManager.Instance != null
                    && InventoryManager.Instance.GetQuantity(maintenanceMaterial) >= maintenanceCostPerReset)
                {
                    lastAttentionWasMaintenance = true;
                    InventoryManager.Instance.RemoveItem(maintenanceMaterial, maintenanceCostPerReset);
                    ResetDrift();
                    NotificationManager.Post($"{specializedName}: feels present again.");
                    Debug.Log($"{specializedName}: maintained directly by player.", this);
                    currentTickInterval = RollTickInterval();
                    return;
                }

                // Priority 2: visit (fully developed, OR universal-spec NPC maintenance path).
                if (GetNextStage() == null)
                {
                    lastAttentionWasVisit = true;
                    RunVisitInteraction();
                    // Visiting always resets drift — for universal-spec NPCs this is the
                    // only maintenance path (no archetype material).
                    if (DriftProgress > 0) ResetDrift();
                    currentTickInterval = RollTickInterval();
                    return;
                }
            }

            // Priority 3: pre-specialization development or post-spec stage development.
            ResolveAttention();
            currentTickInterval = RollTickInterval();
        }

        public void OnAttentionCancelled() { }

        // When drift reaches the cold threshold, suspend all WorldState flags this NPC
        // has set (post-spec stage effects go dormant until drift resets).
        protected override void OnDriftChanged()
        {
            if (DriftProgress < DriftThreshold) return;
            foreach (var flag in setWorldStateFlags)
                WorldState.SetFlag(flag, false);
            Debug.Log($"{DisplayName}: gone cold — WorldState effects suspended.", this);
        }

        // When drift resets, restore all WorldState flags.
        protected override void OnDriftReset()
        {
            foreach (var flag in setWorldStateFlags)
                WorldState.SetFlag(flag, true);
            if (setWorldStateFlags.Count > 0)
                Debug.Log($"{DisplayName}: WorldState effects restored.", this);
        }

        private void RunVisitInteraction()
        {
            if (drawnExchangePool == null)
            {
                NotificationManager.Post($"{specializedName}: settles quietly into their work nearby.");
                return;
            }

            bool exchanged = false;

            if (drawnExchangePool.gifts != null && drawnExchangePool.gifts.Length > 0
                && Random.value < drawnExchangePool.exchangeChance)
            {
                int giftIndex = PickWeightedGiftIndex(drawnExchangePool.gifts);
                var gift = drawnExchangePool.gifts[giftIndex];
                if (gift?.Item != null)
                {
                    int qty = Random.Range(gift.MinQuantity, gift.MaxQuantity + 1);
                    int added = InventoryManager.Instance != null
                        ? InventoryManager.Instance.AddItem(gift.Item, qty) : 0;

                    if (added > 0)
                    {
                        exchanged = true;
                        // Exchange flavors are parallel to gifts — use same index so text matches item.
                        string flavor = drawnExchangePool.exchangeFlavors != null
                            && giftIndex < drawnExchangePool.exchangeFlavors.Length
                            ? drawnExchangePool.exchangeFlavors[giftIndex]
                            : $"offers {added}x {gift.Item.DisplayName}.";
                        NotificationManager.Post($"{specializedName}: {flavor}");
                        Debug.Log($"{specializedName}: gave {added}x {gift.Item.DisplayName}. {flavor}", this);
                    }
                }
            }

            if (!exchanged)
            {
                string flavor = PickRandom(drawnExchangePool.visitFlavors);
                if (string.IsNullOrEmpty(flavor)) flavor = "is quiet company.";
                NotificationManager.Post($"{specializedName}: {flavor}");
                Debug.Log($"{specializedName}: {flavor}", this);
            }
        }

        private void HandleDeveloped(DevelopmentStage stage)
        {
            // --- Specialization draw ---
            if (specializationInfo.TryGetValue(stage.Id, out var info))
            {
                drawnSpecializationId = stage.Id;
                specializedName = $"{genericName} the {info.Title}";
                if (spriteRenderer != null) spriteRenderer.color = info.Tint;
                RealizedSpecializations.Declare(stage.Id);

                // Set maintenance material from this archetype's common yield.
                foreach (var archetype in WorldGenerator.SelectedArchetypes)
                {
                    if (archetype.SpecializationId != stage.Id) continue;
                    maintenanceMaterial = archetype.CommonYields?.Length > 0
                        ? archetype.CommonYields[0].Item : null;
                    maintenanceCostPerReset = archetype.NpcMaintenanceCost;
                    coldFlavor = archetype.NpcColdFlavor;
                    break;
                }
                // Universal specs get no maintenance material — visits are their path.

                // Build exchange pool from archetype if this is an archetype spec,
                // otherwise use hardcoded universal-spec flavors (no item exchange).
                drawnExchangePool = BuildExchangePool(stage.Id);

                foreach (var id in poolStageIds)
                    if (id != stage.Id) MarkStageAsApplied(id);

                foreach (var kvp in postSpecStageIdsBySpecId)
                    if (kvp.Key != stage.Id)
                        foreach (var stageId in kvp.Value)
                            MarkStageAsApplied(stageId);

                // Unseal this NPC's own post-spec stages now that specialization has
                // fired. They were pre-sealed in Awake to prevent cross-NPC contamination
                // (SpecializationRealizedCondition is global).
                if (postSpecStageIdsBySpecId.TryGetValue(stage.Id, out var ownPostStageIds))
                    foreach (var stageId in ownPostStageIds)
                        MarkStageAsAvailable(stageId);

                return;
            }

            // --- Post-specialization stage ---
            if (postSpecStageDefs.TryGetValue(stage.Id, out var stageDef))
            {
                if (!string.IsNullOrEmpty(stageDef.worldStateFlag))
                {
                    WorldState.SetFlag(stageDef.worldStateFlag, true);
                    setWorldStateFlags.Add(stageDef.worldStateFlag);
                }
                Debug.Log($"{specializedName}: {stageDef.flavorText}", this);
            }
            else
            {
                WorldState.SetFlag(stage.Id, true);
                setWorldStateFlags.Add(stage.Id);
                Debug.Log($"{specializedName}: {stage.DisplayName}.", this);
            }
        }

        // Builds the exchange pool for the drawn specialization. Archetype specs get
        // gift pools and flavors from PlaceArchetype; universal specs get hardcoded
        // flavor-only pools (no item exchange — their "gift" is time and knowledge).
        private NpcExchangePool BuildExchangePool(string specId)
        {
            foreach (var archetype in WorldGenerator.SelectedArchetypes)
            {
                if (archetype.SpecializationId != specId) continue;
                return new NpcExchangePool(
                    archetype.NpcExchangeChance,
                    archetype.NpcExchangeGifts,
                    archetype.NpcVisitFlavors,
                    archetype.NpcExchangeFlavors);
            }

            // Universal spec — flavor only, no item exchange.
            if (universalSpecFlavors.TryGetValue(specId, out var flavors))
                return new NpcExchangePool(0f, null, flavors.visitFlavors, flavors.exchangeFlavors);

            return null;
        }

        private List<DevelopmentStage> BuildPostSpecStages(PlaceArchetype archetype)
        {
            var stages = new List<DevelopmentStage>();
            if (archetype.NpcPostSpecStages == null || archetype.NpcPostSpecStages.Length == 0)
                return stages;

            var item1 = archetype.CommonYields?.Length > 0 ? archetype.CommonYields[0].Item : null;
            var item2 = archetype.RareYield?.Item;

            foreach (var def in archetype.NpcPostSpecStages)
            {
                var item = def.useRareItem ? item2 : item1;
                if (item == null) continue;

                stages.Add(new DevelopmentStage(def.stageId, def.displayName, def.progressCost,
                    new SpecializationRealizedCondition(archetype.SpecializationId,
                        $"needs a {archetype.NpcTitle} in town"),
                    new ItemAvailableCondition(item, def.itemCount)));
            }

            return stages;
        }

        private static int PickWeightedGiftIndex(ItemYield[] gifts)
        {
            float total = 0f;
            foreach (var g in gifts) total += Mathf.Max(0f, g.Weight);
            if (total <= 0f) return gifts.Length - 1;

            float roll = Random.value * total;
            float cumulative = 0f;
            for (int i = 0; i < gifts.Length; i++)
            {
                cumulative += Mathf.Max(0f, gifts[i].Weight);
                if (roll <= cumulative) return i;
            }
            return gifts.Length - 1;
        }

        private static string PickRandom(string[] lines)
        {
            if (lines == null || lines.Length == 0) return null;
            return lines[Random.Range(0, lines.Length)];
        }

        // Holds per-spec visit and exchange data built at specialization time.
        private class NpcExchangePool
        {
            public readonly float exchangeChance;
            public readonly ItemYield[] gifts;
            public readonly string[] visitFlavors;
            public readonly string[] exchangeFlavors;

            public NpcExchangePool(float exchangeChance, ItemYield[] gifts,
                string[] visitFlavors, string[] exchangeFlavors)
            {
                this.exchangeChance = exchangeChance;
                this.gifts = gifts;
                this.visitFlavors = visitFlavors;
                this.exchangeFlavors = exchangeFlavors;
            }
        }
    }
}
