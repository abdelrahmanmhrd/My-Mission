using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SpaceMission.Core;

namespace SpaceMission.UI
{
    public class UISettings : MonoBehaviour
    {


        public static UISettings Build(Transform parent)
        {
            var screen = new GameObject("SettingsScreen", typeof(RectTransform), typeof(UISettings));
            var rect = screen.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            // solid readable backing (space background shows around the edges)
            UITheme.Panel(rect, "SolidBackdrop", new Color(0.024f, 0.031f, 0.055f, 0.96f), raycast: true);

            var page = screen.GetComponent<UISettings>();
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

            var backBtn = UITheme.TextButton(header, "< BACK", UITheme.FontBody);
            var backRect = backBtn.GetComponent<RectTransform>();
            backRect.anchorMin = new Vector2(0, 0);
            backRect.anchorMax = new Vector2(0, 1);
            backRect.pivot = new Vector2(0, 0.5f);
            backRect.sizeDelta = new Vector2(140, 0);
            backRect.offsetMin = new Vector2(24, 14);
            backRect.offsetMax = new Vector2(164, -14);
            backBtn.onClick.AddListener(() => ScreenManager.Instance.Back());

            UITheme.Label(header, "SETTINGS", UITheme.FontH2, UITheme.TextPrimary, TextAnchor.MiddleCenter, FontStyle.Bold);

            var panel = UITheme.Panel(rect, "Panel", UITheme.BgPanel);
            panel.anchorMin = new Vector2(0.5f, 0.5f);
            panel.anchorMax = new Vector2(0.5f, 0.5f);
            panel.pivot = new Vector2(0.5f, 0.5f);
            panel.sizeDelta = new Vector2(720, 480);

            var unitsNote = UITheme.Label(panel,
                "UNITS\nAll values use real engineering units:\nthrust in kN · mass in tonnes · fuel in tonnes · distances in km · money in USD.",
                UITheme.FontBody, UITheme.TextPrimary, TextAnchor.UpperLeft);
            var uNoteRect = unitsNote.GetComponent<RectTransform>();
            uNoteRect.anchorMin = new Vector2(0, 1);
            uNoteRect.anchorMax = new Vector2(1, 1);
            uNoteRect.pivot = new Vector2(0, 1);
            uNoteRect.offsetMin = new Vector2(32, -130);
            uNoteRect.offsetMax = new Vector2(-32, -30);

            // Audio
            var audioTitle = UITheme.Label(panel, "AUDIO", UITheme.FontH3, UITheme.Accent, TextAnchor.UpperLeft, FontStyle.Bold);
            var aRect = audioTitle.GetComponent<RectTransform>();
            aRect.anchorMin = new Vector2(0, 1);
            aRect.anchorMax = new Vector2(1, 1);
            aRect.pivot = new Vector2(0, 1);
            aRect.offsetMin = new Vector2(32, -300);
            aRect.offsetMax = new Vector2(-32, -258);

            MakeAudioRow(panel, "Music", GameState.Settings.musicVolume, v => GameState.Settings.SetMusicVolume(v));
            MakeAudioRow(panel, "Sound effects", GameState.Settings.sfxVolume, v => GameState.Settings.SetSfxVolume(v));

            // Credits / licenses
            var creditsBtn = UITheme.TextButton(panel, "CREDITS & LICENSES", UITheme.FontBody);
            var cRect = creditsBtn.GetComponent<RectTransform>();
            cRect.anchorMin = new Vector2(0.5f, 0);
            cRect.anchorMax = new Vector2(0.5f, 0);
            cRect.pivot = new Vector2(0.5f, 0);
            cRect.sizeDelta = new Vector2(320, 48);
            cRect.anchoredPosition = new Vector2(0, 24);
            creditsBtn.onClick.AddListener(() => ScreenManager.Instance.Show("credits"));


        }

        void MakeAudioRow(RectTransform panel, string name, float value, System.Action<float> set)
        {
            var label = UITheme.Label(panel, name, UITheme.FontBody, UITheme.TextPrimary, TextAnchor.MiddleLeft);
            var lRect = label.GetComponent<RectTransform>();
            lRect.anchorMin = new Vector2(0, 1);
            lRect.anchorMax = new Vector2(0, 1);
            lRect.pivot = new Vector2(0, 1);
            lRect.sizeDelta = new Vector2(220, 30);
            lRect.anchoredPosition = new Vector2(32, name == "Music" ? -350 : -420);

            var sliderRect = new GameObject("SliderHolder", typeof(RectTransform));
            var sRect = sliderRect.GetComponent<RectTransform>();
            sRect.SetParent(panel, false);
            sRect.anchorMin = new Vector2(0, 1);
            sRect.anchorMax = new Vector2(1, 1);
            sRect.pivot = new Vector2(0, 1);
            sRect.offsetMin = new Vector2(280, name == "Music" ? -350 : -420);
            sRect.offsetMax = new Vector2(-32, name == "Music" ? -322 : -392);
            var slider = UITheme.HSlider(sRect, value, UITheme.Accent);
            slider.onValueChanged.AddListener(v => set(v));
        }
    }
}
