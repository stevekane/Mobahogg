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
  [SerializeField] int AttackComboLength = 3;

  [Header("Writes To")]
  [SerializeField] Hitbox Hitbox;

  int Index;
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
  }

  public bool ShouldHit(Combatant combatant) => !Struck.Contains(combatant);

  public void Hit(Combatant combatant) => Struck.Add(combatant);

  public override bool IsRunning
    => State != AttackState.Ready;

  public override bool CanRun
    => State == AttackState.Recovery
    || State == AttackState.Ready;

  public override bool CanStop
    => State == AttackState.Recovery;

  public override void Run() {
    Struck.Clear();
    Animator.SetTrigger($"Ground Attack {Index}");
    Index = (Index + 1) % AttackComboLength;
  }

  // Available on first frame to direct the attack
  public void Aim(Vector2 direction) {
    // TODO: Possibly use the previously struck list to inform the aim assist system further?
    var bestTarget = AimAssistManager.Instance.BestTarget(AbilityManager.transform, AimAssistQuery);
    if (bestTarget) {
      var delta = bestTarget.transform.position-transform.position;
      CharacterController.Rotation.Set(Quaternion.LookRotation(delta.normalized));
    } else if (direction.magnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction.XZ()));
    }
  }

  public override void Stop() {
    Animator.SetTrigger("Cancel");
    State = AttackState.Ready;
  }

  void FixedUpdate() {
    if (!IsRunning)
      return;
    Hitbox.CollisionEnabled = State == AttackState.Active;
  }
}