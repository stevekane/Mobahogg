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
    TerrainQueryResult? result = null;
    for (var i = 0; i < hitCount; i++) {
      var hit = RaycastHits[i];
      var matchesTag = hit.transform.CompareTag(RequiredTag);
      if (matchesTag && (!result.HasValue || result.Value.Point.y < hit.point.y)) {
        result = new TerrainQueryResult { Point = hit.point };
      }
    }
    return result;
  }
}

[Serializable]
public struct TerrainQueryResult {
  public Vector3 Point;
}
