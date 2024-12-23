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

  Quaternion Left;
  Quaternion Right;
  Quaternion Desired;
  Vector3 SearchForward;

  public static Quaternion InterpolateWithSine(float frequency, Quaternion start, Quaternion end, float time) {
    float sineValue = 0.5f * (1 + Mathf.Sin(2 * Mathf.PI * frequency * time));
    return Quaternion.Slerp(start, end, sineValue);
  }

  IEnumerator Start() {
    SearchForward = transform.forward;
    Desired = transform.rotation;
    Left = Quaternion.Euler(0, -45, 0);
    Right = Quaternion.Euler(0, 45, 0);
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
    if (target) {
      var toTarget = target.transform.position-transform.position;
      var direction = toTarget.XZ().normalized;
      Desired = Quaternion.LookRotation(direction);
    } else {
      Desired = Quaternion.LookRotation(InterpolateWithSine(SearchFrequency, Left, Right, LocalClock.Time()) * SearchForward);
    }
    Barrel.rotation = Quaternion.RotateTowards(Barrel.rotation, Desired, LocalClock.DeltaTime() * TurningSpeed);
  }
}