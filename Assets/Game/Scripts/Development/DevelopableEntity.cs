using System;
using System.Collections.Generic;
using Mossmark.Attention;
using Mossmark.Visuals;
using UnityEngine;

namespace Mossmark.Development
{
    // Base class for Building/NPC/POI/Town (generalizes P1's Entity/TownEntity).
    public abstract class DevelopableEntity : MonoBehaviour
    {
        private readonly HashSet<string> appliedStageIds = new();
        // Ordered record of stages actually applied via TryApplyStage (excludes stages
        // sealed via MarkStageAsApplied, which are suppressed pool entries, not achievements).
        private readonly List<DevelopmentStage> appliedStagesInOrder = new();

        public abstract string DisplayName { get; }
        protected abstract DevelopmentTrack Track { get; }

        public int PendingProgress { get; private set; }
        public int CurrentStageIndex { get; private set; } = -1;
        public DevelopmentStage LastAppliedStage { get; private set; }

        // Iteration 29: maintenance drift. Incremented by MaintenanceManager each rest
        // for entities that have crossed at least one development threshold. Reset by
        // direct attend-with-material or silent chest consumption.
        public int DriftProgress { get; private set; }

        // Called by MaintenanceManager once per rest for developed entities.
        public void IncrementDrift()
        {
            if (CurrentStageIndex < 0) return;
            DriftProgress++;
            OnDriftChanged();
        }

        // Called by MaintenanceManager (chest consumption) and by subclass OnAttentionComplete
        // (direct tending). Subclasses override OnDriftReset to restore suspended flags.
        public void ResetDrift()
        {
            if (DriftProgress == 0) return;
            DriftProgress = 0;
            OnDriftReset();
        }

        // Subclasses override to react when drift crosses the cold threshold — e.g.
        // NpcAttendable suspends WorldState flags.
        protected virtual void OnDriftChanged() { }

        // Subclasses override to react when drift resets — e.g. NpcAttendable restores flags.
        protected virtual void OnDriftReset() { }

        // Returns the entity description including an appropriate drift suffix, for use
        // in IAttendable.GetOverlayDescription() overrides.
        //   Warning (>= 60% of threshold): appends "— needs tending"
        //   Cold (>= threshold): returns coldFlavor (or a generic fallback)
        //   No drift: returns displayName unchanged
        protected string GetDriftOverlayDescription(string displayName, int driftThreshold, string coldFlavor)
        {
            if (DriftProgress <= 0) return displayName;
            if (DriftProgress >= driftThreshold)
                return !string.IsNullOrEmpty(coldFlavor)
                    ? coldFlavor
                    : $"{displayName} — seems neglected";
            if (DriftProgress >= Mathf.RoundToInt(driftThreshold * 0.6f))
                return $"{displayName} — needs tending";
            return displayName;
        }

        // Whether this tick's dependencies were satisfied (progress was productive),
        // regardless of whether a stage applied. Drives RequiresDaylight.
        public bool LastAttentionMadeProgress { get; private set; }

        // Whether this tick was the one that crossed a stage threshold. A stage-crossing
        // tick is also the hold's last tick (Development Application's interrupt rule),
        // so ongoing attendables read this alongside LastAttentionMadeProgress for
        // ContinueAttending.
        public bool LastAttentionAppliedStage { get; private set; }

        public event Action<DevelopmentStage> OnDeveloped;

        // Fired from ResolveAttention() whenever a tick made productive progress
        // (dependencies satisfied, progress advanced), whether or not a stage crossed.
        // Does NOT fire when dependencies are unsatisfied (no-progress tick).
        public event Action OnProgressMade;

        public void AddProgress(int amount = 1) => PendingProgress += amount;

        // Lets a subclass raise the progress-tick feedback signal directly when its own
        // "productive tick" doesn't fit ResolveAttention's dependency-gated model — e.g.
        // wilderness spots (Iteration 43), where yields happen every attend regardless of
        // whether a pending stage's own gate is currently satisfied.
        protected void RaiseProgressMade() => OnProgressMade?.Invoke();

        public List<DevelopmentStage> GetAvailableStages()
        {
            var available = new List<DevelopmentStage>();

            foreach (var stage in Track.Stages)
            {
                if (appliedStageIds.Contains(stage.Id)) continue;
                if (stage.AreDependenciesSatisfied(this)) available.Add(stage);
            }

            return available;
        }

