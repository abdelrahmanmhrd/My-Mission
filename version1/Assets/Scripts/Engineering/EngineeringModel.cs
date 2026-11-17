using System;
using System.Collections.Generic;
using SpaceMission.Data;
using SpaceMission.State;

namespace SpaceMission.Engineering
{
    public class CostBreakdown
    {
        public float vehicle;
        public float launchpad;
        public float spacecraft;
        public float fuel;
        public float instruments;
        public float total;
    }

    public class StageEvaluation
    {
        public VehicleStage stage;
        public StageSelection selection;
        public FuelData fuel;
        public List<EngineData> engines = new List<EngineData>();
        public float effectiveIspS;
        public float totalThrustKN;
        public float totalMassKg;         // stage dry + fixed boosters + fuel + engines
        public float payloadAboveKg;      // everything this stage carries at burn start
        public float achievableDvMS;      // delta-v this stage can produce with its selected fuel
        public bool valid;
    }

    public class DeltaVPhaseResult
    {
        public string phase;
        public float requiredMS;
        public float achievedMS;
        public bool sufficient;
    }

    public class EngineeringResult
    {
        public CostBreakdown cost = new CostBreakdown();
        public float payloadMassKg;
        public float payloadCapacityKg;
        public bool withinPayloadCapacity = true;
        public float liftoffMassKg;
        public float liftoffThrustKN;
        public float twr;
        public bool twrSufficient = true;
        public List<StageEvaluation> stages = new List<StageEvaluation>();
        public List<DeltaVPhaseResult> deltaV = new List<DeltaVPhaseResult>();
        public float departureDvMS;
        public float spacecraftArrivalDvMS;
        public float spacecraftReturnDvMS;
        public float scienceValue;
        public float powerW;
        public float powerGenerationW;
        public float dataGbPerDay;
        public float commCapacityGbPerDay;
        public float overBudgetUSD;
        public float overPayloadKg;
    }

    /// Deterministic engineering calculations. All physics in real SI units;
    /// display conversion to game units happens in the UI layer only (GDD 8.4/15.1).
    ///
    /// Model notes (documented simplifications):
    /// - Stage dryMassKg includes the stage structure and its standard engine
    ///   complement; swapping engine selections does not change stage mass
    ///   (gameplay approximation).
    /// - Achievable delta-v per stage uses the ideal rocket equation with the
    ///   vacuum Isp scaled by the fuel's ispMultiplier.
    public static class EngineeringModel
    {
        public const float G0 = 9.80665f;
        public const float MinTwr = 1.05f;

        public static float EffectiveIsp(EngineData engine, FuelData fuel) =>
            engine == null || fuel == null ? 0f : engine.ispVacS * fuel.ispMultiplier;

        public static EngineeringResult Evaluate(MissionData mission, MissionConfig config, GameDatabase db)
        {
            var result = new EngineeringResult();
            if (mission == null || config == null || db == null) return result;

            var vehicle = config.Vehicle(db);
            var spacecraft = config.Spacecraft(db);
            var pad = config.Launchpad(db);
            var capacityT = mission.capacityMetric == "TMI" ? vehicle?.payloadTMIT ?? 0 : vehicle?.payloadTLIT ?? 0;

            // --- Payload mass: spacecraft + selected instruments ---
            var instrumentMassKg = 0f;
            result.scienceValue = 0f;
            result.powerW = 0f;
            result.dataGbPerDay = 0f;
            foreach (var instId in config.instrumentIds)
            {
                var inst = db.GetInstrument(instId);
                if (inst == null) continue;
                instrumentMassKg += inst.massKg;
                result.scienceValue += inst.scienceValue;
                result.powerW += inst.powerW;
                result.dataGbPerDay += inst.dataGbPerDay;
            }
            result.powerGenerationW = spacecraft?.powerGenerationW ?? 0f;
            result.commCapacityGbPerDay = spacecraft?.commCapacityGbPerDay ?? 0f;

            result.payloadMassKg = (spacecraft?.massT ?? 0) * 1000f + instrumentMassKg;
            result.payloadCapacityKg = capacityT * 1000f;
            result.withinPayloadCapacity = result.payloadMassKg <= result.payloadCapacityKg + 0.001f;
            result.overPayloadKg = MathfMax(result.payloadMassKg - result.payloadCapacityKg);

            // --- Cost ---
            result.cost.vehicle = vehicle?.costUSD ?? 0;
            result.cost.launchpad = pad?.costUSD ?? 0;
            result.cost.spacecraft = spacecraft?.costUSD ?? 0;
            foreach (var stageEval in EvaluateStages(vehicle, config, db, result.payloadMassKg))
            {
                result.stages.Add(stageEval);
                if (stageEval.fuel != null)
                    result.cost.fuel += stageEval.selection.fuelAmountFU * stageEval.fuel.costPerFU;
            }

            foreach (var instId in config.instrumentIds)
            {
                var inst = db.GetInstrument(instId);
                if (inst != null) result.cost.instruments += inst.costUSD;
            }

            result.cost.total = result.cost.vehicle + result.cost.launchpad +
                                result.cost.spacecraft + result.cost.fuel + result.cost.instruments;
            result.overBudgetUSD = MathfMax(result.cost.total - mission.budgetUSD);

            // --- Liftoff mass and thrust-to-weight ---
            float liftoffMassKg = 0f;
            float liftoffThrustKN = 0f;
            if (result.stages.Count > 0)
            {
                // stage 0 mass + everything above it = the full stack at ignition
                var first = result.stages[0];
                liftoffMassKg = first.totalMassKg + first.payloadAboveKg + first.stage.fixedBoosterMassKg;
                liftoffThrustKN = first.totalThrustKN + first.stage.fixedBoosterThrustKN;
            }
            result.liftoffMassKg = liftoffMassKg;
            result.liftoffThrustKN = liftoffThrustKN;
            result.twr = liftoffMassKg > 0f ? liftoffThrustKN * 1000f / (liftoffMassKg * G0) : 0f;
            result.twrSufficient = result.twr >= MinTwr;

            // --- Departure delta-v: all vehicle stages burn in sequence ---
            result.departureDvMS = SumStageDeltaV(result.stages);

            // --- Spacecraft capability for arrival/return (precomputed real values) ---
            result.spacecraftArrivalDvMS = spacecraft?.arrivalDvMS ?? 0f;
            result.spacecraftReturnDvMS = spacecraft?.returnDvMS ?? 0f;

            // --- Mission delta-v phases ---
            foreach (var phase in mission.deltaV)
            {
                var achieved = phase.scope == "vehicle"
                    ? result.departureDvMS
                    : (phase.phase == "return" ? result.spacecraftReturnDvMS : result.spacecraftArrivalDvMS);
                var pr = new DeltaVPhaseResult
                {
                    phase = phase.phase,
                    requiredMS = phase.dvMS,
                    achievedMS = achieved,
                    sufficient = achieved + 0.001f >= phase.dvMS
                };
                result.deltaV.Add(pr);
            }

            return result;
        }

