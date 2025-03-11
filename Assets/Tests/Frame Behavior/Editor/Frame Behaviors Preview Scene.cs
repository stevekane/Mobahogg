using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering.Universal;
using UnityEditor.Experimental.GraphView;

class FrameBehaviorsPreviewScene : VisualElement {
  PreviewRenderUtility Preview;
  Image RenderImage;
  IEnumerable<FrameBehavior> ReferenceFrameBehaviors = new List<FrameBehavior>();
  List<FrameBehavior> FrameBehaviors = new List<FrameBehavior>();
  MonoBehaviour ReferenceProvider;
  MonoBehaviour Provider;
  Vector3 lastPointerPosition;

  int Frame;

  public float MinZoom = 1f;
  public float MaxZoom = 10f;
  public float ZoomScalar = 0.25f;
  public float RotationScalar = 1f;
  public float CameraFocusHeight = 1f;
  public float CameraZoom = 5f;
  public Vector2 CameraRotation = new Vector2(15, 90);

  Vector3 CameraLookAtTarget =>
    Provider
      ? Provider.transform.position + CameraFocusHeight * Vector3.up
      : CameraFocusHeight * Vector3.up;
  Vector3 ComputedCameraOffset =>
    CameraZoom * (Quaternion.Euler(CameraRotation.x, CameraRotation.y, 0) * Vector3.back);

  public FrameBehaviorsPreviewScene() {
    style.height = 512;
    RenderImage = new Image();
    Add(RenderImage);
    RegisterCallback<AttachToPanelEvent>(OnAttach);
    RegisterCallback<DetachFromPanelEvent>(OnDetach);
    RegisterCallback<WheelEvent>(OnWheelEvent);
    RegisterCallback<PointerDownEvent>(OnPointerDown);
    RegisterCallback<PointerMoveEvent>(OnPointerMove);
    RegisterCallback<PointerUpEvent>(OnPointerUp);
  }

  // Store the provider prefab. We instantiate a new instance from it every frame.
  public void SetProvider(MonoBehaviour provider) {
    ReferenceProvider = provider;
  }

  public void Seek(int targetFrame) {
    Frame = targetFrame;
  }

  public void SetFrameBehaviors(IEnumerable<FrameBehavior> frameBehaviors) {
    ReferenceFrameBehaviors = frameBehaviors;
  }

  void OnAttach(AttachToPanelEvent evt) {
    BuildPreview();
    EditorApplication.update += Update;
  }

  void OnDetach(DetachFromPanelEvent evt) {
    FrameBehavior.PreviewCancelActiveBehaviors(FrameBehaviors, Frame, Preview);
    FrameBehavior.PreviewCleanupBehaviors(FrameBehaviors, Provider);
    Provider.TryDestroyImmediateGameObject();
    if (Preview != null) {
      Preview.Cleanup();
      Preview = null;
    }
    EditorApplication.update -= Update;
  }

  void BuildPreview() {
    Preview = new PreviewRenderUtility();
    Preview.camera.fieldOfView = 45f;
    Preview.camera.nearClipPlane = 0.1f;
    Preview.camera.farClipPlane = 1000f;
    Preview.camera.clearFlags = CameraClearFlags.SolidColor;
    Preview.camera.backgroundColor = Color.black;
    Preview.camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
    var cameraContainer = EditorUtility.CreateGameObjectWithHideFlags(
      "Camera Container",
      HideFlags.HideAndDontSave);
    Preview.AddSingleGO(cameraContainer);
    Preview.camera.transform.SetParent(cameraContainer.transform);
    /*
    TODO: This kind of sucks...
    It feels like you should be able to restrict physics in these scenes somehow...

    Steve -
    var parameters = new SceneParameters(LocalPhysicsMode.Physics3D);
    There is an API during SceneConstruction: SceneManager.CreateScene("Name", paramaters);

    According to what I can find, this creates an isolated PhysicsScene of the type Physics3D
    and all gameobjects in this scene supposedly only interact with one another.

    I have not tested this as doing so is a bit tedious.

    You would need to bypass PreviewRenderUtility (Which I am close to being able to do
    anyway) and create your own Scene ( Preview Scene ideally ... ) which also creates this
    isolated physics world...yay.
    */
    var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
    GameObject.DestroyImmediate(ground.GetComponent<Rigidbody>());
    GameObject.DestroyImmediate(ground.GetComponent<Collider>());
    ground.hideFlags = HideFlags.HideAndDontSave;
    ground.transform.localScale = new Vector3(10, 10, 10);
    Preview.AddSingleGO(ground);
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
    Rect rct = contentRect;
    if (rct.width > 0 && rct.height > 0) {
      Preview.BeginPreview(rct, GUIStyle.none);
      Preview.RenderURP();
      Texture tex = Preview.EndPreview();
      RenderImage.image = tex;
    }

    // Cancel and cleanup any active frame behaviors.
    FrameBehavior.PreviewCancelActiveBehaviors(FrameBehaviors, Frame, Preview);
    FrameBehavior.PreviewCleanupBehaviors(FrameBehaviors, Provider);
    // Destroy old Provider
    Provider.TryDestroyImmediateGameObject();

    // Create this frame's Provider
    if (ReferenceProvider != null) {
      Provider = MonoBehaviour.Instantiate(ReferenceProvider);
      if (Provider.TryGetComponent(out AvatarAttacher avatarAttacher))
        avatarAttacher.Attach();
      Preview.AddSingleGO(Provider.gameObject);
      Provider.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
      FrameBehaviors = ReferenceFrameBehaviors.Select(fb => fb.Clone()).ToList();
      FrameBehavior.PreviewInitializeBehaviors(FrameBehaviors, Provider);
      for (var i = 0; i <= Frame; i++) {
        FrameBehavior.PreviewStartBehaviors(FrameBehaviors, i, Preview);
        FrameBehavior.PreviewUpdateBehaviors(FrameBehaviors, i, Preview);
        FrameBehavior.PreviewLateUpdateBehaviors(FrameBehaviors, i, Preview);
        FrameBehavior.PreviewEndBehaviors(FrameBehaviors, i, Preview);
      }
    }

    // Update camera last as subject's pose may have changed during behavior updates.
    Preview.camera.transform.parent.position = CameraLookAtTarget + ComputedCameraOffset;
    Preview.camera.transform.parent.LookAt(CameraLookAtTarget);
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
    this.CapturePointer(e.pointerId);
    lastPointerPosition = e.position;
    e.StopPropagation();
  }

  void OnPointerMove(PointerMoveEvent e) {
    if (this.HasPointerCapture(e.pointerId)) {
      Vector2 delta = e.position - lastPointerPosition;
      CameraRotation += RotationScalar * new Vector2(delta.y, delta.x);
      CameraRotation.x = Mathf.Clamp(CameraRotation.x, -30, 89); // avoid gimbal lock
      lastPointerPosition = e.position;
      e.StopPropagation();
    }
  }

  void OnPointerUp(PointerUpEvent e) {
    this.ReleasePointer(e.pointerId);
    e.StopPropagation();
  }
}
