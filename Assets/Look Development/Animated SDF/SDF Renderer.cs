using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public class SDFRenderer : MonoBehaviour
{
  [SerializeField] Material Material;
  Mesh FullScreenQuad;

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
}