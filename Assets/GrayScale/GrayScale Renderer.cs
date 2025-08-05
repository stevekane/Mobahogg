using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
public class GrayScaleRenderer : MonoBehaviour {
  [SerializeField] Material Material;

  GrayScaleRenderPass RenderPass;

  void OnEnable()
  {
    RenderPass = new()
    {
      renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing,
      requiresIntermediateTexture = true,
      Material = Material
    };
    RenderPipelineManager.beginCameraRendering += InjectRenderPass;
  }

  void OnDisable()
  {
    RenderPass = null;
    RenderPipelineManager.beginCameraRendering -= InjectRenderPass;
  }

  void InjectRenderPass(ScriptableRenderContext ctx, Camera camera)
  {
    var isPreview = camera.cameraType == CameraType.Preview;
    var isReflection = camera.cameraType == CameraType.Reflection;
    if (isPreview || isReflection) return;
    RenderPass.Material = Material;
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(RenderPass);
  }
}

class GrayScaleRenderPass : ScriptableRenderPass
{
  // This name is important as it is what URP expects you to use to bind textures that can be
  // sampled in shader graphs using the URP buffer node ( with target blit )
  static int BlitTexturePropertyID = Shader.PropertyToID("_BlitTexture");

  public Material Material;

  class PassData
  {
    public Material Material;
    public TextureHandle Source;
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    var description = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    var colorTexture = renderGraph.CreateTexture(description);
    renderGraph.AddCopyPass(
      source: resourceData.activeColorTexture,
      destination: colorTexture,
      passName: "GrayScale Copy");
    using (var builder = renderGraph.AddRasterRenderPass<PassData>("GrayScale Render", out var passData))
    {
      passData.Material = Material;
      passData.Source = colorTexture;
      builder.UseTexture(colorTexture);
      builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
      builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
      {
        var propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetTexture(BlitTexturePropertyID, data.Source);
        ctx.cmd.DrawProcedural(
          Matrix4x4.identity,
          data.Material,
          shaderPass: 0,
          MeshTopology.Triangles,
          vertexCount: 3,
          instanceCount: 1,
          propertyBlock);
      });
    }
  }
}