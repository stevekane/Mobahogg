using System;
using System.Threading;
using Abilities;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class LightPowerUltimate : Ability, IAimed, ISteered {
  [SerializeField] LightPowerSettings Settings;
  [SerializeField] float TurnSpeed = 30;

  CancellationTokenSource CancellationTokenSource;

  public override bool CanRun => true;
  public override void Run() {
    CancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(this.destroyCancellationToken);
    AbilityTask(CancellationTokenSource.Token).Forget();
  }

  bool IsActive;
  public override bool IsRunning => IsActive;
  public override bool CanCancel => false;
  public override void Cancel() {
    if (CancellationTokenSource != null && !CancellationTokenSource.IsCancellationRequested) {
      CancellationTokenSource.Cancel();
      CancellationTokenSource.Dispose();
    }
  }
  public bool CanAim => CanRun;
  public void Aim(Vector2 direction) {
    var forward = direction.XZ();
    if (forward.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(forward.normalized));
    }
  }

  bool Steering;
  public bool CanSteer => Steering;
  public void Steer(Vector2 input) {
    if (input.sqrMagnitude > 0) {
      var desiredForward = input.XZ().normalized;
      var currentForward = CharacterController.Rotation.Forward.XZ();
      var currentRotation = Quaternion.LookRotation(currentForward);
      var desiredRotation = Quaternion.LookRotation(desiredForward);
      var maxDegrees = TurnSpeed * LocalClock.DeltaTime();
      var nextRotation = Quaternion.RotateTowards(currentRotation, desiredRotation, maxDegrees);
      CharacterController.Rotation.Set(nextRotation);
    }
  }

  async UniTask AbilityTask(CancellationToken token) {
    GameObject sphere = null;
    GameObject chargeBeam = null;
    try {
      var spellAffected = AbilityManager.GetComponent<SpellAffected>();
      IsActive = true;
      Animator.SetTrigger("Light Ultimate");
      sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphere.transform.SetPositionAndRotation(
        AbilityManager.transform.position + AbilityManager.transform.forward + Vector3.up,
        AbilityManager.transform.rotation);
      sphere.transform.SetParent(transform, true);
      Destroy(sphere.GetComponent<Collider>());
      sphere.GetComponent<MeshRenderer>().sharedMaterial = Settings.ChargeSphereMaterial;

      await Tasks.EveryFrame(Settings.UltimateChargeDuration.Ticks, LocalClock, f => {
        sphere.transform.localScale = 2*(float)f/Settings.UltimateChargeDuration.Ticks * Vector3.one;
        spellAffected.MultiplySpeed(0);
      }, token);

      Steering = true;
      chargeBeam = Instantiate(Settings.UltimateChargeBeamPrefab);
      var lineRenderer = chargeBeam.GetComponent<LineRenderer>();
      chargeBeam.transform.SetParent(transform);
      await Tasks.EveryFrame(Settings.UltimateChannelDuration.Ticks, LocalClock, f => {
        spellAffected.MultiplySpeed(0);
        lineRenderer.SetPosition(0, sphere.transform.position);
        lineRenderer.SetPosition(1, sphere.transform.position + 100*AbilityManager.transform.forward);
      }, token);
      Steering = false;

    } catch (Exception e) {
      Debug.LogWarning(e.Message);
    } finally {
      sphere.Destroy();
      chargeBeam.Destroy();
      Animator.SetTrigger("Stop Hold");
      Steering = false;
      IsActive = false;
      // N.B. THIS IS IMPORTANT LOL. Once you remove this, the ability itself is removed and this task is canceled
      AbilityManager.GetComponent<SpellHolder>().Remove();
    }
  }
}