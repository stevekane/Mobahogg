using System.Collections.Generic;
using UnityEngine;
using Abilities;
using System.Linq;

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
  [SerializeField, InlineEditor] FrameBehaviors FrameBehaviors;
  [SerializeField, Min(0)] int Frame;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;

  List<FrameBehavior> Behaviors = new(8);

  void Awake() {
    Frame = FrameBehaviors.EndFrame+1;
    Hitbox.CollisionEnabled = false;
  }

  // TODO: Possibly this could live elsewhere? Perhaps somewhere more...generic like the player / owner?
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
    Frame = FrameBehaviors.EndFrame+1;
  }

  public override bool IsRunning => Frame <= FrameBehaviors.EndFrame;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Frame = 0;
    Behaviors.Clear();
    FrameBehaviors.Behaviors.ForEach(b => Behaviors.Add(b.Clone()));
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