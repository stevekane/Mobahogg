using UnityEngine;
using UnityEditor;
using UnityEngine.VFX;

public class VFXPreviewWindow : EditorWindow {
  PreviewRenderUtility preview;
  VisualEffect vfxInstance;
  VisualEffect vfxInstanceCloned;
  VisualEffectAsset vfxAsset;
  int frame = 0;
  int distance = 1;
  Vector3 CameraAxis = new Vector3(0,2,-2).normalized;

  [MenuItem("Window/VFX Preview")]
  public static void ShowWindow() {
    GetWindow<VFXPreviewWindow>("VFX Preview");
  }

  void OnEnable() {
    preview = new PreviewRenderUtility();
    preview.camera.fieldOfView = 45;
    preview.camera.transform.position = CameraAxis;
    preview.camera.transform.LookAt(Vector3.zero);
    preview.camera.nearClipPlane = 0.1f;
    preview.camera.farClipPlane = 1000f;
    preview.camera.clearFlags = CameraClearFlags.SolidColor;
    preview.camera.backgroundColor = Color.black;
    preview.camera.cameraType = CameraType.Preview;
  }

  void OnDisable() {
    if (preview != null) {
      preview.Cleanup();
      preview = null;
    }
    if (vfxInstance != null) {
      DestroyImmediate(vfxInstance.gameObject);
      vfxInstance = null;
    }
    if (vfxInstanceCloned != null) {
      DestroyImmediate(vfxInstanceCloned.gameObject);
      vfxInstanceCloned = null;
    }
  }

  void OnGUI() {
    EditorGUILayout.BeginVertical();
    var newAsset = (VisualEffectAsset)EditorGUILayout.ObjectField(
        "VFX Asset",
        vfxAsset,
        typeof(VisualEffectAsset),
        false);
    var assignedNewAsset = newAsset != vfxAsset;
    if (assignedNewAsset) {
      vfxAsset = newAsset;
      vfxInstance = CreateVFXInstance(vfxInstance, Vector3.left, Quaternion.Euler(0, 90, 0));
      UpdateVFX(vfxInstance);
      vfxInstanceCloned = CreateVFXInstance(vfxInstanceCloned, Vector3.right, Quaternion.Euler(0, 90, 0));
      UpdateVFX(vfxInstanceCloned);
    }
    int newFrame = EditorGUILayout.IntSlider("Frame", frame, 0, 120);
    if (newFrame != frame || assignedNewAsset) {
      frame = newFrame;
      UpdateVFX(vfxInstance);
      vfxInstanceCloned = CreateVFXInstance(vfxInstanceCloned, Vector3.right, Quaternion.Euler(0, 90, 0));
      UpdateVFX(vfxInstanceCloned);
    }
    distance = EditorGUILayout.IntSlider("Zoom", distance, 1, 10);
    preview.camera.transform.position = distance * CameraAxis;
    EditorGUILayout.EndVertical();

    Rect rct = GUILayoutUtility.GetRect(256, 256);
    preview.BeginPreview(rct, GUIStyle.none);
    preview.Render(true, true);
    Texture tex = preview.EndPreview();
    GUI.DrawTexture(rct, tex, ScaleMode.StretchToFill, false);
    Repaint();
  }

  VisualEffect CreateVFXInstance(VisualEffect instance, Vector3 position, Quaternion rotation) {
    if (instance != null) {
      DestroyImmediate(instance.gameObject);
    }
    GameObject go = new GameObject("VFXInstance");
    go.hideFlags = HideFlags.HideAndDontSave;
    instance = go.AddComponent<VisualEffect>();
    instance.visualEffectAsset = vfxAsset;
    instance.resetSeedOnPlay = false;
    preview.AddSingleGO(go);
    go.transform.SetPositionAndRotation(position, rotation);
    return instance;
  }

  void UpdateVFX(VisualEffect instance) {
    instance.pause = false;
    instance.Reinit();
    instance.Simulate(1/60f, (uint)frame);
    instance.pause = true;
  }
}
