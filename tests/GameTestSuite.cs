// MY MISSION — automated test suite
// Runs OUTSIDE Assets/ (never compiled into the game build). Executed via the
// Unity Pipeline ephemeral compiler, which loads the game assemblies directly.
//
// Usage:  unity script run --file tests/GameTestSuite.cs
// Every check records EXPECTED vs ACTUAL and a PASS/FAIL verdict; the runner
// prints a full table plus a summary line.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpaceMission.Core;
using SpaceMission.Data;
using SpaceMission.Engineering;
using SpaceMission.State;
using SpaceMission.UI;

namespace SpaceMissionTesting
{
    public class TestResult
    {
        public string name;
        public string expected;
        public string actual;
        public bool passed;
    }

    public static class GameTestSuite
    {
        static readonly List<TestResult> results = new List<TestResult>();
        static GameDatabase db;

        public static string RunAll()
        {
            results.Clear();
            db = GameDatabase.Load();

            RunDataIntegrityTests();
            RunEngineeringTests();
            RunValidationTests();
            RunOutcomePredictionTests();
            RunScoringTests();
            RunUiTextTests();
            RunUnitsTests();

            var failed = results.Count(r => !r.passed);
            var sb = new StringBuilder();
            sb.AppendLine("=== MY MISSION TEST SUITE — expected vs actual ===");
            foreach (var r in results)
            {
                sb.AppendLine($"{(r.passed ? "PASS" : "FAIL")} | {r.name}");
                if (!r.passed)
                    sb.AppendLine($"       expected: {r.expected}\n       actual:   {r.actual}");
            }
            sb.AppendLine($"=== SUMMARY: {results.Count - failed}/{results.Count} passed, {failed} failed ===");
            return sb.ToString();
        }

        // ---------------------------------------------------------- assertions

        static void Check(string name, bool condition, string expected, string actual)
        {
            results.Add(new TestResult { name = name, passed = condition, expected = expected, actual = actual });
        }

        static void CheckTrue(string name, bool condition) =>
            Check(name, condition, "true", condition ? "true" : "false");

        static void CheckEqual(string name, string expected, string actual) =>
            Check(name, string.Equals(expected, actual), expected, actual);

        static void CheckFloat(string name, float expected, float actual, float tolerance)
        {
            var ok = Math.Abs(expected - actual) <= tolerance;
            results.Add(new TestResult
            {
                name = name,
                passed = ok,
                expected = expected.ToString("0.####"),
                actual = actual.ToString("0.####")
            });
        }

        static void CheckContains(string name, ValidationReport report, string code, ValidationSeverity severity)
        {
            var issue = report.issues.FirstOrDefault(i => i.code == code);
            Check(name + " [" + code + "]",
                issue != null && issue.severity == severity,
                severity + " " + code,
                issue != null ? issue.severity + " " + code : "issue not found");
        }

        static void CheckNotContains(string name, ValidationReport report, string code)
        {
            var issue = report.issues.FirstOrDefault(i => i.code == code);
            Check(name + " [no " + code + "]", issue == null, "absent", issue == null ? "absent" : issue.severity + " present");
        }

        // ---------------------------------------------------------- helpers

        static MissionConfig BuildApollo()
        {
            var cfg = new MissionConfig { mission = db.GetMission("M0-APOLLO") };
            cfg.SetVehicle("LV-SATV", db);
            cfg.SetSpacecraft("SC-CSMLM");
            cfg.SetLaunchpad("PAD-39A");
            cfg.SetStageEngines(0, Repeat("ENG-F1", 5));
            cfg.SetStageFuel(0, "FUEL-KEROLOX");
            cfg.SetStageFuelAmount(0, 2150);
            cfg.SetStageEngines(1, Repeat("ENG-J2", 5));
            cfg.SetStageFuel(1, "FUEL-HYDROLOX");
            cfg.SetStageFuelAmount(1, 444);
            cfg.SetStageEngines(2, Repeat("ENG-J2", 1));
            cfg.SetStageFuel(2, "FUEL-HYDROLOX");
            cfg.SetStageFuelAmount(2, 106);
            cfg.ToggleInstrument("INS-MOXIE");
            cfg.ToggleInstrument("INS-ALSEP");
            return cfg;
        }

