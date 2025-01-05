using UnityEngine;

public static class PhysicsUtils {
  public static float DistanceFromLine(Vector3 lineStart, Vector3 lineEnd, Vector3 point) {
    Vector3 lineDirection = lineEnd - lineStart;
    float lineLengthSquared = lineDirection.sqrMagnitude;
    if (lineLengthSquared == 0.0f) {
      return Vector3.Distance(lineStart, point);
    }
    Vector3 pointToStart = point - lineStart;
    float t = Mathf.Clamp01(Vector3.Dot(pointToStart, lineDirection) / lineLengthSquared);
    Vector3 projection = lineStart + t * lineDirection;
    return Vector3.Distance(point, projection);
  }
}