using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class PreviewRenderUtilityExtensions {
  public static void RenderURP(this PreviewRenderUtility preview) {
    if (!EditorApplication.isUpdating && Unsupported.SetOverrideLightingSettings(preview.camera.scene)) {
      RenderSettings.ambientMode = AmbientMode.Flat;
      RenderSettings.ambientLight = default;
    }
    preview.lights.ForEach(l => l.enabled = true);
    bool useScriptableRenderPipeline = Unsupported.useScriptableRenderPipeline;
    Unsupported.useScriptableRenderPipeline = true;
    UniversalRenderPipeline.SingleCameraRequest request = new() {
      destination = preview.camera.targetTexture
    };
    RenderPipeline.SubmitRenderRequest(preview.camera, request);
    Unsupported.useScriptableRenderPipeline = useScriptableRenderPipeline;
  }
}