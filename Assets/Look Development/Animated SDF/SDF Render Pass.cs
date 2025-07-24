using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

public class SDFRenderPass : ScriptableRenderPass
{
  public Material DepthMaterial;
  public Material RenderingMaterial;
  public ComputeShader SDFScreenSpaceComputeShader;

  static class ShaderIDs
  {
    public static readonly int _SDFMaskTexture = Shader.PropertyToID("_SDFMaskTexture");
    public static readonly int _SDFDepthTexture = Shader.PropertyToID("_SDFDepthTexture");
    public static readonly int _ColorTexture = Shader.PropertyToID("_ColorTexture");
  }

  public class SDFDepthMaskPassData
  {
    public Material DepthMaskMaterial;
  }

  public class SDFScreenSpacePassData
  {
    public ComputeShader ComputeShader;
    public TextureHandle PingTexture;
    public TextureHandle PongTexture;
    public TextureHandle SDFMaskTexture;
    public TextureHandle SDFScreenSpaceTexture;
    public int Width;
    public int Height;
  }

  public class SDFRenderPassData
  {
    public Material RenderingMaterial;
    public TextureHandle _SDFDepthTexture;
    public TextureHandle _SDFMaskTexture;
    public TextureHandle _ColorTexture;
  }

  public SDFRenderPass()
  {
    // We read the color buffer to do gravitational distortion and therefore must have the renderGraph
    // generate an intermediate texture for us since that is not allowed
    requiresIntermediateTexture = true;
    renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
  }

  static void ExecuteDepthMaskPass(SDFDepthMaskPassData data, RasterGraphContext ctx)
  {
    Blitter.BlitTexture(ctx.cmd, Vector4.one, data.DepthMaskMaterial, 0);
  }

  static void SetComputeTextureParamsForAllKernels(
  ComputeCommandBuffer cmd,
  ComputeShader shader,
  string name,
  TextureHandle textureHandle)
  {
    var kernelInit = shader.FindKernel("JumpFloodInit");
    var kernelStep = shader.FindKernel("JumpFloodStep");
    var kernelFinalize = shader.FindKernel("JumpFloodFinalize");
    cmd.SetComputeTextureParam(shader, kernelInit, name, textureHandle);
    cmd.SetComputeTextureParam(shader, kernelStep, name, textureHandle);
    cmd.SetComputeTextureParam(shader, kernelFinalize, name, textureHandle);
  }

