using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using SpaceMission.Core;
using SpaceMission.Data;
using SpaceMission.Engineering;
using SpaceMission.UI;

namespace SpaceMission.Simulation
{
    /// Builds the launch scene contents procedurally (pad, rocket assembled from the
    /// player's configuration, effects, camera, HUD) and runs the ascent sequence.
    public class SimSceneBootstrap : MonoBehaviour
    {
        /// Called by GameManager once the SimLaunch scene is loaded additively.
        /// Static + synchronous: no loader-object lifetime issues.
        public static void BuildScene()
        {
            var report = GameState.LastValidation;
            if (report == null) report = GameManager.Instance.ValidateNow();
            BuildContents(report);
        }

        void Start()
        {
            // Legacy path (loader object); the GameManager coroutine path is preferred.
            var sim = SceneManager.GetSceneByName("SimLaunch");
            if (sim.isLoaded && UnityEngine.Object.FindAnyObjectByType<MissionSimulationDirector>() == null)
                BuildContents(GameState.LastValidation ?? GameManager.Instance.ValidateNow());
        }

        static void BuildContents(ValidationReport report)
        {
            if (report == null) report = GameManager.Instance.ValidateNow();

            // --- Environment: ground, pad, sky ---
            var env = new GameObject("Environment");
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            ground.transform.SetParent(env.transform, false);
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            var groundMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            groundMat.color = new Color(0.23f, 0.25f, 0.20f);
            ground.GetComponent<Renderer>().material = groundMat;

            var padRoot = new GameObject("LaunchPad");
            padRoot.transform.SetParent(env.transform, false);
            var gantryModel = ModelLibrary.Load(ModelLibrary.LaunchpadModelPath(null));
            if (gantryModel != null)
            {
                var gantry = Instantiate(gantryModel, padRoot.transform);
                gantry.transform.localPosition = new Vector3(6f, 0f, -3f);
                NormalizeScale(gantry, 40f);
            }
            else
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                tower.name = "Tower";
                tower.transform.SetParent(padRoot.transform, false);
                tower.transform.localPosition = new Vector3(6f, 20f, -3f);
                tower.transform.localScale = new Vector3(1.5f, 40f, 1.5f);
                tower.GetComponent<Renderer>().material.color = new Color(0.55f, 0.56f, 0.58f);
            }

            // --- Rocket assembled from the player's vehicle + spacecraft ---
            var rocketRoot = new GameObject("Rocket");
            float stackTop = 0f;

            var vehicleModel = ModelLibrary.Load(ModelLibrary.VehicleModelPath(GameState.Config.vehicleId));
            if (vehicleModel != null)
            {
                var v = Instantiate(vehicleModel, rocketRoot.transform);
                NormalizeScale(v, 30f, out var height);
                v.transform.localPosition = Vector3.zero;
                stackTop += height;
            }
            else
            {
                var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                body.name = "PlaceholderBody";
                body.transform.SetParent(rocketRoot.transform, false);
                body.transform.localPosition = new Vector3(0f, 15f, 0f);
                body.transform.localScale = new Vector3(3f, 15f, 3f);
                body.GetComponent<Renderer>().material.color = new Color(0.85f, 0.85f, 0.86f);
                stackTop = 30f;
            }

            if (!string.IsNullOrEmpty(GameState.Config.spacecraftId))
            {
                var craftModel = ModelLibrary.Load(ModelLibrary.SpacecraftModelPath(GameState.Config.spacecraftId));
                if (craftModel != null)
                {
                    var c = Instantiate(craftModel, rocketRoot.transform);
                    NormalizeScale(c, 10f, out _);
                    c.transform.localPosition = new Vector3(0f, stackTop, 0f);
                }
            }

            // --- Effects: engine flame + smoke at the base ---
            var effectsRoot = new GameObject("Effects");
            effectsRoot.transform.SetParent(rocketRoot.transform, false);
            var flame = CreateEffect(effectsRoot.transform, "Flame", new Color(1f, 0.55f, 0.15f), 2.2f, 24, 14f, 1.6f);
            var smoke = CreateEffect(effectsRoot.transform, "Smoke", new Color(0.75f, 0.75f, 0.78f, 0.6f), 6f, 18, 6f, 4f);

            // --- Camera + director + HUD ---
            var cameraGo = new GameObject("SimCamera", typeof(Camera), typeof(AudioListener), typeof(SimCamera));
            var simCamera = cameraGo.GetComponent<SimCamera>();
            cameraGo.tag = "MainCamera";
            // render above any other scene camera (the boot scene's camera stays behind)
            cameraGo.GetComponent<Camera>().depth = 10;
            simCamera.transform.position = new Vector3(24f, 18f, -30f);
            simCamera.Setup(rocketRoot.transform);

            var directorGo = new GameObject("MissionSimulationDirector");
            var director = directorGo.AddComponent<MissionSimulationDirector>();
            director.Begin(report);

            var launchGo = new GameObject("LaunchSimulation");
            var launchSim = launchGo.AddComponent<LaunchSimulation>();
            launchSim.rocketRoot = rocketRoot;
            launchSim.flameParticles = flame;
            launchSim.smokeParticles = smoke;

            var hudCanvas = new GameObject("SimHUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = hudCanvas.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;
            var scaler = hudCanvas.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            if (UnityEngine.Object.FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
                new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
            SimTelemetryUI.Build(hudCanvas.transform, launchSim);

            launchSim.Begin(director, director.AscentDuration);

            director.Finished += _ =>
            {
                GameManager.Instance.RunMission();
                var sm = ScreenManager.Instance;
                if (sm != null) sm.Show("debrief");
                SceneManager.UnloadSceneAsync("SimLaunch");   // fire-and-forget
            };
        }

        static void NormalizeScale(GameObject model, float targetHeight) =>
            NormalizeScale(model, targetHeight, out _);

        static void NormalizeScale(GameObject model, float targetHeight, out float height)
        {
            var bounds = new Bounds(model.transform.position, Vector3.zero);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            var size = bounds.size.y > 0.001f ? bounds.size.y : 1f;
            var factor = targetHeight / size;
            model.transform.localScale = model.transform.localScale * factor;
            var b2 = new Bounds(model.transform.position, Vector3.zero);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) b2.Encapsulate(r.bounds);
            var center = model.transform.parent.InverseTransformPoint(b2.center);
            model.transform.localPosition -= new Vector3(0f, center.y - b2.extents.y / factor, 0f);
            height = targetHeight;
        }

        static ParticleSystem CreateEffect(Transform parent, string name, Color color, float lifetime, int count, float speed, float size)
        {
            var go = new GameObject(name, typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = Mathf.Infinity;
            main.loop = true;
            main.startLifetime = lifetime;
            main.startSpeed = speed;
            main.startSize = size;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = count;
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 12f;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = color;
            return ps;
        }
    }
}
