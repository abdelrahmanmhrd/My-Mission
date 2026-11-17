using UnityEngine;
using SpaceMission.Data;

namespace SpaceMission.UI
{
    /// Animated space background for the UI (GDD 4.2): Earth rotating, the ISS
    /// orbiting it, and a starfield — rendered live behind the menu screens.
    /// Runs in the boot scene; the UI canvas draws on top.
    public class MenuSpaceBackground : MonoBehaviour
    {
        public static MenuSpaceBackground Instance { get; private set; }

        Transform earth;
        Transform iss;
        Transform starsFar;
        Transform starsNear;
        Camera bgCamera;
        float issOrbitAngle;
        float earthSpin;

        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            Build();
        }

        void Build()
        {
            // --- Camera (background layers only; UI canvas is overlay) ---
            var camGo = new GameObject("BgCamera", typeof(Camera));
            bgCamera = camGo.GetComponent<Camera>();
            bgCamera.depth = -10;                     // renders first
            bgCamera.clearFlags = CameraClearFlags.SolidColor;
            bgCamera.backgroundColor = new Color(0.012f, 0.016f, 0.035f);
            bgCamera.transform.position = new Vector3(0f, 0f, -60f);
            bgCamera.transform.Rotate(0f, 0f, -8f);

            // URP doesn't composite multiple base cameras — the scene's default
            // camera would overwrite this one entirely. Disable it; the UI is an
            // overlay canvas and the 3D sim brings its own higher-depth camera.
            var mainCam = Camera.main;
            if (mainCam != null && mainCam != bgCamera)
                mainCam.gameObject.SetActive(false);

            var root = new GameObject("SpaceBackground");

            // dedicated light for the background scene (independent of gameplay lights)
            var sun = new GameObject("BgSun", typeof(Light));
            sun.transform.SetParent(root.transform, false);
            sun.transform.rotation = Quaternion.Euler(-25f, -35f, 0f);
            var light = sun.GetComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.6f;
            light.color = new Color(1f, 0.96f, 0.9f);

            // --- Earth ---
            var earthModel = ModelLibrary.Load(ModelLibrary.PlanetModelPath("PLNT-EARTH"));
            if (earthModel != null)
            {
                earth = Instantiate(earthModel, root.transform).transform;
                earth.localPosition = new Vector3(-8f, -2f, 30f);
                FitDiameter(earth.gameObject, 26f);
                // use the Earth texture if the model came untextured
                var earthTex = Resources.Load<Texture2D>("Textures/earth_2k");
                if (earthTex != null)
                    foreach (var r in earth.GetComponentsInChildren<Renderer>())
                    {
                        var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        mat.mainTexture = earthTex;
                        mat.EnableKeyword("_EMISSION");
                        mat.SetTexture("_EmissionMap", earthTex);
                        mat.SetColor("_EmissionColor", new Color(0.75f, 0.75f, 0.75f));
                        r.material = mat;
                    }
            }
            else
            {
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.transform.SetParent(root.transform, false);
                sphere.transform.localPosition = new Vector3(-8f, -2f, 30f);
                sphere.transform.localScale = Vector3.one * 26f;
                var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                var tex = Resources.Load<Texture2D>("Textures/earth_2k");
                if (tex != null) mat.mainTexture = tex;
                sphere.GetComponent<Renderer>().material = mat;
                earth = sphere.transform;
            }

            // --- ISS orbiting ---
            var issGo = new GameObject("ISSOrbit");
            issGo.transform.SetParent(root.transform, false);
            issGo.transform.position = earth.position;
            var issModel = Resources.Load<GameObject>("Models/Launchpad/ISS");
            if (issModel != null)
            {
                var issInst = Instantiate(issModel, issGo.transform);
                issInst.transform.localPosition = new Vector3(0f, 0f, 15.5f);   // orbit radius
                issInst.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
                FitDiameter(issInst, 2.2f);
                iss = issGo.transform;
            }

            // --- Starfield: two parallax layers, even coverage, continuous drift ---
            starsFar = MakeStarLayer(root.transform, "StarsFar", 900, new Vector2(0.22f, 0.4f), new Vector3(760f, 440f, 1f), new Color(0.75f, 0.8f, 1f));
            starsNear = MakeStarLayer(root.transform, "StarsNear", 350, new Vector2(0.5f, 0.9f), new Vector3(700f, 400f, 1f), new Color(1f, 1f, 1f));
            starsNear.transform.localPosition = new Vector3(0f, 0f, 60f);   // closer → parallax
        }

        static Transform MakeStarLayer(Transform parent, string name, int count, Vector2 sizeRange, Vector3 boxScale, Color color)
        {
            var go = new GameObject(name, typeof(ParticleSystem));
            go.transform.SetParent(parent, false);
            var ps = go.GetComponent<ParticleSystem>();
            var main = ps.main;
            main.duration = Mathf.Infinity;
            main.loop = true;
            main.startLifetime = Mathf.Infinity;
            main.startSpeed = 0f;
            main.startSize = new ParticleSystem.MinMaxCurve(sizeRange.x, sizeRange.y);   // random sizes
            main.maxParticles = count;
            main.simulationSpace = ParticleSystemSimulationSpace.Local;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, (short)count) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = boxScale;
            var renderer = go.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            renderer.material.color = color;
            return go.transform;
        }

        static void FitDiameter(GameObject model, float targetDiameter)
        {
            var bounds = new Bounds(model.transform.position, Vector3.zero);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            if (bounds.size.magnitude < 0.001f) return;
            var maxDim = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (maxDim < 0.001f) return;
            model.transform.localScale = model.transform.localScale * (targetDiameter / maxDim);
        }

        void Update()
        {
            if (earth != null) earth.Rotate(Vector3.up, 1.2f * Time.deltaTime, Space.Self);
            if (iss != null)
            {
                issOrbitAngle += 6f * Time.deltaTime;   // one visible orbit ~60 s
                iss.rotation = Quaternion.Euler(0f, issOrbitAngle, 18f);
            }
            // parallax drift: both layers move continuously and wrap around,
            // so the field always reads as alive (never static, never empty edges)
            if (starsFar != null) Drift(starsFar, 1.6f, 380f);
            if (starsNear != null) Drift(starsNear, -3.2f, 350f);
        }

        static void Drift(Transform layer, float unitsPerSecond, float wrap)
        {
            var p = layer.localPosition;
            p.x += unitsPerSecond * Time.deltaTime;
            if (p.x > wrap) p.x -= wrap * 2f;
            if (p.x < -wrap) p.x += wrap * 2f;
            p.y += unitsPerSecond * 0.15f * Time.deltaTime;
            if (p.y > wrap * 0.5f) p.y -= wrap;
            if (p.y < -wrap * 0.5f) p.y += wrap;
            layer.localPosition = p;
        }
    }
}
