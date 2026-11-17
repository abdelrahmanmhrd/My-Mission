using System.Collections.Generic;
using SpaceMission.Data;
using SpaceMission.State;

namespace SpaceMission.Engineering
{
    public enum ValidationSeverity { Error, Warning, Info }

    public class ValidationIssue
    {
        public string code;
        public ValidationSeverity severity;
        public string message;   // what is wrong (player-facing)
        public string hint;      // what to do about it (player-facing)

        public ValidationIssue(string code, ValidationSeverity severity, string message, string hint = "")
        {
            this.code = code;
            this.severity = severity;
            this.message = message;
            this.hint = hint;
        }
    }

    public enum ReadinessState { NotReady, ValidationRequired, ReadyForLaunch }

    public class ValidationReport
    {
        public ReadinessState readiness;
        public List<ValidationIssue> issues = new List<ValidationIssue>();
        public EngineeringResult engineering;

        public bool CanLaunch => readiness == ReadinessState.ReadyForLaunch;
        public bool HasErrors => issues.Exists(i => i.severity == ValidationSeverity.Error);
    }

    /// Deterministic validation (GDD 15.4): same mission + same config always
    /// produces the same issues, each with a player-facing reason.
    public static class ValidationEngine
    {
        public static ValidationReport Validate(MissionData mission, MissionConfig config, GameDatabase db)
        {
            var report = new ValidationReport();
            if (mission == null || config == null || db == null)
            {
                report.readiness = ReadinessState.NotReady;
                report.issues.Add(new ValidationIssue("NO_MISSION", ValidationSeverity.Error, "No mission loaded."));
                return report;
            }

            report.engineering = EngineeringModel.Evaluate(mission, config, db);

            var anyErrors = false;

            // --- Category completeness ---
            var vehicle = config.Vehicle(db);
            var spacecraft = config.Spacecraft(db);
            var pad = config.Launchpad(db);

            if (vehicle == null)
                Add(ref anyErrors, report, "MISSING_VEHICLE", "No launch vehicle selected.", "Open the ROCKETS card and choose a vehicle.");
            if (spacecraft == null)
                Add(ref anyErrors, report, "MISSING_SPACECRAFT", "No spacecraft selected.", "Open the ROCKETS card and choose a spacecraft.");
            if (pad == null)
                Add(ref anyErrors, report, "MISSING_PAD", "No launch facility selected.", "Open the LAUNCHPADS card and choose a facility.");

            // --- Stage selections ---
            if (vehicle != null)
            {
                for (var i = 0; i < vehicle.stages.Length; i++)
                {
                    var stageData = vehicle.stages[i];
                    var selection = i < config.stages.Count ? config.stages[i] : null;
                    var label = $"{vehicle.name} / {stageData.stageName}";

                    if (selection == null || selection.engineIds.Count == 0)
                    {
                        Add(ref anyErrors, report, "MISSING_ENGINE", $"{label}: no engines assigned.", $"Open the ENGINES card and assign {stageData.boosterSlots + stageData.upperSlots} engine(s) to {stageData.stageName}.");
                        continue;
                    }

                    // slot count
                    if (selection.engineIds.Count != stageData.boosterSlots + stageData.upperSlots)
                        Add(ref anyErrors, report, "ENGINE_COUNT", $"{label}: expected {stageData.boosterSlots + stageData.upperSlots} engine(s), found {selection.engineIds.Count}.", "Assign exactly one engine per stage slot.");

                    var roleNeeded = stageData.boosterSlots > 0 ? "booster" : "upper";
                    foreach (var engineId in selection.engineIds)
                    {
                        var engine = db.GetEngine(engineId);
                        if (engine == null)
                        {
                            Add(ref anyErrors, report, "ENGINE_UNKNOWN", $"{label}: unknown engine assignment.", "Reassign the engine.");
                            continue;
                        }

                        if (engine.stageRole != roleNeeded && engine.stageRole != "any")
                            Add(ref anyErrors, report, "ENGINE_ROLE_MISMATCH", $"{label}: {engine.name} cannot be used on a {roleNeeded} stage.", $"Choose a {roleNeeded}-stage engine for {stageData.stageName}.");

                        if (engine.compatibleVehicles != null && engine.compatibleVehicles.Length > 0 &&
                            !System.Array.Exists(engine.compatibleVehicles, v => v == vehicle.id))
                            Add(ref anyErrors, report, "ENGINE_VEHICLE_INCOMPAT", $"{label}: {engine.name} is not compatible with {vehicle.name}.", "This engine was built for a different vehicle family.");

                        if (stageData.restartable && !engine.restartable)
                            Add(ref anyErrors, report, "ENGINE_NOT_RESTARTABLE", $"{label}: {engine.name} cannot restart in flight, but {stageData.stageName} must burn twice on this mission profile.", "Select a restartable engine for this stage.");

                        if (!string.IsNullOrEmpty(selection.fuelId))
                        {
                            var fuel = db.GetFuel(selection.fuelId);
                            if (fuel != null && engine.fuelId != fuel.id)
                                Add(ref anyErrors, report, "ENGINE_FUEL_INCOMPAT", $"{label}: {engine.name} does not burn {fuel.name}.", $"Pick a fuel the engine supports, or an engine that burns {fuel.name}.");
                        }
                    }

                    if (string.IsNullOrEmpty(selection.fuelId))
                        Add(ref anyErrors, report, "MISSING_FUEL", $"{label}: no fuel type selected.", "Open the FUELS card and choose a propellant for this stage.");
                    else if (selection.fuelAmountFU <= 0f)
                        Add(ref anyErrors, report, "MISSING_FUEL_AMOUNT", $"{label}: no fuel loaded.", "Set a fuel amount for this stage.");
                    else if (selection.fuelAmountFU > stageData.maxFuelFU + 0.001f)
                        Add(ref anyErrors, report, "FUEL_OVER_CAPACITY", $"{label}: {selection.fuelAmountFU:0.#} FU loaded exceeds the {stageData.maxFuelFU:0.#} FU capacity.", "Reduce the fuel amount to fit the tank.");
                }
            }

            // --- Vehicle-level rules ---
            if (vehicle != null)
            {
                if (mission.crewRatedVehicleRequired && !vehicle.crewRated)
                    Add(ref anyErrors, report, "VEHICLE_NOT_CREW_RATED", $"{vehicle.name} is not human-rated, but this mission carries crew.", "Choose a human-rated launch vehicle.");

                if (pad != null && !System.Array.Exists(pad.compatibleVehicles, v => v == vehicle.id))
                    Add(ref anyErrors, report, "PAD_VEHICLE_INCOMPAT", $"{pad.name} cannot launch the {vehicle.name}.", "Select a launch facility built for this vehicle.");
                if (mission.crewedPadRequired && pad != null && !pad.crewInfrastructure)
                    Add(ref anyErrors, report, "PAD_NO_CREW_INFRA", $"{pad.name} has no crew-access infrastructure, but this mission carries astronauts.", "Choose a pad with crew access (LC-39A or LC-39B).");
            }

            // --- Spacecraft rules ---
            if (spacecraft != null)
            {
                var needsCrew = mission.missionType == "crewed";
                if (needsCrew && !spacecraft.crewed)
                    Add(ref anyErrors, report, "SPACECRAFT_TYPE_MISMATCH", $"{spacecraft.name} cannot carry crew, but this is a human-led mission.", "Select a crewed spacecraft.");
                if (!needsCrew && spacecraft.crewed)
                    Add(ref anyErrors, report, "SPACECRAFT_TYPE_MISMATCH", $"{spacecraft.name} is a crewed spacecraft, but this mission is automated.", "Select an automated spacecraft or change the mission type.");

                if (mission.crew > spacecraft.crewCapacity)
                    Add(ref anyErrors, report, "CREW_CAPACITY", $"{spacecraft.name} carries {spacecraft.crewCapacity} crew, but the mission needs {mission.crew}.", "Choose a spacecraft with sufficient crew capacity.");
            }

            if (mission.requiredSpacecraftId != null && mission.requiredSpacecraftId.Length > 0 &&
                config.spacecraftId != mission.requiredSpacecraftId)
            {
                var required = db.GetSpacecraft(mission.requiredSpacecraftId);
                Add(ref anyErrors, report, "SPACECRAFT_NOT_REQUIRED", $"This mission requires the {required?.name ?? mission.requiredSpacecraftId}.", $"Select {required?.name ?? mission.requiredSpacecraftId} as the spacecraft.");
            }

            // --- Hard resource constraints (block launch) ---
            if (!report.engineering.withinPayloadCapacity)
                Add(ref anyErrors, report, "OVER_PAYLOAD", $"Payload ({report.engineering.payloadMassKg / 1000f:0.0} t) exceeds vehicle capacity ({report.engineering.payloadCapacityKg / 1000f:0.0} t) by {report.engineering.overPayloadKg / 1000f:0.00} t.", "Remove instruments or choose a more capable vehicle.");

            if (report.engineering.overBudgetUSD > 0.001f)
                Add(ref anyErrors, report, "OVER_BUDGET", $"Total cost ${report.engineering.cost.total / 1000000f:0}M exceeds the ${mission.budgetUSD / 1000000f:0}M budget by ${report.engineering.overBudgetUSD / 1000000f:0}M.", "Trim instruments, fuel, or pick cheaper components.");

            if (vehicle != null && report.engineering.stages.Count > 0 && !report.engineering.twrSufficient)
                Add(ref anyErrors, report, "LOW_TWR", $"Liftoff thrust-to-weight ratio is {report.engineering.twr:0.00}, below the minimum of {EngineeringModel.MinTwr:0.00}. The rocket cannot lift itself.", "Use higher-thrust engines, less fuel in the first stage, or a lighter payload.");

            // --- Instrument requirements (mission-specific) ---
            EvaluateInstrumentRequirements(mission, config, db, report, ref anyErrors);

            if (report.engineering.scienceValue < mission.minScienceValue)
                Add(ref anyErrors, report, "MIN_SCIENCE", $"Science value {report.engineering.scienceValue:0} is below the mission requirement of {mission.minScienceValue:0}.", "Add more scientific instruments.");

            // --- Power: instruments must fit the spacecraft's generation (challenge feature) ---
            if (spacecraft != null && report.engineering.powerW > report.engineering.powerGenerationW + 0.01f)
                Add(ref anyErrors, report, "POWER_OVERDRAW",
                    $"Instruments need {report.engineering.powerW:0} W but {spacecraft.name} only generates {report.engineering.powerGenerationW:0} W.",
                    "Drop power-hungry instruments or choose a spacecraft with more electrical power.");

            // --- Communications: data generation must fit the downlink (challenge feature) ---
            if (spacecraft != null && report.engineering.dataGbPerDay > report.engineering.commCapacityGbPerDay + 0.01f)
                Add(ref anyErrors, report, "COMM_OVERLOAD",
                    $"Instruments produce {report.engineering.dataGbPerDay:0.0} Gb/day but {spacecraft.name} can only downlink {report.engineering.commCapacityGbPerDay:0.0} Gb/day.",
                    "Remove high-data instruments or choose a spacecraft with a stronger communications system.");

            // --- Orbital constraint: informational line on the trajectory requirement ---
            if (spacecraft != null)
                report.issues.Add(new ValidationIssue("ORBIT_REQUIREMENT", ValidationSeverity.Info,
                    mission.capacityMetric == "TMI"
                        ? "Orbital constraint: the vehicle must deliver the stack onto a trans-Mars injection trajectory."
                        : "Orbital constraint: the vehicle must deliver the stack onto a trans-Lunar Injection trajectory.",
                    "Checked through the vehicle's destination payload capacity in the mass budget."));

            // --- Delta-v feasibility: warning only; underfueled configs are allowed
            //     to launch and fail in the simulation (per design decision). ---
            foreach (var dv in report.engineering.deltaV)
            {
                if (dv.sufficient) continue;
                var scopeText = dv.phase == "departure" ? "launch vehicle" : "spacecraft engines";
                report.issues.Add(new ValidationIssue($"INSUFFICIENT_DV_{dv.phase.ToUpperInvariant()}",
                    ValidationSeverity.Warning,
                    $"Warning: {scopeText} cannot produce the {dv.requiredMS:0} m/s required for the {dv.phase} burn (achieved {dv.achievedMS:0} m/s).",
                    "Load more fuel, choose more efficient engines, or reduce payload mass."));
            }

            report.readiness = anyErrors
                ? (config.HasAllRequiredSelections(db) ? ReadinessState.ValidationRequired : ReadinessState.NotReady)
                : ReadinessState.ReadyForLaunch;
            return report;
        }

