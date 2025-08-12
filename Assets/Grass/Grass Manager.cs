using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

[ExecuteAlways]
[DefaultExecutionOrder(-1)] // run before sdf ( could  / should be done better )
public class GrassManager : MonoBehaviour
{
  [SerializeField] MeshFilter MeshFilter;

  public GrassRenderPass RenderPass;
  public ComputeShader GrassComputeShader;
  [Range(1, 1000)] public float MaxDensity = 100;

  const string KernelName = "GenerateGrass";

  [StructLayout(LayoutKind.Sequential)]
  struct GrassInstance
  {
    public Vector3 position;
  }

  static readonly int TrianglesID = Shader.PropertyToID("Triangles");
  static readonly int VertexPositionsID = Shader.PropertyToID("VertexPositions");
  static readonly int VertexNormalsID = Shader.PropertyToID("VertexNormals");
  static readonly int VertexDensitiesID = Shader.PropertyToID("VertexDensities");
  static readonly int GrassInstancesID = Shader.PropertyToID("GrassInstances");
  static readonly int TriangleCountID = Shader.PropertyToID("TriangleCount");
  static readonly int MaxDensityID = Shader.PropertyToID("MaxDensity");
  static readonly int ObjectToWorldSpaceMatrixID = Shader.PropertyToID("ObjectToWorldSpaceMatrix");

  GraphicsBuffer TrianglesBuffer;
  GraphicsBuffer VertexPositionsBuffer;
  GraphicsBuffer VertexNormalsBuffer;
  GraphicsBuffer VertexDensitiesBuffer;
  GraphicsBuffer GrassInstancesBuffer;
  GraphicsBuffer IndirectArgsBuffer;
  uint[] IndirectArgs;

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
    if (MeshFilter == null) return;
    if (MeshFilter.sharedMesh == null) return;
    if (RenderPass == null) return;
    if (RenderPass.InstanceMaterial == null) return;
    if (RenderPass.InstanceMesh == null) return;

