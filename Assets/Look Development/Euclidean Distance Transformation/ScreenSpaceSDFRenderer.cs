using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

[ExecuteAlways]
public class ScreenSpaceSDFRenderer : MonoBehaviour
{
  [SerializeField] Material Material;

  ScreenSpaceSDFRenderPass RenderPass;

  void OnEnable()
  {
    RenderPass = new()
    {
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
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(RenderPass);
  }
}

class ScreenSpaceSDFRenderPass : ScriptableRenderPass
{
  public Material Material;

  public ScreenSpaceSDFRenderPass()
  {
    renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
  }

  class InitPassData
  {
    public Material Material;
  }

  class StepPassData
  {
    public int Step;
    public Vector2 ScreenSize;
    public Material Material;
    public TextureHandle Source;
  }

  static void RunInit(InitPassData passData, RasterGraphContext ctx)
  {
    ctx.cmd.DrawProcedural(
      matrix: Matrix4x4.identity,
      material: passData.Material,
      shaderPass: 0,
      topology: MeshTopology.Triangles,
      vertexCount: 3,
      instanceCount: 1);
  }

  static void RunStep(StepPassData passData, RasterGraphContext ctx)
  {
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetFloat("_Step", passData.Step);
    propertyBlock.SetTexture("_Source", passData.Source);
    propertyBlock.SetVector("_ScreenSize", passData.ScreenSize);
    ctx.cmd.DrawProcedural(
      matrix: Matrix4x4.identity,
      material: passData.Material,
      shaderPass: 1,
      topology: MeshTopology.Triangles,
      vertexCount: 3,
      instanceCount: 1,
      propertyBlock);
  }

  public override void RecordRenderGraph(
  RenderGraph renderGraph,
  ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    var pingTextureDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    pingTextureDesc.name = "ping";
    // TODO: Not certain about the formats here. It probably ultimately makes sense to store floats for these values
    // and only need to be 2-component.
    pingTextureDesc.format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_UInt;
    pingTextureDesc.wrapMode = TextureWrapMode.Clamp;
    pingTextureDesc.filterMode = FilterMode.Point;
    var pongTextureDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    pongTextureDesc.name = "pong";
    pongTextureDesc.format = UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_UInt;
    pongTextureDesc.wrapMode = TextureWrapMode.Clamp;
    pongTextureDesc.filterMode = FilterMode.Point;
    var pingTexture = renderGraph.CreateTexture(pingTextureDesc);
    var pongTexture = renderGraph.CreateTexture(pongTextureDesc);
    using (var builder = renderGraph.AddRasterRenderPass<InitPassData>("ScreenSpaceSDF Init", out var passData))
    {
      passData.Material = Material;
      builder.SetRenderAttachment(pingTexture, 0);
      builder.SetRenderFunc<InitPassData>(RunInit);
    }

    var maxDimension = Mathf.Max(pingTextureDesc.width, pingTextureDesc.height);
    var maxStep = Mathf.NextPowerOfTwo(maxDimension);
    var screenSize = new Vector2(pingTextureDesc.width, pingTextureDesc.height);
    for (var step = maxStep; step >= 1; step /= 2)
    {
      using (var builder = renderGraph.AddRasterRenderPass<StepPassData>("ScreenSpaceSDF Steps ", out var passData))
      {
        passData.Material = Material;
        passData.Step = step;
        passData.Source = pingTexture;
        passData.ScreenSize = screenSize;
        builder.UseTexture(pingTexture);
        builder.SetRenderAttachment(pongTexture, 0);
        builder.SetRenderFunc<StepPassData>(RunStep);
      }
      (pingTexture, pongTexture) = (pongTexture, pingTexture);
    }

    renderGraph.AddCopyPass(pingTexture, resourceData.activeColorTexture);
  }
}