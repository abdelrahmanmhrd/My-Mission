using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;

namespace SpaceMission.UI
{
    /// Mission briefing: classified-style document, header/body/params/acknowledge (GDD 5).
    public class UIBriefing : MonoBehaviour
    {
        static Data.MissionData mission;
        static TextMeshProUGUI typeBadgeText;

        public static TextMeshProUGUI TypeBadgeText => typeBadgeText;

        public static UIBriefing Build(Transform parent)
        {
            var screen = new GameObject("BriefingScreen", typeof(RectTransform), typeof(UIBriefing));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            // solid readable backing (space background shows around the edges)
            UITheme.Panel(rect, "SolidBackdrop", new Color(0.024f, 0.031f, 0.055f, 0.96f), raycast: true);


            var panel = UITheme.BorderedPanel(rect, "Document", UITheme.BgPanel, UITheme.AccentDim, 2);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(880, 720);

            // Classification strip (top)
            var strip = UITheme.Panel(panel, "ClassStrip", new Color(0.55f, 0.12f, 0.12f));
            strip.anchorMin = new Vector2(0, 1);
            strip.anchorMax = new Vector2(1, 1);
            strip.pivot = new Vector2(0.5f, 1f);
            strip.sizeDelta = new Vector2(0, 26);
            strip.anchoredPosition = Vector2.zero;
            UITheme.Label(strip, "CONFIDENTIAL — AUTHORIZED PERSONNEL ONLY", UITheme.FontTiny,
                UITheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Header band
            var header = UITheme.Panel(panel, "Header", UITheme.BgPanelAlt);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0, 86);
            header.anchoredPosition = new Vector2(0, -26);

            UITheme.Label(header, "MISSION DIRECTORATE — OFFICIAL DIRECTIVE",
                UITheme.FontTiny, UITheme.TextMuted, TextAnchor.MiddleLeft);

            var title = UITheme.Label(header, "", UITheme.FontH2, UITheme.TextPrimary, TextAnchor.LowerLeft, FontStyle.Bold);
            var tRect = title.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0, 0);
            tRect.anchorMax = new Vector2(0.72f, 1);
            tRect.offsetMin = new Vector2(24, 8);
            tRect.offsetMax = new Vector2(0, -8);

            var typeBadge = UITheme.BorderedPanel(header, "TypeBadge", new Color(0.13f, 0.22f, 0.34f), UITheme.Accent, 1);
            typeBadge.anchorMin = new Vector2(1, 0.5f);
            typeBadge.anchorMax = new Vector2(1, 0.5f);
            typeBadge.pivot = new Vector2(1, 0.5f);
            typeBadge.sizeDelta = new Vector2(190, 44);
            typeBadge.anchoredPosition = new Vector2(-24, 0);
            typeBadgeText = UITheme.Label(typeBadge, "", UITheme.FontSmall, UITheme.Accent, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Body
            var bodyGo = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            var body = bodyGo.GetComponent<RectTransform>();
            body.SetParent(panel, false);
            body.anchorMin = new Vector2(0, 1);
            body.anchorMax = new Vector2(1, 1);
            body.pivot = new Vector2(0.5f, 1f);
            body.sizeDelta = new Vector2(0, 0);
            body.offsetMin = new Vector2(28, -110);
            body.offsetMax = new Vector2(-28, -110);

            var vlg = body.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 18;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;

            var csf = bodyGo.GetComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var directive = AddText(body, "MISSION DIRECTIVE", UITheme.FontH3, UITheme.Accent, FontStyle.Bold);
            var directiveText = AddText(body, "", UITheme.FontBody, UITheme.TextPrimary);
            var paramsTitle = AddText(body, "MISSION PARAMETERS", UITheme.FontH3, UITheme.Accent, FontStyle.Bold);
            var paramsText = AddText(body, "", UITheme.FontBody, UITheme.TextMuted);
            var closing = AddText(body, "Validate all systems before launch. Mission success depends on your engineering decisions.",
                UITheme.FontBody, UITheme.TextPrimary, FontStyle.Italic);
            closing.GetComponent<LayoutElement>().minHeight = 30;

            // Acknowledge
            var ack = UITheme.TextButton(panel, "ACKNOWLEDGE — PROCEED TO CONFIGURATION", UITheme.FontBody,
                UITheme.BtnAccent, UITheme.TextPrimary, UITheme.Accent);
            var ackRect = ack.GetComponent<RectTransform>();
            ackRect.anchorMin = new Vector2(0.5f, 0);
            ackRect.anchorMax = new Vector2(0.5f, 0);
            ackRect.pivot = new Vector2(0.5f, 0);
            ackRect.sizeDelta = new Vector2(560, 56);
            ackRect.anchoredPosition = new Vector2(0, 28);
            ack.onClick.AddListener(() =>
            {
                ScreenManager.Instance.ClearHistory();
                ScreenManager.Instance.Show("configHub");
            });

            // Fill content when the screen is shown
            var filler = screen.AddComponent<BriefingFiller>();
            filler.Init(title, directiveText, paramsText);
            return screen.GetComponent<UIBriefing>();
        }

        static TextMeshProUGUI AddText(RectTransform parent, string text, int size, Color color, FontStyle style = FontStyle.Normal)
        {
            var label = UITheme.Label(parent, text, size, color, TextAnchor.UpperLeft, style);
            var rect = label.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            label.gameObject.AddComponent<AutoHeight>();
            return label;
        }
    }

    /// Populates briefing text whenever the screen becomes visible.
    public class BriefingFiller : MonoBehaviour
    {
        TextMeshProUGUI title;
        TextMeshProUGUI directiveText;
        TextMeshProUGUI paramsText;

        public void Init(TextMeshProUGUI title, TextMeshProUGUI directiveText, TextMeshProUGUI paramsText)
        {
            this.title = title;
            this.directiveText = directiveText;
            this.paramsText = paramsText;
        }

        void OnEnable()
        {
            var m = GameState.CurrentMission;
            if (m == null) return;
            title.text = m.briefTitle;
            directiveText.text = m.briefBody;

            var planet = GameState.DB.GetPlanet(m.destinationPlanetId);
            var typeText = m.missionType == "crewed" ? $"CREWED · {m.crew} ASTRONAUTS" : "AUTOMATED";
            if (UIBriefing.TypeBadgeText != null)
                UIBriefing.TypeBadgeText.text = typeText;

            // aligned parameter rows (fixed-width keys → tidy column)
            paramsText.text =
                $"DESTINATION        {planet?.name ?? m.destinationPlanetId}\n" +
                $"SPACECRAFT         {GameState.DB.GetSpacecraft(m.requiredSpacecraftId)?.name ?? m.requiredSpacecraftId}\n" +
                $"BUDGET             {Formats.Money(m.budgetUSD)}\n" +
                $"DISTANCE           {Formats.Distance(m.distanceKm)}\n" +
                $"SCIENCE MINIMUM    {m.minScienceValue:0} points\n" +
                $"PROFILE            {(m.capacityMetric == "TMI" ? "Trans-Mars injection" : "Trans-Lunar injection")}";
        }
    }
}
