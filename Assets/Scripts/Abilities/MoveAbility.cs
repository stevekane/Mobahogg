using State;
using Abilities;
using UnityEngine;

public class MoveAbility : Ability {
  [Header("Reads From")]
  [SerializeField] MoveSpeed MoveSpeed;

  public bool Moving;

  public override bool IsRunning => false;
  public override bool CanRun => true;
  public override void Run() {
    Moving = true;
  }

  public override bool CanCancel => false;
  public override void Cancel() {
    Moving = false;
  }

  public bool CanSteer => Moving;
  public void Steer(Vector2 input) {
    var delta = input.XZ();
    var direction = delta.normalized;
    if (delta.sqrMagnitude > 0) {
      CharacterController.Rotation.Set(Quaternion.LookRotation(direction));
    }
    var velocity = MoveSpeed.Value * delta;
    CharacterController.DirectVelocity.Add(velocity);
    Animator.SetFloat("Speed", velocity.magnitude);
  }
}