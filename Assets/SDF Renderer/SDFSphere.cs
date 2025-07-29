using UnityEngine;

// run after SDFRenderer runs.
[DefaultExecutionOrder(1)]
[ExecuteAlways]
public class SDFSphere : MonoBehaviour
{
  public float Radius = 1;
  public float StretchFraction = 1;
  public Vector3 StretchAxis = Vector3.up;

  void OnEnable()
  {
    var renderer = FindFirstObjectByType<SDFRenderer>();
    if (renderer)
    {
      renderer.Spheres.Add(this);
    }
  }

  void OnDisable()
  {
    var renderer = FindFirstObjectByType<SDFRenderer>();
    if (renderer)
    {
      renderer.Spheres.Remove(this);
    }
  }
}