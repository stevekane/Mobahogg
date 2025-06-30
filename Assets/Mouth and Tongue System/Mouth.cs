using System.Collections;
using Melee;
using UnityEngine;

class Mouth : MonoBehaviour
{
  [Header("Prefab References")]
  [SerializeField] GameObject ShatteredClawPrefab;
  [SerializeField] ParticleSystem TongueExplosionFragmentsParticleSystem;

  [Header("Child References")]
  [SerializeField] Transform MouthModel;

  [SerializeField] Vector3 ClosedLocalMouthPosition = new Vector3(0, 0, -5);
  [SerializeField] Vector3 OpenLocalMouthPosition = new Vector3(0, 0, 0);
  [SerializeField] float ClosedLocalXRotation = 45;
  [SerializeField] float OpenLocalXRotation = 0;
  [SerializeField] Timeval ClosedDuration = Timeval.FromSeconds(5);
  [SerializeField] Timeval OpenDuration = Timeval.FromSeconds(0.5f);
  [SerializeField] Timeval OpeningDuration = Timeval.FromSeconds(0.5f);
  [SerializeField] Timeval ClosingDuration = Timeval.FromSeconds(0.1f);
  [SerializeField] Timeval FireTravelDuration = Timeval.FromTicks(3);
  [SerializeField] float PullingStrength = 1;
  [SerializeField] float ClawImpactCameraShakeIntensity = 10;
  [SerializeField] float ClawImpactVibrationIntensity = 0.25f;
  [SerializeField] float ClawImpactImpulseDistance = 1;
  [SerializeField] Timeval ClawImpactImpulseDuration = Timeval.FromTicks(10);
  [SerializeField] EasingFunctions.EasingFunctionName ClawImpactEasingFunctionName;
  [SerializeField] float TongueStrikeVibrationAmplitude = 1;
  [SerializeField] int TongueMaxHealth = 3;
  [SerializeField] Timeval TongueStrikeFlashDuration = Timeval.FromSeconds(0.25f);
  [SerializeField] float TongueStrikeImpulseDistance = 1;
  [SerializeField] EasingFunctions.EasingFunctionName TongueStrikeImpulseEasingFunction = EasingFunctions.EasingFunctionName.EaseOutCubic;

  public Sphere Sphere;
  public Tongue Tongue;
  public Claw Claw;

  public void OnTongueHurt(MeleeAttackEvent meleeAttackEvent)
  {
    Tongue.Damage(1);
    Tongue.Vibrate(TongueStrikeVibrationAmplitude);
    Claw.GetComponent<Flash>().Set(TongueStrikeFlashDuration.Ticks);
    Sphere.Impulses.Add(
      new(
        direction: -transform.forward,
        distance: 1,
        ticks: TongueStrikeFlashDuration.Ticks,
        easingFunctionName: TongueStrikeImpulseEasingFunction));
  }


  [ContextMenu("Force Close")]
  void ForceClose()
  {
    StopAllCoroutines();
    StartCoroutine(ClosingBehavior());
  }

  IEnumerator Start() {
    yield return Both(
      SlerpLocalXRotation(ClosedLocalXRotation, 1),
      LerpLocal(ClosedLocalMouthPosition, 1));
    yield return ClosedBehavior();
  }

  IEnumerator ClosedBehavior()
  {
    yield return WaitFixed(ClosedDuration);
    yield return OpeningBehavior();
  }

  IEnumerator OpenBehavior()
  {
    yield return WaitFixed(OpenDuration);
    yield return FiringBehavior();
  }

  IEnumerator OpeningBehavior()
  {
    Claw.transform.position = transform.position;
    Claw.gameObject.SetActive(true);
    yield return Both(
      SlerpLocalXRotation(OpenLocalXRotation, OpeningDuration.Ticks),
      LerpLocal(OpenLocalMouthPosition, OpeningDuration.Ticks));
    yield return OpenBehavior();
  }

  IEnumerator ClosingBehavior()
  {
    Tongue.gameObject.SetActive(false);
    Claw.gameObject.SetActive(false);
    Destroy(Instantiate(ShatteredClawPrefab, Claw.transform.position, Claw.transform.rotation), 3);
    yield return Both(
      SlerpLocalXRotation(ClosedLocalXRotation, ClosingDuration.Ticks),
      LerpLocal(ClosedLocalMouthPosition, ClosingDuration.Ticks));
    yield return ClosedBehavior();
  }

