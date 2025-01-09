using System.Collections;
using System.Diagnostics;
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

  IEnumerator Start() {
    var nextFixedUpdate = new WaitForFixedUpdate();
    while (true) {
      for (var i = 0; i < ShotCooldown.Ticks; i+=LocalClock.DeltaFrames()) {
        yield return nextFixedUpdate;
      }
      CannonTarget target;
      while (true) {
        target = CannonManager.Instance.BestTarget(this);
        if (target) {
          Destroy(Instantiate(CannonFireExplosionPrefab, LaunchSite.position, Barrel.rotation, null), 3);
          Destroy(Instantiate(CannonBallPrefab, LaunchSite.position, Barrel.rotation, null), 5);
          CameraManager.Instance.Shake(CameraShakeIntensity);
          break;
        } else {
          yield return nextFixedUpdate;
        }
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
    Mesh arcMesh = ProceduralMesh.AnnularSector(FieldOfView, MinRange, MaxRange, Segments);
    if (arcMesh != null) {
      Gizmos.color = new Color(0f, 1f, 0f, 0.25f); // Semi-transparent green
      Gizmos.DrawMesh(arcMesh, Barrel.position, transform.rotation);
    }
  }
}