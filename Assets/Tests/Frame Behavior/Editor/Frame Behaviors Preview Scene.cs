using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

class FrameBehaviorsPreviewScene : VisualElement {
  PreviewRenderUtility Preview;
  Image RenderImage;
  GameObject Ground;
  List<FrameBehavior> ReferenceFrameBehaviors = new List<FrameBehavior>();
  List<FrameBehavior> FrameBehaviors = new List<FrameBehavior>();
  MonoBehaviour Provider;
  int Frame;

  // Camera control fields
  public float MinZoom = 1f;
  public float MaxZoom = 10f;
  public float ZoomScalar = 0.25f;
  public float RotationScalar = 1f;
  public float CameraFocusHeight = 1f;
  public float CameraZoom = 5f;
  public Vector2 CameraRotation = new Vector2(15, 90);

  Vector3 CameraLookAtTarget =>
    Provider ? Provider.transform.position + CameraFocusHeight * Vector3.up : CameraFocusHeight * Vector3.up;
  Vector3 ComputedCameraOffset =>
    Quaternion.Euler(CameraRotation.x, CameraRotation.y, 0) * Vector3.back * CameraZoom;

  // For pointer-drag detection
  bool isDragging = false;
  Vector3 lastPointerPosition;

  void OnAttach(AttachToPanelEvent evt) {
    style.height = 512;
    EditorApplication.update += Update;
  }

  void OnDetach(DetachFromPanelEvent evt) {
    FrameBehavior.PreviewCancelActiveBehaviors(FrameBehaviors, Frame, Preview);
    FrameBehavior.PreviewCleanupBehaviors(FrameBehaviors, Provider);
    Preview.Cleanup();
    EditorApplication.update -= Update;
  }

  public void SetProvider(MonoBehaviour provider) {
    if (Provider) {
      GameObject.DestroyImmediate(Provider.gameObject);
    }
    Provider = MonoBehaviour.Instantiate(provider);
    if (Provider.TryGetComponent(out AvatarAttacher avatarAttacher)) {
      avatarAttacher.Attach();
    }
    Preview.AddSingleGO(Provider.gameObject);
  }

  public void Seek(int targetFrame) {
    Frame = targetFrame;
  }

  public void SetFrameBehaviors(IEnumerable<FrameBehavior> frameBehaviors) {
    ReferenceFrameBehaviors = frameBehaviors.ToList();
  }

  public FrameBehaviorsPreviewScene() {
    RenderImage = new Image();
    Add(RenderImage);
    RegisterCallback<AttachToPanelEvent>(OnAttach);
    RegisterCallback<DetachFromPanelEvent>(OnDetach);
    RegisterCallback<WheelEvent>(OnWheelEvent);
    RegisterCallback<PointerDownEvent>(OnPointerDown);
    RegisterCallback<PointerMoveEvent>(OnPointerMove);
    RegisterCallback<PointerUpEvent>(OnPointerUp);
    BuildPreview();
  }

  void BuildPreview() {
    Preview = new PreviewRenderUtility();
    Preview.camera.nearClipPlane = 0.1f;
    Preview.camera.farClipPlane = 1000f;
    Preview.camera.fieldOfView = 45f;
    Preview.camera.clearFlags = CameraClearFlags.SolidColor | CameraClearFlags.Depth;
    Preview.camera.backgroundColor = Color.black;
    Preview.camera.cameraType = CameraType.SceneView;
    Preview.camera.transform.position = CameraLookAtTarget + ComputedCameraOffset;
    Preview.camera.GetOrAddComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
    Ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
    Ground.transform.position = Vector3.zero;
    Ground.hideFlags = HideFlags.HideAndDontSave;
    Preview.AddSingleGO(Ground);
    var keyLight = Preview.lights[0];
    keyLight.type = LightType.Directional;
    keyLight.transform.position = new Vector3(-2, 3, 2);
    keyLight.transform.LookAt(Vector3.zero);
    keyLight.intensity = 1f;
    keyLight.color = new Color(1.0f, 0.85f, 0.73f);
    keyLight.shadows = LightShadows.Hard;
    keyLight.shadowCustomResolution = 2048 * 2;
    keyLight.shadowStrength = 0.8f;
    var rimLight = Preview.lights[1];
    rimLight.type = LightType.Directional;
    rimLight.transform.position = new Vector3(0, 3, -2);
    rimLight.transform.LookAt(Vector3.up);
    rimLight.intensity = 0.35f;
    rimLight.color = new Color(0.88f, 0.94f, 1.0f);
    rimLight.shadows = LightShadows.Soft;
    rimLight.shadowStrength = 0.3f;
    rimLight.shadowCustomResolution = 2048;
  }

  void Update() {
    // Render previous frame
    Preview.camera.transform.position = CameraLookAtTarget + ComputedCameraOffset;
    Preview.camera.transform.LookAt(CameraLookAtTarget);
    Rect rct = contentRect;
    if (rct.width > 0 && rct.height > 0) {
      Preview.BeginPreview(rct, GUIStyle.none);
      Preview.Render(true);
      Texture tex = Preview.EndPreview();
      RenderImage.image = tex;
      RenderImage.MarkDirtyRepaint();
    }

    // Simulate current frame
    FrameBehavior.PreviewCancelActiveBehaviors(FrameBehaviors, Frame, Preview);
    FrameBehavior.PreviewCleanupBehaviors(FrameBehaviors, Provider);
    Provider.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
    FrameBehaviors = ReferenceFrameBehaviors.Select(fb => fb.Clone()).ToList();
    FrameBehavior.PreviewInitializeBehaviors(FrameBehaviors, Provider);
    for (var i = 0; i <= Frame; i++) {
      FrameBehavior.PreviewStartBehaviors(FrameBehaviors, i, Preview);
    }
    for (var i = 0; i <= Frame; i++) {
      FrameBehavior.PreviewUpdateBehaviors(FrameBehaviors, i, Preview);
    }
    for (var i = 0; i <= Frame; i++) {
      FrameBehavior.PreviewEndBehaviors(FrameBehaviors, i, Preview);
    }
  }

  void OnWheelEvent(WheelEvent e) {
    Rect rct = contentRect;
    if (rct.Contains(e.localMousePosition)) {
      CameraZoom += ZoomScalar * e.delta.y;
      CameraZoom = Mathf.Clamp(CameraZoom, MinZoom, MaxZoom);
      e.StopPropagation();
    }
  }

  void OnPointerDown(PointerDownEvent e) {
    if (e.button == 2) {
      isDragging = true;
      lastPointerPosition = e.position;
      e.StopPropagation();
    }
  }

  void OnPointerMove(PointerMoveEvent e) {
    if (isDragging) {
      Vector2 delta = e.position - lastPointerPosition;
      CameraRotation += RotationScalar * new Vector2(delta.y, delta.x);
      CameraRotation.x = Mathf.Clamp(CameraRotation.x, -30, 90);
      lastPointerPosition = e.position;
      e.StopPropagation();
    }
  }

  void OnPointerUp(PointerUpEvent e) {
    if (e.button == 2) {
      isDragging = false;
      e.StopPropagation();
    }
  }
}
