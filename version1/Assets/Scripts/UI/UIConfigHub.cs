using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;
using SpaceMission.Engineering;

namespace SpaceMission.UI
{
    public enum ConfigCategory { Engines, Fuels, Rockets, Launchpads, Payload }

    /// Mission configuration hub: four 2x2 category cards + wide PAYLOAD card,
    /// persistent status area, inbox briefing shortcut, validate + launch gate (GDD 6).
    public class UIConfigHub : MonoBehaviour
    {
        public static UIConfigHub Instance { get; private set; }

        TextMeshProUGUI budgetLabel;
        TextMeshProUGUI payloadLabel;
        TextMeshProUGUI readinessLabel;
        TextMeshProUGUI missionTitle;
        readonly Dictionary<ConfigCategory, GameObject> cards = new Dictionary<ConfigCategory, GameObject>();
        bool validationStale = true;

        public static UIConfigHub Build(Transform parent)
        {
            var screen = new GameObject("ConfigHubScreen", typeof(RectTransform), typeof(UIConfigHub));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var hub = screen.GetComponent<UIConfigHub>();
            hub.BuildContent(rect);
            return hub;
        }

        void BuildContent(RectTransform rect)
        {
            Instance = this;

            // --- Top bar ---
            var top = UITheme.Panel(rect, "TopBar", UITheme.BgPanel);
            top.anchorMin = new Vector2(0, 1);
            top.anchorMax = new Vector2(1, 1);
            top.pivot = new Vector2(0.5f, 1f);
            top.sizeDelta = new Vector2(0, 84);
            top.anchoredPosition = Vector2.zero;

            // Inbox (briefing) shortcut — visually secondary, top-left (GDD 6.4)
            var inbox = UITheme.TextButton(top, "BRIEF", UITheme.FontSmall);
            var inboxRect = inbox.GetComponent<RectTransform>();
            inboxRect.anchorMin = new Vector2(0, 0);
            inboxRect.anchorMax = new Vector2(0, 1);
            inboxRect.pivot = new Vector2(0, 0.5f);
            inboxRect.sizeDelta = new Vector2(90, 0);
            inboxRect.offsetMin = new Vector2(16, 14);
            inboxRect.offsetMax = new Vector2(106, -14);
            inbox.onClick.AddListener(() => ScreenManager.Instance.Show("briefing"));

            var back = UITheme.TextButton(top, "EXIT", UITheme.FontSmall);
            var backRect = back.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0);
            backRect.anchorMax = new Vector2(0, 1);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.sizeDelta = new Vector2(90, 0);
            backRect.offsetMin = new Vector2(116, 14);
            backRect.offsetMax = new Vector2(206, -14);
            back.onClick.AddListener(() =>
            {
                // EXIT always returns to the home screen (user decision)
                ScreenManager.Instance.ClearHistory();
                ScreenManager.Instance.Show("mainMenu", false);
            });

            missionTitle = UITheme.Label(top, "", UITheme.FontH3, UITheme.TextPrimary, TextAnchor.MiddleLeft, FontStyle.Bold);
            var titleRect = missionTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.offsetMin = new Vector2(226, 10);
            titleRect.offsetMax = new Vector2(-700, -10);

            // --- Status area (persistent, GDD 6.6) ---
            var statusGo = new GameObject("Status", typeof(RectTransform), typeof(VerticalLayoutGroup));
            var statusRect = statusGo.GetComponent<RectTransform>();
            statusRect.SetParent(top, false);
            statusRect.anchorMin = new Vector2(1, 0);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.pivot = new Vector2(1, 0.5f);
            statusRect.offsetMin = new Vector2(-560, 10);
            statusRect.offsetMax = new Vector2(-16, -10);
            var svlg = statusGo.GetComponent<VerticalLayoutGroup>();
            svlg.spacing = 2;
            svlg.childControlWidth = true;
            svlg.childControlHeight = true;
            svlg.childAlignment = TextAnchor.UpperRight;
            svlg.childForceExpandWidth = true;

            budgetLabel = MakeStatusLine(statusRect);
            payloadLabel = MakeStatusLine(statusRect);
            readinessLabel = MakeStatusLine(statusRect, UITheme.Warning);

