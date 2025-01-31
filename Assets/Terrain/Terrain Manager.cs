using UnityEngine;
using System.Collections.Generic;
using System;

public class TerrainManager : SingletonBehavior<TerrainManager> {
    public static void RemoveCollinearPoints(List<Vector3> points, float tolerance = 0.0001f) {
        if (points == null || points.Count < 3)
            return; // Not enough points to simplify

        for (int i = points.Count - 2; i > 0; i--) { // Iterate backwards, skipping first and last
            if (IsCollinear(points[i - 1], points[i], points[i + 1], tolerance)) {
                points.RemoveAt(i); // Remove the middle point if collinear
            }
        }
    }

  public static bool IsCollinear(Vector3 a, Vector3 b, Vector3 c, float tolerance) {
      Vector3 ab = (b - a).normalized;
      Vector3 bc = (c - b).normalized;
      return Vector3.Cross(ab, bc).sqrMagnitude < tolerance; // If cross product is near zero, they are collinear
  }

  public static void SmoothVector3List(List<Vector3> points, int iterations = 1) {
    int count = points.Count;
    if (count < 3) return; // Not enough points to smooth

    for (int iter = 0; iter < iterations; iter++) {
      Vector3 previous = points[0]; // Store the first point

      for (int i = 1; i < count - 1; i++) {
        Vector3 temp = points[i]; // Store current value before modifying
        points[i] = (previous + points[i] + points[i + 1]) / 3f;
        previous = temp; // Move to the next element
      }
    }
  }

  public static void LaplacianSmooth(List<Vector3> points, float alpha = 0.5f) {
    int count = points.Count;
    if (count < 3) return;

    for (int i = 1; i < count - 1; i++) {
      points[i] = Vector3.Lerp(points[i], (points[i - 1] + points[i + 1]) / 2f, alpha);
    }
  }

  public static void GaussianSmooth(List<Vector3> points, float weight = 0.6f) {
    int count = points.Count;
    if (count < 3) return;

    for (int i = 1; i < count - 1; i++) {
      points[i] = points[i] * weight + (points[i - 1] + points[i + 1]) * ((1 - weight) / 2);
    }
  }

  const float RayOriginHeight = 100;
  const float RayMaxDistance = 150;
  const float SampleDistance = 0.25f;
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

  public bool TryAddPointToPath(TerrainPath path, Vector3 point) {
    var result = SamplePoint(point);
    if (result.HasValue) {
      path.Points.Add(result.Value.Point);
      return true;
    } else {
      return false;
    }
  }

  public TerrainPath Path(Vector3 start, Vector3 end) {
    var delta = end-start;
    var totalDistance = delta.magnitude;
    var direction = delta / totalDistance;
    var middleSamples = Mathf.FloorToInt(totalDistance / SampleDistance);
    var middleDistance = middleSamples * SampleDistance;
    var paddingDistance = totalDistance - middleDistance;
    var initialSample = start + paddingDistance / 2 * direction;
    var lastSample = end - paddingDistance / 2 * direction;
    var terrainPath = new TerrainPath { Points = new() };
    if (!TryAddPointToPath(terrainPath, start))
      return terrainPath;
    for (var i = 0; i < middleSamples; i++) {
      if (!TryAddPointToPath(terrainPath, Vector3.Lerp(initialSample, lastSample, (float)i/(middleSamples-1))))
        return terrainPath;
    }
    TryAddPointToPath(terrainPath, end);
    return terrainPath;
  }
}

[Serializable]
public struct TerrainPath {
  public List<Vector3> Points;
}

[Serializable]
public struct TerrainQueryResult {
  public Vector3 Point;
}