        public bool TryApplyStage()
        {
            var available = GetAvailableStages();
            available.RemoveAll(stage => PendingProgress < stage.ProgressCost);

            if (available.Count == 0) return false;

            // Building -> NPC demand (Development Application): if any available stage
            // answers a currently-declared specialization need, the draw is restricted
            // to those stages rather than the full candidate pool.
            var needed = available.FindAll(stage => DeclaredSpecializationNeeds.Contains(stage.Id));
            if (needed.Count > 0) available = needed;

            var chosen = available[UnityEngine.Random.Range(0, available.Count)];
            PendingProgress -= chosen.ProgressCost;
            appliedStageIds.Add(chosen.Id);
            appliedStagesInOrder.Add(chosen);
            CurrentStageIndex++;
            LastAppliedStage = chosen;
            DeclaredSpecializationNeeds.Consume(chosen.Id);
            OnDeveloped?.Invoke(chosen);
            return true;
        }

        // Protected so NpcAttendable can detect "fully developed" (null) for overlay text.
        protected DevelopmentStage GetNextStage()
        {
            foreach (var stage in Track.Stages)
            {
                if (!appliedStageIds.Contains(stage.Id)) return stage;
            }

            return null;
        }

        // Lets subclasses seal pool stages that were not drawn in a random pick,
        // so GetNextStage() advances past them to the drawn specialization's own stages.
        public void MarkStageAsApplied(string stageId) => appliedStageIds.Add(stageId);

        // Display names of all stages that were genuinely applied (sealed/suppressed pool
        // entries excluded). Used by the detail overlay UI to list entity developments.
        public IReadOnlyList<string> GetAppliedUpgradeNames() =>
            appliedStagesInOrder.ConvertAll(s => s.DisplayName);

        // Reverse of MarkStageAsApplied: removes from the sealed set so GetNextStage()
        // can reach stages that were pre-sealed to prevent premature firing.
        public void MarkStageAsAvailable(string stageId) => appliedStageIds.Remove(stageId);

        // True if attending right now would be productive: the next not-yet-applied
        // stage exists and its dependencies are currently satisfied. Single-stage
        // entities (Old Cairn, Buildings, Watcher's Post) gate CanAttend() on this so a
        // blocked attend never enters the Attending hold in the first place.
        public bool CanMakeProgress()
        {
            var stage = GetNextStage();
            return stage != null && stage.AreDependenciesSatisfied(this);
        }

        // Logs each authored condition's current satisfied/unsatisfied state for the
        // next not-yet-applied stage - the iteration 8 "reports ... via debug output" deliverable.
        public void LogDependencyReport()
        {
            var stage = GetNextStage();
            if (stage == null)
            {
                Debug.Log($"{DisplayName}: fully developed.", this);
                return;
            }

            foreach (var (condition, satisfied) in stage.EvaluateDependencies(this))
            {
                Debug.Log($"{DisplayName} - {stage.DisplayName}: {condition.GetNeedsDescription(this)} -> {(satisfied ? "satisfied" : "unsatisfied")}", this);
            }
        }

        // The generic 3-step resolver from PROTOTYPE2.md: dependencies satisfied -> produce
        // progress/apply a stage (daylight consumed); otherwise surface "needs" (daylight not
        // consumed). Results are read afterwards via LastAttentionMadeProgress/
        // LastAttentionAppliedStage, same read-after-complete pattern as TendedSpotAttendable.
        public bool ResolveAttention()
        {
            AddProgress();

            if (TryApplyStage())
            {
                Debug.Log($"{DisplayName}: developed - {LastAppliedStage.DisplayName}!", this);
                NotificationManager.Post($"{DisplayName}: {LastAppliedStage.DisplayName}");
                LastAttentionMadeProgress = true;
                LastAttentionAppliedStage = true;
                OnProgressMade?.Invoke();
                return true;
            }

            LastAttentionAppliedStage = false;

            var nextStage = GetNextStage();
            if (nextStage != null && !nextStage.AreDependenciesSatisfied(this))
            {
                LastAttentionMadeProgress = false;
                return false;
            }

            Debug.Log($"{DisplayName}: progress {PendingProgress}.", this);
            LastAttentionMadeProgress = true;
            OnProgressMade?.Invoke();
            return true;
        }

        public string GetNeedsOrDefault(string defaultLine)
        {
            var stage = GetNextStage();
            if (stage == null) return defaultLine;

            var needs = stage.GetUnsatisfiedNeedsDescription(this);
            return needs != null ? $"{DisplayName} {needs}" : defaultLine;
        }
    }
}