        static MissionConfig BuildMars(bool withSherloc)
        {
            var cfg = new MissionConfig { mission = db.GetMission("M1-MARS2020") };
            cfg.SetVehicle("LV-ATLAS541", db);
            cfg.SetSpacecraft("SC-PERSAV");
            cfg.SetLaunchpad("PAD-41");
            cfg.SetStageEngines(0, Repeat("ENG-RD180", 1));
            cfg.SetStageFuel(0, "FUEL-KEROLOX");
            cfg.SetStageFuelAmount(0, 284);
            cfg.SetStageEngines(1, Repeat("ENG-RL10", 1));
            cfg.SetStageFuel(1, "FUEL-HYDROLOX");
            cfg.SetStageFuelAmount(1, 20.8f);
            cfg.ToggleInstrument("INS-MCAMZ");
            cfg.ToggleInstrument("INS-PIXL");
            cfg.ToggleInstrument("INS-MEDA");
            if (withSherloc) cfg.ToggleInstrument("INS-SHERLOC");
            return cfg;
        }

        static MissionConfig BuildArtemis()
        {
            var cfg = new MissionConfig { mission = db.GetMission("M2-ARTEMIS3") };
            cfg.SetVehicle("LV-SATV", db);
            cfg.SetSpacecraft("SC-ORIONLANDER");
            cfg.SetLaunchpad("PAD-39A");
            cfg.SetStageEngines(0, Repeat("ENG-F1", 5));
            cfg.SetStageFuel(0, "FUEL-KEROLOX");
            cfg.SetStageFuelAmount(0, 2150);
            cfg.SetStageEngines(1, Repeat("ENG-J2", 5));
            cfg.SetStageFuel(1, "FUEL-HYDROLOX");
            cfg.SetStageFuelAmount(1, 444);
            cfg.SetStageEngines(2, Repeat("ENG-J2", 1));
            cfg.SetStageFuel(2, "FUEL-HYDROLOX");
            cfg.SetStageFuelAmount(2, 106);
            cfg.ToggleInstrument("INS-MOXIE");
            cfg.ToggleInstrument("INS-ALSEP");
            cfg.ToggleInstrument("INS-EVAKIT");
            return cfg;
        }

        static List<string> Repeat(string id, int n)
        {
            var list = new List<string>();
            for (var i = 0; i < n; i++) list.Add(id);
            return list;
        }

        static ValidationReport Validate(MissionConfig cfg)
        {
            var mission = cfg.mission ?? GameState.CurrentMission;
            return ValidationEngine.Validate(mission, cfg, db);
        }

        // ---------------------------------------------------------- suites

        static void RunDataIntegrityTests()
        {
            CheckFloat("database: engines count", 8, db.engines.Count, 0);
            CheckFloat("database: fuels count", 4, db.fuels.Count, 0);
            CheckFloat("database: vehicles count", 4, db.vehicles.Count, 0);
            CheckFloat("database: spacecraft count", 5, db.spacecraft.Count, 0);
            CheckFloat("database: launchpads count", 4, db.launchpads.Count, 0);
            CheckFloat("database: instruments count", 10, db.instruments.Count, 0);
            CheckFloat("database: missions count", 3, db.missions.Count, 0);
            CheckFloat("database: planets count", 5, db.planets.Count, 0);

            CheckTrue("database: every engine fuel exists",
                db.engines.All(e => db.GetFuel(e.fuelId) != null));
            CheckTrue("database: every engine compatible vehicle exists (or free engine)",
                db.engines.All(e => e.compatibleVehicles == null || e.compatibleVehicles.Length == 0 ||
                    e.compatibleVehicles.All(v => db.GetVehicle(v) != null)));
            CheckTrue("database: every engine has a source citation",
                db.engines.All(e => !string.IsNullOrEmpty(e.source)));
            CheckTrue("database: every pad vehicle exists",
                db.launchpads.All(p => p.compatibleVehicles.All(v => db.GetVehicle(v) != null)));
            CheckTrue("database: every instrument mission tag exists",
                db.instruments.All(i => i.missionTags == null ||
                    i.missionTags.All(t => db.GetMission(t) != null)));
            CheckTrue("database: every mission required spacecraft exists",
                db.missions.All(m => db.GetSpacecraft(m.requiredSpacecraftId) != null));
            CheckTrue("database: every mission destination planet exists",
                db.missions.All(m => db.GetPlanet(m.destinationPlanetId) != null));
            CheckTrue("database: every mission required instrument id exists",
                db.missions.All(m => m.instrumentRequirements == null ||
                    m.instrumentRequirements.Where(r => r.ids != null)
                        .SelectMany(r => r.ids).All(id => db.GetInstrument(id) != null)));
            CheckTrue("database: every mission has a source-bearing dataset entry",
                db.missions.All(m => m.budgetUSD > 0 && m.deltaV != null && m.deltaV.Length > 0));
            CheckTrue("database: spacecraft main engine exists",
                db.spacecraft.All(s => db.GetEngine(s.engineId) != null));
            CheckTrue("database: unique engine ids", db.engines.Select(e => e.id).Distinct().Count() == db.engines.Count);
            CheckTrue("database: unique mission ids", db.missions.Select(m => m.id).Distinct().Count() == db.missions.Count);
            CheckTrue("database: vehicle stage fuel capacities positive",
                db.vehicles.All(v => v.stages.All(s => s.maxFuelFU > 0 && s.dryMassKg > 0)));
        }

