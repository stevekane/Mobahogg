using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(PreviewWindowMaker))]
public class PreviewWindowMakerWindow : Editor {
  PreviewRenderUtility Preview;
  PreviewWindowMaker WindowMaker;
  GameObject Cube;
  GameObject Plane;
  Vector3 CameraPosition = new Vector3(0, 5, -5);

  void OnEnable() {
    WindowMaker = target as PreviewWindowMaker;
    Preview = new PreviewRenderUtility();
    Preview.camera.fieldOfView = 45;
    Preview.camera.nearClipPlane = 0.1f;
    Preview.camera.farClipPlane = 25f;
    Preview.camera.gameObject.AddComponent<UniversalAdditionalCameraData>();

    // Key Light
    var keyLight = Preview.lights[0];
    keyLight.transform.position = new Vector3(2, 3, -2);
    keyLight.transform.LookAt(Vector3.up);
    keyLight.intensity = 1.0f;
    keyLight.gameObject.AddComponent<UniversalAdditionalLightData>();
    keyLight.shadows = LightShadows.Soft;

    // Fill Light
    var fillLight = Preview.lights[1];
    fillLight.transform.position = new Vector3(-2, 2, -2);
    fillLight.transform.LookAt(Vector3.up);
    fillLight.intensity = 0.5f;
    fillLight.gameObject.AddComponent<UniversalAdditionalLightData>();

    // Back Light
    var backLight = (new GameObject()).AddComponent<Light>();
    backLight.gameObject.AddComponent<UniversalAdditionalLightData>();
    backLight.type = LightType.Directional;
    backLight.transform.position = new Vector3(0, 3, 2);
    backLight.transform.LookAt(Vector3.zero);
    backLight.intensity = 0.6f;
    backLight.color = Color.white;
    Preview.AddSingleGO(backLight.gameObject);

    Plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
    Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
    Preview.AddSingleGO(Cube);
    Preview.AddSingleGO(Plane);
  }

  void OnDisable() {
    Preview.Cleanup();
    Preview = null;
  }

  public override void OnInspectorGUI() {
    GUILayout.BeginVertical();
    // WindowMaker.Clip = (AnimationClip)EditorGUILayout.ObjectField("Clip", WindowMaker.Clip, typeof(AnimationClip), false);
    // WindowMaker.Animator = (Animator)EditorGUILayout.ObjectField("Animator Prefab", WindowMaker.Clip, typeof(Animator), false);
    WindowMaker.Material = (Material)EditorGUILayout.ObjectField("Material", WindowMaker.Material, typeof(Material), false);
    CameraPosition = EditorGUILayout.Vector3Field("Camera Position", CameraPosition);

    var rect = GUILayoutUtility.GetRect(256, 256, GUILayout.ExpandWidth(true));

    Preview.BeginPreview(rect, GUIStyle.none);
    var toModel = Vector3.up-CameraPosition;

    Preview.camera.transform.SetPositionAndRotation(CameraPosition, Quaternion.LookRotation(toModel.normalized));
    Plane.GetComponent<MeshRenderer>().sharedMaterial = WindowMaker.Material;
    Cube.GetComponent<MeshRenderer>().sharedMaterial = WindowMaker.Material;
    Preview.Render();
    var texture = Preview.EndPreview();

    GUI.DrawTexture(rect, texture, ScaleMode.StretchToFill, false);
    GUILayout.EndVertical();
    if (GUI.changed)
      EditorUtility.SetDirty(target);
  }
}