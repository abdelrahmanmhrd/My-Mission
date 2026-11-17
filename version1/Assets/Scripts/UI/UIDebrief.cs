using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;
using SpaceMission.Engineering;

namespace SpaceMission.UI
{
    /// Post-mission debrief (GDD 12): verdict banner with rank medal, animated
    /// score count-up, integer-consistent breakdown bars, engineering lessons,
    /// retry path. All panels AutoHeight — nothing overlaps or clips.
    public class UIDebrief : MonoBehaviour
    {
        TextMeshProUGUI bannerTitle;
        TextMeshProUGUI bannerRank;
        TextMeshProUGUI totalScoreLabel;
        TextMeshProUGUI[] rowValues = new TextMeshProUGUI[4];
        RectTransform[] rowFills = new RectTransform[4];
        float[] rowTargets = new float[4];
        int totalTarget;
        float countElapsed;
        bool counting;

        public static UIDebrief Build(Transform parent)
        {
            var screen = new GameObject("DebriefScreen", typeof(RectTransform), typeof(UIDebrief));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return screen.GetComponent<UIDebrief>();
        }

        void OnEnable() => Rebuild();

        void Rebuild()
        {
            for (var i = transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(transform.GetChild(i).gameObject);

            var mission = GameState.CurrentMission;
            var outcome = GameState.LastOutcome;
            var score = GameState.LastScore;
            var engineering = GameState.LastEngineering;
            if (mission == null || outcome == null || score == null) return;

            var success = outcome.success;

            // ================= BANNER =================
            var banner = UITheme.BorderedPanel(transform, "Banner", UITheme.BgPanel,
                success ? UITheme.Success : UITheme.Danger, 2);
            banner.anchorMin = new Vector2(0.5f, 1f);
            banner.anchorMax = new Vector2(0.5f, 1f);
            banner.pivot = new Vector2(0.5f, 1f);
            banner.sizeDelta = new Vector2(1100, 130);
            banner.anchoredPosition = new Vector2(0, -24);

            bannerTitle = UITheme.Label(banner,
                success ? "MISSION SUCCESS" : "MISSION FAILED",
                UITheme.FontH1, success ? UITheme.Success : UITheme.Danger, TextAnchor.MiddleLeft, FontStyle.Bold);
            var btRect = bannerTitle.GetComponent<RectTransform>();
            btRect.anchorMin = new Vector2(0, 0);
            btRect.anchorMax = new Vector2(0.55f, 1);
            btRect.offsetMin = new Vector2(28, 12);
            btRect.offsetMax = new Vector2(0, -12);

            // right side: rank + total (count-up)
            bannerRank = UITheme.Label(banner, score.rank, UITheme.FontH3, UITheme.Warning, TextAnchor.UpperRight, FontStyle.Bold);
            var rRank = bannerRank.GetComponent<RectTransform>();
            rRank.anchorMin = new Vector2(0.55f, 0.5f);
            rRank.anchorMax = new Vector2(1, 1);
            rRank.offsetMin = new Vector2(8, 10);
            rRank.offsetMax = new Vector2(-28, -12);

            totalScoreLabel = UITheme.Label(banner, "0 / 100", UITheme.FontH1, UITheme.TextPrimary, TextAnchor.LowerRight, FontStyle.Bold);
            var tRect = totalScoreLabel.GetComponent<RectTransform>();
            tRect.anchorMin = new Vector2(0.55f, 0);
            tRect.anchorMax = new Vector2(1, 0.5f);
            tRect.offsetMin = new Vector2(8, 12);
            tRect.offsetMax = new Vector2(-28, -10);

            // ================= SCORE PANEL (breakdown) =================
            var scorePanel = UITheme.BorderedPanel(transform, "Score", UITheme.BgPanel, UITheme.Border, 1);
            scorePanel.anchorMin = new Vector2(0.5f, 1f);
            scorePanel.anchorMax = new Vector2(0.5f, 1f);
            scorePanel.pivot = new Vector2(0.5f, 1f);
            scorePanel.sizeDelta = new Vector2(1100, 270);
            scorePanel.anchoredPosition = new Vector2(0, -170);

            AddScoreRow(scorePanel, 0, "Mission success", score.SuccessPoints, 35);
            AddScoreRow(scorePanel, 1, "Scientific value", score.SciencePoints, 25);
            AddScoreRow(scorePanel, 2, "Resource efficiency", score.EfficiencyPoints, 20);
            AddScoreRow(scorePanel, 3, "Engineering reliability", score.ReliabilityPoints, 20);

            // ================= FAILURE REASON =================
            var top = -456f;
            if (!success)
            {
                var reasonPanel = UITheme.BorderedPanel(transform, "Failure", UITheme.CardSelected, UITheme.Danger, 1);
                reasonPanel.anchorMin = new Vector2(0.5f, 1f);
                reasonPanel.anchorMax = new Vector2(0.5f, 1f);
                reasonPanel.pivot = new Vector2(0.5f, 1f);
                reasonPanel.sizeDelta = new Vector2(1100, 110);
                reasonPanel.anchoredPosition = new Vector2(0, top);

                var rTitle = UITheme.Label(reasonPanel, "WHAT WENT WRONG", UITheme.FontSmall, UITheme.Danger, TextAnchor.UpperLeft, FontStyle.Bold);
                var rtRect = rTitle.GetComponent<RectTransform>();
                rtRect.anchorMin = new Vector2(0, 1);
                rtRect.anchorMax = new Vector2(1, 1);
                rtRect.pivot = new Vector2(0, 1);
                rtRect.offsetMin = new Vector2(28, -30);
                rtRect.offsetMax = new Vector2(-28, -8);

                var reasonText = UITheme.Label(reasonPanel, outcome.failureReason, UITheme.FontBody, UITheme.TextPrimary, TextAnchor.UpperLeft);
                var rRect = reasonText.GetComponent<RectTransform>();
                rRect.anchorMin = new Vector2(0, 0);
                rRect.anchorMax = new Vector2(1, 1);
                rRect.offsetMin = new Vector2(28, 36);
                rRect.offsetMax = new Vector2(-28, -8);
                reasonText.gameObject.AddComponent<AutoHeight>().padding = 16f;
                top -= 126f;
            }

            // ================= LESSONS =================
            var lessonPanel = UITheme.BorderedPanel(transform, "Lessons", UITheme.BgPanel, UITheme.Border, 1);
            lessonPanel.anchorMin = new Vector2(0.5f, 1f);
            lessonPanel.anchorMax = new Vector2(0.5f, 1f);
            lessonPanel.pivot = new Vector2(0.5f, 1f);
            lessonPanel.sizeDelta = new Vector2(1100, 96);
            lessonPanel.anchoredPosition = new Vector2(0, top);

            var lessonText = UITheme.Label(lessonPanel,
                "FOR THE NEXT ATTEMPT:  " + (outcome.lessons.Count > 0
                    ? string.Join("  ·  ", outcome.lessons.ToArray())
                    : success
                        ? "Solid engineering. Try a cheaper configuration for a better efficiency score, or more instruments for a higher science score."
                        : "Review the mission requirements and the validation hints before launching again."),
                UITheme.FontSmall, UITheme.TextMuted, TextAnchor.UpperLeft);
            var lRect = lessonText.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0, 0);
            lRect.anchorMax = new Vector2(1, 1);
            lRect.offsetMin = new Vector2(28, 14);
            lRect.offsetMax = new Vector2(-28, -14);
            lessonText.gameObject.AddComponent<AutoHeight>().padding = 12f;

            // ================= EXPENDITURE =================
            var spendPanel = UITheme.Panel(transform, "Spend", UITheme.BgPanel);
            spendPanel.anchorMin = new Vector2(0.5f, 1f);
            spendPanel.anchorMax = new Vector2(0.5f, 1f);
            spendPanel.pivot = new Vector2(0.5f, 1f);
            spendPanel.sizeDelta = new Vector2(1100, 100);
            spendPanel.anchoredPosition = new Vector2(0, top - 106);
            var spendLabel = UITheme.Label(spendPanel, ScoringModel.BudgetSummary(mission, engineering),
                UITheme.FontSmall, UITheme.TextMuted, TextAnchor.UpperLeft);
            spendLabel.GetComponent<RectTransform>().offsetMin = new Vector2(28, 12);
            spendLabel.GetComponent<RectTransform>().offsetMax = new Vector2(-28, -12);
            spendLabel.gameObject.AddComponent<AutoHeight>().padding = 12f;

            // ================= BUTTONS =================
            var retry = UITheme.TextButton(transform, "RETRY MISSION", UITheme.FontBody,
                UITheme.BtnAccent, UITheme.TextPrimary, UITheme.Accent);
            var rBtnRect = retry.GetComponent<RectTransform>();
            rBtnRect.anchorMin = new Vector2(0.5f, 0);
            rBtnRect.anchorMax = new Vector2(0.5f, 0);
            rBtnRect.pivot = new Vector2(1, 0);
            rBtnRect.sizeDelta = new Vector2(300, 56);
            rBtnRect.anchoredPosition = new Vector2(-14, 20);
            retry.onClick.AddListener(() =>
            {
                ScreenManager.Instance.ClearHistory();
                ScreenManager.Instance.Show("configHub");
            });

            var select = UITheme.TextButton(transform, "MISSION SELECT", UITheme.FontBody);
            var sBtnRect = select.GetComponent<RectTransform>();
            sBtnRect.anchorMin = new Vector2(0.5f, 0);
            sBtnRect.anchorMax = new Vector2(0.5f, 0);
            sBtnRect.pivot = new Vector2(0, 0);
            sBtnRect.sizeDelta = new Vector2(280, 56);
            sBtnRect.anchoredPosition = new Vector2(14, 20);
            select.onClick.AddListener(() =>
            {
                ScreenManager.Instance.ClearHistory();
                ScreenManager.Instance.Show("missionSelect");
            });

            // start the animated count-up
            totalTarget = score.TotalPoints;
            rowTargets[0] = score.SuccessPoints; rowTargets[1] = score.SciencePoints;
            rowTargets[2] = score.EfficiencyPoints; rowTargets[3] = score.ReliabilityPoints;
            countElapsed = 0f;
            counting = true;
        }

