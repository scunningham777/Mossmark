using System.Collections.Generic;
using Mossmark.Attention;
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
    public class NpcAttendable : DevelopableEntity, IAttendable
    {
        [SerializeField] private string genericName = "Wanderer";
        [SerializeField, Min(1)] private int progressCost = 8;
        [SerializeField, Min(0.1f)] private float minTickInterval = 1f;
        [SerializeField, Min(0.1f)] private float maxTickInterval = 1.5f;

        private DevelopmentTrack track;
        private SpriteRenderer spriteRenderer;
        private string specializedName;
        private string drawnSpecializationId;
        private List<string> poolStageIds;
        private Dictionary<string, List<string>> postSpecStageIdsBySpecId;
        private Dictionary<string, (string Title, Color Tint)> specializationInfo;
        private Dictionary<string, NpcPostSpecStageDef> postSpecStageDefs;
        private float currentTickInterval;

        // Iteration 28.5: exchange pool set when specialization fires.
        private NpcExchangePool drawnExchangePool;
        private bool lastAttentionWasVisit;

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

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => lastAttentionWasVisit || LastAttentionMadeProgress;
        public bool ContinueAttending => !lastAttentionWasVisit && LastAttentionMadeProgress && !LastAttentionAppliedStage;

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

        public bool CanAttend()
        {
            if (drawnSpecializationId == null) return true;
            if (GetNextStage() == null) return true; // fully developed — still visitable
            return CanMakeProgress();
        }

        public string GetOverlayDescription() => DisplayName;

        public string GetOverlayInteractionLine()
        {
            if (drawnSpecializationId == null)
                return GetNeedsOrDefault($"Hold E to spend time with {genericName} - they need more time to find their place");

            if (GetNextStage() == null)
                return $"Hold E to visit with {specializedName}";

            return GetNeedsOrDefault($"Hold E to help {specializedName} develop their craft further");
        }

        public void OnAttentionComplete()
        {
            lastAttentionWasVisit = false;

            if (drawnSpecializationId != null && GetNextStage() == null)
            {
                lastAttentionWasVisit = true;
                RunVisitInteraction();
                currentTickInterval = RollTickInterval();
                return;
            }

            ResolveAttention();
            currentTickInterval = RollTickInterval();
        }

        public void OnAttentionCancelled() { }

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
                    WorldState.SetFlag(stageDef.worldStateFlag, true);
                Debug.Log($"{specializedName}: {stageDef.flavorText}", this);
            }
            else
            {
                WorldState.SetFlag(stage.Id, true);
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