  // TODO: All the shader property names could probably be turned into Ints here as we have done
  // elsewhere in this file
  static void ExecuteScreenSpaceSDFPass(SDFScreenSpacePassData data, ComputeGraphContext ctx)
  {
    var cmd = ctx.cmd;
    var maxStep = Mathf.CeilToInt(Mathf.Log(Mathf.Max(data.Width, data.Height), 2));
    var kernelInit = data.ComputeShader.FindKernel("JumpFloodInit");
    var kernelStep = data.ComputeShader.FindKernel("JumpFloodStep");
    var kernelFinalize = data.ComputeShader.FindKernel("JumpFloodFinalize");
    var pingTexture = data.PingTexture;
    var pongTexture = data.PongTexture;
    var threadGroupsX = Mathf.CeilToInt(data.Width / 8f);
    var threadGroupsY = Mathf.CeilToInt(data.Height / 8f);
    const int threadGroupsZ = 1;
    cmd.SetComputeIntParam(data.ComputeShader, "_TexWidth", data.Width);
    cmd.SetComputeIntParam(data.ComputeShader, "_TexHeight", data.Height);
    // Must set names for every kernel apparently..
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, "_Mask", data.SDFMaskTexture);
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, "_JumpFloodDistance", data.SDFScreenSpaceTexture);
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, "_JumpFloodPing", data.PingTexture);
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, "_JumpFloodPong", data.PongTexture);
    cmd.DispatchCompute(
      data.ComputeShader,
      kernelInit,
      threadGroupsX,
      threadGroupsY,
      threadGroupsZ);

    for (int i = maxStep; i > 0; i /= 2)
    {
      cmd.SetComputeIntParam(data.ComputeShader, "_Step", i);
      cmd.SetComputeTextureParam(data.ComputeShader, kernelStep, "_JumpFloodPing", pingTexture);
      cmd.SetComputeTextureParam(data.ComputeShader, kernelStep, "_JumpFloodPong", pongTexture);
      cmd.DispatchCompute(
        data.ComputeShader,
        kernelStep,
        threadGroupsX,
        threadGroupsY,
        threadGroupsZ);
      (pingTexture, pongTexture) = (pongTexture, pingTexture);
    }
    cmd.DispatchCompute(
      data.ComputeShader,
      kernelFinalize,
      threadGroupsX,
      threadGroupsY,
      threadGroupsZ);
  }

  static void ExecuteRenderPass(SDFRenderPassData data, RasterGraphContext ctx)
  {
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture(ShaderIDs._SDFMaskTexture, data._SDFMaskTexture);
    propertyBlock.SetTexture(ShaderIDs._SDFDepthTexture, data._SDFDepthTexture);
    propertyBlock.SetTexture(ShaderIDs._ColorTexture, data._ColorTexture);
    ctx.cmd.DrawProcedural(Matrix4x4.identity, data.RenderingMaterial, 0, MeshTopology.Triangles, 3, 1, propertyBlock);
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    var SDFDepthTextureDescription = renderGraph.GetTextureDesc(resourceData.cameraDepth);
    SDFDepthTextureDescription.name = "SDF Depth";
    SDFDepthTextureDescription.clearBuffer = true;
    SDFDepthTextureDescription.clearColor = Color.white;

    var SDFMaskTextureDescription = renderGraph.GetTextureDesc(resourceData.cameraNormalsTexture);
    SDFMaskTextureDescription.name = "SDF Mask";
    SDFMaskTextureDescription.clearBuffer = true;
    SDFMaskTextureDescription.clearColor = Color.black;
    SDFMaskTextureDescription.colorFormat = GraphicsFormat.R8_SNorm;

    var ColorTextureDescription = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    ColorTextureDescription.name = "Camera Color copy For SDF Render Pass";

    var pingDesc = SDFMaskTextureDescription;
    pingDesc.name = "JumpFlood Ping";
    pingDesc.colorFormat = GraphicsFormat.R32G32_SFloat;
    pingDesc.enableRandomWrite = true;

    var pongDesc = pingDesc;
    pongDesc.name = "JumpFlood Pong";
    pongDesc.enableRandomWrite = true;

    var distDesc = SDFMaskTextureDescription;
    distDesc.name = "JumpFlood Distance";
    distDesc.colorFormat = GraphicsFormat.R32_SFloat;
    distDesc.enableRandomWrite = true;

    var SDFDepthTexture = renderGraph.CreateTexture(SDFDepthTextureDescription);
    var SDFMaskTexture = renderGraph.CreateTexture(SDFMaskTextureDescription);
    var ColorTexture = renderGraph.CreateTexture(ColorTextureDescription);
    var PingTexture = renderGraph.CreateTexture(pingDesc);
    var PongTexture = renderGraph.CreateTexture(pongDesc);
    var SDFScreenSpaceTexture = renderGraph.CreateTexture(distDesc);

    using (var builder = renderGraph.AddRasterRenderPass<SDFDepthMaskPassData>("SDF Depth/Mask", out var passData))
    {
      passData.DepthMaskMaterial = DepthMaterial;
      builder.SetRenderAttachment(SDFMaskTexture, 0);
      builder.SetRenderAttachmentDepth(SDFDepthTexture);
      builder.SetRenderFunc<SDFDepthMaskPassData>(ExecuteDepthMaskPass);
    }

    renderGraph.AddCopyPass(
      source: resourceData.activeColorTexture,
      destination: ColorTexture);

    using (var builder = renderGraph.AddComputePass<SDFScreenSpacePassData>("SDF Screenspace", out var passData))
    {
      passData.ComputeShader = SDFScreenSpaceComputeShader;
      passData.PingTexture = PingTexture;
      passData.PongTexture = PongTexture;
      passData.SDFMaskTexture = SDFMaskTexture;
      passData.SDFScreenSpaceTexture = SDFScreenSpaceTexture;
      passData.Width = SDFMaskTextureDescription.width;
      passData.Height = SDFMaskTextureDescription.height;
      builder.UseTexture(SDFMaskTexture, AccessFlags.Read);
      builder.UseTexture(PingTexture, AccessFlags.ReadWrite);
      builder.UseTexture(PongTexture, AccessFlags.ReadWrite);
      builder.UseTexture(SDFScreenSpaceTexture, AccessFlags.ReadWrite);
      builder.AllowPassCulling(false);
      builder.SetRenderFunc<SDFScreenSpacePassData>(ExecuteScreenSpaceSDFPass);
    }

    using (var builder = renderGraph.AddRasterRenderPass<SDFRenderPassData>("SDF Render", out var passData))
    {
      passData.RenderingMaterial = RenderingMaterial;
      passData._SDFDepthTexture = SDFDepthTexture;
      passData._SDFMaskTexture = SDFMaskTexture;
      passData._ColorTexture = ColorTexture;
      builder.UseTexture(SDFDepthTexture);
      builder.UseTexture(SDFMaskTexture);
      builder.UseTexture(ColorTexture);
      builder.SetRenderAttachment(resourceData.cameraColor, 0);
      builder.SetRenderAttachmentDepth(resourceData.cameraDepth);
      builder.SetRenderFunc<SDFRenderPassData>(ExecuteRenderPass);
    }
  }
}