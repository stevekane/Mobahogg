using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.Universal;

public class EuclideanDistanceTransformationComputePass : ScriptableRenderPass
{
  static readonly string InitKernelName = "JumpFloodInit";
  static readonly string StepKernelName = "JumpFloodStep";
  static readonly string FinalizeKernelName = "JumpFloodFinalize";

  static readonly string PingTextureName = "EDT Ping";
  static readonly string PongTextureName = "EDT Pong";
  static readonly string DistanceTextureName = "EDT Distance";

  static readonly int PingTextureID = Shader.PropertyToID("_JumpFloodPing");
  static readonly int PongTextureID = Shader.PropertyToID("_JumpFloodPong");
  static readonly int DistanceTextureID = Shader.PropertyToID("_JumpFloodDistance");
  static readonly int MaskTextureID = Shader.PropertyToID("_Mask");
  static readonly int TextureWidthPropertyID = Shader.PropertyToID("_TexWidth");
  static readonly int TextureHeightPropertyID = Shader.PropertyToID("_TexHeight");
  static readonly int StepPropertyID = Shader.PropertyToID("_Step");

  static readonly string ComputePassName = "Euclidean Distance Transformation";

  public ComputeShader ComputeShader;
  public TextureHandle InputMaskTextureHandle;
  public TextureHandle OutputEDTTextureHandle;

  class PassData
  {
    public ComputeShader ComputeShader;
    public TextureHandle MaskTexture;
    public TextureHandle PingTexture;
    public TextureHandle PongTexture;
    public TextureHandle EDTTexture;
    public int Width;
    public int Height;
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var pingDesc = renderGraph.GetTextureDesc(InputMaskTextureHandle);
    pingDesc.name = PingTextureName;
    pingDesc.colorFormat = GraphicsFormat.R32G32_SFloat;
    pingDesc.enableRandomWrite = true;

    var pongDesc = pingDesc;
    pongDesc.name = PongTextureName;
    pongDesc.enableRandomWrite = true;

    var distDesc = pingDesc;
    distDesc.name = DistanceTextureName;
    distDesc.colorFormat = GraphicsFormat.R32_SFloat;
    distDesc.enableRandomWrite = true;

    var PingTexture = renderGraph.CreateTexture(pingDesc);
    var PongTexture = renderGraph.CreateTexture(pongDesc);
    var DistTexture = renderGraph.CreateTexture(distDesc);

    using (var builder = renderGraph.AddComputePass<PassData>(ComputePassName, out var passData))
    {
      passData.ComputeShader = ComputeShader;
      passData.MaskTexture = InputMaskTextureHandle;
      passData.PingTexture = PingTexture;
      passData.PongTexture = PongTexture;
      passData.EDTTexture = OutputEDTTextureHandle;
      passData.Width = distDesc.width;
      passData.Height = distDesc.height;
      builder.UseTexture(passData.MaskTexture, AccessFlags.ReadWrite);
      builder.UseTexture(passData.PingTexture, AccessFlags.ReadWrite);
      builder.UseTexture(passData.PongTexture, AccessFlags.ReadWrite);
      builder.UseTexture(passData.EDTTexture, AccessFlags.ReadWrite);
      builder.AllowPassCulling(false);
      builder.SetRenderFunc<PassData>(ExecuteComputePass);
    }

    OutputEDTTextureHandle = DistTexture;
  }

  static void SetComputeTextureParamsForAllKernels(
  ComputeCommandBuffer cmd,
  ComputeShader shader,
  int nameID,
  TextureHandle textureHandle)
  {
    var kernelInit = shader.FindKernel(InitKernelName);
    var kernelStep = shader.FindKernel(StepKernelName);
    var kernelFinalize = shader.FindKernel(FinalizeKernelName);
    cmd.SetComputeTextureParam(shader, kernelInit, nameID, textureHandle);
    cmd.SetComputeTextureParam(shader, kernelStep, nameID, textureHandle);
    cmd.SetComputeTextureParam(shader, kernelFinalize, nameID, textureHandle);
  }

  static void ExecuteComputePass(PassData data, ComputeGraphContext ctx)
  {
    var cmd = ctx.cmd;
    var kernelInit = data.ComputeShader.FindKernel(InitKernelName);
    var kernelStep = data.ComputeShader.FindKernel(StepKernelName);
    var kernelFinalize = data.ComputeShader.FindKernel(FinalizeKernelName);
    var pingTexture = data.PingTexture;
    var pongTexture = data.PongTexture;
    var threadGroupsX = Mathf.CeilToInt(data.Width / 8f);
    var threadGroupsY = Mathf.CeilToInt(data.Height / 8f);
    const int threadGroupsZ = 1;
    cmd.SetComputeIntParam(data.ComputeShader, TextureWidthPropertyID, data.Width);
    cmd.SetComputeIntParam(data.ComputeShader, TextureHeightPropertyID, data.Height);
    // Must set names for every kernel apparently..
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, MaskTextureID, data.MaskTexture);
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, DistanceTextureID, data.EDTTexture);
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, PingTextureID, data.PingTexture);
    SetComputeTextureParamsForAllKernels(cmd, data.ComputeShader, PongTextureID, data.PongTexture);
    cmd.DispatchCompute(
      data.ComputeShader,
      kernelInit,
      threadGroupsX,
      threadGroupsY,
      threadGroupsZ);

    var maxDimension = Mathf.Max(data.Width, data.Height);
    var maxStep = Mathf.NextPowerOfTwo(maxDimension) / 2;
    for (int i = maxStep; i > 0; i /= 2)
    {
      cmd.SetComputeIntParam(data.ComputeShader, StepPropertyID, i);
      cmd.SetComputeTextureParam(data.ComputeShader, kernelStep, PingTextureID, pingTexture);
      cmd.SetComputeTextureParam(data.ComputeShader, kernelStep, PongTextureID, pongTexture);
      cmd.DispatchCompute(
        data.ComputeShader,
        kernelStep,
        threadGroupsX,
        threadGroupsY,
        threadGroupsZ);
      (pingTexture, pongTexture) = (pongTexture, pingTexture);
    }
    cmd.SetComputeTextureParam(data.ComputeShader, kernelFinalize, PingTextureID, pingTexture);
    cmd.DispatchCompute(
      data.ComputeShader,
      kernelFinalize,
      threadGroupsX,
      threadGroupsY,
      threadGroupsZ);
  }
}