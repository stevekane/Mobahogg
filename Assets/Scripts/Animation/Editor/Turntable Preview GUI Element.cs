using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TurntablePreviewGUIElement : PreviewRenderUtility {
  public float MinZoom = 1;
  public float MaxZoom = 10;
  public float ZoomScalar = 0.25f;
  public float RotationScalar = 1;
  public float CameraFocusHeight = 1;
  public float CameraZoom = 5;
  public Vector2 CameraRotation = new(15, 90);
  public GameObject Subject;

  Vector3 CameraLookAtTarget
    => Subject
    ? Subject.transform.position + CameraFocusHeight * Vector3.up
    : CameraFocusHeight * Vector3.up;

  Vector3 CameraOffset =>
    Quaternion.Euler(CameraRotation.x, CameraRotation.y, 0) * Vector3.back * CameraZoom;

  public void SetSubject(GameObject subject) {
    if (Subject)
      Object.DestroyImmediate(Subject);
    Subject = subject;
    if (!Subject)
      return;
    AddSingleGO(Subject);
  }

  void PositionLights(Light keyLight, Light fillLight, Light rimLight) {
    // Key Light settings
    keyLight.type = LightType.Directional;
    keyLight.transform.position = new Vector3(-2, 3, 2);
    keyLight.transform.LookAt(Vector3.zero);
    keyLight.intensity = 1f;
    keyLight.color = new Color(1.0f, 0.85f, 0.73f); // Warm white (#FFDAB9)
    keyLight.shadows = LightShadows.Hard;
    keyLight.shadowCustomResolution = 2048*2;
    keyLight.shadowStrength = 0.8f;
    // Fill Light settings
    fillLight.type = LightType.Directional;
    fillLight.transform.position = new Vector3(2, 1.5f, 2);
    fillLight.transform.LookAt(Vector3.zero);
    fillLight.intensity = 0.5f;
    fillLight.color = new Color(0.69f, 0.79f, 1.0f); // Cool white (#AFCBFF)
    fillLight.shadows = LightShadows.Soft; // Optional shadows
    fillLight.shadowCustomResolution = 2048;
    // Rim Light settings
    rimLight.type = LightType.Directional;
    rimLight.transform.position = new Vector3(0, 3, -2);
    rimLight.transform.LookAt(Vector3.zero);
    rimLight.intensity = 0.35f;
    rimLight.color = new Color(0.88f, 0.94f, 1.0f); // Neutral/cool white (#E0EFFF)
    rimLight.shadows = LightShadows.Soft;
    rimLight.shadowStrength = 0.3f;
    rimLight.shadowCustomResolution = 2048;
  }

  public TurntablePreviewGUIElement(string shaderName = "Standard") : base() {
    camera.nearClipPlane = 0.1f;
    camera.farClipPlane = 1000f;
    camera.fieldOfView = 45f;
    camera.clearFlags = CameraClearFlags.Depth;
    camera.backgroundColor = Color.black;
    camera.cameraType = CameraType.Preview;
    var keyLight = lights[0];
    var fillLight = lights[1];
    var rimLight = new GameObject().AddComponent<Light>();
    var groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
    groundPlane.transform.localScale = 10 * Vector3.one;
    AddSingleGO(rimLight.gameObject);
    AddSingleGO(groundPlane);
    PositionLights(keyLight, fillLight, rimLight);
  }

  public void Update(Rect rect, Event e) {
    if (rect.Contains(e.mousePosition) && e.type == EventType.ScrollWheel) {
      CameraZoom += ZoomScalar * e.delta.y;
      CameraZoom = Mathf.Clamp(CameraZoom, MinZoom, MaxZoom);
      e.Use();
    }
    if (rect.Contains(e.mousePosition) && e.type == EventType.MouseDrag && e.button == 2) {
      CameraRotation += RotationScalar * new Vector2(e.delta.y, e.delta.x);
      CameraRotation.x = Mathf.Clamp(CameraRotation.x, -30, 90);
      e.Use();
    }
    camera.transform.position = CameraLookAtTarget + CameraOffset;
    camera.transform.LookAt(CameraLookAtTarget);
    BeginPreview(rect, GUIStyle.none);
    Render(allowScriptableRenderPipeline: true);
    GUI.DrawTexture(rect, EndPreview(), ScaleMode.StretchToFill, false);
  }
}