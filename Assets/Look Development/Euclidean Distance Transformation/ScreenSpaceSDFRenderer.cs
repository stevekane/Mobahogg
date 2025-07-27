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
    RenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
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
    public int Step;
  }

  class DistancePassData
  {
    public Material Material;
    public TextureHandle Source;
  }

  static int StepPropertyID = Shader.PropertyToID("_Step");
  static int SourcePropertyID = Shader.PropertyToID("_Source");
  static int MaskPropertyID = Shader.PropertyToID("_Mask");
  const int INIT_PASS_ID = 0;
  const int STEP_PASS_ID = 1;
  const int DISTANCE_PASS_ID = 2;

  static void RunInit(InitPassData passData, RasterGraphContext ctx)
  {
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture(MaskPropertyID, passData.Mask);
    ctx.cmd.DrawProcedural(
      matrix: Matrix4x4.identity,
      material: passData.Material,
      shaderPass: INIT_PASS_ID,
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
    propertyBlock.SetFloat(StepPropertyID, passData.Step);
    ctx.cmd.DrawProcedural(
      matrix: Matrix4x4.identity,
      material: passData.Material,
      shaderPass: STEP_PASS_ID,
      topology: MeshTopology.Triangles,
      vertexCount: 3,
      instanceCount: 1,
      propertyBlock);
  }

  static void RunDistance(DistancePassData passData, RasterGraphContext ctx)
  {
    var propertyBlock = new MaterialPropertyBlock();
    propertyBlock.SetTexture(SourcePropertyID, passData.Source);
    ctx.cmd.DrawProcedural(
      matrix: Matrix4x4.identity,
      material: passData.Material,
      shaderPass: DISTANCE_PASS_ID,
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
    var pingPongDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    pingPongDesc.format = GraphicsFormat.R32G32B32A32_SFloat;
    pingPongDesc.wrapMode = TextureWrapMode.Clamp;
    pingPongDesc.filterMode = FilterMode.Point;
    var pingTextureDesc = pingPongDesc;
    pingTextureDesc.name = $"{namePrefix}SSSDF_Ping";
    var pongTextureDesc = pingPongDesc;
    pongTextureDesc.name = $"{namePrefix}SSSDF_Pong";
    var distanceTextureDesc = renderGraph.GetTextureDesc(resourceData.activeColorTexture);
    distanceTextureDesc.name = $"{namePrefix}SSSDF_Distance";
    // distanceTextureDesc.format = GraphicsFormat.R32G32B32A32_SFloat;
    distanceTextureDesc.format = GraphicsFormat.R32_SFloat;
    distanceTextureDesc.wrapMode = TextureWrapMode.Clamp;
    distanceTextureDesc.filterMode = FilterMode.Point;
    var pingTexture = renderGraph.CreateTexture(pingTextureDesc);
    var pongTexture = renderGraph.CreateTexture(pongTextureDesc);
    var distanceTexture = renderGraph.CreateTexture(distanceTextureDesc);

    var initPassName = $"{namePrefix}SSSDF_Init";
    using (var builder = renderGraph.AddRasterRenderPass<InitPassData>(initPassName, out var passData))
    {
      passData.Material = material;
      passData.Mask = mask;
      builder.SetRenderAttachment(pingTexture, 0);
      builder.SetRenderFunc<InitPassData>(RunInit);
    }

    var maxDimension = Mathf.Max(pingTextureDesc.width, pingTextureDesc.height);
    var maxStep = Mathf.NextPowerOfTwo(maxDimension) / 2;
    for (var step = maxStep; step >= 1; step /= 2)
    {
      var stepPassName = $"{namePrefix}SSSDF_Step(Size={step})";
      using (var builder = renderGraph.AddRasterRenderPass<StepPassData>(stepPassName, out var passData))
      {
        passData.Material = material;
        passData.Mask = mask;
        passData.Source = pingTexture;
        passData.Step = step;
        builder.UseTexture(pingTexture);
        builder.SetRenderAttachment(pongTexture, 0);
        builder.SetRenderFunc<StepPassData>(RunStep);
      }
      (pingTexture, pongTexture) = (pongTexture, pingTexture);
    }

    // Debug.Log($"DESC.NAME:{distanceTextureDesc.name} | TEX.DESC.NAME:{distanceTexture.GetDescriptor(renderGraph).name}");
    var distancePassName = $"{namePrefix}SSSDF_Distance";
    using (var builder = renderGraph.AddRasterRenderPass<DistancePassData>(distancePassName, out var passData)) {
      passData.Source = pingTexture;
      passData.Material = material;
      builder.UseTexture(pingTexture);
      builder.SetRenderAttachment(distanceTexture, 0);
      builder.SetRenderFunc<DistancePassData>(RunDistance);
    }
    return distanceTexture;
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