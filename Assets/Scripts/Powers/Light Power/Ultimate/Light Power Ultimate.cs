using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Abilities;
using Cysharp.Threading.Tasks;
using State;
using UnityEngine;

public class LightPowerUltimate : UniTaskAbility, IAimed, ISteered {
  [SerializeField] LightPowerSettings Settings;

  RaycastHit[] RaycastHits = new RaycastHit[64];
  Dictionary<SpellAffected, int> BeamProcTargetToFrameCooldown = new();

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
    BeamProcTargetToFrameCooldown.Clear();
    try {
      var spellAffected = AbilityManager.GetComponent<SpellAffected>();
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
      chargeBeam.transform.SetParent(transform);
      var lineRenderer = chargeBeam.GetComponent<LineRenderer>();
      await Tasks.EveryFrame(Settings.UltimateChannelDuration.Ticks, LocalClock, f => {
        var maxDistance = 100;
        var ray = new Ray(sphere.transform.position, AbilityManager.transform.forward);
        var count = Physics.RaycastNonAlloc(ray, RaycastHits, maxDistance, Settings.UltimateLayerMask);
        var rayStopPosition = ray.origin + maxDistance * ray.direction;
        for (var i = 0; i < count; i++) {
          if (RaycastHits[i].collider.TryGetComponent(out SpellAffected targetAffected)) {
            BeamProcTargetToFrameCooldown.TryAdd(targetAffected, 0);
          } else {
            rayStopPosition = RaycastHits[i].point;
            break;
          }
        }

        // TODO: Fucking crazy stupid allocation... jesus christ
        var keys = BeamProcTargetToFrameCooldown.Keys.ToArray();
        foreach (var affected in keys) {
          // volatile references... gotta check they are not null
          if (affected) {
            if (BeamProcTargetToFrameCooldown[affected] <= 0) {
              var healthChangeSign = Team.SameTeam(AbilityManager, affected) ? 1 : -1;
              affected.ChangeHealth(healthChangeSign * Settings.UltimateProcHealthChange);
              BeamProcTargetToFrameCooldown[affected] = Settings.UltimateProcCooldown.Ticks;
            } else {
              BeamProcTargetToFrameCooldown[affected]--;
            }
          } else {
            BeamProcTargetToFrameCooldown.Remove(affected);
          }
        }
        lineRenderer.SetPosition(0, ray.origin);
        lineRenderer.SetPosition(1, rayStopPosition);
        spellAffected.MultiplySpeed(0);
      }, token);
      Steering = false;

    } catch (Exception e) {
      Debug.LogWarning(e.Message);
    } finally {
      sphere.Destroy();
      chargeBeam.Destroy();
      Animator.SetTrigger("Stop Hold");
      BeamProcTargetToFrameCooldown.Clear();
      Steering = false;
      // N.B. THIS IS IMPORTANT LOL. Once you remove this, the ability itself is removed and this task is canceled
      AbilityManager.GetComponent<SpellHolder>().Remove();
    }
  }
}