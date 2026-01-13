using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Grass
{
  [ExecuteAlways]
  [DefaultExecutionOrder((int)ExecutionGroups.Managers)]
  public class GrassManager : MonoBehaviour
  {
    [SerializeField] Mesh InstanceMesh;
    [SerializeField] Material InstanceMaterial;
    [SerializeField] ComputeShader PlaceGrassOnMeshComputeShader;
    [SerializeField, Range(1, 100)] float MaxDensity = 100;

    GrassRenderPass RenderPass;

    const string KernelName = "GenerateGrass";

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

    public static GrassManager Instance;

    void OnEnable()
    {
      Instance = this;
      RenderPass?.ArgsBuffer.Release();
      RenderPass?.InstanceBuffer.Release();
      RenderPass = new(InstanceMesh, InstanceMaterial);
      RenderPipelineManager.beginCameraRendering += InjectRenderPass;
    }

    void OnDisable()
    {
      RenderPipelineManager.beginCameraRendering -= InjectRenderPass;
      RenderPass?.ArgsBuffer.Release();
      RenderPass?.InstanceBuffer.Release();
      RenderPass = null;
      // Instance = null;
    }

    public void UpdateGrass(Mesh mesh)
    {
      if (RenderPass == null)
        return;

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

      int kernel = PlaceGrassOnMeshComputeShader.FindKernel(KernelName);
      PlaceGrassOnMeshComputeShader.SetBuffer(kernel, TrianglesID, TrianglesBuffer);
      PlaceGrassOnMeshComputeShader.SetBuffer(kernel, VertexPositionsID, VertexPositionsBuffer);
      PlaceGrassOnMeshComputeShader.SetBuffer(kernel, VertexNormalsID, VertexNormalsBuffer);
      PlaceGrassOnMeshComputeShader.SetBuffer(kernel, VertexDensitiesID, VertexDensitiesBuffer);
      PlaceGrassOnMeshComputeShader.SetBuffer(kernel, GrassInstancesID, RenderPass.InstanceBuffer);
      PlaceGrassOnMeshComputeShader.SetMatrix(ObjectToWorldSpaceMatrixID, transform.localToWorldMatrix);
      PlaceGrassOnMeshComputeShader.SetInt(TriangleCountID, triangleCount);
      PlaceGrassOnMeshComputeShader.SetFloat(MaxDensityID, MaxDensity);
      PlaceGrassOnMeshComputeShader.GetKernelThreadGroupSizes(kernel, out var thx, out var _, out var _);
      int groupCount = Mathf.CeilToInt(triangleCount / (float)thx);
      PlaceGrassOnMeshComputeShader.Dispatch(kernel, groupCount, 1, 1);

      GraphicsBuffer.CopyCount(
        src: RenderPass.InstanceBuffer,
        dst: RenderPass.ArgsBuffer,
        dstOffsetBytes: sizeof(uint)); // write to the second index of args buffer

      TrianglesBuffer?.Release();
      VertexPositionsBuffer?.Release();
      VertexNormalsBuffer?.Release();
      VertexDensitiesBuffer?.Release();
    }

    void InjectRenderPass(ScriptableRenderContext context, Camera camera)
    {
      if (camera.cameraType == CameraType.Preview) return;
      if (camera.cameraType == CameraType.Reflection) return;
      camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(RenderPass);
    }
  }

  public class GrassRenderPass : ScriptableRenderPass
  {
    public readonly Mesh InstanceMesh;
    public readonly Material InstanceMaterial;
    public readonly GraphicsBuffer InstanceBuffer;
    public readonly GraphicsBuffer ArgsBuffer;

    uint[] IndirectArgsFromMesh(Mesh mesh) => new uint[5] {
    mesh.GetIndexCount(submesh: 0),
    0,
    mesh.GetIndexStart(submesh: 0),
    mesh.GetBaseVertex(submesh: 0),
    0
  };

    public GrassRenderPass(Mesh instanceMesh, Material instanceMaterial)
    {
      var MAX_GRASS_COUNT = (int)Mathf.Pow(2, 20);
      InstanceMesh = instanceMesh;
      InstanceMaterial = instanceMaterial;
      InstanceBuffer = new GraphicsBuffer(
        GraphicsBuffer.Target.Append,
        count: MAX_GRASS_COUNT,
        stride: Marshal.SizeOf(typeof(GrassInstance)));

      var indirectArgs = IndirectArgsFromMesh(InstanceMesh);
      ArgsBuffer = new GraphicsBuffer(
        GraphicsBuffer.Target.IndirectArguments,
        count: 1,
        stride: sizeof(uint) * indirectArgs.Length);
      ArgsBuffer.SetData(indirectArgs);
      renderPassEvent = RenderPassEvent.AfterRenderingOpaques;
    }

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
      using var builder = renderGraph.AddRasterRenderPass<PassData>("Grass Render", out var passData);
      passData.InstanceMesh = InstanceMesh;
      passData.InstanceMaterial = InstanceMaterial;
      passData.InstanceBuffer = InstanceBuffer;
      passData.ArgsBuffer = ArgsBuffer;
      var resourceData = frameData.Get<UniversalResourceData>();
      builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
      builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);
      builder.SetRenderFunc<PassData>(Render);
    }
  }

  [StructLayout(LayoutKind.Sequential)]
  struct GrassInstance
  {
    public Vector3 position;
  }
}