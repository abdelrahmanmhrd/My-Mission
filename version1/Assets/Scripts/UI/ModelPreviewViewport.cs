using UnityEngine;
using UnityEngine.UI;
using SpaceMission.Data;

namespace SpaceMission.UI
{
    /// 3D model preview viewport for selection pages: an offscreen camera renders
    /// the component's model to a RenderTexture shown in a RawImage frame.
    /// The model slowly rotates. Procedural stand-ins cover items without models:
    /// fuel types get a tank, instruments get a science module.
    public class ModelPreviewViewport : MonoBehaviour
    {
        Camera previewCam;
        RenderTexture rt;
        GameObject modelInstance;
        Transform pivot;
        float spin;

        public static ModelPreviewViewport Create(Transform parent, string title)
        {
            // frame + heading
            var frame = UITheme.BorderedPanel(parent, "PreviewFrame", UITheme.BgPanelAlt, UITheme.Border, 1);
            frame.anchorMin = new Vector2(1, 1);
            frame.anchorMax = new Vector2(1, 1);
            frame.pivot = new Vector2(1, 1);
            frame.sizeDelta = new Vector2(360, 400);
            frame.anchoredPosition = new Vector2(-24, -150);

            var heading = UITheme.Label(frame, title, UITheme.FontSmall, UITheme.TextMuted, TextAnchor.UpperCenter, FontStyle.Bold);
            var hRect = heading.GetComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0, 1);
            hRect.anchorMax = new Vector2(1, 1);
            hRect.pivot = new Vector2(0.5f, 1);
            hRect.offsetMin = new Vector2(8, -28);
            hRect.offsetMax = new Vector2(-8, -6);

            var viewportGo = new GameObject("Viewport", typeof(RectTransform), typeof(RawImage));
            var vpRect = viewportGo.GetComponent<RectTransform>();
            vpRect.SetParent(frame, false);
            vpRect.anchorMin = new Vector2(0.08f, 0.05f);
            vpRect.anchorMax = new Vector2(0.92f, 0.93f);
            vpRect.offsetMin = Vector2.zero;
            vpRect.offsetMax = Vector2.zero;
            var raw = viewportGo.GetComponent<RawImage>();
            raw.raycastTarget = false;

            var go = new GameObject("ModelPreviewViewport", typeof(ModelPreviewViewport));
            var vp = go.GetComponent<ModelPreviewViewport>();
            vp.Init(raw);
            return vp;
        }

        void Init(RawImage target)
        {
            rt = new RenderTexture(512, 512, 24);
            rt.antiAliasing = 4;

            var camGo = new GameObject("PreviewCam", typeof(Camera));
            previewCam = camGo.GetComponent<Camera>();
            previewCam.targetTexture = rt;
            previewCam.clearFlags = CameraClearFlags.SolidColor;
            previewCam.backgroundColor = new Color(0.03f, 0.04f, 0.07f);
            previewCam.transform.position = new Vector3(0f, 0.8f, -3.2f);
            previewCam.transform.rotation = Quaternion.Euler(8f, 0f, 0f);

            var lightGo = new GameObject("PreviewLight", typeof(Light));
            lightGo.GetComponent<Light>().type = LightType.Directional;
            lightGo.transform.rotation = Quaternion.Euler(-40f, -30f, 0f);
            lightGo.GetComponent<Light>().intensity = 1.5f;

            pivot = new GameObject("Pivot").transform;

            target.texture = rt;
        }

        /// Shows a model by Resources path; null path clears the preview.
        public void ShowModel(string resourcePath)
        {
            ClearModel();
            var model = ModelLibrary.Load(resourcePath);
            if (model == null) return;
            modelInstance = Instantiate(model, pivot);
            Fit(modelInstance, 1.8f);
            modelInstance.transform.localPosition = new Vector3(0f, -0.5f, 0f);
        }

        /// Procedural tank stand-in for fuel types (colored by fuel).
        public void ShowFuelTank(Color tint, string label)
        {
            ClearModel();
            var root = new GameObject("FuelTankPreview");
            root.transform.SetParent(pivot, false);

            var body = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            body.transform.SetParent(root.transform, false);
            body.transform.localScale = new Vector3(0.55f, 0.9f, 0.55f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = tint;
            body.GetComponent<Renderer>().material = mat;

            var cap = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            cap.transform.SetParent(root.transform, false);
            cap.transform.localPosition = new Vector3(0f, 0.95f, 0f);
            cap.transform.localScale = new Vector3(0.3f, 0.22f, 0.3f);
            var capMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            capMat.color = new Color(0.7f, 0.72f, 0.75f);
            cap.GetComponent<Renderer>().material = capMat;

            modelInstance = root;
            modelInstance.transform.localPosition = new Vector3(0f, -0.35f, 0f);
        }

        /// Procedural science-module stand-in for instruments.
        public void ShowInstrumentBox(Color tint)
        {
            ClearModel();
            var root = new GameObject("InstrumentPreview");
            root.transform.SetParent(pivot, false);

            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.transform.SetParent(root.transform, false);
            box.transform.localScale = new Vector3(0.8f, 0.6f, 0.8f);
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = tint;
            box.GetComponent<Renderer>().material = mat;

            var dish = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dish.transform.SetParent(root.transform, false);
            dish.transform.localPosition = new Vector3(0f, 0.55f, 0f);
            dish.transform.localScale = new Vector3(0.35f, 0.18f, 0.35f);
            var dishMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            dishMat.color = new Color(0.85f, 0.86f, 0.9f);
            dish.GetComponent<Renderer>().material = dishMat;

            modelInstance = root;
            modelInstance.transform.localPosition = new Vector3(0f, -0.25f, 0f);
        }

        public void ClearModel()
        {
            if (modelInstance != null) Destroy(modelInstance);
            modelInstance = null;
        }

        static void Fit(GameObject model, float targetSize)
        {
            var bounds = new Bounds(model.transform.position, Vector3.zero);
            foreach (var r in model.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds);
            var maxDim = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (maxDim < 0.001f) return;
            model.transform.localScale = model.transform.localScale * (targetSize / maxDim);
        }

        void Update()
        {
            if (pivot != null)
            {
                spin += 40f * Time.deltaTime;
                pivot.rotation = Quaternion.Euler(0f, spin, 0f);
            }
        }

        void OnDestroy()
        {
            if (rt != null) rt.Release();
            if (previewCam != null) Destroy(previewCam.gameObject);
            if (pivot != null) Destroy(pivot.gameObject);
        }
    }
}
