using System.Collections.Generic;
using UnityEngine;
using AimAssist;
using Abilities;
using System;
using UnityEngine.VFX;

[Serializable]
public class RootMotionBehavior : FrameBehavior {
  public float Multiplier;
}

[Serializable]
public class AimAssistBehavior : FrameBehavior {
  public float TurnSpeed = 90;
  [InlineEditor] public AimAssistQuery AimAssistQuery;
}

[Serializable]
public class WeaponAimBehavior : FrameBehavior {
  public Vector3 Direction;
}

[Serializable]
public class HitboxBehavior : FrameBehavior {
}

[Serializable]
public class AudioOneShotBehavior : FrameBehavior {
  public AudioClip AudioClip;
}

[Serializable]
public class AnimatorControllerCrossFadeBehavior : FrameBehavior {
  // TODO: Very stupid names. Should be StartStateName EndStateName
  public string StateName;
  public string BaseName;
  public int LayerIndex;
  public float CrossFadeDuration = 0.25f;
  public override string Name => string.IsNullOrEmpty(StateName) ? base.Name : $"AnimatorState({StateName})";
}

[Serializable]
public class VisualEffectBehavior : FrameBehavior {
  public string StartEventName = "OnPlay";
  public string UpdateEventName = "";
  public string EndEventName = "";
}

[Serializable]
public class CancelBehavior : FrameBehavior {
}

public class AttackAbility : Ability {
  [SerializeField] RootMotionBehavior RootMotionBehavior;
  [SerializeField] AimAssistBehavior AimAssistBehavior;
  [SerializeField] WeaponAimBehavior WeaponAimBehavior;
  [SerializeField] HitboxBehavior HitboxBehavior;
  [SerializeField] AudioOneShotBehavior AudioOneShotBehavior;
  [SerializeField] AnimatorControllerCrossFadeBehavior CrossFadeStateBehavior;
  [SerializeField] VisualEffectBehavior VisualEffectBehavior;
  [SerializeField] CancelBehavior CancelBehavior;
  [SerializeField, Min(0)] int EndFrame = 24;
  [SerializeField, Min(0)] int Frame;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;
  [SerializeField] VisualEffect VisualEffect;
  [SerializeField] AudioSource AudioSource;

  bool Cancellable;
  WeaponAim WeaponAim;
  List<Combatant> Struck = new(16);

  void Awake() {
    Frame = EndFrame;
    Hitbox.CollisionEnabled = false;
    Struck.Clear();
  }

  void Start() {
    WeaponAim = AbilityManager.LocateComponent<WeaponAim>();
  }

  void OnAnimatorMove() {
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(RootMotionBehavior.Multiplier * v);
  }

  public bool ShouldHit(Combatant combatant) => !Struck.Contains(combatant);

  public void Hit(Combatant combatant) => Struck.Add(combatant);

  public override bool IsRunning => Frame <= EndFrame;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Frame = 0;
  }

  public bool CanAim => Frame == 0;
  public void Aim(Vector2 direction) {
    if (direction.sqrMagnitude > 0)
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
  }

  public override bool CanCancel => Cancellable;
  public override void Cancel() {
    CancelActiveBehaviors();
    Frame = EndFrame+1;
  }

  void FixedUpdate() {
    if (IsRunning) {
      StartBehaviors();
      UpdateBehaviors();
      EndBehaviors();
      Frame++;
    }
  }

  void StartBehaviors() {
    if (RootMotionBehavior.Starting(Frame))
      AnimatorCallbackHandler.OnRootMotion.Listen(OnAnimatorMove);
    if (AimAssistBehavior.Starting(Frame)) {}
    if (WeaponAimBehavior.Starting(Frame))
      WeaponAim.AimDirection = WeaponAimBehavior.Direction;
    if (HitboxBehavior.Starting(Frame)) {
      Hitbox.CollisionEnabled = true;
      Struck.Clear();
    }
    if (AudioOneShotBehavior.Starting(Frame))
      AudioSource.PlayOptionalOneShot(AudioOneShotBehavior.AudioClip);
    if (CrossFadeStateBehavior.Starting(Frame)) {
      Animator.CrossFadeInFixedTime(
        CrossFadeStateBehavior.StateName,
        CrossFadeStateBehavior.CrossFadeDuration,
        CrossFadeStateBehavior.LayerIndex);
    }
    if (VisualEffectBehavior.Starting(Frame))
      VisualEffect.PlayNonEmptyEvent(VisualEffectBehavior.StartEventName);
    if (CancelBehavior.Starting(Frame))
      Cancellable = true;
  }

  void EndBehaviors() {
    if (RootMotionBehavior.Ending(Frame))
      AnimatorCallbackHandler.OnRootMotion.Unlisten(OnAnimatorMove);
    if (AimAssistBehavior.Ending(Frame)) {}
    if (WeaponAimBehavior.Ending(Frame))
      WeaponAim.AimDirection = null;
    if (HitboxBehavior.Ending(Frame)) {
      Hitbox.CollisionEnabled = false;
      Struck.Clear();
    }
    if (AudioOneShotBehavior.Ending(Frame)) {}
    if (CrossFadeStateBehavior.Ending(Frame)) {
      Animator.Play(CrossFadeStateBehavior.BaseName, CrossFadeStateBehavior.LayerIndex);
    }
    if (VisualEffectBehavior.Ending(Frame))
      VisualEffect.PlayNonEmptyEvent(VisualEffectBehavior.EndEventName);
    if (CancelBehavior.Ending(Frame))
      Cancellable = false;
  }

  void UpdateBehaviors() {
    if (RootMotionBehavior.Active(Frame)) {}
    if (AimAssistBehavior.Active(Frame)) {
      var bestTarget = AimAssistManager.Instance.BestTarget(transform, AimAssistBehavior.AimAssistQuery);
      if (bestTarget) {
        var direction = bestTarget.transform.position-transform.position;
        var maxDegrees = AimAssistBehavior.TurnSpeed * LocalClock.DeltaTime();
        var nextRotation = Quaternion.RotateTowards(
          CharacterController.transform.rotation,
          Quaternion.LookRotation(direction),
          maxDegrees);
        CharacterController.Rotation.Set(nextRotation);
      }
    }
    if (WeaponAimBehavior.Active(Frame)) {}
    if (HitboxBehavior.Active(Frame)) {}
    if (AudioOneShotBehavior.Active(Frame)) {}
    if (CrossFadeStateBehavior.Active(Frame)) {}
    if (VisualEffectBehavior.Active(Frame))
      VisualEffect.PlayNonEmptyEvent(VisualEffectBehavior.UpdateEventName);
    if (CancelBehavior.Active(Frame)) {}
  }

  void CancelActiveBehaviors() {
    if (RootMotionBehavior.Active(Frame))
      AnimatorCallbackHandler.OnRootMotion.Unlisten(OnAnimatorMove);
    if (AimAssistBehavior.Active(Frame)) {}
    if (WeaponAimBehavior.Active(Frame))
      WeaponAim.AimDirection = null;
    if (HitboxBehavior.Active(Frame)) {
      Struck.Clear();
      Hitbox.CollisionEnabled = false;
    }
    if (AudioOneShotBehavior.Active(Frame)) {}
    if (CrossFadeStateBehavior.Active(Frame)) {
      Animator.Play(CrossFadeStateBehavior.BaseName, CrossFadeStateBehavior.LayerIndex);
    }
    if (VisualEffectBehavior.Active(Frame))
      VisualEffect.PlayNonEmptyEvent(VisualEffectBehavior.EndEventName);
    if (CancelBehavior.Active(Frame)) {
      Cancellable = false;
    }
  }
}