            // --- Card grid ---
            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            var grid = gridGo.GetComponent<RectTransform>();
            grid.SetParent(rect, false);
            grid.anchorMin = new Vector2(0, 0);
            grid.anchorMax = new Vector2(1, 1);
            grid.offsetMin = new Vector2(60, 60);
            grid.offsetMax = new Vector2(-60, -104);

            var glg = grid.GetComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(760, 220);
            glg.spacing = new Vector2(24, 24);
            glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            glg.constraintCount = 2;

            cards[ConfigCategory.Engines] = BuildCard(grid, ConfigCategory.Engines, "ENGINES",
                "Propulsion for every stage. Thrust lifts the vehicle; efficiency decides how far the fuel takes it.");
            cards[ConfigCategory.Fuels] = BuildCard(grid, ConfigCategory.Fuels, "FUELS",
                "Propellant type per stage. Higher efficiency costs more or needs bigger tanks.");
            cards[ConfigCategory.Rockets] = BuildCard(grid, ConfigCategory.Rockets, "ROCKETS",
                "Launch vehicle (defines your stages and payload capacity) and the spacecraft that flies the mission.");
            cards[ConfigCategory.Launchpads] = BuildCard(grid, ConfigCategory.Launchpads, "LAUNCHPADS",
                "The facility that supports your vehicle. Compatibility is not cosmetic.");
            cards[ConfigCategory.Payload] = BuildWideCard(grid, ConfigCategory.Payload, "PAYLOAD — SCIENTIFIC INSTRUMENTS",
                "Instruments define the mission's science value, mass, power and cost. Some are required by the mission.");

            // --- Validate + Launch (bottom bar) ---
            var validateBtn = UITheme.TextButton(rect, "VALIDATE CONFIGURATION", UITheme.FontBody);
            var vRect = validateBtn.GetComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0.5f, 0);
            vRect.anchorMax = new Vector2(0.5f, 0);
            vRect.pivot = new Vector2(1, 0);
            vRect.sizeDelta = new Vector2(320, 52);
            vRect.anchoredPosition = new Vector2(-16, 8);
            validateBtn.onClick.AddListener(() => OpenValidation());

