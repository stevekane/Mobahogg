using System.Collections.Generic;
using UnityEngine;
using Abilities;

public interface ICancellable {
  public bool Cancellable { get; set; }
}

public class AttackAbility :
  Ability,
  ICancellable,
  IProvider<Animator>,
  IProvider<AnimatorCallbackHandler>,
  IProvider<KCharacterController>,
  IProvider<WeaponAim>,
  IProvider<Hitbox>,
  IProvider<GameObject>,
  IProvider<LocalClock>,
  IProvider<ICancellable>
{
  [SerializeField] RootMotionBehavior RootMotionBehavior;
  [SerializeField] AimAssistFrameBehavior AimAssistBehavior;
  [SerializeField] WeaponAimFrameBehavior WeaponAimBehavior;
  [SerializeField] HitboxBehavior HitboxBehavior;
  [SerializeField] SFXOneShotFrameBehavior AudioOneShotBehavior;
  [SerializeField] AnimationOneShotFrameBehavior CrossFadeStateBehavior;
  [SerializeField] VFXOneShotFrameBehavior VisualEffectBehavior;
  [SerializeField] CancellableFrameBehavior CancelBehavior;
  [SerializeField, Min(0)] int EndFrame = 24;
  [SerializeField, Min(0)] int Frame;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;

  IEnumerable<FrameBehavior> Behaviors {
    get {
      yield return RootMotionBehavior;
      yield return AimAssistBehavior;
      yield return WeaponAimBehavior;
      yield return HitboxBehavior;
      yield return AudioOneShotBehavior;
      yield return CrossFadeStateBehavior;
      yield return VisualEffectBehavior;
      yield return CancelBehavior;
    }
  }

  void Awake() {
    Frame = EndFrame+1;
    Hitbox.CollisionEnabled = false;
  }

  // Steve - It is worth contemplating whether most or all characters might have a single
  // top-level provider that implements this shit. If so, you could move this noise out of here
  // and create a very elegant Ability / User of FrameBehaviors
  AnimatorCallbackHandler IProvider<AnimatorCallbackHandler>.Value(BehaviorTag tag) => AnimatorCallbackHandler;
  Animator IProvider<Animator>.Value(BehaviorTag tag) => AnimatorCallbackHandler.Animator;
  KCharacterController IProvider<KCharacterController>.Value(BehaviorTag tag) => CharacterController;
  WeaponAim IProvider<WeaponAim>.Value(BehaviorTag tag) => AbilityManager.LocateComponent<WeaponAim>();
  GameObject IProvider<GameObject>.Value(BehaviorTag tag) => AbilityManager.gameObject;
  LocalClock IProvider<LocalClock>.Value(BehaviorTag tag) => LocalClock;
  Hitbox IProvider<Hitbox>.Value(BehaviorTag tag) => Hitbox;
  ICancellable IProvider<ICancellable>.Value(BehaviorTag tag) => this;

  public bool Cancellable { get; set; }
  public override bool CanCancel => Cancellable;
  public override void Cancel() {
    FrameBehavior.CancelActiveBehaviors(Behaviors, Frame);
    Frame = EndFrame+1;
  }

  public override bool IsRunning => Frame <= EndFrame;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Frame = 0;
    FrameBehavior.InitializeBehaviors(Behaviors, this);
  }

  public bool CanAim => Frame == 0;
  public void Aim(Vector2 direction) {
    if (direction.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
    }
  }

  void FixedUpdate() {
    if (IsRunning) {
      FrameBehavior.StartBehaviors(Behaviors, Frame);
      FrameBehavior.UpdateBehaviors(Behaviors, Frame);
      FrameBehavior.EndBehaviors(Behaviors, Frame);
      Frame++;
    }
  }
}