using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace SpaceMission.UI
{
    /// Owns the root Canvas and routes between screen panels (single-scene flow).
    public class ScreenManager : MonoBehaviour
    {
        public static ScreenManager Instance { get; private set; }

        readonly Dictionary<string, GameObject> screens = new Dictionary<string, GameObject>();
        readonly Stack<string> history = new Stack<string>();
        public string current = "";

        public event Action<string> ScreenChanged;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public void Register(string id, GameObject panel)
        {
            screens[id] = panel;
            panel.SetActive(false);
        }

        public void Show(string id, bool pushHistory = true)
        {
            if (string.IsNullOrEmpty(id) || !screens.ContainsKey(id)) return;
            if (!string.IsNullOrEmpty(current) && screens.ContainsKey(current)) screens[current].SetActive(false);
            if (pushHistory && !string.IsNullOrEmpty(current)) history.Push(current);
            current = id;
            screens[id].SetActive(true);
            ScreenChanged?.Invoke(id);
        }

        public void Back()
        {
            if (history.Count > 0) Show(history.Pop(), false);
        }

        public void ClearHistory() => history.Clear();
    }

    /// Builds the entire UI once at startup: Canvas, EventSystem, all screens.
    public class UIRoot : MonoBehaviour
    {
        void Awake()
        {
            var canvasGo = new GameObject("UICanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;

            var root = canvasGo.transform;
            // semi-transparent scrim so the animated 3D space background shows through
            var bg = UITheme.Panel(root, "Background", new Color(0.016f, 0.02f, 0.04f, 0.35f), raycast: true);

            EnsureEventSystem();

            // Animated Earth + ISS + starfield behind all menu screens
            var bgGo = new GameObject("MenuSpaceBackground");
            bgGo.AddComponent<MenuSpaceBackground>();

            var managerGo = new GameObject("ScreenManager");
            managerGo.transform.SetParent(root, false);
            var manager = managerGo.AddComponent<ScreenManager>();

            var mainMenu = UIMainMenu.Build(root);
            manager.Register("mainMenu", mainMenu.gameObject);

            var missionSelect = UIMissionSelect.Build(root);
            manager.Register("missionSelect", missionSelect.gameObject);

            var briefing = UIBriefing.Build(root);
            manager.Register("briefing", briefing.gameObject);

            var configHub = UIConfigHub.Build(root);
            manager.Register("configHub", configHub.gameObject);

            var selection = UISelection.Build(root);
            manager.Register("selection", selection.gameObject);

            var settings = UISettings.Build(root);
            manager.Register("settings", settings.gameObject);

            var info = UIInfo.Build(root);
            manager.Register("info", info.gameObject);

            var debrief = UIDebrief.Build(root);
            manager.Register("debrief", debrief.gameObject);

            var credits = UICredits.Build(root);
            manager.Register("credits", credits.gameObject);

            manager.Show("mainMenu", false);
        }

        static void EnsureEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
        }
    }
}
