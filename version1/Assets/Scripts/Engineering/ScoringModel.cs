using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using SpaceMission.Data;
using SpaceMission.State;

namespace SpaceMission.Engineering
{
    /// Deterministic mission verdict + scoring (GDD 11/12).
    /// Same mission + same config always produce the same outcome and score.
    public enum MissionPhase { Launch, Departure, Cruise, Arrival, SurfaceOps, Return, Complete }

    public class MissionOutcome
    {
        public bool success;
        public MissionPhase phaseReached = MissionPhase.Launch;
        public string failureReason = "";      // player-facing: what went wrong
        public string lesson = "";             // player-facing: what to change next time
        public List<string> lessons = new List<string>();
    }

    public class ScoreBreakdown
    {
        public float missionSuccess;      // /35
        public float scientificValue;     // /25
        public float resourceEfficiency;  // /20
        public float engineeringReliability; // /20
        public float total;               // /100
        public string grade;
        public string rank;               // game-like rank medal

        /// Integer-consistent display: the parts sum EXACTLY to the displayed total
        /// (fixes the "score doesn't add up" bug — 35+25+1+18 must equal the total shown).
        public int SuccessPoints => Mathf.RoundToInt(missionSuccess);
        public int SciencePoints => Mathf.RoundToInt(scientificValue);
        public int EfficiencyPoints => Mathf.RoundToInt(resourceEfficiency);
        public int ReliabilityPoints => Mathf.RoundToInt(engineeringReliability);
        public int TotalPoints => SuccessPoints + SciencePoints + EfficiencyPoints + ReliabilityPoints;
    }

    public static class ScoringModel
    {
        /// Produces the deterministic outcome from the validated engineering state.
        /// (When the 3D simulation exists it visualizes this verdict and can extend it
        /// with phase-specific events, but the base verdict stays deterministic.)
        public static MissionOutcome EvaluateOutcome(MissionData mission, MissionConfig config, EngineeringResult engineering)
        {
            var outcome = new MissionOutcome();

            if (engineering == null) return outcome;

            // spacecraft capability shortfalls end the mission at the phase where they bite
            foreach (var dv in engineering.deltaV)
            {
                if (dv.sufficient) continue;
                outcome.success = false;
                switch (dv.phase)
                {
                    case "departure":
                        outcome.phaseReached = MissionPhase.Departure;
                        outcome.failureReason = $"The launch vehicle ran out of capability during ascent/departure: it needed {dv.requiredMS:0} m/s but could only produce {dv.achievedMS:0} m/s.";
                        outcome.lesson = "Load more fuel into your stages, choose more efficient engines, or carry a lighter payload. Remember every extra tonne costs fuel.";
                        break;
                    case "arrival":
                        outcome.phaseReached = MissionPhase.Arrival;
                        outcome.failureReason = $"The spacecraft cannot brake at the destination: it needs {dv.requiredMS:0} m/s but can only produce {dv.achievedMS:0} m/s.";
                        outcome.lesson = "Choose a spacecraft whose arrival capability covers the mission's arrival burn, or reduce the payload mass.";
                        break;
                    case "return":
                        outcome.phaseReached = MissionPhase.Return;
                        outcome.failureReason = $"The spacecraft cannot come home: it needs {dv.requiredMS:0} m/s for the return burns but can only produce {dv.achievedMS:0} m/s.";
                        outcome.lesson = "Return capability is non-negotiable for crewed missions — a spacecraft that cannot come back loses its crew.";
                        break;
                    default:
                        outcome.phaseReached = MissionPhase.Cruise;
                        outcome.failureReason = $"Insufficient capability for the {dv.phase} burn.";
                        break;
                }
                outcome.lessons.Add(outcome.lesson);
                return outcome;
            }

            // fully capable configuration completes the objective
            outcome.success = true;
            outcome.phaseReached = MissionPhase.Complete;
            return outcome;
        }

        /// GDD 12.1 provisional weights: success 35 / science 25 / efficiency 20 / reliability 20.
        public static ScoreBreakdown Score(MissionData mission, MissionConfig config, EngineeringResult engineering, MissionOutcome outcome)
        {
            var s = new ScoreBreakdown();

            // 1. Mission success (35): binary — the objective was achieved
            s.missionSuccess = outcome != null && outcome.success ? 35f : 0f;

            // 2. Scientific value (25): science achieved vs the mission requirement
            var required = mission != null ? mission.minScienceValue : 0f;
            var science = engineering?.scienceValue ?? 0f;
            s.scientificValue = required > 0f ? 25f * Clamp01(science / required) : 25f;

            // 3. Resource efficiency (20): unspent budget fraction
            var budget = mission != null ? mission.budgetUSD : 0f;
            var spent = engineering?.cost.total ?? 0f;
            s.resourceEfficiency = budget > 0f ? 20f * Clamp01((budget - spent) / budget) : 0f;

            // 4. Engineering reliability (20): performance margin over requirements.
            //    Full marks at 25% dv margin or more, zero at exactly meeting them.
            var margin = MinDvMargin(engineering);
            s.engineeringReliability = 20f * Clamp01((margin - 1f) / 0.25f);

            s.total = s.missionSuccess + s.scientificValue + s.resourceEfficiency + s.engineeringReliability;
            s.grade = GradeFor(s.total, outcome != null && outcome.success);
            s.rank = RankFor(s.TotalPoints, outcome != null && outcome.success);
            return s;
        }

        /// Game-like rank medal (teen-friendly): S / A / B / C / D.
        static string RankFor(int points, bool success)
        {
            if (!success) return "MISSION FAILED";
            if (points >= 95) return "S — LEGENDARY DIRECTOR";
            if (points >= 85) return "A — OUTSTANDING";
            if (points >= 70) return "B — SOLID WORK";
            if (points >= 55) return "C — PASSED";
            return "D — BARELY MADE IT";
        }

        static float MinDvMargin(EngineeringResult engineering)
        {
            if (engineering == null || engineering.deltaV == null || engineering.deltaV.Count == 0)
                return 1f;
            var min = float.MaxValue;
            foreach (var dv in engineering.deltaV)
            {
                if (dv.requiredMS <= 0f) continue;
                min = Math.Min(min, dv.achievedMS / dv.requiredMS);
            }
            return min == float.MaxValue ? 1f : min;
        }

        static float Clamp01(float v) => v < 0f ? 0f : v > 1f ? 1f : v;

        static string GradeFor(float total, bool success)
        {
            if (!success) return total >= 60f ? "C (mission failed)" : "D (mission failed)";
            if (total >= 90f) return "A";
            if (total >= 75f) return "B";
            if (total >= 60f) return "C";
            return "D";
        }

        /// Player-facing expenditure/summary text for the debrief.
        public static string BudgetSummary(MissionData mission, EngineeringResult engineering)
        {
            if (mission == null || engineering == null) return "";
            var spentM = engineering.cost.total / 1e6f;
            var budgetM = mission.budgetUSD / 1e6f;
            var sb = new StringBuilder();
            sb.AppendLine($"Vehicle: {engineering.cost.vehicle / 1e6f:0}M   ·   Launch facility: {engineering.cost.launchpad / 1e6f:0}M");
            sb.AppendLine($"Spacecraft: {engineering.cost.spacecraft / 1e6f:0}M   ·   Fuel: {engineering.cost.fuel / 1e6f:0.0}M   ·   Instruments: {engineering.cost.instruments / 1e6f:0}M");
            sb.AppendLine($"Total spent: {spentM:0}M of {budgetM:0}M budget ({100f * Clamp01(spentM / budgetM):0}% used)");
            return sb.ToString();
        }
    }
}
