using UnityEngine;
using UnityEngine.UI;
using SpaceMission.Core;

namespace SpaceMission.UI
{
    public class UIMissionSelect : MonoBehaviour
    {
        Transform listContent;

        public static UIMissionSelect Build(Transform parent)
        {
            var screen = new GameObject("MissionSelectScreen", typeof(RectTransform), typeof(UIMissionSelect));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            // solid readable backing (space background shows around the edges)
            UITheme.Panel(rect, "SolidBackdrop", new Color(0.024f, 0.031f, 0.055f, 0.96f), raycast: true);

            var page = screen.GetComponent<UIMissionSelect>();

            // Header
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
            backRect.offsetMin = new Vector2(24, 12);
            backRect.offsetMax = new Vector2(164, -12);
            backBtn.onClick.AddListener(() => ScreenManager.Instance.Back());

            var title = UITheme.Label(header, "SELECT MISSION", UITheme.FontH2, UITheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            // Scroll list
            var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(RectTransform))
                .GetComponent<RectTransform>();
            scrollGo.SetParent(rect, false);
            scrollGo.anchorMin = new Vector2(0, 0);
            scrollGo.anchorMax = new Vector2(1, 1);
            scrollGo.offsetMin = new Vector2(240, 40);
            scrollGo.offsetMax = new Vector2(-240, -110);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(RectMask2D));
            var viewport = viewportGo.GetComponent<RectTransform>();
            viewport.SetParent(scrollGo, false);
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
            content.offsetMin = new Vector2(0, 0);
            content.offsetMax = new Vector2(0, 0);

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 16;
            vlg.padding = new RectOffset(0, 0, 8, 24);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var sr = scrollGo.GetComponent<ScrollRect>();
            sr.content = content;
            sr.viewport = viewport;
            sr.horizontal = false;
            sr.vertical = true;
            sr.scrollSensitivity = 30;

            page.listContent = content;
            return page;
        }

        void OnEnable()
        {
            if (listContent == null) return;
            Rebuild();
        }

        void Rebuild()
        {
            for (var i = listContent.childCount - 1; i >= 0; i--)
                DestroyImmediate(listContent.GetChild(i).gameObject);

            if (GameState.DB == null || GameState.DB.missions == null) return;

            foreach (var mission in GameState.DB.missions)
            {
                var unlocked = GameState.Progress.IsUnlocked(mission.id);
                var row = UITheme.BorderedPanel(listContent, mission.id, UITheme.BgPanel,
                    unlocked ? UITheme.Border : new Color(0.13f, 0.15f, 0.19f), 1, raycast: true);
                var layout = row.gameObject.AddComponent<LayoutElement>();
                layout.preferredHeight = 130;

                var btn = row.gameObject.AddComponent<Button>();
                btn.interactable = unlocked;
                btn.transition = Selectable.Transition.None;

                var info = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
                var infoRect = info.GetComponent<RectTransform>();
                infoRect.SetParent(row, false);
                infoRect.anchorMin = new Vector2(0, 0);
                infoRect.anchorMax = new Vector2(1, 1);
                infoRect.offsetMin = new Vector2(28, 14);
                infoRect.offsetMax = new Vector2(-220, -14);
                var ivlg = info.GetComponent<VerticalLayoutGroup>();
                ivlg.spacing = 4;
                ivlg.childControlWidth = true;
                ivlg.childControlHeight = true;
                ivlg.childForceExpandHeight = false;

                var nameLabel = UITheme.Label(infoRect, unlocked ? mission.name : $"{mission.name}  [LOCKED]",
                    UITheme.FontH3, unlocked ? UITheme.TextPrimary : UITheme.TextMuted);
                nameLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 34);

                var typeText = mission.isTutorial
                    ? "TUTORIAL · " + mission.missionType.ToUpper()
                    : mission.missionType.ToUpper();
                var typeLabel = UITheme.Label(infoRect, typeText, UITheme.FontSmall, UITheme.TextMuted);
                typeLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 22);

                var descLabel = UITheme.Label(infoRect, mission.successCondition, UITheme.FontSmall,
                    unlocked ? UITheme.TextMuted : new Color(0.45f, 0.47f, 0.52f));
                descLabel.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 22);

                // Right side: budget + best score
                var rightLabel = UITheme.Label(row,
                    $"Budget  {Formats.Money(mission.budgetUSD)}\nBest  {GameState.Progress.GetBestScore(mission.id)}",
                    UITheme.FontBody, UITheme.TextMuted, TextAnchor.MiddleRight);
                var rRect = rightLabel.GetComponent<RectTransform>();
                rRect.anchorMin = new Vector2(1, 0);
                rRect.anchorMax = new Vector2(1, 1);
                rRect.pivot = new Vector2(1, 0.5f);
                rRect.offsetMin = new Vector2(-210, 14);
                rRect.offsetMax = new Vector2(-28, -14);

                if (unlocked)
                    btn.onClick.AddListener(() => SelectMission(mission));
            }
        }

        void SelectMission(Data.MissionData mission)
        {
            GameManager.Instance.StartMission(mission);
            ScreenManager.Instance.ClearHistory();
            ScreenManager.Instance.Show("briefing");
        }
    }
}