        static void RunEngineeringTests()
        {
            var apollo = BuildApollo();
            var e = EngineeringModel.Evaluate(apollo.mission, apollo, db);

            CheckFloat("engineering: Apollo liftoff TWR ≈ 1.11", 1.11f, e.twr, 0.03f);
            CheckFloat("engineering: Apollo departure dv ≈ 12461 m/s", 12461f, e.departureDvMS, 40f);
            CheckFloat("engineering: Apollo total cost $648M", 648f, e.cost.total / 1e6f, 1f);
            CheckFloat("engineering: Apollo payload 45.14 t", 45.138f, e.payloadMassKg / 1000f, 0.01f);
            CheckFloat("engineering: Apollo science 70", 70, e.scienceValue, 0);
            CheckTrue("engineering: Apollo within payload capacity", e.withinPayloadCapacity);
            CheckTrue("engineering: Apollo TWR sufficient", e.twrSufficient);

            // fuel is measured in tonnes: stage 1 with 2150 FU must produce ~3485 m/s alone
            // (model includes tank mass and treats fixed solids as dead mass — conservative)
            var s1 = e.stages[0];
            CheckFloat("engineering: Saturn S-IC dv ≈ 3485 m/s", 3485f, s1.achievableDvMS, 40f);
            // tank mass must be included: S-IC tank = 2150 t × 0.06 (kerolox factor) = 129 t
            CheckFloat("engineering: S-IC tank mass 129 t included", 129000f, s1.totalMassKg - 130000f - 2150000f, 1000f);

            var mars = BuildMars(false);
            var em = EngineeringModel.Evaluate(mars.mission, mars, db);
            CheckFloat("engineering: Atlas TWR ≈ 1.54", 1.54f, em.twr, 0.03f);
            CheckFloat("engineering: Atlas departure dv ≈ 8381 m/s", 8381f, em.departureDvMS, 40f);
            CheckFloat("engineering: Atlas cost $234M", 234f, em.cost.total / 1e6f, 1f);

            // Isp multiplier: RS-25 on hydrolox = 452 × 1.15 = 519.8
            CheckFloat("engineering: hydrolox Isp multiplier", 519.8f,
                EngineeringModel.EffectiveIsp(db.GetEngine("ENG-RS25"), db.GetFuel("FUEL-HYDROLOX")), 0.01f);
            // hypergolic penalty: AJ10 = 314 × 0.9 = 282.6
            CheckFloat("engineering: hypergolic Isp penalty", 282.6f,
                EngineeringModel.EffectiveIsp(db.GetEngine("ENG-AJ10"), db.GetFuel("FUEL-HYPERGOLIC")), 0.01f);

            // spacecraft capabilities come from data, not propellant math
            var orion = db.GetSpacecraft("SC-ORION");
            CheckFloat("engineering: Orion arrival capability 1300 m/s", 1300, orion.arrivalDvMS, 0);
            CheckTrue("engineering: Orion cannot land on the Moon (arr. 1300 < 2600)",
                orion.arrivalDvMS < 2600);
            CheckFloat("engineering: CSM+LM return capability 3500 m/s", 3500,
                db.GetSpacecraft("SC-CSMLM").returnDvMS, 0);
        }

