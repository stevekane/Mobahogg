using System.Collections;
using Melee;
using UnityEngine;

public class Mouth : MonoBehaviour
{
  [Header("Prefab References")]
  public GameObject ShatteredClawPrefab;
  public ParticleSystem TongueExplosionFragmentsParticleSystem;
  public LiveWireShockEffect LiveWireShockEffectPrefab;

  [Header("Child References")]
  public Transform MouthModel;
  public Sphere Sphere;
  public Tongue Tongue;
  public Claw Claw;

  [Header("Mouth Behavior")]
  public float ClosedLocalXRotation = 45;
  public float OpenLocalXRotation = 0;
  public float PullingStrength = 1;
  public Timeval ClosedDuration = Timeval.FromSeconds(5);
  public Timeval OpenDuration = Timeval.FromSeconds(0.5f);
  public Timeval OpeningDuration = Timeval.FromSeconds(0.5f);
  public Timeval ClosingDuration = Timeval.FromSeconds(0.1f);
  public Timeval FireTravelDuration = Timeval.FromTicks(3);

  [Header("Claw Impact")]
  public float ClawImpactCameraShakeIntensity = 10;
  public float ClawImpactVibrationIntensity = 0.25f;
  public float ClawImpactTongueVibrationIntensity = 0.5f;
  public float ClawImpactImpulseDistance = 1;
  public Timeval ClawImpactFlashDuration = Timeval.FromMillis(500);
  public Timeval ClawImpactImpulseDuration = Timeval.FromTicks(10);
  public EasingFunctions.EasingFunctionName ClawImpactEasingFunctionName;

  [Header("Tongue Strike")]
  public int TongueMaxHealth = 3;
  public float TongueStrikeVibrationAmplitude = 1;
  public float TongueStrikeImpulseDistance = 1;
  public Timeval TongueStrikeFlashDuration = Timeval.FromSeconds(0.25f);
  public EasingFunctions.EasingFunctionName TongueStrikeImpulseEasingFunction = EasingFunctions.EasingFunctionName.EaseOutCubic;

  [Header("Tongue Healing")]
  int TongueHealth = 3;
  public float TongueDeathCameraShakeIntensity = 25;
  public Timeval TongueDeathDuration = Timeval.FromSeconds(0.2f);
  public Timeval TimeSinceLastDamageBeforeHealing = Timeval.FromSeconds(2);
  public Timeval HealTickPeriod = Timeval.FromSeconds(0.5f);

  public void OnTongueHurt(MeleeAttackEvent meleeAttackEvent)
  {
    var effectManager = meleeAttackEvent.Attacker.GetComponent<EffectManager>();
    var shockEffect = Instantiate(LiveWireShockEffectPrefab);
    shockEffect.EffectManager = effectManager;
    shockEffect.SpawnPlasmaArc = TongueHealth > 1;
    shockEffect.ContactPoint = meleeAttackEvent.EstimatedContactPoint;
    effectManager.Register(shockEffect);
    Tongue.Vibrate(TongueStrikeVibrationAmplitude);
    Claw.GetComponent<Flash>().Set(TongueStrikeFlashDuration.Ticks);
    Sphere.Impulses.Add(
      new(
        direction: TongueHealth == 1 ? transform.forward : -transform.forward,
        distance: 1,
        ticks: TongueStrikeFlashDuration.Ticks,
        easingFunctionName: TongueStrikeImpulseEasingFunction));
    TongueHealth -= 1;
  }

  IEnumerator Start() {
    yield return MouthModel.transform.SlerpLocalEulerX(ClosedLocalXRotation, 1);
    StartCoroutine(ClosedBehavior());
  }

  IEnumerator ClosedBehavior()
  {
    yield return CoroutineDelay.WaitFixed(ClosedDuration);
    StartCoroutine(OpeningBehavior());
  }

  IEnumerator OpenBehavior()
  {
    yield return CoroutineDelay.WaitFixed(OpenDuration);
    StartCoroutine(FiringBehavior());
  }

  IEnumerator OpeningBehavior()
  {
    TongueHealth = TongueMaxHealth;
    Tongue.gameObject.SetActive(true);
    Claw.transform.position = transform.position;
    Claw.gameObject.SetActive(true);
    yield return MouthModel.transform.SlerpLocalEulerX(OpenLocalXRotation, OpeningDuration.Ticks);
    StartCoroutine(OpenBehavior());
  }

