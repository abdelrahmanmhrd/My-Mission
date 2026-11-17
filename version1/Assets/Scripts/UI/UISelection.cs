using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;
using SpaceMission.Data;
using SpaceMission.Engineering;

namespace SpaceMission.UI
{
    /// Generic catalog + detail selection page (GDD 7): list on the left,
    /// details on the right, confirm control, back navigation.
    public class UISelection : MonoBehaviour
    {
        public static ConfigCategory CurrentCategory { get; set; } = ConfigCategory.Engines;

        Transform catalogContent;
        RectTransform detailPanel;
        TextMeshProUGUI detailTitle;
        TextMeshProUGUI detailBody;
        Button confirmButton;
        ModelPreviewViewport previewViewport;
        GameObject stageRow;
        TextMeshProUGUI stageLabel;
        Slider fuelSlider;
        TextMeshProUGUI fuelAmountLabel;

        int selectedStage = 0;
        string pendingSelectionId;
        Data.MissionData mission => GameState.CurrentMission;

        public static UISelection Build(Transform parent)
        {
            var screen = new GameObject("SelectionScreen", typeof(RectTransform), typeof(UISelection));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var page = screen.GetComponent<UISelection>();
            page.BuildContent(rect);
            return page;
        }

        void BuildContent(RectTransform rect)
        {
            var header = UITheme.Panel(rect, "Header", UITheme.BgPanel);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0, 80);
            header.anchoredPosition = Vector2.zero;

            var backBtn = UITheme.TextButton(header, "< BACK TO CONFIGURATION", UITheme.FontSmall);
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0);
            backRect.anchorMax = new Vector2(0, 1);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.sizeDelta = new Vector2(300, 0);
            backRect.offsetMin = new Vector2(24, 14);
            backRect.offsetMax = new Vector2(324, -14);
            backBtn.onClick.AddListener(() =>
            {
                if (GameState.Config != null) GameState.Config.NotifyChanged();
                ScreenManager.Instance.Back();
            });

            pageTitle = UITheme.Label(header, "", UITheme.FontH2, UITheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Left: catalog scroll list
            var catalogPanel = UITheme.Panel(rect, "Catalog", UITheme.BgDeep);
            catalogPanel.anchorMin = new Vector2(0, 0);
            catalogPanel.anchorMax = new Vector2(0, 1);
            catalogPanel.pivot = new Vector2(0, 0.5f);
            catalogPanel.offsetMin = new Vector2(16, 24);
            catalogPanel.offsetMax = new Vector2(560, -100);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            var scroll = scrollGo.GetComponent<RectTransform>();
            scroll.SetParent(catalogPanel, false);
            scroll.anchorMin = Vector2.zero;
            scroll.anchorMax = Vector2.one;
            scroll.offsetMin = new Vector2(10, 10);
            scroll.offsetMax = new Vector2(-10, -10);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scroll, false);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.offsetMin = Vector2.zero;
            viewport.offsetMax = Vector2.zero;
            viewportGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var content = contentGo.GetComponent<RectTransform>();
            content.SetParent(viewport, false);
            content.anchorMin = new Vector2(0, 1);
            content.anchorMax = new Vector2(1, 1);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;
            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.padding = new RectOffset(0, 0, 4, 16);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scroll.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.scrollSensitivity = 28;

            catalogContent = content;

            // Right: detail panel
            detailPanel = UITheme.BorderedPanel(rect, "Detail", UITheme.BgPanel, UITheme.Border, 1);
            detailPanel.anchorMin = new Vector2(0, 0);
            detailPanel.anchorMax = new Vector2(1, 1);
            detailPanel.pivot = new Vector2(0, 0.5f);
            detailPanel.offsetMin = new Vector2(592, 24);
            detailPanel.offsetMax = new Vector2(-16, -100);

            detailTitle = UITheme.Label(detailPanel, "Select an item", UITheme.FontH2, UITheme.TextPrimary, TextAnchor.UpperLeft, FontStyle.Bold);
            // 3D model preview viewport (engines / vehicles / spacecraft / pads / payload / fuel tanks)
            previewViewport = ModelPreviewViewport.Create(detailPanel, "3D PREVIEW");
            var titleRect = detailTitle.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.offsetMin = new Vector2(24, -52);
            titleRect.offsetMax = new Vector2(-24, -8);