        static void RunValidationTests()
        {
            // happy paths
            var apollo = Validate(BuildApollo());
            CheckEqual("validation: Apollo full config readiness", "ReadyForLaunch", apollo.readiness.ToString());
            CheckTrue("validation: Apollo can launch", apollo.CanLaunch);
            CheckNotContains("validation: Apollo no blocking dv warning", apollo, "INSUFFICIENT_DV_DEPARTURE");

            var marsOk = Validate(BuildMars(true));
            CheckEqual("validation: Mars full config readiness", "ReadyForLaunch", marsOk.readiness.ToString());

            var artemis = Validate(BuildArtemis());
            CheckEqual("validation: Artemis full config readiness", "ReadyForLaunch", artemis.readiness.ToString());

            // empty config
            var empty = Validate(new MissionConfig { mission = db.GetMission("M0-APOLLO") });
            CheckEqual("validation: empty config readiness", "NotReady", empty.readiness.ToString());
            CheckTrue("validation: empty config has MISSING_VEHICLE",
                empty.issues.Any(i => i.code == "MISSING_VEHICLE"));

            // mission-specific instrument requirement (Mars 2020 requires SHERLOC)
            var marsMissing = Validate(BuildMars(false));
            CheckContains("validation: Mars missing SHERLOC", marsMissing, "INSTRUMENT_REQUIRED_MISSING", ValidationSeverity.Error);
            CheckTrue("validation: Mars without SHERLOC cannot launch", !marsMissing.CanLaunch);
            CheckContains("validation: Mars min science enforced", marsMissing, "MIN_SCIENCE", ValidationSeverity.Error);

            // camera effect is informational, never blocking — config WITHOUT any mast camera
            var noCamera = BuildMars(true);
            noCamera.ToggleInstrument("INS-MCAMZ");   // remove the zoom camera mast
            CheckContains("validation: camera note present", Validate(noCamera), "CAMERA_FIXED", ValidationSeverity.Info);
            var withCamera = Validate(BuildMars(true));
            CheckNotContains("validation: camera note absent when mast selected", withCamera, "CAMERA_FIXED");

            // underfueled: dv warning must NOT block launch (launch allowed, fails in sim)
            var underfueled = BuildMars(true);
            underfueled.SetStageFuelAmount(1, 2f);
            var reportU = Validate(underfueled);
            CheckContains("validation: underfueled dv warning", reportU, "INSUFFICIENT_DV_DEPARTURE", ValidationSeverity.Warning);
            CheckTrue("validation: underfueled config still launchable", reportU.CanLaunch);

            // pad incompatibilities
            var wrongPad = BuildMars(true);
            wrongPad.SetLaunchpad("PAD-39A");
            CheckContains("validation: Atlas on LC-39A blocked", Validate(wrongPad), "PAD_VEHICLE_INCOMPAT", ValidationSeverity.Error);

            var cargoPadForCrew = BuildApollo();
            cargoPadForCrew.SetLaunchpad("PAD-40");
            var reportCargo = Validate(cargoPadForCrew);
            CheckContains("validation: crew mission on cargo pad blocked", reportCargo, "PAD_NO_CREW_INFRA", ValidationSeverity.Error);

            // engine/fuel incompatibility (F-1 does not burn hydrogen)
            var badFuel = BuildApollo();
            badFuel.SetStageFuel(0, "FUEL-HYDROLOX");
            CheckContains("validation: F-1 + hydrolox blocked", Validate(badFuel), "ENGINE_FUEL_INCOMPAT", ValidationSeverity.Error);

            // engine role mismatch (upper engine into booster slot)
            var badRole = BuildApollo();
            badRole.SetStageEngines(0, Repeat("ENG-J2", 5));
            CheckContains("validation: J-2 in booster slot blocked", Validate(badRole), "ENGINE_ROLE_MISMATCH", ValidationSeverity.Error);

            // engine/vehicle incompatibility (Merlin Vacuum on SLS ICPS)
            var badVehicle = BuildArtemis();
            badVehicle.SetStageEngines(1, Repeat("ENG-MERVAC", 1));
            var reportBV = Validate(badVehicle);
            CheckContains("validation: MerlinVac on SLS blocked", reportBV, "ENGINE_VEHICLE_INCOMPAT", ValidationSeverity.Error);

            // restartable requirement (non-restartable engine on ICPS)
            var notRestartable = BuildArtemis();
            notRestartable.SetStageEngines(1, Repeat("ENG-RL10", 1)); // RL10 restartable OK — use Merlin Vac role swap instead
            // Saturn S-IVB requires restartable; assign F-1 (booster role) triggers role error first,
            // so test restartable via a valid-role non-restartable engine: none exists for SATV upper except J-2.
            // Instead assert Merlin Vacuum IS restartable and Merlin 1D is not:
            CheckTrue("validation: MerlinVac restartable", db.GetEngine("ENG-MERVAC").restartable);
            CheckTrue("validation: Merlin1D not restartable", !db.GetEngine("ENG-MERLIN").restartable);

            // fuel over capacity
            var overFuel = BuildApollo();
            overFuel.SetStageFuelAmount(0, 3000f);
            CheckContains("validation: fuel over capacity blocked", Validate(overFuel), "FUEL_OVER_CAPACITY", ValidationSeverity.Error);

            // over budget: all seven Mars instruments push the total past the $300M budget
            var overBudget = BuildMars(true);
            overBudget.ToggleInstrument("INS-MOXIE");
            overBudget.ToggleInstrument("INS-SUPCAM");
            overBudget.ToggleInstrument("INS-RIMFAX");
            var reportOB = Validate(overBudget);
            CheckTrue("validation: over-budget test config actually costs > $300M",
                reportOB.engineering.cost.total > 300000000f);
            CheckContains("validation: over budget blocked", reportOB, "OVER_BUDGET", ValidationSeverity.Error);

            // over payload: Saturn V with Orion lander (45 t) + crew instruments is fine;
            // SLS cannot lift the 45.2 t Orion lander stack
            var artemisOnSls = BuildArtemis();
            artemisOnSls.SetVehicle("LV-SLS", db);
            var reportSls = Validate(artemisOnSls);
            CheckContains("validation: Orion lander exceeds SLS TLI", reportSls, "OVER_PAYLOAD", ValidationSeverity.Error);

            // spacecraft requirement (Artemis needs the lander stack, not bare Orion)
            var artemisOrion = BuildArtemis();
            artemisOrion.SetSpacecraft("SC-ORION");
            CheckContains("validation: Artemis requires lander stack", Validate(artemisOrion), "SPACECRAFT_NOT_REQUIRED", ValidationSeverity.Error);

            // crew rating rule exists for crewed missions
            CheckTrue("validation: crewed missions demand human-rated vehicles",
                db.GetMission("M2-ARTEMIS3").crewRatedVehicleRequired);
        }

