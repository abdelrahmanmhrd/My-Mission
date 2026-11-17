using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceMission.UI
{
    /// Credits: game authorship + third-party license attributions (ASSET_TRACKING.md).
    public class UICredits : MonoBehaviour
    {
        /// Single source of truth for in-game credits. Rows are added here whenever
        /// an attributed asset enters the project (see ASSET_TRACKING.md process).
        public static readonly string[] Entries =
        {
            "GAME & CODE",
            "My Mission — written by the team for the NASA Space Apps Challenge 2026.",
            "All game code, JSON datasets and UI were authored for this project.",
            "",
            "ENGINE & TOOLS",
            "Unity 6000 · Universal Render Pipeline · TextMeshPro (LiberationSans SDF) — Unity Technologies, Unity license.",
            "",
            "DATA SOURCES",
            "Engineering values derived from public NASA fact sheets and mission documentation",
            "(NASA NSSDCA planetary fact sheets, Orion and Mars 2020 press kits, SLS and",
            "Saturn V references, Perseverance instrument pages). Simplified for gameplay.",
            "",
            "3D MODELS, ART & AUDIO",
            "Team-produced models and assets. No third-party attributed assets used yet.",
            "",
            "BRANDING NOTE",
            "This is an educational fan-made simulation for the NASA Space Apps Challenge.",
            "It is not produced by, endorsed by, or affiliated with NASA. The NASA insignia",
            "is not used; agency-style styling is fictional.",
        };

        public static UICredits Build(Transform parent)
        {
            var screen = new GameObject("CreditsScreen", typeof(RectTransform), typeof(UICredits));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            // solid readable backing (space background shows around the edges)
            UITheme.Panel(rect, "SolidBackdrop", new Color(0.024f, 0.031f, 0.055f, 0.96f), raycast: true);

            var header = UITheme.Panel(rect, "Header", UITheme.BgPanel);
            header.anchorMin = new Vector2(0, 1);
            header.anchorMax = new Vector2(1, 1);
            header.pivot = new Vector2(0.5f, 1f);
            header.sizeDelta = new Vector2(0, 80);
            header.anchoredPosition = Vector2.zero;

            var backBtn = UITheme.TextButton(header, "< BACK", UITheme.FontBody);
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0);
            backRect.anchorMax = new Vector2(0, 1);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.sizeDelta = new Vector2(140, 0);
            backRect.offsetMin = new Vector2(24, 14);
            backRect.offsetMax = new Vector2(164, -14);
            backBtn.onClick.AddListener(() => ScreenManager.Instance.Back());

            UITheme.Label(header, "CREDITS & LICENSES", UITheme.FontH2, UITheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            var scroll = scrollGo.GetComponent<RectTransform>();
            scroll.SetParent(rect, false);
            scroll.anchorMin = new Vector2(0, 0);
            scroll.anchorMax = new Vector2(1, 1);
            scroll.offsetMin = new Vector2(320, 24);
            scroll.offsetMax = new Vector2(-320, -100);

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
            vlg.padding = new RectOffset(24, 24, 16, 32);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scroll.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.scrollSensitivity = 30;

            var headingColor = UITheme.Accent;
            foreach (var line in Entries)
            {
                if (line.Length == 0)
                {
                    AddSpacer(content, 14);
                    continue;
                }
                var isHeading = line == line.ToUpperInvariant() && line.Length > 3;
                var label = UITheme.Label(content, line,
                    isHeading ? UITheme.FontH3 : UITheme.FontBody,
                    isHeading ? headingColor : UITheme.TextPrimary, TextAnchor.UpperLeft,
                    isHeading ? FontStyle.Bold : FontStyle.Normal);
                label.gameObject.AddComponent<AutoHeight>().padding = 6f;
            }

            return screen.GetComponent<UICredits>();
        }

        static void AddSpacer(RectTransform content, float height)
        {
            var go = new GameObject("Spacer", typeof(RectTransform), typeof(LayoutElement));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(content, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            go.GetComponent<LayoutElement>().preferredHeight = height;
        }
    }
}
