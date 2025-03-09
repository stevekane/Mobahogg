using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.Universal;

[CustomEditor(typeof(CameraPreviewRenderer))]
public class CameraPreviewRendererEditor : Editor {
  private RenderTexture previewTexture;
  private const int previewWidth = 512;
  private const int previewHeight = 512;

  void OnEnable() {
    // Formatting here is important if you want HDR-based effects like Bloom
    GraphicsFormat colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
    previewTexture = new RenderTexture(
      previewWidth,
      previewHeight,
      colorFormat,
      SystemInfo.GetGraphicsFormat(DefaultFormat.DepthStencil));
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
      UniversalRenderPipeline.SingleCameraRequest request = new() {
        destination = previewTexture
      };
      RenderPipeline.SubmitRenderRequest(comp.previewCamera, request);
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