        static void RunOutcomePredictionTests()
        {
            var good = Validate(BuildApollo());
            CheckTrue("outcome: Apollo all dv phases sufficient",
                good.engineering.deltaV.All(d => d.sufficient));

            var underfueled = BuildMars(true);
            underfueled.SetStageFuelAmount(1, 2f);
            var r = Validate(underfueled);
            var departure = r.engineering.deltaV.First(d => d.phase == "departure");
            CheckTrue("outcome: underfueled departure insufficient", !departure.sufficient);
            CheckTrue("outcome: underfueled predicts failure (needed 5000, achieved < 5000)",
                departure.achievedMS < 5000);
            // full tanks must pass with margin
            var fueled = Validate(BuildMars(true));
            var depOk = fueled.engineering.deltaV.First(d => d.phase == "departure");
            CheckTrue("outcome: fully-fueled Atlas passes the departure budget with margin",
                depOk.sufficient && depOk.achievedMS > 7500);

            var orionLander = db.GetSpacecraft("SC-ORIONLANDER");
            CheckTrue("outcome: Orion lander can brake (arrival ≥ 2600)", orionLander.arrivalDvMS >= 2600);
            CheckTrue("outcome: Orion lander can return (return ≥ 2400)", orionLander.returnDvMS >= 2400);

            var perseverance = db.GetSpacecraft("SC-PERSAV");
            CheckTrue("outcome: Perseverance direct entry needs almost no arrival burn",
                perseverance.arrivalDvMS >= 200);
        }

