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
  [SerializeField] int StartActiveFrame = 9;
  [SerializeField] int StartRecoveryFrame = 12;
  [SerializeField] int LastFrame = 24;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;

  int Frame;
  WeaponAim WeaponAim;
  AttackState State {
    get {
      if (Frame < StartActiveFrame) return AttackState.Windup;
      if (Frame < StartRecoveryFrame) return AttackState.Active;
      if (Frame < LastFrame) return AttackState.Recovery;
      return AttackState.Ready;
    }
  }
  List<Combatant> Struck = new(16);

  void Awake() {
    Frame = LastFrame;
    Hitbox.CollisionEnabled = false;
  }

  void Start() {
    AnimatorCallbackHandler.OnRootMotion.Listen(OnAnimatorMove);
    WeaponAim = AbilityManager.LocateComponent<WeaponAim>();
  }

  void OnDestroy() {
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

  public bool ShouldHit(Combatant combatant) => !Struck.Contains(combatant);

  public void Hit(Combatant combatant) => Struck.Add(combatant);

  public override bool IsRunning => State != AttackState.Ready;
  public override bool CanRun => CharacterController.IsGrounded;
  public override void Run() {
    Struck.Clear();
    Frame = 0;
    WeaponAim.AimDirection = Vector3.forward;
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
    Frame = LastFrame;
    Hitbox.CollisionEnabled = false;
    WeaponAim.AimDirection = null;
    Animator.SetTrigger("Cancel");
  }

  void FixedUpdate() {
    if (IsRunning) {
      Hitbox.CollisionEnabled = State == AttackState.Active;
      Frame = Mathf.Min(Frame+1, LastFrame);
      if (Frame == LastFrame) {
        Hitbox.CollisionEnabled = false;
        WeaponAim.AimDirection = null;
      }
    }
  }
}