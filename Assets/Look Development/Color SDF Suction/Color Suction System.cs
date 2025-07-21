using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
class ColorSuctionSystem : MonoBehaviour
{
  [SerializeField] Material Material;
  ColorSuctionRenderPass ColorSuctionRenderPass;

  void OnEnable()
  {
    ColorSuctionRenderPass = new ColorSuctionRenderPass();
    ColorSuctionRenderPass.Material = Material;
    RenderPipelineManager.beginCameraRendering += InjectColorSuctionRenderPass;
  }

  void OnDisable()
  {
    RenderPipelineManager.beginCameraRendering -= InjectColorSuctionRenderPass;
  }

  void InjectColorSuctionRenderPass(ScriptableRenderContext ctx, Camera camera)
  {
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(ColorSuctionRenderPass);
  }
}

class ColorSuctionRenderPass : ScriptableRenderPass
{
  public Material Material;

  public ColorSuctionRenderPass()
  {
    requiresIntermediateTexture = true;
    renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
  }

  public override void RecordRenderGraph(
  RenderGraph renderGraph,
  ContextContainer frameData)
  {
    const string PASS_NAME = "Color Suction Field";
    var resourceData = frameData.Get<UniversalResourceData>();
    var source = resourceData.activeColorTexture;
    var destinationDescription = renderGraph.GetTextureDesc(source);
    destinationDescription.name = $"{PASS_NAME} RT";
    destinationDescription.clearBuffer = true;
    var destination = renderGraph.CreateTexture(destinationDescription);
    var blitParameters = new RenderGraphUtils.BlitMaterialParameters(
      source,
      destination,
      Material,
      shaderPass: 0);
    renderGraph.AddBlitPass(blitParameters, PASS_NAME);
    resourceData.cameraColor = destination;
  }
}