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
  public Vector3 ClosedLocalMouthPosition = new Vector3(0, 0, -5);
  public Vector3 OpenLocalMouthPosition = new Vector3(0, 0, 0);
  public float ClosedLocalXRotation = 45;
  public float OpenLocalXRotation = 0;
  public Timeval ClosedDuration = Timeval.FromSeconds(5);
  public Timeval OpenDuration = Timeval.FromSeconds(0.5f);
  public Timeval OpeningDuration = Timeval.FromSeconds(0.5f);
  public Timeval ClosingDuration = Timeval.FromSeconds(0.1f);
  public Timeval FireTravelDuration = Timeval.FromTicks(3);
  public float PullingStrength = 1;
  [Header("Claw Impact")]
  public float ClawImpactCameraShakeIntensity = 10;
  public float ClawImpactVibrationIntensity = 0.25f;
  public float ClawImpactImpulseDistance = 1;
  public Timeval ClawImpactImpulseDuration = Timeval.FromTicks(10);
  public EasingFunctions.EasingFunctionName ClawImpactEasingFunctionName;
  [Header("Tongue Strike")]
  public float TongueStrikeVibrationAmplitude = 1;
  public int TongueMaxHealth = 3;
  public Timeval TongueStrikeFlashDuration = Timeval.FromSeconds(0.25f);
  public float TongueStrikeImpulseDistance = 1;
  public EasingFunctions.EasingFunctionName TongueStrikeImpulseEasingFunction = EasingFunctions.EasingFunctionName.EaseOutCubic;
  [Header("Tongue Healing")]
  int TongueHealth = 3;
  public Timeval TongueDeathDuration = Timeval.FromSeconds(0.2f);
  public float TongueDeathCameraShakeIntensity = 25;
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
    yield return this.BothCoroutines(
      MouthModel.transform.SlerpLocalEulerX(ClosedLocalXRotation, 1),
      MouthModel.transform.LerpLocal(ClosedLocalMouthPosition, 1));
    yield return ClosedBehavior();
  }

  IEnumerator ClosedBehavior()
  {
    yield return CoroutineDelay.WaitFixed(ClosedDuration);
    yield return OpeningBehavior();
  }

  IEnumerator OpenBehavior()
  {
    yield return CoroutineDelay.WaitFixed(OpenDuration);
    yield return FiringBehavior();
  }

  IEnumerator OpeningBehavior()
  {
    TongueHealth = TongueMaxHealth;
    Tongue.gameObject.SetActive(true);
    Claw.transform.position = transform.position;
    Claw.gameObject.SetActive(true);
    yield return this.BothCoroutines(
      MouthModel.transform.SlerpLocalEulerX(OpenLocalXRotation, OpeningDuration.Ticks),
      MouthModel.transform.LerpLocal(OpenLocalMouthPosition, OpeningDuration.Ticks));
    yield return OpenBehavior();
  }

  IEnumerator ClosingBehavior()
  {
    yield return this.BothCoroutines(
      MouthModel.transform.SlerpLocalEulerX(ClosedLocalXRotation, ClosingDuration.Ticks),
      MouthModel.transform.LerpLocal(ClosedLocalMouthPosition, ClosingDuration.Ticks));
    yield return ClosedBehavior();
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
    yield return ClosingBehavior();
  }

  void LateUpdate()
  {
    var clawAttachmentPoint = Claw.transform.position-Claw.Length*Claw.transform.forward;
    Tongue.SetTongueEnd(clawAttachmentPoint);
  }
}