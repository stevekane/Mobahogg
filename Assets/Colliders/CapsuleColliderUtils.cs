using UnityEngine;

public static class CapsuleColliderUtils
{
  public static void AlignCapsuleBetweenPoints(
  this CapsuleCollider capsuleCollider,
  Vector3 pointA,
  Vector3 pointB)
  {
    var transform = capsuleCollider.transform;
    var direction = pointB - pointA;
    var distance = direction.magnitude;
    var midPoint = (pointA + pointB) * 0.5f;
    transform.position = midPoint;
    transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);
    capsuleCollider.direction = 1; // 0 = X, 1 = Y, 2 = Z (default is Y)
    capsuleCollider.height = distance;
    capsuleCollider.height = Mathf.Max(capsuleCollider.height, 2f * capsuleCollider.radius);
  }
}