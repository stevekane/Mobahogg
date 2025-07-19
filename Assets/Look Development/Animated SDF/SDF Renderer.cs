using UnityEngine;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SDFRenderer : MonoBehaviour
{
  public static SDFRenderer GlobalSystem;

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

  [SerializeField] Material Material;
  public List<SDFSphere> Spheres = new(MAX_SPHERE_COUNT);
  SphereData[] SphereArray = new SphereData[MAX_SPHERE_COUNT];
  ComputeBuffer SphereBuffer;
  Mesh FullScreenQuad;

  void OnEnable()
  {
    GlobalSystem = this;
    var filter = GetComponent<MeshFilter>();
    var renderer = GetComponent<MeshRenderer>();
    filter.sharedMesh = ProceduralMesh.FullScreenQuad();
    renderer.sharedMaterial = Material;
    SphereBuffer = new ComputeBuffer(MAX_SPHERE_COUNT, Marshal.SizeOf(typeof(SphereData)));
  }

  void OnDisable()
  {
    SphereBuffer.Dispose();
    if (!FullScreenQuad)
      return;
#if UNITY_EDITOR
    if (Application.isPlaying)
    {
      Destroy(FullScreenQuad);
    }
    else
    {
      DestroyImmediate(FullScreenQuad);
    }
#else
    Destroy(FullScreenQuad);
#endif
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
    // TODO: Must this be set every frame? Seems unlikely?
    // maybe you only need to update the data in it?
    Material.SetBuffer("_Spheres", SphereBuffer);
    Material.SetInt("_SphereCount", Spheres.Count);
  }
}