  IEnumerator FiringBehavior() {
    Claw.GetComponent<Flash>().TurnOff();
    Tongue.SetHealth(TongueMaxHealth);
    Tongue.gameObject.SetActive(true);
    var initialPosition = Claw.transform.position;
    for (var i = 0; i < FireTravelDuration.Ticks; i++)
    {
      var interpolant = (float)i / (FireTravelDuration.Ticks - 1);
      var targetPosition = Sphere.transform.position;
      Claw.transform.position = Vector3.Lerp(initialPosition, targetPosition, interpolant);
      yield return new WaitForFixedUpdate();
    }
    Claw.transform.SetParent(Sphere.transform, worldPositionStays: true);

    // FX
    var impactPosition = Claw.transform.position - Claw.transform.forward * Sphere.Radius;
    var impactVFX = Instantiate(Sphere.ImpactVFXPrefab, impactPosition, Quaternion.identity);
    Destroy(impactVFX.gameObject, 3);
    CameraManager.Instance.Shake(ClawImpactCameraShakeIntensity);
    var sphereImpulse = new SphereImpulse(
      transform.forward,
      ClawImpactImpulseDistance,
      ClawImpactImpulseDuration.Ticks,
      ClawImpactEasingFunctionName);
    Sphere.Impulses.Add(sphereImpulse);
    Sphere.GetComponent<Vibrator>().StartVibrate(Claw.transform.forward, 20, ClawImpactVibrationIntensity, 20);
    Sphere.GetComponent<Flash>().Set(20);
    Claw.GetComponent<Vibrator>().StartVibrate(Claw.transform.forward, 20, ClawImpactVibrationIntensity, 20);
    yield return PullingBehavior();
  }

  IEnumerator PullingBehavior() {
    IEnumerator Pull()
    {
      while (true)
      {
        Sphere.DirectVelocity -= PullingStrength * (Sphere.transform.position - transform.position).XZ().normalized;
        yield return new WaitForFixedUpdate();
      }
    }
    IEnumerator TongueDied()
    {
      yield return new WaitUntil(() => Tongue.IsDead);
      var explosionVFX = Instantiate(TongueExplosionFragmentsParticleSystem, transform);
      var moveTo = explosionVFX.GetComponent<TongueExplosionVFX>();
      var clawAttachmentPoint = Claw.transform.position-Claw.Length*Claw.transform.forward;
      moveTo.transform.position = clawAttachmentPoint;
      moveTo.Destination = transform.position;
    }
    yield return Either(Pull(), TongueDied());
    yield return ClosingBehavior();
  }

  IEnumerator WaitFixed(Timeval timeval) {
    for (var i = 0; i < timeval.Ticks; i++)
    {
      yield return new WaitForFixedUpdate();
    }
  }

  IEnumerator SlerpLocalXRotation(float targetDegrees, int ticks)
  {
    var initialRotation = MouthModel.localRotation;
    var targetRotation = Quaternion.Euler(targetDegrees, MouthModel.localEulerAngles.y, MouthModel.localEulerAngles.z);
    for (int i = 0; i < ticks; i++)
    {
      float t = (i + 1f) / ticks;
      MouthModel.localRotation = Quaternion.Slerp(initialRotation, targetRotation, t);
      yield return new WaitForFixedUpdate();
    }
    MouthModel.localRotation = targetRotation;
  }

  IEnumerator LerpLocal(Vector3 localPosition, int ticks)
  {
    var initialPosition = MouthModel.localPosition;
    for (var i = 0; i < ticks; i++)
    {
      float t = (i + 1f) / ticks;
      MouthModel.localPosition = Vector3.Slerp(initialPosition, localPosition, t);
      yield return new WaitForFixedUpdate();
    }
    MouthModel.localPosition = localPosition;
  }

  IEnumerator Both(IEnumerator a, IEnumerator b)
  {
    var aDone = false;
    var bDone = false;
    IEnumerator AWrapper() {
      yield return a;
      aDone = true;
    }
    IEnumerator BWrapper() {
      yield return b;
      bDone = true;
    }
    StartCoroutine(AWrapper());
    StartCoroutine(BWrapper());
    yield return new WaitUntil(() => aDone && bDone);
  }

  IEnumerator Either(IEnumerator a, IEnumerator b)
  {
    var aDone = false;
    var bDone = false;
    IEnumerator AWrapper() {
      yield return a;
      aDone = true;
    }
    IEnumerator BWrapper() {
      yield return b;
      bDone = true;
    }
    var aRoutine = StartCoroutine(AWrapper());
    var bRoutine = StartCoroutine(BWrapper());
    yield return new WaitUntil(() => aDone || bDone);
    StopCoroutine(aRoutine);
    StopCoroutine(bRoutine);
  }

  void LateUpdate()
  {
    var clawAttachmentPoint = Claw.transform.position-Claw.Length*Claw.transform.forward;
    Tongue.SetTongueEnd(clawAttachmentPoint);
  }
}