        void AddScoreRow(RectTransform parent, int index, string name, int value, int max)
        {
            var y = -34f - index * 60f;

            var nameLabel = UITheme.Label(parent, name, UITheme.FontBody, UITheme.TextPrimary, TextAnchor.MiddleLeft);
            var nRect = nameLabel.GetComponent<RectTransform>();
            nRect.anchorMin = new Vector2(0, 1);
            nRect.anchorMax = new Vector2(0.32f, 1);
            nRect.offsetMin = new Vector2(28, y - 20);
            nRect.offsetMax = new Vector2(0, y + 10);

            var barBg = UITheme.Panel(parent, "BarBg", UITheme.BgPanelAlt);
            barBg.anchorMin = new Vector2(0.34f, 1);
            barBg.anchorMax = new Vector2(0.86f, 1);
            barBg.offsetMin = new Vector2(0, y - 18);
            barBg.offsetMax = new Vector2(0, y + 8);

            var fill = UITheme.Panel(barBg, "Fill", index == 0 ? UITheme.Success : UITheme.Accent);
            fill.anchorMin = Vector2.zero;
            fill.offsetMin = Vector2.zero;
            fill.offsetMax = Vector2.zero;
            fill.sizeDelta = Vector2.zero;

            rowValues[index] = UITheme.Label(parent, $"0 / {max}", UITheme.FontBody, UITheme.TextMuted, TextAnchor.MiddleRight);
            var vRect = rowValues[index].GetComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0.88f, 1);
            vRect.anchorMax = new Vector2(1, 1);
            vRect.offsetMin = new Vector2(0, y - 20);
            vRect.offsetMax = new Vector2(-28, y + 10);

            rowFills[index] = fill;
        }

        void Update()
        {
            if (!counting) return;
            countElapsed += Time.deltaTime;
            var t = Mathf.Clamp01(countElapsed / 1.6f);          // 1.6 s count-up
            var eased = 1f - Mathf.Pow(1f - t, 3f);              // ease-out

            var shownTotal = Mathf.RoundToInt(totalTarget * eased);
            totalScoreLabel.text = $"{shownTotal} / 100";
            var maxes = new[] { 35, 25, 20, 20 };
            for (var i = 0; i < 4; i++)
            {
                var shown = Mathf.RoundToInt(rowTargets[i] * eased);
                rowValues[i].text = $"{shown} / {maxes[i]}";
                rowFills[i].anchorMax = new Vector2(maxes[i] > 0 ? Mathf.Clamp01(rowTargets[i] * eased / maxes[i]) : 0f, 1f);
            }
            if (t >= 1f) counting = false;
        }
    }
}
