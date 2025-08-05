using System.Runtime.InteropServices;
using UnityEngine;

[ExecuteAlways]
public class GrassManager : MonoBehaviour
{
  public Mesh GrassMesh;
  public Material GrassMaterial;
  public ComputeShader GrassComputeShader;
  public int GrassCount = 100000;
  public float TerrainSize = 60f;

  [StructLayout(LayoutKind.Sequential)]
  struct GrassInstance
  {
    public Vector3 position;
    public float scale;
  }

  const string KernelName = "GenerateGrass";
  const int ThreadGroupsXSize = 1024;
  int GrassInstancesPropertyID = Shader.PropertyToID("GrassInstances");
  int GrassCountPropertyID = Shader.PropertyToID("GrassCount");
  int TerrainSizePropertyID = Shader.PropertyToID("TerrainSize");
  GraphicsBuffer GrassInstancesBuffer;
  GraphicsBuffer IndirectArgsBuffer;
  uint[] IndirectArgs = new uint[5] { 0, 0, 0, 0, 0 };

  void OnValidate() {
    Cleanup();
    Setup();
  }

  void OnEnable() => Setup();

  void OnDisable() => Cleanup();

  void OnDestroy() => Cleanup();

  void Update()
  {
    var renderParams = new RenderParams(GrassMaterial)
    {
      worldBounds = new Bounds(Vector3.zero, TerrainSize * Vector3.one),
      // layer = gameObject.layer
    };
    GrassMaterial.SetBuffer(GrassInstancesPropertyID, GrassInstancesBuffer);
    // This may be getting called at the wrong time...
    // The problem with the lighting may be that there is no lighting data at the time
    // this shader is run.
    // This code may need to be migrated into a render pass that is setup to run after opaques
    // or something
    Graphics.RenderMeshIndirect(renderParams, GrassMesh, IndirectArgsBuffer);
  }

  void Setup()
  {
    IndirectArgs[0] = GrassMesh.GetIndexCount(0);
    IndirectArgs[1] = (uint)GrassCount;
    IndirectArgs[2] = GrassMesh.GetIndexStart(0);
    IndirectArgs[3] = GrassMesh.GetBaseVertex(0);
    IndirectArgs[4] = 0;
    GrassInstancesBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Structured,
      count: GrassCount,
      stride: Marshal.SizeOf(typeof(GrassInstance)));
    IndirectArgsBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.IndirectArguments,
      count: 1,
      stride: 5 * sizeof(uint));
    IndirectArgsBuffer.SetData(IndirectArgs);
    int kernelHandle = GrassComputeShader.FindKernel(KernelName);
    int threadGroupsX = Mathf.Max(1, Mathf.CeilToInt(GrassCount / ThreadGroupsXSize));
    GrassComputeShader.SetBuffer(kernelHandle, GrassInstancesPropertyID, GrassInstancesBuffer);
    GrassComputeShader.SetInt(GrassCountPropertyID, GrassCount);
    GrassComputeShader.SetFloat(TerrainSizePropertyID, TerrainSize);
    GrassComputeShader.Dispatch(kernelHandle, threadGroupsX, threadGroupsY: 1, threadGroupsZ: 1);
  }

  void Cleanup()
  {
    if (GrassInstancesBuffer != null) GrassInstancesBuffer.Release();
    if (IndirectArgsBuffer != null) IndirectArgsBuffer.Release();
    GrassInstancesBuffer = null;
    IndirectArgsBuffer = null;
  }
}