    // TODO: is it possible to use the already-uploaded buffers created by the Mesh?
    var mesh = MeshFilter.sharedMesh;
    var vertexCount = mesh.vertexCount;
    var vertexPositions = mesh.vertices;
    var vertexNormals = mesh.normals;
    var vertexDensities = new float[mesh.vertexCount];
    var triangles = mesh.triangles;
    var trianglesSize = triangles.Length;
    var triangleCount = trianglesSize / 3;
    for (var i = 0; i < vertexCount; i++)
    {
      vertexDensities[i] = 1;
    }
    TrianglesBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Structured,
      count: trianglesSize,
      stride: Marshal.SizeOf(typeof(uint)));
    TrianglesBuffer.SetData(triangles);

    VertexPositionsBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Structured,
      count: vertexCount,
      stride: Marshal.SizeOf(typeof(Vector3)));
    VertexPositionsBuffer.SetData(vertexPositions);

    VertexNormalsBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Structured,
      count: vertexCount,
      stride: Marshal.SizeOf(typeof(Vector3)));
    VertexNormalsBuffer.SetData(vertexNormals);

    VertexDensitiesBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Structured,
      count: vertexCount,
      stride: Marshal.SizeOf(typeof(float)));
    VertexDensitiesBuffer.SetData(vertexDensities);

    var MAX_GRASS_COUNT = (int)Mathf.Pow(2, 24); // ~16M. Could be set higher probably if needed
    GrassInstancesBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Append,
      count: MAX_GRASS_COUNT,
      stride: Marshal.SizeOf(typeof(GrassInstance)));

    IndirectArgs = new uint[5];
    IndirectArgs[0] = RenderPass.InstanceMesh.GetIndexCount(0);
    IndirectArgs[1] = 0;
    IndirectArgs[2] = RenderPass.InstanceMesh.GetIndexStart(0);
    IndirectArgs[3] = RenderPass.InstanceMesh.GetBaseVertex(0);
    IndirectArgs[4] = 0;
    IndirectArgsBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.IndirectArguments,
      count: 1,
      stride: sizeof(uint) * 5);
    IndirectArgsBuffer.SetData(IndirectArgs);

    int kernel = GrassComputeShader.FindKernel(KernelName);

    GrassComputeShader.SetBuffer(kernel, TrianglesID, TrianglesBuffer);
    GrassComputeShader.SetBuffer(kernel, VertexPositionsID, VertexPositionsBuffer);
    GrassComputeShader.SetBuffer(kernel, VertexNormalsID, VertexNormalsBuffer);
    GrassComputeShader.SetBuffer(kernel, VertexDensitiesID, VertexDensitiesBuffer);
    GrassComputeShader.SetBuffer(kernel, GrassInstancesID, GrassInstancesBuffer);
    GrassComputeShader.SetMatrix(ObjectToWorldSpaceMatrixID, transform.localToWorldMatrix);
    GrassComputeShader.SetInt(TriangleCountID, triangleCount);
    GrassComputeShader.SetFloat(MaxDensityID, MaxDensity);
    GrassComputeShader.GetKernelThreadGroupSizes(kernel, out var thx, out var _, out var _);
    int groupCount = Mathf.CeilToInt(triangleCount / (float)thx);
    GrassComputeShader.Dispatch(kernel, groupCount, 1, 1);

    GraphicsBuffer.CopyCount(
      src: GrassInstancesBuffer,
      dst: IndirectArgsBuffer,
      dstOffsetBytes: sizeof(uint)); // the second index

    RenderPass.ArgsBuffer = IndirectArgsBuffer;
    RenderPass.InstanceBuffer = GrassInstancesBuffer;
    RenderPass.renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
  }

  void Cleanup()
  {
    TrianglesBuffer?.Release();
    VertexPositionsBuffer?.Release();
    VertexNormalsBuffer?.Release();
    VertexDensitiesBuffer?.Release();
    GrassInstancesBuffer?.Release();
    IndirectArgsBuffer?.Release();
  }

  void InjectRenderPass(ScriptableRenderContext context, Camera camera)
  {
    if (MeshFilter == null) return;
    if (MeshFilter.sharedMesh == null) return;
    if (RenderPass == null) return;
    if (RenderPass.InstanceMaterial == null) return;
    if (RenderPass.InstanceMesh == null) return;
    if (camera.cameraType == CameraType.Preview) return;
    if (camera.cameraType == CameraType.Reflection) return;
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(RenderPass);
  }
}

[System.Serializable]
public class GrassRenderPass : ScriptableRenderPass
{
  public Mesh InstanceMesh;
  public Material InstanceMaterial;
  public GraphicsBuffer InstanceBuffer;
  public GraphicsBuffer ArgsBuffer;

  static readonly int GrassInstancesID = Shader.PropertyToID("GrassInstances");

  static void Render(PassData passData, RasterGraphContext ctx)
  {
    passData.InstanceMaterial.SetBuffer(GrassInstancesID, passData.InstanceBuffer);
    ctx.cmd.DrawMeshInstancedIndirect(
      passData.InstanceMesh,
      submeshIndex: 0,
      passData.InstanceMaterial,
      shaderPass: 0,
      passData.ArgsBuffer);
  }

  class PassData
  {
    public Mesh InstanceMesh;
    public Material InstanceMaterial;
    public GraphicsBuffer InstanceBuffer;
    public GraphicsBuffer ArgsBuffer;
  }

  public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
  {
    var resourceData = frameData.Get<UniversalResourceData>();
    using (var builder = renderGraph.AddRasterRenderPass<PassData>("Grass Render", out var passData))
    {
      passData.InstanceMesh = InstanceMesh;
      passData.InstanceMaterial = InstanceMaterial;
      passData.InstanceBuffer = InstanceBuffer;
      passData.ArgsBuffer = ArgsBuffer;
      builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
      builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
      builder.SetRenderFunc<PassData>(Render);
    }
  }
}