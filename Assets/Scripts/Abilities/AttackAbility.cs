using System.Collections.Generic;
using UnityEngine;
using AimAssist;
using Abilities;
using System;
using UnityEngine.VFX;

[Serializable]
class RootMotionBehavior : FrameBehavior {
  public float Multiplier;
}

[Serializable]
class AimAssistBehavior : FrameBehavior {
  [InlineEditor] public AimAssistQuery AimAssistQuery;
}

[Serializable]
class WeaponAimBehavior : FrameBehavior {
  public Vector3 Direction;
}

[Serializable]
class HitboxBehavior : FrameBehavior {
}

[Serializable]
class AudioOneShotBehavior : FrameBehavior {
  public AudioClip AudioClip;
}

[Serializable]
class AnimationClipBehavior : FrameBehavior {
  public string TriggerName;
}

[Serializable]
class VisualEffectBehavior : FrameBehavior {
  public string StartEventName = "OnPlay";
  public string UpdateEventName = "";
  public string EndEventName = "";
}

[Serializable]
class CancelBehavior : FrameBehavior {

}

public class AttackAbility : Ability {
  [SerializeField, MaxFrames("EndFrame")] RootMotionBehavior RootMotionBehavior;
  [SerializeField, MaxFrames("EndFrame")] AimAssistBehavior AimAssistBehavior;
  [SerializeField, MaxFrames("EndFrame")] WeaponAimBehavior WeaponAimBehavior;
  [SerializeField, MaxFrames("EndFrame")] HitboxBehavior HitboxBehavior;
  [SerializeField, MaxFrames("EndFrame")] AudioOneShotBehavior AudioOneShotBehavior;
  [SerializeField, MaxFrames("EndFrame")] AnimationClipBehavior AnimationClipBehavior;
  [SerializeField, MaxFrames("EndFrame")] VisualEffectBehavior VisualEffectBehavior;
  [SerializeField, MaxFrames("EndFrame")] CancelBehavior CancelBehavior;
  [SerializeField] int EndFrame = 24;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;
  [SerializeField] VisualEffect VisualEffect;
  [SerializeField] AudioSource AudioSource;

  int Frame;
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
    if (AnimationClipBehavior.Starting(Frame))
      Animator.SetTrigger(AnimationClipBehavior.TriggerName);
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
    if (AnimationClipBehavior.Ending(Frame)) {}
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
        CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ().normalized));
      }
    }
    if (WeaponAimBehavior.Active(Frame)) {}
    if (HitboxBehavior.Active(Frame)) {}
    if (AudioOneShotBehavior.Active(Frame)) {}
    if (AnimationClipBehavior.Active(Frame)) {}
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
    if (AnimationClipBehavior.Active(Frame)) {}
    if (VisualEffectBehavior.Active(Frame))
      VisualEffect.PlayNonEmptyEvent(VisualEffectBehavior.EndEventName);
    if (CancelBehavior.Active(Frame)) {
      Cancellable = false;
    }
  }
}