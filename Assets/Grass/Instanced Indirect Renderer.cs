using UnityEngine;

[ExecuteAlways]
public class InstancedIndirectRenderer : MonoBehaviour
{
  public Mesh mesh;
  public Material material;
  public ComputeShader computeShader;

  const int INSTANCE_COUNT = 100;
  GraphicsBuffer argsBuffer;
  GraphicsBuffer matrixBuffer;

  void OnEnable()
  {
    argsBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.IndirectArguments,
      count: 1,
      stride: 5 * sizeof(uint));
    uint[] args = new uint[5] {
      mesh.GetIndexCount(0),
      (uint)INSTANCE_COUNT,
      mesh.GetIndexStart(0),
      mesh.GetBaseVertex(0),
      0
    };
    argsBuffer.SetData(args);
    matrixBuffer = new GraphicsBuffer(
      GraphicsBuffer.Target.Structured,
      count: INSTANCE_COUNT,
      stride: sizeof(float) * 16);
    int kernel = computeShader.FindKernel("CSMain");
    computeShader.SetBuffer(kernel, "_Matrices", matrixBuffer);
    computeShader.Dispatch(kernel, INSTANCE_COUNT, 1, 1);
  }

  void OnDisable()
  {
    argsBuffer?.Release();
    matrixBuffer?.Release();
  }

  void OnDestroy()
  {
    argsBuffer?.Release();
    matrixBuffer?.Release();
  }

  void Update()
  {
    material.SetBuffer("_Matrices", matrixBuffer);
    var rp = new RenderParams(material)
    {
      worldBounds = new Bounds(Vector3.zero, Vector3.one * 1000),
      // shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On
    };
    Graphics.RenderMeshIndirect(rp, mesh, argsBuffer);
  }
}