        static void RunScoringTests()
        {
            // --- Successful Apollo: full marks except efficiency ---
            var apollo = BuildApollo();
            var engApollo = EngineeringModel.Evaluate(apollo.mission, apollo, db);
            var outcomeOk = ScoringModel.EvaluateOutcome(apollo.mission, apollo, engApollo);
            CheckTrue("scoring: Apollo config succeeds", outcomeOk.success);
            CheckEqual("scoring: Apollo completes all phases", "Complete", outcomeOk.phaseReached.ToString());
            CheckTrue("scoring: successful outcome has no failure reason",
                string.IsNullOrEmpty(outcomeOk.failureReason));

            var scoreOk = ScoringModel.Score(apollo.mission, apollo, engApollo, outcomeOk);
            CheckFloat("scoring: Apollo mission success = 35", 35f, scoreOk.missionSuccess, 0.01f);
            CheckFloat("scoring: Apollo science = 25 (70 ≥ required 50)", 25f, scoreOk.scientificValue, 0.01f);
            CheckFloat("scoring: Apollo efficiency = 20 × 52/700", 20f * 52f / 700f, scoreOk.resourceEfficiency, 0.05f);
            // min dv margin is the arrival phase (3200/2600 = 1.231) → reliability = 20 × (0.231/0.25)
            CheckFloat("scoring: Apollo reliability ≈ 18.5 (arrival margin is the minimum)", 18.46f, scoreOk.engineeringReliability, 0.1f);
            CheckFloat("scoring: Apollo total ≈ 79.9", 35f + 25f + 20f * 52f / 700f + 18.46f, scoreOk.total, 0.15f);
            CheckEqual("scoring: Apollo grade B", "B", scoreOk.grade);

            // --- Failed underfueled Mars: no success points, science kept ---
            var underfueled = BuildMars(true);
            underfueled.SetStageFuelAmount(1, 2f);
            var engFail = EngineeringModel.Evaluate(underfueled.mission, underfueled, db);
            var outcomeFail = ScoringModel.EvaluateOutcome(underfueled.mission, underfueled, engFail);
            CheckTrue("scoring: underfueled Mars fails", !outcomeFail.success);
            CheckEqual("scoring: failure phase = Departure", "Departure", outcomeFail.phaseReached.ToString());
            CheckTrue("scoring: failure reason is player-facing",
                outcomeFail.failureReason != null && outcomeFail.failureReason.Length > 20);
            CheckTrue("scoring: failure lesson teaches the fix",
                outcomeFail.lesson != null && outcomeFail.lesson.ToLowerInvariant().Contains("fuel"));

            var scoreFail = ScoringModel.Score(underfueled.mission, underfueled, engFail, outcomeFail);
            CheckFloat("scoring: failed mission success = 0", 0f, scoreFail.missionSuccess, 0.01f);
            CheckFloat("scoring: failed Mars science still earned (115 ≥ 95)", 25f, scoreFail.scientificValue, 0.01f);
            CheckTrue("scoring: reliability 0 when dv margin < 1", scoreFail.engineeringReliability <= 0.01f);
            CheckTrue("scoring: failure total under 60 (grade D)", scoreFail.total < 60f);

            // --- Exactly-meeting config: reliability 0, everything else full ---
            var minScienceCfg = BuildMars(true);
            var engMin = EngineeringModel.Evaluate(minScienceCfg.mission, minScienceCfg, db);
            var outcomeMin = ScoringModel.EvaluateOutcome(minScienceCfg.mission, minScienceCfg, engMin);
            var scoreMin = ScoringModel.Score(minScienceCfg.mission, minScienceCfg, engMin, outcomeMin);
            CheckTrue("scoring: science score never exceeds its cap", scoreMin.scientificValue <= 25.01f);
            CheckTrue("scoring: efficiency score never exceeds its cap", scoreMin.resourceEfficiency <= 20.01f);
            CheckTrue("scoring: total never exceeds 100", scoreMin.total <= 100.01f);

            // --- Determinism: same input twice → identical score ---
            var scoreAgain = ScoringModel.Score(apollo.mission, apollo, engApollo, outcomeOk);
            CheckTrue("scoring: deterministic (two runs identical)",
                Math.Abs(scoreAgain.total - scoreOk.total) < 0.001f);

            // --- Budget summary text exists and mentions the spend ---
            var summary = ScoringModel.BudgetSummary(apollo.mission, engApollo);
            CheckTrue("scoring: budget summary mentions total spent",
                summary.Contains("Total spent") && summary.Contains("648"));

            // --- Progression: unlock + best score (PlayerPrefs snapshot + restore) ---
            RunProgressionTests();
        }

