using System.Collections;
using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Managed)]
public class Cannon : MonoBehaviour {
  [SerializeField] LocalClock LocalClock;
  [SerializeField] GameObject CannonBallPrefab;
  [SerializeField] GameObject CannonFireExplosionPrefab;
  [SerializeField] Transform Barrel;
  [SerializeField] Transform LaunchSite;
  [SerializeField] Timeval ShotCooldown = Timeval.FromSeconds(3);
  [SerializeField] float CameraShakeIntensity = 3;
  [SerializeField] float TurningSpeed = 360;
  [SerializeField] float SearchFrequency = 0.25f;

  public float FieldOfView = 90;
  public float MinRange = 2;
  public float MaxRange = 30;

  public static Quaternion InterpolateWithSine(float frequency, Quaternion start, Quaternion end, float time) {
    float sineValue = 0.5f * (1 + Mathf.Sin(2 * Mathf.PI * frequency * time));
    return Quaternion.Slerp(start, end, sineValue);
  }

  /*
  TODO:

  I believe, technically, this yielding to FixedUpdate doesn't work the way you might assume. I think it
  actually yields until some coroutine managers next FixedUpdate and thus doesn't respect Execution Order
  as you might expect.
  */
  IEnumerator Start() {
    var nextFixedUpdate = new WaitForFixedUpdate();
    while (true) {
      for (var i = 0; i < ShotCooldown.Ticks; i+=LocalClock.DeltaFrames()) {
        yield return nextFixedUpdate;
      }
      var target = CannonManager.Instance.BestTarget(this);
      if (target) {
        Destroy(Instantiate(CannonFireExplosionPrefab, LaunchSite.position, Barrel.rotation, transform), 3);
        Destroy(Instantiate(CannonBallPrefab, LaunchSite.position, Barrel.rotation, transform), 5);
        CameraManager.Instance.Shake(CameraShakeIntensity);
      }
    }
  }

  void FixedUpdate() {
    var target = CannonManager.Instance.BestTarget(this);
    Quaternion desired;
    if (target) {
      var toTarget = target.transform.position-transform.position;
      var direction = toTarget.XZ().normalized;
      desired = Quaternion.LookRotation(direction);
    } else {
      var forward = transform.forward;
      var left = Quaternion.Euler(0, -FieldOfView/2, 0);
      var right = Quaternion.Euler(0, FieldOfView/2, 0);
      desired = Quaternion.LookRotation(InterpolateWithSine(SearchFrequency, left, right, LocalClock.Time()) * forward);
    }
    Barrel.rotation = Quaternion.RotateTowards(Barrel.rotation, desired, LocalClock.DeltaTime() * TurningSpeed);
  }

  void OnDrawGizmosSelected() {
    var Segments = 64;
    Mesh arcMesh = GenerateFieldOfViewMesh(FieldOfView, MinRange, MaxRange, Segments);
    if (arcMesh != null) {
      Gizmos.color = new Color(0f, 1f, 0f, 0.25f); // Semi-transparent green
      Gizmos.DrawMesh(arcMesh, Barrel.position, transform.rotation);
    }
  }

  Mesh GenerateFieldOfViewMesh(float angle, float innerRadius, float outerRadius, int segments) {
    Mesh mesh = new Mesh();

    // Convert angle to radians for trigonometric functions
    float halfAngleRad = Mathf.Deg2Rad * (angle / 2f);
    float segmentAngle = angle / segments; // Angle step in degrees

    // Vertex and triangle data
    int vertexCount = (segments + 1) * 2; // Each segment has 2 vertices (inner + outer)
    Vector3[] vertices = new Vector3[vertexCount];
    int[] triangles = new int[segments * 6];

    // Generate vertices
    for (int i = 0; i <= segments; i++) {
      // Angle from -FOV/2 to +FOV/2
      float currentAngleRad = Mathf.Deg2Rad * (-angle / 2f + i * segmentAngle);
      float cos = Mathf.Cos(currentAngleRad);
      float sin = Mathf.Sin(currentAngleRad);

      // Outer and inner vertices (aligned to the forward direction)
      vertices[i * 2] = new Vector3(sin * outerRadius, 0f, cos * outerRadius); // Outer vertex
      vertices[i * 2 + 1] = new Vector3(sin * innerRadius, 0f, cos * innerRadius); // Inner vertex
    }

    // Generate triangles with correct winding order (clockwise for upward-facing)
    for (int i = 0; i < segments; i++) {
      int startIndex = i * 2;

      // Outer triangle
      triangles[i * 6] = startIndex;
      triangles[i * 6 + 1] = startIndex + 2;
      triangles[i * 6 + 2] = startIndex + 1;

      // Inner triangle
      triangles[i * 6 + 3] = startIndex + 2;
      triangles[i * 6 + 4] = startIndex + 3;
      triangles[i * 6 + 5] = startIndex + 1;
    }

    // Assign vertices and triangles to the mesh
    mesh.vertices = vertices;
    mesh.triangles = triangles;
    mesh.RecalculateNormals();

    return mesh;
  }
}