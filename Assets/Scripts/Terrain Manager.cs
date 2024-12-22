using UnityEngine;
using System;

public class TerrainManager : SingletonBehavior<TerrainManager> {
  const float RayOriginHeight = 100;
  const float RayMaxDistance = 150;
  public string RequiredTag = "Ground";

  RaycastHit[] RaycastHits = new RaycastHit[256];

  public TerrainQueryResult? SamplePoint(Vector3 p) {
    var rayOrigin = p;
    rayOrigin.y = RayOriginHeight;
    var hitCount = Physics.RaycastNonAlloc(rayOrigin, Vector3.down, RaycastHits, RayMaxDistance);
    for (var i = 0; i < hitCount; i++) {
      var hit = RaycastHits[i];
      var matchesTag = hit.transform.CompareTag(RequiredTag);
      if (matchesTag) {
        return new TerrainQueryResult { Point = hit.point };
      }
    }
    return null;
  }

  void OnDrawGizmos() {
    var start = new Vector3(-30, 10, 0);
    var end = new Vector3(30, 10, 0);
    for (var i = 0; i < 20; i++) {
      var position = Vector3.Lerp(start, end, (float)i/19);
      var sample = SamplePoint(position);
      if (sample.HasValue) {
        Debug.DrawRay(sample.Value.Point, 10 * Vector3.up);
      }
    }
  }
}

[Serializable]
public struct TerrainQueryResult {
  public Vector3 Point;
}
