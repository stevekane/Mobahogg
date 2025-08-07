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
  const int MAX_SPHERE_COUNT = 32;

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
  [SerializeField] Material ScreenSpaceSDFMaterial;
  public List<SDFSphere> Spheres = new(MAX_SPHERE_COUNT);

  SphereData[] SphereArray = new SphereData[MAX_SPHERE_COUNT];
  ComputeBuffer SphereBuffer;
  SDFRenderPass SDFRenderPass;

  void OnEnable()
  {
    Spheres.Clear();
    SphereBuffer = new ComputeBuffer(MAX_SPHERE_COUNT, Marshal.SizeOf(typeof(SphereData)));
    SDFRenderPass = new SDFRenderPass();
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
    SDFRenderPass.DepthMaterial = DepthMaterial;
    SDFRenderPass.RenderingMaterial = RenderingMaterial;
    SDFRenderPass.ScreenSpaceSDFMaterial = ScreenSpaceSDFMaterial;
    DepthMaterial.SetBuffer("_Spheres", SphereBuffer);
    DepthMaterial.SetInt("_SphereCount", Spheres.Count);
  }

  void InjectRenderPass(ScriptableRenderContext ctx, Camera camera) {
    if (camera.cameraType == CameraType.Preview) return;
    if (camera.cameraType == CameraType.Reflection) return;
    camera.GetUniversalAdditionalCameraData().scriptableRenderer.EnqueuePass(SDFRenderPass);
  }
}