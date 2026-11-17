using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;
using SpaceMission.Engineering;

namespace SpaceMission.UI
{
    /// Modal validation report: blocking issues with reasons and hints (GDD 6.7 / 10).
    public class UIValidation : MonoBehaviour
    {
        static GameObject modal;
        static Transform content;

        public static void ShowModal(ValidationReport report)
        {
            if (modal == null)
                BuildModal();
            modal.SetActive(true);

            for (var i = content.childCount - 1; i >= 0; i--)
                DestroyImmediate(content.GetChild(i).gameObject);

            var title = modal.GetComponentInChildren<TextMeshProUGUI>(true);
            if (report == null) return;

            if (report.CanLaunch)
            {
                title.text = "READY FOR LAUNCH";
                title.color = UITheme.Success;
                AddLine("All mission constraints pass: budget, payload capacity, thrust, compatibility and instrument requirements.",
                    UITheme.Success);
                foreach (var issue in report.issues)
                    AddLine($"{issue.message}\n{issue.hint}", UITheme.TextMuted);
                AddLaunchButton();
            }
            else
            {
                title.text = report.HasErrors ? "VALIDATION FAILED" : "VALIDATION REQUIRED";
                title.color = report.HasErrors ? UITheme.Danger : UITheme.Warning;
                if (report.issues.Count == 0)
                    AddLine("No blocking issues found. Complete the remaining selections.", UITheme.TextPrimary);
                foreach (var issue in report.issues)
                    AddLine($"{issue.message}\n→ {issue.hint}",
                        issue.severity == ValidationSeverity.Error ? UITheme.Danger
                        : issue.severity == ValidationSeverity.Warning ? UITheme.Warning : UITheme.TextMuted);
            }
        }

        static void BuildModal()
        {
            var root = GameObject.Find("UICanvas");
            if (root == null) return;

            modal = new GameObject("ValidationModal", typeof(RectTransform), typeof(Image));
            var rect = modal.GetComponent<RectTransform>();
            rect.SetParent(root.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            modal.GetComponent<Image>().color = new Color(0, 0, 0, 0.72f);

            var panel = UITheme.BorderedPanel(rect, "Panel", UITheme.BgPanel, UITheme.AccentDim, 2);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(980, 640);

            var titleGo = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.SetParent(panel, false);
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.offsetMin = new Vector2(28, -60);
            titleRect.offsetMax = new Vector2(-28, -14);
            var title = titleGo.GetComponent<TextMeshProUGUI>();
            title.fontSize = UITheme.FontH2;
            title.fontStyle = FontStyles.Bold;
            title.alignment = TextAlignmentOptions.Left;
            title.raycastTarget = false;

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(panel, false);
            viewport.anchorMin = new Vector2(0, 0);
            viewport.anchorMax = new Vector2(1, 1);
            viewport.offsetMin = new Vector2(28, 84);
            viewport.offsetMax = new Vector2(-28, -76);
            viewportGo.GetComponent<Image>().color = new Color(0, 0, 0, 0.01f);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            var scroll = scrollGo.GetComponent<RectTransform>();
            scroll.SetParent(viewport, false);
            scroll.anchorMin = Vector2.zero;
            scroll.anchorMax = Vector2.one;
            scroll.offsetMin = Vector2.zero;
            scroll.offsetMax = Vector2.zero;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.SetParent(viewport, false);
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;
            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 12;
            vlg.padding = new RectOffset(0, 0, 8, 20);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scroll.GetComponent<ScrollRect>();
            sr.content = contentRect;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.scrollSensitivity = 28;

            content = contentRect;

            var closeBtn = UITheme.TextButton(panel, "CLOSE", UITheme.FontBody);
            var cRect = closeBtn.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0);
            cRect.anchorMax = new Vector2(0.5f, 0);
            cRect.pivot = new Vector2(0.5f, 0);
            cRect.sizeDelta = new Vector2(260, 48);
            cRect.anchoredPosition = new Vector2(0, 18);
            closeBtn.onClick.AddListener(() => modal.SetActive(false));

            modal.SetActive(false);
        }

        static void AddLine(string text, Color color)
        {
            var label = UITheme.Label(content, text, UITheme.FontBody, color, TextAnchor.UpperLeft);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            label.gameObject.AddComponent<AutoHeight>().padding = 10f;
        }

        static void AddLaunchButton()
        {
            var btn = UITheme.TextButton(content, "PROCEED TO LAUNCH", UITheme.FontH3,
                UITheme.BtnAccent, UITheme.TextPrimary, UITheme.Accent);
            var rect = btn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 64);
            btn.onClick.AddListener(() =>
            {
                modal.SetActive(false);
                GameManager.Instance.BeginLaunch();
                if (UnityEngine.GameObject.Find("DebriefScreen") != null && ScreenManager.Instance != null)
                    ScreenManager.Instance.Show("debrief");
            });
        }

        /// The engineering layer already determines the outcome deterministically;
        /// the 3D simulation is the visual presentation of that verdict (GDD 15.5).
        public static void ShowLaunchOutcome(ValidationReport report)
        {
            if (report == null || !report.CanLaunch)
            {
                ShowModal(report);
                return;
            }

            if (modal == null) BuildModal();
            modal.SetActive(true);
            for (var i = content.childCount - 1; i >= 0; i--)
                DestroyImmediate(content.GetChild(i).gameObject);

            var title = modal.GetComponentInChildren<TextMeshProUGUI>(true);

            // Predicted outcome from the engineering layer
            var failedPhase = report.engineering.deltaV.Find(d => !d.sufficient);
            if (failedPhase != null)
            {
                var phaseText = failedPhase.phase == "departure" ? "during ascent/departure"
                    : failedPhase.phase == "arrival" ? "at destination arrival"
                    : "on the return journey";
                title.text = "LAUNCH — MISSION FAILURE PREDICTED";
                title.color = UITheme.Danger;
                AddLine($"Engineering verdict: the vehicle runs out of capability {phaseText} " +
                        $"(needs {failedPhase.requiredMS:0} m/s, can produce {failedPhase.achievedMS:0} m/s). " +
                        "In the simulation this will end in mission failure.",
                    UITheme.Danger);
            }
            else
            {
                title.text = "LAUNCH — MISSION SUCCESS PREDICTED";
                title.color = UITheme.Success;
                AddLine("Engineering verdict: the configuration can complete the mission profile. " +
                        "The simulation will visualize the flight and confirm the outcome.",
                    UITheme.Success);
            }

            foreach (var issue in report.issues)
                AddLine($"{issue.message}\n→ {issue.hint}", UITheme.TextMuted);

            AddLine("The 3D flight simulation is under construction — this screen shows the deterministic outcome the simulation will visualize.",
                UITheme.TextMuted);

            var closeBtn = UITheme.TextButton(content, "CLOSE", UITheme.FontBody);
            var rect = closeBtn.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 52);
            closeBtn.onClick.AddListener(() => modal.SetActive(false));
        }
    }
}
