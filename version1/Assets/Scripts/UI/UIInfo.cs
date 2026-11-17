using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;

namespace SpaceMission.UI
{
    /// Info / encyclopedia: plain-language engineering concept explanations (GDD 4.4).
    public class UIInfo : MonoBehaviour
    {
        public static UIInfo Build(Transform parent)
        {
            var screen = new GameObject("InfoScreen", typeof(RectTransform), typeof(UIInfo));
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

            UITheme.Label(header, "INFO — ENGINEERING CONCEPTS", UITheme.FontH2, UITheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Scrollable encyclopedia
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
            var scroll = scrollGo.GetComponent<RectTransform>();
            scroll.SetParent(rect, false);
            scroll.anchorMin = new Vector2(0, 0);
            scroll.anchorMax = new Vector2(1, 1);
            scroll.offsetMin = new Vector2(280, 24);
            scroll.offsetMax = new Vector2(-280, -100);

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
            vlg.spacing = 20;
            vlg.padding = new RectOffset(24, 24, 16, 32);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scroll.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.scrollSensitivity = 30;

            AddEntry(content, "THRUST", "Thrust is the push a rocket engine produces. At liftoff, total engine thrust must be greater than the vehicle's weight or the rocket cannot climb. The game expresses this as the thrust-to-weight ratio, which must be at least 1.05.");
            AddEntry(content, "FUEL TYPES & CONSUMPTION", "Each fuel burns with different efficiency. Kerosene is dense and cheap; hydrogen is the most efficient but needs very large, heavy tanks; hypergolic fuels ignite on contact and are storable but less efficient. The amount of fuel you load decides how far you can go.");
            AddEntry(content, "ENGINE EFFICIENCY (ISP)", "Isp measures how many seconds an engine can produce one unit of thrust from one unit of propellant. Higher Isp means each tonne of fuel buys more velocity (delta-v). Vacuum Isp is always higher than sea-level Isp because there is no air to push against.");
            AddEntry(content, "VEHICLE & PAYLOAD MASS", "Everything you carry costs fuel to move: the spacecraft, its instruments and the fuel itself. Each launch vehicle has a maximum payload it can place on course — exceeding it fails validation or the mission.");
            AddEntry(content, "DELTA-V", "Delta-v is the total velocity change a vehicle can produce: its reach. Missions list required delta-v per phase — departure, arrival and (for crew) return. If your vehicle or spacecraft cannot produce the required delta-v, the mission cannot complete.");
            AddEntry(content, "BUDGET & COMPONENT COSTS", "Every component costs money: vehicles, pads, spacecraft, fuel and instruments. Each mission gives you a fixed budget. Staying under budget is required to launch, and leftover budget improves your resource-efficiency score.");
            AddEntry(content, "SPACECRAFT TYPES", "Automated spacecraft carry robotic payloads and need no life support. Crewed spacecraft carry astronauts and need oxygen generation and crew capacity. Crewed missions to the lunar surface also need a lander — that is why Artemis III flies Orion docked with a human landing system.");
            AddEntry(content, "HUMAN-LED VS AUTOMATED", "Human-led missions must carry the oxygen generator: if it is missing, the crew cannot breathe and the mission ends in disaster. Automated missions only need their scientific instruments.");
            AddEntry(content, "LAUNCH FACILITIES", "Launch pads are built for specific vehicles: a Saturn V cannot lift off from an Atlas pad, and astronauts need a pad with crew-access infrastructure. Facility compatibility is a hard rule.");
            AddEntry(content, "TRAJECTORY & DESTINATION", "The flight path is determined by the destination: lunar missions need trans-lunar injection, Mars missions need trans-Mars injection. The simulation shows the planned route as a dotted line and the flown route as a solid line.");

            return screen.GetComponent<UIInfo>();
        }

        static void AddEntry(RectTransform content, string title, string body)
        {
            var entry = UITheme.BorderedPanel(content, title, UITheme.BgPanel, UITheme.Border, 1);
            var layout = entry.gameObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 150;

            var titleLabel = UITheme.Label(entry, title, UITheme.FontH3, UITheme.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            var tRect = titleLabel.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0, 1);
            tRect.anchorMax = new Vector2(1, 1);
            tRect.pivot = new Vector2(0, 1);
            tRect.offsetMin = new Vector2(24, -44);
            tRect.offsetMax = new Vector2(-24, -10);

            var bodyLabel = UITheme.Label(entry, body, UITheme.FontBody, UITheme.TextPrimary, TextAnchor.UpperLeft);
            var bRect = bodyLabel.GetComponent<RectTransform>();
            bRect.anchorMin = new Vector2(0, 0);
            bRect.anchorMax = new Vector2(1, 1);
            bRect.offsetMin = new Vector2(24, 12);
            bRect.offsetMax = new Vector2(-24, -54);
        }
    }
}
