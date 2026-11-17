using UnityEngine;
using UnityEngine.UI;
using SpaceMission.Core;

namespace SpaceMission.UI
{
    public class UIMainMenu : MonoBehaviour
    {
        public static UIMainMenu Build(Transform parent)
        {
            var screen = new GameObject("MainMenuScreen", typeof(RectTransform), typeof(UIMainMenu));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var menu = screen.GetComponent<UIMainMenu>();

            UITheme.Panel(rect, "Dim", new Color(0f, 0f, 0f, 0.25f), raycast: false);

            // Title block
            var titleRect = new GameObject("Title", typeof(RectTransform)).GetComponent<RectTransform>();
            titleRect.SetParent(rect, false);
            titleRect.anchorMin = new Vector2(0.5f, 1f);
            titleRect.anchorMax = new Vector2(0.5f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(900, 220);
            titleRect.anchoredPosition = new Vector2(0, -140);

            var title = UITheme.Label(titleRect, "MY MISSION", UITheme.FontH1, UITheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.transform.SetParent(titleRect, false);

            var subtitleGo = new GameObject("Subtitle", typeof(RectTransform));
            var subtitleRect = subtitleGo.GetComponent<RectTransform>();
            subtitleRect.SetParent(titleRect, false);
            subtitleRect.anchorMin = Vector2.zero;
            subtitleRect.anchorMax = Vector2.one;
            subtitleRect.offsetMin = new Vector2(0, 0);
            subtitleRect.offsetMax = new Vector2(0, -70);
            UITheme.Label(subtitleRect, "Space Mission Design Simulator", UITheme.FontH3, UITheme.TextMuted, TextAnchor.MiddleCenter);

            // Buttons column
            var buttons = new GameObject("Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup)).GetComponent<RectTransform>();
            buttons.SetParent(rect, false);
            buttons.anchorMin = new Vector2(0.5f, 0.5f);
            buttons.anchorMax = new Vector2(0.5f, 0.5f);
            buttons.pivot = new Vector2(0.5f, 0.5f);
            buttons.sizeDelta = new Vector2(360, 260);
            buttons.anchoredPosition = new Vector2(0, -100);

            var vlg = buttons.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 18;
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;

            var startBtn = UITheme.TextButton(buttons, "START", UITheme.FontH2, UITheme.BtnAccent, UITheme.TextPrimary, UITheme.Accent);
            var startLayout = startBtn.gameObject.AddComponent<LayoutElement>();
            startLayout.preferredHeight = 70;
            startBtn.onClick.AddListener(() =>
            {
                ScreenManager.Instance.ClearHistory();
                ScreenManager.Instance.Show("missionSelect");
            });

            var infoBtn = UITheme.TextButton(buttons, "INFO", UITheme.FontBody);
            var infoLayout = infoBtn.gameObject.AddComponent<LayoutElement>();
            infoLayout.preferredHeight = 60;
            infoBtn.onClick.AddListener(() => ScreenManager.Instance.Show("info"));

            var settingsBtn = UITheme.TextButton(buttons, "SETTINGS", UITheme.FontBody);
            var settingsLayout = settingsBtn.gameObject.AddComponent<LayoutElement>();
            settingsLayout.preferredHeight = 60;
            settingsBtn.onClick.AddListener(() => ScreenManager.Instance.Show("settings"));

            // Footer
            var footer = UITheme.Label(rect, "NASA Space Apps Challenge 2026 — Educational Simulation",
                UITheme.FontSmall, UITheme.TextMuted, TextAnchor.MiddleCenter);
            var fRect = footer.GetComponent<RectTransform>();
            fRect.anchorMin = new Vector2(0.5f, 0f);
            fRect.anchorMax = new Vector2(0.5f, 0f);
            fRect.pivot = new Vector2(0.5f, 0f);
            fRect.sizeDelta = new Vector2(900, 30);
            fRect.anchoredPosition = new Vector2(0, 24);

            return menu;
        }
    }
}
