using System.Collections.Generic;
using UnityEngine;
using AimAssist;
using Abilities;

public enum AttackState {
  Ready,
  Windup,
  Active,
  Recovery
}

public class AttackAbility : Ability {
  [Header("Reads From")]
  [SerializeField] AimAssistQuery AimAssistQuery;
  [SerializeField] float RootMotionMultiplier = 1;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;
  [SerializeField] WeaponAim WeaponAim;

  AttackState State;
  List<Combatant> Struck = new(16);

  void Awake() {
    Hitbox.CollisionEnabled = false;
  }

  void Start() {
    AnimatorCallbackHandler.OnEvent.Listen(OnEvent);
    AnimatorCallbackHandler.OnRootMotion.Listen(OnAnimatorMove);
  }

  void OnDestroy() {
    AnimatorCallbackHandler.OnEvent.Unlisten(OnEvent);
    AnimatorCallbackHandler.OnRootMotion.Unlisten(OnAnimatorMove);
  }

  void OnAnimatorMove() {
    if (!IsRunning)
      return;
    var forward = CharacterController.Rotation.Forward;
    var dp = Vector3.Project(AnimatorCallbackHandler.Animator.deltaPosition, forward);
    var v = dp / LocalClock.DeltaTime();
    CharacterController.DirectVelocity.Add(RootMotionMultiplier * v);
  }

  void OnEvent(string name) {
    State = name switch {
      "Ready" => AttackState.Ready,
      "Windup" => AttackState.Windup,
      "Active" => AttackState.Active,
      "Recovery" => AttackState.Recovery,
      _ => State
    };
    Hitbox.CollisionEnabled = State == AttackState.Active;
  }

  public bool ShouldHit(Combatant combatant) => !Struck.Contains(combatant);

  public void Hit(Combatant combatant) => Struck.Add(combatant);

  public override bool IsRunning => State != AttackState.Ready;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Struck.Clear();
    WeaponAim.AimDirection = Vector3.forward;
    State = AttackState.Ready;
    Animator.SetTrigger($"Ground Attack {0}");
  }

  public bool CanAim => State == AttackState.Ready;
  public void Aim(Vector2 direction) {
    var bestTarget = AimAssistManager.Instance.BestTarget(AbilityManager.transform, AimAssistQuery);
    if (bestTarget) {
      var delta = bestTarget.transform.position-transform.position;
      CharacterController.Rotation.Set(Quaternion.LookRotation(delta.normalized));
    } else if (direction.magnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
    }
  }

  public override bool CanCancel => State == AttackState.Recovery && Struck.Count > 0;
  public override void Cancel() {
    Struck.Clear();
    WeaponAim.AimDirection = null;
    State = AttackState.Ready;
    Animator.SetTrigger("Cancel");
  }
}