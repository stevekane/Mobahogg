using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

[ExecuteAlways]
// TODO: Just a test to see if I can force this rendering to occur before SDF rendering.
[DefaultExecutionOrder(-1)]
public class GrassManager : MonoBehaviour
{
  public GrassRenderPass RenderPass;
  public ComputeShader GrassComputeShader;
  public int GrassCount = 100000;
  public float TerrainSize = 60f;

  const string KernelName = "GenerateGrass";
  const int ThreadsPerGroup = 64;

  [StructLayout(LayoutKind.Sequential)]
  struct GrassInstance
  {
    public Vector3 position;
    public float scale;
  }

  static readonly int GrassInstancesID = Shader.PropertyToID("GrassInstances");
  static readonly int GrassCountID = Shader.PropertyToID("GrassCount");
  static readonly int TerrainSizeID = Shader.PropertyToID("TerrainSize");

  GraphicsBuffer GrassInstancesBuffer;
  GraphicsBuffer IndirectArgsBuffer;
  uint[] IndirectArgs = new uint[5];

  void OnEnable()
  {
    Setup();
    RenderPipelineManager.beginCameraRendering += InjectRenderPass;
  }

  void OnDisable()
  {
    Cleanup();
    RenderPipelineManager.beginCameraRendering -= InjectRenderPass;
  }

  void OnValidate()
  {
    Cleanup();
    Setup();
  }

  void OnDestroy()
  {
    Cleanup();
  }

  void Setup()
  {
    IndirectArgs[0] = RenderPass.Mesh.GetIndexCount(0);
    IndirectArgs[1] = (uint)GrassCount; // could set inside compute shader if total count may vary
    IndirectArgs[2] = RenderPass.Mesh.GetIndexStart(0);
    IndirectArgs[3] = RenderPass.Mesh.GetBaseVertex(0);
    IndirectArgs[4] = 0;
    GrassInstancesBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Structured,
      count: GrassCount,
      stride: Marshal.SizeOf(typeof(GrassInstance)));
    IndirectArgsBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.IndirectArguments,
      count: 1,
      stride: sizeof(uint) * 5);
    IndirectArgsBuffer.SetData(IndirectArgs);
    int kernel = GrassComputeShader.FindKernel(KernelName);
    GrassComputeShader.SetBuffer(kernel, GrassInstancesID, GrassInstancesBuffer);
    GrassComputeShader.SetInt(GrassCountID, GrassCount);
    GrassComputeShader.SetFloat(TerrainSizeID, TerrainSize);
    int groupCount = Mathf.CeilToInt((float)GrassCount / ThreadsPerGroup);
    GrassComputeShader.Dispatch(kernel, groupCount, 1, 1);
    RenderPass.ArgsBuffer = IndirectArgsBuffer;
    RenderPass.InstanceBuffer = GrassInstancesBuffer;
  }

  void Cleanup()
  {
    GrassInstancesBuffer?.Release();
    IndirectArgsBuffer?.Release();
    GrassInstancesBuffer = null;
    IndirectArgsBuffer = null;
  }

  void InjectRenderPass(ScriptableRenderContext context, Camera camera)
  {
    if (RenderPass == null) return;
    if (RenderPass.Material == null) return;
    if (RenderPass.Mesh == null) return;
    if (camera.cameraType == CameraType.Preview) return;
    if (camera.cameraType == CameraType.Reflection) return;
    RenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(RenderPass);
  }
}

[System.Serializable]
public class GrassRenderPass : ScriptableRenderPass
{
  public Material Material;
  public Mesh Mesh;
  public GraphicsBuffer InstanceBuffer;
  public GraphicsBuffer ArgsBuffer;

  static readonly int GrassInstancesID = Shader.PropertyToID("GrassInstances");

  static void Render(PassData passData, RasterGraphContext ctx)
  {
    passData.Material.SetBuffer(GrassInstancesID, passData.InstanceBuffer);
    ctx.cmd.DrawMeshInstancedIndirect(
      passData.Mesh,
      submeshIndex: 0,
      passData.Material,
      shaderPass: 0,
      passData.ArgsBuffer);
  }

  class PassData
  {
    public Mesh Mesh;
    public Material Material;
    public GraphicsBuffer InstanceBuffer;
    public GraphicsBuffer ArgsBuffer;
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    using (var builder = renderGraph.AddRasterRenderPass<PassData>("Grass Render", out var passData))
    {
      passData.Mesh = Mesh;
      passData.Material = Material;
      passData.InstanceBuffer = InstanceBuffer;
      passData.ArgsBuffer = ArgsBuffer;
      builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
      builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
      builder.SetRenderFunc<PassData>(Render);
    }
  }
}