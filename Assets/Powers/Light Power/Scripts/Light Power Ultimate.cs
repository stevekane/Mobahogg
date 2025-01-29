using System;
using System.Collections.Generic;
using System.Threading;
using Abilities;
using Cysharp.Threading.Tasks;
using State;
using UnityEngine;

public class LightPowerUltimate : UniTaskAbility, IAimed, ISteered {
  [SerializeField] LightPowerSettings Settings;

  RaycastHit[] RaycastHits = new RaycastHit[64];
  Dictionary<SpellAffected, int> BeamTargetToNextProcFrame = new();

  public override bool CanRun => true;
  public override bool CanCancel => false;

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
      var maxDegrees = Settings.UltimateTurnSpeed * LocalClock.DeltaTime();
      var nextRotation = Quaternion.RotateTowards(currentRotation, desiredRotation, maxDegrees);
      CharacterController.Rotation.Set(nextRotation);
    }
  }

  protected override async UniTask Task(CancellationToken token) {
    GameObject sphere = null;
    GameObject chargeBeam = null;
    BeamTargetToNextProcFrame.Clear();
    try {
      var spellAffected = AbilityManager.GetComponent<SpellAffected>();
      Animator.SetTrigger("Light Ultimate");
      sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      sphere.transform.SetPositionAndRotation(
        AbilityManager.transform.position + AbilityManager.transform.forward + Vector3.up,
        AbilityManager.transform.rotation);
      sphere.transform.SetParent(transform, true);
      sphere.GetComponent<MeshRenderer>().sharedMaterial = Settings.ChargeSphereMaterial;
      Destroy(sphere.GetComponent<Collider>());

      await Tasks.EveryFrame(Settings.UltimateChargeDuration.Ticks, LocalClock, f => {
        sphere.transform.localScale = 2*(float)f/Settings.UltimateChargeDuration.Ticks * Vector3.one;
        spellAffected.MultiplySpeed(0);
      }, token);

      Steering = true;
      chargeBeam = Instantiate(
        Settings.UltimateChargeBeamPrefab,
        sphere.transform.position,
        Quaternion.LookRotation(AbilityManager.transform.forward),
        transform);

      bool StopsBeam(RaycastHit hit) => !hit.collider.GetComponent<SpellAffected>();
      await Tasks.EveryFrame(Settings.UltimateChannelDuration.Ticks, LocalClock, f => {
        var maxDistance = 100f;
        var ray = new Ray(sphere.transform.position, AbilityManager.transform.forward);
        var count = Physics.RaycastNonAlloc(ray, RaycastHits, maxDistance, Settings.UltimateLayerMask);

        var rayDistance = maxDistance;
        for (var i = 0; i < count; i++) {
          var hit = RaycastHits[i];
          if (StopsBeam(hit)) {
            rayDistance = Mathf.Min(hit.distance, rayDistance);
          }
        }
        var frame = LocalClock.FixedFrame();
        var cooldownFrames = Settings.UltimateProcCooldown.Ticks;
        var healthChange = Settings.UltimateProcHealthChange;
        for (var i = 0; i < count; i++) {
          var hit = RaycastHits[i];
          if (hit.distance <= rayDistance && hit.collider.TryGetComponent(out SpellAffected targetAffected)) {
            var nextProcFrame = BeamTargetToNextProcFrame.GetOrAdd(targetAffected, frame);
            if (nextProcFrame <= frame) {
              var sign = Team.SameTeam(targetAffected, AbilityManager) ? 1 : -1;
              targetAffected.ChangeHealth(sign * healthChange);
              BeamTargetToNextProcFrame[targetAffected] = frame + cooldownFrames;
            }
          }
        }
        spellAffected.MultiplySpeed(0);
      }, token);
      Steering = false;
    } catch (Exception e) {
      Debug.LogWarning(e.Message);
    } finally {
      sphere.Destroy();
      chargeBeam.Destroy();
      if (Animator)
        Animator.SetTrigger("Stop Hold");
      BeamTargetToNextProcFrame.Clear();
      Steering = false;
      // N.B. THIS IS IMPORTANT LOL. Once you remove this, the ability itself is removed and this task is canceled
      if (AbilityManager)
        AbilityManager.GetComponent<SpellHolder>().Remove();
    }
  }
}