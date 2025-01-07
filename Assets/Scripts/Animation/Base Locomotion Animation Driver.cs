using UnityEngine;

[DefaultExecutionOrder((int)ExecutionGroups.Rendering)]
public class BaseLocomotionAnimationDriver : MonoBehaviour {
  [SerializeField] Animator Animator;
  [SerializeField] KCharacterController CharacterController;

  void FixedUpdate() {
    Animator.SetBool("Grounded", CharacterController.IsGrounded);
    Animator.SetBool("Rising", CharacterController.Velocity.Current.y > 0);
  }
}