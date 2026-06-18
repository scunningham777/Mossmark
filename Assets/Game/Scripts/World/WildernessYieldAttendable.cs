using Mossmark.Attention;
using Mossmark.Development;
using UnityEngine;

namespace Mossmark.World
{
    // Shared base class for GenericWildernessSpotAttendable and PoiAttendable (G7). Both
    // had identical fields, tick logic, and IAttendable plumbing — only their overlay
    // text and gate logic differ. Subclasses implement the three abstract overlay methods
    // and may override GetEffectiveRareChance() to bias the drop chance with modifiers.
    public abstract class WildernessYieldAttendable : MonoBehaviour, IAttendable
    {
        [SerializeField] protected string displayName = "Spot";
        [SerializeField] protected string interactionVerb = "forage";
        [SerializeField] protected ItemYield[] commonYields;
        [SerializeField] protected ItemYield rareYield;
        [SerializeField, Range(0f, 1f)] protected float rareDropChance = 0.08f;

        [SerializeField, Min(0.1f)] protected float minTickInterval = 1.5f;
        [SerializeField, Min(0.1f)] protected float maxTickInterval = 2f;

        protected bool continueAttending;
        protected float currentTickInterval;
        protected string foundVerb = "foraged";

        protected void InitializeBase(string displayName, string interactionVerb,
            ItemYield[] commonYields, ItemYield rareYield, float rareDropChance,
            float minTickInterval, float maxTickInterval)
        {
            this.displayName = displayName;
            this.interactionVerb = interactionVerb;
            this.commonYields = commonYields;
            this.rareYield = rareYield;
            this.rareDropChance = rareDropChance;
            this.minTickInterval = minTickInterval;
            this.maxTickInterval = maxTickInterval;
        }

        protected virtual void Awake()
        {
            RollTickInterval();
        }

        public float AttentionDuration => currentTickInterval;
        public bool RequiresDaylight => true;
        public bool ContinueAttending => continueAttending;

        public abstract bool CanAttend();
        public abstract string GetOverlayDescription();
        public abstract string GetOverlayInteractionLine();

        public void OnAttentionComplete()
        {
            continueAttending = true;
            ItemYieldRoller.Roll(displayName, foundVerb, commonYields, rareYield, GetEffectiveRareChance());
            RollTickInterval();
        }

        public void OnAttentionCancelled() { }

        // Subclasses that apply outcome modifiers (e.g. TwilightChanceModifier) override this.
        // PoiAttendable uses the base rareDropChance unchanged — POIs are distinctive
        // rare-access locations, not ambient foraging spots.
        protected virtual float GetEffectiveRareChance() => rareDropChance;

        protected void RollTickInterval()
        {
            currentTickInterval = Random.Range(minTickInterval, maxTickInterval);
        }
    }
}
