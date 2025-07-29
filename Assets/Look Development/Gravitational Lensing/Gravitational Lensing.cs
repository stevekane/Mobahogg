using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class GravitationalLensing : MonoBehaviour
{
  [SerializeField] Material Material;
  [SerializeField] RenderPassEvent RenderPassEvent;

  GravitationalLensingRenderPass RenderPass;

  void OnEnable()
  {
    RenderPass = new();
    RenderPipelineManager.beginCameraRendering += InjectRenderPass;
  }

  void OnDisable()
  {
    RenderPass = null;
    RenderPipelineManager.beginCameraRendering -= InjectRenderPass;
  }

  void InjectRenderPass(ScriptableRenderContext ctx, Camera camera) {
    if (RenderPass != null)
    {
      RenderPass.Material = Material;
      RenderPass.requiresIntermediateTexture = true;
      RenderPass.renderPassEvent = RenderPassEvent;
      camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(RenderPass);
    }
  }
}

class GravitationalLensingRenderPass : ScriptableRenderPass {
  public Material Material;

  class PassData {
    public Material Material;
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    var description = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    var colorTexture = renderGraph.CreateTexture(description);
    using (var builder = renderGraph.AddRasterRenderPass<PassData>("Gravitational Lensing", out var passData))
    {
      passData.Material = Material;
      builder.UseTexture(resourceData.activeColorTexture);
      builder.SetRenderAttachment(colorTexture, 0);
      builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
      {
        ctx.cmd.DrawProcedural(
          Matrix4x4.identity,
          data.Material,
          0,
          MeshTopology.Triangles,
          3,
          1);
      });
    }
    renderGraph.AddCopyPass(source: colorTexture, destination: resourceData.activeColorTexture);
  }
}