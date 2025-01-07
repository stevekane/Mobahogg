using State;
using Abilities;
using UnityEngine;

public class MoveAbility : MonoBehaviour, IAbility<Vector2> {
  [Header("Reads From")]
  [SerializeField] MoveSpeed MoveSpeed;

  [Header("Writes To")]
  [SerializeField] KCharacterController CharacterController;
  [SerializeField] Animator Animator;

  public bool CanRun => true;

  public void Run(Vector2 input) {
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