        public static List<StageEvaluation> EvaluateStages(VehicleData vehicle, MissionConfig config, GameDatabase db, float payloadMassKg)
        {
            var evals = new List<StageEvaluation>();
            if (vehicle == null || config == null) return evals;

            // Walk stages from the top (last) down: payload above each stage =
            // remaining stages' mass + fuel + the payload itself.
            var massAbove = payloadMassKg;
            for (var i = vehicle.stages.Length - 1; i >= 0; i--)
            {
                var stageData = vehicle.stages[i];
                var selection = i < config.stages.Count ? config.stages[i] : null;

                var eval = new StageEvaluation
                {
                    stage = stageData,
                    selection = selection
                };

                float fuelMassKg = 0f;
                float tankMassKg = 0f;
                if (selection != null)
                {
                    eval.fuel = db.GetFuel(selection.fuelId);
                    fuelMassKg = selection.fuelAmountFU * 1000f;   // 1 FU = 1 tonne
                    tankMassKg = eval.fuel != null ? fuelMassKg * eval.fuel.tankMassFactor : 0f;

                    var slotCount = stageData.boosterSlots + stageData.upperSlots;
                    foreach (var engineId in selection.engineIds)
                    {
                        var engine = db.GetEngine(engineId);
                        if (engine == null) continue;
                        eval.engines.Add(engine);
                        eval.effectiveIspS = MathfMaxF(eval.effectiveIspS, EffectiveIsp(engine, eval.fuel));
                    }

                    if (eval.engines.Count > 0 && eval.effectiveIspS > 0f)
                        eval.valid = true;

                    // total thrust from selected liquid engines
                    foreach (var e in eval.engines)
                        eval.totalThrustKN += (stageData.boosterSlots > 0 ? e.thrustSlKN : e.thrustVacKN);
                }

                var stageStructMass = stageData.dryMassKg + stageData.fixedBoosterMassKg;
                eval.totalMassKg = stageStructMass + tankMassKg + fuelMassKg;
                eval.payloadAboveKg = massAbove;

                // Delta-v with the fuel the player actually loaded
                if (eval.valid && eval.effectiveIspS > 0f && fuelMassKg > 0f)
                {
                    var burnStart = stageStructMass + tankMassKg + fuelMassKg + massAbove;
                    var burnEnd = stageStructMass + tankMassKg + massAbove;
                    eval.achievableDvMS = eval.effectiveIspS * G0 * MathfLog(burnStart / burnEnd);
                }

                evals.Insert(0, eval);
                massAbove += stageStructMass + tankMassKg + fuelMassKg;
            }
            return evals;
        }

        public static float SumStageDeltaV(List<StageEvaluation> stages)
        {
            var sum = 0f;
            foreach (var s in stages) sum += s.achievableDvMS;
            return sum;
        }

        static float MathfMax(float v) => v > 0f ? v : 0f;
        static float MathfMaxF(float a, float b) => a > b ? a : b;
        static float MathfLog(float v) => v > 0f ? (float)Math.Log(v) : 0f;
    }
}