            detailBody = UITheme.Label(detailPanel, "", UITheme.FontBody, UITheme.TextMuted, TextAnchor.UpperLeft);
            var bodyRect = detailBody.GetComponent<RectTransform>();
            bodyRect.anchorMin = new Vector2(0, 0);
            bodyRect.anchorMax = new Vector2(1, 1);
            bodyRect.offsetMin = new Vector2(24, 140);
            bodyRect.offsetMax = new Vector2(-24, -70);

            confirmButton = UITheme.TextButton(detailPanel, "CONFIRM SELECTION", UITheme.FontBody, UITheme.BtnAccent, UITheme.TextPrimary, UITheme.Accent);
            var confirmRect = confirmButton.GetComponent<RectTransform>();
            confirmRect.anchorMin = new Vector2(1, 0);
            confirmRect.anchorMax = new Vector2(1, 0);
            confirmRect.pivot = new Vector2(1, 0);
            confirmRect.sizeDelta = new Vector2(320, 52);
            confirmRect.anchoredPosition = new Vector2(-24, 20);
            confirmButton.interactable = false;
            confirmButton.onClick.AddListener(OnConfirm);
        }

        TextMeshProUGUI pageTitle;

        void OnEnable()
        {
            if (catalogContent == null) return;   // OnEnable fires during construction, before BuildContent
            pageTitle.text = CurrentCategory switch
            {
                ConfigCategory.Engines => "ENGINES",
                ConfigCategory.Fuels => "FUELS",
                ConfigCategory.Rockets => "ROCKETS — LAUNCH VEHICLE & SPACECRAFT",
                ConfigCategory.Launchpads => "LAUNCHPADS",
                ConfigCategory.Payload => "PAYLOAD — SCIENTIFIC INSTRUMENTS",
                _ => ""
            };
            RebuildCatalog();
        }

        void RebuildCatalog()
        {
            if (previewViewport != null) previewViewport.ClearModel();
            // DestroyImmediate: Destroy() is deferred one frame, which left stale
            // duplicate entries (with live click handlers) in the catalog — the
            // cause of selections applying to the wrong item (user-reported bug).
            for (var i = catalogContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(catalogContent.GetChild(i).gameObject);

            var cfg = GameState.Config;
            var db = GameState.DB;
            if (cfg == null || db == null) return;

            switch (CurrentCategory)
            {
                case ConfigCategory.Engines: CatalogEngines(db); break;
                case ConfigCategory.Fuels: CatalogFuels(db); break;
                case ConfigCategory.Rockets: CatalogRockets(db); break;
                case ConfigCategory.Launchpads: CatalogLaunchpads(db); break;
                case ConfigCategory.Payload: CatalogPayload(db); break;
            }
        }

        GameObject AddCatalogItem(string title, string subtitle, bool selected, bool disabled, Action onClick)
        {
            var item = UITheme.BorderedPanel(catalogContent, "Item",
                selected ? UITheme.CardSelected : UITheme.BgPanel,
                selected ? UITheme.Accent : UITheme.Border, 1, raycast: true);
            var layout = item.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 84;

            if (!disabled)
            {
                var btn = item.gameObject.AddComponent<Button>();
                btn.transition = Selectable.Transition.None;
                btn.onClick.AddListener(() => onClick());
            }

            var titleLabel = UITheme.Label(item, title, UITheme.FontBody,
                disabled ? UITheme.TextMuted : UITheme.TextPrimary);
            var tRect = titleLabel.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0, 0.5f);
            tRect.anchorMax = new Vector2(1, 1);
            tRect.offsetMin = new Vector2(18, 6);
            tRect.offsetMax = new Vector2(-110, -6);

            var subLabel = UITheme.Label(item, subtitle, UITheme.FontSmall, UITheme.TextMuted);
            var sRect = subLabel.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(0, 0);
            sRect.anchorMax = new Vector2(1, 0.5f);
            sRect.offsetMin = new Vector2(18, 4);
            sRect.offsetMax = new Vector2(-110, -4);

            var stateLabel = UITheme.Label(item, disabled ? "LOCKED" : selected ? "SELECTED" : "",
                UITheme.FontSmall, disabled ? UITheme.TextMuted : UITheme.Success, TextAnchor.MiddleRight);
            var stRect = stateLabel.GetComponent<RectTransform>();
            stRect.anchorMin = new Vector2(1, 0);
            stRect.anchorMax = new Vector2(1, 1);
            stRect.pivot = new Vector2(1, 0.5f);
            stRect.offsetMin = new Vector2(-100, 8);
            stRect.offsetMax = new Vector2(-14, -8);

            return item.gameObject;
        }

        void ShowDetail(string title, string body, bool confirmable, string confirmText = "CONFIRM SELECTION")
        {
            detailTitle.text = title;
            detailBody.text = body;
            confirmButton.interactable = confirmable;
            var label = confirmButton.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null) label.text = confirmText;
        }

        void DestroyTransient()
        {
            if (stageRow != null) { DestroyImmediate(stageRow); stageRow = null; }
            if (fuelSlider != null) { DestroyImmediate(fuelSlider.gameObject); fuelSlider = null; fuelAmountLabel = null; }
            if (rocketsTabLabel != null) { DestroyImmediate(rocketsTabLabel.transform.parent.gameObject); rocketsTabLabel = null; }
        }

        // ------------------------------------------------------------------ ENGINES

        void CatalogEngines(GameDatabase db)
        {
            DestroyTransient();
            ShowDetail("ENGINES", "Choose an engine, then assign it to a vehicle stage. Booster engines provide liftoff thrust; upper-stage engines provide in-space performance.", false);
            selectedStage = Mathf.Clamp(selectedStage, 0, Math.Max(0, GameState.Config.stages.Count - 1));
            BuildStageRow();
            RefreshStageRow();

            var vehicle = GameState.Config.Vehicle(db);
            foreach (var engine in db.engines)
            {
                if (engine.stageRole == "spacecraft") continue;
                var compatible = vehicle == null || engine.compatibleVehicles == null ||
                                 Array.Exists(engine.compatibleVehicles, v => v == vehicle.id);
                var fuel = db.GetFuel(engine.fuelId);
                var subtitle = $"{Formats.Thrust(engine.thrustVacKN)} vac  ·  Isp {engine.ispVacS:0}s  ·  {fuel?.name ?? engine.fuelId}";
                var assigned = AssignedToStageSummary(engine.id);
                AddCatalogItem(engine.name + (assigned.Length > 0 ? $"  [{assigned}]" : ""),
                    subtitle, IsEngineSelected(engine.id), !compatible,
                    () => { pendingSelectionId = engine.id; ShowEngineDetail(db, engine); });
            }
        }

        string AssignedToStageSummary(string engineId)
        {
            var parts = new List<string>();
            for (var i = 0; i < GameState.Config.stages.Count; i++)
            {
                var count = GameState.Config.stages[i].engineIds.Count;
                var contains = GameState.Config.stages[i].engineIds.Contains(engineId);
                if (contains)
                    parts.Add(i == 0 ? "S1" : $"S{i + 1}×{count}");
            }
            return string.Join(" ", parts.ToArray());
        }

        bool IsEngineSelected(string engineId)
        {
            foreach (var stage in GameState.Config.stages)
                if (stage.engineIds.Contains(engineId)) return true;
            return false;
        }

        void ShowEngineDetail(GameDatabase db, Data.EngineData engine)
        {
            var fuel = db.GetFuel(engine.fuelId);
            var vehicle = GameState.Config.Vehicle(db);

            var sb = new StringBuilder();
            sb.AppendLine(engine.description);
            sb.AppendLine();
            sb.AppendLine($"Thrust:  {Formats.Thrust(engine.thrustSlKN)} sea-level / {Formats.Thrust(engine.thrustVacKN)} vacuum");
            sb.AppendLine($"Efficiency (Isp):  {engine.ispVacS:0} s  ·  Fuel:  {fuel?.name}");
            sb.AppendLine($"Engine mass:  {Formats.Mass(engine.dryMassKg)}   ·   Cost:  {Formats.Money(engine.costUSD)}");
            sb.AppendLine();
            sb.AppendLine("Why it matters:  thrust must exceed weight at liftoff (TWR ≥ 1.05), while Isp decides how much delta-v each tonne of fuel delivers.");
            if (vehicle == null)
                sb.AppendLine("\nSelect a launch vehicle first — engines are assigned to vehicle stage slots.");

            ShowDetail($"ENGINE — {engine.name}", sb.ToString(), vehicle != null,
                vehicle != null ? "ASSIGN TO STAGE" : "SELECT VEHICLE FIRST");
            if (previewViewport != null) previewViewport.ShowModel(ModelLibrary.EngineModelPath(engine.id));
            if (vehicle != null)
            {
                BuildStageRow();
                RefreshStageRow();
            }
        }

        void BuildStageRow()
        {
            if (stageRow != null) DestroyImmediate(stageRow);
            var row = new GameObject("StageRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            var rowRect = row.GetComponent<RectTransform>();
            rowRect.SetParent(detailPanel, false);
            rowRect.anchorMin = new Vector2(0, 0);
            rowRect.anchorMax = new Vector2(1, 0);
            rowRect.pivot = new Vector2(0, 0);
            rowRect.offsetMin = new Vector2(24, 92);
            rowRect.offsetMax = new Vector2(-24, 140);

            var hlg = row.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 10;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandHeight = true;

            stageRow = row;
            stageLabel = UITheme.Label(rowRect, "Assign to:", UITheme.FontBody, UITheme.TextMuted, TextAnchor.MiddleRight);
            stageLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(120, 0);
        }

        void RefreshStageRow()
        {
            // clear old stage buttons
            while (stageRow.transform.childCount > 1)
                DestroyImmediate(stageRow.transform.GetChild(stageRow.transform.childCount - 1).gameObject);

            var vehicle = GameState.Config.Vehicle(GameState.DB);
            if (vehicle == null) return;

            for (var i = 0; i < vehicle.stages.Length; i++)
            {
                var index = i;
                var stageData = vehicle.stages[i];
                var roleNeeded = stageData.boosterSlots > 0 ? "booster" : "upper";
                var isCurrent = selectedStage == index;
                var btn = UITheme.TextButton(stageRow.transform,
                    $"STAGE {index + 1}{(isCurrent ? " ●" : "")}", UITheme.FontSmall,
                    isCurrent ? UITheme.BtnAccent : UITheme.BtnNormal, UITheme.TextPrimary,
                    isCurrent ? UITheme.Accent : UITheme.Border);
                btn.interactable = IsEngineRoleCompatible(pendingSelectionId, roleNeeded);
                var element = btn.gameObject.AddComponent<LayoutElement>();
                element.preferredWidth = 110;
                btn.onClick.AddListener(() =>
                {
                    selectedStage = index;
                    if (pendingSelectionId != null) ShowEngineDetail(GameState.DB, GameState.DB.GetEngine(pendingSelectionId));
                    RefreshStageRow();
                });
            }
        }

        bool IsEngineRoleCompatible(string engineId, string roleNeeded)
        {
            var engine = GameState.DB.GetEngine(engineId);
            if (engine == null) return true;
            return engine.stageRole == roleNeeded || engine.stageRole == "any";
        }

        // ------------------------------------------------------------------ FUELS

        void CatalogFuels(GameDatabase db)
        {
            DestroyTransient();
            ShowDetail("FUELS", "Pick a propellant type and load amount for the selected stage. Amount is a trade-off: more fuel means more delta-v but also more mass and cost.", false);
            selectedStage = Mathf.Clamp(selectedStage, 0, Math.Max(0, GameState.Config.stages.Count - 1));
            BuildStageRow();
            RefreshStageRow();

            foreach (var fuel in db.fuels)
            {
                var subtitle = $"Isp ×{fuel.ispMultiplier:0.00}  ·  tanks ×{fuel.tankMassFactor:0.00}  ·  {Formats.Money(fuel.costPerFU)}/FU";
                var currentlyLoaded = selectedStage < GameState.Config.stages.Count &&
                                      GameState.Config.stages[selectedStage].fuelId == fuel.id;
                AddCatalogItem(fuel.name, subtitle, currentlyLoaded, false,
                    () => { pendingSelectionId = fuel.id; ShowFuelDetail(db, fuel); });
            }
        }

        void ShowFuelDetail(GameDatabase db, Data.FuelData fuel)
        {
            // tank preview in both branches
            if (previewViewport != null)
                previewViewport.ShowFuelTank(fuel.id == "FUEL-HYDROLOX" ? new UnityEngine.Color(0.65f, 0.75f, 0.95f)
                    : fuel.id == "FUEL-METHALOX" ? new UnityEngine.Color(0.55f, 0.8f, 0.6f)
                    : fuel.id == "FUEL-HYPERGOLIC" ? new UnityEngine.Color(0.8f, 0.55f, 0.25f)
                    : new UnityEngine.Color(0.75f, 0.55f, 0.35f), fuel.name);

            // Fuel can only be loaded once a vehicle (and its stages) exists
            if (GameState.Config.stages.Count == 0)
            {
                ShowDetail($"FUEL — {fuel.name}",
                    fuel.description + "\n\nSelect a launch vehicle first: fuel is loaded into vehicle stages.",
                    false, "NO VEHICLE SELECTED");
                return;
            }

            var stageIdx = Mathf.Clamp(selectedStage, 0, GameState.Config.stages.Count - 1);
            var vehicle = GameState.Config.Vehicle(db);
            var stageData = vehicle.stages[stageIdx];

            var sb = new StringBuilder();
            sb.AppendLine(fuel.description);
            sb.AppendLine();
            sb.AppendLine($"Efficiency multiplier:  ×{fuel.ispMultiplier:0.00} on compatible engines");
            sb.AppendLine($"Tank mass factor:  {fuel.tankMassFactor:0.00} t of tank per FU (heavier tanks eat payload capacity)");
            sb.AppendLine($"Price:  {Formats.Money(fuel.costPerFU)} per FU (1 FU = 1 tonne)");
            sb.AppendLine();
            sb.AppendLine($"Stage capacity:  {Formats.Fuel(stageData.maxFuelFU)}");

            ShowDetail($"FUEL — {fuel.name}", sb.ToString(), true, "LOAD FUEL");

            // Fuel amount slider
            if (fuelSlider == null || fuelSlider.transform.parent != detailPanel)
            {
                if (fuelSlider != null) DestroyImmediate(fuelSlider.gameObject);
                var sliderGo = new GameObject("FuelSlider", typeof(RectTransform));
                var sRect = sliderGo.GetComponent<RectTransform>();
                sRect.SetParent(detailPanel, false);
                sRect.anchorMin = new Vector2(0, 0);
                sRect.anchorMax = new Vector2(1, 0);
                sRect.pivot = new Vector2(0, 0);
                sRect.offsetMin = new Vector2(24, 96);
                sRect.offsetMax = new Vector2(-24, 140);
                fuelSlider = UITheme.HSlider(sRect, 0.5f, UITheme.Accent);
                fuelSlider.onValueChanged.AddListener(_ => { UpdateFuelLabel(); });
                fuelAmountLabel = UITheme.Label(sRect, "", UITheme.FontSmall, UITheme.TextPrimary, TextAnchor.MiddleRight);
                var laRect = fuelAmountLabel.GetComponent<RectTransform>();
                laRect.anchorMin = new Vector2(0, 1);
                laRect.anchorMax = new Vector2(1, 1);
                laRect.pivot = new Vector2(0.5f, 0);
                laRect.offsetMin = new Vector2(0, 14);
                laRect.offsetMax = new Vector2(0, 40);
            }

            var currentAmount = GameState.Config.stages[stageIdx].fuelAmountFU;
            // default to full capacity for a stage the player hasn't loaded yet
            fuelSlider.SetValueWithoutNotify(currentAmount > 0f && stageData.maxFuelFU > 0
                ? currentAmount / stageData.maxFuelFU
                : 1f);
            UpdateFuelLabel();
        }

        void UpdateFuelLabel()
        {
            var stageIdx = Mathf.Clamp(selectedStage, 0, GameState.Config.stages.Count - 1);
            var vehicle = GameState.Config.Vehicle(GameState.DB);
            if (vehicle == null || vehicle.stages.Length <= stageIdx) return;
            var max = vehicle.stages[stageIdx].maxFuelFU;
            var amount = fuelSlider.value * max;
            fuelAmountLabel.text = $"{Formats.Fuel(amount)}  of  {Formats.Fuel(max)}";
        }

        // ------------------------------------------------------------------ ROCKETS (vehicle + spacecraft)

        bool rocketsVehicleTab = true;
        TextMeshProUGUI rocketsTabLabel;

        void CatalogRockets(GameDatabase db)
        {
            DestroyTransient();
            selectedStage = 0;

            // Tab switch
            if (rocketsTabLabel == null || rocketsTabLabel.transform.parent != detailPanel)
            {
                var tabBtn = UITheme.TextButton(detailPanel, "", UITheme.FontSmall);
                var tRect = tabBtn.GetComponent<RectTransform>();
                tRect.anchorMin = new Vector2(1, 1);
                tRect.anchorMax = new Vector2(1, 1);
                tRect.pivot = new Vector2(1, 1);
                tRect.sizeDelta = new Vector2(240, 44);
                tRect.anchoredPosition = new Vector2(-24, -8);
                rocketsTabLabel = tabBtn.GetComponentInChildren<TextMeshProUGUI>(true);
                tabBtn.onClick.AddListener(() =>
                {
                    rocketsVehicleTab = !rocketsVehicleTab;
                    RebuildCatalog();
                });
            }
            rocketsTabLabel.text = rocketsVehicleTab ? "SWITCH: SPACECRAFT" : "SWITCH: LAUNCH VEHICLES";
            rocketsTabLabel.raycastTarget = false;

            if (rocketsVehicleTab)
            {
                ShowDetail("LAUNCH VEHICLE", "The vehicle defines your stages, engine slots, payload capacity and cost. Its capacity must cover the spacecraft plus instruments.", false);
                foreach (var vehicle in db.vehicles)
                {
                    var subtitle = $"LEO {vehicle.payloadLEOT:0.#}t  ·  Moon {vehicle.payloadTLIT:0.#}t  ·  Mars {vehicle.payloadTMIT:0.#}t  ·  {Formats.Money(vehicle.costUSD)}{(vehicle.crewRated ? "  ·  HUMAN-RATED" : "")}";
                    AddCatalogItem(vehicle.name, subtitle, GameState.Config.vehicleId == vehicle.id, false,
                        () => { pendingSelectionId = vehicle.id; ShowVehicleDetail(vehicle); });
                }
            }
            else
            {
                var needsCrew = mission != null && mission.missionType == "crewed";
                ShowDetail("SPACECRAFT", needsCrew
                    ? "This is a human-led mission: only crewed spacecraft are eligible."
                    : "This is an automated mission: only uncrewed spacecraft are eligible.", false);
                foreach (var craft in db.spacecraft)
                {
                    var allowed = mission == null || craft.crewed == (mission.missionType == "crewed");
                    var subtitle = $"{(craft.crewed ? "Crewed" : "Automated")}  ·  {craft.crewCapacity} crew max  ·  {craft.massT:0.#} t  ·  {Formats.Money(craft.costUSD)}";
                    AddCatalogItem(craft.name, subtitle, GameState.Config.spacecraftId == craft.id, !allowed,
                        () => { pendingSelectionId = craft.id; ShowSpacecraftDetail(craft); });
                }
            }
        }

        void ShowVehicleDetail(Data.VehicleData vehicle)
        {
            var sb = new StringBuilder();
            sb.AppendLine(vehicle.description);
            sb.AppendLine();
            sb.AppendLine($"Payload capacity:  LEO {vehicle.payloadLEOT:0.#} t  ·  to the Moon {vehicle.payloadTLIT:0.#} t  ·  to Mars {vehicle.payloadTMIT:0.#} t");
            sb.AppendLine($"Crew-rated:  {(vehicle.crewRated ? "Yes" : "No")}   ·   Cost:  {Formats.Money(vehicle.costUSD)}");
            sb.AppendLine();
            sb.AppendLine("Stage plan:");
            for (var i = 0; i < vehicle.stages.Length; i++)
            {
                var s = vehicle.stages[i];
                var role = s.boosterSlots > 0 ? $"{s.boosterSlots} booster engine slot(s)" : $"{s.upperSlots} upper engine slot(s)";
                var solids = s.fixedSolidBoosters > 0 ? $" + {s.fixedSolidBoosters} fixed solid booster(s)" : "";
                sb.AppendLine($"  Stage {i + 1} ({s.stageName}):  {role}{solids}  ·  fuel capacity {Formats.Fuel(s.maxFuelFU)}");
            }
            ShowDetail($"LAUNCH VEHICLE — {vehicle.name}", sb.ToString(), true);
            if (previewViewport != null) previewViewport.ShowModel(ModelLibrary.VehicleModelPath(vehicle.id));
        }

        void ShowSpacecraftDetail(Data.SpacecraftData craft)
        {
            var sb = new StringBuilder();
            sb.AppendLine(craft.description);
            sb.AppendLine();
            sb.AppendLine($"Crew capacity:  {craft.crewCapacity}   ·   Mass:  {craft.massT:0.#} t   ·   Cost:  {Formats.Money(craft.costUSD)}");
            sb.AppendLine($"Arrival burn capability:  {Formats.DeltaV(craft.arrivalDvMS)}   ·   Return capability:  {Formats.DeltaV(craft.returnDvMS)}");
            sb.AppendLine($"Power generation:  {Formats.Power(craft.powerGenerationW)}   ·   Downlink:  {Formats.Data(craft.commCapacityGbPerDay)}");
            sb.AppendLine();
            sb.AppendLine("Why it matters:  instruments consume power and produce data — the spacecraft must generate enough electricity and downlink bandwidth for everything you load onto it.");
            sb.AppendLine("The arrival/return capabilities must cover the mission's burn requirements — a spacecraft that cannot brake or come home fails the mission.");
            ShowDetail($"SPACECRAFT — {craft.name}", sb.ToString(), true);
            if (previewViewport != null) previewViewport.ShowModel(ModelLibrary.SpacecraftModelPath(craft.id));
        }

        // ------------------------------------------------------------------ LAUNCHPADS

        void CatalogLaunchpads(GameDatabase db)
        {
            DestroyTransient();
            ShowDetail("LAUNCHPADS", "The facility must physically support your launch vehicle, and crewed missions need a pad with crew-access infrastructure.", false);
            var vehicleId = GameState.Config.vehicleId;
            foreach (var pad in db.launchpads)
            {
                var compatible = vehicleId == null || vehicleId.Length == 0 ||
                                 Array.Exists(pad.compatibleVehicles, v => v == vehicleId);
                var subtitle = $"{(pad.crewInfrastructure ? "Crew infrastructure" : "Cargo only")}  ·  {Formats.Money(pad.costUSD)}";
                AddCatalogItem(pad.name, subtitle, GameState.Config.launchpadId == pad.id, !compatible,
                    () => { pendingSelectionId = pad.id; ShowLaunchpadDetail(pad); });
            }
        }

        void ShowLaunchpadDetail(Data.LaunchpadData pad)
        {
            var sb = new StringBuilder();
            sb.AppendLine(pad.description);
            sb.AppendLine();
            sb.AppendLine($"Supports:  {string.Join(", ", pad.compatibleVehicles)}");
            sb.AppendLine($"Crew infrastructure:  {(pad.crewInfrastructure ? "Yes" : "No")}   ·   Cost:  {Formats.Money(pad.costUSD)}");
            sb.AppendLine();
            sb.AppendLine("Why it matters:  a pad that cannot handle your vehicle cannot launch it, and astronauts need a crew access arm.");
            ShowDetail($"LAUNCH FACILITY — {pad.name}", sb.ToString(), true);
            if (previewViewport != null) previewViewport.ShowModel(ModelLibrary.LaunchpadModelPath(pad.id));
        }

        // ------------------------------------------------------------------ PAYLOAD (instruments)

        void CatalogPayload(GameDatabase db)
        {
            DestroyTransient();
            ShowDetail("SCIENTIFIC INSTRUMENTS", "Every instrument adds science value but also mass, power, data and cost. Missions require specific instruments — check the briefing requirements.", false);

            foreach (var inst in db.instruments)
            {
                var relevant = mission == null || mission.instrumentRequirements == null ||
                               IsInstrumentRelevant(inst, mission);
                var subtitle = $"{Formats.Mass(inst.massKg)}  ·  {inst.powerW:0} W  ·  science {inst.scienceValue:0}  ·  {Formats.Money(inst.costUSD)}";
                var tagText = inst.effect == "cameraControl" ? "  [unlocks free camera]"
                    : inst.effect == "crewOxygen" ? "  [crew life support]" : "";
                AddCatalogItem(inst.name + tagText, subtitle,
                    GameState.Config.instrumentIds.Contains(inst.id), !relevant,
                    () => { pendingSelectionId = inst.id; ShowInstrumentDetail(inst); });
            }
        }

        static bool IsInstrumentRelevant(Data.InstrumentData inst, Data.MissionData m)
        {
            if (m.instrumentRequirements == null) return true;
            var required = new List<string>();
            foreach (var req in m.instrumentRequirements)
                if (req.type == "required" && req.ids != null) required.AddRange(req.ids);
            // relevant if tagged for this mission or explicitly required
            return required.Contains(inst.id) ||
                   (inst.missionTags != null && Array.Exists(inst.missionTags, t => t == m.id));
        }

        void ShowInstrumentDetail(Data.InstrumentData inst)
        {
            var selected = GameState.Config.instrumentIds.Contains(inst.id);
            var sb = new StringBuilder();
            sb.AppendLine(inst.description);
            sb.AppendLine();
            sb.AppendLine($"Mass:  {Formats.Mass(inst.massKg)}   ·   Power:  {inst.powerW:0} W   ·   Data:  {inst.dataGbPerDay:0.0} Gb/day");
            sb.AppendLine($"Science value:  {inst.scienceValue:0}   ·   Cost:  {Formats.Money(inst.costUSD)}");
            sb.AppendLine();
            if (inst.effect == "cameraControl")
                sb.AppendLine("Effect:  without a mast camera the flight camera stays in a fixed position.");
            if (inst.effect == "crewOxygen")
                sb.AppendLine("Effect:  required on crewed missions — the crew breathes the oxygen it generates.");
            sb.AppendLine();
            sb.AppendLine(selected ? "Currently SELECTED on the payload." : "Currently NOT selected.");
            ShowDetail($"INSTRUMENT — {inst.name}", sb.ToString(), true, selected ? "DESELECT INSTRUMENT" : "SELECT INSTRUMENT");
            if (previewViewport != null)
                previewViewport.ShowInstrumentBox(inst.effect == "crewOxygen"
                    ? new UnityEngine.Color(0.3f, 0.75f, 0.5f)
                    : inst.effect == "cameraControl" ? new UnityEngine.Color(0.4f, 0.6f, 0.9f)
                    : new UnityEngine.Color(0.55f, 0.5f, 0.65f));
        }

        // ------------------------------------------------------------------ CONFIRM

        void OnConfirm()
        {
            var cfg = GameState.Config;
            var db = GameState.DB;
            if (cfg == null || pendingSelectionId == null) return;

            switch (CurrentCategory)
            {
                case ConfigCategory.Engines:
                {
                    var engine = db.GetEngine(pendingSelectionId);
                    var vehicle = cfg.Vehicle(db);
                    if (engine != null && vehicle != null && selectedStage < vehicle.stages.Length)
                    {
                        var stageData = vehicle.stages[selectedStage];
                        var slotCount = stageData.boosterSlots + stageData.upperSlots;
                        var ids = new List<string>();
                        for (var i = 0; i < slotCount; i++) ids.Add(engine.id);
                        cfg.SetStageEngines(selectedStage, ids);
                    }
                    RebuildCatalog();
                    break;
                }
                case ConfigCategory.Fuels:
                {
                    var fuel = db.GetFuel(pendingSelectionId);
                    var vehicle = cfg.Vehicle(db);
                    if (fuel != null && vehicle != null && selectedStage < vehicle.stages.Length)
                    {
                        cfg.SetStageFuel(selectedStage, fuel.id);
                        var max = vehicle.stages[selectedStage].maxFuelFU;
                        cfg.SetStageFuelAmount(selectedStage, fuelSlider != null ? fuelSlider.value * max : max);
                    }
                    RebuildCatalog();
                    break;
                }
                case ConfigCategory.Rockets:
                    if (rocketsVehicleTab)
                    {
                        cfg.SetVehicle(pendingSelectionId, db);
                        selectedStage = 0;
                    }
                    else
                        cfg.SetSpacecraft(pendingSelectionId);
                    RebuildCatalog();
                    break;
                case ConfigCategory.Launchpads:
                    cfg.SetLaunchpad(pendingSelectionId);
                    RebuildCatalog();
                    break;
                case ConfigCategory.Payload:
                    cfg.ToggleInstrument(pendingSelectionId);
                    RebuildCatalog();
                    break;
            }
            ShowDetail("Selection updated", "Pick another item or go back to the configuration screen.", false);
        }
    }
}
