using UnityEngine;

[ExecuteAlways]
public class SDFSphere : MonoBehaviour
{
  public float Radius = 1;
  public float StretchFraction = 1;
  public Vector3 StretchAxis = Vector3.up;

  void OnEnable()
  {
    SDFRenderer.GlobalSystem.Spheres.Add(this);
  }

  void OnDisable()
  {
    SDFRenderer.GlobalSystem.Spheres.Remove(this);
  }
}