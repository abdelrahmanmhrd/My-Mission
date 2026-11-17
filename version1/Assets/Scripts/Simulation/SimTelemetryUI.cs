using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;
using SpaceMission.UI;

namespace SpaceMission.Simulation
{
    /// Telemetry HUD for the flight simulation (GDD 10.5): altitude, velocity,
    /// fuel, phase — subordinate to the 3D view.
    public class SimTelemetryUI : MonoBehaviour
    {
        public LaunchSimulation launch;
        TextMeshProUGUI altitudeLabel;
        TextMeshProUGUI velocityLabel;
        TextMeshProUGUI fuelLabel;
        TextMeshProUGUI phaseLabel;
        TextMeshProUGUI cameraNote;
        Slider fuelBar;

        public static SimTelemetryUI Build(Transform parent, LaunchSimulation launchRef)
        {
            var go = new GameObject("SimTelemetry", typeof(RectTransform), typeof(SimTelemetryUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var ui = go.GetComponent<SimTelemetryUI>();
            ui.launch = launchRef;
            ui.BuildContent(rect);
            return ui;
        }

        void BuildContent(RectTransform rect)
        {
            var panel = UITheme.BorderedPanel(rect, "Panel", new Color(0.05f, 0.07f, 0.11f, 0.82f), UITheme.Border, 1);
            panel.anchorMin = new Vector2(0, 1);
            panel.anchorMax = new Vector2(0, 1);
            panel.pivot = new Vector2(0, 1);
            panel.sizeDelta = new Vector2(360, 210);
            panel.anchoredPosition = new Vector2(20, -20);

            phaseLabel = UITheme.Label(panel, "T-MINUS", UITheme.FontH3, UITheme.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            var pRect = phaseLabel.GetComponent<RectTransform>();
            pRect.anchorMin = new Vector2(0, 1);
            pRect.anchorMax = new Vector2(1, 1);
            pRect.pivot = new Vector2(0, 1);
            pRect.offsetMin = new Vector2(18, -40);
            pRect.offsetMax = new Vector2(-18, -10);

            altitudeLabel = MakeRow(panel, "ALTITUDE", -78);
            velocityLabel = MakeRow(panel, "VELOCITY", -106);
            fuelLabel = MakeRow(panel, "FUEL", -134);

            var barBg = UITheme.Panel(panel, "FuelBar", UITheme.BgPanelAlt);
            barBg.anchorMin = new Vector2(0.28f, 1);
            barBg.anchorMax = new Vector2(0.97f, 1);
            barBg.pivot = new Vector2(0, 1);
            barBg.offsetMin = new Vector2(0, -160);
            barBg.offsetMax = new Vector2(0, -148);
            fuelBar = UITheme.HSlider(barBg, 1f, UITheme.Accent);
            fuelBar.interactable = false;

            cameraNote = UITheme.Label(rect, "", UITheme.FontSmall, UITheme.TextMuted, TextAnchor.LowerLeft);
            var cRect = cameraNote.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0, 0);
            cRect.anchorMax = new Vector2(1, 0);
            cRect.pivot = new Vector2(0, 0);
            cRect.offsetMin = new Vector2(24, 14);
            cRect.offsetMax = new Vector2(-24, 40);

            var skip = UITheme.TextButton(rect, "SKIP", UITheme.FontSmall);
            var sRect = skip.GetComponent<RectTransform>();
            sRect.anchorMin = new Vector2(1, 0);
            sRect.anchorMax = new Vector2(1, 0);
            sRect.pivot = new Vector2(1, 0);
            sRect.sizeDelta = new Vector2(110, 40);
            sRect.anchoredPosition = new Vector2(-20, 14);
            skip.onClick.AddListener(() =>
            {
                var dir = MissionSimulationDirector.Instance;
                if (dir != null && dir.phase == SimPhase.Ascent)
                {
                    Time.timeScale = 12f;
                    StartCoroutine(RestoreTimeScale());
                }
            });
        }

        static TextMeshProUGUI MakeRow(RectTransform panel, string title, float y)
        {
            var t = UITheme.Label(panel, title, UITheme.FontTiny, UITheme.TextMuted, TextAnchor.UpperLeft);
            var tRect = t.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0, 1);
            tRect.anchorMax = new Vector2(0.28f, 1);
            tRect.offsetMin = new Vector2(18, y - 16);
            tRect.offsetMax = new Vector2(0, y + 2);

            var v = UITheme.Label(panel, "—", UITheme.FontBody, UITheme.TextPrimary, TextAnchor.UpperRight);
            var vRect = v.GetComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0.28f, 1);
            vRect.anchorMax = new Vector2(1, 1);
            vRect.offsetMin = new Vector2(0, y - 18);
            vRect.offsetMax = new Vector2(-18, y + 2);
            return v;
        }

        IEnumerator RestoreTimeScale()
        {
            yield return new WaitForSeconds(4f);
            Time.timeScale = 1f;
        }

        void Update()
        {
            if (launch == null) return;
            var director = MissionSimulationDirector.Instance;
            if (director != null)
                phaseLabel.text = director.phase switch
                {
                    SimPhase.LaunchPad => "T-MINUS — IGNITION SEQUENCE",
                    SimPhase.Ascent => "ASCENT",
                    SimPhase.OrbitAchieved => "ORBIT ACHIEVED",
                    SimPhase.Failure => "VEHICLE FAILURE",
                    _ => director.phase.ToString().ToUpperInvariant()
                };
            altitudeLabel.text = $"{launch.AltitudeKm:0.0} km";
            velocityLabel.text = $"{launch.VelocityMS:0} m/s";
            fuelLabel.text = $"{launch.FuelFraction * 100f:0}%";
            fuelBar.value = launch.FuelFraction;

            var cfg = GameState.Config;
            var db = GameState.DB;
            if (cfg != null && db != null)
            {
                var hasCamera = false;
                foreach (var id in cfg.instrumentIds)
                {
                    var inst = db.GetInstrument(id);
                    if (inst != null && inst.effect == "cameraControl") hasCamera = true;
                }
                cameraNote.text = hasCamera
                    ? "Right-drag to orbit the camera · scroll to zoom"
                    : "Camera locked — no mast camera on the payload (add Zoom Camera Mast for free camera)";
            }
        }
    }
}