            var launchBtn = UITheme.TextButton(rect, "LAUNCH", UITheme.FontH3, UITheme.BtnAccent, UITheme.TextPrimary, UITheme.Accent);
            var lRect = launchBtn.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0.5f, 0);
            lRect.anchorMax = new Vector2(0.5f, 0);
            lRect.pivot = new Vector2(0, 0);
            lRect.sizeDelta = new Vector2(240, 52);
            lRect.anchoredPosition = new Vector2(16, 8);
            launchBtn.onClick.AddListener(() => TryLaunch());
            launchBtn.interactable = false;

            launchGate = launchBtn;
        }

        Button launchGate;

        static TextMeshProUGUI MakeStatusLine(RectTransform parent, Color? color = null)
        {
            var label = UITheme.Label(parent, "", UITheme.FontBody, color ?? UITheme.TextPrimary, TextAnchor.UpperRight);
            label.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 24);
            return label;
        }

        GameObject BuildCard(RectTransform grid, ConfigCategory category, string title, string description)
        {
            var card = UITheme.BorderedPanel(grid, category.ToString(), UITheme.BgPanel, UITheme.Border, 1, raycast: true);
            var layout = card.gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 760;
            layout.preferredHeight = 220;
            card.gameObject.AddComponent<Button>().onClick.AddListener(() =>
            {
                UISelection.CurrentCategory = category;
                ScreenManager.Instance.Show("selection");
            });

            // Image placeholder (left)
            var image = UITheme.Panel(card, "Image", UITheme.BgPanelAlt);
            image.anchorMin = new Vector2(0, 0);
            image.anchorMax = new Vector2(0, 1);
            image.pivot = new Vector2(0, 0.5f);
            image.offsetMin = new Vector2(16, 16);
            image.offsetMax = new Vector2(196, -16);

            // Text block to the right of the image (GDD 6.1)
            var text = new GameObject("Text", typeof(RectTransform), typeof(VerticalLayoutGroup));
            var textRect = text.GetComponent<RectTransform>();
            textRect.SetParent(card, false);
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(210, 14);
            textRect.offsetMax = new Vector2(-16, -14);
            var vlg = text.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 4;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var titleLabel = UITheme.Label(textRect, title, UITheme.FontH3, UITheme.TextPrimary, TextAnchor.UpperLeft, FontStyle.Bold);
            titleLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 30);
            var descLabel = UITheme.Label(textRect, description, UITheme.FontSmall, UITheme.TextMuted, TextAnchor.UpperLeft);
            descLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 60);
            var statusLabel = UITheme.Label(textRect, "NOT SELECTED", UITheme.FontBody, UITheme.Warning, TextAnchor.LowerLeft);
            statusLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 26);

            // store references for RefreshCards
            card.gameObject.AddComponent<CardRefs>().statusLabel = statusLabel;
            return card.gameObject;
        }

        GameObject BuildWideCard(RectTransform grid, ConfigCategory category, string title, string description)
        {
            var card = BuildCard(grid, category, title, description);
            // stretch across both columns by expanding cell size for this row via LayoutElement ignoredSize
            // GridLayoutGroup sizes all cells equally; instead place it manually as last row center using
            // a wrapper is complex — keep it in the second column slot of row 3 (GDD allows layout refinement).
            return card;
        }

        void OnEnable()
        {
            if (budgetLabel == null) return;   // OnEnable fires during construction, before BuildContent
            validationStale = true;
            var m = GameState.CurrentMission;
            if (m != null)
            {
                missionTitle.text = m.name + "  —  MISSION CONFIGURATION";
            }
            if (GameState.Config != null)
                GameState.Config.Changed += OnConfigChanged;
            RefreshCards();
        }

        void OnDisable()
        {
            if (GameState.Config != null)
                GameState.Config.Changed -= OnConfigChanged;
        }

        void OnConfigChanged()
        {
            validationStale = true;
            RefreshCards();
        }

        public void RefreshCards()
        {
            var db = GameState.DB;
            var cfg = GameState.Config;
            if (db == null || cfg == null || GameState.CurrentMission == null) return;

            var vehicle = cfg.Vehicle(db);
            var spacecraft = cfg.Spacecraft(db);

            // A category counts as selected only when IT has a real selection —
            // not merely when a vehicle exists (user-reported bug).
            var enginesAssigned = vehicle != null && cfg.stages.Count > 0 &&
                                  cfg.stages.TrueForAll(s => s.engineIds.Count > 0);
            var fuelLoaded = vehicle != null && cfg.stages.Count > 0 &&
                             cfg.stages.TrueForAll(s => !string.IsNullOrEmpty(s.fuelId) && s.fuelAmountFU > 0f);

            SetCardStatus(ConfigCategory.Engines, DescribeEngines(vehicle, cfg), enginesAssigned);
            SetCardStatus(ConfigCategory.Fuels, DescribeFuels(vehicle, cfg), fuelLoaded);
            SetCardStatus(ConfigCategory.Rockets,
                vehicle != null || spacecraft != null
                    ? $"{(vehicle != null ? vehicle.name : "select a vehicle")}  +  {(spacecraft != null ? spacecraft.name : "select a spacecraft")}"
                    : null, vehicle != null);   // partial: vehicle chosen counts, hint shows what's missing
            SetCardStatus(ConfigCategory.Launchpads, cfg.Launchpad(db)?.name, cfg.Launchpad(db) != null);
            SetCardStatus(ConfigCategory.Payload,
                cfg.instrumentIds.Count > 0 ? $"{cfg.instrumentIds.Count} instrument(s) selected" : null,
                cfg.instrumentIds.Count > 0);

            var report = GameState.LastValidation;
            var engineResult = EngineeringModel.Evaluate(GameState.CurrentMission, cfg, db);
            budgetLabel.text = $"Budget:  {Formats.Money(Mathf.Max(0, GameState.CurrentMission.budgetUSD - engineResult.cost.total))} remaining of {Formats.Money(GameState.CurrentMission.budgetUSD)}";
            payloadLabel.text = $"Payload:  {Formats.Payload(engineResult.payloadMassKg)} / {Formats.Payload(engineResult.payloadCapacityKg)}   ·   Science:  {engineResult.scienceValue:0}   ·   Power:  {engineResult.powerW:0}/{engineResult.powerGenerationW:0} W   ·   Data:  {engineResult.dataGbPerDay:0.0}/{engineResult.commCapacityGbPerDay:0.0} Gb/day";
            if (report != null && !validationStale)
                readinessLabel.text = report.CanLaunch ? "READY FOR LAUNCH" : "VALIDATION REQUIRED";
            else if (cfg.HasAllRequiredSelections(db))
                readinessLabel.text = "VALIDATION REQUIRED";
            else if (vehicle == null)
                readinessLabel.text = "NOT READY — start with the ROCKETS card";
            else
                readinessLabel.text = "NOT READY";
            readinessLabel.color = report != null && !validationStale && report.CanLaunch ? UITheme.Success : cfg.HasAllRequiredSelections(db) ? UITheme.Warning : UITheme.Danger;

            if (launchGate != null)
                launchGate.interactable = true;   // LAUNCH always available; validation decides what happens
        }

        static string DescribeEngines(Data.VehicleData vehicle, State.MissionConfig cfg)
        {
            if (vehicle == null) return null;
            // compact: "F-1 ×5 · J-2 ×5 · J-2" — stage names dropped to avoid clipping
            var counts = new List<string>();
            var seen = new List<string>();
            foreach (var stage in cfg.stages)
                foreach (var e in stage.engineIds)
                {
                    var name = GameState.DB.GetEngine(e)?.name ?? "?";
                    if (!seen.Contains(name)) seen.Add(name);
                }
            foreach (var name in seen)
            {
                var total = 0;
                foreach (var stage in cfg.stages)
                    foreach (var e in stage.engineIds)
                        if ((GameState.DB.GetEngine(e)?.name ?? "?") == name) total++;
                counts.Add(total > 1 ? $"{name} ×{total}" : name);
            }
            return counts.Count > 0 ? string.Join("  ·  ", counts.ToArray()) : null;
        }

        static string DescribeFuels(Data.VehicleData vehicle, State.MissionConfig cfg)
        {
            if (vehicle == null) return null;
            var parts = new List<string>();
            foreach (var stage in cfg.stages)
            {
                if (string.IsNullOrEmpty(stage.fuelId)) continue;
                var fuel = GameState.DB.GetFuel(stage.fuelId);
                parts.Add($"{fuel?.name ?? stage.fuelId} {Formats.Fuel(stage.fuelAmountFU)}");
            }
            return parts.Count > 0 ? string.Join("  ·  ", parts.ToArray()) : null;
        }

        void SetCardStatus(ConfigCategory category, string value, bool selected)
        {
            if (!cards.TryGetValue(category, out var card) || card == null) return;
            var refs = card.GetComponent<CardRefs>();
            if (refs == null) return;
            refs.statusLabel.text = selected && !string.IsNullOrEmpty(value)
                ? $"SELECTED  ·  {value}"
                : selected ? "PARTIAL — finish this category"
                : "NOT SELECTED";
            refs.statusLabel.color = selected && !string.IsNullOrEmpty(value) ? UITheme.Success : UITheme.Warning;
        }

        void OpenValidation()
        {
            GameManager.Instance.ValidateNow();
            validationStale = false;
            UIValidation.ShowModal(GameState.LastValidation);
            RefreshCards();
        }

        void TryLaunch()
        {
            var report = GameManager.Instance.ValidateNow();
            validationStale = false;
            if (report == null || !report.CanLaunch)
            {
                UIValidation.ShowModal(report);
                RefreshCards();
                return;
            }
            // Warnings don't block: the engineering layer already knows the outcome.
            // Underfueled configs launch and fail — the debrief teaches what went wrong.
            GameManager.Instance.BeginLaunch();
            ScreenManager.Instance.Show("debrief");
        }
    }

    /// Per-card cached references.
    public class CardRefs : MonoBehaviour
    {
        public TextMeshProUGUI statusLabel;
    }
}
