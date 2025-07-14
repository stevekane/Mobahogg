using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SDFRenderer : MonoBehaviour
{
  [SerializeField] Material Material;
  [SerializeField] Vector4[] Spheres;
  Mesh FullScreenQuad;
  Vector4[] InternalSpheres = new Vector4[16];

  void OnEnable()
  {
    var filter = GetComponent<MeshFilter>();
    var renderer = GetComponent<MeshRenderer>();
    filter.sharedMesh = ProceduralMesh.FullScreenQuad();
    renderer.sharedMaterial = Material;
  }

  void OnDisable()
  {
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
    for (var i = 0; i < Spheres.Length; i++)
    {
      InternalSpheres[i] = Spheres[i];
    }
    Material.SetInt("_SphereCount", Spheres.Length);
    Material.SetVectorArray("_Spheres", InternalSpheres);
  }
}