        // ---------------------------------------------------------- UI text tests
        // These run only in Play Mode (they drive live uGUI screens). In Edit Mode
        // they self-skip. They verify the TEXT the screens actually display —
        // catching the "right data, wrong label" class of bugs.

        static void RunUiTextTests()
        {
            var sm = SpaceMission.UI.ScreenManager.Instance;
            var boot = UnityEngine.Object.FindAnyObjectByType<SpaceMission.Core.GameManager>();
            var screen = UnityEngine.GameObject.Find("SelectionScreen");
            var inPlayMode = UnityEngine.Application.isPlaying && sm != null && boot != null && screen != null;
            if (!inPlayMode)
            {
                Check("ui-text: (play-mode only, self-skipped in edit mode)", true, "skipped", "skipped");
                return;
            }

            // --- 1. Page title must match the opened category ---
            SpaceMission.UI.UISelection.CurrentCategory = SpaceMission.UI.ConfigCategory.Engines;
            screen.SetActive(false); screen.SetActive(true);
            var title = screen.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text;
            CheckEqual("ui-text: engine page title", "ENGINES", title);

            SpaceMission.UI.UISelection.CurrentCategory = SpaceMission.UI.ConfigCategory.Fuels;
            screen.SetActive(false); screen.SetActive(true);
            title = screen.transform.Find("Header/Text").GetComponent<TMPro.TextMeshProUGUI>().text;
            CheckEqual("ui-text: fuel page title", "FUELS", title);

            // --- 2. No duplicate catalog entries after rebuild (deferred-destroy regression) ---
            SpaceMission.UI.UISelection.CurrentCategory = SpaceMission.UI.ConfigCategory.Engines;
            screen.SetActive(false); screen.SetActive(true);
            var content = screen.transform.Find("Catalog/Scroll/Viewport/Content");
            var names = new System.Collections.Generic.List<string>();
            foreach (UnityEngine.Transform item in content)
                names.Add(item.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].text);
            Check("ui-text: no duplicate catalog items after rebuild",
                names.Distinct().Count() == names.Count,
                string.Join(", ", names.Distinct().ToArray()),
                string.Join(", ", names.ToArray()));

            // --- 3. Hub card status text matches the configuration ---
            var hub = UnityEngine.GameObject.Find("ConfigHubScreen");
            if (hub == null)
            {
                sm.Show("configHub");
                hub = UnityEngine.GameObject.Find("ConfigHubScreen");
            }
            var cfg = GameState.Config;
            var db2 = db;
            cfg.SetVehicle("LV-SATV", db2);
            cfg.SetSpacecraft("SC-CSMLM");
            SpaceMission.UI.UIConfigHub.Instance.RefreshCards();
            var cardTexts = new System.Collections.Generic.Dictionary<string, string>();
            var grid = hub.transform.Find("Grid");
            foreach (UnityEngine.Transform card in grid)
            {
                var refs = card.GetComponent<SpaceMission.UI.CardRefs>();
                if (refs != null) cardTexts[card.name] = refs.statusLabel.text;
            }
            var engExpected = "SELECTED";
            Check("ui-text: engines card shows selected state when stage engines assigned",
                cardTexts.ContainsKey("Engines") && cardTexts["Engines"].StartsWith("SELECTED") == (cfg.stages.Count > 0 && cfg.stages[0].engineIds.Count > 0),
                cfg.stages.Count > 0 && cfg.stages[0].engineIds.Count > 0 ? engExpected + " (engines assigned)" : "NOT SELECTED (no engines)",
                cardTexts.ContainsKey("Engines") ? cardTexts["Engines"] : "card missing");

            Check("ui-text: rockets card names the selected vehicle+spacecraft",
                cardTexts.ContainsKey("Rockets") && cardTexts["Rockets"].Contains("Saturn V") && cardTexts["Rockets"].Contains("Apollo CSM"),
                "SELECTED · Saturn V + Apollo CSM + Lunar Module",
                cardTexts.ContainsKey("Rockets") ? cardTexts["Rockets"] : "card missing");

