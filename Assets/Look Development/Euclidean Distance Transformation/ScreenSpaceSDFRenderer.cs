using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;

[ExecuteAlways]
public class ScreenSpaceSDFRenderer : MonoBehaviour
{
  [SerializeField] Material Material;
  [SerializeField] Texture2D MaskTexture;

  ScreenSpaceSDFRenderPass RenderPass;

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

  void InjectRenderPass(ScriptableRenderContext ctx, Camera camera)
  {
    RenderPass.Material = Material;
    RenderPass.Mask = MaskTexture;
    RenderPass.renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(RenderPass);
  }
}

public static class ScreenSpaceSDFRenderUtils {
  class InitPassData
  {
    public Material Material;
    public TextureHandle Mask;
  }

  class StepPassData
  {
    public Material Material;
    public TextureHandle Source;
    public TextureHandle Mask;
    public Vector2 ScreenSize;
    public int Step;
  }

  static int StepPropertyID = Shader.PropertyToID("_Step");
  static int SourcePropertyID = Shader.PropertyToID("_Source");
  static int MaskPropertyID = Shader.PropertyToID("_Mask");
  static int ScreenSizePropertyID = Shader.PropertyToID("_ScreenSize");

  static void RunInit(InitPassData passData, RasterGraphContext ctx)
  {
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture(MaskPropertyID, passData.Mask);
    ctx.cmd.DrawProcedural(
      matrix: Matrix4x4.identity,
      material: passData.Material,
      shaderPass: 0,
      topology: MeshTopology.Triangles,
      vertexCount: 3,
      instanceCount: 1,
      propertyBlock);
  }

  static void RunStep(StepPassData passData, RasterGraphContext ctx)
  {
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture(MaskPropertyID, passData.Mask);
    propertyBlock.SetTexture(SourcePropertyID, passData.Source);
    propertyBlock.SetVector(ScreenSizePropertyID, passData.ScreenSize);
    propertyBlock.SetFloat(StepPropertyID, passData.Step);
    ctx.cmd.DrawProcedural(
      matrix: Matrix4x4.identity,
      material: passData.Material,
      shaderPass: 1,
      topology: MeshTopology.Triangles,
      vertexCount: 3,
      instanceCount: 1,
      propertyBlock);
  }


  public static TextureHandle ScreenSpaceSDFRenderPass(
  RenderGraph renderGraph,
  ContextContainer contextContainer,
  TextureHandle mask,
  Material material,
  string namePrefix = "")
  {
    var resourceData = contextContainer.Get<UniversalResourceData>();
    var pingTextureDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    pingTextureDesc.name = $"{namePrefix}_Ping";
    pingTextureDesc.format = GraphicsFormat.R16G16_SFloat;
    pingTextureDesc.wrapMode = TextureWrapMode.Clamp;
    pingTextureDesc.filterMode = FilterMode.Point;
    var pongTextureDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    pongTextureDesc.name = $"{namePrefix}_Pong";
    pongTextureDesc.format = GraphicsFormat.R16G16_SFloat;
    pongTextureDesc.wrapMode = TextureWrapMode.Clamp;
    pongTextureDesc.filterMode = FilterMode.Point;
    var pingTexture = renderGraph.CreateTexture(pingTextureDesc);
    var pongTexture = renderGraph.CreateTexture(pongTextureDesc);
    var initPassName = $"{namePrefix}_ScreenSpaceSDF_Init";
    using (var builder = renderGraph.AddRasterRenderPass<InitPassData>(initPassName, out var passData))
    {
      passData.Material = material;
      passData.Mask = mask;
      builder.SetRenderAttachment(pingTexture, 0);
      builder.SetRenderFunc<InitPassData>(RunInit);
    }

    var maxDimension = Mathf.Max(pingTextureDesc.width, pingTextureDesc.height);
    var maxStep = Mathf.NextPowerOfTwo(maxDimension);
    var screenSize = new Vector2(pingTextureDesc.width, pingTextureDesc.height);
    for (var step = maxStep; step >= 1; step /= 2)
    {
      var stepPassName = $"{namePrefix}_ScreenSpaceSDF_Step(Size={step})";
      using (var builder = renderGraph.AddRasterRenderPass<StepPassData>(stepPassName, out var passData))
      {
        passData.Material = material;
        passData.Mask = mask;
        passData.Source = pingTexture;
        passData.ScreenSize = screenSize;
        passData.Step = step;
        builder.UseTexture(pingTexture);
        builder.SetRenderAttachment(pongTexture, 0);
        builder.SetRenderFunc<StepPassData>(RunStep);
      }
      (pingTexture, pongTexture) = (pongTexture, pingTexture);
    }

    // TODO: Implement the distance texture here
    var distanceTextureDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    distanceTextureDesc.name = $"{namePrefix}_ScreenSpaceDistance";
    distanceTextureDesc.format = GraphicsFormat.R16_SFloat;

    return pingTexture;
  }
}

class ScreenSpaceSDFRenderPass : ScriptableRenderPass
{
  public Material Material;
  public Texture2D Mask;

  RTHandle MaskRTHandle;

  public ScreenSpaceSDFRenderPass() {}

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer contextContainer)
  {
    var resourceData = contextContainer.Get<UniversalResourceData>();
    if (MaskRTHandle != null)
    {
      MaskRTHandle.Release();
    }
    MaskRTHandle = RTHandles.Alloc(Mask);
    var sssdf = ScreenSpaceSDFRenderUtils.ScreenSpaceSDFRenderPass(
      renderGraph,
      contextContainer,
      mask: renderGraph.ImportTexture(MaskRTHandle),
      Material);
    renderGraph.AddCopyPass(sssdf, resourceData.activeColorTexture);
  }
}