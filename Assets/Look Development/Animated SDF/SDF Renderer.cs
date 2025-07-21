using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using System.Collections.Generic;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class SDFRenderer : MonoBehaviour
{
  const int MAX_SPHERE_COUNT = 16;

  [StructLayout(LayoutKind.Sequential)]
  struct SphereData
  {
    public float3 center;
    public float radius;
    public float3 stretchAxis;
    public float stretchFraction;
    public float inverseSquareRootStretchFraction;
  }

  [SerializeField] Material DepthMaterial;
  [SerializeField] Material RenderingMaterial;
  public List<SDFSphere> Spheres = new(MAX_SPHERE_COUNT);

  SphereData[] SphereArray = new SphereData[MAX_SPHERE_COUNT];
  ComputeBuffer SphereBuffer;
  SDFRenderPass SDFRenderPass;

  void OnEnable()
  {
    Spheres.Clear();
    SphereBuffer = new ComputeBuffer(MAX_SPHERE_COUNT, Marshal.SizeOf(typeof(SphereData)));
    DepthMaterial.SetBuffer("_Spheres", SphereBuffer);
    SDFRenderPass = new SDFRenderPass();
    SDFRenderPass.DepthMaterial = DepthMaterial;
    SDFRenderPass.RenderingMaterial = RenderingMaterial;
    RenderPipelineManager.beginCameraRendering += InjectRenderPass;
  }

  void OnDisable()
  {
    Spheres.Clear();
    SphereBuffer.Dispose();
    RenderPipelineManager.beginCameraRendering -= InjectRenderPass;
  }

  void Update() {
    for (var i = 0; i < Spheres.Count; i++)
    {
      SphereArray[i] = new()
      {
        center = Spheres[i].transform.position,
        radius = Spheres[i].Radius,
        stretchAxis = Spheres[i].StretchAxis,
        stretchFraction = Spheres[i].StretchFraction,
        inverseSquareRootStretchFraction = 1f / Mathf.Sqrt(Spheres[i].StretchFraction)
      };
    }
    SphereBuffer.SetData(SphereArray);
    DepthMaterial.SetBuffer("_Spheres", SphereBuffer);
    DepthMaterial.SetInt("_SphereCount", Spheres.Count);
  }

  void InjectRenderPass(ScriptableRenderContext ctx, Camera camera) {
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(SDFRenderPass);
  }
}