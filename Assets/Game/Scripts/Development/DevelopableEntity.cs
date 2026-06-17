using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mossmark.Development
{
    // Base class for Building/NPC/POI/Town (generalizes P1's Entity/TownEntity).
    public abstract class DevelopableEntity : MonoBehaviour
    {
        private readonly HashSet<string> appliedStageIds = new();

        public abstract string DisplayName { get; }
        protected abstract DevelopmentTrack Track { get; }

        public int PendingProgress { get; private set; }
        public int CurrentStageIndex { get; private set; } = -1;
        public DevelopmentStage LastAppliedStage { get; private set; }

        // Whether this tick's dependencies were satisfied (progress was productive),
        // regardless of whether a stage applied. Drives RequiresDaylight.
        public bool LastAttentionMadeProgress { get; private set; }

        // Whether this tick was the one that crossed a stage threshold. A stage-crossing
        // tick is also the hold's last tick (Development Application's interrupt rule),
        // so ongoing attendables read this alongside LastAttentionMadeProgress for
        // ContinueAttending.
        public bool LastAttentionAppliedStage { get; private set; }

        public event Action<DevelopmentStage> OnDeveloped;

        public void AddProgress(int amount = 1) => PendingProgress += amount;

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
                LastAttentionMadeProgress = true;
                LastAttentionAppliedStage = true;
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
