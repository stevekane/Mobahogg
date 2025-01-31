using UnityEngine;

public class TerrainManagerTester : MonoBehaviour {
  [SerializeField] Transform p0;
  [SerializeField] Transform p1;
  [SerializeField, Range(0, 20)] int SmoothingIterations = 1;
  [SerializeField] bool Simplify;

  void LateUpdate() {
    var path = TerrainManager.Instance.Path(p0.position, p1.position);
    for (var i = 0; i < SmoothingIterations; i++) {
      TerrainManager.LaplacianSmooth(path.Points);
    }
    if (Simplify) {
      TerrainManager.RemoveCollinearPoints(path.Points);
    }
    foreach(var point in path.Points) {
      Debug.DrawRay(point, Vector3.up, Color.green);
    }
  }
}