using UnityEngine;

class Tongue : MonoBehaviour
{
  public static void AlignCapsuleBetweenPoints(CapsuleCollider capsuleCollider, Vector3 pointA, Vector3 pointB)
  {
    var transform = capsuleCollider.transform;

    // Compute direction and distance
    Vector3 direction = pointB - pointA;
    float distance = direction.magnitude;
    Vector3 midPoint = (pointA + pointB) * 0.5f;

    // Move the object to the midpoint
    transform.position = midPoint;

    // Orient the object to face from A to B
    transform.rotation = Quaternion.FromToRotation(Vector3.up, direction);

    // Adjust the capsule height to fit the distance
    capsuleCollider.direction = 1; // 0 = X, 1 = Y, 2 = Z (default is Y)
    capsuleCollider.height = distance;

    // Optional: clamp to minimum size so height >= 2 * radius
    capsuleCollider.height = Mathf.Max(capsuleCollider.height, 2f * capsuleCollider.radius);
  }

  [SerializeField] LineRenderer LineRenderer;
  [SerializeField] CapsuleCollider Collider;

  public void SetTongueEnd(Vector3 end)
  {
    AlignCapsuleBetweenPoints(Collider, end, transform.position);
    LineRenderer.SetPosition(0, transform.position);
    LineRenderer.SetPosition(1, end);
  }
}