        static void EvaluateInstrumentRequirements(MissionData mission, MissionConfig config, GameDatabase db, ValidationReport report, ref bool anyErrors)
        {
            if (mission.instrumentRequirements == null) return;

            foreach (var req in mission.instrumentRequirements)
            {
                if (req.type == "required")
                {
                    foreach (var id in req.ids)
                    {
                        if (config.instrumentIds.Contains(id)) continue;
                        var inst = db.GetInstrument(id);
                        var severity = req.severity == "block" ? ValidationSeverity.Error : ValidationSeverity.Warning;
                        if (req.severity == "block") anyErrors = true;
                        report.issues.Add(new ValidationIssue(
                            "INSTRUMENT_REQUIRED_MISSING", severity,
                            $"Required instrument missing: {inst?.name ?? id}. {req.consequence}",
                            $"Add {inst?.name ?? id} on the PAYLOAD card."));
                    }
                }
                else if (req.type == "anyOfRole")
                {
                    var count = 0;
                    foreach (var instId in config.instrumentIds)
                    {
                        var inst = db.GetInstrument(instId);
                        if (inst != null && inst.role == req.role) count++;
                    }
                    if (count >= req.count) continue;

                    var severity = req.severity == "block" ? ValidationSeverity.Error : ValidationSeverity.Info;
                    if (req.severity == "block") anyErrors = true;
                    report.issues.Add(new ValidationIssue(
                        "INSTRUMENT_ROLE_MISSING", severity,
                        $"Expected at least {req.count} '{req.role}' instrument(s); found {count}. {req.consequence}",
                        $"Open the PAYLOAD card and add a {req.role}-type instrument."));
                }
            }

            // Camera-control effect: informational note when missing on a mission that flies with telemetry.
            var hasCamera = false;
            foreach (var instId in config.instrumentIds)
            {
                var inst = db.GetInstrument(instId);
                if (inst != null && inst.effect == "cameraControl") hasCamera = true;
            }
            if (!hasCamera)
                report.issues.Add(new ValidationIssue("CAMERA_FIXED", ValidationSeverity.Info,
                    "No mast camera selected: the flight camera will be locked to a fixed position.",
                    "Add the Zoom Camera Mast for free camera control during the simulation."));
        }

        static void Add(ref bool anyErrors, ValidationReport report, string code, string message, string hint)
        {
            anyErrors = true;
            report.issues.Add(new ValidationIssue(code, ValidationSeverity.Error, message, hint));
        }
    }
}
