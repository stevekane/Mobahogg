using System;
using System.Threading;
using Abilities;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.VFX;

public class EarthSpellActive : UniTaskAbility, IAimed {
  [SerializeField] EarthSpellSettings Settings;
  [SerializeField] VisualEffect SpikesVisualEffect;

  int Frame;
  SpellAffected SpellAffected;
  Vibrator Vibrator;
  HitStop HitStop;

  void Start() {
    Frame = Settings.ActiveAnimationMontage.FrameDuration;
    SpellAffected = AbilityManager.LocateComponent<SpellAffected>();
    Vibrator = AbilityManager.LocateComponent<Vibrator>();
    HitStop = AbilityManager.LocateComponent<HitStop>();
  }

  public bool CanAim => Frame == 0;
  public void Aim(Vector2 input) {
    var direction = input.XZ();
    if (direction.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction));
    }
  }

  void OnNotifyStart(AnimationNotify notify) {
    if (notify.Name == "Slam") {
      OnSlam();
    }
    if (notify.Name == "Root Motion") {
      OnStartRootMotion();
    }
  }

  void OnNotifyUpdate(AnimationNotify notify) {
  }

  void OnNotifyEnd(AnimationNotify notify) {
    if (notify.Name == "Root Motion") {
      OnStopRootMotion();
    }
  }

  void OnSlam() {
    CameraManager.Instance.Shake(Settings.ActiveCameraShakeIntensity);
    Vibrator.StartVibrate(Vector3.up, Settings.ActiveSlamHitStop.Ticks, 0.125f, 20);
    HitStop.FramesRemaining = Settings.ActiveSlamHitStop.Ticks;
    foreach (var player in LivesManager.Active.Players) {
      var delta = player.transform.position - AbilityManager.transform.position;
      if (delta.magnitude < Settings.ActiveKnockbackRadius && player.gameObject != AbilityManager.gameObject) {
        var direction = delta.XZ().normalized;
        if (player.TryGetComponent(out SpellAffected targetSpellAffected)) {
          var spikesEffect = Instantiate(Settings.ActiveSpawnSpikesEffectPrefab);
          spikesEffect.SetVector3("Path_start", AbilityManager.transform.position+2*AbilityManager.transform.forward);
          spikesEffect.SetVector3("Path_end", player.transform.position);
          spikesEffect.SendEvent("SpawnSpikes");
          targetSpellAffected.Knockback(Settings.ActiveKnockbackStrength * direction);
        }
      }
    }
  }

  void OnStartRootMotion() {
    AnimatorCallbackHandler.OnRootMotion.Listen(OnAnimatorMove);
  }
  void OnStopRootMotion() {
    AnimatorCallbackHandler.OnRootMotion.Unlisten(OnAnimatorMove);
  }
  void OnAnimatorMove() {
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(Settings.ActiveRootMotionMultiplier * v);
  }

  public override bool IsRunning => Frame < Settings.ActiveAnimationMontage.FrameDuration;
  public override bool CanRun => true;
  public override bool CanCancel => false;

  protected override async UniTask Task(CancellationToken token) {
    try {
      Frame = 0;
      Animator.SetTrigger("Earth Active");
      await Tasks.EveryFrame(Settings.ActiveAnimationMontage.FrameDuration, LocalClock, token, f => {
        Frame = f;
        foreach (var notify in Settings.ActiveAnimationMontage.Notifies) {
          if (notify.StartFrame == Frame) {
            OnNotifyStart(notify);
          }
          if (notify.EndFrame == Frame) {
            OnNotifyEnd(notify);
          }
          if (Frame > notify.StartFrame && Frame < notify.EndFrame) {
            OnNotifyUpdate(notify);
          }
        }
        SpellAffected.MultiplySpeed(0);
      });
    } catch (Exception e) {
      foreach (var notify in Settings.ActiveAnimationMontage.Notifies) {
        // Cleanup running notifies... is this really the same as "end"? maybe should be own thing?
        if (Frame >= notify.StartFrame && Frame <= notify.EndFrame) {
          OnNotifyEnd(notify);
        }
      }
    } finally {
      Frame = Settings.ActiveAnimationMontage.FrameDuration;
      Animator.SetTrigger("Stop Hold");
    }
  }
}