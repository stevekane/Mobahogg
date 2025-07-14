using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DynamicGreyScaleRenderPass : MonoBehaviour
{
  void OnEnable()
  {
    RenderPipelineManager.beginCameraRendering += AddRenderPass;
  }

  void OnDisable()
  {
    RenderPipelineManager.beginCameraRendering -= AddRenderPass;
  }

  void AddRenderPass(ScriptableRenderContext ctx, Camera camera)
  {
    // TODO: You would add the greyscale render pass here. Sadly, the boilerplate to create a
    // render pass is enormous for so, so little value...
  }
}