  IEnumerator ClosingBehavior()
  {
    yield return MouthModel.transform.SlerpLocalEulerX(ClosedLocalXRotation, ClosingDuration.Ticks);
    StartCoroutine(ClosedBehavior());
  }

  IEnumerator FiringBehavior() {
    var initialPosition = Claw.transform.position;
    for (var i = 0; i < FireTravelDuration.Ticks; i++)
    {
      var interpolant = (float)i / (FireTravelDuration.Ticks - 1);
      var targetPosition = Sphere.transform.position;
      Claw.transform.position = Vector3.Lerp(initialPosition, targetPosition, interpolant);
      yield return new WaitForFixedUpdate();
    }
    Claw.transform.SetParent(Sphere.transform, worldPositionStays: true);
    var sphereImpulse = new SphereImpulse(
      transform.forward,
      ClawImpactImpulseDistance,
      ClawImpactImpulseDuration.Ticks,
      ClawImpactEasingFunctionName);
    Sphere.Impulses.Add(sphereImpulse);

    // FX
    var impactPosition = Claw.transform.position - Claw.transform.forward * Sphere.Radius;
    var impactVFX = Instantiate(Sphere.ImpactVFXPrefab, impactPosition, Quaternion.identity);
    Destroy(impactVFX.gameObject, 3);
    CameraManager.Instance.Shake(ClawImpactCameraShakeIntensity);
    Sphere.GetComponent<Vibrator>().StartVibrate(
      Claw.transform.forward,
      ClawImpactFlashDuration.Ticks,
      ClawImpactVibrationIntensity,
      20);
    Sphere.GetComponent<Flash>().Set(20);
    // TODO: Get rid of this by sending event to manager object that knows about all mouths
    foreach (var mouth in FindObjectsByType<Mouth>(FindObjectsSortMode.None))
    {
      mouth.Claw.GetComponent<Flash>().Set(ClawImpactFlashDuration.Ticks);
      mouth.Claw.GetComponent<Vibrator>().StartVibrate(
        Claw.transform.forward,
        ClawImpactFlashDuration.Ticks,
        ClawImpactVibrationIntensity,
        20);
      mouth.Tongue.Vibrate(ClawImpactTongueVibrationIntensity);
    }
    StartCoroutine(PullingBehavior());
  }

  IEnumerator PullingBehavior() {
    IEnumerator Pull()
    {
      while (true)
      {
        var sdfBlackhole = Sphere.GetComponentInChildren<SDFSphere>();
        Claw.transform.position = Sphere.transform.position - sdfBlackhole.Radius * Claw.transform.forward;
        Sphere.DirectVelocity -= PullingStrength * (Sphere.transform.position - transform.position).XZ().normalized;
        yield return new WaitForFixedUpdate();
      }
    }
    IEnumerator TongueDied()
    {
      yield return new WaitUntil(() => TongueHealth <= 0);
      var explosionVFX = Instantiate(TongueExplosionFragmentsParticleSystem, transform);
      var moveTo = explosionVFX.GetComponent<TongueExplosionVFX>();
      var clawAttachmentPoint = Claw.transform.position-Claw.Length*Claw.transform.forward;
      moveTo.transform.position = clawAttachmentPoint;
      moveTo.Destination = transform.position;
      Destroy(Instantiate(ShatteredClawPrefab, Claw.transform.position, Claw.transform.rotation), 3);
      CameraManager.Instance.Shake(TongueDeathCameraShakeIntensity);
      yield return CoroutineDelay.WaitFixed(TongueDeathDuration);
      Tongue.gameObject.SetActive(false);
      Claw.gameObject.SetActive(false);
      Claw.GetComponent<Flash>().TurnOff();
    }
    yield return this.FirstCoroutine(Pull(), TongueDied());
    StartCoroutine(ClosingBehavior());
  }

  // TODO: Sketchy. This is largely visual but also has gameplay effects including
  // setting the size of the collider
  void LateUpdate()
  {
    var clawAttachmentPoint = Claw.transform.position-Claw.Length*Claw.transform.forward;
    Tongue.SetTongueEnd(clawAttachmentPoint);
  }
}