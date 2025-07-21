using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class SDFRenderPass : ScriptableRenderPass
{
  public Material DepthMaterial;
  public Material RenderingMaterial;

  static class ShaderIDs
  {
    public static readonly int _SDFMaskTexture = Shader.PropertyToID("_SDFMaskTexture");
    public static readonly int _SDFDepthTexture = Shader.PropertyToID("_SDFDepthTexture");
  }

  public class SDFDepthMaskPassData
  {
    public Material DepthMaskMaterial;
  }

  public class SDFRenderPassData
  {
    public Material RenderingMaterial;
    public TextureHandle _SDFDepthTexture;
    public TextureHandle _SDFMaskTexture;
  }

  public SDFRenderPass()
  {
    renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
  }

  static void ExecuteDepthMaskPass(SDFDepthMaskPassData data, RasterGraphContext ctx)
  {
    Blitter.BlitTexture(ctx.cmd, Vector4.one, data.DepthMaskMaterial, 0);
  }

  static void ExecuteRenderPass(SDFRenderPassData data, RasterGraphContext ctx)
  {
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture(ShaderIDs._SDFMaskTexture, data._SDFMaskTexture);
    propertyBlock.SetTexture(ShaderIDs._SDFDepthTexture, data._SDFDepthTexture);
    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.RenderingMaterial, 0, MeshTopology.Triangles, 3, 1, propertyBlock);
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    var primaryDepthTexture = resourceData.cameraDepth;
    var primaryNormalsTexture = resourceData.cameraNormalsTexture;

    var SDFDepthTextureDescription = renderGraph.GetTextureDesc(primaryDepthTexture);
    SDFDepthTextureDescription.name = "SDF Depth";
    SDFDepthTextureDescription.clearBuffer = true;
    SDFDepthTextureDescription.clearColor = Color.white;

    var SDFMaskTextureDescription = renderGraph.GetTextureDesc(primaryNormalsTexture);
    SDFMaskTextureDescription.name = "SDF Mask";
    SDFMaskTextureDescription.clearBuffer = true;
    SDFMaskTextureDescription.clearColor = Color.black;
    SDFMaskTextureDescription.colorFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.R8_SNorm;

    var SDFDepthTexture = renderGraph.CreateTexture(SDFDepthTextureDescription);
    var SDFMaskTexture = renderGraph.CreateTexture(SDFMaskTextureDescription);

    using (var builder = renderGraph.AddRasterRenderPass<SDFDepthMaskPassData>("SDF Depth/Mask", out var passData))
    {
      passData.DepthMaskMaterial = DepthMaterial;
      builder.AllowPassCulling(false);
      builder.SetRenderAttachment(SDFMaskTexture, 0, AccessFlags.Write);
      builder.SetRenderAttachmentDepth(SDFDepthTexture, AccessFlags.Write);
      builder.SetRenderFunc<SDFDepthMaskPassData>(ExecuteDepthMaskPass);
    }

    using (var builder = renderGraph.AddRasterRenderPass<SDFRenderPassData>("SDF Render", out var passData))
    {
      passData.RenderingMaterial = RenderingMaterial;
      passData._SDFDepthTexture = SDFDepthTexture;
      passData._SDFMaskTexture = SDFMaskTexture;
      builder.AllowPassCulling(false);
      builder.UseTexture(SDFDepthTexture, AccessFlags.Read);
      builder.UseTexture(SDFMaskTexture, AccessFlags.Read);
      builder.SetRenderAttachment(resourceData.cameraColor, 0, AccessFlags.Write);
      builder.SetRenderAttachmentDepth(resourceData.cameraDepth, AccessFlags.Write);
      builder.SetRenderFunc<SDFRenderPassData>(ExecuteRenderPass);
    }
  }
}