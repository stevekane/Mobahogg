using UnityEngine;

class NavCornerSmoothingVisualizer : MonoBehaviour
{
  [SerializeField] float MaxSpeed = 10;
  [SerializeField] float MaxAcceleration = 20;
  [SerializeField] float MaxTurnSpeed = 360;
  [SerializeField] Vector3[] Points = new Vector3[3]
  {
    Vector3.zero,
    10 * Vector3.right,
    10 * Vector3.right + 10 * Vector3.forward,
  };

  void OnDrawGizmos()
  {
    var cornerFillet = NavCornerSmoothing.ComputeCornerFillet(
      Points[0],
      Points[1],
      Points[2],
      MaxSpeed,
      MaxAcceleration,
      MaxTurnSpeed * Mathf.Deg2Rad);
    CornerFilletGizmos.Draw(cornerFillet, arcSegments: 32);
    Gizmos.color = Color.cyan;
    Gizmos.DrawLineStrip(Points, looped: false);
  }
}