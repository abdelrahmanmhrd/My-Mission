using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SpaceMission.UI
{
    /// Dark aerospace-control-room visual system (GDD 13). All values central here.
    public static class UITheme
    {
        public static readonly Color BgDeep = new Color(0.043f, 0.055f, 0.098f);       // deep navy near-black
        public static readonly Color BgPanel = new Color(0.078f, 0.098f, 0.145f);      // panel
        public static readonly Color BgPanelAlt = new Color(0.102f, 0.125f, 0.180f);   // hover/alt panel
        public static readonly Color Border = new Color(0.180f, 0.230f, 0.310f);
        public static readonly Color Accent = new Color(0.231f, 0.510f, 0.965f);       // restrained blue
        public static readonly Color AccentDim = new Color(0.231f, 0.510f, 0.965f, 0.35f);
        public static readonly Color TextPrimary = new Color(0.898f, 0.910f, 0.937f);
        public static readonly Color TextMuted = new Color(0.612f, 0.639f, 0.698f);
        public static readonly Color Danger = new Color(0.937f, 0.325f, 0.314f);
        public static readonly Color Warning = new Color(0.965f, 0.729f, 0.212f);
        public static readonly Color Success = new Color(0.220f, 0.784f, 0.400f);
        public static readonly Color CardSelected = new Color(0.100f, 0.160f, 0.230f);

        public static readonly Color BtnNormal = new Color(0.125f, 0.155f, 0.220f);
        public static readonly Color BtnHover = new Color(0.160f, 0.200f, 0.280f);
        public static readonly Color BtnAccent = new Color(0.150f, 0.300f, 0.560f);

        public const int FontH1 = 44;
        public const int FontH2 = 30;
        public const int FontH3 = 24;
        public const int FontBody = 20;
        public const int FontSmall = 16;
        public const int FontTiny = 13;

        // TMP assigns its default font automatically; explicit override available if needed.
        public static TMPro.TMP_FontAsset DefaultFont => TMPro.TMP_Settings.defaultFontAsset;

        // --- Widget builders ---

        public static RectTransform Panel(Transform parent, string name, Color color, bool raycast = false)
        {
            var go = new GameObject(name, typeof(Image));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var img = go.GetComponent<Image>();
            img.color = color;
            img.raycastTarget = raycast;
            return rect;
        }

        public static RectTransform BorderedPanel(Transform parent, string name, Color fill, Color border, float borderSize = 1f, bool raycast = false)
        {
            var go = new GameObject(name, typeof(Image), typeof(Outline));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            var img = go.GetComponent<Image>();
            img.color = fill;
            img.raycastTarget = raycast;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = border;
            outline.effectDistance = new Vector2(-borderSize, borderSize);
            return rect;
        }

        public static TMPro.TextMeshProUGUI Label(Transform parent, string text, int size,
            Color color, TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var go = new GameObject("Text", typeof(TMPro.TextMeshProUGUI));
            var rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            var tmp = go.GetComponent<TMPro.TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.color = color;
            tmp.alignment = ToAlignment(align);
            tmp.fontStyle = ToFontStyle(style);
            tmp.raycastTarget = false;
            return tmp;
        }

        static TextAlignmentOptions ToAlignment(TextAnchor anchor)
        {
            switch (anchor)
            {
                case TextAnchor.UpperLeft: return TextAlignmentOptions.TopLeft;
                case TextAnchor.UpperCenter: return TextAlignmentOptions.Top;
                case TextAnchor.UpperRight: return TextAlignmentOptions.TopRight;
                case TextAnchor.MiddleCenter: return TextAlignmentOptions.Center;
                case TextAnchor.MiddleRight: return TextAlignmentOptions.Right;
                case TextAnchor.LowerLeft: return TextAlignmentOptions.BottomLeft;
                case TextAnchor.LowerCenter: return TextAlignmentOptions.Bottom;
                case TextAnchor.LowerRight: return TextAlignmentOptions.BottomRight;
                default: return TextAlignmentOptions.Left;
            }
        }

        static FontStyles ToFontStyle(FontStyle style)
        {
            switch (style)
            {
                case FontStyle.Bold: return FontStyles.Bold;
                case FontStyle.Italic: return FontStyles.Italic;
                case FontStyle.BoldAndItalic: return FontStyles.Bold | FontStyles.Italic;
                default: return FontStyles.Normal;
            }
        }

        public static Button TextButton(Transform parent, string text, int size = FontBody,
            Color? bg = null, Color? fg = null, Color? border = null)
        {
            var rect = BorderedPanel(parent, "Button", bg ?? BtnNormal, border ?? Border, 1f, raycast: true);
            var btn = rect.gameObject.AddComponent<Button>();
            var colors = btn.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            colors.fadeDuration = 0.08f;
            btn.colors = colors;

            var tmp = Label(rect, text, size, fg ?? TextPrimary, TextAnchor.MiddleCenter);
            tmp.raycastTarget = false;
            return btn;
        }

        public static Slider HSlider(Transform parent, float value, Color fillColor)
        {
            var root = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
            var rect = root.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var bg = Panel(rect, "Background", BgPanelAlt);
            bg.anchorMin = new Vector2(0, 0.35f);
            bg.anchorMax = new Vector2(1, 0.65f);
            bg.offsetMin = Vector2.zero;
            bg.offsetMax = Vector2.zero;

            var fillGo = new GameObject("Fill Area", typeof(RectTransform));
            var fillRect = fillGo.GetComponent<RectTransform>();
            fillRect.SetParent(rect, false);
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(0, 0);
            fillRect.offsetMax = Vector2.zero;

            var fillPanel = Panel(fillRect, "Fill", fillColor);
            fillPanel.anchorMin = Vector2.zero;
            fillPanel.anchorMax = Vector2.one;
            fillPanel.offsetMin = Vector2.zero;
            fillPanel.offsetMax = Vector2.zero;

            var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            var handleRect = handleGo.GetComponent<RectTransform>();
            handleRect.SetParent(rect, false);
            handleRect.sizeDelta = new Vector2(10, 0);
            handleGo.GetComponent<Image>().color = TextPrimary;

            var slider = root.GetComponent<Slider>();
            slider.fillRect = fillPanel;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleGo.GetComponent<Image>();
            slider.direction = Slider.Direction.LeftToRight;
            slider.minValue = 0;
            slider.maxValue = 1;
            slider.value = value;
            return slider;
        }
    }

    /// Keeps a LayoutElement's preferred height in sync with TMP text reflow.
    public class AutoHeight : MonoBehaviour
    {
        TMPro.TMP_Text tmp;
        LayoutElement le;
        public float padding = 8f;

        void Awake()
        {
            tmp = GetComponent<TMPro.TMP_Text>();
            le = GetComponent<LayoutElement>();
            if (le == null) le = gameObject.AddComponent<LayoutElement>();
        }

        void LateUpdate()
        {
            if (tmp != null && tmp.gameObject.activeInHierarchy)
                le.preferredHeight = tmp.preferredHeight + padding;
        }
    }

    // Helper types used only inside UITheme font resolution.
    class TMPSettingsHolder { public TMPro.TMP_FontAsset defaultFontAsset; }
}
