using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CameraPreviewRenderer))]
public class CameraPreviewRendererEditor : Editor {
  private RenderTexture previewTexture;
  private const int previewWidth = 256;
  private const int previewHeight = 256;

  void OnEnable() {
    previewTexture = new RenderTexture(previewWidth, previewHeight, 16, RenderTextureFormat.ARGB32);
    previewTexture.Create();
  }

  void OnDisable() {
    if (previewTexture != null) {
      previewTexture.Release();
      DestroyImmediate(previewTexture);
    }
  }

  public override void OnInspectorGUI() {
    DrawDefaultInspector();

    CameraPreviewRenderer comp = (CameraPreviewRenderer)target;
    if (comp.previewCamera != null) {
      // Set the camera to render into our render texture.
      comp.previewCamera.targetTexture = previewTexture;
      comp.previewCamera.Render();
      comp.previewCamera.targetTexture = null;

      GUILayout.Space(10);
      GUILayout.Label("Camera Preview:", EditorStyles.boldLabel);
      Rect rect = GUILayoutUtility.GetRect(previewWidth, previewHeight, GUILayout.ExpandWidth(false));
      EditorGUI.DrawPreviewTexture(rect, previewTexture);
      Repaint();
    }
    else {
      EditorGUILayout.HelpBox("Assign a Camera to preview.", MessageType.Info);
    }
  }
}