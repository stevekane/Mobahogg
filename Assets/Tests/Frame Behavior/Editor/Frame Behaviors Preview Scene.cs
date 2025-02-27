using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using System.Collections.Generic;
using System.Linq;

class FrameBehaviorsPreviewScene : VisualElement {
  PreviewRenderUtility Preview;
  Image m_Image;
  GameObject Ground;
  List<FrameBehavior> FrameBehaviors = new List<FrameBehavior>();
  MonoBehaviour Provider;
  int Frame;
  Vector3 CameraOffset = new(3, 3, 3);

  public FrameBehaviorsPreviewScene() {
    style.minHeight = 256;
    style.maxHeight = 1024;
    m_Image = new Image();
    Add(m_Image);
    RegisterCallback<AttachToPanelEvent>(OnAttach);
    RegisterCallback<DetachFromPanelEvent>(OnDetach);
  }

  void OnAttach(AttachToPanelEvent evt) {
    BuildPreview();
  }

  void OnDetach(DetachFromPanelEvent evt) {
    FrameBehavior.PreviewCancelActiveBehaviors(FrameBehaviors, Frame, Preview);
    if (Preview != null) {
      Preview.Cleanup();
    }
  }

  // if any of the inputs have changed ( by virtue of this being called, then rebuild everything )
  public void Seek(int targetFrame, IEnumerable<FrameBehavior> frameBehaviors, MonoBehaviour provider) {
    BuildPreview();
    if (provider != null) {
      Provider = MonoBehaviour.Instantiate(provider);
      Preview.AddSingleGO(Provider.gameObject);
    }
    FrameBehavior.PreviewCancelActiveBehaviors(FrameBehaviors, Frame, Preview);
    FrameBehaviors = frameBehaviors.Select(fb => fb.Clone()).ToList();
    FrameBehavior.PreviewInitializeBehaviors(FrameBehaviors, Provider);
    Frame = 0;
    for (var i = 0; i <= targetFrame; i++) {
      Frame = i;
      Run();
    }
    var cameraTarget = Provider ? Provider.transform.position : Vector3.zero;
    Preview.camera.transform.position = cameraTarget + CameraOffset;
    Preview.camera.transform.LookAt(cameraTarget + Vector3.up);
    Rect rct = contentRect;
    if (rct.width > 0 && rct.height > 0) {
      Preview.BeginPreview(rct, GUIStyle.none);
      Preview.Render(true);
      Texture tex = Preview.EndPreview();
      m_Image.image = tex;
      m_Image.MarkDirtyRepaint();
    }
  }

  public void Advance() {
    Frame++;
    Run();
  }

  void Run() {
    FrameBehavior.PreviewStartBehaviors(FrameBehaviors, Frame, Preview);
    FrameBehavior.PreviewUpdateBehaviors(FrameBehaviors, Frame, Preview);
    FrameBehavior.PreviewEndBehaviors(FrameBehaviors, Frame, Preview);
  }

  void BuildPreview() {
    if (Preview != null) {
      Preview.Cleanup();
    }
    Preview = new PreviewRenderUtility();
    Preview.camera.nearClipPlane = 0.1f;
    Preview.camera.farClipPlane = 1000f;
    Preview.camera.fieldOfView = 45f;
    Preview.camera.clearFlags = CameraClearFlags.Depth;
    Preview.camera.backgroundColor = Color.black;
    Preview.camera.cameraType = CameraType.Preview;
    Preview.camera.transform.position = new(3, 3, 3);
    Ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
    Ground.hideFlags = HideFlags.HideAndDontSave;
    Ground.transform.position = Vector3.zero;
    Preview.AddSingleGO(Ground);
    var keyLight = Preview.lights[0];
    keyLight.type = LightType.Directional;
    keyLight.transform.position = new Vector3(-2, 3, 2);
    keyLight.transform.LookAt(Vector3.zero);
    keyLight.intensity = 1f;
    keyLight.color = new Color(1.0f, 0.85f, 0.73f); // Warm white (#FFDAB9)
    keyLight.shadows = LightShadows.Hard;
    keyLight.shadowCustomResolution = 2048*2;
    keyLight.shadowStrength = 0.8f;
    var rimLight = Preview.lights[1];
    rimLight.type = LightType.Directional;
    rimLight.transform.position = new Vector3(0, 3, -2);
    rimLight.transform.LookAt(Vector3.up);
    rimLight.intensity = 0.35f;
    rimLight.color = new Color(0.88f, 0.94f, 1.0f); // Neutral/cool white (#E0EFFF)
    rimLight.shadows = LightShadows.Soft;
    rimLight.shadowStrength = 0.3f;
    rimLight.shadowCustomResolution = 2048;
  }
}