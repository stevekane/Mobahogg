using System.Collections.Generic;
using UnityEngine;
using Abilities;
using System;

public interface ICancellable {
  public bool Cancellable { get; set; }
}

public class AttackAbility :
  Ability,
  ICancellable,
  ITypeAndTagProvider<BehaviorTag>
{
  [SerializeField, InlineEditor] FrameBehaviors FrameBehaviors;
  [SerializeField] Hitbox Hitbox;

  List<FrameBehavior> Behaviors = new(8);
  int Frame;

  void Awake() {
    Frame = FrameBehaviors.EndFrame+1;
    Hitbox.CollisionEnabled = false;
  }

  public object Get(Type type, BehaviorTag tag) => (type, tag) switch {
    _ when type == typeof(Hitbox) => Hitbox,
    _ when type == typeof(ICancellable) => this,
    _ => AbilityManager.LocateComponent<FrameBehaviorProvider>().Get(type, tag)
  };

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