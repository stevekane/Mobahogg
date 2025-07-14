using UnityEngine;

public class WobblyMeshPointerTest : MonoBehaviour
{
  public WobblyMesh target;

  void Update()
  {
    if (Input.GetMouseButtonDown(0))
    {
      Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
      if (Physics.Raycast(ray, out var hit))
      {
        Vector3 impulseDir = hit.normal;
        target.ApplyImpulse(hit.point, impulseDir, 5f);
      }
    }
  }
}