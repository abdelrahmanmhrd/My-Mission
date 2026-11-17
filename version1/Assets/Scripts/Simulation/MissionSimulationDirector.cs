using System;
using System.Collections;
using UnityEngine;
using SpaceMission.Core;
using SpaceMission.Engineering;

namespace SpaceMission.Simulation
{
    public enum SimPhase { LaunchPad, Ascent, OrbitAchieved, Cruise, Arrival, Complete, Failure }

    /// Drives the deterministic mission simulation flow. The engineering layer has
    /// already decided the outcome (GDD 15.5); this director paces it through the
    /// visual phases and keeps telemetry synchronized with the model.
    public class MissionSimulationDirector : MonoBehaviour
    {
        public static MissionSimulationDirector Instance { get; private set; }

        public SimPhase phase = SimPhase.LaunchPad;
        public event Action<SimPhase> PhaseChanged;
        public event Action<MissionOutcome> Finished;

        ValidationReport report;
        MissionOutcome outcome;
        float dvDepartureAchieved;
        float dvDepartureRequired;

        public float AscentDuration = 35f;   // GDD: compressed ascent 30–45 s
        public float failureAtAltitudeFraction;   // where the flame-out happens during ascent if underfueled

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Begin(ValidationReport validationReport)
        {
            report = validationReport;
            outcome = ScoringModel.EvaluateOutcome(GameState.CurrentMission, GameState.Config, report.engineering);
            dvDepartureAchieved = report.engineering.departureDvMS;
            dvDepartureRequired = 0f;
            foreach (var dv in report.engineering.deltaV)
                if (dv.phase == "departure") dvDepartureRequired = dv.requiredMS;
            // flame-out point: proportional shortfall — a config that only half-satisfies
            // the departure budget fails roughly halfway through ascent.
            failureAtAltitudeFraction = dvDepartureRequired > 0f
                ? Mathf.Clamp01(dvDepartureAchieved / dvDepartureRequired)
                : 1f;
        }

        public void SetPhase(SimPhase next)
        {
            phase = next;
            PhaseChanged?.Invoke(next);
        }

        /// Called by the launch simulation when ascent completes (or fails).
        public void OnAscentEnded(bool reachedOrbit)
        {
            if (!reachedOrbit)
            {
                SetPhase(SimPhase.Failure);
                Finished?.Invoke(outcome);
                return;
            }
            SetPhase(SimPhase.OrbitAchieved);
            if (outcome.success && outcome.phaseReached == MissionPhase.Complete)
            {
                SetPhase(SimPhase.Complete);
                Finished?.Invoke(outcome);
            }
            else
            {
                SetPhase(SimPhase.Cruise);
                Finished?.Invoke(outcome);
            }
        }
    }
}
