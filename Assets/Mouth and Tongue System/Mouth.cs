using System.Collections;
using UnityEngine;

class Mouth : MonoBehaviour
{
  [SerializeField] Timeval FireTravelDuration = Timeval.FromTicks(3);
  [SerializeField] float ClawImpactCameraShakeIntensity = 10;
  [SerializeField] float ClawImpactVibrationIntensity = 0.25f;

  public Tongue Tongue;
  public Claw Claw;

  public void Fire(Sphere sphere)
  {
    StopAllCoroutines();
    StartCoroutine(FireClaw(sphere));
  }

  IEnumerator FireClaw(Sphere sphere)
  {
    var initialPosition = Claw.transform.position;
    for (var i = 0; i < FireTravelDuration.Ticks; i++)
    {
      var interpolant = (float)i / (FireTravelDuration.Ticks - 1);
      var targetPosition = sphere.transform.position;
      Claw.transform.position = Vector3.Lerp(initialPosition, targetPosition, interpolant);
      yield return new WaitForFixedUpdate();
    }
    Claw.transform.SetParent(sphere.transform, worldPositionStays: true);

    // FX
    var impactPosition = Claw.transform.position - Claw.transform.forward * sphere.Radius;
    var impactVFX = Instantiate(sphere.ImpactVFXPrefab, impactPosition, Quaternion.identity);
    Destroy(impactVFX.gameObject, 3);

    // Camera
    CameraManager.Instance.Shake(ClawImpactCameraShakeIntensity);
    sphere.GetComponent<Vibrator>().StartVibrate(Claw.transform.forward, 60, ClawImpactVibrationIntensity, 20);
    sphere.GetComponent<Flash>().Set(60);
  }

  void LateUpdate()
  {
    var clawAttachmentPoint = Claw.transform.position-Claw.Length*Claw.transform.forward;
    Tongue.SetTongueEnd(clawAttachmentPoint);
  }
}