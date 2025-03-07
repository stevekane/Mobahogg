using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomSceneViewWindow : EditorWindow {
  [MenuItem("Window/Custom Scene View (PreviewRenderUtility)")]
  public static void ShowWindow() {
    var window = GetWindow<CustomSceneViewWindow>();
    window.titleContent = new GUIContent("Custom Scene View");
    window.Show();
  }
  public void CreateGUI() {
    var previewElement = new PreviewRenderUtilityElement();
    previewElement.style.flexGrow = 1;
    rootVisualElement.Add(previewElement);
  }
}

public class PreviewRenderUtilityElement : VisualElement {
  PreviewRenderUtility previewUtility;
  Volume Volume;
  Image imageElement;
  int currentWidth;
  int currentHeight;

  public PreviewRenderUtilityElement() {
    RegisterCallback<AttachToPanelEvent>(OnAttach);
    RegisterCallback<DetachFromPanelEvent>(OnDetach);
    RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
  }

  void OnAttach(AttachToPanelEvent evt) {
    InitPreview();
    EditorApplication.update += UpdatePreview;
  }

  void OnDetach(DetachFromPanelEvent evt) {
    EditorApplication.update -= UpdatePreview;
    CleanupPreview();
  }

  void OnGeometryChanged(GeometryChangedEvent evt) {
    currentWidth = Mathf.Max(1, (int)evt.newRect.width);
    currentHeight = Mathf.Max(1, (int)evt.newRect.height);
  }

  void InitPreview() {
    previewUtility = new PreviewRenderUtility();
    previewUtility.camera.fieldOfView = 30f;
    previewUtility.camera.nearClipPlane = 0.1f;
    previewUtility.camera.farClipPlane = 100f;
    previewUtility.camera.clearFlags = CameraClearFlags.SolidColor;
    previewUtility.camera.backgroundColor = Color.white;
    previewUtility.camera.transform.SetPositionAndRotation(new(0,0,-5), Quaternion.LookRotation(Vector3.forward));
    previewUtility.camera.forceIntoRenderTexture = true;
    previewUtility.camera.allowHDR = true;
    var additionalData = previewUtility.camera.GetUniversalAdditionalCameraData();
    additionalData.renderPostProcessing = true;
    additionalData.requiresDepthTexture = true;
    additionalData.SetRenderer(0);
    previewUtility.lights[0].intensity = 1f;
    previewUtility.lights[0].transform.rotation = Quaternion.Euler(50, -30, 0);
    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    cube.hideFlags = HideFlags.HideAndDontSave;
    cube.transform.position = Vector3.zero;
    Material emissiveMat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
    emissiveMat.hideFlags = HideFlags.HideAndDontSave;
    emissiveMat.EnableKeyword("_EMISSION");
    emissiveMat.SetColor("_EmissionColor", Color.red * 25f);
    cube.GetComponent<MeshRenderer>().sharedMaterial = emissiveMat;
    previewUtility.AddSingleGO(cube);
    GameObject volumeGO = new GameObject("Preview Post Process Volume");
    Volume = volumeGO.AddComponent<Volume>();
    Volume.hideFlags = HideFlags.HideAndDontSave;
    Volume.weight = 1;
    Volume.isGlobal = true;
    VolumeProfile profile = ScriptableObject.CreateInstance<VolumeProfile>();
    Vignette vignette = profile.Add<Vignette>(true);
    vignette.active = true;
    vignette.intensity.value = 0.25f;
    Bloom bloom = profile.Add<Bloom>(true);
    bloom.active = true;
    bloom.intensity.value = 2f;
    bloom.threshold.value = 0f;
    bloom.scatter.value = 0.7f;
    Volume.profile = profile;
    previewUtility.AddSingleGO(volumeGO);
    imageElement = new Image();
    imageElement.scaleMode = ScaleMode.StretchToFill;
    Add(imageElement);
  }

  void UpdatePreview() {
    if (previewUtility == null)
      return;
    if (currentWidth == 0 || currentHeight == 0)
      return;
    Rect rect = new Rect(0, 0, currentWidth, currentHeight);
    previewUtility.BeginPreview(rect, GUIStyle.none);
    previewUtility.RenderURP();
    Texture tex = previewUtility.EndPreview();
    imageElement.image = tex;
  }

  void CleanupPreview() {
    if (previewUtility != null) {
      previewUtility.Cleanup();
      previewUtility = null;
    }
  }
}