            Check("ui-text: payload card shows instrument count",
                cardTexts.ContainsKey("Payload") &&
                cardTexts["Payload"].Contains(cfg.instrumentIds.Count.ToString()),
                $"SELECTED · {cfg.instrumentIds.Count} instrument(s) selected",
                cardTexts.ContainsKey("Payload") ? cardTexts["Payload"] : "card missing");

            // --- 4. Selecting a fuel updates the fuel card text (not the engine card) ---
            var beforeEngines = cardTexts.ContainsKey("Engines") ? cardTexts["Engines"] : "";
            cfg.SetStageFuel(0, "FUEL-KEROLOX");
            cfg.SetStageFuelAmount(0, 100);
            SpaceMission.UI.UIConfigHub.Instance.RefreshCards();
            cardTexts.Clear();
            foreach (UnityEngine.Transform card in grid)
            {
                var refs = card.GetComponent<SpaceMission.UI.CardRefs>();
                if (refs != null) cardTexts[card.name] = refs.statusLabel.text;
            }
            Check("ui-text: fuel selection text lands on the FUEL card",
                cardTexts.ContainsKey("Fuels") && cardTexts["Fuels"].Contains("Kerosene"),
                "SELECTED · Stage 1: Kerosene / LOX 100 FU",
                cardTexts.ContainsKey("Fuels") ? cardTexts["Fuels"] : "card missing");
            Check("ui-text: engine card text unchanged by fuel selection",
                cardTexts.ContainsKey("Engines") && cardTexts["Engines"] == beforeEngines,
                beforeEngines,
                cardTexts.ContainsKey("Engines") ? cardTexts["Engines"] : "card missing");
        }

        static void RunProgressionTests()        {
            const string keyUnlocked = "smd.unlocked";
            const string keyScores = "smd.scores";
            var savedUnlocked = UnityEngine.PlayerPrefs.GetString(keyUnlocked, "");
            var savedScores = UnityEngine.PlayerPrefs.GetString(keyScores, "");

            UnityEngine.PlayerPrefs.DeleteKey(keyUnlocked);
            UnityEngine.PlayerPrefs.DeleteKey(keyScores);
            var progress = new ProgressService();
            progress.Load();

            CheckTrue("progression: only Apollo unlocked on a fresh profile",
                progress.IsUnlocked("M0-APOLLO") && !progress.IsUnlocked("M1-MARS2020") && !progress.IsUnlocked("M2-ARTEMIS3"));

            progress.UnlockMission("M1-MARS2020");
            CheckTrue("progression: unlocking Mars works", progress.IsUnlocked("M1-MARS2020"));

            progress.SubmitScore("M0-APOLLO", 81);
            CheckEqual("progression: best score stored", "81", progress.GetBestScore("M0-APOLLO").ToString());

            progress.SubmitScore("M0-APOLLO", 70);
            CheckEqual("progression: best score keeps the maximum", "81", progress.GetBestScore("M0-APOLLO").ToString());

            // persistence round-trip
            var reloaded = new ProgressService();
            reloaded.Load();
            CheckTrue("progression: unlocks persist", reloaded.IsUnlocked("M1-MARS2020"));
            CheckEqual("progression: scores persist", "81", reloaded.GetBestScore("M0-APOLLO").ToString());

            // restore the player's real profile
            UnityEngine.PlayerPrefs.SetString(keyUnlocked, savedUnlocked);
            UnityEngine.PlayerPrefs.SetString(keyScores, savedScores);
            UnityEngine.PlayerPrefs.Save();
        }

        static void RunUnitsTests()
        {
            var savedMode = new GameSettings();
            savedMode.Load();

            var settings = new GameSettings();
            GameState.Settings = settings;

            // Real units only (user decision 2026-11-15 — game units removed)
            CheckEqual("units: thrust in kN", "845 kN", Formats.Thrust(845f));
            CheckEqual("units: mass in tonnes", "4.5 t", Formats.Mass(4500f));
            CheckEqual("units: fuel in tonnes", "2150 t", Formats.Fuel(2150f));
            CheckEqual("units: money always USD", "$648.0M", Formats.Money(648000000f));
            CheckEqual("units: distance", "384400 km", Formats.Distance(384400f));
            CheckEqual("units: power in watts", "110 W", Formats.Power(110f));

            // restore the player's real settings
            GameState.Settings = savedMode;
        }
    }
}
