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

  public void Run(Vector2 value) {
    var velocity = MoveSpeed.Value * value.XZ();
    CharacterController.DirectVelocity.Add(velocity);
    Animator.SetFloat("Speed", velocity.magnitude);
  }
}